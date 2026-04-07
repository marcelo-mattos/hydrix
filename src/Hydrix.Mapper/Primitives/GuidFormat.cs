namespace Hydrix.Mapper.Primitives
{
    /// <summary>
    /// Defines the standard Guid format specifiers used when a Guid value is converted to a string.
    /// </summary>
    public enum GuidFormat
    {
        /// <summary>
        /// Uses the canonical 32 hexadecimal digits separated by hyphens.
        /// </summary>
        Hyphenated = 0,

        /// <summary>
        /// Uses 32 hexadecimal digits without separators.
        /// </summary>
        DigitsOnly = 1,

        /// <summary>
        /// Uses the canonical hyphenated format wrapped in braces.
        /// </summary>
        Braces = 2,

        /// <summary>
        /// Uses the canonical hyphenated format wrapped in parentheses.
        /// </summary>
        Parentheses = 3,
    }
}
