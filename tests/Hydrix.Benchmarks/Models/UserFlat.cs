using Hydrix.Schemas.Contract;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hydrix.Benchmarks.Models
{
    /// <summary>
    /// Represents the flat user projection returned by the benchmark queries that target only user columns.
    /// </summary>
    /// <remarks>
    /// This type intentionally mirrors the columns selected by the flat SQL queries so Dapper, Hydrix, Entity
    /// Framework Core, and manual ADO.NET materialization all project into the same shape during measurements.
    /// </remarks>
    [Table("Users")]
    public sealed class UserFlat :
        ITable
    {
        /// <summary>
        /// Gets or sets the primary key value read from the <c>Users.Id</c> column.
        /// </summary>
        [Column("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user name read from the <c>Users.Name</c> column.
        /// </summary>
        /// <remarks>
        /// The property is initialized with <see cref="string.Empty"/> so ad-hoc object construction inside the benchmark
        /// project remains null-safe even before the database populates the instance.
        /// </remarks>
        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the age read from the <c>Users.Age</c> column.
        /// </summary>
        [Column("Age")]
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the logical user status read from the <c>Users.Status</c> column.
        /// </summary>
        [Column("Status")]
        public UserStatus Status { get; set; }
    }
}
