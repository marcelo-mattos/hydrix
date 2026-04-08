using Hydrix.Mapper.Primitives;

namespace Hydrix.Mapper.Configuration
{
    /// <summary>
    /// Defines the default formatting and timezone rules used when date and time values are converted to strings.
    /// </summary>
    /// <remarks>
    /// The configured values are consumed by the mapping-plan builder to create the expression tree that formats the
    /// source member. The defaults therefore affect every compatible destination property that does not declare a
    /// dedicated <c>MapConversionAttribute</c> override.
    /// </remarks>
    public sealed class DateTimeOptions
    {
        /// <summary>
        /// Gets or sets the format string passed to <c>ToString(format, culture)</c> when serializing a date or time value.
        /// </summary>
        public string StringFormat { get; set; } = "O";

        /// <summary>
        /// Gets or sets the timezone normalization applied before a date or time value is formatted.
        /// </summary>
        public DateTimeZone TimeZone { get; set; } = DateTimeZone.None;

        /// <summary>
        /// Gets or sets the culture name used to resolve the <see cref="System.IFormatProvider"/> employed during
        /// date and time formatting.
        /// </summary>
        /// <remarks>
        /// An empty string instructs the mapper to use <see cref="System.Globalization.CultureInfo.InvariantCulture"/>.
        /// </remarks>
        public string Culture { get; set; } = string.Empty;
    }
}
