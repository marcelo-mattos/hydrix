using Hydrix.Extensions;
using System;
using Xunit;

namespace Hydrix.UnitTests.Extensions
{
    /// <summary>
    /// Contains unit tests for the ObjectExtensions class, verifying the behavior of the As&lt;T&gt;() extension method under
    /// various input scenarios.
    /// </summary>
    /// <remarks>These tests ensure that the As&lt;T&gt;() method correctly handles null values, DBNull, convertible
    /// types, and non-convertible types, returning default values or throwing exceptions as appropriate. The tests help
    /// validate the robustness and correctness of type conversion logic implemented in ObjectExtensions.</remarks>
    public class ObjectExtensionsTests
    {
        /// <summary>
        /// Verifies that the As&lt;T&gt; extension method returns the default value of the specified type when the input
        /// object is null.
        /// </summary>
        /// <remarks>This test ensures that converting a null object using As&lt;T&gt; yields the default value
        /// for value types and null for reference types, confirming safe handling of null inputs.</remarks>
        [Fact]
        public void As_ReturnsDefault_WhenValueIsNull()
        {
            object value = null;
            Assert.Equal(0, value.As<int>());
            Assert.Null(value.As<string>());
        }

        /// <summary>
        /// Verifies that the As&lt;T&gt; extension method returns the default value for the target type when the input is
        /// DBNull.
        /// </summary>
        /// <remarks>This test ensures that As&lt;T&gt; safely handles database values that are DBNull by
        /// returning default(T) for value types and null for reference types, preventing exceptions during
        /// conversion.</remarks>
        [Fact]
        public void As_ReturnsDefault_WhenValueIsDBNull()
        {
            object value = DBNull.Value;
            Assert.Equal(0, value.As<int>());
            Assert.Null(value.As<string>());
        }

        /// <summary>
        /// Verifies that the As&lt;T&gt; extension method correctly converts an object to the specified target type when the
        /// conversion is valid.
        /// </summary>
        /// <remarks>This test ensures that convertible values are accurately cast to the desired type
        /// using the As&lt;T&gt; method. It covers scenarios where the input object can be converted to an integer or a
        /// double, validating the expected outcomes.</remarks>
        [Fact]
        public void As_ConvertsValue_WhenConvertible()
        {
            object value = "123";
            Assert.Equal(123, value.As<int>());

            object doubleValue = 42;
            Assert.Equal(42.0, doubleValue.As<double>());
        }

        /// <summary>
        /// Verifies that a FormatException is thrown when attempting to convert a non-convertible object to an integer
        /// using the As&lt;T&gt; extension method.
        /// </summary>
        /// <remarks>This test ensures that the As&lt;T&gt; method correctly raises a FormatException when the
        /// input value is a string that cannot be parsed as an integer. It validates the method's behavior for invalid
        /// conversions and helps ensure robust error handling.</remarks>
        [Fact]
        public void As_ThrowsException_WhenNotConvertible()
        {
            object value = "abc";
            Assert.Throws<FormatException>(() => value.As<int>());
        }
    }
}