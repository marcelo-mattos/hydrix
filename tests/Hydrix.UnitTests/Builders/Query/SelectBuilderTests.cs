using Hydrix.Builders.Query;
using Hydrix.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace Hydrix.UnitTests.Builders.Query
{
    /// <summary>
    /// Contains unit tests for the SelectBuilder class, validating SQL SELECT statement generation based on entity
    /// metadata and join configurations.
    /// </summary>
    /// <remarks>These tests ensure that the SelectBuilder correctly generates SQL queries for various
    /// scenarios, including cases with and without joins, and handles invalid input appropriately. Each test case
    /// verifies specific aspects of the SQL generation process, such as the inclusion of mapped columns and the
    /// handling of null or whitespace aliases.</remarks>
    public class SelectBuilderTests
    {
        /// <summary>
        /// Represents a simple entity with an identifier and a name for demonstration purposes.
        /// </summary>
        /// <remarks>This class includes properties that map to database columns, as well as a property
        /// marked with <see cref="NotMappedAttribute"/> that is not persisted in the database. Intended for use in test
        /// or example scenarios.</remarks>
        private class DummyEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [Column("id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets name for the entity.
            /// </summary>
            [Column("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the value that is not mapped to the database.
            /// </summary>
            /// <remarks>This property is not persisted in the database. It is typically used for
            /// values that are calculated, derived, or used only within the application logic.</remarks>
            [NotMapped]
            public string NotMapped { get; set; }
        }

        /// <summary>
        /// Creates a metadata object that describes a database column for a specified property of the entity.
        /// </summary>
        /// <remarks>This method is typically used to assist in building test metadata for
        /// entity-to-database column mappings. The property must exist on the DummyEntity type.</remarks>
        /// <param name="property">The name of the property in the entity for which to create column metadata. Cannot be null.</param>
        /// <param name="column">The name of the corresponding column in the database. Cannot be null.</param>
        /// <param name="isKey">true if the column represents a key in the database; otherwise, false.</param>
        /// <param name="isRequired">true if the column is required (non-nullable) in the database; otherwise, false.</param>
        /// <returns>A ColumnBuilderMetadata instance containing metadata for the specified property and column.</returns>
        private static ColumnBuilderMetadata CreateColumn(string property, string column, bool isKey = false, bool isRequired = false)
        {
            var propInfo = typeof(DummyEntity).GetProperty(property);
            return new ColumnBuilderMetadata(
                property,
                column,
                isKey,
                isRequired,
                _ => null // getter não é usado nos testes de SQL
            );
        }

        /// <summary>
        /// Verifies that the SQL SELECT statement generated for the main entity includes only the specified columns and
        /// excludes properties marked as not mapped when no joins are present.
        /// </summary>
        /// <remarks>This test ensures that the generated SQL contains the expected column names for the
        /// main entity and does not include any columns corresponding to properties that are not mapped. It is intended
        /// for scenarios where the entity is queried directly without related entities.</remarks>
        [Fact]
        public void Build_GeneratesSelectForMainEntityOnly_WhenNoJoins()
        {
            var columns = new List<ColumnBuilderMetadata>
            {
                CreateColumn("Id", "id", true, true),
                CreateColumn("Name", "name")
            };

            var metadata = new EntityBuilderMetadata(
                "DummyEntity",
                typeof(DummyEntity),
                "dummy",
                "dbo",
                columns,
                new List<JoinBuilderMetadata>()
            );

            var aliasContext = new AliasContext();
            var sql = SelectBuilder.Build(metadata, "d", aliasContext);

            Assert.Contains("SELECT", sql);
            Assert.Contains("d.id", sql);
            Assert.Contains("d.name", sql);
            Assert.DoesNotContain("NotMapped", sql);
        }

        /// <summary>
        /// Verifies that the SQL SELECT statement generated by the SelectBuilder includes the specified columns and
        /// join columns as defined in the entity metadata.
        /// </summary>
        /// <remarks>This test ensures that the generated SQL output correctly reflects the structure and
        /// aliases specified in the entity and join metadata, including both direct and joined columns. It checks that
        /// the SELECT statement contains the expected column references and aliases for joined tables.</remarks>
        [Fact]
        public void Build_GeneratesSelectWithJoinColumns()
        {
            var columns = new List<ColumnBuilderMetadata>
            {
                CreateColumn("Id", "id", true, true)
            };

            var joinColumns = new List<ForeignColumnMetadata>
            {
                new ForeignColumnMetadata("child_id", "child.child_id"),
                new ForeignColumnMetadata("child_name", "child.child_name")
            };

            var join = new JoinBuilderMetadata(
                entity: "Child",
                table: "child",
                schema: "dbo",
                primaryKeys: new[] { "ChildId" },
                foreignKeys: new[] { "ChildId" },
                isRequiredJoin: false,
                columns: joinColumns
            );

            var metadataWithJoin = new EntityBuilderMetadata(
                "DummyEntity",
                typeof(DummyEntity),
                "dummy",
                "dbo",
                columns,
                new List<JoinBuilderMetadata> { join }
            );

            var aliasContext = new AliasContext();
            var sql = SelectBuilder.Build(metadataWithJoin, "d", aliasContext);

            Assert.Contains("d.id", sql);
            Assert.Contains("c.child_id AS \"child.child_id\"", sql);
            Assert.Contains("c.child_name AS \"child.child_name\"", sql);
        }

        /// <summary>
        /// Verifies that building a join column without a Column attribute uses the property name as the column alias
        /// in the generated SQL statement.
        /// </summary>
        /// <remarks>This test ensures that when an entity property does not have a Column attribute, the
        /// join operation correctly references the property name in the SQL output. It is important for maintaining
        /// consistency between entity definitions and the resulting SQL queries.</remarks>
        [Fact]
        public void Build_JoinColumn_WithoutColumnAttribute_UsesPropertyName()
        {
            var columns = new List<ColumnBuilderMetadata>();
            var joinColumns = new List<ForeignColumnMetadata>
            {
                new ForeignColumnMetadata("NoColumnAttr", "join.NoColumnAttr")
            };

            var join = new JoinBuilderMetadata(
                entity: "Join",
                table: "join",
                schema: "dbo",
                primaryKeys: new[] { "NoColumnAttr" },
                foreignKeys: new[] { "NoColumnAttr" },
                isRequiredJoin: false,
                columns: joinColumns
            );

            var metadata = new EntityBuilderMetadata(
                "DummyEntity",
                typeof(DummyEntity),
                "main",
                "dbo",
                columns,
                new List<JoinBuilderMetadata> { join }
            );

            var aliasContext = new AliasContext();
            var sql = SelectBuilder.Build(metadata, "m", aliasContext);

            Assert.Contains("j.NoColumnAttr AS \"join.NoColumnAttr\"", sql);
        }

        /// <summary>
        /// Verifies that the Build method throws an ArgumentNullException when the metadata parameter is null.
        /// </summary>
        /// <remarks>This test ensures that the Build method enforces its contract by validating that the
        /// metadata argument is not null, helping to prevent runtime errors caused by invalid input.</remarks>
        [Fact]
        public void Build_ThrowsIfMetadataIsNull()
        {
            var aliasContext = new AliasContext();
            Assert.Throws<ArgumentNullException>(() => SelectBuilder.Build(null, "a", aliasContext));
        }

        /// <summary>
        /// Verifies that the SelectBuilder.Build method throws an ArgumentNullException when the main alias parameter
        /// is null, empty, or consists only of whitespace.
        /// </summary>
        /// <remarks>This test ensures that the SelectBuilder enforces input validation for the main alias
        /// parameter, which is required for building a valid query. Supplying a null or whitespace value for the main
        /// alias is not supported and should result in an exception to prevent invalid query construction.</remarks>
        [Fact]
        public void Build_ThrowsIfMainAliasIsNullOrWhitespace()
        {
            var metadata = new EntityBuilderMetadata(
                "DummyEntity",
                typeof(DummyEntity),
                "dummy",
                "dbo",
                new List<ColumnBuilderMetadata>(),
                new List<JoinBuilderMetadata>()
            );
            var aliasContext = new AliasContext();

            Assert.Throws<ArgumentNullException>(() => SelectBuilder.Build(metadata, null, aliasContext));
            Assert.Throws<ArgumentNullException>(() => SelectBuilder.Build(metadata, "", aliasContext));
            Assert.Throws<ArgumentNullException>(() => SelectBuilder.Build(metadata, " ", aliasContext));
        }
    }
}
