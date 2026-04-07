using Hydrix.Mapper.Primitives;
using System;

namespace Hydrix.Mapper.Attributes
{
    /// <summary>
    /// Overrides the global conversion rules for a single destination property.
    /// </summary>
    /// <remarks>
    /// The mapper reads this attribute during mapping-plan compilation and folds the selected values into the compiled
    /// delegate, which means the override has no per-call reflection overhead.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class MapConversionAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the string transformation pipeline applied when both the source and destination members are strings.
        /// </summary>
        public StringTransforms StringTransform { get; set; } = StringTransforms.None;

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="StringTransforms"/> must override the global string settings
        /// even when the selected transform is <see cref="StringTransforms.None"/>.
        /// </summary>
        public bool OverrideStringTransform { get; set; }

        /// <summary>
        /// Gets or sets the Guid format applied when a Guid source member is converted to a string destination member.
        /// </summary>
        public GuidFormat GuidFormat { get; set; } = GuidFormat.Hyphenated;

        /// <summary>
        /// Gets or sets a value indicating whether the Guid-related attribute values should override the global settings.
        /// </summary>
        public bool OverrideGuid { get; set; }

        /// <summary>
        /// Gets or sets the casing applied to hexadecimal Guid digits after formatting.
        /// </summary>
        public GuidCase GuidCase { get; set; } = GuidCase.Lower;

        /// <summary>
        /// Gets or sets the rounding strategy used when a decimal, double, or float source member is converted to an
        /// integral destination member.
        /// </summary>
        public NumericRounding NumericRounding { get; set; } = NumericRounding.Truncate;

        /// <summary>
        /// Gets or sets the overflow behavior applied after numeric rounding.
        /// </summary>
        public NumericOverflow NumericOverflow { get; set; } = NumericOverflow.Truncate;

        /// <summary>
        /// Gets or sets a value indicating whether the numeric attribute values should override the global settings.
        /// </summary>
        public bool OverrideNumeric { get; set; }

        /// <summary>
        /// Gets or sets the format string used when a date or time value is converted to text.
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// Gets or sets the timezone normalization applied before a date or time value is formatted.
        /// </summary>
        public DateTimeZone DateTimeZone { get; set; } = DateTimeZone.None;

        /// <summary>
        /// Gets or sets the culture name used to resolve the format provider for date and time formatting.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the date and time attribute values should override the global settings.
        /// </summary>
        public bool OverrideDateTime { get; set; }

        /// <summary>
        /// Gets or sets the bool-to-string format preset applied to the decorated destination property.
        /// </summary>
        public BoolStringFormat BoolFormat { get; set; } = BoolStringFormat.TrueOrFalse;

        /// <summary>
        /// Gets or sets the custom text emitted for a <see langword="true"/> value when <see cref="BoolFormat"/> is
        /// configured as <see cref="BoolStringFormat.Custom"/>.
        /// </summary>
        public string TrueValue { get; set; }

        /// <summary>
        /// Gets or sets the custom text emitted for a <see langword="false"/> value when <see cref="BoolFormat"/> is
        /// configured as <see cref="BoolStringFormat.Custom"/>.
        /// </summary>
        public string FalseValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the bool-related attribute values should override the global settings.
        /// </summary>
        public bool OverrideBool { get; set; }
    }
}
