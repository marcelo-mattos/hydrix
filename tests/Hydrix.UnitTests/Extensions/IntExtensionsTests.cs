using Hydrix.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Extensions
{
    /// <summary>
    /// Contains unit tests for <see cref="IntExtensions"/>.
    /// </summary>
    /// <remarks>These tests validate that <see cref="IntExtensions.IsStandardDbType(int)"/> returns
    /// <see langword="true"/> for every defined <see cref="DbType"/> member and <see langword="false"/>
    /// for values outside the standard enum range.</remarks>
    public class IntExtensionsTests
    {
        /// <summary>
        /// Gets all standard <see cref="DbType"/> integer values.
        /// </summary>
        /// <returns>A sequence containing each defined <see cref="DbType"/> value as theory data.</returns>
        public static IEnumerable<object[]> StandardDbTypeValues()
        {
            foreach (DbType dbType in Enum.GetValues(typeof(DbType)))
                yield return new object[] { (int)dbType };
        }

        /// <summary>
        /// Gets invalid integer values that are not mapped to standard <see cref="DbType"/> members.
        /// </summary>
        /// <returns>A sequence containing out-of-range and arbitrary invalid values.</returns>
        public static IEnumerable<object[]> InvalidDbTypeValues()
        {
            var values = new[]
            {
                -1,
                int.MinValue,
                int.MaxValue,
                777,
                9999
            };

            foreach (var value in values)
                yield return new object[] { value };
        }

        /// <summary>
        /// Verifies that each defined <see cref="DbType"/> value is recognized as a standard database type.
        /// </summary>
        /// <param name="dbTypeValue">The integer value that represents a defined <see cref="DbType"/> member.</param>
        [Theory]
        [MemberData(nameof(StandardDbTypeValues))]
        public void IsStandardDbType_ReturnsTrue_ForAllDefinedDbTypeValues(int dbTypeValue)
        {
            var result = dbTypeValue.IsStandardDbType();

            Assert.True(result);
        }

        /// <summary>
        /// Verifies that values not defined in <see cref="DbType"/> are not recognized as standard database types.
        /// </summary>
        /// <param name="dbTypeValue">The invalid integer value to validate.</param>
        [Theory]
        [MemberData(nameof(InvalidDbTypeValues))]
        public void IsStandardDbType_ReturnsFalse_ForUndefinedValues(int dbTypeValue)
        {
            var result = dbTypeValue.IsStandardDbType();

            Assert.False(result);
        }
    }
}