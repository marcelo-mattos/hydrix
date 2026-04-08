using Hydrix.Mapper.Builders;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Plans;
using Hydrix.Mapper.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Hydrix.Mapper.Caching
{
    /// <summary>
    /// Provides the thread-safe cache that stores compiled mapping plans keyed by source type, destination type, and
    /// effective mapper configuration.
    /// </summary>
    internal static class MapPlanCache
    {
        /// <summary>
        /// Provides a thread-safe cache for storing and retrieving mapping plans associated with specific keys.
        /// </summary>
        /// <remarks>This dictionary uses lazy initialization to ensure that each mapping plan is created
        /// only once per key, even in multithreaded scenarios. The cache improves performance by avoiding redundant
        /// computation of mapping plans.</remarks>
        private static readonly ConcurrentDictionary<MapPlanKey, Lazy<MapPlan>> Cache =
            new ConcurrentDictionary<MapPlanKey, Lazy<MapPlan>>();

        /// <summary>
        /// Provides O(1) membership testing for whether any plan exists for a given source–destination type pair,
        /// regardless of which option snapshot was used when the plan was compiled.
        /// </summary>
        private static readonly ConcurrentDictionary<(Type Source, Type Destination), byte> TypePairIndex =
            new ConcurrentDictionary<(Type, Type), byte>();

        /// <summary>
        /// Stores the number of times a plan has been compiled.
        /// </summary>
        /// <remarks>This field is intended for internal tracking of plan compilation operations. It
        /// should not be accessed directly outside of the containing class or its internal subclasses.</remarks>
        private static int _planCompilationCount;

        /// <summary>
        /// Returns the cached mapping plan for the supplied type pair and option snapshot, creating and caching it on first
        /// use.
        /// </summary>
        /// <param name="sourceType">
        /// The source type used as the left side of the mapping-plan key.
        /// </param>
        /// <param name="destType">
        /// The destination type used as the right side of the mapping-plan key.
        /// </param>
        /// <param name="options">
        /// The option snapshot used when a new plan must be compiled.
        /// </param>
        /// <returns>
        /// The cached or newly created <see cref="MapPlan"/> for the requested type pair and configuration.
        /// </returns>
        internal static MapPlan GetOrAdd(
            Type sourceType,
            Type destType,
            HydrixMapperOptions options)
        {
            var optionsKey = MapPlanOptionsKey.Create(
                options);
            var key = new MapPlanKey(
                sourceType,
                destType,
                optionsKey);

            if (!Cache.TryGetValue(
                    key,
                    out var lazyPlan))
            {
                var snapshot = optionsKey.ToOptions();
                lazyPlan = Cache.GetOrAdd(
                    key,
                    _ => CreatePlanLazy(
                        sourceType,
                        destType,
                        snapshot));
            }

            return GetPlanValue(
                key,
                lazyPlan,
                sourceType,
                destType);
        }

        /// <summary>
        /// Retrieves an existing mapping plan from the cache or creates and adds a new one for the specified source and
        /// destination types with the given options.
        /// </summary>
        /// <remarks>If a mapping plan for the specified types and options already exists in the cache, it
        /// is returned. Otherwise, a new plan is created, added to the cache, and returned. This method is
        /// thread-safe.</remarks>
        /// <param name="sourceType">The type of the source object to map from. Cannot be null.</param>
        /// <param name="destType">The type of the destination object to map to. Cannot be null.</param>
        /// <param name="options">The mapping options to use when creating a new mapping plan. Cannot be null.</param>
        /// <param name="optionsKey">A key representing the mapping plan options, used to distinguish different mapping configurations.</param>
        /// <returns>A mapping plan that defines how to map objects from the specified source type to the destination type using
        /// the provided options.</returns>
        internal static MapPlan GetOrAdd(
            Type sourceType,
            Type destType,
            HydrixMapperOptions options,
            MapPlanOptionsKey optionsKey)
        {
            var key = new MapPlanKey(
                sourceType,
                destType,
                optionsKey);

            if (!Cache.TryGetValue(
                    key,
                    out var lazyPlan))
            {
                lazyPlan = Cache.GetOrAdd(
                    key,
                    _ => CreatePlanLazy(
                        sourceType,
                        destType,
                        options));
            }

            return GetPlanValue(
                key,
                lazyPlan,
                sourceType,
                destType);
        }

        /// <summary>
        /// Determines whether the cache already contains a compiled plan for the supplied type pair, regardless of the
        /// option snapshot used to build it.
        /// </summary>
        /// <param name="sourceType">
        /// The source type that composes the lookup key.
        /// </param>
        /// <param name="destType">
        /// The destination type that composes the lookup key.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when at least one plan for the pair is already cached; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        internal static bool IsCached(
            Type sourceType,
            Type destType) =>
            TypePairIndex.ContainsKey(
                (sourceType, destType));

        /// <summary>
        /// Determines whether the cache already contains a compiled plan for the supplied type pair and option snapshot.
        /// </summary>
        /// <param name="sourceType">
        /// The source type that composes the lookup key.
        /// </param>
        /// <param name="destType">
        /// The destination type that composes the lookup key.
        /// </param>
        /// <param name="options">
        /// The option snapshot that must match the cached plan.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when a plan for the pair and options is already cached; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        internal static bool IsCached(
            Type sourceType,
            Type destType,
            HydrixMapperOptions options) =>
            Cache.TryGetValue(
                new MapPlanKey(
                    sourceType,
                    destType,
                    MapPlanOptionsKey.Create(
                        options)),
                out var lazyPlan)
            && lazyPlan.IsValueCreated;

        /// <summary>
        /// Clears all cached data and resets internal counters used by the type materialization infrastructure.
        /// </summary>
        /// <remarks>This method removes all entries from the internal cache and type pair index, and
        /// resets the plan compilation count to zero. It is intended for internal use to ensure a clean state, such as
        /// during testing or when reinitializing the materializer system.</remarks>
        internal static void Clear()
        {
            Cache.Clear();
            TypePairIndex.Clear();
            Interlocked.Exchange(
                ref _planCompilationCount,
                0);
        }

        /// <summary>
        /// Gets the total number of times a query plan has been compiled during the application's execution.
        /// </summary>
        /// <remarks>This property is intended for diagnostic or monitoring purposes to track how often
        /// query plans are compiled. Frequent plan compilations may indicate suboptimal query caching or
        /// parameterization.</remarks>
        internal static int PlanCompilationCount =>
            Volatile.Read(
                ref _planCompilationCount);

        /// <summary>
        /// Creates a lazily initialized mapping plan for the specified source and destination types using the provided
        /// mapping options.
        /// </summary>
        /// <remarks>The mapping plan is compiled only once, even if accessed concurrently from multiple
        /// threads. Subsequent accesses return the same compiled plan.</remarks>
        /// <param name="sourceType">The type of the source object to map from. Cannot be null.</param>
        /// <param name="destType">The type of the destination object to map to. Cannot be null.</param>
        /// <param name="options">The mapping options to use when building the mapping plan. Cannot be null.</param>
        /// <returns>A Lazy&lt;MapPlan&gt; instance that initializes the mapping plan when first accessed.</returns>
        private static Lazy<MapPlan> CreatePlanLazy(
            Type sourceType,
            Type destType,
            HydrixMapperOptions options) =>
            new Lazy<MapPlan>(
                () =>
                {
                    Interlocked.Increment(
                        ref _planCompilationCount);

                    return MapPlanBuilder.Build(
                        sourceType,
                        destType,
                        options);
                },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Retrieves the mapping plan associated with the specified key and type pair, initializing it if necessary.
        /// </summary>
        /// <remarks>If the mapping plan initialization fails, the faulted entry is removed from the cache
        /// before the exception is rethrown. This method also ensures that the type pair index is updated for efficient
        /// cache lookups.</remarks>
        /// <param name="key">The key that uniquely identifies the mapping plan to retrieve.</param>
        /// <param name="lazyPlan">A lazy-initialized mapping plan to be evaluated and returned if not already created.</param>
        /// <param name="sourceType">The source type involved in the mapping operation.</param>
        /// <param name="destType">The destination type involved in the mapping operation.</param>
        /// <returns>The mapping plan associated with the specified key and type pair.</returns>
        private static MapPlan GetPlanValue(
            MapPlanKey key,
            Lazy<MapPlan> lazyPlan,
            Type sourceType,
            Type destType)
        {
            try
            {
                var plan = lazyPlan.Value;

                // Populate the secondary type-pair index so IsCached(Type, Type) remains O(1).
                TypePairIndex.TryAdd(
                    (sourceType, destType),
                    0);

                return plan;
            }
            catch
            {
                TryRemoveFaultedEntry(
                    key,
                    lazyPlan);
                throw;
            }
        }

        /// <summary>
        /// Attempts to remove the specified cache entry if it matches the provided lazy plan instance.
        /// </summary>
        /// <remarks>
        /// <para>This method ensures that only the exact faulted entry associated with the provided key and lazy plan
        /// instance is removed from the cache. If the cache entry does not match the provided instance, no action is
        /// taken.</para>
        /// <para>The branch where <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey,TValue}.TryGetValue"/>
        /// returns <see langword="false"/> is only reachable under a concurrent race: a second thread calling this method
        /// observes that the first thread already removed the entry. Coverage instrumentation cannot reliably trigger this
        /// window, so the method is excluded from branch analysis.</para>
        /// </remarks>
        /// <param name="key">The key identifying the cache entry to remove.</param>
        /// <param name="lazyPlan">The lazy plan instance to compare against the cached entry. The entry is removed only if it is the same
        /// instance.</param>
        [ExcludeFromCodeCoverage]
        private static void TryRemoveFaultedEntry(
            MapPlanKey key,
            Lazy<MapPlan> lazyPlan)
        {
            if (Cache.TryGetValue(
                    key,
                    out var cachedLazy)
                && ReferenceEquals(
                    cachedLazy,
                    lazyPlan))
            {
                Cache.TryRemove(
                    key,
                    out _);
            }
        }
    }

    /// <summary>
    /// Represents the immutable value-type key used to identify a cached mapping plan.
    /// </summary>
    internal readonly struct MapPlanKey :
        IEquatable<MapPlanKey>
    {
        /// <summary>
        /// Stores the source type portion of the cache key.
        /// </summary>
        internal readonly Type Source;

        /// <summary>
        /// Stores the destination type portion of the cache key.
        /// </summary>
        internal readonly Type Destination;

        /// <summary>
        /// Stores the option snapshot portion of the cache key.
        /// </summary>
        internal readonly MapPlanOptionsKey Options;

        /// <summary>
        /// Initializes a new cache key for the supplied source type and destination type by using the default mapper-option
        /// snapshot.
        /// </summary>
        /// <param name="source">
        /// The source type component of the key.
        /// </param>
        /// <param name="destination">
        /// The destination type component of the key.
        /// </param>
        internal MapPlanKey(
            Type source,
            Type destination)
            : this(
                source,
                destination,
                MapPlanOptionsKey.Default)
        { }

        /// <summary>
        /// Initializes a new cache key for the supplied source type, destination type, and option snapshot.
        /// </summary>
        /// <param name="source">
        /// The source type component of the key.
        /// </param>
        /// <param name="destination">
        /// The destination type component of the key.
        /// </param>
        /// <param name="options">
        /// The option snapshot component of the key.
        /// </param>
        internal MapPlanKey(
            Type source,
            Type destination,
            MapPlanOptionsKey options)
        {
            Source = source;
            Destination = destination;
            Options = options;
        }

        /// <summary>
        /// Determines whether the current key matches another cache key.
        /// </summary>
        /// <param name="other">
        /// The other cache key to compare with the current instance.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when both keys contain the same source type, destination type, and option snapshot;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(
            MapPlanKey other) =>
            Source == other.Source &&
            Destination == other.Destination &&
            Options.Equals(
                other.Options);

        /// <summary>
        /// Determines whether the current key matches another object instance.
        /// </summary>
        /// <param name="obj">
        /// The object to compare with the current cache key.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when <paramref name="obj"/> is a <see cref="MapPlanKey"/> with the same source type,
        /// destination type, and option snapshot; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(
            object obj) =>
            obj is MapPlanKey key && Equals(
                key);

        /// <summary>
        /// Returns the hash code used by the concurrent dictionary for this cache key.
        /// </summary>
        /// <returns>
        /// A stable hash code computed from the source type, destination type, and option snapshot.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Source.GetHashCode() * 397) ^ Destination.GetHashCode();
                return (hashCode * 397) ^ Options.GetHashCode();
            }
        }
    }

    /// <summary>
    /// Represents the immutable option snapshot used to segment cached plans by effective mapper configuration.
    /// </summary>
    /// <remarks>
    /// The nested-mapping portion of this key uses structural (content-based) equality so that two independently
    /// created <see cref="HydrixMapperOptions"/> instances with identical nested-mapping registrations produce the same
    /// cache key. This prevents cache fragmentation that would otherwise occur when each <see cref="HydrixMapper"/>
    /// instance clones the options on construction.
    /// </remarks>
    internal readonly struct MapPlanOptionsKey :
        IEquatable<MapPlanOptionsKey>
    {
        /// <summary>
        /// Stores the default mapper-option snapshot used by convenience overloads and tests.
        /// </summary>
        internal static readonly MapPlanOptionsKey Default = Create(
            new HydrixMapperOptions());

        /// <summary>
        /// Stores the configured string transformation pipeline.
        /// </summary>
        private readonly StringTransforms _stringTransform;

        /// <summary>
        /// Stores the configured Guid formatting specifier.
        /// </summary>
        private readonly GuidFormat _guidFormat;

        /// <summary>
        /// Stores the configured Guid letter casing.
        /// </summary>
        private readonly GuidCase _guidCase;

        /// <summary>
        /// Stores the configured numeric rounding behavior.
        /// </summary>
        private readonly NumericRounding _numericRounding;

        /// <summary>
        /// Stores the configured numeric overflow behavior.
        /// </summary>
        private readonly NumericOverflow _numericOverflow;

        /// <summary>
        /// Stores the configured date and time format string.
        /// </summary>
        private readonly string _dateTimeFormat;

        /// <summary>
        /// Stores the configured date and time timezone normalization.
        /// </summary>
        private readonly DateTimeZone _dateTimeZone;

        /// <summary>
        /// Stores the configured date and time culture name.
        /// </summary>
        private readonly string _dateTimeCulture;

        /// <summary>
        /// Stores the configured boolean string preset.
        /// </summary>
        private readonly BoolStringFormat _boolStringFormat;

        /// <summary>
        /// Stores the configured custom <see langword="true"/> string.
        /// </summary>
        private readonly string _boolTrueValue;

        /// <summary>
        /// Stores the configured custom <see langword="false"/> string.
        /// </summary>
        private readonly string _boolFalseValue;

        /// <summary>
        /// Stores the configured strict-mode flag.
        /// </summary>
        private readonly bool _strictMode;

        /// <summary>
        /// Stores a snapshot of the nested-mapping registrations for structural equality comparison.
        /// <see langword="null"/> when no nested mappings are registered.
        /// </summary>
        private readonly Dictionary<Type, Type> _nestedMappings;

        /// <summary>
        /// Stores the pre-computed structural hash of <see cref="_nestedMappings"/> so that
        /// <see cref="GetHashCode"/> can incorporate nested-mapping content without iterating the
        /// dictionary on every call.
        /// </summary>
        private readonly int _nestedMappingsHash;

        /// <summary>
        /// Initializes a new immutable option snapshot from the supplied effective values.
        /// </summary>
        [SuppressMessage(
            "Major Code Smell",
            "S107:Methods should not have too many parameters",
            Justification = "Performance-critical internal method. Parameters are passed explicitly to avoid additional allocations, indirection, or context objects, ensuring optimal JIT inlining and minimal overhead during expression tree construction.")]
        private MapPlanOptionsKey(
            StringTransforms stringTransform,
            GuidFormat guidFormat,
            GuidCase guidCase,
            NumericRounding numericRounding,
            NumericOverflow numericOverflow,
            string dateTimeFormat,
            DateTimeZone dateTimeZone,
            string dateTimeCulture,
            BoolStringFormat boolStringFormat,
            string boolTrueValue,
            string boolFalseValue,
            bool strictMode,
            Dictionary<Type, Type> nestedMappings)
        {
            _stringTransform = stringTransform;
            _guidFormat = guidFormat;
            _guidCase = guidCase;
            _numericRounding = numericRounding;
            _numericOverflow = numericOverflow;
            _dateTimeFormat = dateTimeFormat;
            _dateTimeZone = dateTimeZone;
            _dateTimeCulture = dateTimeCulture;
            _boolStringFormat = boolStringFormat;
            _boolTrueValue = boolTrueValue;
            _boolFalseValue = boolFalseValue;
            _strictMode = strictMode;
            _nestedMappings = nestedMappings;
            _nestedMappingsHash = nestedMappings != null
                ? ComputeNestedMappingsHash(nestedMappings)
                : 0;
        }

        /// <summary>
        /// Creates a new option snapshot from the supplied mapper options instance.
        /// </summary>
        /// <param name="options">
        /// The mapper options whose current effective values should be captured.
        /// </param>
        /// <returns>
        /// A value-type snapshot that can safely participate in cache-key equality and hashing.
        /// </returns>
        internal static MapPlanOptionsKey Create(
            HydrixMapperOptions options) =>
            new MapPlanOptionsKey(
                options.String.Transform,
                options.Guid.Format,
                options.Guid.Case,
                options.Numeric.DecimalToIntRounding,
                options.Numeric.Overflow,
                options.DateTime.StringFormat,
                options.DateTime.TimeZone,
                options.DateTime.Culture,
                options.Bool.StringFormat,
                options.Bool.TrueValue,
                options.Bool.FalseValue,
                options.StrictMode,
                options.NestedMappings.Count > 0
                    ? options.NestedMappings
                    : null);

        /// <summary>
        /// Creates a detached options instance that mirrors the captured values.
        /// </summary>
        /// <returns>
        /// A new <see cref="HydrixMapperOptions"/> instance populated from the current snapshot.
        /// </returns>
        internal HydrixMapperOptions ToOptions()
        {
            var options = new HydrixMapperOptions
            {
                StrictMode = _strictMode,
            };

            options.String.Transform = _stringTransform;
            options.Guid.Format = _guidFormat;
            options.Guid.Case = _guidCase;
            options.Numeric.DecimalToIntRounding = _numericRounding;
            options.Numeric.Overflow = _numericOverflow;
            options.DateTime.StringFormat = _dateTimeFormat;
            options.DateTime.TimeZone = _dateTimeZone;
            options.DateTime.Culture = _dateTimeCulture;
            options.Bool.StringFormat = _boolStringFormat;
            options.Bool.TrueValue = _boolTrueValue;
            options.Bool.FalseValue = _boolFalseValue;

            if (_nestedMappings != null &&
                _nestedMappings.Count > 0)
            {
                options.ImportNestedMappings(
                    _nestedMappings);
            }

            return options;
        }

        /// <summary>
        /// Determines whether the current option snapshot matches another snapshot.
        /// </summary>
        /// <param name="other">
        /// The other option snapshot to compare with the current instance.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when both snapshots contain the same effective mapper-option values; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Equals(
            MapPlanOptionsKey other) =>
            _stringTransform == other._stringTransform &&
            _guidFormat == other._guidFormat &&
            _guidCase == other._guidCase &&
            _numericRounding == other._numericRounding &&
            _numericOverflow == other._numericOverflow &&
            string.Equals(
                _dateTimeFormat,
                other._dateTimeFormat,
                StringComparison.Ordinal) &&
            _dateTimeZone == other._dateTimeZone &&
            string.Equals(
                _dateTimeCulture,
                other._dateTimeCulture,
                StringComparison.Ordinal) &&
            _boolStringFormat == other._boolStringFormat &&
            string.Equals(
                _boolTrueValue,
                other._boolTrueValue,
                StringComparison.Ordinal) &&
            string.Equals(
                _boolFalseValue,
                other._boolFalseValue,
                StringComparison.Ordinal) &&
            _strictMode == other._strictMode &&
            _nestedMappingsHash == other._nestedMappingsHash &&
            NestedMappingsEqual(
                _nestedMappings,
                other._nestedMappings);

        /// <summary>
        /// Determines whether the current option snapshot matches another object instance.
        /// </summary>
        /// <param name="obj">
        /// The object to compare with the current option snapshot.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when <paramref name="obj"/> is a <see cref="MapPlanOptionsKey"/> with the same captured
        /// values; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(
            object obj) =>
            obj is MapPlanOptionsKey key && Equals(
                key);

        /// <summary>
        /// Returns the hash code used by the concurrent dictionary for this option snapshot.
        /// </summary>
        /// <returns>
        /// A stable hash code computed from the effective mapper-option values captured by the snapshot. The nested-mapping
        /// portion uses the pre-computed structural hash so the dictionary lookup stays O(1).
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_stringTransform;
                hashCode = (hashCode * 397) ^ (int)_guidFormat;
                hashCode = (hashCode * 397) ^ (int)_guidCase;
                hashCode = (hashCode * 397) ^ (int)_numericRounding;
                hashCode = (hashCode * 397) ^ (int)_numericOverflow;
                hashCode = (hashCode * 397) ^ GetStringHashCode(
                    _dateTimeFormat);
                hashCode = (hashCode * 397) ^ (int)_dateTimeZone;
                hashCode = (hashCode * 397) ^ GetStringHashCode(
                    _dateTimeCulture);
                hashCode = (hashCode * 397) ^ (int)_boolStringFormat;
                hashCode = (hashCode * 397) ^ GetStringHashCode(
                    _boolTrueValue);
                hashCode = (hashCode * 397) ^ GetStringHashCode(
                    _boolFalseValue);
                hashCode = (hashCode * 397) ^ _strictMode.GetHashCode();
                hashCode = (hashCode * 397) ^ _nestedMappingsHash;
                return hashCode;
            }
        }

        /// <summary>
        /// Computes a stable structural hash of the supplied nested-mapping dictionary by sorting entries by destination
        /// type assembly-qualified name before combining their type-handle hashes. Sorting ensures the result is
        /// independent of dictionary insertion order.
        /// </summary>
        /// <param name="dict">The non-null, non-empty nested-mapping dictionary to hash.</param>
        /// <returns>A stable integer hash of the dictionary contents.</returns>
        private static int ComputeNestedMappingsHash(
            Dictionary<Type, Type> dict)
        {
            unchecked
            {
                var hash = 0;

                // Sort by destination-type name for a stable ordering that is independent of
                // dictionary insertion order and consistent across independently created instances.
                foreach (var kv in dict.OrderBy(
                    k => k.Key.AssemblyQualifiedName,
                    StringComparer.Ordinal))
                {
                    // RuntimeTypeHandle gives a stable, allocation-free integer per type.
                    hash = (hash * 397) ^ kv.Key.TypeHandle.GetHashCode();
                    hash = (hash * 397) ^ kv.Value.TypeHandle.GetHashCode();
                }

                return hash;
            }
        }

        /// <summary>
        /// Compares two nested-mapping dictionaries for structural (content) equality. A fast-path reference check and
        /// count pre-screen keep this O(1) for identical instances or empty pairs.
        /// </summary>
        /// <param name="a">The first dictionary, or <see langword="null"/> when no mappings are registered.</param>
        /// <param name="b">The second dictionary, or <see langword="null"/> when no mappings are registered.</param>
        /// <returns>
        /// <see langword="true"/> when both dictionaries contain exactly the same source–destination type registrations;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The defensive null and count checks, and the value-mismatch branch inside the loop, are only reachable under
        /// hash collisions — which cannot be engineered in unit tests — so this method is excluded from coverage analysis.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        private static bool NestedMappingsEqual(
            Dictionary<Type, Type> a,
            Dictionary<Type, Type> b)
        {
            if (ReferenceEquals(a, b))
                return true;

            var aCount = a?.Count ?? 0;
            var bCount = b?.Count ?? 0;

            if (aCount != bCount)
                return false;

            if (aCount == 0)
                return true;

            // At this point both are non-null and have the same non-zero count.
            foreach (var kv in a)
            {
                if (!b.TryGetValue(kv.Key, out var bSrc) || bSrc != kv.Value)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the ordinal string hash code used by the option snapshot hash implementation.
        /// </summary>
        /// <param name="value">
        /// The string value to hash.
        /// </param>
        /// <returns>
        /// The ordinal hash code for <paramref name="value"/>, or zero when the value is <see langword="null"/>.
        /// </returns>
        private static int GetStringHashCode(
            string value) =>
            value == null
                ? 0
                : StringComparer.Ordinal.GetHashCode(
                    value);
    }
}
