using System;
using System.Collections;
using System.Data;

namespace Hydrix.Wrappers
{
    /// <summary>
    /// Wraps a provider-agnostic IDataReader and disposes the creating command when the reader completes.
    /// </summary>
    internal sealed class CommandOwningDataReader :
        IDataReader
    {
        /// <summary>
        /// Represents the database command associated with the current operation.
        /// </summary>
        private readonly IDbCommand _command;

        /// <summary>
        /// Provides access to the underlying data reader used for retrieving data from a data source.
        /// </summary>
        private readonly IDataReader _reader;

        /// <summary>
        /// Indicates whether the object has been disposed.
        /// </summary>
        /// <remarks>This field is typically used to prevent multiple disposal operations and to detect
        /// object usage after disposal. It should be set to <see langword="true"/> when the object's resources have
        /// been released.</remarks>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the CommandOwningDataReader class with the specified command and data reader.
        /// </summary>
        /// <remarks>The provided command and data reader are expected to be valid and open for use. This
        /// constructor does not take ownership of the underlying database connection.</remarks>
        /// <param name="command">The database command that is associated with the data reader. Cannot be null.</param>
        /// <param name="reader">The data reader to be managed by this instance. Cannot be null.</param>
        public CommandOwningDataReader(
            IDbCommand command,
            IDataReader reader)
        {
            _command = command;
            _reader = reader;
        }

        /// <summary>
        /// Gets the value of the specified column in its native format.
        /// </summary>
        /// <remarks>If the specified index is out of range, an exception may be thrown. Use this indexer
        /// to access column values by their ordinal position in the result set.</remarks>
        /// <param name="ordinal">The zero-based column ordinal whose value to retrieve.</param>
        /// <returns>The value of the column at the specified index. The type of the returned object depends on the underlying
        /// data source.</returns>
        public object this[int ordinal]
            => _reader[ordinal];

        /// <summary>
        /// Gets the value of the column with the specified name from the underlying data reader.
        /// </summary>
        /// <remarks>If the specified column name does not exist, an exception may be thrown by the
        /// underlying data reader.</remarks>
        /// <param name="name">The name of the column whose value to retrieve. The name is case-sensitive.</param>
        /// <returns>The value of the specified column. Returns DBNull if the column value is database null.</returns>
        public object this[string name]
            => _reader[name];

        /// <summary>
        /// Gets the current depth of the data reader in the data source hierarchy.
        /// </summary>
        /// <remarks>The depth indicates the level of nesting for the current row. A depth of zero
        /// indicates the outermost result set, while higher values represent nested result sets, such as those returned
        /// by executing batch queries or stored procedures that return multiple result sets.</remarks>
        public int Depth
            => _reader.Depth;

        /// <summary>
        /// Gets a value indicating whether the data reader is closed.
        /// </summary>
        public bool IsClosed
            => _reader.IsClosed;

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </summary>
        /// <remarks>The value is -1 for statements that do not affect rows or if no rows were affected.
        /// This property is typically used after executing an update, insert, or delete command to determine how many
        /// rows were impacted.</remarks>
        public int RecordsAffected
            => _reader.RecordsAffected;

        /// <summary>
        /// Gets the number of columns in the current row of the data reader.
        /// </summary>
        public int FieldCount
            => _reader.FieldCount;

        /// <summary>
        /// Closes the underlying data reader and releases associated resources.
        /// </summary>
        /// <remarks>This method should be called when data reading is complete to ensure that all
        /// resources are properly released. After calling this method, the reader cannot be used for further
        /// operations.</remarks>
        public void Close()
        {
            try
            {
                _reader.Close();
            }
            finally
            {
                DisposeCommandOnce();
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the class.
        /// </summary>
        /// <remarks>Call this method when you are finished using the object to free unmanaged resources
        /// held by the underlying reader and command. After calling this method, the object should not be
        /// used.</remarks>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                _reader.Dispose();
            }
            finally
            {
                _command.Dispose();
            }
        }

        /// <summary>
        /// Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column as a Boolean. Returns <see langword="true"/> if the value is true;
        /// otherwise, <see langword="false"/>.</returns>
        public bool GetBoolean(
            int ordinal)
            => _reader.GetBoolean(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a byte.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal of the value to retrieve.</param>
        /// <returns>The value of the specified column as a byte.</returns>
        public byte GetByte(
            int ordinal)
            => _reader.GetByte(ordinal);

        /// <summary>
        /// Reads a stream of bytes from the specified column offset into the provided buffer as an array, starting at
        /// the given buffer offset.
        /// </summary>
        /// <remarks>If the buffer parameter is null, this method returns the total length of the field in
        /// bytes rather than reading data. This method can be used to read large binary values in chunks.</remarks>
        /// <param name="ordinal">The zero-based column ordinal from which to read the bytes.</param>
        /// <param name="fieldOffset">The index within the field from which to begin reading bytes.</param>
        /// <param name="buffer">The buffer into which the bytes will be read. If null, the method returns the total number of bytes
        /// available in the field.</param>
        /// <param name="bufferoffset">The index within the buffer at which to start placing the data read from the field.</param>
        /// <param name="length">The maximum number of bytes to read from the field.</param>
        /// <returns>The actual number of bytes read into the buffer. Returns 0 if no bytes are available or if the end of the
        /// field is reached.</returns>
        public long GetBytes(
            int ordinal,
            long fieldOffset,
            byte[] buffer,
            int bufferoffset,
            int length)
            => _reader.GetBytes(
                ordinal,
                fieldOffset,
                buffer,
                bufferoffset,
                length);

        /// <summary>
        /// Gets the value of the specified column as a Unicode character.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>A Unicode character representing the value of the specified column.</returns>
        public char GetChar(
            int ordinal)
            => _reader.GetChar(ordinal);

        /// <summary>
        /// Reads a stream of characters from the specified column offset into the provided buffer as an array, starting
        /// at the given buffer offset.
        /// </summary>
        /// <remarks>If the buffer parameter is null, this method returns the total number of characters
        /// available in the field. This method can be used to retrieve large character data in chunks.</remarks>
        /// <param name="ordinal">The zero-based column ordinal from which to read the characters.</param>
        /// <param name="fieldoffset">The index within the field from which to start reading characters.</param>
        /// <param name="buffer">The buffer into which the characters are read. Can be null to obtain the length of the field.</param>
        /// <param name="bufferoffset">The index within the buffer at which to start placing the data.</param>
        /// <param name="length">The maximum number of characters to read from the field.</param>
        /// <returns>The actual number of characters read into the buffer. Returns the total length of the field in characters if
        /// the buffer is null.</returns>
        public long GetChars(
            int ordinal,
            long fieldoffset,
            char[] buffer,
            int bufferoffset,
            int length)
            => _reader.GetChars(
                ordinal,
                fieldoffset,
                buffer,
                bufferoffset,
                length);

        /// <summary>
        /// Gets an IDataReader to access the data at the specified column ordinal.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal indicating which column's data to retrieve.</param>
        /// <returns>An IDataReader for the specified column, allowing access to its data.</returns>
        public IDataReader GetData(
            int ordinal)
            => _reader.GetData(ordinal);

        /// <summary>
        /// Gets the data type information for the specified column ordinal as a string.
        /// </summary>
        /// <remarks>The returned data type name is provider-specific and may not correspond directly to a
        /// .NET type. Use this method to obtain the database type as defined by the underlying data source.</remarks>
        /// <param name="ordinal">The zero-based column ordinal for which to retrieve the data type name.</param>
        /// <returns>A string representing the data type of the specified column.</returns>
        public string GetDataTypeName(
            int ordinal)
            => _reader.GetDataTypeName(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a DateTime object.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal of the value to retrieve.</param>
        /// <returns>A DateTime value representing the data in the specified column.</returns>
        public DateTime GetDateTime(
            int ordinal)
            => _reader.GetDateTime(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a decimal number.
        /// </summary>
        /// <remarks>Use this method when the column value is expected to be a decimal. An exception is
        /// thrown if the value cannot be cast to decimal or if the column is null.</remarks>
        /// <param name="ordinal">The zero-based column ordinal of the value to retrieve.</param>
        /// <returns>The value of the specified column as a decimal.</returns>
        public decimal GetDecimal(
            int ordinal)
            => _reader.GetDecimal(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a double-precision floating-point number.
        /// </summary>
        /// <remarks>No conversions are performed; the data must already be a double-precision
        /// floating-point number or convertible to one. Use IsDBNull to check for null values before calling this
        /// method to avoid exceptions.</remarks>
        /// <param name="ordinal">The zero-based column ordinal of the value to retrieve.</param>
        /// <returns>The value of the specified column as a double-precision floating-point number.</returns>
        public double GetDouble(
            int ordinal)
            => _reader.GetDouble(ordinal);

        /// <summary>
        /// Gets the data type of the specified column.
        /// </summary>
        /// <remarks>Use this method to determine the .NET type that corresponds to the data stored in the
        /// column at the given ordinal. This can be useful for dynamic data processing or when working with columns of
        /// unknown types.</remarks>
        /// <param name="ordinal">The zero-based column ordinal for which to retrieve the data type.</param>
        /// <returns>A Type object that represents the data type of the specified column.</returns>
        public Type GetFieldType(
            int ordinal)
            => _reader.GetFieldType(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a single-precision floating-point number.
        /// </summary>
        /// <remarks>If the column value is not already a single-precision floating-point number, an
        /// attempt is made to convert it. Use IsDBNull to check for null values before calling this method to avoid
        /// exceptions.</remarks>
        /// <param name="ordinal">The zero-based column ordinal of the value to retrieve.</param>
        /// <returns>The value of the specified column as a single-precision floating-point number.</returns>
        public float GetFloat(
            int ordinal)
            => _reader.GetFloat(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a GUID.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal of the value to retrieve.</param>
        /// <returns>A GUID representing the value of the specified column.</returns>
        public Guid GetGuid(
            int ordinal)
            => _reader.GetGuid(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a 16-bit signed integer.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal of the value to retrieve.</param>
        /// <returns>The 16-bit signed integer value of the specified column.</returns>
        public short GetInt16(
            int ordinal)
            => _reader.GetInt16(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a 32-bit signed integer.
        /// </summary>
        /// <remarks>If the column value is equivalent to DBNull, an exception is thrown. Ensure that the
        /// column contains a valid 32-bit integer value before calling this method.</remarks>
        /// <param name="ordinal">The zero-based column ordinal indicating which column's value to retrieve.</param>
        /// <returns>The 32-bit signed integer value of the specified column.</returns>
        public int GetInt32(
            int ordinal)
            => _reader.GetInt32(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a 64-bit signed integer.
        /// </summary>
        /// <remarks>Use this method when you are certain that the column contains a 64-bit signed integer
        /// value; otherwise, an exception may be thrown if the value cannot be cast to Int64.</remarks>
        /// <param name="ordinal">The zero-based column ordinal of the value to retrieve.</param>
        /// <returns>The 64-bit signed integer value of the specified column.</returns>
        public long GetInt64(
            int ordinal)
            => _reader.GetInt64(ordinal);

        /// <summary>
        /// Gets the name of the column at the specified zero-based ordinal position.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal whose name is to be retrieved. Must be greater than or equal to 0 and less
        /// than the total number of columns.</param>
        /// <returns>The name of the column at the specified ordinal position.</returns>
        public string GetName(
            int ordinal)
            => _reader.GetName(ordinal);

        /// <summary>
        /// Returns the zero-based column ordinal given the name of the column.
        /// </summary>
        /// <remarks>If the specified column name does not exist, an exception is thrown. Column name
        /// lookups may be slower than ordinal lookups; for performance-critical code, consider caching the
        /// ordinal.</remarks>
        /// <param name="name">The name of the column to find the ordinal for. The comparison is typically case-insensitive, but this may
        /// depend on the underlying data source.</param>
        /// <returns>The zero-based ordinal of the column with the specified name.</returns>
        public int GetOrdinal(
            string name)
            => _reader.GetOrdinal(name);

        /// <summary>
        /// Returns a DataTable that describes the column metadata of the current result set.
        /// </summary>
        /// <remarks>The schema table provides detailed information about the columns in the result set,
        /// which can be used for dynamic data processing or schema discovery. The structure and content of the returned
        /// DataTable follow the standard schema table format used by ADO.NET.</remarks>
        /// <returns>A DataTable that contains metadata about each column in the current result set, such as column name, data
        /// type, and other schema information. Returns null if there is no current result set.</returns>
        public DataTable GetSchemaTable()
            => _reader.GetSchemaTable();

        /// <summary>
        /// Gets the value of the specified column as a string.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal indicating which column's value to retrieve.</param>
        /// <returns>The string value of the specified column.</returns>
        public string GetString(
            int ordinal)
            => _reader.GetString(ordinal);

        /// <summary>
        /// Gets the value of the specified column in its native format given the column ordinal.
        /// </summary>
        /// <remarks>Use this method to retrieve the value of a column without type conversion. To obtain
        /// a value of a specific type, use the appropriate typed accessor method.</remarks>
        /// <param name="ordinal">The zero-based column ordinal indicating which column's value to retrieve.</param>
        /// <returns>An object representing the value of the specified column. Returns DBNull if the column value is database
        /// null.</returns>
        public object GetValue(
            int ordinal)
            => _reader.GetValue(ordinal);

        /// <summary>
        /// Copies the column values of the current row into the provided array.
        /// </summary>
        /// <remarks>If the array is longer than the number of columns, the remaining elements are not
        /// modified. If the array is shorter, only as many values as the length of the array are copied.</remarks>
        /// <param name="values">An array of objects to receive the column values. The array must have a length equal to or greater than the
        /// number of columns in the current row.</param>
        /// <returns>The number of column values copied into the array.</returns>
        public int GetValues(
            object[] values)
            => _reader.GetValues(values);

        /// <summary>
        /// Determines whether the column at the specified ordinal position contains a database null value.
        /// </summary>
        /// <remarks>Use this method to check for database null values before retrieving data from the
        /// specified column to avoid runtime exceptions.</remarks>
        /// <param name="ordinal">The zero-based column ordinal to check for a database null value.</param>
        /// <returns>true if the specified column value is equivalent to DBNull; otherwise, false.</returns>
        public bool IsDBNull(
            int ordinal)
            => _reader.IsDBNull(ordinal);

        /// <summary>
        /// Advances the data reader to the next result set, if any exist.
        /// </summary>
        /// <remarks>Use this method to process multiple result sets returned by a batch query. After
        /// calling this method, the data reader is positioned at the start of the next result set, if
        /// available.</remarks>
        /// <returns>true if there are more result sets; otherwise, false.</returns>
        public bool NextResult()
            => _reader.NextResult();

        /// <summary>
        /// Advances the reader to the next record in the result set.
        /// </summary>
        /// <remarks>Call this method to iterate through the rows of the result set. The method must be
        /// called before accessing data in the first row.</remarks>
        /// <returns>true if there are more rows; otherwise, false.</returns>
        public bool Read()
            => _reader.Read();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
            => ((IEnumerable)_reader).GetEnumerator();

        /// <summary>
        /// Releases the resources used by the underlying command object if they have not already been released.
        /// </summary>
        /// <remarks>This method ensures that the command object is disposed only once, preventing
        /// multiple disposal attempts. It should be called when the command is no longer needed to free associated
        /// resources.</remarks>
        private void DisposeCommandOnce()
        {
            if (_disposed)
                return;

            _disposed = true;
            _command.Dispose();
        }
    }
}
