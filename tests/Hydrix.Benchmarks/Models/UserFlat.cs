using Hydrix.Schemas.Contract;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hydrix.Benchmarks.Models
{
    /// <summary>
    /// Represents a user record mapped to the 'Users' table in the database. Provides properties for the user's
    /// identifier, name, age, and status.
    /// </summary>
    /// <remarks>This class is sealed to prevent inheritance and is intended for use as a data transfer object
    /// when interacting with user data in the database.</remarks>
    [Table("Users")]
    public sealed class UserFlat :
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
        /// <remarks>The name is initialized to an empty string. Ensure that the name is not null or empty
        /// when used in operations that require a valid name.</remarks>
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
        /// <remarks>The status indicates the user's availability and can be used to determine if the user
        /// is active, inactive, or in another state. Ensure to validate the status before applying any business logic
        /// based on it.</remarks>
        [Column("Status")]
        public UserStatus Status { get; set; }
    }
}
