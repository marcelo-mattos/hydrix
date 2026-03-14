using Hydrix.Attributes.Schemas;
using Hydrix.Schemas.Contract;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hydrix.Benchmarks.Models
{
    /// <summary>
    /// Represents a user entity that includes personal information and an associated order.
    /// </summary>
    /// <remarks>This class maps to the 'Users' table and includes a foreign key relationship to the user's
    /// order details via the 'Order' property. When retrieving user data, ensure that the 'Order' property is properly
    /// populated to access related order information.</remarks>
    [Table("Users")]
    public sealed class UserWithOrder :
        ITable
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [Column("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name associated with the entity.
        /// </summary>
        /// <remarks>The name is initialized to an empty string. It is recommended to provide a meaningful
        /// name that accurately represents the entity.</remarks>
        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the age of the individual.
        /// </summary>
        [Column("Age")]
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the current status of the user.
        /// </summary>
        /// <remarks>The status indicates the user's current state within the application, such as active,
        /// inactive, or suspended.</remarks>
        [Column("Status")]
        public UserStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the order associated with this entity.
        /// </summary>
        /// <remarks>This property is mapped to the Orders table. When querying, columns must be aliased
        /// as "Order.&lt;ColumnName&gt;" (for example, "Order.Id").</remarks>
        [ForeignTable("Orders", PrimaryKeys = new[] { "Id" })]
        public Order Order { get; set; }
    }
}