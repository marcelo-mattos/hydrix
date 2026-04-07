namespace Hydrix.Mapper.Primitives
{
    /// <summary>
    /// Defines the built-in bool-to-string output presets.
    /// </summary>
    public enum BoolStringFormat
    {
        /// <summary>
        /// Uses the framework default <c>True</c> and <c>False</c> strings.
        /// </summary>
        TrueOrFalse,

        /// <summary>
        /// Uses lowercase <c>true</c> and <c>false</c> strings.
        /// </summary>
        LowercaseTrueOrFalse,

        /// <summary>
        /// Uses <c>Yes</c> and <c>No</c>.
        /// </summary>
        YesOrNo,

        /// <summary>
        /// Uses <c>Y</c> and <c>N</c>.
        /// </summary>
        YOrN,

        /// <summary>
        /// Uses <c>1</c> and <c>0</c>.
        /// </summary>
        OneOrZero,

        /// <summary>
        /// Uses <c>T</c> and <c>F</c>.
        /// </summary>
        TOrF,

        /// <summary>
        /// Uses the custom values supplied through the configuration or attribute override.
        /// </summary>
        Custom,
    }
}
