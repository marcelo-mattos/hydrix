using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for verifying the default values of constants in the SqlMaterializer class.
    /// </summary>
    /// <remarks>These tests ensure that the internal constants of the SqlMaterializer class remain consistent
    /// and unchanged. The tests use reflection to access and validate the values of private static fields. This class
    /// is intended for use with automated test frameworks such as xUnit.</remarks>
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the DefaultTimeout field has the expected value of 30 seconds.
        /// </summary>
        /// <remarks>This test ensures that the DefaultTimeout constant in the target type is set
        /// correctly. If the value changes, dependent components or tests may require updates.</remarks>
        [Fact]
        public void DefaultTimeout_HasExpectedValue()
        {
            var materializer = CreateInstance();
            var type = materializer.GetType();
            Assert.NotNull(type);

            var field = type.GetField("DefaultTimeout", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(field);

            var value = field.GetValue(null);
            Assert.Equal(30, value);
        }

        /// <summary>
        /// Verifies that the value of the DefaultParameterPrefix field is set to the expected parameter prefix.
        /// </summary>
        /// <remarks>This test ensures that the DefaultParameterPrefix field is initialized to "@", which
        /// is commonly used as a parameter prefix in SQL queries. The test will fail if the field is not present or
        /// does not have the expected value.</remarks>
        [Fact]
        public void DefaultParameterPrefix_HasExpectedValue()
        {
            var materializer = CreateInstance();
            var type = materializer.GetType();
            Assert.NotNull(type);

            var field = type.GetField("DefaultParameterPrefix", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(field);

            var value = field.GetValue(null);
            Assert.Equal("@", value);
        }
    }
}