using Hydrix.Attributes.Schemas;
using Hydrix.Schemas.Contract;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hydrix.Benchmarks.Models
{
    /// <summary>
    /// Represents the nested user projection returned by the join benchmarks that materialize a related order.
    /// </summary>
    /// <remarks>
    /// The benchmark queries hydrate this type from the <c>Users</c> table and then assign the nested
    /// <see cref="Order"/> instance from joined order columns so each data access strategy produces the same object
    /// graph for comparison.
    /// </remarks>
    [Table("Users")]
    public sealed class UserWithOrder :
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
        /// The property starts with <see cref="string.Empty"/> so ad-hoc object creation inside the benchmark project
        /// remains null-safe even before database materialization occurs.
        /// </remarks>
        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the age read from the <c>Users.Age</c> column.
        /// </summary>
        [Column("Age")]
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the logical status read from the <c>Users.Status</c> column.
        /// </summary>
        [Column("Status")]
        public UserStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the related order materialized from the joined order columns.
        /// </summary>
        /// <remarks>
        /// Hydrix expects the SQL projection for this member to alias nested columns using the <c>Order.&lt;Column&gt;</c>
        /// pattern so the framework can bind the child object correctly during nested mapping benchmarks.
        /// </remarks>
        [ForeignTable("Orders", PrimaryKeys = new[] { "Id" })]
        public Order Order { get; set; } = null!;
    }
}
