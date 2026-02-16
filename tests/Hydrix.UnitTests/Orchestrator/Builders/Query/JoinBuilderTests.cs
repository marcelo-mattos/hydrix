using Hydrix.Orchestrator.Builders.Query;
using Hydrix.Orchestrator.Metadata.Builders;
using System.Collections.Generic;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Builders.Query
{
    /// <summary>
    /// Contains unit tests for the JoinBuilder class, validating the generation of SQL join clauses based on provided
    /// metadata.
    /// </summary>
    /// <remarks>These tests cover various scenarios including left joins, inner joins, multiple joins, and
    /// the case where no joins are specified. Each test verifies that the generated SQL matches the expected format and
    /// conditions.</remarks>
    public class JoinBuilderTests
    {
        /// <summary>
        /// Verifies that the JoinBuilder.Build method generates a LEFT JOIN SQL clause with the correct schema and join
        /// condition.
        /// </summary>
        /// <remarks>This test ensures that when provided with entity and join metadata including a
        /// schema, the generated SQL includes the schema-qualified table name and the appropriate ON clause for the
        /// join. It validates that the LEFT JOIN is constructed as expected for scenarios involving schemas.</remarks>
        [Fact]
        public void Build_GeneratesLeftJoinClause_WithSchema()
        {
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

            var metadata = new EntityBuilderMetadata(
                entityType: typeof(object),
                table: "orders",
                schema: "sales",
                alias: "o",
                columns: new List<ColumnBuilderMetadata>(),
                joins: joins
            );

            var sql = JoinBuilder.Build(metadata, "o");

            Assert.Contains("LEFT JOIN sales.customers c", sql);
            Assert.Contains("o.CustomerId = c.Id", sql);
        }

        /// <summary>
        /// Verifies that the Build method generates a correct SQL INNER JOIN clause for the specified entity metadata
        /// when no schema information is provided.
        /// </summary>
        /// <remarks>This test ensures that the generated SQL includes the expected INNER JOIN statement
        /// and join condition, even when the schema is omitted from the metadata. It validates that the join is
        /// constructed using the provided table names, aliases, and key columns.</remarks>
        [Fact]
        public void Build_GeneratesInnerJoinClause_WithoutSchema()
        {
            var joins = new List<JoinBuilderMetadata>
            {
                new JoinBuilderMetadata(
                    table: "products",
                    schema: null,
                    alias: "p",
                    primaryKeys: new[] { "ProductId" },
                    foreignKeys: new[] { "Id" },
                    isRequiredJoin: true,
                    navigationProperty: null)
            };

            var metadata = new EntityBuilderMetadata(
                entityType: typeof(object),
                table: "order_items",
                schema: null,
                alias: "oi",
                columns: new List<ColumnBuilderMetadata>(),
                joins: joins
            );

            var sql = JoinBuilder.Build(metadata, "oi");

            Assert.Contains("INNER JOIN products p", sql);
            Assert.Contains("oi.Id = p.ProductId", sql);
        }

        /// <summary>
        /// Tests that the SQL generation logic correctly produces multiple JOIN clauses based on provided join
        /// metadata.
        /// </summary>
        /// <remarks>This test verifies that both required and optional joins are handled appropriately by
        /// the JoinBuilder, ensuring that the resulting SQL includes the correct JOIN types and ON conditions for each
        /// join defined in the entity metadata.</remarks>
        [Fact]
        public void Build_GeneratesMultipleJoins()
        {
            var joins = new List<JoinBuilderMetadata>
            {
                new JoinBuilderMetadata(
                    table: "customers",
                    schema: "sales",
                    alias: "c",
                    primaryKeys: new[] { "Id" },
                    foreignKeys: new[] { "CustomerId" },
                    isRequiredJoin: false,
                    navigationProperty: null),
                new JoinBuilderMetadata(
                    table: "agents",
                    schema: "sales",
                    alias: "a",
                    primaryKeys: new[] { "Id" },
                    foreignKeys: new[] { "AgentId" },
                    isRequiredJoin: true,
                    navigationProperty: null)
            };

            var metadata = new EntityBuilderMetadata(
                entityType: typeof(object),
                table: "orders",
                schema: "sales",
                alias: "o",
                columns: new List<ColumnBuilderMetadata>(),
                joins: joins
            );

            var sql = JoinBuilder.Build(metadata, "o");

            Assert.Contains("LEFT JOIN sales.customers c", sql);
            Assert.Contains("o.CustomerId = c.Id", sql);
            Assert.Contains("INNER JOIN sales.agents a", sql);
            Assert.Contains("o.AgentId = a.Id", sql);
        }

        /// <summary>
        /// Verifies that the Build method returns an empty string when no join metadata is specified.
        /// </summary>
        /// <remarks>This test ensures that the SQL join clause is omitted when the provided metadata does
        /// not contain any joins. It helps confirm that the Build method behaves correctly in scenarios where no join
        /// operations are required.</remarks>
        [Fact]
        public void Build_ReturnsEmptyString_WhenNoJoins()
        {
            var metadata = new EntityBuilderMetadata(
                entityType: typeof(object),
                table: "orders",
                schema: "sales",
                alias: "o",
                columns: new List<ColumnBuilderMetadata>(),
                joins: new List<JoinBuilderMetadata>()
            );

            var sql = JoinBuilder.Build(metadata, "o");

            Assert.Equal(string.Empty, sql);
        }
    }
}