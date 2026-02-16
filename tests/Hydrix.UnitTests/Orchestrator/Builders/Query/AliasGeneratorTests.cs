using Hydrix.Orchestrator.Builders.Query;
using System;
using System.Collections.Concurrent;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Builders.Query
{
    /// <summary>
    /// Contains unit tests for the AliasGenerator class, validating the alias generation logic based on various input
    /// names and conditions.
    /// </summary>
    /// <remarks>These tests cover scenarios including expected alias generation, handling of null or
    /// whitespace names, and collision resolution for already used aliases. The tests ensure that the alias generation
    /// behaves correctly under different conditions, providing confidence in the functionality of the
    /// AliasGenerator.</remarks>
    public class AliasGeneratorTests
    {
        /// <summary>
        /// Verifies that the alias generated from the specified name matches the expected value when there are no
        /// naming collisions.
        /// </summary>
        /// <remarks>This test ensures that the alias generation logic produces consistent and correct
        /// results for various input names without considering collision scenarios. It covers different casing and
        /// naming conventions to validate the behavior.</remarks>
        /// <param name="name">The name from which to generate the alias. This should be a valid identifier that the alias will be derived
        /// from.</param>
        /// <param name="expected">The expected alias that should be returned for the given name. This serves as a reference for validating the
        /// alias generation.</param>
        [Theory]
        [InlineData("OrderDetail", "od")]
        [InlineData("Customer", "c")]
        [InlineData("SQLQuery", "sqlq")]
        [InlineData("A", "a")]
        [InlineData("Order", "o")]
        [InlineData("order", "o")]
        [InlineData("o", "o")]
        [InlineData("OrderID", "oid")]
        [InlineData("OrderId", "oi")]
        [InlineData("OrderIDNumber", "oidn")]
        [InlineData("SQL", "sql")]
        public void FromName_ReturnsExpectedAlias_WhenNoCollisions(string name, string expected)
        {
            var alias = AliasGenerator.FromName(name);
            Assert.Equal(expected, alias);
        }

        /// <summary>
        /// Verifies that the AliasGenerator.FromName method throws an ArgumentException when the provided name is null,
        /// empty, or consists only of whitespace characters.
        /// </summary>
        /// <remarks>This test ensures that AliasGenerator.FromName enforces input validation by rejecting
        /// invalid name values, helping to prevent improper alias generation and maintain consistent
        /// behavior.</remarks>
        [Fact]
        public void FromName_ThrowsArgumentException_WhenNameIsNullOrWhitespace()
        {
            Assert.Throws<ArgumentException>(() => AliasGenerator.FromName(null));
            Assert.Throws<ArgumentException>(() => AliasGenerator.FromName(""));
            Assert.Throws<ArgumentException>(() => AliasGenerator.FromName("   "));
        }

        /// <summary>
        /// Verifies that the FromName method appends a numeric suffix to the generated alias when the base alias is
        /// already present in the collection of used aliases.
        /// </summary>
        /// <remarks>This test ensures that AliasGenerator.FromName produces unique aliases by
        /// incrementing a number when collisions occur, maintaining uniqueness within the provided usedAliases
        /// dictionary.</remarks>
        [Fact]
        public void FromName_AppendsNumber_WhenAliasAlreadyUsed()
        {
            var usedAliases = new ConcurrentDictionary<string, byte>();
            usedAliases.TryAdd("od", 0); // Simulate "od" already used

            var alias = AliasGenerator.FromName("OrderDetail", usedAliases);
            Assert.Equal("od1", alias);

            // Add "od1" and test next collision
            usedAliases.TryAdd("od1", 0);
            var alias2 = AliasGenerator.FromName("OrderDetail", usedAliases);
            Assert.Equal("od2", alias2);
        }

        /// <summary>
        /// Verifies that the FromName method generates a base alias for the specified name when the alias is not
        /// already in use.
        /// </summary>
        /// <remarks>This test ensures that when a new name is provided to FromName and the corresponding
        /// alias is not present in the usedAliases dictionary, the method returns the expected base alias and updates
        /// the dictionary accordingly.</remarks>
        [Fact]
        public void FromName_UsesBaseAlias_WhenNotUsed()
        {
            var usedAliases = new ConcurrentDictionary<string, byte>();
            var alias = AliasGenerator.FromName("Customer", usedAliases);
            Assert.Equal("c", alias);
            Assert.True(usedAliases.ContainsKey("c"));
        }

        /// <summary>
        /// Verifies that the BuildBaseAlias method returns the first letter of the input string when no capital letters
        /// are present.
        /// </summary>
        /// <remarks>This test uses reflection to access the private BuildBaseAlias method of the
        /// AliasGenerator class. It ensures that, for an input string without capital letters, the method returns the
        /// expected single-character alias.</remarks>
        [Fact]
        public void BuildBaseAlias_ReturnsFirstLetter_WhenNoCapitals()
        {
            // Use reflection to access private method
            var method = typeof(AliasGenerator).GetMethod("BuildBaseAlias", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var result = (string)method.Invoke(null, new object[] { "order" });
            Assert.Equal("o", result);
        }

        /// <summary>
        /// Verifies that the BuildBaseAlias method returns the lowercase initials of the words in the input string when
        /// such initials are present.
        /// </summary>
        /// <remarks>This test ensures that the BuildBaseAlias method, when invoked with a PascalCase
        /// string such as "OrderDetail", produces the expected alias by extracting and lowercasing the initial letters
        /// of each word. The method is accessed via reflection as it is not publicly exposed.</remarks>
        [Fact]
        public void BuildBaseAlias_ReturnsLowercaseCapitals_WhenPresent()
        {
            var method = typeof(AliasGenerator).GetMethod("BuildBaseAlias", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var result = (string)method.Invoke(null, new object[] { "OrderDetail" });
            Assert.Equal("od", result);
        }
    }
}