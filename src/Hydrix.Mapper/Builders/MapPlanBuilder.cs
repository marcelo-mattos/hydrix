using Hydrix.Mapper.Attributes;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Converters;
using Hydrix.Mapper.Plans;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Hydrix.Mapper.Builders
{
    /// <summary>
    /// Compiles the immutable <see cref="MapPlan"/> used to map one source and destination type pair.
    /// </summary>
    /// <remarks>
    /// Every expensive operation required by the mapper cold path happens here: constructor discovery, source and
    /// destination property inspection, attribute lookup, expression-tree assembly, and delegate compilation. The
    /// compiled plan fuses destination construction and all property assignments into a single delegate, eliminating
    /// one virtual call and all redundant casts on the hot path.
    ///
    /// Two delegates are compiled per plan: a typed <c>Func&lt;TSource, TTarget&gt;</c> that eliminates boundary
    /// casts from the hot path for the generic typed API, and an object-based <c>Func&lt;object, object&gt;</c>
    /// used by the polymorphic untyped API. Property discovery, attribute reads, and strict-mode validation run
    /// exactly once and are shared by both compilations.
    /// </remarks>
    internal static class MapPlanBuilder
    {
        /// <summary>
        /// Builds the complete mapping plan for the supplied source and destination types.
        /// </summary>
        /// <param name="sourceType">
        /// The source type whose public readable properties will be inspected.
        /// </param>
        /// <param name="targetType">
        /// The destination type whose public writable properties will be assigned.
        /// </param>
        /// <param name="options">
        /// The option snapshot that controls conversion behavior and strict-mode validation.
        /// </param>
        /// <returns>
        /// A fully compiled <see cref="MapPlan"/> instance ready to be cached and reused.
        /// </returns>
        internal static MapPlan Build(
            Type sourceType,
            Type targetType,
            HydrixMapperOptions options)
        {
            if (targetType.IsValueType)
            {
                throw new InvalidOperationException(
                    $"Hydrix.Mapper: destination type '{targetType.FullName}' cannot be a value type. Map to a reference type instead.");
            }

            var constructor = targetType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null) ??
                    throw new InvalidOperationException(
                        $"Hydrix.Mapper: destination type '{targetType.FullName}' must have a public " +
                        "parameterless constructor.");

            var sourceProperties = BuildPropertyLookup(
                sourceType);
            var destinationProperties = targetType.GetProperties(
                BindingFlags.Instance | BindingFlags.Public);

            // Property discovery, attribute reads, and strict-mode validation run once and are shared by both
            // compilations below.
            var propertyPairs = ResolvePropertyPairs(
                sourceType,
                targetType,
                options,
                sourceProperties,
                destinationProperties);

            var typedDelegate = BuildTypedDelegate(
                sourceType,
                targetType,
                options,
                constructor,
                propertyPairs);

            var objectExecute = BuildObjectExecute(
                sourceType,
                targetType,
                options,
                constructor,
                propertyPairs);

            return new MapPlan(
                objectExecute,
                typedDelegate);
        }

        /// <summary>
        /// Filters the destination properties to the valid, mappable set, resolves each matching source property and
        /// per-property conversion attribute, and validates strict-mode constraints.
        /// </summary>
        /// <param name="sourceType">
        /// The source type used for strict-mode error messages.
        /// </param>
        /// <param name="targetType">
        /// The destination type used for strict-mode error messages.
        /// </param>
        /// <param name="options">
        /// The option snapshot whose <see cref="HydrixMapperOptions.StrictMode"/> flag controls validation behavior.
        /// </param>
        /// <param name="sourceProperties">
        /// The pre-built lookup of public readable source properties keyed by name.
        /// </param>
        /// <param name="destinationProperties">
        /// The full array of public destination properties to evaluate.
        /// </param>
        /// <returns>
        /// An ordered list of validated source–destination property pairs together with any per-property conversion
        /// attribute. Only properties that pass all filters and have a matching source are included.
        /// </returns>
        private static List<(PropertyInfo Source, PropertyInfo Target, MapConversionAttribute Attr)>
            ResolvePropertyPairs(
                Type sourceType,
                Type targetType,
                HydrixMapperOptions options,
                Dictionary<string, PropertyInfo> sourceProperties,
                PropertyInfo[] destinationProperties)
        {
            var pairs =
                new List<(PropertyInfo Source, PropertyInfo Dest, MapConversionAttribute Attr)>(
                    destinationProperties.Length);

            foreach (var destinationProperty in destinationProperties)
            {
                if (!destinationProperty.CanWrite)
                    continue;

                if (!destinationProperty.SetMethod.IsPublic)
                    continue;

                if (destinationProperty.GetIndexParameters().Length > 0)
                    continue;

                if (destinationProperty.IsDefined(
                    typeof(NotMappedAttribute),
                    inherit: true))
                {
                    continue;
                }

                if (!sourceProperties.TryGetValue(
                        destinationProperty.Name,
                        out var sourceProperty))
                {
                    if (options.StrictMode)
                    {
                        throw new InvalidOperationException(
                            $"Hydrix.Mapper (strict): destination property " +
                            $"'{targetType.Name}.{destinationProperty.Name}' has no matching source property " +
                            $"in '{sourceType.Name}'. Set StrictMode = false to ignore unmatched properties.");
                    }

                    continue;
                }

                var attribute = destinationProperty.GetCustomAttribute<MapConversionAttribute>(
                    inherit: true);

                pairs.Add(
                    (sourceProperty, destinationProperty, attribute));
            }

            return pairs;
        }

        /// <summary>
        /// Builds the strongly typed compiled delegate (<c>Func&lt;TSource, TTarget&gt;</c>) that accepts and returns
        /// strongly typed instances, eliminating source and destination boundary casts from the hot path.
        /// </summary>
        /// <remarks>
        /// The source parameter of the compiled lambda is the exact source type, so no <c>castclass</c> instruction is
        /// emitted for the source. The return type is the destination type, so no boxing occurs for class destinations.
        /// </remarks>
        /// <param name="sourceType">
        /// The source type used as the typed lambda parameter.
        /// </param>
        /// <param name="targetType">
        /// The destination type used as the typed lambda return and local variable.
        /// </param>
        /// <param name="options">
        /// The option snapshot passed through to conversion-expression building.
        /// </param>
        /// <param name="constructor">
        /// The pre-resolved parameterless constructor used to create each destination instance.
        /// </param>
        /// <param name="propertyPairs">
        /// The pre-validated list of source–destination property pairs with their optional conversion attributes.
        /// </param>
        /// <returns>
        /// A compiled <see cref="Delegate"/> whose runtime type is <c>Func&lt;TSource, TTarget&gt;</c>.
        /// </returns>
        private static Delegate BuildTypedDelegate(
            Type sourceType,
            Type targetType,
            HydrixMapperOptions options,
            ConstructorInfo constructor,
            List<(PropertyInfo Source, PropertyInfo Target, MapConversionAttribute Attr)> propertyPairs)
        {
            // Typed source parameter — no castclass instruction emitted for the source.
            var sourceParam = Expression.Parameter(
                sourceType,
                "source");

            var typedDestVar = Expression.Variable(
                targetType,
                "d");

            // Pre-size for: new-dest + N assignments + return.
            var expressions = new List<Expression>(
                propertyPairs.Count + 2);

            // d = new DestType()
            expressions.Add(
                Expression.Assign(
                    typedDestVar,
                    Expression.New(
                        constructor)));

            foreach (var (sourceProp, destProp, attr) in propertyPairs)
            {
                expressions.Add(
                    BuildPropertyAssignment(
                        sourceType,
                        targetType,
                        sourceParam,
                        typedDestVar,
                        sourceProp,
                        destProp,
                        options,
                        attr));
            }

            // Return d — no boxing for class destinations.
            expressions.Add(
                typedDestVar);

            var body = Expression.Block(
                new[] { typedDestVar },
                expressions);

            var delegateType = typeof(Func<,>).MakeGenericType(
                sourceType,
                targetType);

            return Expression.Lambda(
                    delegateType,
                    body,
                    sourceParam)
                .Compile();
        }

        /// <summary>
        /// Builds the object-based compiled delegate (<c>Func&lt;object, object&gt;</c>) that accepts a boxed source
        /// and returns a boxed destination, used by the polymorphic untyped API.
        /// </summary>
        /// <remarks>
        /// The compiled expression introduces a typed local variable for the source so the <c>castclass</c> instruction
        /// is emitted once regardless of property count. The last expression widens the destination to
        /// <see cref="object"/>, which is a zero-cost reference widening for class types.
        /// </remarks>
        /// <param name="sourceType">
        /// The source type used for the typed source local variable.
        /// </param>
        /// <param name="targetType">
        /// The destination type used as the typed destination local variable and return widening.
        /// </param>
        /// <param name="options">
        /// The option snapshot passed through to conversion-expression building.
        /// </param>
        /// <param name="constructor">
        /// The pre-resolved parameterless constructor used to create each destination instance.
        /// </param>
        /// <param name="propertyPairs">
        /// The pre-validated list of source–destination property pairs with their optional conversion attributes.
        /// </param>
        /// <returns>
        /// A compiled <see cref="Func{Object, Object}"/> delegate that performs the complete mapping operation.
        /// </returns>
        private static Func<object, object> BuildObjectExecute(
            Type sourceType,
            Type targetType,
            HydrixMapperOptions options,
            ConstructorInfo constructor,
            List<(PropertyInfo Source, PropertyInfo Target, MapConversionAttribute Attr)> propertyPairs)
        {
            // Single boxed-source parameter for the outer lambda.
            var sourceParam = Expression.Parameter(
                typeof(object),
                "source");

            // Typed locals: the source cast is emitted once, not once per property.
            var typedSourceVar = Expression.Variable(
                sourceType,
                "s");
            var typedDestVar = Expression.Variable(
                targetType,
                "d");

            // Pre-size for: cast-source + new-dest + N assignments + return.
            var expressions = new List<Expression>(
                propertyPairs.Count + 3);

            // s = (SourceType)source
            expressions.Add(
                Expression.Assign(
                    typedSourceVar,
                    Expression.Convert(
                        sourceParam,
                        sourceType)));

            // d = new TargetType()
            expressions.Add(
                Expression.Assign(
                    typedDestVar,
                    Expression.New(
                        constructor)));

            foreach (var (sourceProp, targetProp, attr) in propertyPairs)
            {
                expressions.Add(
                    BuildPropertyAssignment(
                        sourceType,
                        targetType,
                        typedSourceVar,
                        typedDestVar,
                        sourceProp,
                        targetProp,
                        options,
                        attr));
            }

            // Return value: (object)d — zero-cost widening for reference types.
            expressions.Add(
                Expression.Convert(
                    typedDestVar,
                    typeof(object)));

            var body = Expression.Block(
                new[] { typedSourceVar, typedDestVar },
                expressions);

            return Expression.Lambda<Func<object, object>>(
                body,
                sourceParam).Compile();
        }

        /// <summary>
        /// Builds a lookup table containing every public readable, non-indexed source property keyed by property name.
        /// </summary>
        /// <param name="type">
        /// The source type to inspect.
        /// </param>
        /// <returns>
        /// A dictionary keyed by property name and containing the corresponding source <see cref="PropertyInfo"/>.
        /// </returns>
        private static Dictionary<string, PropertyInfo> BuildPropertyLookup(
            Type type)
        {
            var properties = type.GetProperties(
                BindingFlags.Instance | BindingFlags.Public);
            var lookup = new Dictionary<string, PropertyInfo>(
                properties.Length,
                StringComparer.Ordinal);

            foreach (var property in properties)
            {
                if (!property.CanRead)
                    continue;

                if (!property.GetMethod.IsPublic)
                    continue;

                if (property.GetIndexParameters().Length > 0)
                    continue;

                lookup[property.Name] = property;
            }

            return lookup;
        }

        /// <summary>
        /// Builds the expression that reads one source property, converts it, and assigns it to the matching destination
        /// property.
        /// </summary>
        /// <param name="sourceType">
        /// The source type for the current mapping plan.
        /// </param>
        /// <param name="targetType">
        /// The destination type for the current mapping plan.
        /// </param>
        /// <param name="typedSource">
        /// The typed-source expression that exposes the source object without repeated casting.
        /// </param>
        /// <param name="typedTarget">
        /// The typed-destination expression that exposes the destination object without repeated casting.
        /// </param>
        /// <param name="sourceProperty">
        /// The source property that provides the input value.
        /// </param>
        /// <param name="targetProperty">
        /// The destination property that receives the converted value.
        /// </param>
        /// <param name="options">
        /// The option snapshot used while compiling the conversion expression.
        /// </param>
        /// <param name="attribute">
        /// The per-property override attribute found on the destination property, if any.
        /// </param>
        /// <returns>
        /// The expression that performs the selected property assignment.
        /// </returns>
        [SuppressMessage(
            "Major Code Smell",
            "S107:Methods should not have too many parameters",
            Justification = "Performance-critical internal method. Parameters are passed explicitly to avoid additional allocations, indirection, or context objects, ensuring optimal JIT inlining and minimal overhead during expression tree construction.")]
        private static BinaryExpression BuildPropertyAssignment(
            Type sourceType,
            Type targetType,
            Expression typedSource,
            Expression typedTarget,
            PropertyInfo sourceProperty,
            PropertyInfo targetProperty,
            HydrixMapperOptions options,
            MapConversionAttribute attribute)
        {
            var sourceAccess = Expression.Property(
                typedSource,
                sourceProperty);

            Expression converted;
            try
            {
                converted = ConverterFactory.BuildConversionExpression(
                    sourceAccess,
                    sourceProperty.PropertyType,
                    targetProperty.PropertyType,
                    options,
                    attribute);
            }
            catch (NotSupportedException ex)
            {
                throw new InvalidOperationException(
                    $"Hydrix.Mapper: cannot map '{sourceType.Name}.{sourceProperty.Name}' " +
                    $"({sourceProperty.PropertyType.Name}) to '{targetType.Name}.{targetProperty.Name}' " +
                    $"({targetProperty.PropertyType.Name}). {ex.Message}",
                    ex);
            }

            var destinationAccess = Expression.Property(
                typedTarget,
                targetProperty);
            return Expression.Assign(
                destinationAccess,
                converted);
        }
    }
}
