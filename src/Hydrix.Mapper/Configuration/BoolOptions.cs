namespace Hydrix.Mapper.Configuration
{
    /// <summary>
    /// Defines the default bool-to-string conversion rules applied by the mapper when a boolean source property is assigned
    /// to a string destination property and no per-property override is present.
    /// </summary>
    /// <remarks>
    /// These options are evaluated during mapping-plan compilation. After a plan has been compiled for a source and
    /// destination pair, subsequent changes do not affect the already cached delegates for that pair.
    /// </remarks>
    public sealed class BoolOptions
    {
        /// <summary>
        /// Gets or sets the predefined formatting preset used when a boolean value is converted to text.
        /// </summary>
        /// <remarks>
        /// Use <see cref="BoolStringFormat.Custom"/> together with <see cref="TrueValue"/> and <see cref="FalseValue"/>
        /// when the built-in presets do not match the desired output contract.
        /// </remarks>
        public BoolStringFormat StringFormat { get; set; } = BoolStringFormat.TrueFalse;

        /// <summary>
        /// Gets or sets the custom text emitted for a <see langword="true"/> value when
        /// <see cref="StringFormat"/> is configured as <see cref="BoolStringFormat.Custom"/>.
        /// </summary>
        public string TrueValue { get; set; } = "true";

        /// <summary>
        /// Gets or sets the custom text emitted for a <see langword="false"/> value when
        /// <see cref="StringFormat"/> is configured as <see cref="BoolStringFormat.Custom"/>.
        /// </summary>
        public string FalseValue { get; set; } = "false";
    }
}
