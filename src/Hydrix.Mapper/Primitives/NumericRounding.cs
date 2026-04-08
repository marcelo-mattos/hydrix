namespace Hydrix.Mapper.Primitives
{
    /// <summary>
    /// Defines the rounding behavior used when a floating-point or decimal value is converted to an integral type.
    /// </summary>
    public enum NumericRounding
    {
        /// <summary>
        /// Removes the fractional portion by truncating toward zero.
        /// </summary>
        Truncate,

        /// <summary>
        /// Rounds toward positive infinity.
        /// </summary>
        Ceiling,

        /// <summary>
        /// Rounds toward negative infinity.
        /// </summary>
        Floor,

        /// <summary>
        /// Rounds to the nearest integer and resolves ties away from zero.
        /// </summary>
        Nearest,

        /// <summary>
        /// Rounds to the nearest integer and resolves ties to the nearest even value.
        /// </summary>
        Banker,
    }
}
