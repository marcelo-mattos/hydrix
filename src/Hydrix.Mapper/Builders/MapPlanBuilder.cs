using Hydrix.Mapper.Attributes;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Converters;
using Hydrix.Mapper.Internals;
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
        /// Caches the open-generic <c>NestedCollectionHelper.MapList</c> method resolved once at class load.
        /// Resolving at class load ensures a fast fail during startup rather than inside a compiled expression tree,
        /// and eliminates the per-call null branch that would otherwise require a dead-code defensive check.
        /// </summary>
        private static readonly MethodInfo MapListOpenMethod =
            typeof(NestedCollectionHelper).GetMethod(
                nameof(NestedCollectionHelper.MapList),
                BindingFlags.Public | BindingFlags.Static);

        /// <summary>
        /// Tracks the source–destination type pairs currently being compiled on the current thread to detect circular
        /// nested mapping references and fail fast with a clear error.
        /// </summary>
        [ThreadStatic]
        private static HashSet<(Type, Type)> _compilingPairs;

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
            _compilingPairs ??= new HashSet<(Type, Type)>();
            _compilingPairs.Add((sourceType, targetType));

            try
            {
                if (targetType.IsValueType)
                {
                    throw new InvalidOperationException(
                        $"Hydrix.Mapper: destination type '{targetType.FullName}' cannot be a value type. Map to a reference type instead.");
                }

                var constructor = ResolveConstructorOrThrow(
                    targetType);

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
            finally
            {
                _compilingPairs.Remove(
                    (sourceType, targetType));
            }
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
                var nestedExpr = TryBuildNestedExpression(
                    sourceAccess,
                    sourceProperty.PropertyType,
                    targetProperty.PropertyType,
                    options);

                converted = nestedExpr ??
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

        /// <summary>
        /// Attempts to build a nested mapping expression for a property pair whose types cannot be handled by the
        /// standard converter pipeline.
        /// </summary>
        /// <param name="srcPropAccess">The expression that reads the source property value.</param>
        /// <param name="srcPropType">The source property type.</param>
        /// <param name="destPropType">The destination property type.</param>
        /// <param name="options">The option snapshot used to resolve and compile the nested plan.</param>
        /// <returns>
        /// An <see cref="Expression"/> for the right-hand side of the property assignment, or
        /// <see langword="null"/> when no nested mapping could be resolved for the type pair.
        /// </returns>
        private static Expression TryBuildNestedExpression(
            Expression srcPropAccess,
            Type srcPropType,
            Type destPropType,
            HydrixMapperOptions options)
        {
            // Case 1 — direct nested object: Customer → CustomerDto
            if (options.TryGetNestedSourceType(
                    destPropType,
                    out var nestedSrcType)
                && nestedSrcType == srcPropType)
            {
                // Guard against circular inline-body recursion: if this pair is already being compiled on
                // the current thread the inline build path would recur forever without going through Build(),
                // so detecting it here prevents the infinite recursion.
                if (_compilingPairs.Contains((srcPropType, destPropType)))
                    throw new InvalidOperationException(
                        $"Hydrix.Mapper: circular nested mapping detected: '{srcPropType.Name}' → " +
                        $"'{destPropType.Name}' is already being compiled. Circular object references are not " +
                        "supported. Use [NotMapped] or remove the circular registration.");

                return BuildNestedObjectExpression(
                    srcPropAccess,
                    srcPropType,
                    destPropType,
                    options);
            }

            // Case 2 — nested collection: List<Customer> → List<CustomerDto> (or IReadOnlyList<T> etc.)
            if (TryGetEnumerableElementType(
                    srcPropType,
                    out var srcElementType)
                && TryGetCollectionDestElementType(
                    destPropType,
                    out var destElementType)
                && options.TryGetNestedSourceType(
                    destElementType,
                    out var nestedCollSrcType)
                && nestedCollSrcType == srcElementType)
            {
                return BuildNestedCollectionExpression(
                    srcPropAccess,
                    srcElementType,
                    destElementType,
                    destPropType,
                    options);
            }

            return null;
        }

        /// <summary>
        /// Builds the inline body expression for a nested object property, capturing the source value exactly once and
        /// constructing a new destination instance with all mapped properties assigned inline.
        /// For reference-type sources the null guard is included; for value-type sources the body is called directly.
        /// </summary>
        /// <param name="srcPropAccess">The expression that reads the source property value.</param>
        /// <param name="srcPropType">The source property type.</param>
        /// <param name="destPropType">The destination property type.</param>
        /// <param name="options">The option snapshot used to resolve the nested plan.</param>
        /// <returns>
        /// An <see cref="Expression"/> that produces the mapped destination instance, guarded by a null check for
        /// reference-type sources.
        /// </returns>
        private static Expression BuildNestedObjectExpression(
            Expression srcPropAccess,
            Type srcPropType,
            Type destPropType,
            HydrixMapperOptions options)
        {
            // Capture the source property value exactly once in a typed local variable.
            var capturedSrcVar = Expression.Variable(srcPropType, "ns");

            // Build the inline nested property-assignment block using the captured local.
            var inlineBody = BuildInlineNestedBodyBlock(
                capturedSrcVar,
                srcPropType,
                destPropType,
                options);

            // For value-type sources there is no null to check; capture and map directly.
            if (srcPropType.IsValueType)
            {
                return Expression.Block(
                    new[]
                    {
                        capturedSrcVar,
                    },
                    Expression.Assign(
                        capturedSrcVar,
                        srcPropAccess),
                    inlineBody);
            }

            // For reference-type sources add a null guard.
            return Expression.Block(
                new[]
                {
                    capturedSrcVar,
                },
                Expression.Assign(
                    capturedSrcVar,
                    srcPropAccess),
                Expression.Condition(
                    Expression.Equal(
                        capturedSrcVar,
                        Expression.Constant(
                            null,
                            srcPropType)),
                    Expression.Default(
                        destPropType),
                    inlineBody));
        }

        /// <summary>
        /// Builds the expression block that creates a new destination instance and assigns all mapped properties from the
        /// supplied typed source expression. The block's value is the populated destination instance.
        /// </summary>
        /// <remarks>
        /// This method is called during plan compilation to inline the nested mapping body directly into the outer
        /// compiled delegate, eliminating the delegate-boundary overhead of a separate nested plan invocation.
        /// </remarks>
        /// <param name="srcExpr">
        /// The typed expression that provides the source object for the nested body — typically a captured local variable.
        /// </param>
        /// <param name="srcType">
        /// The source type whose public readable properties are inspected to build the nested body.
        /// </param>
        /// <param name="destType">
        /// The destination type that is constructed and populated inside the returned block.
        /// </param>
        /// <param name="options">
        /// The option snapshot used when resolving and compiling each nested property assignment.
        /// </param>
        /// <returns>
        /// A <see cref="BlockExpression"/> that declares a local destination variable, assigns all mapped property values
        /// to it, and yields the populated destination instance as the block's result.
        /// </returns>
        private static Expression BuildInlineNestedBodyBlock(
            Expression srcExpr,
            Type srcType,
            Type destType,
            HydrixMapperOptions options)
        {
            var nestedConstructor = ResolveConstructorOrThrow(
                destType);

            var nestedSourceProperties = BuildPropertyLookup(
                srcType);

            var nestedDestProperties = destType.GetProperties(
                BindingFlags.Instance |
                BindingFlags.Public);

            var nestedPairs = ResolvePropertyPairs(
                srcType,
                destType,
                options,
                nestedSourceProperties,
                nestedDestProperties);

            var nestedDestVar = Expression.Variable(
                destType,
                "nd");

            var bodyExprs = new List<Expression>(
                nestedPairs.Count + 2);

            bodyExprs.Add(
                Expression.Assign(
                    nestedDestVar,
                    Expression.New(
                        nestedConstructor)));

            foreach (var (srcProp, dstProp, attr) in nestedPairs)
            {
                bodyExprs.Add(
                    BuildPropertyAssignment(
                        srcType,
                        destType,
                        srcExpr,
                        nestedDestVar,
                        srcProp,
                        dstProp,
                        options,
                        attr));
            }

            bodyExprs.Add(nestedDestVar);

            return Expression.Block(
                new[] { nestedDestVar },
                bodyExprs);
        }

        /// <summary>
        /// Returns the public parameterless constructor of the specified type, or throws if one does not exist.
        /// </summary>
        /// <param name="type">
        /// The type to inspect for a public parameterless constructor.
        /// </param>
        /// <returns>
        /// The <see cref="ConstructorInfo"/> for the public parameterless constructor of <paramref name="type"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="type"/> does not expose a public parameterless constructor.
        /// </exception>
        private static ConstructorInfo ResolveConstructorOrThrow(Type type)
            => type.GetConstructor(
                BindingFlags.Instance |
                BindingFlags.Public,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null) ??
            throw new InvalidOperationException(
                $"Hydrix.Mapper: destination type '{type.FullName}' must have a public parameterless constructor.");

        /// <summary>
        /// Builds an expression that maps a source collection property to a destination collection property, applying an
        /// inline nested mapping body for each element.
        /// </summary>
        /// <remarks>The returned expression will convert the mapped collection to the specified
        /// destination property type if necessary. Fast paths use an inline for-loop with index access for
        /// <see cref="List{T}"/> and <see cref="IList{T}"/> sources. General <see cref="IEnumerable{T}"/> sources fall
        /// back to <see cref="NestedCollectionHelper.MapList{TSrc,TDest}"/> with an inline-compiled element
        /// delegate.</remarks>
        /// <param name="srcPropAccess">An expression representing access to the source collection property to be mapped.</param>
        /// <param name="srcElementType">The type of elements contained in the source collection.</param>
        /// <param name="destElementType">The type of elements to be created in the destination collection.</param>
        /// <param name="destPropType">The type of the destination property that will receive the mapped collection.</param>
        /// <param name="options">The mapping options to use when constructing the nested mapping plan.</param>
        /// <returns>An expression that, when executed, maps the source collection to the destination collection using the
        /// specified mapping plan.</returns>
        private static Expression BuildNestedCollectionExpression(
            Expression srcPropAccess,
            Type srcElementType,
            Type destElementType,
            Type destPropType,
            HydrixMapperOptions options)
        {
            var listSrcType = typeof(List<>).MakeGenericType(
                srcElementType);
            var ilistSrcType = typeof(IList<>).MakeGenericType(
                srcElementType);
            var listDestType = typeof(List<>).MakeGenericType(
                destElementType);

            var srcPropType = srcPropAccess.Type;

            // Fast paths: inline for-loop using index access.
            if (listSrcType.IsAssignableFrom(srcPropType) ||
                ilistSrcType.IsAssignableFrom(srcPropType))
            {
                var indexedType = listSrcType.IsAssignableFrom(
                    srcPropType)
                    ? listSrcType
                    : ilistSrcType;

                return BuildIndexedCollectionLoop(
                    srcPropAccess,
                    srcPropType,
                    indexedType,
                    srcElementType,
                    destElementType,
                    listDestType,
                    destPropType,
                    options);
            }

            // Fallback: use NestedCollectionHelper with an inline-compiled element delegate.
            return BuildEnumerableFallback(
                srcPropAccess,
                srcPropType,
                srcElementType,
                destElementType,
                listDestType,
                destPropType,
                options);
        }

        /// <summary>
        /// Builds an expression tree that iterates over an indexed source collection, transforms each element, and
        /// constructs a destination collection with the mapped elements.
        /// </summary>
        /// <remarks>This method is intended for use in expression tree construction scenarios where
        /// performance is critical. It assumes the source collection supports indexed access and a Count property. Null
        /// source collections are handled by returning null for the destination property.</remarks>
        /// <param name="srcPropAccess">An expression representing access to the source collection property.</param>
        /// <param name="srcPropType">The type of the source property being accessed.</param>
        /// <param name="indexedType">The type that provides indexed access to the source collection (e.g., IList&lt;T&gt;).</param>
        /// <param name="srcElementType">The type of elements contained in the source collection.</param>
        /// <param name="destElementType">The type of elements to be created in the destination collection.</param>
        /// <param name="listDestType">The concrete type to use for the destination collection (e.g., List&lt;T&gt;).</param>
        /// <param name="destPropType">The type of the destination property to which the collection will be assigned.</param>
        /// <param name="options">The mapping options to use when transforming elements from the source to the destination collection.</param>
        /// <returns>An expression that, when executed, creates and populates a destination collection by mapping each element
        /// from the source collection.</returns>
        [SuppressMessage(
            "Major Code Smell",
            "S107:Methods should not have too many parameters",
            Justification = "Performance-critical internal method. Parameters are passed explicitly to avoid additional allocations, indirection, or context objects, ensuring optimal JIT inlining and minimal overhead during expression tree construction.")]
        private static Expression BuildIndexedCollectionLoop(
            Expression srcPropAccess,
            Type srcPropType,
            Type indexedType,
            Type srcElementType,
            Type destElementType,
            Type listDestType,
            Type destPropType,
            HydrixMapperOptions options)
        {
            // For interface types (e.g. IList<T>), "Count" is declared on ICollection<T>, not on the interface
            // itself — interface reflection does not include inherited interface members by default.
            var countProp = indexedType.GetProperty(
                "Count")
                ?? typeof(ICollection<>)
                    .MakeGenericType(
                        srcElementType)
                    .GetProperty(
                        "Count");

            var indexerProp = indexedType.GetProperty(
                "Item");

            var listDestCtor = listDestType.GetConstructor(
                new[]
                {
                    typeof(int)
                });

            var destAddMethod = listDestType.GetMethod(
                "Add",
                new[]
                {
                    destElementType
                });

            var srcColVar = Expression.Variable(
                indexedType,
                "srcCol");

            var resultVar = Expression.Variable(
                listDestType,
                "result");

            var iVar = Expression.Variable(
                typeof(int),
                "i");

            var elemVar = Expression.Variable(
                srcElementType,
                "elem");

            var loopBreakLabel = Expression.Label(
                typeof(void),
                "loopBreak");

            // Inline element body: elem → TDest block
            var inlineBody = BuildInlineNestedBodyBlock(
                elemVar,
                srcElementType,
                destElementType,
                options);

            var perElementBlock = srcElementType.IsValueType
                ? Expression.Block(
                    new[]
                    {
                        elemVar,
                    },
                    Expression.Assign(
                        elemVar,
                        Expression.Property(
                            srcColVar,
                            indexerProp,
                            iVar)),
                    Expression.Call(
                        resultVar,
                        destAddMethod,
                        inlineBody))
                : Expression.Block(
                    new[]
                    {
                        elemVar,
                    },
                    Expression.Assign(
                        elemVar,
                        Expression.Property(
                            srcColVar,
                            indexerProp,
                            iVar)),
                    Expression.IfThen(
                        Expression.NotEqual(
                            elemVar,
                            Expression.Constant(
                                null,
                                srcElementType)),
                        Expression.Call(
                            resultVar,
                            destAddMethod,
                            inlineBody)));

            var loopBlock = Expression.Loop(
                Expression.Block(
                    Expression.IfThen(
                        Expression.GreaterThanOrEqual(
                            iVar,
                            Expression.Property(
                                srcColVar,
                                countProp)),
                        Expression.Break(
                            loopBreakLabel)),
                    perElementBlock,
                    Expression.Assign(
                        iVar,
                        Expression.Add(
                            iVar,
                            Expression.Constant(1)))),
                loopBreakLabel);

            // Full block: capture, null-guard, build result, return.
            return Expression.Block(
                new[]
                {
                    srcColVar,
                    resultVar,
                    iVar,
                },
                Expression.Assign(
                    srcColVar,
                    indexedType == srcPropType
                        ? srcPropAccess
                        : (Expression)Expression.Convert(
                            srcPropAccess,
                            indexedType)),

                Expression.IfThen(
                    Expression.NotEqual(
                        srcColVar,
                        Expression.Constant(
                            null,
                            indexedType)),
                    Expression.Block(
                        Expression.Assign(
                            resultVar,
                            Expression.New(
                                listDestCtor,
                                Expression.Property(
                                    srcColVar,
                                    countProp))),
                        Expression.Assign(
                            iVar,
                            Expression.Constant(0)),
                        loopBlock)),
                destPropType == listDestType
                    ? (Expression)resultVar
                    : Expression.Convert(
                        resultVar,
                        destPropType));
        }

        /// <summary>
        /// Builds an expression that maps a source enumerable property to a destination collection property using a
        /// fallback mapping strategy.
        /// </summary>
        /// <remarks>This method is used as a fallback when a direct mapping between collection types is
        /// not available. It dynamically constructs a mapping delegate for element conversion and invokes a helper
        /// method to perform the collection mapping.</remarks>
        /// <param name="srcPropAccess">An expression representing access to the source property to be mapped.</param>
        /// <param name="srcPropType">The type of the source property, expected to be an enumerable type.</param>
        /// <param name="srcElementType">The type of elements contained in the source enumerable.</param>
        /// <param name="destElementType">The type of elements expected in the destination collection.</param>
        /// <param name="listDestType">The concrete collection type to use for the destination property if applicable.</param>
        /// <param name="destPropType">The type of the destination property, which may differ from the concrete collection type.</param>
        /// <param name="options">The mapping options to use when constructing the mapping expression.</param>
        /// <returns>An expression that, when executed, maps the source enumerable to the destination collection type, converting
        /// each element as needed.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the required mapping method cannot be found on the helper type.</exception>
        private static Expression BuildEnumerableFallback(
            Expression srcPropAccess,
            Type srcPropType,
            Type srcElementType,
            Type destElementType,
            Type listDestType,
            Type destPropType,
            HydrixMapperOptions options)
        {
            // Compile the inline body as a typed delegate to pass to the helper.
            var elemParam = Expression.Parameter(
                srcElementType,
                "elem");

            var inlineBody = BuildInlineNestedBodyBlock(
                elemParam,
                srcElementType,
                destElementType,
                options);

            var funcType = typeof(Func<,>).MakeGenericType(
                srcElementType,
                destElementType);

            var inlineDelegate = Expression
                .Lambda(
                    funcType,
                    inlineBody,
                    elemParam)
                .Compile();

            var genericMethod = MapListOpenMethod.MakeGenericMethod(
                srcElementType,
                destElementType);

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(
                srcElementType);

            var srcAsEnumerable = enumerableType == srcPropType
                ? srcPropAccess
                : (Expression)Expression.Convert(
                    srcPropAccess,
                    enumerableType);

            var callExpr = Expression.Call(
                null,
                genericMethod,
                srcAsEnumerable,
                Expression.Constant(
                    inlineDelegate,
                    funcType));

            return destPropType == listDestType
                ? callExpr
                : (Expression)Expression.Convert(
                    callExpr,
                    destPropType);
        }

        /// <summary>
        /// Returns the element type when the supplied type is or implements <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <param name="elementType">
        /// When this method returns <see langword="true"/>, contains the enumerable element type; otherwise,
        /// <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when <paramref name="type"/> is or implements <see cref="IEnumerable{T}"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        private static bool TryGetEnumerableElementType(
            Type type,
            out Type elementType)
        {
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }

            foreach (var iface in type.GetInterfaces())
            {
                if (iface.IsGenericType &&
                    iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    elementType = iface.GetGenericArguments()[0];
                    return true;
                }
            }

            elementType = null;
            return false;
        }

        /// <summary>
        /// Returns the element type when the supplied type is a supported destination collection:
        /// <see cref="List{T}"/>, <see cref="IList{T}"/>, <see cref="IReadOnlyList{T}"/>, or
        /// <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <param name="elementType">
        /// When this method returns <see langword="true"/>, contains the collection element type; otherwise,
        /// <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when <paramref name="type"/> is a supported destination collection type;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        private static bool TryGetCollectionDestElementType(
            Type type,
            out Type elementType)
        {
            if (!type.IsGenericType)
            {
                elementType = null;
                return false;
            }

            var def = type.GetGenericTypeDefinition();
            if (def == typeof(List<>)
                || def == typeof(IList<>)
                || def == typeof(IReadOnlyList<>)
                || def == typeof(IEnumerable<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }

            elementType = null;
            return false;
        }
    }
}
