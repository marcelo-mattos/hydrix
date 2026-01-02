using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Builders
{
    /// <summary>
    /// Contains unit tests for the SqlWhereBuilder OR/OR NOT group methods,
    /// verifying correct SQL clause composition and conditional logic.
    /// </summary>
    public partial class SqlWhereBuilderTests
    {
        /// <summary>
        /// Verifies that a single condition passed to OrAndGroup is correctly grouped in the generated SQL statement.
        /// </summary>
        /// <remarks>This test ensures that when only one condition is provided to the OrAndGroup method,
        /// the resulting SQL includes the condition within an OR group as expected. Use this test to validate correct
        /// SQL grouping behavior for single-condition scenarios.</remarks>
        [Fact]
        public void OrAndGroup_SingleCondition_GeneratesGroupedSql()
        {
            var sql = BuildWhere(w => w.Where("1 = 1").OrAndGroup("A = 1"));
            Assert.Equal("WHERE 1 = 1 OR (A = 1)", sql);
        }

        /// <summary>
        /// Verifies that the OrAndGroup method generates a grouped SQL WHERE clause using AND for multiple conditions.
        /// </summary>
        /// <remarks>This test ensures that when multiple conditions are provided to OrAndGroup, the
        /// resulting SQL groups the conditions with AND inside an OR clause. This helps confirm correct logical
        /// grouping in generated SQL statements.</remarks>
        [Fact]
        public void OrAndGroup_MultipleConditions_GeneratesGroupedSqlWithAnd()
        {
            var sql = BuildWhere(w => w.Where("1 = 1").OrAndGroup("A = 1", "B = 2"));
            Assert.Equal("WHERE 1 = 1 OR (A = 1 AND B = 2)", sql);
        }

        /// <summary>
        /// Verifies that a single condition passed to OrAndNotGroup generates the expected grouped SQL with a NOT
        /// operator.
        /// </summary>
        /// <remarks>This test ensures that when OrAndNotGroup is called with a single condition, the
        /// resulting SQL is correctly grouped and prefixed with NOT. It helps validate the correct behavior of SQL
        /// generation logic for grouped NOT conditions.</remarks>
        [Fact]
        public void OrAndNotGroup_SingleCondition_GeneratesGroupedSqlWithNot()
        {
            var sql = BuildWhere(w => w.Where("1 = 1").OrAndNotGroup("A = 1"));
            Assert.Equal("WHERE 1 = 1 OR NOT (A = 1)", sql);
        }

        /// <summary>
        /// Verifies that the OrAndNotGroup method generates the correct SQL grouping with multiple conditions using NOT
        /// and AND operators.
        /// </summary>
        /// <remarks>This test ensures that when multiple conditions are provided to OrAndNotGroup, the
        /// resulting SQL groups the conditions with AND inside a OR NOT clause. This helps validate the correct logical
        /// grouping in generated SQL statements.</remarks>
        [Fact]
        public void OrAndNotGroup_MultipleConditions_GeneratesGroupedSqlWithNotAnd()
        {
            var sql = BuildWhere(w => w.Where("1 = 1").OrAndNotGroup("A = 1", "B = 2"));
            Assert.Equal("WHERE 1 = 1 OR NOT (A = 1 AND B = 2)", sql);
        }

        /// <summary>
        /// Verifies that the OrAndGroupIf method conditionally generates a SQL WHERE clause based on the specified
        /// predicate.
        /// </summary>
        /// <param name="predicate">A value indicating whether the SQL condition should be included. If <see langword="true"/>, the condition is
        /// added; otherwise, it is omitted.</param>
        /// <param name="expected">The expected SQL string result after applying the OrAndGroupIf method.</param>
        [Theory]
        [InlineData(true, "WHERE 1 = 1 OR (A = 1)")]
        [InlineData(false, "WHERE 1 = 1")]
        public void OrAndGroupIf_Predicate_GeneratesSqlConditionally(bool predicate, string expected)
        {
            var sql = BuildWhere(w => w.Where("1 = 1").OrAndGroupIf(predicate, "A = 1"));
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that the SQL WHERE clause is generated conditionally based on the provided array of predicate
        /// values.
        /// </summary>
        /// <param name="predicates">An array of Boolean values indicating which predicates should be included in the SQL condition. If null or
        /// contains any false values, the condition is not generated.</param>
        /// <param name="expected">The expected SQL WHERE clause output for the given predicates.</param>
        [Theory]
        [InlineData(new[] { true, true }, "WHERE 1 = 1 OR (A = 1)")]
        [InlineData(new[] { true, false }, "WHERE 1 = 1")]
        [InlineData(null, "WHERE 1 = 1")]
        public void OrAndGroupIf_ArrayPredicate_GeneratesSqlConditionally(bool[] predicates, string expected)
        {
            var sql = BuildWhere(w => w.Where("1 = 1").OrAndGroupIf(predicates, "A = 1"));
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that the SQL WHERE clause is conditionally generated based on the specified predicate when using
        /// OrAndNotGroupIf.
        /// </summary>
        /// <param name="predicate">A value indicating whether the OR NOT group condition should be included in the generated SQL.</param>
        /// <param name="expected">The expected SQL WHERE clause output for the given predicate value.</param>
        [Theory]
        [InlineData(true, "WHERE 1 = 1 OR NOT (A = 1)")]
        [InlineData(false, "WHERE 1 = 1")]
        public void OrAndNotGroupIf_Predicate_GeneratesSqlConditionally(bool predicate, string expected)
        {
            var sql = BuildWhere(w => w.Where("1 = 1").OrAndNotGroupIf(predicate, "A = 1"));
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that the OrAndNotGroupIf method generates the expected SQL WHERE clause based on the provided
        /// predicate array.
        /// </summary>
        /// <param name="predicates">An array of Boolean values that determine whether the SQL condition should be included. If null or contains
        /// false, the condition is not included.</param>
        /// <param name="expected">The expected SQL WHERE clause output for the given predicates.</param>
        [Theory]
        [InlineData(new[] { true, true }, "WHERE 1 = 1 OR NOT (A = 1)")]
        [InlineData(new[] { true, false }, "WHERE 1 = 1")]
        [InlineData(null, "WHERE 1 = 1")]
        public void OrAndNotGroupIf_ArrayPredicate_GeneratesSqlConditionally(bool[] predicates, string expected)
        {
            var sql = BuildWhere(w => w.Where("1 = 1").OrAndNotGroupIf(predicates, "A = 1"));
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that calling OrAndGroup with no conditions produces an empty group in the generated SQL WHERE
        /// clause.
        /// </summary>
        /// <remarks>This test ensures that the SQL builder correctly handles cases where OrAndGroup is
        /// invoked without any conditions, resulting in an empty group expression.</remarks>
        [Fact]
        public void OrAndGroup_EmptyConditions_ProducesEmptyGroup()
        {
            var sql = BuildWhere(w => w.Where("1 = 1").OrAndGroup());
            Assert.Equal("WHERE 1 = 1", sql);
        }

        /// <summary>
        /// Verifies that calling OrAndNotGroup with no conditions produces an empty group in the generated SQL WHERE
        /// clause.
        /// </summary>
        [Fact]
        public void OrAndNotGroup_EmptyConditions_ProducesEmptyGroup()
        {
            var sql = BuildWhere(w => w.Where("1 = 1").OrAndNotGroup());
            Assert.Equal("WHERE 1 = 1", sql);
        }
    }
}