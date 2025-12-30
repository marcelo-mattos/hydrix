using Hydrix.Orchestrator.Adapters;
using System;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Adapters
{
    /// <summary>
    /// Contains unit tests for the DataRowDataRecordAdapter class, verifying its behavior when adapting DataRow objects
    /// to IDataRecord interface methods.
    /// </summary>
    /// <remarks>These tests cover various scenarios including value retrieval by column name and ordinal,
    /// type-specific getters, null handling, and exception cases. The tests ensure that DataRowDataRecordAdapter
    /// correctly implements IDataRecord semantics and throws appropriate exceptions for unsupported operations or
    /// invalid arguments.</remarks>
    public class DataRowDataRecordAdapterTests
    {
        /// <summary>
        /// Creates a new DataTable instance populated with a single row containing test data of various data types.
        /// </summary>
        /// <remarks>The returned DataTable includes columns for Boolean, Byte, Char, Guid, Int16, Int32,
        /// Int64, Single, Double, String, Decimal, DateTime, and Object types. The last column contains a DBNull value
        /// to represent a null entry.</remarks>
        /// <returns>A DataTable with predefined columns for common .NET types and a single row of sample values.</returns>
        private DataTable CreateTestTable()
        {
            var table = new DataTable();
            table.Columns.Add("BoolCol", typeof(bool));
            table.Columns.Add("ByteCol", typeof(byte));
            table.Columns.Add("CharCol", typeof(char));
            table.Columns.Add("GuidCol", typeof(Guid));
            table.Columns.Add("ShortCol", typeof(short));
            table.Columns.Add("IntCol", typeof(int));
            table.Columns.Add("LongCol", typeof(long));
            table.Columns.Add("FloatCol", typeof(float));
            table.Columns.Add("DoubleCol", typeof(double));
            table.Columns.Add("StringCol", typeof(string));
            table.Columns.Add("DecimalCol", typeof(decimal));
            table.Columns.Add("DateTimeCol", typeof(DateTime));
            table.Columns.Add("NullCol", typeof(object));
            table.Rows.Add(
                true, (byte)42, 'A', Guid.NewGuid(), (short)7, 123, 456L, 1.23f, 4.56, "test", 7.89m, DateTime.Today, DBNull.Value
            );
            return table;
        }

        /// <summary>
        /// Verifies that the DataRowDataRecordAdapter constructor throws an ArgumentNullException when passed a null
        /// DataRow.
        /// </summary>
        [Fact]
        public void Constructor_NullRow_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DataRowDataRecordAdapter(null));
        }

        /// <summary>
        /// Verifies that the FieldCount property returns the correct number of columns for the data record adapter.
        /// </summary>
        /// <remarks>This test ensures that the FieldCount property of the DataRowDataRecordAdapter
        /// matches the number of columns in the underlying DataTable. Use this test to confirm that the adapter
        /// accurately reflects the schema of the data source.</remarks>
        [Fact]
        public void FieldCount_ReturnsColumnCount()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal(table.Columns.Count, adapter.FieldCount);
        }

        /// <summary>
        /// Verifies that the indexer of the DataRowDataRecordAdapter returns the expected value when accessed by
        /// ordinal position.
        /// </summary>
        /// <remarks>This test ensures that accessing the adapter by a zero-based column index retrieves
        /// the correct value from the underlying data row. It checks both boolean and string values to validate correct
        /// mapping of column ordinals to values.</remarks>
        [Fact]
        public void Indexer_ByOrdinal_ReturnsValue()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal(true, adapter[0]);
            Assert.Equal("test", adapter[9]);
        }

        /// <summary>
        /// Verifies that the indexer of the DataRowDataRecordAdapter returns the correct value when accessed by column
        /// name.
        /// </summary>
        /// <remarks>This test ensures that the adapter correctly retrieves values for both populated and
        /// null columns using the column name indexer.</remarks>
        [Fact]
        public void Indexer_ByName_ReturnsValue()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal(123, adapter["IntCol"]);
            Assert.Equal(DBNull.Value, adapter["NullCol"]);
        }

        /// <summary>
        /// Verifies that the GetName method returns the correct column name for a given column ordinal.
        /// </summary>
        /// <remarks>This test ensures that the DataRowDataRecordAdapter.GetName method returns the
        /// expected column names for specified column indexes in a test data table.</remarks>
        [Fact]
        public void GetName_ReturnsColumnName()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal("BoolCol", adapter.GetName(0));
            Assert.Equal("StringCol", adapter.GetName(9));
        }

        /// <summary>
        /// Verifies that the GetDataTypeName method returns the expected type name for specified column indexes.
        /// </summary>
        /// <remarks>This test checks that the DataRowDataRecordAdapter correctly maps column indexes to
        /// their corresponding data type names, ensuring accurate type information retrieval.</remarks>
        [Fact]
        public void GetDataTypeName_ReturnsTypeName()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal("Boolean", adapter.GetDataTypeName(0));
            Assert.Equal("String", adapter.GetDataTypeName(9));
        }

        /// <summary>
        /// Verifies that the GetFieldType method returns the correct Type for specified field indexes.
        /// </summary>
        /// <remarks>This test ensures that the DataRowDataRecordAdapter.GetFieldType method returns the
        /// expected .NET Type for given column indexes in a data row. It checks both boolean and string field types to
        /// validate correct type mapping.</remarks>
        [Fact]
        public void GetFieldType_ReturnsType()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal(typeof(bool), adapter.GetFieldType(0));
            Assert.Equal(typeof(string), adapter.GetFieldType(9));
        }

        /// <summary>
        /// Verifies that the GetValue method returns the expected value for a specified column index.
        /// </summary>
        [Fact]
        public void GetValue_ReturnsValue()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal(456L, adapter.GetValue(6));
        }

        /// <summary>
        /// Tests that the GetValues method copies all field values from the data record into the provided array.
        /// </summary>
        /// <remarks>This test verifies that GetValues returns the correct number of fields and that the
        /// values are copied as expected. It ensures that the method populates the array with the data from the record,
        /// matching the field count and expected values.</remarks>
        [Fact]
        public void GetValues_CopiesValues()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            var arr = new object[adapter.FieldCount];
            var count = adapter.GetValues(arr);
            Assert.Equal(adapter.FieldCount, count);
            Assert.Equal("test", arr[9]);
        }

        /// <summary>
        /// Verifies that calling GetValues with a null array throws an ArgumentNullException.
        /// </summary>
        /// <remarks>This unit test ensures that the GetValues method enforces its contract by validating
        /// input parameters. It is intended to confirm correct exception handling for invalid arguments.</remarks>
        [Fact]
        public void GetValues_NullArray_Throws()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Throws<ArgumentNullException>(() => adapter.GetValues(null));
        }

        /// <summary>
        /// Verifies that the GetOrdinal method returns the correct column ordinal when provided with a valid column
        /// name.
        /// </summary>
        /// <remarks>This test ensures that the adapter correctly maps a valid column name to its
        /// corresponding ordinal index. It is intended to validate the expected behavior of the GetOrdinal method for
        /// existing columns.</remarks>
        [Fact]
        public void GetOrdinal_ValidName_ReturnsOrdinal()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal(1, adapter.GetOrdinal("ByteCol"));
        }

        /// <summary>
        /// Verifies that calling GetOrdinal with an invalid column name throws an IndexOutOfRangeException.
        /// </summary>
        /// <remarks>This test ensures that the GetOrdinal method correctly handles cases where the
        /// specified column name does not exist in the data record, and throws the expected exception.</remarks>
        [Fact]
        public void GetOrdinal_InvalidName_Throws()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Throws<IndexOutOfRangeException>(() => adapter.GetOrdinal("NotACol"));
        }

        /// <summary>
        /// Verifies that the GetBoolean method returns the expected Boolean value for the specified column index.
        /// </summary>
        /// <remarks>This test ensures that the DataRowDataRecordAdapter correctly retrieves Boolean
        /// values from the underlying data row. It is intended to validate the adapter's behavior when accessing
        /// Boolean data.</remarks>
        [Fact]
        public void GetBoolean_ReturnsValue()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.True(adapter.GetBoolean(0));
        }

        /// <summary>
        /// Verifies that the GetByte method returns the expected byte value for the specified column index.
        /// </summary>
        [Fact]
        public void GetByte_ReturnsValue()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal((byte)42, adapter.GetByte(1));
        }

        /// <summary>
        /// Verifies that the GetChar method returns the expected character value from the data record.
        /// </summary>
        /// <remarks>This test ensures that GetChar retrieves the correct character from the specified
        /// column index in the data record adapter.</remarks>
        [Fact]
        public void GetChar_ReturnsValue()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal('A', adapter.GetChar(2));
        }

        /// <summary>
        /// Verifies that the GetGuid method returns the expected Guid value from the specified column in a
        /// DataRowDataRecordAdapter.
        /// </summary>
        /// <remarks>This test ensures that the GetGuid method correctly retrieves a Guid from the
        /// underlying data record when accessed by column index.</remarks>
        [Fact]
        public void GetGuid_ReturnsValue()
        {
            var table = CreateTestTable();
            var guid = (Guid)table.Rows[0][3];
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal(guid, adapter.GetGuid(3));
        }

        /// <summary>
        /// Verifies that the GetInt16 method returns the expected Int16 value from the specified column in the data
        /// record adapter.
        /// </summary>
        [Fact]
        public void GetInt16_ReturnsValue()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal((short)7, adapter.GetInt16(4));
        }

        /// <summary>
        /// Verifies that the GetInt32 method returns the expected integer value from the specified column in the data
        /// record adapter.
        /// </summary>
        [Fact]
        public void GetInt32_ReturnsValue()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal(123, adapter.GetInt32(5));
        }

        /// <summary>
        /// Verifies that the GetInt64 method returns the expected 64-bit integer value from the specified column.
        /// </summary>
        [Fact]
        public void GetInt64_ReturnsValue()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal(456L, adapter.GetInt64(6));
        }

        /// <summary>
        /// Verifies that the GetFloat method returns the expected float value for the specified column index.
        /// </summary>
        /// <remarks>This test ensures that the DataRowDataRecordAdapter correctly retrieves a float value
        /// from the underlying data row when accessed by column index.</remarks>
        [Fact]
        public void GetFloat_ReturnsValue()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal(1.23f, adapter.GetFloat(7));
        }

        /// <summary>
        /// Verifies that the GetDouble method returns the expected double value for the specified column index.
        /// </summary>
        /// <remarks>This test ensures that the DataRowDataRecordAdapter correctly retrieves a double
        /// value from the underlying data row when accessed by column index.</remarks>
        [Fact]
        public void GetDouble_ReturnsValue()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal(4.56, adapter.GetDouble(8));
        }

        /// <summary>
        /// Verifies that the GetString method returns the expected string value for the specified column index.
        /// </summary>
        /// <remarks>This test ensures that the DataRowDataRecordAdapter correctly retrieves a string
        /// value from the underlying data row when accessed by column index.</remarks>
        [Fact]
        public void GetString_ReturnsValue()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal("test", adapter.GetString(9));
        }

        /// <summary>
        /// Verifies that the GetDecimal method returns the expected decimal value from the data record adapter.
        /// </summary>
        [Fact]
        public void GetDecimal_ReturnsValue()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal(7.89m, adapter.GetDecimal(10));
        }

        /// <summary>
        /// Verifies that the GetDateTime method returns the expected DateTime value from the specified column in the
        /// data record adapter.
        /// </summary>
        [Fact]
        public void GetDateTime_ReturnsValue()
        {
            var table = CreateTestTable();
            var date = (DateTime)table.Rows[0][11];
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Equal(date, adapter.GetDateTime(11));
        }

        /// <summary>
        /// Verifies that the IsDBNull method returns <see langword="true"/> when the specified column contains a DBNull
        /// value.
        /// </summary>
        /// <remarks>This test ensures that the adapter correctly identifies database null values in the
        /// underlying data row. It is intended to validate the behavior of the IsDBNull method for columns containing
        /// DBNull.</remarks>
        [Fact]
        public void IsDBNull_ReturnsTrueForDBNull()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.True(adapter.IsDBNull(12));
        }

        /// <summary>
        /// Verifies that the IsDBNull method returns false when the specified column contains a non-null value.
        /// </summary>
        /// <remarks>This test ensures that IsDBNull correctly identifies columns with actual data as not
        /// containing database null values. It is intended to validate the expected behavior of the
        /// DataRowDataRecordAdapter when accessing non-null fields.</remarks>
        [Fact]
        public void IsDBNull_ReturnsFalseForValue()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.False(adapter.IsDBNull(0));
        }

        /// <summary>
        /// Verifies that calling GetBytes on a DataRowDataRecordAdapter instance throws a NotSupportedException.
        /// </summary>
        /// <remarks>This test ensures that the GetBytes method is not supported by the
        /// DataRowDataRecordAdapter and that it throws the expected exception when invoked.</remarks>
        [Fact]
        public void GetBytes_ThrowsNotSupported()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Throws<NotSupportedException>(() => adapter.GetBytes(0, 0, null, 0, 0));
        }

        /// <summary>
        /// Verifies that the GetChars method throws a NotSupportedException when called on a DataRowDataRecordAdapter
        /// instance.
        /// </summary>
        /// <remarks>This test ensures that the GetChars method is not supported by the
        /// DataRowDataRecordAdapter and that it correctly throws a NotSupportedException when invoked. This behavior is
        /// important for consumers to understand the limitations of the adapter.</remarks>
        [Fact]
        public void GetChars_ThrowsNotSupported()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Throws<NotSupportedException>(() => adapter.GetChars(0, 0, null, 0, 0));
        }

        /// <summary>
        /// Verifies that calling the GetData method on a DataRowDataRecordAdapter instance throws a
        /// NotSupportedException.
        /// </summary>
        /// <remarks>This test ensures that the GetData method is not supported by the
        /// DataRowDataRecordAdapter and that it correctly throws a NotSupportedException when invoked.</remarks>
        [Fact]
        public void GetData_ThrowsNotSupported()
        {
            var table = CreateTestTable();
            var adapter = new DataRowDataRecordAdapter(table.Rows[0]);
            Assert.Throws<NotSupportedException>(() => adapter.GetData(0));
        }
    }
}