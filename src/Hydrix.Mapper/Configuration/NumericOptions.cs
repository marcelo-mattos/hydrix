using Hydrix.Mapper.Primitives;

namespace Hydrix.Mapper.Configuration
{
    /// <summary>
    /// Defines the default numeric conversion rules used by the mapper when a source numeric type maps to a destination
    /// numeric type with different semantics.
    /// </summary>
    public sealed class NumericOptions
    {
        /// <summary>
        /// Gets or sets the rounding strategy applied when a decimal, double, or float source value is converted into an
        /// integral destination type.
        /// </summary>
        public NumericRounding DecimalToIntRounding { get; set; } = NumericRounding.Truncate;

        /// <summary>
        /// Gets or sets the overflow behavior applied after rounding when the converted value exceeds the destination range.
        /// </summary>
        public NumericOverflow Overflow { get; set; } = NumericOverflow.Truncate;
    }
}
