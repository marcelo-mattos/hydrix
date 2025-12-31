using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Builders
{
    /// <summary>
    /// Contains unit tests for the SqlWhereBuilder OR/OR NOT methods,
    /// verifying correct SQL clause composition and conditional logic.
    /// </summary>
    public partial class SqlWhereBuilderTests
    {
        /// <summary>
        /// Verifies that applying a single condition with the Or method generates the expected SQL WHERE clause.
        /// </summary>
        /// <remarks>This test ensures that the SQL builder correctly formats a simple WHERE clause when a
        /// single OR condition is provided.</remarks>
        [Fact]
        public void Or_SingleCondition_GeneratesSql()
        {
            var sql = BuildWhere(w => w.Or("A = 1"));
            Assert.Equal("WHERE A = 1", sql);
        }

        /// <summary>
        /// Verifies that combining multiple conditions with 'Or' generates a SQL WHERE clause that joins the
        /// conditions using the OR operator.
        /// </summary>
        /// <remarks>This test ensures that the query builder correctly formats SQL when multiple
        /// conditions are combined using the Or method. It checks that the resulting SQL string includes all specified
        /// conditions joined by the OR keyword.</remarks>
        [Fact]
        public void Or_MultipleConditions_GeneratesSqlWithOr()
        {
            var sql = BuildWhere(w =>
            {
                w.Or("A = 1");
                w.Or("B = 2");
            });
            Assert.Equal("WHERE A = 1 OR B = 2", sql);
        }

        /// <summary>
        /// Verifies that the OrNot method adds a NOT condition to the SQL WHERE clause as expected.
        /// </summary>
        /// <remarks>This test ensures that calling OrNot with a condition results in the correct SQL
        /// syntax, combining the condition with OR NOT in the WHERE clause.</remarks>
        [Fact]
        public void OrNot_AddsNotCondition()
        {
            var sql = BuildWhere(w =>
            {
                w.Or("A = 1");
                w.OrNot("B = 2");
            });
            Assert.Equal("WHERE A = 1 OR NOT B = 2", sql);
        }

        /// <summary>
        /// Verifies that the OrIf method adds the specified condition to the WHERE clause when the predicate is true.
        /// </summary>
        /// <remarks>This test ensures that calling OrIf with a predicate value of <see langword="true"/>
        /// results in the condition being appended to the SQL WHERE clause. It helps confirm the correct behavior of
        /// conditional query building.</remarks>
        [Fact]
        public void OrIf_PredicateTrue_AddsCondition()
        {
            var sql = BuildWhere(w =>
            {
                w.Or("A = 1");
                w.OrIf(true, "B = 2");
            });
            Assert.Equal("WHERE A = 1 OR B = 2", sql);
        }

        /// <summary>
        /// Verifies that the OrIf method does not add the specified condition when the predicate is false.
        /// </summary>
        /// <remarks>This test ensures that calling OrIf with a false predicate leaves the SQL WHERE
        /// clause unchanged, confirming that conditions are only added when the predicate evaluates to true.</remarks>
        [Fact]
        public void OrIf_PredicateFalse_DoesNotAddCondition()
        {
            var sql = BuildWhere(w =>
            {
                w.Or("A = 1");
                w.OrIf(false, "B = 2");
            });
            Assert.Equal("WHERE A = 1", sql);
        }

        /// <summary>
        /// Verifies that the OrIf method adds the specified condition to the WHERE clause only when all elements in
        /// the predicates array are true.
        /// </summary>
        /// <param name="predicates">An array of Boolean values that determine whether the additional condition should be included. If all
        /// elements are true, the condition is added; if any element is false or the array is null, the condition is
        /// not added.</param>
        /// <param name="expected">The expected SQL WHERE clause after applying the OrIf method.</param>
        [Theory]
        [InlineData(new[] { true, true }, "WHERE A = 1 OR B = 2")]
        [InlineData(new[] { true, false }, "WHERE A = 1")]
        [InlineData(null, "WHERE A = 1")]
        public void OrIf_ArrayPredicate_AddsConditionWhenAllTrue(bool[] predicates, string expected)
        {
            var sql = BuildWhere(w =>
            {
                w.Or("A = 1");
                w.OrIf(predicates, "B = 2");
            });
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that the OrNotIf method adds a NOT condition to the SQL WHERE clause when the predicate is true.
        /// </summary>
        /// <remarks>This test ensures that calling OrNotIf with a true predicate results in the
        /// specified condition being wrapped in a NOT clause and appended to the existing WHERE statement. It validates
        /// the correct behavior of conditional negation in SQL query construction.</remarks>
        [Fact]
        public void OrNotIf_PredicateTrue_AddsNotCondition()
        {
            var sql = BuildWhere(w =>
            {
                w.Or("A = 1");
                w.OrNotIf(true, "B = 2");
            });
            Assert.Equal("WHERE A = 1 OR NOT B = 2", sql);
        }

        /// <summary>
        /// Verifies that the OrNotIf method does not add the specified condition when the predicate is false.
        /// </summary>
        /// <remarks>This test ensures that calling OrNotIf with a false predicate leaves the SQL WHERE
        /// clause unchanged, confirming that conditions are only added when the predicate evaluates to true.</remarks>
        [Fact]
        public void OrNotIf_PredicateFalse_DoesNotAddCondition()
        {
            var sql = BuildWhere(w =>
            {
                w.Or("A = 1");
                w.OrNotIf(false, "B = 2");
            });
            Assert.Equal("WHERE A = 1", sql);
        }

        /// <summary>
        /// Verifies that the OrNotIf method adds a NOT condition to the WHERE clause only when all elements in the
        /// predicates array are true.
        /// </summary>
        /// <param name="predicates">An array of boolean values that determine whether the NOT condition should be added. If all elements are
        /// true, the NOT condition is included; if any element is false or the array is null, the condition is omitted.</param>
        /// <param name="expected">The expected SQL WHERE clause result after applying the OrNotIf method.</param>
        [Theory]
        [InlineData(new[] { true, true }, "WHERE A = 1 OR NOT B = 2")]
        [InlineData(new[] { true, false }, "WHERE A = 1")]
        [InlineData(null, "WHERE A = 1")]
        public void OrNotIf_ArrayPredicate_AddsNotConditionWhenAllTrue(bool[] predicates, string expected)
        {
            var sql = BuildWhere(w =>
            {
                w.Or("A = 1");
                w.OrNotIf(predicates, "B = 2");
            });
            Assert.Equal(expected, sql);
        }
    }
}