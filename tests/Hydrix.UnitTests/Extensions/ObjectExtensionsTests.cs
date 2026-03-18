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
        /// Represents an enum used to validate enum conversions in As&lt;T&gt; tests.
        /// </summary>
        private enum TestStatus
        {
            Unknown = 0,
            Active = 1,
            Inactive = 2
        }

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

        /// <summary>
        /// Verifies that As&lt;T&gt; correctly converts values to enum types from both string and numeric sources.
        /// </summary>
        [Fact]
        public void As_ConvertsEnum_FromStringAndNumeric()
        {
            object enumName = "active";
            object enumNumber = 2;

            Assert.Equal(TestStatus.Active, enumName.As<TestStatus>());
            Assert.Equal(TestStatus.Inactive, enumNumber.As<TestStatus>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; correctly converts Guid values from Guid and string inputs.
        /// </summary>
        [Fact]
        public void As_ConvertsGuid_FromGuidAndString()
        {
            var guid = Guid.NewGuid();
            object guidValue = guid;
            object guidText = guid.ToString();

            Assert.Equal(guid, guidValue.As<Guid>());
            Assert.Equal(guid, guidText.As<Guid>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts a valid Guid string to a nullable Guid target.
        /// </summary>
        [Fact]
        public void As_ConvertsGuidString_ToNullableGuid()
        {
            var guid = Guid.NewGuid();
            object guidText = guid.ToString();

            Guid? converted = guidText.As<Guid?>();

            Assert.True(converted.HasValue);
            Assert.Equal(guid, converted.Value);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts values to nullable target types.
        /// </summary>
        [Fact]
        public void As_ConvertsValue_ToNullableType()
        {
            object value = "123";
            int? converted = value.As<int?>();

            Assert.True(converted.HasValue);
            Assert.Equal(123, converted.Value);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; returns the original value when the input is already of the target type.
        /// </summary>
        [Fact]
        public void As_ReturnsSameValue_WhenAlreadyTargetType()
        {
            object value = 77;

            Assert.Equal(77, value.As<int>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; returns null for nullable value types when the input is null.
        /// </summary>
        [Fact]
        public void As_ReturnsNull_WhenNullableTargetAndValueIsNull()
        {
            object value = null;
            int? converted = value.As<int?>();

            Assert.Null(converted);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts Guid values to nullable Guid targets through the Guid specialized path.
        /// </summary>
        [Fact]
        public void As_ConvertsGuid_ToNullableGuid()
        {
            var guid = Guid.NewGuid();
            object value = guid;

            Guid? converted = value.As<Guid?>();

            Assert.True(converted.HasValue);
            Assert.Equal(guid, converted.Value);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; throws a format exception when converting an invalid Guid string.
        /// </summary>
        [Fact]
        public void As_ThrowsFormatException_WhenGuidStringIsInvalid()
        {
            object value = "invalid-guid";

            Assert.Throws<FormatException>(() => value.As<Guid>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; throws when target is Guid and source is neither Guid nor string.
        /// </summary>
        [Fact]
        public void As_ThrowsInvalidCastException_WhenGuidTargetAndSourceIsUnsupported()
        {
            object value = 10;

            Assert.Throws<InvalidCastException>(() => value.As<Guid>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; throws when target is nullable Guid and source is neither Guid nor string.
        /// </summary>
        [Fact]
        public void As_ThrowsInvalidCastException_WhenNullableGuidTargetAndSourceIsUnsupported()
        {
            object value = 10;

            Assert.Throws<InvalidCastException>(() => value.As<Guid?>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts numeric values to nullable enum targets.
        /// </summary>
        [Fact]
        public void As_ConvertsNumericValue_ToNullableEnum()
        {
            object value = 1;

            TestStatus? converted = value.As<TestStatus?>();

            Assert.True(converted.HasValue);
            Assert.Equal(TestStatus.Active, converted.Value);
        }
    }
}