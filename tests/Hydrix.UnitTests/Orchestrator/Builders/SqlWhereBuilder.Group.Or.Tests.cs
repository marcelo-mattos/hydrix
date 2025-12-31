using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Builders
{
    /// <summary>
    /// Contains unit tests for the SqlWhereBuilder grouped OR/OR NOT methods,
    /// verifying correct SQL clause composition and conditional logic.
    /// </summary>
    public partial class SqlWhereBuilderTests
    {
        /// <summary>
        /// Verifies that using a single condition within an OR group generates the correct grouped SQL WHERE clause.
        /// </summary>
        [Fact]
        public void OrGroup_SingleCondition_GeneratesGroupedSql()
        {
            var sql = BuildWhere(w => w.OrGroup(g => g.Or("A = 1")));
            Assert.Equal("WHERE (A = 1)", sql);
        }

        /// <summary>
        /// Verifies that grouping multiple conditions with 'OrGroup' generates a SQL WHERE clause with the conditions
        /// combined using the OR operator and enclosed in parentheses.
        /// </summary>
        [Fact]
        public void OrGroup_MultipleConditions_GeneratesGroupedSqlWithOr()
        {
            var sql = BuildWhere(w => w.OrGroup(g =>
            {
                g.Or("A = 1");
                g.Or("B = 2");
            }));
            Assert.Equal("WHERE (A = 1 OR B = 2)", sql);
        }

        /// <summary>
        /// Verifies that using OrNotGroup with a single condition generates the correct SQL with a grouped NOT clause.
        /// </summary>
        [Fact]
        public void OrNotGroup_SingleCondition_GeneratesGroupedSqlWithNot()
        {
            var sql = BuildWhere(w => w.OrNotGroup(g => g.Or("A = 1")));
            Assert.Equal("WHERE NOT (A = 1)", sql);
        }

        /// <summary>
        /// Verifies that the OrNotGroup method generates a SQL WHERE clause with multiple conditions grouped using NOT
        /// and OR operators.
        /// </summary>
        [Fact]
        public void OrNotGroup_MultipleConditions_GeneratesGroupedSqlWithNotOr()
        {
            var sql = BuildWhere(w => w.OrNotGroup(g =>
            {
                g.Or("A = 1");
                g.Or("B = 2");
            }));
            Assert.Equal("WHERE NOT (A = 1 OR B = 2)", sql);
        }

        /// <summary>
        /// Verifies that the OrGroupIf method conditionally generates a SQL WHERE clause based on the specified
        /// predicate.
        /// </summary>
        [Theory]
        [InlineData(true, "WHERE (A = 1)")]
        [InlineData(false, "")]
        public void OrGroupIf_Predicate_GeneratesSqlConditionally(bool predicate, string expected)
        {
            var sql = BuildWhere(w => w.OrGroupIf(predicate, g => g.Or("A = 1")));
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that the OrGroupIf method generates the expected SQL WHERE clause based on the provided predicate
        /// array.
        /// </summary>
        [Theory]
        [InlineData(new[] { true, true }, "WHERE (A = 1)")]
        [InlineData(new[] { true, false }, "")]
        [InlineData(null, "")]
        public void OrGroupIf_ArrayPredicate_GeneratesSqlConditionally(bool[] predicates, string expected)
        {
            var sql = BuildWhere(w => w.OrGroupIf(predicates, g => g.Or("A = 1")));
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that the OrNotGroupIf method conditionally generates a SQL WHERE clause with a NOT group based on
        /// the specified predicate.
        /// </summary>
        [Theory]
        [InlineData(true, "WHERE NOT (A = 1)")]
        [InlineData(false, "")]
        public void OrNotGroupIf_Predicate_GeneratesSqlConditionally(bool predicate, string expected)
        {
            var sql = BuildWhere(w => w.OrNotGroupIf(predicate, g => g.Or("A = 1")));
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that the OrNotGroupIf method generates the expected SQL condition based on the provided predicate
        /// array.
        /// </summary>
        [Theory]
        [InlineData(new[] { true, true }, "WHERE NOT (A = 1)")]
        [InlineData(new[] { true, false }, "")]
        [InlineData(null, "")]
        public void OrNotGroupIf_ArrayPredicate_GeneratesSqlConditionally(bool[] predicates, string expected)
        {
            var sql = BuildWhere(w => w.OrNotGroupIf(predicates, g => g.Or("A = 1")));
            Assert.Equal(expected, sql);
        }

        /// <summary>
        /// Verifies that nested OR groups in a WHERE clause are correctly translated to SQL syntax.
        /// </summary>
        [Fact]
        public void OrGroup_NestedGroups_GeneratesCorrectSql()
        {
            var sql = BuildWhere(w => w.OrGroup(g =>
            {
                g.Or("A = 1");
                g.OrGroup(h => h.Or("B = 2"));
            }));
            Assert.Equal("WHERE (A = 1 OR (B = 2))", sql);
        }

        /// <summary>
        /// Verifies that nested calls to OrNotGroup generate the correct SQL WHERE clause with proper logical grouping
        /// and negation.
        /// </summary>
        [Fact]
        public void OrNotGroup_NestedGroups_GeneratesCorrectSql()
        {
            var sql = BuildWhere(w => w.OrNotGroup(g =>
            {
                g.Or("A = 1");
                g.OrNotGroup(h => h.Or("B = 2"));
            }));
            Assert.Equal("WHERE NOT (A = 1 OR NOT (B = 2))", sql);
        }
    }
}