using System;

namespace Hydrix.Benchmarks.Benchmarks
{
    /// <summary>
    /// Declares the twelve scalar properties shared by <see cref="FlatMediumSrc"/> and <see cref="FlatMediumDto"/>,
    /// eliminating the duplicated member declarations between the two benchmark model classes.
    /// </summary>
    public abstract class FlatMediumBase
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
}
