using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Mapping.Internal;
using Hydrix.Mapper.Plans;
using System;
using System.Collections.Concurrent;

namespace Hydrix.Mapper.Caching
{
    /// <summary>
    /// Provides the thread-safe cache that stores compiled mapping plans keyed by source type, destination type, and
    /// effective mapper configuration.
    /// </summary>
    internal static class MapPlanCache
    {
        /// <summary>
        /// Stores the compiled plans keyed by the unique source type, destination type, and option snapshot tuple.
        /// </summary>
        private static readonly ConcurrentDictionary<MapPlanKey, MapPlan> Cache =
            new ConcurrentDictionary<MapPlanKey, MapPlan>();

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

            if (Cache.TryGetValue(
                    key,
                    out var cachedPlan))
            {
                return cachedPlan;
            }

            var snapshot = optionsKey.ToOptions();
            return Cache.GetOrAdd(
                key,
                cacheKey => MapPlanBuilder.Build(
                    cacheKey.Source,
                    cacheKey.Destination,
                    snapshot));
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
            Type destType)
        {
            foreach (var key in Cache.Keys)
            {
                if (key.Source == sourceType && key.Destination == destType)
                    return true;
            }

            return false;
        }

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
            Cache.ContainsKey(
                new MapPlanKey(
                    sourceType,
                    destType,
                    MapPlanOptionsKey.Create(
                        options)));

        /// <summary>
        /// Removes every compiled plan from the cache.
        /// </summary>
        internal static void Clear() =>
            Cache.Clear();
    }

    /// <summary>
    /// Represents the immutable value-type key used to identify a cached mapping plan.
    /// </summary>
    internal readonly struct MapPlanKey : IEquatable<MapPlanKey>
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
        {
        }

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
    internal readonly struct MapPlanOptionsKey : IEquatable<MapPlanOptionsKey>
    {
        /// <summary>
        /// Stores the default mapper-option snapshot used by convenience overloads and tests.
        /// </summary>
        internal static readonly MapPlanOptionsKey Default = Create(
            new HydrixMapperOptions());

        /// <summary>
        /// Stores the configured string transformation pipeline.
        /// </summary>
        private readonly StringTransform _stringTransform;

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
        /// Initializes a new immutable option snapshot from the supplied effective values.
        /// </summary>
        /// <param name="stringTransform">
        /// The configured string transformation pipeline.
        /// </param>
        /// <param name="guidFormat">
        /// The configured Guid formatting specifier.
        /// </param>
        /// <param name="guidCase">
        /// The configured Guid letter casing.
        /// </param>
        /// <param name="numericRounding">
        /// The configured numeric rounding behavior.
        /// </param>
        /// <param name="numericOverflow">
        /// The configured numeric overflow behavior.
        /// </param>
        /// <param name="dateTimeFormat">
        /// The configured date and time format string.
        /// </param>
        /// <param name="dateTimeZone">
        /// The configured date and time timezone normalization.
        /// </param>
        /// <param name="dateTimeCulture">
        /// The configured date and time culture name.
        /// </param>
        /// <param name="boolStringFormat">
        /// The configured boolean string preset.
        /// </param>
        /// <param name="boolTrueValue">
        /// The configured custom <see langword="true"/> string.
        /// </param>
        /// <param name="boolFalseValue">
        /// The configured custom <see langword="false"/> string.
        /// </param>
        /// <param name="strictMode">
        /// The configured strict-mode flag.
        /// </param>
        private MapPlanOptionsKey(
            StringTransform stringTransform,
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
            bool strictMode)
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
                options.StrictMode);

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
            _strictMode == other._strictMode;

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
        /// A stable hash code computed from the effective mapper-option values captured by the snapshot.
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
                return hashCode;
            }
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
