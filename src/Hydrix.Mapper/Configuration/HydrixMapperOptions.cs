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
    }
}
