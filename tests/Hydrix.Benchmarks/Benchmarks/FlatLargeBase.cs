using System;

namespace Hydrix.Benchmarks.Benchmarks
{
    /// <summary>
    /// Declares the twenty synthetic properties shared by <see cref="FlatLargeSrc"/> and <see cref="FlatLargeDto"/>,
    /// eliminating the duplicated member declarations between the two benchmark model classes.
    /// </summary>
    public abstract class FlatLargeBase
    {
        /// <summary>
        /// Gets or sets synthetic slot P01, modeled as an integer value.
        /// </summary>
        public int P01 { get; set; }

        /// <summary>
        /// Gets or sets synthetic slot P02, modeled as a string value.
        /// </summary>
        public string P02 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets synthetic slot P03, modeled as a string value.
        /// </summary>
        public string P03 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets synthetic slot P04, modeled as a string value.
        /// </summary>
        public string P04 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets synthetic slot P05, modeled as an integer value.
        /// </summary>
        public int P05 { get; set; }

        /// <summary>
        /// Gets or sets synthetic slot P06, modeled as a decimal value.
        /// </summary>
        public decimal P06 { get; set; }

        /// <summary>
        /// Gets or sets synthetic slot P07, modeled as a double value.
        /// </summary>
        public double P07 { get; set; }

        /// <summary>
        /// Gets or sets synthetic slot P08, modeled as a boolean value.
        /// </summary>
        public bool P08 { get; set; }

        /// <summary>
        /// Gets or sets synthetic slot P09, modeled as a timestamp value.
        /// </summary>
        public DateTime P09 { get; set; }

        /// <summary>
        /// Gets or sets synthetic slot P10, modeled as a GUID value.
        /// </summary>
        public Guid P10 { get; set; }

        /// <summary>
        /// Gets or sets synthetic slot P11, modeled as a long value.
        /// </summary>
        public long P11 { get; set; }

        /// <summary>
        /// Gets or sets synthetic slot P12, modeled as a short value.
        /// </summary>
        public short P12 { get; set; }

        /// <summary>
        /// Gets or sets synthetic slot P13, modeled as a float value.
        /// </summary>
        public float P13 { get; set; }

        /// <summary>
        /// Gets or sets synthetic slot P14, modeled as a string value.
        /// </summary>
        public string P14 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets synthetic slot P15, modeled as a string value.
        /// </summary>
        public string P15 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets synthetic slot P16, modeled as an integer value.
        /// </summary>
        public int P16 { get; set; }

        /// <summary>
        /// Gets or sets synthetic slot P17, modeled as an integer value.
        /// </summary>
        public int P17 { get; set; }

        /// <summary>
        /// Gets or sets synthetic slot P18, modeled as an integer value.
        /// </summary>
        public int P18 { get; set; }

        /// <summary>
        /// Gets or sets synthetic slot P19, modeled as an integer value.
        /// </summary>
        public int P19 { get; set; }

        /// <summary>
        /// Gets or sets synthetic slot P20, modeled as an integer value.
        /// </summary>
        public int P20 { get; set; }
    }
}
