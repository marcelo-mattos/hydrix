using Hydrix.Attributes.Schemas;
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
    /// mappable members, and caching behavior for repeated validations.</remarks>
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
        /// Represents an entity mapped by convention through its public properties.
        /// </summary>
        [Table("NoColumnEntity")]
        private class NoColumnEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents an entity that has no properties eligible for mapping.
        /// </summary>
        [Table("NoMappedMembersEntity")]
        private class NoMappedMembersEntity
        {
            /// <summary>
            /// Gets or sets a value ignored by Hydrix mapping.
            /// </summary>
            [NotMapped]
            public int Ignored { get; set; }
        }

        /// <summary>
        /// Represents an entity that only exposes foreign table navigation properties.
        /// </summary>
        [Table("ForeignOnlyEntity")]
        private class ForeignOnlyEntity
        {
            /// <summary>
            /// Gets or sets a foreign table navigation property that should not be considered scalar-mappable.
            /// </summary>
            [ForeignTable("Child")]
            public ChildEntity Child { get; set; }
        }

        /// <summary>
        /// Represents an entity containing both scalar-mappable and foreign table navigation properties.
        /// </summary>
        [Table("MixedForeignEntity")]
        private class MixedForeignEntity
        {
            /// <summary>
            /// Gets or sets the scalar identifier that should be considered mappable.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets a foreign table navigation property that should be ignored by scalar validation.
            /// </summary>
            [ForeignTable("Child")]
            public ChildEntity Child { get; set; }
        }

        /// <summary>
        /// Represents a child entity type used for foreign table navigation properties in tests.
        /// </summary>
        private class ChildEntity
        {
            /// <summary>
            /// Gets or sets the child identifier.
            /// </summary>
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents an entity that only exposes static properties and therefore has no mappable instance members.
        /// </summary>
        [Table("StaticOnlyEntity")]
        private class StaticOnlyEntity
        {
            /// <summary>
            /// Gets or sets a static value that should not be considered mappable.
            /// </summary>
            public static int StaticValue { get; set; }
        }

        /// <summary>
        /// Represents an entity that only exposes an indexer and therefore has no regular mappable instance members.
        /// </summary>
        [Table("IndexerOnlyEntity")]
        private class IndexerOnlyEntity
        {
            /// <summary>
            /// Gets or sets a value by index. This indexer should not be considered mappable.
            /// </summary>
            /// <param name="index">The index position to access.</param>
            /// <returns>The value at the specified index.</returns>
            public string this[int index]
            {
                get => string.Empty;
                set { }
            }
        }

        /// <summary>
        /// Represents an entity that does not correspond to a database table, intended for in-memory operations or
        /// scenarios where persistence is not required.
        /// </summary>
        private class NoTableEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [Column("Id")]
            public int Id { get; set; }
        }

        /// <summary>
        /// Verifies that the entity validation returns true when a mapped member is present.
        /// </summary>
        [Fact]
        public void Validate_ReturnsTrue_WhenTableAndColumnPresent()
        {
            Assert.True(EntityRequestValidationCache.Validate(typeof(ValidEntity)));
        }

        /// <summary>
        /// Verifies that convention-based properties still count as valid mapped members.
        /// </summary>
        [Fact]
        public void Validate_ReturnsTrue_WhenTableAndConventionMappedPropertyPresent()
        {
            Assert.True(EntityRequestValidationCache.Validate(typeof(NoColumnEntity)));
        }

        /// <summary>
        /// Verifies that validation returns false when the entity has no mappable properties.
        /// </summary>
        [Fact]
        public void Validate_ReturnsFalse_WhenNoMappablePropertiesExist()
        {
            Assert.False(EntityRequestValidationCache.Validate(typeof(NoMappedMembersEntity)));
        }

        /// <summary>
        /// Verifies that validation returns false when only foreign table navigation properties exist.
        /// </summary>
        [Fact]
        public void Validate_ReturnsFalse_WhenOnlyForeignTablePropertiesExist()
        {
            Assert.False(EntityRequestValidationCache.Validate(typeof(ForeignOnlyEntity)));
        }

        /// <summary>
        /// Verifies that validation returns true when at least one scalar property exists alongside foreign table
        /// navigation properties.
        /// </summary>
        [Fact]
        public void Validate_ReturnsTrue_WhenScalarPropertyExistsAlongsideForeignTableProperty()
        {
            Assert.True(EntityRequestValidationCache.Validate(typeof(MixedForeignEntity)));
        }

        /// <summary>
        /// Verifies that validation returns false when only static properties exist.
        /// </summary>
        [Fact]
        public void Validate_ReturnsFalse_WhenOnlyStaticPropertiesExist()
        {
            Assert.False(EntityRequestValidationCache.Validate(typeof(StaticOnlyEntity)));
        }

        /// <summary>
        /// Verifies that validation returns false when only indexer properties exist.
        /// </summary>
        [Fact]
        public void Validate_ReturnsFalse_WhenOnlyIndexerPropertiesExist()
        {
            Assert.False(EntityRequestValidationCache.Validate(typeof(IndexerOnlyEntity)));
        }

        /// <summary>
        /// Validates that an exception is thrown when the specified entity type lacks a TableAttribute.
        /// </summary>
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
        [Fact]
        public void Validate_UsesCache_ForSameType()
        {
            Assert.True(EntityRequestValidationCache.Validate(typeof(ValidEntity)));
            Assert.True(EntityRequestValidationCache.Validate(typeof(ValidEntity)));
        }
    }
}
