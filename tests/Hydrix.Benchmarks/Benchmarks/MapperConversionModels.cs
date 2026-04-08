using System;

namespace Hydrix.Benchmarks.Benchmarks
{
    /// <summary>
    /// Represents the source model used by the conversion benchmark scenarios.
    /// </summary>
    /// <remarks>
    /// This shape intentionally mixes string normalization, GUID formatting, date formatting, and numeric rounding so the
    /// benchmark can compare mapper behavior when conversions are required instead of plain assignment.
    /// </remarks>
    public sealed class ConversionSrc
    {
        /// <summary>
        /// Gets or sets the source string that requires trimming before assignment.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source GUID that must be formatted as text.
        /// </summary>
        public Guid ExternalId { get; set; }

        /// <summary>
        /// Gets or sets the source timestamp that must be formatted as text.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the source decimal score that must be rounded to an integer.
        /// </summary>
        public decimal Score { get; set; }
    }

    /// <summary>
    /// Represents the destination model used by the conversion benchmark scenarios.
    /// </summary>
    public sealed class ConversionDto
    {
        /// <summary>
        /// Gets or sets the trimmed destination string.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the formatted destination GUID text.
        /// </summary>
        public string ExternalId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the formatted destination timestamp text.
        /// </summary>
        public string CreatedAt { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the rounded destination score.
        /// </summary>
        public int Score { get; set; }
    }
}
