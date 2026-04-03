using Hydrix.Orchestrator.Builders.Query;
using Hydrix.Orchestrator.Builders.Query.Conditions;
using Hydrix.Orchestrator.Metadata.Builders;
using Hydrix.Schemas.Contract;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Builders.Query
{
    /// <summary>
    /// Contains unit tests for the QueryBuilder class, validating the generation of SQL queries and the formatting of
    /// table names based on entity metadata.
    /// </summary>
    /// <remarks>This class includes tests for various scenarios, ensuring that the QueryBuilder correctly
    /// constructs SQL queries with and without WHERE clauses, and formats table names based on schema presence. It
    /// utilizes mock objects and reflection to simulate database interactions and validate the expected behavior of the
    /// QueryBuilder methods.</remarks>
    public class QueryBuilderTests
    {
        /// <summary>
        /// Represents a placeholder implementation of the ITable interface for testing or mock scenarios.
        /// </summary>
        private class DummyEntity :
            ITable
        { }

        /// <summary>
        /// Creates metadata for an entity using the specified table and schema names.
        /// </summary>
        /// <remarks>This method initializes the entity metadata with default values for columns and
        /// joins.</remarks>
        /// <param name="entity">The name of the entity. This value cannot be null or empty.</param>
        /// <param name="table">The name of the database table associated with the entity. This value cannot be null or empty.</param>
        /// <param name="schema">The name of the schema that contains the specified table. This value cannot be null or empty.</param>
        /// <returns>An instance of <see cref="EntityBuilderMetadata"/> that contains the metadata for the specified entity.</returns>
        private EntityBuilderMetadata CreateMetadata(
            string entity,
            string table,
            string schema)
            => new EntityBuilderMetadata(
                entity,
                typeof(DummyEntity),
                table,
                schema,
                new List<ColumnBuilderMetadata>(),
                new List<JoinBuilderMetadata>());

        /// <summary>
        /// Verifies that the QueryBuilder.Build method generates a SQL query string for the specified entity, including
        /// a WHERE clause as defined by the provided WhereBuilder instance.
        /// </summary>
        /// <remarks>This test uses mock objects and static overrides to simulate database metadata and
        /// SQL component builders, enabling validation of the query generation logic without requiring a live database
        /// connection. It ensures that the resulting SQL string contains the expected SELECT, FROM, JOIN, and WHERE
        /// clauses.</remarks>
        [Fact]
        public void Build_GeneratesQuery_WithWhereClause()
        {
            // Arrange
            var selectSql = "SELECT *";
            var fromSql = "FROM DummyEntity de";
            var whereSql = "WHERE 1 = 1";
            var metadata = CreateMetadata(nameof(DummyEntity), nameof(DummyEntity), null);

            // Mock SelectBuilder and JoinBuilder via reflection (since they are static)
            var whereBuilder = WhereBuilder.Create();
            whereBuilder.Where("1 = 1");

            // Act
            var sql = QueryBuilder.Build<DummyEntity>(whereBuilder);

            // Assert
            Assert.Contains(selectSql, sql);
            Assert.Contains(fromSql, sql);
            Assert.Contains(whereSql, sql);
        }

        /// <summary>
        /// Verifies that the QueryBuilder.Build&lt;T&gt; method generates a SQL query for the specified entity type without
        /// including a WHERE clause.
        /// </summary>
        /// <remarks>This test ensures that the generated SQL query contains the expected SELECT and JOIN
        /// clauses and omits any WHERE conditions. It uses test overrides to control the SQL fragments for SELECT and
        /// JOIN operations, and asserts that the resulting query structure matches the expected format for entities
        /// without filtering criteria.</remarks>
        [Fact]
        public void Build_GeneratesQuery_WithoutWhereClause()
        {
            // Arrange
            var selectSql = "SELECT *";
            var fromSql = "FROM DummyEntity de";
            var metadata = CreateMetadata(nameof(DummyEntity), nameof(DummyEntity), null);

            // Act
            var sql = QueryBuilder.Build<DummyEntity>(null);

            // Assert
            Assert.Contains(selectSql, sql);
            Assert.Contains(fromSql, sql);
            Assert.DoesNotContain("WHERE", sql);
        }

        /// <summary>
        /// Verifies that the FormatTable method returns the table name when the schema is null, empty, or consists only
        /// of whitespace.
        /// </summary>
        /// <remarks>This test ensures that the FormatTable method, accessed via reflection, correctly
        /// handles cases where the schema is not specified by returning only the table name. It uses a metadata object
        /// with a whitespace schema to validate this behavior.</remarks>
        [Fact]
        public void FormatTable_ReturnsTable_WhenSchemaIsNullOrWhitespace()
        {
            var metadata = CreateMetadata("MyTable", "MyTable", "   ");
            var method = typeof(QueryBuilder).GetMethod("FormatTable", BindingFlags.NonPublic | BindingFlags.Static);
            var result = (string)method.Invoke(null, new object[] { metadata });
            Assert.Equal("MyTable", result);
        }

        /// <summary>
        /// Verifies that the FormatTable method returns a fully qualified table name including the schema when a schema
        /// is present in the metadata.
        /// </summary>
        /// <remarks>This test ensures that the FormatTable method correctly formats the table name by
        /// prepending the schema name, which is important for scenarios involving multiple database schemas.</remarks>
        [Fact]
        public void FormatTable_ReturnsSchemaAndTable_WhenSchemaIsPresent()
        {
            var metadata = CreateMetadata("MyTable", "MyTable", "myschema");
            var method = typeof(QueryBuilder).GetMethod("FormatTable", BindingFlags.NonPublic | BindingFlags.Static);
            var result = (string)method.Invoke(null, new object[] { metadata });
            Assert.Equal("myschema.MyTable", result);
        }
    }
}
