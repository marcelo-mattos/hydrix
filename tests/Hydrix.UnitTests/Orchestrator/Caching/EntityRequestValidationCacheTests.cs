using Hydrix.Orchestrator.Caching;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Caching
{
    /// <summary>
    /// Contains unit tests for validating entity request attributes in the EntityRequestValidationCache class.
    /// </summary>
    /// <remarks>This class tests various scenarios for entity validation, including the presence of table and
    /// column attributes, and caching behavior for repeated validations.</remarks>
    public class EntityRequestValidationCacheTests
    {
        /// <summary>
        /// Represents an entity with a unique identifier for database mapping.
        /// </summary>
        [Table("ValidEntity")]
        private class ValidEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [Column("Id")]
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents an entity that is mapped to the 'NoColumnEntity' table in the database.
        /// </summary>
        /// <remarks>This class contains only an identifier property and does not define additional
        /// columns. It can be used as a placeholder or for scenarios where only the primary key is required.</remarks>
        [Table("NoColumnEntity")]
        private class NoColumnEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents an entity that does not correspond to a database table, intended for in-memory operations or
        /// scenarios where persistence is not required.
        /// </summary>
        /// <remarks>This class is useful for modeling data that needs to be processed or validated
        /// without being stored in a database. The Id property uniquely identifies each instance of the
        /// entity.</remarks>
        private class NoTableEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [Column("Id")]
            public int Id { get; set; }
        }

        /// <summary>
        /// Verifies that the entity validation returns true when both the specified table and column are present in the
        /// model.
        /// </summary>
        /// <remarks>This test ensures that the Validate method of EntityRequestValidationCache correctly
        /// identifies a valid entity configuration. It is intended to confirm that the validation logic works as
        /// expected when all required schema elements exist.</remarks>
        [Fact]
        public void Validate_ReturnsTrue_WhenTableAndColumnPresent()
        {
            Assert.True(EntityRequestValidationCache.Validate(typeof(ValidEntity)));
        }

        /// <summary>
        /// Validates that an exception is thrown when the specified entity type lacks a TableAttribute.
        /// </summary>
        /// <remarks>This test verifies that the EntityRequestValidationCache.Validate method throws a
        /// MissingMemberException if the provided entity type does not have a TableAttribute. This ensures that entity
        /// types are properly configured for database mapping and that missing attributes are detected at validation
        /// time.</remarks>
        [Fact]
        public void Validate_Throws_WhenNoTableAttribute()
        {
            var ex = Assert.Throws<MissingMemberException>(() =>
                EntityRequestValidationCache.Validate(typeof(NoTableEntity)));
            Assert.Contains("TableAttribute", ex.Message);
        }

        /// <summary>
        /// Verifies that the EntityRequestValidationCache returns cached validation results for repeated requests of
        /// the same entity type.
        /// </summary>
        /// <remarks>This test ensures that after the initial validation of a specific entity type,
        /// subsequent validations for the same type utilize the cache rather than re-evaluating the entity. This
        /// behavior is important for performance and consistency in scenarios where entity validation is requested
        /// multiple times.</remarks>
        [Fact]
        public void Validate_UsesCache_ForSameType()
        {
            Assert.True(EntityRequestValidationCache.Validate(typeof(ValidEntity)));
            Assert.True(EntityRequestValidationCache.Validate(typeof(ValidEntity)));
        }
    }
}