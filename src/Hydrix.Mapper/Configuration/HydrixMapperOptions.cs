using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Hydrix.Mapper.Configuration
{
    /// <summary>
    /// Represents the complete set of configurable rules used by <c>Hydrix.Mapper</c> when it compiles mapping plans.
    /// </summary>
    /// <remarks>
    /// Each option group controls a specific conversion family. The mapper reads these values when it builds the cached
    /// delegates for a source and destination pair, so the options behave as plan-compilation inputs rather than live
    /// per-call switches.
    /// </remarks>
    public sealed class HydrixMapperOptions
    {
        /// <summary>
        /// Stores the registered nested mapping relationships keyed by destination type.
        /// </summary>
        private readonly Dictionary<Type, Type> _nestedMappings = new Dictionary<Type, Type>();

        /// <summary>
        /// Caches the result of <c>GetCustomAttribute&lt;MapFromAttribute&gt;</c> per destination type so the reflection
        /// call is paid at most once per type across the entire process lifetime. The sentinel
        /// <see cref="NoMapFromAttribute"/> is stored when the type carries no attribute.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Type> MapFromAttributeCache =
            new ConcurrentDictionary<Type, Type>();

        /// <summary>
        /// Sentinel stored in <see cref="MapFromAttributeCache"/> when a destination type has no
        /// <see cref="Attributes.MapFromAttribute"/>. Using <c>typeof(void)</c> avoids a separate nullable wrapper
        /// and keeps the dictionary value type uniform.
        /// </summary>
        private static readonly Type NoMapFromAttribute = typeof(void);

        /// <summary>
        /// Gets the default rules used for string-to-string transformations.
        /// </summary>
        public StringOptions String { get; } = new StringOptions();

        /// <summary>
        /// Gets the default rules used when Guid values are formatted as strings.
        /// </summary>
        public GuidOptions Guid { get; } = new GuidOptions();

        /// <summary>
        /// Gets the default rules used for numeric conversions, including rounding and overflow handling.
        /// </summary>
        public NumericOptions Numeric { get; } = new NumericOptions();

        /// <summary>
        /// Gets the default rules used for date and time formatting.
        /// </summary>
        public DateTimeOptions DateTime { get; } = new DateTimeOptions();

        /// <summary>
        /// Gets the default rules used when boolean values are formatted as strings.
        /// </summary>
        public BoolOptions Bool { get; } = new BoolOptions();

        /// <summary>
        /// Gets or sets a value indicating whether mapping should fail when the destination type contains a writable
        /// property that does not have a matching readable source property.
        /// </summary>
        public bool StrictMode { get; set; }

        /// <summary>
        /// Gets the registered nested mapping relationships keyed by destination type.
        /// </summary>
        internal Dictionary<Type, Type> NestedMappings => _nestedMappings;

        /// <summary>
        /// Registers an explicit nested mapping relationship between a source entity type and a destination DTO type.
        /// </summary>
        /// <typeparam name="TSource">The source entity type to map from.</typeparam>
        /// <typeparam name="TDest">The destination DTO type to map to.</typeparam>
        public void MapNested<TSource, TDest>()
            where TSource : class
            where TDest : class
        {
            _nestedMappings[typeof(TDest)] = typeof(TSource);
        }

        /// <summary>
        /// Attempts to resolve the source type registered for the specified destination type, checking first the
        /// explicit registration dictionary and then falling back to the <see cref="Attributes.MapFromAttribute"/>
        /// on the destination type.
        /// </summary>
        /// <param name="destType">The destination type to look up.</param>
        /// <param name="sourceType">
        /// When this method returns <see langword="true"/>, contains the resolved source type; otherwise,
        /// <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when a source type was resolved for <paramref name="destType"/>; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        internal bool TryGetNestedSourceType(
            Type destType,
            out Type sourceType)
        {
            // Explicit registration always takes precedence and is already O(1).
            if (_nestedMappings.TryGetValue(
                    destType,
                    out sourceType))
            {
                return true;
            }

            // Fast path: cache hit — avoids GetCustomAttribute reflection on repeated calls.
            if (MapFromAttributeCache.TryGetValue(
                    destType,
                    out var cached))
            {
                if (cached == NoMapFromAttribute)
                {
                    sourceType = null;
                    return false;
                }

                sourceType = cached;
                return true;
            }

            // Cold path: reflect once and populate the process-wide cache.
            var attribute = destType.GetCustomAttribute<Attributes.MapFromAttribute>(
                inherit: false);

            if (attribute != null)
            {
                MapFromAttributeCache.TryAdd(destType, attribute.SourceType);
                sourceType = attribute.SourceType;
                return true;
            }

            MapFromAttributeCache.TryAdd(destType, NoMapFromAttribute);
            sourceType = null;
            return false;
        }

        /// <summary>
        /// Copies all key-value pairs from the supplied dictionary into the internal nested-mappings registry.
        /// </summary>
        /// <param name="source">The dictionary whose entries should be merged into this instance.</param>
        internal void ImportNestedMappings(
            Dictionary<Type, Type> source)
        {
            foreach (var pair in source)
            {
                _nestedMappings[pair.Key] = pair.Value;
            }
        }

        /// <summary>
        /// Creates a deep copy of this options instance, including all nested-mapping registrations.
        /// </summary>
        /// <returns>
        /// A new <see cref="HydrixMapperOptions"/> instance with identical configuration values and a separate
        /// nested-mappings registry that does not share state with the original.
        /// </returns>
        public HydrixMapperOptions Clone()
        {
            var clone = new HydrixMapperOptions
            {
                StrictMode = StrictMode,
            };

            clone.String.Transform = String.Transform;
            clone.Guid.Format = Guid.Format;
            clone.Guid.Case = Guid.Case;
            clone.Numeric.DecimalToIntRounding = Numeric.DecimalToIntRounding;
            clone.Numeric.Overflow = Numeric.Overflow;
            clone.DateTime.StringFormat = DateTime.StringFormat;
            clone.DateTime.TimeZone = DateTime.TimeZone;
            clone.DateTime.Culture = DateTime.Culture;
            clone.Bool.StringFormat = Bool.StringFormat;
            clone.Bool.TrueValue = Bool.TrueValue;
            clone.Bool.FalseValue = Bool.FalseValue;
            clone.ImportNestedMappings(_nestedMappings);

            return clone;
        }
    }
}
