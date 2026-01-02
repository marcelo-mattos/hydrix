using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Builders
{
    /// <summary>
    /// Contains unit tests for the SqlWhereBuilder AND/AND NOT group methods,
    /// verifying correct SQL clause composition and conditional logic.
    /// </summary>
    public partial class SqlWhereBuilderTests
    {
        /// <summary>
        /// Verifies that a single condition passed to AndOrGroup is correctly grouped in the generated SQL statement.
        /// </summary>
        [Fact]
        public void AndOrGroup_SingleCondition_GeneratesGroupedSql()
        {
            var sql = BuildWhere(w => w.Where("1 = 1").AndOrGroup("A = 1"));
            Assert.Equal("WHERE 1 = 1 AND (A = 1)", sql);
        }

        /// <summary>
        /// Verifies that the AndOrGroup method generates a grouped SQL WHERE clause using OR for multiple conditions.
        /// </summary>
        [Fact]
        public void AndOrGroup_MultipleConditions_GeneratesGroupedSqlWithOr()
        {
            var sql = BuildWhere(w => w.Where("1 = 1").AndOrGroup("A = 1", "B = 2"));
            Assert.Equal("WHERE 1 = 1 AND (A = 1 OR B = 2)", sql);
        }

        /// <summary>
        /// Verifies that a single condition passed to AndOrNotGroup generates the expected grouped SQL with a NOT operator.
        /// </summary>
        [Fact]
        public void AndOrNotGroup_SingleCondition_GeneratesGroupedSqlWithNot()
        {
            var sql = BuildWhere(w => w.Where("1 = 1").AndOrNotGroup("A = 1"));
            Assert.Equal("WHERE 1 = 1 AND NOT (A = 1)", sql);
        }

        /// <summary>
        /// Verifies that the AndOrNotGroup method generates the correct SQL grouping with multiple conditions using NOT and OR operators.
        /// </summary>
        [Fact]
        public void AndOrNotGroup_MultipleConditions_GeneratesGroupedSqlWithNotOr()
        {
            var sql = BuildWhere(w => w.Where("1 = 1").AndOrNotGroup("A = 1", "B = 2"));
            Assert.Equal("WHERE 1 = 1 AND NOT (A = 1 OR B = 2)", sql);
        }

        /// <summary>
        /// Verifies that the AndOrGroupIf method conditionally generates a SQL WHERE clause based on the specified predicate.
        /// </summary>
        [Theory]
        [InlineData(true, "WHERE 1 = 1 AND (A = 1)")]
        [InlineData(false, "WHERE 1 = 1")]
        public void AndOrGroupIf_Predicate_GeneratesSqlConditionally(bool predicate, string expected)
        {
            var sql = BuildWhere(w => w.Where("1 = 1").AndOrGroupIf(predicate, "A = 1"));
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that the SQL WHERE clause is generated conditionally based on the provided array of predicate values.
        /// </summary>
        [Theory]
        [InlineData(new[] { true, true }, "WHERE 1 = 1 AND (A = 1)")]
        [InlineData(new[] { true, false }, "WHERE 1 = 1")]
        [InlineData(null, "WHERE 1 = 1")]
        public void AndOrGroupIf_ArrayPredicate_GeneratesSqlConditionally(bool[] predicates, string expected)
        {
            var sql = BuildWhere(w => w.Where("1 = 1").AndOrGroupIf(predicates, "A = 1"));
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that the SQL WHERE clause is conditionally generated based on the specified predicate when using AndOrNotGroupIf.
        /// </summary>
        [Theory]
        [InlineData(true, "WHERE 1 = 1 AND NOT (A = 1)")]
        [InlineData(false, "WHERE 1 = 1")]
        public void AndOrNotGroupIf_Predicate_GeneratesSqlConditionally(bool predicate, string expected)
        {
            var sql = BuildWhere(w => w.Where("1 = 1").AndOrNotGroupIf(predicate, "A = 1"));
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that the AndOrNotGroupIf method generates the expected SQL WHERE clause based on the provided predicate array.
        /// </summary>
        [Theory]
        [InlineData(new[] { true, true }, "WHERE 1 = 1 AND NOT (A = 1)")]
        [InlineData(new[] { true, false }, "WHERE 1 = 1")]
        [InlineData(null, "WHERE 1 = 1")]
        public void AndOrNotGroupIf_ArrayPredicate_GeneratesSqlConditionally(bool[] predicates, string expected)
        {
            var sql = BuildWhere(w => w.Where("1 = 1").AndOrNotGroupIf(predicates, "A = 1"));
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that calling AndOrGroup with no conditions produces an empty group in the generated SQL WHERE clause.
        /// </summary>
        [Fact]
        public void AndOrGroup_EmptyConditions_ProducesEmptyGroup()
        {
            var sql = BuildWhere(w => w.Where("1 = 1").AndOrGroup());
            Assert.Equal("WHERE 1 = 1", sql);
        }

        /// <summary>
        /// Verifies that calling AndOrNotGroup with no conditions produces an empty group in the generated SQL WHERE clause.
        /// </summary>
        [Fact]
        public void AndOrNotGroup_EmptyConditions_ProducesEmptyGroup()
        {
            var sql = BuildWhere(w => w.Where("1 = 1").AndOrNotGroup());
            Assert.Equal("WHERE 1 = 1", sql);
        }
    }
}