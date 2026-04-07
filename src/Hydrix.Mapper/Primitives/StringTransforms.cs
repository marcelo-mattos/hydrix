using System;

namespace Hydrix.Mapper.Primitives
{
    /// <summary>
    /// Describes the string transformations that can be applied when both the source and destination members are strings.
    /// </summary>
    /// <remarks>
    /// When multiple flags are combined, trimming operations are applied before casing operations.
    /// </remarks>
    [Flags]
    public enum StringTransforms
    {
        /// <summary>
        /// Indicates that no string transformation should be applied.
        /// </summary>
        None = 0,

        /// <summary>
        /// Removes whitespace from both the start and the end of the string.
        /// </summary>
        Trim = 1,

        /// <summary>
        /// Removes whitespace only from the beginning of the string.
        /// </summary>
        TrimStart = 2,

        /// <summary>
        /// Removes whitespace only from the end of the string.
        /// </summary>
        TrimEnd = 4,

        /// <summary>
        /// Converts the resulting string to uppercase by using invariant culture semantics.
        /// </summary>
        Uppercase = 8,

        /// <summary>
        /// Converts the resulting string to lowercase by using invariant culture semantics.
        /// </summary>
        Lowercase = 16,
    }
}
