namespace Hydrix.Mapper.Primitives
{
    /// <summary>
    /// Defines the behavior used when a numeric conversion exceeds the destination type range.
    /// </summary>
    public enum NumericOverflow
    {
        /// <summary>
        /// Throws an overflow exception instead of producing a truncated or clamped value.
        /// </summary>
        Throw,

        /// <summary>
        /// Restricts the converted value to the valid minimum or maximum of the destination type.
        /// </summary>
        Clamp,

        /// <summary>
        /// Performs an unchecked cast and allows the value to wrap or lose high-order information.
        /// </summary>
        Truncate,
    }
}
