using Hydrix.Mapper.Primitives;

namespace Hydrix.Mapper.Configuration
{
    /// <summary>
    /// Defines the default Guid-to-string formatting rules applied when a Guid source property maps to a string destination.
    /// </summary>
    public sealed class GuidOptions
    {
        /// <summary>
        /// Gets or sets the standard Guid format specifier used to generate the textual representation of the value.
        /// </summary>
        public GuidFormat Format { get; set; } = GuidFormat.Hyphenated;

        /// <summary>
        /// Gets or sets the letter casing applied to hexadecimal Guid digits after formatting.
        /// </summary>
        public GuidCase Case { get; set; } = GuidCase.Lower;
    }
}
