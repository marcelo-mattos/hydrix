using System;

namespace Hydrix.Mapper
{
    /// <summary>
    /// Describes the string transformations that can be applied when both the source and destination members are strings.
    /// </summary>
    /// <remarks>
    /// When multiple flags are combined, trimming operations are applied before casing operations.
    /// </remarks>
    [Flags]
    public enum StringTransform
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

    /// <summary>
    /// Defines the standard Guid format specifiers used when a Guid value is converted to a string.
    /// </summary>
    public enum GuidFormat
    {
        /// <summary>
        /// Uses the canonical 32 hexadecimal digits separated by hyphens.
        /// </summary>
        D,

        /// <summary>
        /// Uses 32 hexadecimal digits without separators.
        /// </summary>
        N,

        /// <summary>
        /// Uses the canonical hyphenated format wrapped in braces.
        /// </summary>
        B,

        /// <summary>
        /// Uses the canonical hyphenated format wrapped in parentheses.
        /// </summary>
        P,
    }

    /// <summary>
    /// Defines the casing applied to hexadecimal Guid digits after formatting.
    /// </summary>
    public enum GuidCase
    {
        /// <summary>
        /// Emits lowercase hexadecimal digits.
        /// </summary>
        Lower,

        /// <summary>
        /// Emits uppercase hexadecimal digits.
        /// </summary>
        Upper,
    }

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

    /// <summary>
    /// Defines the timezone normalization performed before a date or time value is formatted as a string.
    /// </summary>
    public enum DateTimeZone
    {
        /// <summary>
        /// Leaves the value unchanged before formatting.
        /// </summary>
        None,

        /// <summary>
        /// Converts the value to Coordinated Universal Time before formatting.
        /// </summary>
        ToUtc,

        /// <summary>
        /// Converts the value to local time before formatting.
        /// </summary>
        ToLocal,
    }

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

    /// <summary>
    /// Defines the built-in bool-to-string output presets.
    /// </summary>
    public enum BoolStringFormat
    {
        /// <summary>
        /// Uses the framework default <c>True</c> and <c>False</c> strings.
        /// </summary>
        TrueFalse,

        /// <summary>
        /// Uses lowercase <c>true</c> and <c>false</c> strings.
        /// </summary>
        LowerCase,

        /// <summary>
        /// Uses <c>Yes</c> and <c>No</c>.
        /// </summary>
        YesNo,

        /// <summary>
        /// Uses <c>Y</c> and <c>N</c>.
        /// </summary>
        YN,

        /// <summary>
        /// Uses <c>1</c> and <c>0</c>.
        /// </summary>
        OneZero,

        /// <summary>
        /// Uses <c>S</c> and <c>N</c>.
        /// </summary>
        SN,

        /// <summary>
        /// Uses <c>Sim</c> and <c>Nao</c>.
        /// </summary>
        SimNao,

        /// <summary>
        /// Uses <c>T</c> and <c>F</c>.
        /// </summary>
        TF,

        /// <summary>
        /// Uses the custom values supplied through the configuration or attribute override.
        /// </summary>
        Custom,
    }
}
