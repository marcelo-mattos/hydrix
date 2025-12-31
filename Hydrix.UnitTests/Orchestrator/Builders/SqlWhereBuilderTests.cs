using Hydrix.Orchestrator.Builders;
using System;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Builders
{
    /// <summary>
    /// Contains unit tests for the SqlWhereBuilder class, verifying its construction, clearing, and SQL clause building behavior.
    /// </summary>
    public partial class SqlWhereBuilderTests
    {
        /// <summary>
        /// Builds a SQL WHERE clause by applying the specified configuration action to a SqlWhereBuilder instance.
        /// </summary>
        /// <param name="builderAction">An action that configures the SqlWhereBuilder to define the conditions for the WHERE clause. Cannot be null.</param>
        /// <returns>A string containing the generated SQL WHERE clause based on the configured conditions.</returns>
        private static string BuildWhere(Action<SqlWhereBuilder> builderAction)
        {
            var builder = SqlWhereBuilder.Create();
            builderAction(builder);
            return builder.Build();
        }

        /// <summary>
        /// Verifies that Create returns a new instance of SqlWhereBuilder.
        /// </summary>
        [Fact]
        public void Create_ReturnsNewInstance()
        {
            var builder = SqlWhereBuilder.Create();
            Assert.NotNull(builder);
            Assert.IsType<SqlWhereBuilder>(builder);
        }

        /// <summary>
        /// Verifies that Clear empties the builder and is chainable.
        /// </summary>
        [Fact]
        public void Clear_EmptiesBuilder_Chainable()
        {
            var builder = SqlWhereBuilder.Create();
            // Add a token using reflection for test purposes
            var tokensField = typeof(SqlWhereBuilder).GetField("_tokens", BindingFlags.NonPublic | BindingFlags.Instance);
            var tokens = (System.Collections.IList)tokensField.GetValue(builder);
            tokens.Add("Id = 1");
            Assert.Single(tokens);

            var result = builder.Clear();
            Assert.Empty((System.Collections.IList)tokensField.GetValue(builder));
            Assert.Same(builder, result);
        }

        /// <summary>
        /// Verifies that Build returns an empty string when no conditions are present.
        /// </summary>
        [Fact]
        public void Build_NoConditions_ReturnsEmptyString()
        {
            var builder = SqlWhereBuilder.Create();
            Assert.Equal(string.Empty, builder.Build());
        }

        /// <summary>
        /// Verifies that Build returns a WHERE clause when conditions are present.
        /// </summary>
        [Fact]
        public void Build_WithConditions_ReturnsWhereClause()
        {
            var builder = SqlWhereBuilder.Create();
            // Add a token using reflection for test purposes
            var tokensField = typeof(SqlWhereBuilder).GetField("_tokens", BindingFlags.NonPublic | BindingFlags.Instance);
            var tokens = (System.Collections.IList)tokensField.GetValue(builder);
            tokens.Add("Id = 1");

            // Use reflection to invoke BuildInternal
            var buildInternalMethod = typeof(SqlWhereBuilder).GetMethod("BuildInternal", BindingFlags.NonPublic | BindingFlags.Instance);
            if (buildInternalMethod == null)
                throw new InvalidOperationException("BuildInternal method not found.");

            var sql = buildInternalMethod.Invoke(builder, null) as string;
            var expected = string.IsNullOrEmpty(sql) ? string.Empty : $"WHERE {sql}";
            Assert.Equal(expected, builder.Build());
            Assert.StartsWith("WHERE ", builder.Build());
        }
    }
}