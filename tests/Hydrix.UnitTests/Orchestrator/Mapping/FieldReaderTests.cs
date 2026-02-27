using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Metadata.Internals;
using Moq;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Mapping
{
    /// <summary>
    /// Contains unit tests for the FieldReader delegate, ensuring it correctly reads values from an IDataRecord.
    /// </summary>
    /// <remarks>This class utilizes the xUnit testing framework to validate the behavior of the FieldReader
    /// delegate. Each test method should be independent and focus on a specific aspect of the FieldReader's
    /// functionality.</remarks>
    public class FieldReaderTests
    {
        /// <summary>
        /// Specifies integer values for basic counting operations.
        /// </summary>
        /// <remarks>This enumeration defines two values, Zero and One, which correspond to the integers 0
        /// and 1. It can be used in scenarios where a limited set of integer constants is required, such as toggling
        /// states or representing binary options.</remarks>
        private enum EnumInt : int
        {
            /// <summary>
            /// Represents the constant value of zero.
            /// </summary>
            Zero = 0,

            /// <summary>
            /// Represents the constant value of one.
            /// </summary>
            One = 1
        }

        /// <summary>
        /// Defines byte-based constants for representing specific values in a type-safe manner.
        /// </summary>
        /// <remarks>This enumeration is useful when working with APIs or data structures that require
        /// explicit byte values, providing improved readability and maintainability over using raw numeric
        /// literals.</remarks>
        private enum EnumByte : byte
        {
            /// <summary>
            /// Represents the constant value of zero.
            /// </summary>
            Zero = 0,

            /// <summary>
            /// Represents the constant value of one.
            /// </summary>
            One = 1
        }

        /// <summary>
        /// Defines a set of named constants that represent specific long integer values.
        /// </summary>
        /// <remarks>This enumeration provides named constants for commonly used long integer values,
        /// enhancing code readability and maintainability.</remarks>
        private enum EnumLong : long
        {
            /// <summary>
            /// Represents the constant value of zero.
            /// </summary>
            Zero = 0,

            /// <summary>
            /// Represents the constant value of one.
            /// </summary>
            One = 1
        }

        /// <summary>
        /// Defines unsigned integer constants for use in scenarios where specific unsigned values are required.
        /// </summary>
        /// <remarks>This enumeration provides named constants for the unsigned integer values zero and
        /// one, enabling type-safe usage in code that requires explicit unsigned values.</remarks>
        private enum EnumUint : uint
        {
            /// <summary>
            /// Represents the constant value of zero.
            /// </summary>
            Zero = 0,

            /// <summary>
            /// Represents the constant value of one.
            /// </summary>
            One = 1
        }

        /// <summary>
        /// Verifies that the FieldReader delegate correctly reads a value from an IDataRecord at the specified ordinal
        /// position.
        /// </summary>
        /// <remarks>This test ensures that the FieldReader delegate retrieves the expected value from an
        /// IDataRecord when provided with a specific ordinal index. It demonstrates the delegate's ability to access
        /// data flexibly based on the ordinal parameter.</remarks>
        [Fact]
        public void FieldReader_Delegate_ReadsValueFromIDataRecord()
        {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            mockRecord.Setup(r => r.GetValue(2)).Returns("expected");

            FieldReader reader = (record, ordinal) => record.GetValue(ordinal);

            // Act
            var result = reader(mockRecord.Object, 2);

            // Assert
            Assert.Equal("expected", result);
        }

        /// <summary>
        /// Verifies that the method correctly retrieves an enumeration value from a data record when the underlying
        /// type is supported.
        /// </summary>
        /// <remarks>This test ensures that when a data record contains a valid integer value
        /// corresponding to an enumeration member, the field reader returns the expected enum value. It uses a mock
        /// implementation of IDataRecord to simulate data retrieval.</remarks>
        [Fact]
        public void EnumWithSupportedUnderlyingType_ReturnsEnum()
        {
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            mock.Setup(r => r.GetInt32(0)).Returns(1);

            var reader = FieldReaderFactory.Create(typeof(EnumInt));
            var result = reader(mock.Object, 0);
            Assert.Equal(EnumInt.One, result);
        }

        /// <summary>
        /// Verifies that reading an enum with an unsupported underlying type from a data record returns the expected
        /// enum value.
        /// </summary>
        /// <remarks>This test ensures that the field reader correctly interprets enums whose underlying
        /// types are not directly supported by the data record, confirming that the appropriate enum value is returned
        /// even when the underlying type is not an integer.</remarks>
        [Fact]
        public void EnumWithUnsupportedUnderlyingType_ReturnsEnum()
        {
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(true);
            mock.Setup(r => r.GetInt32(0)).Returns(1);

            var reader = FieldReaderFactory.Create(typeof(EnumUint));
            var result = reader(mock.Object, 0);
            Assert.Equal(EnumUint.Zero, result);
        }

        /// <summary>
        /// Verifies that when a database field containing an enum with a supported underlying type is DBNull, the
        /// FieldReader returns the default value of the enum.
        /// </summary>
        /// <remarks>This test ensures that the FieldReader correctly handles DBNull values by returning
        /// the default enum value, which is important for scenarios where database records may contain nulls for enum
        /// fields.</remarks>
        [Fact]
        public void EnumWithSupportedUnderlyingType_DBNull_ReturnsDefault()
        {
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(true);

            var reader = FieldReaderFactory.Create(typeof(EnumInt));
            var result = reader(mock.Object, 0);
            Assert.Equal(EnumInt.Zero, result); // default(EnumInt)
        }

        /// <summary>
        /// Verifies that the field reader returns null when the underlying database value is DBNull for a nullable enum
        /// with a supported underlying type.
        /// </summary>
        /// <remarks>This test ensures that when a database field contains a DBNull value, the field
        /// reader correctly returns null for a nullable enum type, rather than an instance of the enum or throwing an
        /// exception. This behavior is important for handling optional enum fields in data access scenarios.</remarks>
        [Fact]
        public void NullableEnumWithSupportedUnderlyingType_DBNull_ReturnsNull()
        {
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(true);

            var reader = FieldReaderFactory.Create(typeof(EnumInt?));
            var result = reader(mock.Object, 0);
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that the field reader correctly retrieves an enum value when the enum has an unsupported underlying
        /// type by falling back to the IDataRecord.GetValue method.
        /// </summary>
        /// <remarks>This test ensures that enums with underlying types not directly supported by the
        /// field reader are still handled correctly by using the general value retrieval mechanism. This helps maintain
        /// compatibility with enums defined with uncommon underlying types.</remarks>
        [Fact]
        public void EnumWithUnsupportedUnderlyingType_FallbacksToGetValue()
        {
            var buffer = new byte[] { 0x01 };
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            mock.Setup(r => r.GetValue(0)).Returns(buffer);

            var reader = FieldReaderFactory.Create(typeof(byte[]));
            var result = reader(mock.Object, 0);
            Assert.Equal(buffer, result);
        }

        /// <summary>
        /// Verifies that reading a nullable enum with an unsupported underlying type returns null when the database
        /// value is DBNull.
        /// </summary>
        /// <remarks>This test ensures that the field reader correctly handles DBNull values for nullable
        /// enums with unsupported underlying types by returning null. This behavior is important for maintaining data
        /// integrity when mapping database values to .NET types.</remarks>
        [Fact]
        public void NullableEnumWithUnsupportedUnderlyingType_DBNull_ReturnsNull()
        {
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(true);

            var reader = FieldReaderFactory.Create(typeof(EnumLong?));
            var result = reader(mock.Object, 0);
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that the field reader returns the value from the data record when the type is not supported by the
        /// base readers and the value is not null.
        /// </summary>
        /// <remarks>This test ensures that the field reader correctly retrieves and returns a non-null
        /// value for types not handled by the default base readers, rather than throwing an exception or returning an
        /// incorrect result.</remarks>
        [Fact]
        public void TypeNotInBaseReaders_ReturnsGetValueOrNull()
        {
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            var obj = new object();
            mock.Setup(r => r.GetValue(0)).Returns(obj);

            var reader = FieldReaderFactory.Create(typeof(object));
            var result = reader(mock.Object, 0);
            Assert.Same(obj, result);
        }

        /// <summary>
        /// Verifies that the field reader returns null when the input value is DBNull and the type is not handled by
        /// the base readers.
        /// </summary>
        /// <remarks>This test ensures that the FieldReaderFactory correctly handles database fields
        /// containing DBNull values for types that do not have a specific base reader implementation. Returning null in
        /// these cases is important for consistent handling of database nulls in data access scenarios.</remarks>
        [Fact]
        public void TypeNotInBaseReaders_DBNull_ReturnsNull()
        {
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(true);

            var reader = FieldReaderFactory.Create(typeof(object));
            var result = reader(mock.Object, 0);
            Assert.Null(result);
        }
    }
}