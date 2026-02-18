using Hydrix.Orchestrator.Builders.Query;
using Hydrix.Orchestrator.Builders.Query.Conditions;
using Hydrix.Orchestrator.Metadata.Builders;
using Hydrix.Orchestrator.Metadata.Internals;
using System.Collections.Generic;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Builders.Query
{
    /// <summary>
    /// Provides unit tests for the QueryBuilder class to verify the correct generation of SQL queries based on various
    /// metadata configurations.
    /// </summary>
    /// <remarks>These tests cover scenarios such as generating queries with select statements, joins, and
    /// where clauses, as well as handling cases with and without database schemas. The tests ensure that the
    /// QueryBuilder produces SQL statements that match expected patterns, helping to validate its functionality and
    /// reliability.</remarks>
    public class QueryBuilderTests
    {
        /// <summary>
        /// Represents a dummy entity used for testing purposes.
        /// </summary>
        private class DummyEntity
        {
            /// <summary>
            /// Gets or sets a value for demonstration or testing purposes.
            /// </summary>
            public object DummyProperty { get; set; }
        }

        /// <summary>
        /// Creates and configures metadata for an entity builder, specifying the associated table, schema, alias,
        /// columns, and joins.
        /// </summary>
        /// <param name="table">The name of the database table to associate with the entity. Defaults to "orders" if not specified.</param>
        /// <param name="schema">The name of the schema that contains the table. Defaults to "sales" if not specified.</param>
        /// <param name="alias">The alias to use for the entity in queries. Defaults to "o" if not specified.</param>
        /// <param name="columns">A list of column metadata to include in the entity. If null, an empty list is used.</param>
        /// <param name="joins">A list of join metadata to include in the entity. If null, an empty list is used.</param>
        /// <returns>An instance of EntityBuilderMetadata containing the specified configuration for the entity.</returns>
        private static EntityBuilderMetadata CreateMetadata(
            string table = "orders",
            string schema = "sales",
            string alias = "o",
            List<ColumnBuilderMetadata> columns = null,
            List<JoinBuilderMetadata> joins = null)
        {
            return new EntityBuilderMetadata(
                entityType: typeof(DummyEntity),
                table: table,
                schema: schema,
                alias: alias,
                columns: columns ?? new List<ColumnBuilderMetadata>(),
                joins: joins ?? new List<JoinBuilderMetadata>()
            );
        }

        /// <summary>
        /// Verifies that the QueryBuilder.Build method generates a SQL query string containing the expected SELECT,
        /// FROM, and LEFT JOIN clauses when provided with column and join metadata.
        /// </summary>
        /// <remarks>This test ensures that the query builder correctly constructs SQL queries with the
        /// specified columns and joins, including support for optional joins. It checks for the presence of key SQL
        /// clauses in the generated query to validate proper query assembly.</remarks>
        [Fact]
        public void Build_GeneratesQuery_WithSelectFromAndJoins()
        {
            var columns = new List<ColumnBuilderMetadata>
            {
                new ColumnBuilderMetadata(
                    "Id",
                    "id",
                    true,
                    true,
                    MetadataFactory.CreateGetter(typeof(DummyEntity).GetProperty("DummyProperty")))
            };
            var joins = new List<JoinBuilderMetadata>
            {
                new JoinBuilderMetadata(
                    table: "customers",
                    schema: "sales",
                    alias: "c",
                    primaryKeys: new[] { "Id" },
                    foreignKeys: new[] { "CustomerId" },
                    isRequiredJoin: false,
                    navigationProperty: null)
            };
            var metadata = CreateMetadata(columns: columns, joins: joins);

            var sql = QueryBuilder.Build(metadata, null);

            Assert.Contains("SELECT", sql);
            Assert.Contains("FROM sales.orders o", sql);
            Assert.Contains("LEFT JOIN sales.customers c", sql);
        }

        /// <summary>
        /// Verifies that the QueryBuilder.Build method generates a SQL query string containing a WHERE clause with the
        /// specified conditions.
        /// </summary>
        /// <remarks>This test ensures that the resulting SQL query includes both the WHERE keyword and
        /// the expected condition expressions, validating correct query construction when filtering criteria are
        /// provided.</remarks>
        [Fact]
        public void Build_GeneratesQuery_WithWhereClause()
        {
            var metadata = CreateMetadata();
            var where = WhereBuilder.Create()
                .And("o.Id > @minId")
                .Or("o.Status = @status");

            var sql = QueryBuilder.Build(metadata, where);

            Assert.Contains("WHERE", sql);
            Assert.Contains("o.Id > @minId", sql);
            Assert.Contains("o.Status = @status", sql);
        }

        /// <summary>
        /// Verifies that the SQL query generated for a specified table does not include schema information when the
        /// schema is not provided.
        /// </summary>
        /// <remarks>This test ensures that the generated SQL query correctly references the table and
        /// alias without including a schema prefix. It is intended for scenarios where schema information is either
        /// unavailable or unnecessary.</remarks>
        [Fact]
        public void Build_GeneratesQuery_WithoutSchema()
        {
            var metadata = CreateMetadata(table: "orders", schema: null, alias: "o");
            var sql = QueryBuilder.Build(metadata, null);

            Assert.Contains("FROM orders o", sql);
        }

        /// <summary>
        /// Verifies that the Build method generates a SQL query without including a WHERE clause when no filter is
        /// provided.
        /// </summary>
        /// <remarks>This test ensures that the query builder returns all records without restrictions
        /// when the filter parameter is null. It is useful for validating scenarios where unfiltered data retrieval is
        /// required.</remarks>
        [Fact]
        public void Build_GeneratesQuery_WithoutWhereClause()
        {
            var metadata = CreateMetadata();
            var sql = QueryBuilder.Build(metadata, null);

            Assert.DoesNotContain("WHERE", sql);
        }

        /// <summary>
        /// Verifies that the query builder does not include a WHERE clause when no conditions are specified.
        /// </summary>
        /// <remarks>This test ensures that when the where clause is empty, the generated SQL query omits
        /// the WHERE keyword, confirming correct query generation in scenarios without filtering.</remarks>
        [Fact]
        public void Build_GeneratesQuery_WithEmptyWhereClause()
        {
            var metadata = CreateMetadata();
            var where = WhereBuilder.Create(); // No conditions
            var sql = QueryBuilder.Build(metadata, where);

            Assert.DoesNotContain("WHERE", sql);
        }
    }
}