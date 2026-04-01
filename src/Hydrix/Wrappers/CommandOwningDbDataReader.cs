using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrix.Wrappers
{
    /// <summary>
    /// Wraps a DbDataReader and preserves the creating command until reader disposal.
    /// </summary>
    internal sealed class CommandOwningDbDataReader :
        DbDataReader
    {
        /// <summary>
        /// Represents the database command associated with the current operation.
        /// </summary>
        private readonly IDbCommand _command;

        /// <summary>
        /// Provides access to the underlying data reader used to retrieve data from a data source.
        /// </summary>
        /// <remarks>This field is intended for internal use to manage data retrieval operations. It
        /// should not be accessed directly outside of the containing class.</remarks>
        private readonly DbDataReader _reader;

        /// <summary>
        /// Indicates whether the object has been disposed.
        /// </summary>
        /// <remarks>This field is typically used to prevent multiple disposal operations and to check the
        /// disposal state within resource management patterns.</remarks>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the CommandOwningDbDataReader class, associating a database command with its
        /// corresponding data reader.
        /// </summary>
        /// <param name="command">The database command that is responsible for creating the data reader. Cannot be null.</param>
        /// <param name="reader">The data reader to be associated with the command. Cannot be null.</param>
        public CommandOwningDbDataReader(
            IDbCommand command,
            DbDataReader reader)
        {
            _command = command;
            _reader = reader;
        }

        /// <summary>
        /// Gets the value of the specified column in its native format.
        /// </summary>
        /// <remarks>Use this indexer to access column values by their ordinal position in the result set.
        /// The returned object should be cast to the expected type. Throws an exception if the ordinal is out of
        /// range.</remarks>
        /// <param name="ordinal">The zero-based column ordinal whose value is to be retrieved.</param>
        /// <returns>The value of the column at the specified ordinal, boxed as an object. Returns DBNull if the column value is
        /// database null.</returns>
        public override object this[int ordinal]
            => _reader[ordinal];

        /// <summary>
        /// Gets the value of the column with the specified name from the current data record.
        /// </summary>
        /// <remarks>If the specified column name does not exist, an exception may be thrown. The returned
        /// value may be DBNull if the column contains a database null.</remarks>
        /// <param name="name">The name of the column to retrieve the value for. The name is case-sensitive.</param>
        /// <returns>The value of the specified column in the current data record.</returns>
        public override object this[string name]
            => _reader[name];

        /// <summary>
        /// Gets the current depth of the data reader in the hierarchy of nested result sets.
        /// </summary>
        /// <remarks>The depth increases when the data reader enters a nested result set, such as when
        /// processing a result set returned from a stored procedure that returns multiple result sets.</remarks>
        public override int Depth
            => _reader.Depth;

        /// <summary>
        /// Gets the number of columns in the current row of the data reader.
        /// </summary>
        /// <remarks>This property returns the number of columns available in the current result set. If
        /// there is no current result set, the value is 0.</remarks>
        public override int FieldCount
            => _reader.FieldCount;

        /// <summary>
        /// Gets a value indicating whether the data reader contains one or more rows.
        /// </summary>
        public override bool HasRows
            => _reader.HasRows;

        /// <summary>
        /// Gets a value indicating whether the data reader is closed.
        /// </summary>
        public override bool IsClosed
            => _reader.IsClosed;

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </summary>
        /// <remarks>This property returns the number of rows affected by the last executed statement
        /// associated with this data reader. If no rows were affected or the statement was not an update, insert, or
        /// delete, the value is -1.</remarks>
        public override int RecordsAffected
            => _reader.RecordsAffected;

        /// <summary>
        /// Gets the number of fields in the current record that are not hidden.
        /// </summary>
        /// <remarks>This property returns the count of fields that are visible to the consumer, excluding
        /// any fields that are marked as hidden by the data provider. Use this property when you need to iterate only
        /// over fields intended for display or processing.</remarks>
        public override int VisibleFieldCount
            => _reader.VisibleFieldCount;

        /// <summary>
        /// Closes the data reader and releases any associated resources.
        /// </summary>
        /// <remarks>This method should be called when the data reader is no longer needed to ensure that
        /// all resources are properly released. After calling this method, attempts to access the reader may result in
        /// exceptions.</remarks>
        public override void Close()
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
        /// Returns a DataTable that describes the column metadata of the current result set.
        /// </summary>
        /// <returns>A DataTable containing schema information for the current result set.</returns>
        public override DataTable GetSchemaTable()
            => _reader.GetSchemaTable();

        /// <summary>
        /// Advances the data reader to the next result set.
        /// </summary>
        /// <returns><see langword="true"/> if there are more result sets; otherwise, <see langword="false"/>.</returns>
        public override bool NextResult()
            => _reader.NextResult();

        /// <summary>
        /// Advances the data reader to the next record.
        /// </summary>
        /// <returns><see langword="true"/> if there are more rows; otherwise, <see langword="false"/>.</returns>
        public override bool Read()
            => _reader.Read();

        /// <summary>
        /// Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The Boolean value of the specified column.</returns>
        public override bool GetBoolean(
            int ordinal)
            => _reader.GetBoolean(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a byte.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The byte value of the specified column.</returns>
        public override byte GetByte(
            int ordinal)
            => _reader.GetByte(ordinal);

        /// <summary>
        /// Reads a stream of bytes from the specified column into the provided buffer.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <param name="dataOffset">The index within the field from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which to copy data.</param>
        /// <param name="bufferOffset">The index in the buffer at which to start writing.</param>
        /// <param name="length">The maximum number of bytes to read.</param>
        /// <returns>The actual number of bytes read.</returns>
        public override long GetBytes(
            int ordinal,
            long dataOffset,
            byte[] buffer,
            int bufferOffset,
            int length)
            => _reader.GetBytes(
                ordinal,
                dataOffset,
                buffer,
                bufferOffset,
                length);

        /// <summary>
        /// Gets the value of the specified column as a character.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The character value of the specified column.</returns>
        public override char GetChar(
            int ordinal)
            => _reader.GetChar(ordinal);

        /// <summary>
        /// Reads a stream of characters from the specified column into the provided buffer.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <param name="dataOffset">The index within the field from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which to copy data.</param>
        /// <param name="bufferOffset">The index in the buffer at which to start writing.</param>
        /// <param name="length">The maximum number of characters to read.</param>
        /// <returns>The actual number of characters read.</returns>
        public override long GetChars(
            int ordinal,
            long dataOffset,
            char[] buffer,
            int bufferOffset,
            int length)
            => _reader.GetChars(
                ordinal,
                dataOffset,
                buffer,
                bufferOffset,
                length);

        /// <summary>
        /// Gets the data type name for the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The provider-specific data type name.</returns>
        public override string GetDataTypeName(
            int ordinal)
            => _reader.GetDataTypeName(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a DateTime.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The DateTime value of the specified column.</returns>
        public override DateTime GetDateTime(
            int ordinal)
            => _reader.GetDateTime(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a decimal.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The decimal value of the specified column.</returns>
        public override decimal GetDecimal(
            int ordinal)
            => _reader.GetDecimal(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a double-precision floating-point number.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The double value of the specified column.</returns>
        public override double GetDouble(
            int ordinal)
            => _reader.GetDouble(ordinal);

        /// <summary>
        /// Returns an enumerator that iterates through the records in the data reader.
        /// </summary>
        /// <returns>An IEnumerator for the current data reader.</returns>
        public override IEnumerator GetEnumerator()
            => ((IEnumerable)_reader).GetEnumerator();

        /// <summary>
        /// Gets the CLR type of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The Type of the specified column.</returns>
        public override Type GetFieldType(
            int ordinal)
            => _reader.GetFieldType(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a single-precision floating-point number.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The float value of the specified column.</returns>
        public override float GetFloat(
            int ordinal)
            => _reader.GetFloat(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a Guid.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The Guid value of the specified column.</returns>
        public override Guid GetGuid(
            int ordinal)
            => _reader.GetGuid(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a 16-bit signed integer.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The Int16 value of the specified column.</returns>
        public override short GetInt16(
            int ordinal)
            => _reader.GetInt16(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a 32-bit signed integer.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The Int32 value of the specified column.</returns>
        public override int GetInt32(
            int ordinal)
            => _reader.GetInt32(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a 64-bit signed integer.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The Int64 value of the specified column.</returns>
        public override long GetInt64(
            int ordinal)
            => _reader.GetInt64(ordinal);

        /// <summary>
        /// Gets the name of the column at the specified ordinal.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The name of the specified column.</returns>
        public override string GetName(
            int ordinal)
            => _reader.GetName(ordinal);

        /// <summary>
        /// Gets the column ordinal for the specified column name.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <returns>The zero-based ordinal of the column.</returns>
        public override int GetOrdinal(
            string name)
            => _reader.GetOrdinal(name);

        /// <summary>
        /// Gets the value of the specified column as a string.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The string value of the specified column.</returns>
        public override string GetString(
            int ordinal)
            => _reader.GetString(ordinal);

        /// <summary>
        /// Gets the value of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override object GetValue(
            int ordinal)
            => _reader.GetValue(ordinal);

        /// <summary>
        /// Populates an array of objects with the column values of the current record.
        /// </summary>
        /// <param name="values">An array of objects to copy the attribute fields into.</param>
        /// <returns>The number of instances of object in the array.</returns>
        public override int GetValues(
            object[] values)
            => _reader.GetValues(values);

        /// <summary>
        /// Determines whether the specified column contains a database null value.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns><see langword="true"/> if the specified column is equivalent to DBNull; otherwise, <see langword="false"/>.</returns>
        public override bool IsDBNull(
            int ordinal)
            => _reader.IsDBNull(ordinal);

        /// <summary>
        /// Gets the value of the specified column as the requested type.
        /// </summary>
        /// <typeparam name="T">The type of the value to return.</typeparam>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column converted to type <typeparamref name="T"/>.</returns>
        public override T GetFieldValue<T>(
            int ordinal)
            => _reader.GetFieldValue<T>(ordinal);

        /// <summary>
        /// Asynchronously gets the value of the specified column as the requested type.
        /// </summary>
        /// <typeparam name="T">The type of the value to return.</typeparam>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the converted value.</returns>
        public override Task<T> GetFieldValueAsync<T>(
            int ordinal,
            CancellationToken cancellationToken)
            => _reader.GetFieldValueAsync<T>(
                ordinal,
                cancellationToken);

        /// <summary>
        /// Asynchronously advances the reader to the next result set.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result is <see langword="true"/> if there are more result sets.</returns>
        public override Task<bool> NextResultAsync(
            CancellationToken cancellationToken)
            => _reader.NextResultAsync(
                cancellationToken);

        /// <summary>
        /// Asynchronously advances the reader to the next record.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result is <see langword="true"/> if there are more rows.</returns>
        public override Task<bool> ReadAsync(
            CancellationToken cancellationToken)
            => _reader.ReadAsync(
                cancellationToken);

        /// <summary>
        /// Asynchronously determines whether the specified column contains a database null value.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result is <see langword="true"/> if the value is DBNull.</returns>
        public override Task<bool> IsDBNullAsync(
            int ordinal,
            CancellationToken cancellationToken)
            => _reader.IsDBNullAsync(
                ordinal,
                cancellationToken);

        /// <summary>
        /// Releases the unmanaged resources used by this instance and optionally releases managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release managed and unmanaged resources; 
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(
            bool disposing)
        {
            if (!disposing || _disposed)
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
        /// Asynchronously releases all resources used by this instance.
        /// </summary>
        /// <returns>A ValueTask that represents the asynchronous dispose operation.</returns>
        public override async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                await _reader.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                _command.Dispose();
            }
        }

        /// <summary>
        /// Disposes the associated command only once, preventing duplicate disposal.
        /// </summary>
        private void DisposeCommandOnce()
        {
            if (_disposed)
                return;

            _disposed = true;
            _command.Dispose();
        }
    }
}
