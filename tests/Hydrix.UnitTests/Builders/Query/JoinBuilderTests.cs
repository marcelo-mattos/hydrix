using Hydrix.Builders.Query;
using Hydrix.Metadata.Builders;
using System.Collections.Generic;
using Xunit;

namespace Hydrix.UnitTests.Builders.Query
{
    /// <summary>
    /// Contains unit tests for the JoinBuilder class, verifying the generation of SQL join statements based on various
    /// configurations and conditions.
    /// </summary>
    /// <remarks>This class includes tests for generating INNER JOIN and LEFT JOIN SQL statements, handling
    /// multiple key pairs, and formatting table names with or without schemas. It ensures that the SQL output is
    /// correct and adheres to expected behaviors in different scenarios.</remarks>
    public class JoinBuilderTests
    {
        /// <summary>
        /// Creates a new instance of the JoinBuilderMetadata class for the specified table and schema, configuring the
        /// join as required or optional and specifying the primary and foreign key columns.
        /// </summary>
        /// <param name="entity">The identifier of the entity associated with this join. Cannot be null or empty.</param>
        /// <param name="table">The name of the table to join.</param>
        /// <param name="schema">The schema of the table to join.</param>
        /// <param name="isRequiredJoin">true to indicate that the join is required; otherwise, false to make the join optional.</param>
        /// <param name="primaryKeys">An array of column names that represent the primary keys of the table.</param>
        /// <param name="foreignKeys">An array of column names that represent the foreign keys referencing the primary keys in the related table.</param>
        /// <param name="columns">A list of ForeignColumnMetadata instances that provide additional metadata about the columns involved in the join. This parameter is required and cannot be null.</param>
        /// <returns>A JoinBuilderMetadata instance that contains the specified join configuration.</returns>
        private JoinBuilderMetadata CreateJoin(
            string entity,
            string table,
            string schema,
            bool isRequiredJoin,
            string[] primaryKeys,
            string[] foreignKeys,
            List<ForeignColumnMetadata> columns)
        {
            // O parâmetro columns é obrigatório, então passamos uma lista vazia.
            return new JoinBuilderMetadata(
                entity,
                table,
                schema,
                primaryKeys,
                foreignKeys,
                isRequiredJoin,
                columns
            );
        }

        /// <summary>
        /// Verifies that the JoinBuilder.Build method generates a correct SQL INNER JOIN statement when a schema is
        /// specified for the joined table.
        /// </summary>
        /// <remarks>This test ensures that the generated SQL includes the schema-qualified table name and
        /// the appropriate join conditions using table aliases. It validates that the INNER JOIN clause references the
        /// correct schema and that the join keys are properly aliased.</remarks>
        [Fact]
        public void Build_GeneratesInnerJoin_WithSchema()
        {
            var joins = new List<JoinBuilderMetadata>
            {
                CreateJoin("Customer", "Customer", "dbo", true, new[] { "Id" }, new[] { "CustomerId" }, new List<ForeignColumnMetadata>())
            };
            var metadata = new EntityBuilderMetadata(
                "Order", typeof(object), "Order", "dbo", new List<ColumnBuilderMetadata>(), joins);

            var aliasContext = new AliasContext();
            var mainAlias = aliasContext.GetAlias("Order");
            aliasContext.GetAlias("Customer"); // Ensures alias is generated

            var sql = JoinBuilder.Build(metadata, mainAlias, aliasContext);

            Assert.Contains("INNER JOIN dbo.Customer", sql);
            Assert.Contains($"{mainAlias}.CustomerId =", sql);
            Assert.Contains("= c.Id", sql); // c is the alias for Customer
        }

        /// <summary>
        /// Verifies that the JoinBuilder.Build method generates a correct LEFT JOIN SQL statement between the 'Order'
        /// and 'Agent' entities when no schema information is provided.
        /// </summary>
        /// <remarks>This test ensures that the join condition is properly constructed using only the
        /// provided metadata, and that the resulting SQL includes the expected LEFT JOIN clause and join condition. It
        /// is intended to validate scenarios where schema details are unavailable, relying solely on entity and column
        /// metadata.</remarks>
        [Fact]
        public void Build_GeneratesLeftJoin_WithoutSchema()
        {
            var joins = new List<JoinBuilderMetadata>
            {
                CreateJoin("Agent", "Agent", null, false, new[] { "Id" }, new[] { "AgentId" }, new List<ForeignColumnMetadata>())
            };
            var metadata = new EntityBuilderMetadata(
                "Order", typeof(object), "Order", null, new List<ColumnBuilderMetadata>(), joins);

            var aliasContext = new AliasContext();
            var mainAlias = aliasContext.GetAlias("Order");
            aliasContext.GetAlias("Agent");

            var sql = JoinBuilder.Build(metadata, mainAlias, aliasContext);

            Assert.Contains("LEFT JOIN Agent", sql);
            Assert.Contains($"{mainAlias}.AgentId = a.Id", sql); // a is the alias for Agent
        }

        /// <summary>
        /// Verifies that the Build method generates correct SQL join statements for multiple join configurations.
        /// </summary>
        /// <remarks>This test ensures that both inner and left joins are constructed as expected based on
        /// the provided entity metadata, including correct table aliases and join conditions.</remarks>
        [Fact]
        public void Build_GeneratesMultipleJoins()
        {
            var joins = new List<JoinBuilderMetadata>
            {
                CreateJoin("Customer", "Customer", "sales", true, new[] { "Id" }, new[] { "CustomerId" }, new List<ForeignColumnMetadata>()),
                CreateJoin("Agent", "Agent", null, false, new[] { "Id" }, new[] { "AgentId" }, new List<ForeignColumnMetadata>())
            };
            var metadata = new EntityBuilderMetadata(
                "Order", typeof(object), "Order", "sales", new List<ColumnBuilderMetadata>(), joins);

            var aliasContext = new AliasContext();
            var mainAlias = aliasContext.GetAlias("Order");
            aliasContext.GetAlias("Customer");
            aliasContext.GetAlias("Agent");

            var sql = JoinBuilder.Build(metadata, mainAlias, aliasContext);

            Assert.Contains("INNER JOIN sales.Customer", sql);
            Assert.Contains("LEFT JOIN Agent", sql);
            Assert.Contains($"{mainAlias}.CustomerId = c.Id", sql);
            Assert.Contains($"{mainAlias}.AgentId = a.Id", sql);
        }

        /// <summary>
        /// Verifies that the SQL query builder correctly handles join conditions involving multiple key pairs.
        /// </summary>
        /// <remarks>This test ensures that the generated SQL includes the expected INNER JOIN clause and
        /// accurately matches each specified key pair between the joined entities. It is important for validating the
        /// join logic in scenarios where composite keys are used.</remarks>
        [Fact]
        public void Build_HandlesMultipleKeyPairs()
        {
            var joins = new List<JoinBuilderMetadata>
            {
                CreateJoin("Order", "Order", "dbo", true, new[] { "Id", "Type" }, new[] { "OrderId", "OrderType" }, new List<ForeignColumnMetadata>())
            };
            var metadata = new EntityBuilderMetadata(
                "Invoice",  typeof(object), "Invoice", "dbo", new List<ColumnBuilderMetadata>(), joins);

            var aliasContext = new AliasContext();
            var mainAlias = aliasContext.GetAlias("Invoice");
            aliasContext.GetAlias("Order");

            var sql = JoinBuilder.Build(metadata, mainAlias, aliasContext);

            Assert.Contains("INNER JOIN dbo.Order", sql);
            Assert.Contains($"{mainAlias}.OrderId = o.Id AND {mainAlias}.OrderType = o.Type", sql);
        }

        /// <summary>
        /// Verifies that the Build method returns an empty string when no join conditions are specified in the
        /// metadata.
        /// </summary>
        /// <remarks>This test ensures that the SQL join string generation logic correctly handles cases
        /// where no joins are present, maintaining valid SQL output.</remarks>
        [Fact]
        public void Build_ReturnsEmptyString_WhenNoJoins()
        {
            var metadata = new EntityBuilderMetadata(
                "Order", typeof(object), "Order", "dbo", new List<ColumnBuilderMetadata>(), new List<JoinBuilderMetadata>());

            var aliasContext = new AliasContext();
            var mainAlias = aliasContext.GetAlias("Order");

            var sql = JoinBuilder.Build(metadata, mainAlias, aliasContext);

            Assert.Equal(string.Empty, sql);
        }

        /// <summary>
        /// Verifies that the FormatTable method returns the table name when the schema is null, empty, or consists only
        /// of whitespace.
        /// </summary>
        /// <remarks>This test ensures that FormatTable does not prepend a schema when the schema
        /// parameter is not provided or is whitespace, and that it returns the table name as expected.</remarks>
        [Fact]
        public void FormatTable_ReturnsTable_WhenSchemaIsNullOrWhitespace()
        {
            var join = CreateJoin("MyTable", "MyTable", "   ", true, new[] { "Id" }, new[] { "Id" }, new List<ForeignColumnMetadata>());
            var method = typeof(JoinBuilder).GetMethod("FormatTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var result = (string)method.Invoke(null, new object[] { join });
            Assert.Equal("MyTable", result);
        }

        /// <summary>
        /// Verifies that the FormatTable method returns the fully qualified table name, including the schema, when a
        /// schema is present.
        /// </summary>
        /// <remarks>This test ensures that the FormatTable method correctly formats the table name by
        /// combining the schema and table name when the schema is specified. It uses reflection to invoke the
        /// non-public static FormatTable method of the JoinBuilder class and asserts that the result matches the
        /// expected format.</remarks>
        [Fact]
        public void FormatTable_ReturnsSchemaAndTable_WhenSchemaIsPresent()
        {
            var join = CreateJoin("MyTable", "MyTable", "myschema", true, new[] { "Id" }, new[] { "Id" }, new List<ForeignColumnMetadata>());
            var method = typeof(JoinBuilder).GetMethod("FormatTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var result = (string)method.Invoke(null, new object[] { join });
            Assert.Equal("myschema.MyTable", result);
        }
    }
}
