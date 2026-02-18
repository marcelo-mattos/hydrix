using Hydrix.Orchestrator.Builders.Query.Conditions;
using System;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Builders.Query.Conditions
{
    /// <summary>
    /// Contains unit tests for the WhereBuilder class, verifying its construction, clearing, and SQL clause building behavior.
    /// </summary>
    public partial class WhereBuilderTests
    {
        /// <summary>
        /// Builds a SQL WHERE clause by applying the specified configuration action to a WhereBuilder instance.
        /// </summary>
        /// <param name="builderAction">An action that configures the WhereBuilder to define the conditions for the WHERE clause. Cannot be null.</param>
        /// <returns>A string containing the generated SQL WHERE clause based on the configured conditions.</returns>
        private static string BuildWhere(Action<WhereBuilder> builderAction)
        {
            var builder = WhereBuilder.Create();
            builderAction(builder);
            return builder.Build();
        }

        /// <summary>
        /// Verifies that Create returns a new instance of WhereBuilder.
        /// </summary>
        [Fact]
        public void Create_ReturnsNewInstance()
        {
            var builder = WhereBuilder.Create();
            Assert.NotNull(builder);
            Assert.IsType<WhereBuilder>(builder);
        }

        /// <summary>
        /// Verifies that Clear empties the builder and is chainable.
        /// </summary>
        [Fact]
        public void Clear_EmptiesBuilder_Chainable()
        {
            var builder = WhereBuilder.Create();
            // Add a token using reflection for test purposes
            var tokensField = typeof(WhereBuilder).GetField("_tokens", BindingFlags.NonPublic | BindingFlags.Instance);
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
            var builder = WhereBuilder.Create();
            Assert.Equal(string.Empty, builder.Build());
        }

        /// <summary>
        /// Verifies that Build returns a WHERE clause when conditions are present.
        /// </summary>
        [Fact]
        public void Build_WithConditions_ReturnsWhereClause()
        {
            var builder = WhereBuilder.Create();
            // Add a token using reflection for test purposes
            var tokensField = typeof(WhereBuilder).GetField("_tokens", BindingFlags.NonPublic | BindingFlags.Instance);
            var tokens = (System.Collections.IList)tokensField.GetValue(builder);
            tokens.Add("Id = 1");

            // Use reflection to invoke BuildInternal
            var buildInternalMethod = typeof(WhereBuilder).GetMethod("BuildInternal", BindingFlags.NonPublic | BindingFlags.Instance);
            if (buildInternalMethod == null)
                throw new InvalidOperationException("BuildInternal method not found.");

            var sql = buildInternalMethod.Invoke(builder, null) as string;
            var expected = string.IsNullOrEmpty(sql) ? string.Empty : $"WHERE {sql}";
            Assert.Equal(expected, builder.Build());
            Assert.StartsWith("WHERE ", builder.Build());
        }
    }
}