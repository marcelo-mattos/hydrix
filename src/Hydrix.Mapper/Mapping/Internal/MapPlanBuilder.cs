using Hydrix.Mapper.Attributes;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Converters;
using Hydrix.Mapper.Plans;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace Hydrix.Mapper.Mapping.Internal
{
    /// <summary>
    /// Compiles the immutable <see cref="MapPlan"/> used to map one source and destination type pair.
    /// </summary>
    /// <remarks>
    /// Every expensive operation required by the mapper cold path happens here: constructor discovery, source and
    /// destination property inspection, attribute lookup, expression-tree assembly, and delegate compilation. The
    /// compiled plan fuses destination construction and all property assignments into a single delegate, eliminating
    /// one virtual call and all redundant casts on the hot path.
    /// </remarks>
    internal static class MapPlanBuilder
    {
        /// <summary>
        /// Builds the complete mapping plan for the supplied source and destination types.
        /// </summary>
        /// <param name="sourceType">
        /// The source type whose public readable properties will be inspected.
        /// </param>
        /// <param name="destType">
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
            Type destType,
            HydrixMapperOptions options)
        {
            var execute = BuildExecute(
                sourceType,
                destType,
                options);
            return new MapPlan(
                execute);
        }

        /// <summary>
        /// Builds the fused compiled delegate that accepts the boxed source, creates a destination instance, transfers
        /// every mapped property, and returns the populated destination.
        /// </summary>
        /// <remarks>
        /// The compiled expression introduces typed local variables for both the source and the destination so each cast
        /// is emitted once regardless of property count. The last expression in the block is the destination widened to
        /// <see cref="object"/>, which is a zero-cost reference widening for class types.
        /// </remarks>
        /// <param name="sourceType">
        /// The source type whose readable properties are considered as mapping inputs.
        /// </param>
        /// <param name="destType">
        /// The destination type whose writable properties are considered as mapping outputs.
        /// </param>
        /// <param name="options">
        /// The option snapshot that influences strict mode and conversion behavior.
        /// </param>
        /// <returns>
        /// A single compiled <see cref="Func{Object,Object}"/> delegate that performs the complete mapping operation
        /// for the selected type pair.
        /// </returns>
        private static Func<object, object> BuildExecute(
            Type sourceType,
            Type destType,
            HydrixMapperOptions options)
        {
            if (destType.IsValueType)
            {
                throw new InvalidOperationException(
                    $"Hydrix.Mapper: destination type '{destType.FullName}' cannot be a value type. Map to a reference type instead.");
            }

            var constructor = destType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null);

            if (constructor == null)
            {
                throw new InvalidOperationException(
                    $"Hydrix.Mapper: destination type '{destType.FullName}' must have a public " +
                    "parameterless constructor.");
            }

            var sourceProperties = BuildPropertyLookup(
                sourceType);
            var destinationProperties = destType.GetProperties(
                BindingFlags.Instance | BindingFlags.Public);

            // Single boxed-source parameter for the outer lambda.
            var sourceParam = Expression.Parameter(
                typeof(object),
                "source");

            // Typed locals: each cast is emitted once, not once per property.
            var typedSourceVar = Expression.Variable(
                sourceType,
                "s");
            var typedDestVar = Expression.Variable(
                destType,
                "d");

            // Pre-size for: cast-source + new-dest + N assignments + return.
            var expressions = new List<Expression>(
                destinationProperties.Length + 3);

            // s = (SourceType)source
            expressions.Add(
                Expression.Assign(
                    typedSourceVar,
                    Expression.Convert(
                        sourceParam,
                        sourceType)));

            // d = new DestType()
            expressions.Add(
                Expression.Assign(
                    typedDestVar,
                    Expression.New(
                        constructor)));

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
                            $"'{destType.Name}.{destinationProperty.Name}' has no matching source property " +
                            $"in '{sourceType.Name}'. Set StrictMode = false to ignore unmatched properties.");
                    }

                    continue;
                }

                var attribute = destinationProperty.GetCustomAttribute<MapConversionAttribute>(
                    inherit: true);
                var assignment = BuildPropertyAssignment(
                    sourceType,
                    destType,
                    typedSourceVar,
                    typedDestVar,
                    sourceProperty,
                    destinationProperty,
                    options,
                    attribute);
                expressions.Add(
                    assignment);
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
        /// <param name="destType">
        /// The destination type for the current mapping plan.
        /// </param>
        /// <param name="typedSource">
        /// The typed-local expression that exposes the source object without repeated casting.
        /// </param>
        /// <param name="typedDestination">
        /// The typed-local expression that exposes the destination object without repeated casting.
        /// </param>
        /// <param name="sourceProperty">
        /// The source property that provides the input value.
        /// </param>
        /// <param name="destinationProperty">
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
        private static Expression BuildPropertyAssignment(
            Type sourceType,
            Type destType,
            Expression typedSource,
            Expression typedDestination,
            PropertyInfo sourceProperty,
            PropertyInfo destinationProperty,
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
                    destinationProperty.PropertyType,
                    options,
                    attribute);
            }
            catch (NotSupportedException ex)
            {
                throw new InvalidOperationException(
                    $"Hydrix.Mapper: cannot map '{sourceType.Name}.{sourceProperty.Name}' " +
                    $"({sourceProperty.PropertyType.Name}) to '{destType.Name}.{destinationProperty.Name}' " +
                    $"({destinationProperty.PropertyType.Name}). {ex.Message}",
                    ex);
            }

            var destinationAccess = Expression.Property(
                typedDestination,
                destinationProperty);
            return Expression.Assign(
                destinationAccess,
                converted);
        }
    }
}
