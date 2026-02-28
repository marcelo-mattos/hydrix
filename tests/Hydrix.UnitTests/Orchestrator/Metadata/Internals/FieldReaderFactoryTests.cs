using Hydrix.Orchestrator.Metadata.Internals;
using Moq;
using System;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Metadata.Internals
{
    /// <summary>
    /// Provides unit tests for the FieldReaderFactory class, verifying its behavior when reading various data types
    /// from an IDataRecord.
    /// </summary>
    /// <remarks>This test class covers scenarios including base types, nullable types, enums, and custom
    /// types. Each test ensures that the FieldReaderFactory.Create method returns the expected value for the specified
    /// type and handles DBNull values appropriately.</remarks>
    public class FieldReaderFactoryTests
    {
        /// <summary>
        /// Specifies binary states with defined integer values of zero and one.
        /// </summary>
        /// <remarks>Use this enumeration to represent options, flags, or states that have two possible
        /// values, such as enabled/disabled or on/off. Using an enumeration improves code clarity and type safety
        /// compared to using raw integer or boolean values.</remarks>
        public enum MyEnum : int
        {
            /// <summary>
            /// Represents the constant value zero.
            /// </summary>
            Zero = 0,

            /// <summary>
            /// Represents the numeric value one.
            /// </summary>
            One = 1
        }

        /// <summary>
        /// Represents a byte-based enumeration with values for zero and one.
        /// </summary>
        /// <remarks>Use this enumeration to express binary states or options in a compact form. The
        /// underlying byte type enables efficient storage and interoperability with APIs that require byte
        /// values.</remarks>
        public enum MyByteEnum : byte
        {
            /// <summary>
            /// Represents the constant value zero.
            /// </summary>
            Zero = 0,

            /// <summary>
            /// Represents the numeric value one.
            /// </summary>
            One = 1
        }

        /// <summary>
        /// Defines a set of named constants with a long underlying type.
        /// </summary>
        /// <remarks>Use this enumeration to represent specific constant values where a long integer type
        /// is required. The long underlying type allows for a wider range of values than the default int type, which
        /// can be useful in scenarios requiring large numeric values or interoperability with APIs expecting long-based
        /// enums.</remarks>
        public enum EnumWithLongUnderlying : long
        {
            /// <summary>
            /// Represents the constant value zero.
            /// </summary>
            Zero = 0,

            /// <summary>
            /// Represents the numeric value one.
            /// </summary>
            One = 1
        }

        /// <summary>
        /// Represents a custom data type used for specific operations within the application.
        /// </summary>
        public class CustomType
        { }

        /// <summary>
        /// Verifies that the correct value is returned for various base types when reading from a data record.
        /// </summary>
        /// <remarks>This test uses a mock IDataRecord to simulate database field retrieval for multiple
        /// base types. It ensures that the appropriate method is called for each type and that the returned value
        /// matches the expected result.</remarks>
        /// <param name="type">The type of the value to retrieve from the data record. Determines which retrieval method is used.</param>
        /// <param name="value">The expected value to be returned for the specified type.</param>
        [Theory]
        [InlineData(typeof(int), 42)]
        [InlineData(typeof(long), 123L)]
        [InlineData(typeof(short), (short)7)]
        [InlineData(typeof(byte), (byte)2)]
        [InlineData(typeof(bool), true)]
        [InlineData(typeof(double), 2.34)]
        [InlineData(typeof(float), 3.45f)]
        [InlineData(typeof(string), "abc")]
        public void BaseTypes_ReturnsValue(Type type, object value)
        {
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(false);

            // Setup the correct method for each type
            if (type == typeof(int)) mock.Setup(r => r.GetInt32(0)).Returns((int)value);
            else if (type == typeof(long)) mock.Setup(r => r.GetInt64(0)).Returns((long)value);
            else if (type == typeof(short)) mock.Setup(r => r.GetInt16(0)).Returns((short)value);
            else if (type == typeof(byte)) mock.Setup(r => r.GetByte(0)).Returns((byte)value);
            else if (type == typeof(bool)) mock.Setup(r => r.GetBoolean(0)).Returns((bool)value);
            else if (type == typeof(double)) mock.Setup(r => r.GetDouble(0)).Returns((double)value);
            else if (type == typeof(float)) mock.Setup(r => r.GetFloat(0)).Returns((float)value);
            else if (type == typeof(string)) mock.Setup(r => r.GetString(0)).Returns((string)value);

            var reader = FieldReaderFactory.Create(type);
            var result = reader(mock.Object, 0);
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Verifies that the method correctly retrieves a decimal value from a data record.
        /// </summary>
        /// <remarks>This test ensures that when the data record is not null, the correct decimal value is
        /// returned. It uses a mock implementation of IDataRecord to simulate the behavior of a database
        /// record.</remarks>
        [Fact]
        public void BaseTypes_ReturnsValue_ForDecimal()
        {
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            mock.Setup(r => r.GetDecimal(0)).Returns(1.23m);

            var reader = FieldReaderFactory.Create(typeof(decimal));
            var result = reader(mock.Object, 0);
            Assert.Equal(1.23m, result);
        }

        /// <summary>
        /// Verifies that a field reader created for the Guid type correctly returns the expected Guid value from an
        /// IDataRecord.
        /// </summary>
        /// <remarks>This test ensures that the field reader produced by FieldReaderFactory for the Guid
        /// type retrieves the value at the specified index when the data record contains a non-null Guid. It uses a
        /// mocked IDataRecord to simulate the presence of a Guid value.</remarks>
        [Fact]
        public void GuidType_ReturnsValue()
        {
            var guid = Guid.NewGuid();
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            mock.Setup(r => r.GetGuid(0)).Returns(guid);

            var reader = FieldReaderFactory.Create(typeof(Guid));
            var result = reader(mock.Object, 0);
            Assert.Equal(guid, result);
        }

        /// <summary>
        /// Verifies that the FieldReader correctly retrieves a DateTime value from an IDataRecord.
        /// </summary>
        /// <remarks>This test ensures that when the IDataRecord indicates a non-null value at the
        /// specified index, the correct DateTime is returned by the FieldReader. It is essential for validating the
        /// behavior of the FieldReader when dealing with DateTime types.</remarks>
        [Fact]
        public void DateTimeType_ReturnsValue()
        {
            var dt = DateTime.Now;
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            mock.Setup(r => r.GetDateTime(0)).Returns(dt);

            var reader = FieldReaderFactory.Create(typeof(DateTime));
            var result = reader(mock.Object, 0);
            Assert.Equal(dt, result);
        }

        /// <summary>
        /// Verifies that reading a nullable integer from an IDataRecord returns the integer value when present, or null
        /// when the database value is DBNull.
        /// </summary>
        /// <remarks>This test ensures that the field reader created for nullable integer types correctly
        /// distinguishes between database nulls and actual integer values. It checks both scenarios: when the data
        /// record contains a value and when it contains a database null.</remarks>
        [Fact]
        public void NullableBaseType_ReturnsValueOrNull()
        {
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            mock.Setup(r => r.GetInt32(0)).Returns(99);

            var reader = FieldReaderFactory.Create(typeof(int?));
            var result = reader(mock.Object, 0);
            Assert.Equal(99, result);

            mock.Setup(r => r.IsDBNull(0)).Returns(true);
            result = reader(mock.Object, 0);
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that reading a non-nullable base type from a data record returns the default value when the data is
        /// DBNull.
        /// </summary>
        /// <remarks>This test ensures that the field reader created for a non-nullable base type, such as
        /// int, returns the type's default value when the data source contains DBNull at the specified index. This
        /// behavior prevents exceptions and provides a safe default for consumers expecting non-nullable
        /// types.</remarks>
        [Fact]
        public void NonNullableBaseType_ReturnsDefaultOnDBNull()
        {
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(true);

            var reader = FieldReaderFactory.Create(typeof(int));
            var result = reader(mock.Object, 0);
            Assert.Equal(0, result); // default(int)
        }

        /// <summary>
        /// Verifies that the field reader correctly returns the corresponding enum value when reading an integer from a
        /// data record.
        /// </summary>
        /// <remarks>This test ensures that when the data record contains an integer value matching a
        /// defined member of the enum type, the field reader returns the expected enum value. It uses a mock data
        /// record to simulate the database interaction.</remarks>
        [Fact]
        public void EnumType_ReturnsEnumValue()
        {
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            mock.Setup(r => r.GetInt32(0)).Returns(1);

            var reader = FieldReaderFactory.Create(typeof(MyEnum));
            var result = reader(mock.Object, 0);
            Assert.Equal(MyEnum.One, result);
        }

        /// <summary>
        /// Verifies that reading a nullable enum value from an IDataRecord returns the corresponding enum value when
        /// present, or null when the value is database null.
        /// </summary>
        /// <remarks>This test ensures that the field reader created for a nullable enum type correctly
        /// interprets both non-null and null database values. It checks that a valid integer value is mapped to the
        /// appropriate enum member, and that a database null results in a null return value.</remarks>
        [Fact]
        public void NullableEnumType_ReturnsEnumOrNull()
        {
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            mock.Setup(r => r.GetInt32(0)).Returns(1);

            var reader = FieldReaderFactory.Create(typeof(MyEnum?));
            var result = reader(mock.Object, 0);
            Assert.Equal(MyEnum.One, result);

            mock.Setup(r => r.IsDBNull(0)).Returns(true);
            result = reader(mock.Object, 0);
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that a field reader correctly converts a byte value from a data record into the corresponding
        /// MyByteEnum value.
        /// </summary>
        /// <remarks>This test ensures that when the underlying type of an enum is byte, the field reader
        /// produced by FieldReaderFactory maps the byte value from the data record to the appropriate enum value. It
        /// uses a mock data record to simulate reading a non-null byte value and asserts that the result matches the
        /// expected MyByteEnum member.</remarks>
        [Fact]
        public void EnumType_WithUnderlyingByte_ReturnsEnumValue()
        {
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            mock.Setup(r => r.GetByte(0)).Returns((byte)1);

            var reader = FieldReaderFactory.Create(typeof(MyByteEnum));
            var result = reader(mock.Object, 0);
            Assert.Equal(MyByteEnum.One, result);
        }

        /// <summary>
        /// Verifies that the field reader correctly falls back to using the GetValue method when reading enum values
        /// with an underlying type not supported by the base readers.
        /// </summary>
        /// <remarks>This test simulates a scenario where an enum has a long underlying type, which is not
        /// natively handled by the base readers. It ensures that the reader can still retrieve the correct enum value
        /// by falling back to a more general retrieval method.</remarks>
        [Fact]
        public void EnumType_WithUnderlyingNotInBaseReaders_FallbacksToGetValue()
        {
            // Simula um enum com underlying type não suportado (ex: long)
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            mock.Setup(r => r.GetValue(0)).Returns(1L);
            mock.Setup(r => r.GetInt64(0)).Returns(1);

            var enumType = typeof(EnumWithLongUnderlying);
            var reader = FieldReaderFactory.Create(enumType);
            var result = reader(mock.Object, 0);
            Assert.Equal(EnumWithLongUnderlying.One, result);
        }

        /// <summary>
        /// Verifies that retrieving a value of a custom type from an IDataRecord falls back to using GetValue when
        /// appropriate.
        /// </summary>
        /// <remarks>This test ensures that the field reader created for a custom type correctly returns
        /// the expected object instance from the data record, even when the type is not natively supported. It confirms
        /// that the fallback mechanism does not throw exceptions for non-null values.</remarks>
        [Fact]
        public void CustomType_FallbacksToGetValue()
        {
            var obj = new CustomType();
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            mock.Setup(r => r.GetValue(0)).Returns(obj);

            var reader = FieldReaderFactory.Create(typeof(CustomType));
            var result = reader(mock.Object, 0);
            Assert.Same(obj, result);
        }

        /// <summary>
        /// Verifies that the field reader for a custom type returns null when the data record contains a DBNull value
        /// at the specified index.
        /// </summary>
        /// <remarks>This test ensures that the field reader created by FieldReaderFactory correctly
        /// handles database records with DBNull values by returning null, which is the expected behavior when reading
        /// nullable fields from a data source.</remarks>
        [Fact]
        public void CustomType_ReturnsNullOnDBNull()
        {
            var mock = new Mock<IDataRecord>();
            mock.Setup(r => r.IsDBNull(0)).Returns(true);

            var reader = FieldReaderFactory.Create(typeof(CustomType));
            var result = reader(mock.Object, 0);
            Assert.Null(result);
        }
    }
}