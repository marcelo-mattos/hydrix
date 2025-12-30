using Hydrix.Orchestrator.Builders;
using System;
using Xunit;

namespace Hydrix.UnitTest.Orchestrator.Builders
{
    /// <summary>
    /// Contains unit tests for the SqlWhereBuilder grouped AND/AND NOT methods,
    /// verifying correct SQL clause composition and conditional logic.
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
        /// Verifies that using a single condition within an AND group generates the correct grouped SQL WHERE clause.
        /// </summary>
        /// <remarks>This test ensures that the SQL builder correctly wraps a single AND condition in
        /// parentheses when constructing the WHERE clause. Grouping is important for maintaining logical correctness in
        /// complex queries.</remarks>
        [Fact]
        public void AndGroup_SingleCondition_GeneratesGroupedSql()
        {
            var sql = BuildWhere(w => w.AndGroup(g => g.And("A = 1")));
            Assert.Equal("WHERE (A = 1)", sql);
        }

        /// <summary>
        /// Verifies that grouping multiple conditions with 'AndGroup' generates a SQL WHERE clause with the conditions
        /// combined using the AND operator and enclosed in parentheses.
        /// </summary>
        /// <remarks>This test ensures that when multiple conditions are added to an 'AndGroup', the
        /// resulting SQL groups the conditions together and joins them with AND, as expected for correct logical
        /// grouping in SQL queries.</remarks>
        [Fact]
        public void AndGroup_MultipleConditions_GeneratesGroupedSqlWithAnd()
        {
            var sql = BuildWhere(w => w.AndGroup(g =>
            {
                g.And("A = 1");
                g.And("B = 2");
            }));
            Assert.Equal("WHERE (A = 1 AND B = 2)", sql);
        }

        /// <summary>
        /// Verifies that using AndNotGroup with a single condition generates the correct SQL with a grouped NOT clause.
        /// </summary>
        /// <remarks>This test ensures that the AndNotGroup method produces a WHERE clause with the
        /// condition grouped inside a NOT expression, matching the expected SQL syntax.</remarks>
        [Fact]
        public void AndNotGroup_SingleCondition_GeneratesGroupedSqlWithNot()
        {
            var sql = BuildWhere(w => w.AndNotGroup(g => g.And("A = 1")));
            Assert.Equal("WHERE NOT (A = 1)", sql);
        }

        /// <summary>
        /// Verifies that the AndNotGroup method generates a SQL WHERE clause with multiple conditions grouped using NOT
        /// and AND operators.
        /// </summary>
        /// <remarks>This test ensures that when multiple conditions are added within an AndNotGroup, the
        /// resulting SQL groups the conditions with parentheses and applies the NOT operator to the entire
        /// group.</remarks>
        [Fact]
        public void AndNotGroup_MultipleConditions_GeneratesGroupedSqlWithNotAnd()
        {
            var sql = BuildWhere(w => w.AndNotGroup(g =>
            {
                g.And("A = 1");
                g.And("B = 2");
            }));
            Assert.Equal("WHERE NOT (A = 1 AND B = 2)", sql);
        }

        /// <summary>
        /// Verifies that the AndGroupIf method conditionally generates a SQL WHERE clause based on the specified
        /// predicate.
        /// </summary>
        /// <param name="predicate">A value indicating whether the SQL condition should be included. If <see langword="true"/>, the condition is
        /// added; otherwise, it is omitted.</param>
        /// <param name="expected">The expected SQL string result after applying the conditional group.</param>
        [Theory]
        [InlineData(true, "WHERE (A = 1)")]
        [InlineData(false, "")]
        public void AndGroupIf_Predicate_GeneratesSqlConditionally(bool predicate, string expected)
        {
            var sql = BuildWhere(w => w.AndGroupIf(predicate, g => g.And("A = 1")));
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that the AndGroupIf method generates the expected SQL WHERE clause based on the provided predicate
        /// array.
        /// </summary>
        /// <remarks>This test ensures that AndGroupIf only includes the grouped SQL condition when all
        /// predicates are true. If any predicate is false or the array is null, no condition is generated.</remarks>
        /// <param name="predicates">An array of Boolean values that determine whether the SQL condition should be included. If the array is null
        /// or contains any false value, the condition is not added.</param>
        /// <param name="expected">The expected SQL WHERE clause to compare against the generated result.</param>
        [Theory]
        [InlineData(new[] { true, true }, "WHERE (A = 1)")]
        [InlineData(new[] { true, false }, "")]
        [InlineData(null, "")]
        public void AndGroupIf_ArrayPredicate_GeneratesSqlConditionally(bool[] predicates, string expected)
        {
            var sql = BuildWhere(w => w.AndGroupIf(predicates, g => g.And("A = 1")));
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that the AndNotGroupIf method conditionally generates a SQL WHERE clause with a NOT group based on
        /// the specified predicate.
        /// </summary>
        /// <param name="predicate">A value indicating whether the NOT group should be included in the generated SQL WHERE clause.</param>
        /// <param name="expected">The expected SQL WHERE clause output for the given predicate value.</param>
        [Theory]
        [InlineData(true, "WHERE NOT (A = 1)")]
        [InlineData(false, "")]
        public void AndNotGroupIf_Predicate_GeneratesSqlConditionally(bool predicate, string expected)
        {
            var sql = BuildWhere(w => w.AndNotGroupIf(predicate, g => g.And("A = 1")));
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that the AndNotGroupIf method generates the expected SQL condition based on the provided predicate
        /// array.
        /// </summary>
        /// <remarks>This test ensures that AndNotGroupIf only generates the 'NOT' group SQL condition
        /// when all elements in the predicates array are true. If the array is null or contains any false value, no
        /// condition is generated.</remarks>
        /// <param name="predicates">An array of Boolean values that determine whether the SQL 'NOT' group condition should be included. If the
        /// array is null or contains any false value, the condition is not generated.</param>
        /// <param name="expected">The expected SQL string result to compare against the generated output.</param>
        [Theory]
        [InlineData(new[] { true, true }, "WHERE NOT (A = 1)")]
        [InlineData(new[] { true, false }, "")]
        [InlineData(null, "")]
        public void AndNotGroupIf_ArrayPredicate_GeneratesSqlConditionally(bool[] predicates, string expected)
        {
            var sql = BuildWhere(w => w.AndNotGroupIf(predicates, g => g.And("A = 1")));
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that nested AND groups in a WHERE clause are correctly translated to SQL syntax.
        /// </summary>
        /// <remarks>This test ensures that combining multiple AND conditions, including nested groups,
        /// produces the expected SQL output. It is useful for validating the correct handling of logical grouping in
        /// query generation.</remarks>
        [Fact]
        public void AndGroup_NestedGroups_GeneratesCorrectSql()
        {
            var sql = BuildWhere(w => w.AndGroup(g =>
            {
                g.And("A = 1");
                g.AndGroup(h => h.And("B = 2"));
            }));
            Assert.Equal("WHERE (A = 1 AND (B = 2))", sql);
        }

        /// <summary>
        /// Verifies that nested calls to AndNotGroup generate the correct SQL WHERE clause with proper logical grouping
        /// and negation.
        /// </summary>
        /// <remarks>This test ensures that combining And and AndNotGroup expressions produces the
        /// expected SQL syntax, particularly when AndNotGroup is nested within another AndNotGroup. It helps validate
        /// the correct handling of logical operators and parentheses in the generated SQL.</remarks>
        [Fact]
        public void AndNotGroup_NestedGroups_GeneratesCorrectSql()
        {
            var sql = BuildWhere(w => w.AndNotGroup(g =>
            {
                g.And("A = 1");
                g.AndNotGroup(h => h.And("B = 2"));
            }));
            Assert.Equal("WHERE NOT (A = 1 AND NOT (B = 2))", sql);
        }
    }
}