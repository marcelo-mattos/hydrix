using System;

namespace Hydrix.Benchmarks.Benchmarks
{
    /// <summary>
    /// Represents the smallest flat source model used by the mapper comparison benchmarks.
    /// </summary>
    /// <remarks>
    /// This type contains only five scalar members so the benchmark can isolate mapper overhead for a compact object
    /// graph before scaling up to wider payloads.
    /// </remarks>
    public sealed class FlatSmallSrc
    {
        /// <summary>
        /// Gets or sets the synthetic identifier value used by the small flat source model.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the synthetic display name used by the small flat source model.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the synthetic email value used by the small flat source model.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the synthetic age value used by the small flat source model.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the synthetic activity flag used by the small flat source model.
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Represents the smallest flat destination model used by the mapper comparison benchmarks.
    /// </summary>
    public sealed class FlatSmallDto
    {
        /// <summary>
        /// Gets or sets the mapped identifier value.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the mapped display name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the mapped email value.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the mapped age value.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the mapped activity flag.
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Represents the medium-width flat source model used by the mapper comparison benchmarks.
    /// </summary>
    /// <remarks>
    /// This type expands the number of mapped members to include strings, numerics, booleans, a date, and a GUID so the
    /// benchmarks can compare how each mapper behaves when the projection width increases.
    /// </remarks>
    public sealed class FlatMediumSrc
    {
        /// <summary>
        /// Gets or sets the synthetic identifier value.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the synthetic first name value.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the synthetic last name value.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the synthetic email value.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the synthetic phone value.
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the synthetic age value.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the synthetic salary value.
        /// </summary>
        public decimal Salary { get; set; }

        /// <summary>
        /// Gets or sets the synthetic activity flag.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the synthetic creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the synthetic external identifier.
        /// </summary>
        public Guid ExternalId { get; set; }

        /// <summary>
        /// Gets or sets the synthetic department value.
        /// </summary>
        public string Department { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the synthetic level value.
        /// </summary>
        public int Level { get; set; }
    }

    /// <summary>
    /// Represents the medium-width flat destination model used by the mapper comparison benchmarks.
    /// </summary>
    public sealed class FlatMediumDto
    {
        /// <summary>
        /// Gets or sets the mapped identifier value.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the mapped first name value.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the mapped last name value.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the mapped email value.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the mapped phone value.
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the mapped age value.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the mapped salary value.
        /// </summary>
        public decimal Salary { get; set; }

        /// <summary>
        /// Gets or sets the mapped activity flag.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the mapped creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the mapped external identifier.
        /// </summary>
        public Guid ExternalId { get; set; }

        /// <summary>
        /// Gets or sets the mapped department value.
        /// </summary>
        public string Department { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the mapped level value.
        /// </summary>
        public int Level { get; set; }
    }
}
