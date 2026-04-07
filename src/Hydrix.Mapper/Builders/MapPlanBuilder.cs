using Hydrix.Mapper.Attributes;
using Hydrix.Mapper.Caching;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Converters;
using Hydrix.Mapper.Internals;
using Hydrix.Mapper.Plans;
using System;
using System.Collections.Concurrent;
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
        // -----------------------------------------------------------------------------------------
        // Static resolve-once fields
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Caches the open-generic <c>NestedCollectionHelper.MapList</c> method resolved once at class load.
        /// Resolving at class load ensures a fast fail during startup rather than inside a compiled expression tree,
        /// and eliminates the per-call null branch that would otherwise require a dead-code defensive check.
        /// </summary>
        private static readonly MethodInfo MapListOpenMethod =
            typeof(NestedCollectionHelper).GetMethod(
                nameof(NestedCollectionHelper.MapList),
                BindingFlags.Public | BindingFlags.Static);

        // -----------------------------------------------------------------------------------------
        // Cold-path build caches
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Caches resolved constructor and property-pair metadata per <c>(srcType, destType, strictMode)</c> triplet.
        /// Avoids redundant reflection and attribute scanning when the same nested type pair appears across multiple
        /// outer mapping plans. Thread-safe; populated lazily on first inline body build.
        /// </summary>
        private static readonly ConcurrentDictionary<(Type, Type, bool), ResolvedTypeMetadata> MetadataCache =
            new ConcurrentDictionary<(Type, Type, bool), ResolvedTypeMetadata>();

        /// <summary>
        /// Caches the compiled element-mapping delegate per
        /// <c>(srcElementType, destElementType, optionsKey)</c> triplet.
        /// Avoids re-compiling the same element delegate when the same nested collection element pair appears in multiple
        /// outer plans compiled under identical options. Thread-safe; populated lazily in the enumerable-fallback path.
        /// </summary>
        private static readonly ConcurrentDictionary<ElementDelegateKey, Delegate> ElementDelegateCache =
            new ConcurrentDictionary<ElementDelegateKey, Delegate>();

        // -----------------------------------------------------------------------------------------
        // ThreadStatic cycle-detection state
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Tracks every source–destination type pair currently being compiled on the current thread (both at the root
        /// <see cref="Build"/> level and inside inline nested-body builds). The set is used by
        /// <see cref="WithCompilingPair{T}"/> to detect direct and indirect circular mapping references early — before
        /// an infinite recursion can cause a stack overflow.
        /// </summary>
        [ThreadStatic]
        private static HashSet<(Type, Type)> _compilingPairs;

        // -----------------------------------------------------------------------------------------
        // Public entry point
        // -----------------------------------------------------------------------------------------

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
            HydrixMapperOptions options) =>
            WithCompilingPair(sourceType, targetType, () =>
            {
                if (targetType.IsValueType)
                {
                    throw new InvalidOperationException(
                        $"Hydrix.Mapper: destination type '{targetType.FullName}' cannot be a value type. " +
                        "Map to a reference type instead.");
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
            });

        // -----------------------------------------------------------------------------------------
        // Cycle-detection helper
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Adds <paramref name="src"/>/<paramref name="dest"/> to the thread-local compiling-pairs set, invokes
        /// <paramref name="build"/>, and removes the pair in a <c>finally</c> block so the set is always consistent
        /// even when <paramref name="build"/> throws.
        /// </summary>
        /// <remarks>
        /// This helper is the single, authoritative place where circular-mapping detection happens. Applying it to both
        /// the root <see cref="Build"/> call and every <see cref="BuildInlineNestedBodyBlock"/> call guarantees that
        /// both direct cycles (<c>A → A</c>) and indirect cycles (<c>A → B → C → B</c>) are caught before a stack
        /// overflow can occur.
        /// </remarks>
        /// <typeparam name="T">The return type of the build callback.</typeparam>
        /// <param name="src">The source type being compiled.</param>
        /// <param name="dest">The destination type being compiled.</param>
        /// <param name="build">The factory that performs the actual compilation work.</param>
        /// <returns>The value returned by <paramref name="build"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="src"/>/<paramref name="dest"/> is already present in the set, indicating a
        /// circular nested mapping registration.
        /// </exception>
        private static T WithCompilingPair<T>(
            Type src,
            Type dest,
            Func<T> build)
        {
            _compilingPairs ??= new HashSet<(Type, Type)>();

            if (!_compilingPairs.Add(
                (src, dest)))
            {
                throw new InvalidOperationException(
                    $"Hydrix.Mapper: circular nested mapping detected: '{src.Name}' → '{dest.Name}' is already " +
                    "being compiled. Circular object references are not supported. " +
                    "Use [NotMapped] or remove the circular registration.");
            }

            try
            {
                return build();
            }
            finally
            {
                _compilingPairs.Remove(
                    (src, dest));
            }
        }

        // -----------------------------------------------------------------------------------------
        // Plan-building internals
        // -----------------------------------------------------------------------------------------

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
        [SuppressMessage(
            "Major Code Smell",
            "S3220:Method overloads should not be ambiguous",
            Justification = "Overloads are intentionally designed for performance and API ergonomics, avoiding additional abstractions or wrappers that would introduce allocations or prevent JIT optimizations.")]
        private static BlockExpression BuildNestedObjectExpression(
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
        /// <para>
        /// This method is called during plan compilation to inline the nested mapping body directly into the outer
        /// compiled delegate, eliminating the delegate-boundary overhead of a separate nested plan invocation.
        /// </para>
        /// <para>
        /// Cycle detection is performed here via <see cref="WithCompilingPair{T}"/> so that both direct cycles
        /// (<c>A → A</c>) and indirect multi-hop cycles (<c>A → B → C → B</c>) are caught regardless of how deep
        /// the nesting goes. Reflection metadata is retrieved from <see cref="MetadataCache"/> to avoid repeated
        /// <c>GetProperties</c>, constructor, and attribute scans across multiple outer plans that share the same
        /// nested pair.
        /// </para>
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
        private static BlockExpression BuildInlineNestedBodyBlock(
            Expression srcExpr,
            Type srcType,
            Type destType,
            HydrixMapperOptions options) =>
            WithCompilingPair(srcType, destType, () =>
            {
                var meta = GetOrBuildMetadata(
                    srcType,
                    destType,
                    options);

                var nestedDestVar = Expression.Variable(
                    destType,
                    "nd");

                var bodyExprs = new List<Expression>(
                    meta.PropertyPairs.Count + 2);

                bodyExprs.Add(
                    Expression.Assign(
                        nestedDestVar,
                        Expression.New(
                            meta.Constructor)));

                foreach (var (srcProp, dstProp, attr) in meta.PropertyPairs)
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
            });

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

        // -----------------------------------------------------------------------------------------
        // Metadata cache helpers
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the reflection metadata for the supplied type pair, constructing and caching it on first use.
        /// </summary>
        /// <remarks>
        /// Only <see cref="HydrixMapperOptions.StrictMode"/> influences which destination properties appear in the pairs
        /// list; all other option values affect expression generation, not property discovery. The cache key therefore
        /// uses <c>(srcType, destType, strictMode)</c>, which is the minimal discriminant needed for correctness.
        /// </remarks>
        /// <param name="srcType">The source type.</param>
        /// <param name="destType">The destination type.</param>
        /// <param name="options">The option snapshot whose <see cref="HydrixMapperOptions.StrictMode"/> is captured.</param>
        /// <returns>The cached or newly created <see cref="ResolvedTypeMetadata"/> for the pair.</returns>
        private static ResolvedTypeMetadata GetOrBuildMetadata(
            Type srcType,
            Type destType,
            HydrixMapperOptions options)
        {
            var key = (srcType, destType, options.StrictMode);

            if (MetadataCache.TryGetValue(key, out var cached))
                return cached;

            var ctor = ResolveConstructorOrThrow(destType);
            var srcProps = BuildPropertyLookup(srcType);
            var destProps = destType.GetProperties(
                BindingFlags.Instance | BindingFlags.Public);
            var pairs = ResolvePropertyPairs(
                srcType,
                destType,
                options,
                srcProps,
                destProps);

            var meta = new ResolvedTypeMetadata(ctor, pairs);

            // GetOrAdd: if another thread raced and won, we use their instance (equivalent result).
            return MetadataCache.GetOrAdd(key, meta);
        }

        /// <summary>
        /// Holds the pre-computed reflection metadata for a source–destination type pair.
        /// Instances are immutable after construction and safe to share across threads.
        /// </summary>
        private sealed class ResolvedTypeMetadata
        {
            /// <summary>
            /// Stores the public parameterless constructor of the destination type used to instantiate each mapped object.
            /// </summary>
            internal readonly ConstructorInfo Constructor;

            /// <summary>
            /// Stores the validated, ordered list of source–destination property pairs with any per-property conversion
            /// attribute. Read-only after construction; safe to iterate concurrently.
            /// </summary>
            internal readonly List<(PropertyInfo Source, PropertyInfo Target, MapConversionAttribute Attr)> PropertyPairs;

            /// <summary>
            /// Initializes a new <see cref="ResolvedTypeMetadata"/> with the supplied constructor and property pairs.
            /// </summary>
            /// <param name="constructor">The pre-resolved parameterless constructor of the destination type.</param>
            /// <param name="propertyPairs">The validated list of mapped property pairs.</param>
            internal ResolvedTypeMetadata(
                ConstructorInfo constructor,
                List<(PropertyInfo, PropertyInfo, MapConversionAttribute)> propertyPairs)
            {
                Constructor = constructor;
                PropertyPairs = propertyPairs;
            }
        }

        // -----------------------------------------------------------------------------------------
        // Collection mapping
        // -----------------------------------------------------------------------------------------

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
        [SuppressMessage(
            "Major Code Smell",
            "S3220:Method overloads should not be ambiguous",
            Justification = "Overloads are intentionally designed for performance and API ergonomics, avoiding additional abstractions or wrappers that would introduce allocations or prevent JIT optimizations.")]
        private static BlockExpression BuildIndexedCollectionLoop(
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
        /// Builds an expression that maps an enumerable source property to a destination enumerable property using a
        /// fallback mapping strategy.
        /// </summary>
        /// <remarks>This method is used when a direct mapping between enumerable types is not available
        /// and a fallback mapping must be constructed. It ensures that element mapping delegates are cached per mapping
        /// options to avoid redundant compilation.</remarks>
        /// <param name="srcPropAccess">The expression representing access to the source property value to be mapped.</param>
        /// <param name="srcPropType">The type of the source property, expected to be an enumerable type.</param>
        /// <param name="srcElementType">The type of elements contained in the source enumerable.</param>
        /// <param name="destElementType">The type of elements to be produced in the destination enumerable.</param>
        /// <param name="listDestType">The concrete list type to use for the destination property if applicable.</param>
        /// <param name="destPropType">The type of the destination property, which may be an interface or concrete collection type.</param>
        /// <param name="options">The mapping options that control conversion behavior and delegate caching.</param>
        /// <returns>An expression that, when executed, produces a destination enumerable with elements mapped from the source
        /// enumerable.</returns>
        private static Expression BuildEnumerableFallback(
            Expression srcPropAccess,
            Type srcPropType,
            Type srcElementType,
            Type destElementType,
            Type listDestType,
            Type destPropType,
            HydrixMapperOptions options)
        {
            var funcType = typeof(Func<,>).MakeGenericType(
                srcElementType,
                destElementType);

            // Retrieve or compile the element-mapping delegate. Keying by options prevents reuse
            // of a delegate that was compiled under different conversion settings.
            var optionsKey = MapPlanOptionsKey.Create(options);
            var delegateKey = new ElementDelegateKey(
                srcElementType,
                destElementType,
                optionsKey);

            if (!ElementDelegateCache.TryGetValue(
                delegateKey,
                out Delegate inlineDelegate))
            {
                var elemParam = Expression.Parameter(
                    srcElementType,
                    "elem");

                var inlineBody = BuildInlineNestedBodyBlock(
                    elemParam,
                    srcElementType,
                    destElementType,
                    options);

                var compiled = Expression
                    .Lambda(
                        funcType,
                        inlineBody,
                        elemParam)
                    .Compile();

                // GetOrAdd(key, value): if a concurrent thread already stored a delegate for this key,
                // use theirs (both are functionally equivalent). This avoids a factory overload that
                // could call Compile() multiple times under contention.
                inlineDelegate = ElementDelegateCache.GetOrAdd(delegateKey, compiled);
            }

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

        // -----------------------------------------------------------------------------------------
        // Element-delegate cache key
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Identifies a cached element-mapping delegate by its source element type, destination element type, and
        /// full option snapshot. All three components must match for the cached delegate to be reused.
        /// </summary>
        private readonly struct ElementDelegateKey :
            IEquatable<ElementDelegateKey>
        {
            /// <summary>Stores the source element type.</summary>
            private readonly Type _srcElement;

            /// <summary>Stores the destination element type.</summary>
            private readonly Type _destElement;

            /// <summary>Stores the full option snapshot that governs element-level conversions.</summary>
            private readonly MapPlanOptionsKey _optionsKey;

            /// <summary>
            /// Initializes a new <see cref="ElementDelegateKey"/>.
            /// </summary>
            /// <param name="srcElement">The source element type.</param>
            /// <param name="destElement">The destination element type.</param>
            /// <param name="optionsKey">The option snapshot that governs element-level conversions.</param>
            internal ElementDelegateKey(
                Type srcElement,
                Type destElement,
                MapPlanOptionsKey optionsKey)
            {
                _srcElement = srcElement;
                _destElement = destElement;
                _optionsKey = optionsKey;
            }

            /// <summary>
            /// Determines whether this key equals another <see cref="ElementDelegateKey"/>.
            /// </summary>
            /// <remarks>
            /// The short-circuit false branches of the conjunctive expression are only reachable via dictionary-bucket
            /// hash collisions between keys with differing element types, which cannot be engineered in unit tests, so
            /// this method is excluded from coverage analysis.
            /// </remarks>
            [ExcludeFromCodeCoverage]
            public bool Equals(
                ElementDelegateKey other) =>
                _srcElement == other._srcElement &&
                _destElement == other._destElement &&
                _optionsKey.Equals(
                    other._optionsKey);

            /// <summary>
            /// Determines whether this key equals another object.
            /// </summary>
            /// <remarks>
            /// <see cref="ConcurrentDictionary{TKey,TValue}"/> always uses the typed <see cref="Equals(ElementDelegateKey)"/>
            /// overload via <see cref="IEquatable{T}"/>; this object-based override is required by the BCL contract but is
            /// never invoked on the hot path, so it is excluded from coverage analysis.
            /// </remarks>
            [ExcludeFromCodeCoverage]
            public override bool Equals(
                object obj) =>
                obj is ElementDelegateKey key && Equals(
                    key);

            /// <summary>
            /// Serves as the default hash function for the object.
            /// </summary>
            /// <remarks>The hash code is computed based on the values of the source element,
            /// destination element, and options key. This method is suitable for use in hashing algorithms and data
            /// structures such as a hash table.</remarks>
            /// <returns>A 32-bit signed integer hash code representing the current object.</returns>
            [SuppressMessage(
                "Style",
                "IDE0079:Remove unnecessary suppression",
                Justification = "Suppressions are intentionally preserved for consistency across builds and analyzers, ensuring stable behavior in performance-critical code paths.")]
            [SuppressMessage(
                "Style",
                "IDE0070:Use 'HashCode.Combine'",
                Justification = "Manual hash computation avoids struct overhead and enables better JIT inlining, which is critical for performance-sensitive cache key scenarios.")]
            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = _srcElement.GetHashCode();
                    hash = (hash * 397) ^ _destElement.GetHashCode();
                    hash = (hash * 397) ^ _optionsKey.GetHashCode();
                    return hash;
                }
            }
        }

        // -----------------------------------------------------------------------------------------
        // Type-inspection helpers
        // -----------------------------------------------------------------------------------------

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
