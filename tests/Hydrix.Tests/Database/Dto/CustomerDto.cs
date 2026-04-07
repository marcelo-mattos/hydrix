using System;
using System.Text.Json.Serialization;

namespace Hydrix.Tests.Database.Dto
{
    /// <summary>
    /// Represents a data transfer object for customer information, including identification, personal details, and
    /// status.
    /// </summary>
    /// <remarks>This class is typically used to transfer customer data between application layers or
    /// services. All properties are mapped to corresponding JSON fields for serialization and deserialization
    /// purposes.</remarks>
    public class CustomerDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name associated with this instance.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the birth date associated with the entity.
        /// </summary>
        [JsonPropertyName("birthdate")]
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// Gets or sets the level value associated with this instance.
        /// </summary>
        [JsonPropertyName("level")]
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the salary amount associated with the entity.
        /// </summary>
        [JsonPropertyName("salary")]
        public decimal? Salary { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is active.
        /// </summary>
        [JsonPropertyName("is_active")]
        public bool? IsActive { get; set; }
    }
}
