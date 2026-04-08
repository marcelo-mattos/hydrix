namespace Hydrix.Mapper.Primitives
{
    /// <summary>
    /// Defines the supported enum projection styles.
    /// </summary>
    public enum EnumMapping
    {
        /// <summary>
        /// Projects the enum value as its textual name.
        /// </summary>
        AsString,

        /// <summary>
        /// Projects the enum value as its underlying integral representation.
        /// </summary>
        AsInt,
    }
}
