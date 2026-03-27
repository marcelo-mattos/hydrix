using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Caching;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Caching
{
    /// <summary>
    /// Contains unit tests for the EntityBuilderMetadataCache class, validating the behavior of metadata building for
    /// entities with foreign table relationships.
    /// </summary>
    /// <remarks>These tests ensure that the metadata builder correctly handles scenarios such as foreign
    /// tables without primary keys and verifies the inclusion of select columns in joins.</remarks>
    public class EntityBuilderMetadataCacheTests
    {
        /// <summary>
        /// Represents an entity that is related to another entity but does not have a primary key defined.
        /// </summary>
        /// <remarks>This class is typically used in scenarios where entities are associated without a
        /// unique identifier. It may be useful in data modeling or object-relational mapping (ORM) contexts where
        /// relationships are established without primary keys.</remarks>
        private class ForeignEntityWithNoPrimaryKey
        { }

        /// <summary>
        /// Represents an entity that contains a reference to a foreign entity which does not define a primary key.
        /// </summary>
        /// <remarks>This class demonstrates a scenario where a foreign entity is referenced without a
        /// primary key. When using such entities, ensure that the foreign table is properly configured to maintain data
        /// integrity and avoid issues with entity relationships.</remarks>
        private class MainEntityWithInvalidForeignTable
        {
            /// <summary>
            /// Gets or sets the foreign entity associated with this entity, representing a relationship to another data
            /// model.
            /// </summary>
            /// <remarks>This property establishes a link to a related entity, enabling navigation
            /// between associated data models. Ensure that the foreign entity is properly configured in the database
            /// context to maintain referential integrity.</remarks>
            [ForeignTable("foreign", PrimaryKeys = new string[0], ForeignKeys = new[] { "ForeignId" })]
            public ForeignEntityWithNoPrimaryKey Foreign { get; set; }

            /// <summary>
            /// Gets or sets the foreign key value associated with the relationship.
            /// </summary>
            public int ForeignId { get; set; }
        }

        /// <summary>
        /// Represents a main entity with an explicit foreign-key name that does not match any property.
        /// </summary>
        private class MainEntityWithNonExistentConfiguredForeignKey
        {
            /// <summary>
            /// Gets or sets the related foreign entity with explicit join key configuration.
            /// </summary>
            [ForeignTable("foreign", PrimaryKeys = new[] { "Id" }, ForeignKeys = new[] { "UnknownForeignKey" })]
            public ForeignEntityWithColumn Foreign { get; set; }

            /// <summary>
            /// Gets or sets a sample property unrelated to the configured foreign key name.
            /// </summary>
            public int AnyValue { get; set; }
        }

        /// <summary>
        /// Represents a main entity with a required foreign-key property.
        /// </summary>
        private class MainEntityWithRequiredForeignKey
        {
            /// <summary>
            /// Gets or sets the related foreign entity with explicit join key configuration.
            /// </summary>
            [ForeignTable("foreign", PrimaryKeys = new[] { "Id" }, ForeignKeys = new[] { "ForeignId" })]
            public ForeignEntityWithColumn Foreign { get; set; }

            /// <summary>
            /// Gets or sets the required foreign-key value used by the join.
            /// </summary>
            [Required]
            public int ForeignId { get; set; }
        }

        /// <summary>
        /// Represents a main entity whose foreign mapping explicitly defines a schema.
        /// </summary>
        private class MainEntityWithExplicitForeignSchema
        {
            /// <summary>
            /// Gets or sets the related foreign entity with an explicit schema in the foreign-table attribute.
            /// </summary>
            [ForeignTable("foreign", Schema = "custom", PrimaryKeys = new[] { "Id" }, ForeignKeys = new[] { "ForeignId" })]
            public ForeignEntityWithColumn Foreign { get; set; }

            /// <summary>
            /// Gets or sets the foreign key value used in the relationship.
            /// </summary>
            public int ForeignId { get; set; }
        }

        /// <summary>
        /// Represents an entity that includes a mapped identifier, a name, and a nested foreign entity, with support
        /// for custom column mapping and ignored properties.
        /// </summary>
        /// <remarks>The 'Id' property is mapped to a database column named 'custom_name'. The 'Ignored'
        /// property is not mapped to any database column. The 'Nested' property represents a related entity from
        /// another table, specified by the 'ForeignTable' attribute.</remarks>
        private class ForeignEntityWithColumn
        {
            /// <summary>
            /// Gets or sets the identifier for this entity, which is mapped to a database column named 'custom_name'.
            /// </summary>
            [Column("custom_name")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with this entity, which is mapped to a database column named 'Name'.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets a property that is ignored in the database mapping, meaning it will not be included in any
            /// database operations.
            /// </summary>
            [NotMapped]
            public string Ignored { get; set; }

            /// <summary>
            /// Gets or sets the nested object associated with this entity, which is linked to the 'other' foreign
            /// table.
            /// </summary>
            [ForeignTable("other")]
            public object Nested { get; set; }
        }

        /// <summary>
        /// Represents an entity that includes a reference to a foreign table without defined primary keys.
        /// </summary>
        /// <remarks>This class is used to establish a relationship with a foreign entity, which is
        /// expected to be defined in a separate table. The foreign entity is referenced through the Foreign property,
        /// which allows for navigation to related data.</remarks>
        private class MainEntityWithForeignTableWithoutPrimaryKeys
        {
            /// <summary>
            /// Gets or sets the foreign entity associated with this entity, representing a relationship to another
            /// table.
            /// </summary>
            /// <remarks>This property is decorated with the ForeignTable attribute, indicating that
            /// it maps to a foreign key relationship in the database. Ensure that the related entity is properly
            /// configured to maintain referential integrity.</remarks>
            [ForeignTable("foreign", PrimaryKeys = null, ForeignKeys = new[] { "ForeignId" })]
            public ForeignEntityWithColumn Foreign { get; set; }

            /// <summary>
            /// Gets or sets the foreign key value associated with the relationship.
            /// </summary>
            public int ForeignId { get; set; }
        }

        /// <summary>
        /// Represents an entity that establishes a relationship with a foreign table through a foreign key property.
        /// </summary>
        /// <remarks>This class is used to associate the main entity with a related foreign entity,
        /// enabling navigation and data access between the two. The Foreign property references the related
        /// ForeignEntityWithColumn instance, while ForeignId serves as the foreign key linking the entities.</remarks>
        private class MainEntityWithForeignTable
        {
            /// <summary>
            /// Gets or sets the foreign entity associated with this entity, representing a relationship to another
            /// table.
            /// </summary>
            /// <remarks>This property is decorated with the ForeignTable attribute, indicating that
            /// it is part of a foreign key relationship. Ensure that the ForeignId is set correctly to maintain
            /// referential integrity.</remarks>
            [ForeignTable("foreign", PrimaryKeys = new[] { "Id" }, ForeignKeys = new[] { "ForeignId" })]
            public ForeignEntityWithColumn Foreign { get; set; }

            /// <summary>
            /// Gets or sets the foreign identifier associated with the entity.
            /// </summary>
            /// <remarks>This property is typically used to establish a relationship with another
            /// entity in a database or data model. Ensure that the value assigned is valid and corresponds to an
            /// existing entity.</remarks>
            public int ForeignId { get; set; }
        }

        /// <summary>
        /// Represents a foreign entity that declares a primary key using data annotations.
        /// </summary>
        private class ForeignEntityWithPrimaryKey
        {
            /// <summary>
            /// Gets or sets the primary key identifier of the foreign entity.
            /// </summary>
            [Key]
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents a main entity with a foreign table relationship but without resolvable foreign keys.
        /// </summary>
        private class MainEntityWithForeignTableWithoutForeignKeys
        {
            /// <summary>
            /// Gets or sets the related foreign entity.
            /// </summary>
            [ForeignTable("foreign")]
            public ForeignEntityWithPrimaryKey Foreign { get; set; }
        }

        /// <summary>
        /// Represents a foreign entity used for composite key mapping scenarios.
        /// </summary>
        private class ForeignEntityWithCompositeKey
        {
            /// <summary>
            /// Gets or sets the first key-like value.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the second key-like value.
            /// </summary>
            public int Code { get; set; }
        }

        /// <summary>
        /// Represents a main entity with mismatched counts between configured primary and foreign keys.
        /// </summary>
        private class MainEntityWithMismatchedKeys
        {
            /// <summary>
            /// Gets or sets the related foreign entity with explicit join key configuration.
            /// </summary>
            [ForeignTable("foreign", PrimaryKeys = new[] { "Id", "Code" }, ForeignKeys = new[] { "ForeignId" })]
            public ForeignEntityWithCompositeKey Foreign { get; set; }

            /// <summary>
            /// Gets or sets the foreign key value used in the relationship.
            /// </summary>
            public int ForeignId { get; set; }
        }

        /// <summary>
        /// Verifies that an InvalidOperationException is thrown when attempting to build metadata for an entity whose
        /// foreign table does not define any primary keys.
        /// </summary>
        /// <remarks>This test ensures that the EntityBuilderMetadataCache enforces the requirement for at
        /// least one primary key in foreign table metadata. The exception message is validated to confirm that the
        /// error is related to missing primary keys.</remarks>
        [Fact]
        public void BuildMetadata_Throws_WhenForeignTablePrimaryKeysIsNull()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                EntityBuilderMetadataCache.GetMetadata(typeof(MainEntityWithForeignTableWithoutPrimaryKeys)));
            Assert.Contains("must define at least one PrimaryKey", ex.Message);
        }

        /// <summary>
        /// Verifies that building metadata for an entity referencing a foreign table without primary keys throws an
        /// InvalidOperationException.
        /// </summary>
        /// <remarks>This test ensures that the metadata builder enforces the requirement for foreign
        /// tables to have defined primary keys. Attempting to build metadata for an entity with an invalid foreign
        /// table configuration results in an exception, indicating the invalid state.</remarks>
        [Fact]
        public void BuildMetadata_Throws_WhenForeignTableHasNoPrimaryKeys()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                EntityBuilderMetadataCache.GetMetadata(typeof(MainEntityWithInvalidForeignTable)));
            Assert.Contains("must define at least one PrimaryKey", ex.Message);
        }

        /// <summary>
        /// Verifies that the metadata for the MainEntityWithForeignTable correctly creates a join with the specified
        /// foreign select columns.
        /// </summary>
        /// <remarks>This test checks that the join includes the expected foreign select columns
        /// 'custom_name' and 'Name', while ensuring that 'Ignored' and 'Nested' columns are not present. It is
        /// essential for validating the integrity of the join configuration in the entity metadata.</remarks>
        [Fact]
        public void BuildMetadata_CreatesJoin_WithForeignSelectColumns()
        {
            var metadata = EntityBuilderMetadataCache.GetMetadata(typeof(MainEntityWithForeignTable));
            Assert.NotEmpty(metadata.Joins);

            var join = metadata.Joins[0];
            Assert.Contains(join.Columns, c => c.ColumnName == "custom_name");
            Assert.Contains(join.Columns, c => c.ColumnName == "Name");

            Assert.DoesNotContain(join.Columns, c => c.ColumnName == "Ignored");
            Assert.DoesNotContain(join.Columns, c => c.ColumnName == "Nested");
        }

        /// <summary>
        /// Verifies that building metadata throws an InvalidOperationException when foreign keys cannot be resolved
        /// from attributes or inferred relationship metadata.
        /// </summary>
        [Fact]
        public void BuildMetadata_Throws_WhenForeignKeysCannotBeResolved()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                EntityBuilderMetadataCache.GetMetadata(typeof(MainEntityWithForeignTableWithoutForeignKeys)));

            Assert.Contains("Foreign key not resolved", ex.Message);
        }

        /// <summary>
        /// Verifies that building metadata throws an InvalidOperationException when configured primary and foreign key
        /// counts do not match.
        /// </summary>
        [Fact]
        public void BuildMetadata_Throws_WhenPrimaryAndForeignKeyCountsMismatch()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                EntityBuilderMetadataCache.GetMetadata(typeof(MainEntityWithMismatchedKeys)));

            Assert.Contains("PrimaryKeys and ForeignKeys count mismatch", ex.Message);
        }

        /// <summary>
        /// Verifies that explicit schema configured in <see cref="ForeignTableAttribute"/> is preserved in join metadata.
        /// </summary>
        [Fact]
        public void BuildMetadata_UsesExplicitSchema_WhenForeignTableAttributeDefinesSchema()
        {
            var metadata = EntityBuilderMetadataCache.GetMetadata(typeof(MainEntityWithExplicitForeignSchema));

            Assert.NotEmpty(metadata.Joins);
            Assert.Equal("custom", metadata.Joins[0].Schema);
        }

        /// <summary>
        /// Verifies that join metadata is marked as required when all configured foreign-key properties are required.
        /// </summary>
        [Fact]
        public void BuildMetadata_JoinIsRequired_WhenForeignKeyPropertyHasRequiredAttribute()
        {
            var metadata = EntityBuilderMetadataCache.GetMetadata(typeof(MainEntityWithRequiredForeignKey));

            Assert.NotEmpty(metadata.Joins);
            Assert.True(metadata.Joins[0].IsRequiredJoin);
        }

        /// <summary>
        /// Verifies that join metadata is optional when the configured foreign-key property cannot be found.
        /// </summary>
        [Fact]
        public void BuildMetadata_JoinIsNotRequired_WhenConfiguredForeignKeyPropertyDoesNotExist()
        {
            var metadata = EntityBuilderMetadataCache.GetMetadata(typeof(MainEntityWithNonExistentConfiguredForeignKey));

            Assert.NotEmpty(metadata.Joins);
            Assert.False(metadata.Joins[0].IsRequiredJoin);
        }
    }
}