using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Caching;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Caching
{
    /// <summary>
    /// Contains unit tests for the EntityMetadataCache class, verifying the correct mapping of entity properties and
    /// relationships.
    /// </summary>
    /// <remarks>These tests ensure that the EntityMetadataCache correctly handles attributes such as Column
    /// and ForeignTable, and that it maintains singleton instances for the same entity type.</remarks>
    public class EntityMetadataCacheTests
    {
        /// <summary>
        /// Represents an entity with columns that map to a database table, including properties for identification and
        /// relationships.
        /// </summary>
        /// <remarks>The 'Id' and 'Name' properties are mapped to corresponding columns in the database.
        /// The 'Related' property establishes a foreign key relationship with another entity. Properties marked with
        /// 'Ignored' or without mapping attributes are not persisted in the database.</remarks>
        private class EntityWithColumns
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [Column("Id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name of the entity.
            /// </summary>
            [Column("Name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the related entity associated with this instance.
            /// </summary>
            /// <remarks>This property represents a foreign key relationship to the RelatedEntity
            /// table. Ensure that the related entity is properly loaded before accessing its properties.</remarks>
            [ForeignTable("Related")]
            public RelatedEntity Related { get; set; }

            /// <summary>
            /// Gets the value of the Ignored column for this entity.
            /// </summary>
            [Column("Ignored")]
            public int Ignored { get; }

            /// <summary>
            /// Gets or sets the value of a property that is not decorated with any mapping attributes.
            /// </summary>
            public int WithoutColumnAttribute { get; set; }

            /// <summary>
            /// Gets or sets the value of the property that is not mapped to a database column.
            /// </summary>
            /// <remarks>This property is intended for use with data that should not be persisted to
            /// the database. It is typically used to hold values that are calculated at runtime or are relevant only
            /// within the application's context.</remarks>
            [NotMapped]
            public string NotMapped { get; set; }
        }

        /// <summary>
        /// Represents an entity that is related to another entity and identified by a unique identifier.
        /// </summary>
        private class RelatedEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the relationship.
            /// </summary>
            [Column("RelId")]
            public int RelId { get; set; }
        }

        /// <summary>
        /// Represents an entity with an identifier and a name.
        /// </summary>
        /// <remarks>This class is used to encapsulate basic information about an entity, including its
        /// unique identifier and descriptive name.</remarks>
        private class NoAttributes
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [NotMapped]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name of the entity.
            /// </summary>
            [NotMapped]
            public string Name { get; set; }
        }

        /// <summary>
        /// Verifies that the EntityMetadataCache.GetOrAdd method returns the same metadata instance for repeated
        /// requests with the same entity type.
        /// </summary>
        /// <remarks>This test ensures that the caching mechanism in EntityMetadataCache is functioning
        /// correctly by confirming that identical type requests yield the same object reference. This behavior is
        /// important for consistency and performance in applications relying on cached metadata.</remarks>
        [Fact]
        public void GetOrAdd_ReturnsSameInstance_ForSameType()
        {
            var meta1 = EntityMetadataCache.GetOrAdd(typeof(EntityWithColumns));
            var meta2 = EntityMetadataCache.GetOrAdd(typeof(EntityWithColumns));
            Assert.Same(meta1, meta2);
        }

        /// <summary>
        /// Verifies that the metadata for an entity correctly maps its columns and foreign table relationships.
        /// </summary>
        /// <remarks>This test checks that the expected fields are present in the metadata and that
        /// ignored or unmapped properties are not included. It also ensures that related entities are correctly
        /// mapped.</remarks>
        [Fact]
        public void BuildMetadata_MapsColumnsAndForeignTablesCorrectly()
        {
            var meta = EntityMetadataCache.BuildMetadata(typeof(EntityWithColumns));
            Assert.NotNull(meta);

            // Verifica se os campos foram mapeados corretamente
            Assert.Contains(meta.Fields, f => f.Name == "Id");
            Assert.Contains(meta.Fields, f => f.Name == "Name");
            Assert.Contains(meta.Fields, f => f.Name == "WithoutColumnAttribute");
            Assert.DoesNotContain(meta.Fields, f => f.Name == "Ignored");
            Assert.DoesNotContain(meta.Fields, f => f.Name == "NotMapped");

            // Verifica se o relacionamento foi mapeado corretamente
            Assert.Contains(meta.Entities, e => e.Property.Name == "Related");
        }

        /// <summary>
        /// Verifies that the BuildMetadata method returns an empty metadata object when the specified type does not
        /// have any attributes.
        /// </summary>
        /// <remarks>This test ensures that types without metadata attributes are handled gracefully,
        /// resulting in empty collections for fields and entities.</remarks>
        [Fact]
        public void BuildMetadata_ReturnsEmpty_WhenNoAttributes()
        {
            var meta = EntityMetadataCache.BuildMetadata(typeof(NoAttributes));
            Assert.Empty(meta.Fields);
            Assert.Empty(meta.Entities);
        }
    }
}