using System;

namespace Hydrix.Benchmarks.Benchmarks
{
    /// <summary>
    /// Represents the wide flat source model used to stress mappers with a larger number of members.
    /// </summary>
    /// <remarks>
    /// The property names are intentionally synthetic because the benchmark is interested in object width and value-type
    /// diversity rather than in domain semantics.
    /// </remarks>
    public sealed class FlatLargeSrc
    {
        /// <summary>Gets or sets synthetic slot P01, modeled as an integer value.</summary>
        public int P01 { get; set; }

        /// <summary>Gets or sets synthetic slot P02, modeled as a string value.</summary>
        public string P02 { get; set; } = string.Empty;

        /// <summary>Gets or sets synthetic slot P03, modeled as a string value.</summary>
        public string P03 { get; set; } = string.Empty;

        /// <summary>Gets or sets synthetic slot P04, modeled as a string value.</summary>
        public string P04 { get; set; } = string.Empty;

        /// <summary>Gets or sets synthetic slot P05, modeled as an integer value.</summary>
        public int P05 { get; set; }

        /// <summary>Gets or sets synthetic slot P06, modeled as a decimal value.</summary>
        public decimal P06 { get; set; }

        /// <summary>Gets or sets synthetic slot P07, modeled as a double value.</summary>
        public double P07 { get; set; }

        /// <summary>Gets or sets synthetic slot P08, modeled as a boolean value.</summary>
        public bool P08 { get; set; }

        /// <summary>Gets or sets synthetic slot P09, modeled as a timestamp value.</summary>
        public DateTime P09 { get; set; }

        /// <summary>Gets or sets synthetic slot P10, modeled as a GUID value.</summary>
        public Guid P10 { get; set; }

        /// <summary>Gets or sets synthetic slot P11, modeled as a long value.</summary>
        public long P11 { get; set; }

        /// <summary>Gets or sets synthetic slot P12, modeled as a short value.</summary>
        public short P12 { get; set; }

        /// <summary>Gets or sets synthetic slot P13, modeled as a float value.</summary>
        public float P13 { get; set; }

        /// <summary>Gets or sets synthetic slot P14, modeled as a string value.</summary>
        public string P14 { get; set; } = string.Empty;

        /// <summary>Gets or sets synthetic slot P15, modeled as a string value.</summary>
        public string P15 { get; set; } = string.Empty;

        /// <summary>Gets or sets synthetic slot P16, modeled as an integer value.</summary>
        public int P16 { get; set; }

        /// <summary>Gets or sets synthetic slot P17, modeled as an integer value.</summary>
        public int P17 { get; set; }

        /// <summary>Gets or sets synthetic slot P18, modeled as an integer value.</summary>
        public int P18 { get; set; }

        /// <summary>Gets or sets synthetic slot P19, modeled as an integer value.</summary>
        public int P19 { get; set; }

        /// <summary>Gets or sets synthetic slot P20, modeled as an integer value.</summary>
        public int P20 { get; set; }
    }

    /// <summary>
    /// Represents the wide flat destination model used to receive mapped values from <see cref="FlatLargeSrc"/>.
    /// </summary>
    public sealed class FlatLargeDto
    {
        /// <summary>Gets or sets the mapped value for synthetic slot P01.</summary>
        public int P01 { get; set; }

        /// <summary>Gets or sets the mapped value for synthetic slot P02.</summary>
        public string P02 { get; set; } = string.Empty;

        /// <summary>Gets or sets the mapped value for synthetic slot P03.</summary>
        public string P03 { get; set; } = string.Empty;

        /// <summary>Gets or sets the mapped value for synthetic slot P04.</summary>
        public string P04 { get; set; } = string.Empty;

        /// <summary>Gets or sets the mapped value for synthetic slot P05.</summary>
        public int P05 { get; set; }

        /// <summary>Gets or sets the mapped value for synthetic slot P06.</summary>
        public decimal P06 { get; set; }

        /// <summary>Gets or sets the mapped value for synthetic slot P07.</summary>
        public double P07 { get; set; }

        /// <summary>Gets or sets the mapped value for synthetic slot P08.</summary>
        public bool P08 { get; set; }

        /// <summary>Gets or sets the mapped value for synthetic slot P09.</summary>
        public DateTime P09 { get; set; }

        /// <summary>Gets or sets the mapped value for synthetic slot P10.</summary>
        public Guid P10 { get; set; }

        /// <summary>Gets or sets the mapped value for synthetic slot P11.</summary>
        public long P11 { get; set; }

        /// <summary>Gets or sets the mapped value for synthetic slot P12.</summary>
        public short P12 { get; set; }

        /// <summary>Gets or sets the mapped value for synthetic slot P13.</summary>
        public float P13 { get; set; }

        /// <summary>Gets or sets the mapped value for synthetic slot P14.</summary>
        public string P14 { get; set; } = string.Empty;

        /// <summary>Gets or sets the mapped value for synthetic slot P15.</summary>
        public string P15 { get; set; } = string.Empty;

        /// <summary>Gets or sets the mapped value for synthetic slot P16.</summary>
        public int P16 { get; set; }

        /// <summary>Gets or sets the mapped value for synthetic slot P17.</summary>
        public int P17 { get; set; }

        /// <summary>Gets or sets the mapped value for synthetic slot P18.</summary>
        public int P18 { get; set; }

        /// <summary>Gets or sets the mapped value for synthetic slot P19.</summary>
        public int P19 { get; set; }

        /// <summary>Gets or sets the mapped value for synthetic slot P20.</summary>
        public int P20 { get; set; }
    }
}
