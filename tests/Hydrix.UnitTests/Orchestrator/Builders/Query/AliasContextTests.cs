using Hydrix.Orchestrator.Builders.Query;
using System;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Builders.Query
{
    /// <summary>
    /// Provides unit tests for the AliasContext class, ensuring correct alias generation behavior.
    /// </summary>
    /// <remarks>These tests validate that the GetAlias method consistently returns the same alias for
    /// repeated calls with the same table name, generates unique aliases for different table names, appends a numeric
    /// index to the base alias when necessary, handles case sensitivity, and enforces input validation by throwing
    /// exceptions for null or empty table names.</remarks>
    public class AliasContextTests
    {
        /// <summary>
        /// Verifies that the GetAlias method returns the same alias for repeated calls with the same table name.
        /// </summary>
        /// <remarks>This test ensures that alias generation is consistent and that the same table name
        /// always yields the same alias, which is important for scenarios requiring stable table references.</remarks>
        [Fact]
        public void GetAlias_ReturnsSameAlias_ForSameTableName()
        {
            var ctx = new AliasContext();
            var alias1 = ctx.GetAlias("Customer");
            var alias2 = ctx.GetAlias("Customer");
            Assert.Equal(alias1, alias2);
        }

        /// <summary>
        /// Verifies that the GetAlias method returns different aliases for different table names.
        /// </summary>
        /// <remarks>This test ensures that the AliasContext generates unique aliases for each distinct
        /// table name, which is important for maintaining correct table references in scenarios such as query
        /// generation or mapping.</remarks>
        [Fact]
        public void GetAlias_ReturnsDifferentAliases_ForDifferentTableNames()
        {
            var ctx = new AliasContext();
            var alias1 = ctx.GetAlias("Customer");
            var alias2 = ctx.GetAlias("Order");
            Assert.NotEqual(alias1, alias2);
        }

        /// <summary>
        /// Verifies that the GetAlias method appends a numeric index to the base alias when the base alias is already
        /// in use, ensuring that each generated alias is unique.
        /// </summary>
        /// <remarks>This test simulates an alias collision by requesting aliases for two names that
        /// produce the same base alias. It asserts that the second alias is distinct from the first and follows the
        /// expected naming convention with a numeric suffix.</remarks>
        [Fact]
        public void GetAlias_AppendsIndex_WhenAliasAlreadyUsed()
        {
            var ctx = new AliasContext();
            var alias1 = ctx.GetAlias("User");
            var alias2 = ctx.GetAlias("user");
            Assert.NotEqual(alias1, alias2);
            Assert.StartsWith(alias1, alias2);
            Assert.Matches(@"[a-zA-Z]+[0-9]+", alias2);
        }

        /// <summary>
        /// Verifies that the GetAlias method generates distinct aliases for table names that differ only by case,
        /// ensuring case sensitivity in alias assignment.
        /// </summary>
        /// <remarks>This test confirms that the aliasing mechanism treats table names with different
        /// casing as unique, preventing collisions and ensuring correct behavior in scenarios where case-sensitive
        /// identifiers are required.</remarks>
        [Fact]
        public void GetAlias_HandlesMultipleCollisions()
        {
            var ctx = new AliasContext();
            var a1 = ctx.GetAlias("Table");
            var a2 = ctx.GetAlias("table");
            var a3 = ctx.GetAlias("TABLE");
            Assert.NotEqual(a1, a2);
            Assert.NotEqual(a1, a3);
            Assert.NotEqual(a2, a3);
        }

        /// <summary>
        /// Verifies that the GetAlias method of AliasContext throws an ArgumentNullException when the table name is
        /// null, empty, or consists only of whitespace.
        /// </summary>
        /// <remarks>This test ensures that the GetAlias method enforces input validation by rejecting
        /// invalid table name arguments. It helps maintain the integrity of the API by confirming that improper usage
        /// results in the expected exception.</remarks>
        [Fact]
        public void GetAlias_Throws_OnNullOrEmptyTableName()
        {
            var ctx = new AliasContext();
            Assert.Throws<ArgumentNullException>(() => ctx.GetAlias(null));
            Assert.Throws<ArgumentNullException>(() => ctx.GetAlias(""));
            Assert.Throws<ArgumentNullException>(() => ctx.GetAlias(" "));
        }
    }
}
