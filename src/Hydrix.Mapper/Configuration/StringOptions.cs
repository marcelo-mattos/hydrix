using Hydrix.Mapper.Primitives;

namespace Hydrix.Mapper.Configuration
{
    /// <summary>
    /// Defines the default string-to-string transformation rules used during mapping.
    /// </summary>
    public sealed class StringOptions
    {
        /// <summary>
        /// Gets or sets the transformation pipeline applied when both the source and destination members are strings.
        /// </summary>
        public StringTransforms Transform { get; set; } = StringTransforms.None;
    }
}
