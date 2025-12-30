using Hydrix.Orchestrator.Builders;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Builders
{
    /// <summary>
    /// Contains unit tests for the SqlWhereBuilder AND-related methods, verifying correct SQL clause composition.
    /// </summary>
    public partial class SqlWhereBuilderTests
    {
        /// <summary>
        /// Helper: Gets the built SQL WHERE clause string.
        /// Assumes SqlWhereBuilder has a Build() method returning the clause, or ToString() as fallback.
        /// </summary>
        private static string GetSql(SqlWhereBuilder builder)
        {
            var buildMethod = builder.GetType().GetMethod("Build");
            
            if (buildMethod != null)
                return (string)buildMethod.Invoke(builder, null);

            return builder.ToString();
        }

        /// <summary>
        /// Verifies that the first condition added to a SqlWhereBuilder using the Where method is not prefixed with the
        /// AND keyword.
        /// </summary>
        /// <remarks>This test ensures that when a single condition is added, the resulting SQL string
        /// contains only the condition itself, without any leading logical operators.</remarks>
        [Fact]
        public void Where_AddsFirstConditionWithoutAnd()
        {
            var builder = SqlWhereBuilder.Create();
            builder.Where("A = 1");
            var sql = GetSql(builder);
            Assert.Equal("WHERE A = 1", sql);
        }

        /// <summary>
        /// Verifies that the And method adds an AND condition to the SQL WHERE clause builder.
        /// </summary>
        /// <remarks>This unit test ensures that chaining the And method after Where results in the
        /// correct SQL syntax, combining conditions with the AND operator.</remarks>
        [Fact]
        public void And_AddsAndCondition()
        {
            var builder = SqlWhereBuilder.Create();
            builder.Where("A = 1").And("B = 2");
            var sql = GetSql(builder);
            Assert.Equal("WHERE A = 1 AND B = 2", sql);
        }

        /// <summary>
        /// Verifies that the AndNot method adds an AND NOT condition to the SQL WHERE clause as expected.
        /// </summary>
        /// <remarks>This test ensures that calling AndNot after Where appends the correct logical
        /// condition to the generated SQL statement.</remarks>
        [Fact]
        public void AndNot_AddsAndNotCondition()
        {
            var builder = SqlWhereBuilder.Create();
            builder.Where("A = 1").AndNot("B = 2");
            var sql = GetSql(builder);
            Assert.Equal("WHERE A = 1 AND NOT B = 2", sql);
        }

        /// <summary>
        /// Verifies that the AndIf method adds the specified condition to the SQL WHERE clause when the predicate is
        /// true.
        /// </summary>
        /// <remarks>This test ensures that calling AndIf with a true predicate appends the provided
        /// condition to the existing WHERE clause using the AND operator.</remarks>
        [Fact]
        public void AndIf_PredicateTrue_AddsCondition()
        {
            var builder = SqlWhereBuilder.Create();
            builder.Where("A = 1").AndIf(true, "B = 2");
            var sql = GetSql(builder);
            Assert.Equal("WHERE A = 1 AND B = 2", sql);
        }

        /// <summary>
        /// Verifies that the AndIf method does not add the specified condition when the predicate is false.
        /// </summary>
        /// <remarks>This test ensures that calling AndIf with a false predicate leaves the SQL WHERE
        /// clause unchanged, confirming that conditions are only added when the predicate evaluates to true.</remarks>
        [Fact]
        public void AndIf_PredicateFalse_DoesNotAddCondition()
        {
            var builder = SqlWhereBuilder.Create();
            builder.Where("A = 1").AndIf(false, "B = 2");
            var sql = GetSql(builder);
            Assert.Equal("WHERE A = 1", sql);
        }

        /// <summary>
        /// Verifies that the AndIf method adds the specified condition when all elements in the predicate array are
        /// true.
        /// </summary>
        /// <remarks>This test ensures that when an array of boolean predicates contains only <see
        /// langword="true"/> values, the AndIf method appends the additional condition to the SQL WHERE
        /// clause.</remarks>
        [Fact]
        public void AndIf_ArrayPredicate_AllTrue_AddsCondition()
        {
            var builder = SqlWhereBuilder.Create();
            builder.Where("A = 1").AndIf(new[] { true, true }, "B = 2");
            var sql = GetSql(builder);
            Assert.Equal("WHERE A = 1 AND B = 2", sql);
        }

        /// <summary>
        /// Verifies that the AndIf method does not add the specified condition when any value in the predicate array is
        /// false.
        /// </summary>
        /// <remarks>This test ensures that when an array of boolean predicates is provided to AndIf, and
        /// at least one value is false, the additional condition is not appended to the SQL WHERE clause.</remarks>
        [Fact]
        public void AndIf_ArrayPredicate_AnyFalse_DoesNotAddCondition()
        {
            var builder = SqlWhereBuilder.Create();
            builder.Where("A = 1").AndIf(new[] { true, false }, "B = 2");
            var sql = GetSql(builder);
            Assert.Equal("WHERE A = 1", sql);
        }

        /// <summary>
        /// Verifies that calling AndIf with a null array predicate does not add the specified condition to the SQL
        /// WHERE clause.
        /// </summary>
        /// <remarks>This test ensures that when a null boolean array is passed as the predicate to the
        /// AndIf method, the additional condition is not appended to the existing SQL WHERE clause. The resulting SQL
        /// should remain unchanged.</remarks>
        [Fact]
        public void AndIf_ArrayPredicate_Null_DoesNotAddCondition()
        {
            var builder = SqlWhereBuilder.Create();
            builder.Where("A = 1").AndIf((bool[])null, "B = 2");
            var sql = GetSql(builder);
            Assert.Equal("WHERE A = 1", sql);
        }

        /// <summary>
        /// Verifies that the AndNotIf method adds a NOT condition to the SQL WHERE clause when the predicate is true.
        /// </summary>
        /// <remarks>This test ensures that calling AndNotIf with a true predicate results in the
        /// specified condition being wrapped in a NOT clause and appended to the existing SQL WHERE statement. It
        /// validates the correct behavior of conditional negation in the query builder.</remarks>
        [Fact]
        public void AndNotIf_PredicateTrue_AddsNotCondition()
        {
            var builder = SqlWhereBuilder.Create();
            builder.Where("A = 1").AndNotIf(true, "B = 2");
            var sql = GetSql(builder);
            Assert.Equal("WHERE A = 1 AND NOT B = 2", sql);
        }

        /// <summary>
        /// Verifies that the AndNotIf method does not add the specified condition when the predicate is false.
        /// </summary>
        /// <remarks>This test ensures that calling AndNotIf with a false predicate leaves the SQL WHERE
        /// clause unchanged, confirming that conditions are only added when the predicate evaluates to true.</remarks>
        [Fact]
        public void AndNotIf_PredicateFalse_DoesNotAddCondition()
        {
            var builder = SqlWhereBuilder.Create();
            builder.Where("A = 1").AndNotIf(false, "B = 2");
            var sql = GetSql(builder);
            Assert.Equal("WHERE A = 1", sql);
        }

        /// <summary>
        /// Verifies that the AndNotIf method adds a NOT condition to the SQL WHERE clause when all elements in the
        /// predicate array are true.
        /// </summary>
        /// <remarks>This test ensures that when AndNotIf is called with an array of boolean values where
        /// all elements are true, the specified condition is correctly wrapped in a NOT clause and appended to the
        /// existing SQL WHERE statement.</remarks>
        [Fact]
        public void AndNotIf_ArrayPredicate_AllTrue_AddsNotCondition()
        {
            var builder = SqlWhereBuilder.Create();
            builder.Where("A = 1").AndNotIf(new[] { true, true }, "B = 2");
            var sql = GetSql(builder);
            Assert.Equal("WHERE A = 1 AND NOT B = 2", sql);
        }

        /// <summary>
        /// Verifies that the AndNotIf method does not add the specified condition when any element in the predicate
        /// array is false.
        /// </summary>
        /// <remarks>This test ensures that when AndNotIf is called with a boolean array containing at
        /// least one false value, the additional condition is not appended to the SQL WHERE clause.</remarks>
        [Fact]
        public void AndNotIf_ArrayPredicate_AnyFalse_DoesNotAddCondition()
        {
            var builder = SqlWhereBuilder.Create();
            builder.Where("A = 1").AndNotIf(new[] { true, false }, "B = 2");
            var sql = GetSql(builder);
            Assert.Equal("WHERE A = 1", sql);
        }

        /// <summary>
        /// Verifies that calling AndNotIf with a null array predicate does not add the specified condition to the SQL
        /// WHERE clause.
        /// </summary>
        /// <remarks>This test ensures that when a null array is passed as the predicate to AndNotIf, the
        /// method does not append the additional condition, and the resulting SQL remains unchanged.</remarks>
        [Fact]
        public void AndNotIf_ArrayPredicate_Null_DoesNotAddCondition()
        {
            var builder = SqlWhereBuilder.Create();
            builder.Where("A = 1").AndNotIf((bool[])null, "B = 2");
            var sql = GetSql(builder);
            Assert.Equal("WHERE A = 1", sql);
        }

        /// <summary>
        /// Verifies that the SqlWhereBuilder supports method chaining and produces the expected SQL WHERE clause.
        /// </summary>
        /// <remarks>This test ensures that chaining multiple conditions using Where, And, AndNot, AndIf,
        /// and AndNotIf methods results in the correct SQL syntax. It also checks that conditional methods only include
        /// clauses when the specified condition is met.</remarks>
        [Fact]
        public void MethodChaining_WorksAsExpected()
        {
            var builder = SqlWhereBuilder.Create();
            builder.Where("A = 1")
                   .And("B = 2")
                   .AndNot("C = 3")
                   .AndIf(true, "D = 4")
                   .AndNotIf(false, "E = 5");
            var sql = GetSql(builder);
            Assert.Equal("WHERE A = 1 AND B = 2 AND NOT C = 3 AND D = 4", sql);
        }
    }
}