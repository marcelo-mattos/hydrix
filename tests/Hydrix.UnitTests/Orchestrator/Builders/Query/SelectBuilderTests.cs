using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Builders.Query;
using Hydrix.Orchestrator.Metadata.Builders;
using Hydrix.Orchestrator.Metadata.Internals;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Builders.Query
{
    /// <summary>
    /// Provides unit tests for the SelectBuilder class, validating the generation of SQL SELECT statements based on
    /// entity metadata.
    /// </summary>
    /// <remarks>These tests cover various scenarios, including generating selects without joins, with join
    /// columns, and handling null or empty metadata and aliases. They ensure that the SelectBuilder behaves as expected
    /// under different conditions.</remarks>
    public class SelectBuilderTests
    {
        /// <summary>
        /// Represents a sample entity with an identifier, a name, and a reference to a child entity. Intended for
        /// demonstration or testing purposes.
        /// </summary>
        /// <remarks>Properties of this class are mapped to database columns, except for the property
        /// marked with the NotMapped attribute, which is excluded from persistence. The Child property references a
        /// related entity and may be used to represent relationships in data models.</remarks>
        private class DummyEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [Column("id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name of the entity.
            /// </summary>
            [Column("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the value that is not mapped to the database.
            /// </summary>
            /// <remarks>This property is used to indicate that the value should not be persisted in
            /// the database. It is typically used for properties that are calculated or derived from other
            /// data.</remarks>
            [NotMapped]
            public string NotMapped { get; set; }

            /// <summary>
            /// Gets or sets the child entity associated with this object.
            /// </summary>
            /// <remarks>This property represents a foreign key relationship to the 'child' table.
            /// Assigning a value to this property should correspond to a valid entry in the related table.</remarks>
            [ForeignTable("child")]
            public ChildEntity Child { get; set; }
        }

        /// <summary>
        /// Represents a child entity with properties for identification and naming.
        /// </summary>
        /// <remarks>This class is typically used to map child entities in a database context. The ChildId
        /// property serves as a unique identifier, while the ChildName property holds the name of the child. The
        /// NotMapped property is not persisted in the database.</remarks>
        private class ChildEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the child entity.
            /// </summary>
            [Column("child_id")]
            public int ChildId { get; set; }

            /// <summary>
            /// Gets or sets the name of the child associated with the record.
            /// </summary>
            [Column("child_name")]
            public string ChildName { get; set; }

            /// <summary>
            /// Gets or sets the value that is not mapped to the database.
            /// </summary>
            /// <remarks>This property is intended for use within application logic and is not
            /// persisted to the underlying data store. Use this property to store values that are required at runtime
            /// but should not be saved in the database.</remarks>
            [NotMapped]
            public string NotMapped { get; set; }
        }

        /// <summary>
        /// Represents the main entity that contains a reference to the associated join entity.
        /// </summary>
        /// <remarks>Use this class to access or manipulate data related to the main entity and its
        /// relationship with the join entity. The association is established through the Join property, which should be
        /// properly initialized before use.</remarks>
        private class MainEntity
        {
            /// <summary>
            /// Gets or sets the join entity associated with the current context.
            /// </summary>
            /// <remarks>This property represents a foreign key relationship to the 'join' table,
            /// allowing for the retrieval and manipulation of related data.</remarks>
            [ForeignTable("join")]
            public JoinEntity Join { get; set; }
        }

        /// <summary>
        /// Represents the entity used for join operations, containing a property that is not mapped to the database.
        /// </summary>
        private class JoinEntity
        {
            /// <summary>
            /// Gets or sets the number of column attributes associated with the entity.
            /// </summary>
            public int NoColumnAttr { get; set; }
        }

        /// <summary>
        /// Verifies that the SelectBuilder.Build method generates a SQL SELECT statement for the main entity only when
        /// no joins are specified.
        /// </summary>
        /// <remarks>This test ensures that the generated SQL includes only the columns defined in the
        /// entity metadata and does not contain any references to properties marked as 'NotMapped'. It validates that
        /// no join-related SQL is produced when the joins collection is empty.</remarks>
        [Fact]
        public void Build_GeneratesSelectForMainEntityOnly_WhenNoJoins()
        {
            var columns = new List<ColumnBuilderMetadata>
            {
                new ColumnBuilderMetadata(
                    "Id",
                    "id",
                    true,
                    true,
                    MetadataFactory.CreateGetter(typeof(DummyEntity).GetProperty(nameof(DummyEntity.Id)))),
                new ColumnBuilderMetadata(
                    "Name",
                    "name",
                    false,
                    false,
                    MetadataFactory.CreateGetter(typeof(DummyEntity).GetProperty(nameof(DummyEntity.Name))))
            };

            var metadata = new EntityBuilderMetadata(
                entityType: typeof(DummyEntity),
                table: "dummy",
                schema: "dbo",
                alias: "d",
                columns: columns,
                joins: new List<JoinBuilderMetadata>()
            );

            var sql = SelectBuilder.Build(metadata, "d");

            Assert.Contains("SELECT", sql);
            Assert.Contains("d.id", sql);
            Assert.Contains("d.name", sql);
            Assert.DoesNotContain("NotMapped", sql);
        }

        /// <summary>
        /// Verifies that the SelectBuilder.Build method generates a SQL SELECT statement including both entity columns
        /// and join columns as specified by the provided entity metadata.
        /// </summary>
        /// <remarks>This test ensures that the generated SQL query contains the expected column names and
        /// aliases for both the main entity and its joined entities. It also checks that properties marked as
        /// 'NotMapped' are excluded from the result.</remarks>
        [Fact]
        public void Build_GeneratesSelectWithJoinColumns()
        {
            var columns = new List<ColumnBuilderMetadata>
            {
                new ColumnBuilderMetadata(
                    "Id",
                    "id",
                    true,
                    true,
                    MetadataFactory.CreateGetter(typeof(DummyEntity).GetProperty(nameof(DummyEntity.Id))))
            };

            var joinProperty = typeof(DummyEntity)
                .GetProperty(nameof(DummyEntity.Child));

            var joinWithChildEntity = new JoinBuilderMetadata(
                table: "child",
                schema: "dbo",
                alias: "c",
                primaryKeys: new[] { "ChildId" },
                foreignKeys: new[] { "ChildId" },
                isRequiredJoin: false,
                navigationProperty: joinProperty
            );

            var metadataWithJoin = new EntityBuilderMetadata(
                entityType: typeof(DummyEntity),
                table: "dummy",
                schema: "dbo",
                alias: "d",
                columns: columns,
                joins: new List<JoinBuilderMetadata> { joinWithChildEntity }
            );

            var sql = SelectBuilder.Build(metadataWithJoin, "d");

            Assert.Contains("d.id", sql);
            Assert.Contains("c.child_id AS \"child.child_id\"", sql);
            Assert.Contains("c.child_name AS \"child.child_name\"", sql);
            Assert.DoesNotContain("NotMapped", sql);
        }

        /// <summary>
        /// Verifies that the SQL generated for a join column without a [Column] attribute uses the property name as the
        /// column name.
        /// </summary>
        /// <remarks>This test ensures that when a join property does not have a [Column] attribute
        /// specified, the generated SQL correctly reflects the property name in the output. It is important for
        /// maintaining consistency in SQL generation when mapping entity properties to database columns.</remarks>
        [Fact]
        public void Build_JoinColumn_WithoutColumnAttribute_UsesPropertyName()
        {
            // Entidade de join sem atributo [Column] em uma das propriedades
            var columns = new List<ColumnBuilderMetadata>();
            var joinProperty = typeof(MainEntity).GetProperty(nameof(MainEntity.Join));
            var join = new JoinBuilderMetadata(
                table: "join",
                schema: "dbo",
                alias: "j",
                primaryKeys: new[] { "NoColumnAttr" },
                foreignKeys: new[] { "NoColumnAttr" },
                isRequiredJoin: false,
                navigationProperty: joinProperty
            );

            var metadata = new EntityBuilderMetadata(
                entityType: typeof(MainEntity),
                table: "main",
                schema: "dbo",
                alias: "m",
                columns: columns,
                joins: new List<JoinBuilderMetadata> { join }
            );

            var sql = SelectBuilder.Build(metadata, "m");

            // O nome da coluna deve ser o nome da propriedade, pois não há [Column]
            Assert.Contains("j.NoColumnAttr AS \"join.NoColumnAttr\"", sql);
        }

        /// <summary>
        /// Verifies that the Build method throws an ArgumentNullException when the metadata parameter is null.
        /// </summary>
        /// <remarks>This test ensures that the Build method enforces its contract by validating input
        /// parameters and preventing null metadata from being processed. Proper exception handling for null arguments
        /// helps maintain robustness and prevents unexpected runtime errors.</remarks>
        [Fact]
        public void Build_ThrowsIfMetadataIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => SelectBuilder.Build(null, "a"));
        }

        /// <summary>
        /// Verifies that the SelectBuilder.Build method throws an ArgumentNullException when the main alias parameter
        /// is null, empty, or consists only of whitespace characters.
        /// </summary>
        /// <remarks>This test ensures that the SelectBuilder enforces the requirement for a non-null,
        /// non-empty main alias, helping to prevent invalid query construction.</remarks>
        [Fact]
        public void Build_ThrowsIfMainAliasIsNullOrWhitespace()
        {
            var metadata = new EntityBuilderMetadata(
                entityType: typeof(DummyEntity),
                table: "dummy",
                schema: "dbo",
                alias: "d",
                columns: new List<ColumnBuilderMetadata>(),
                joins: new List<JoinBuilderMetadata>()
            );

            Assert.Throws<ArgumentNullException>(() => SelectBuilder.Build(metadata, null));
            Assert.Throws<ArgumentNullException>(() => SelectBuilder.Build(metadata, ""));
            Assert.Throws<ArgumentNullException>(() => SelectBuilder.Build(metadata, " "));
        }
    }
}