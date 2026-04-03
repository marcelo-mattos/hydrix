using Hydrix.Wrappers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Hydrix.UnitTests.Engines
{
    /// <summary>
    /// Contains unit tests for command-owning reader wrappers, validating forwarding behavior and ownership-based
    /// disposal semantics.
    /// </summary>
    public class CommandOwningReadersTests
    {
        /// <summary>
        /// Verifies that wrapping a reader throws <see cref="ArgumentNullException"/> when command or reader
        /// arguments are null.
        /// </summary>
        [Fact]
        public void Wrap_Throws_WhenArgumentsAreNull()
        {
            var command = new TrackingCommand();
            var reader = new TrackingDataReader();

            Assert.Throws<ArgumentNullException>(() => CommandOwningReader.Wrap(null, reader));
            Assert.Throws<ArgumentNullException>(() => CommandOwningReader.Wrap(command, null));
        }

        /// <summary>
        /// Verifies that wrapping a non-DbDataReader instance returns <see cref="CommandOwningDataReader"/>.
        /// </summary>
        [Fact]
        public void Wrap_ReturnsNonDbWrapper_ForPlainDataReader()
        {
            var wrapped = CommandOwningReader.Wrap(new TrackingCommand(), new TrackingDataReader());

            Assert.IsType<CommandOwningDataReader>(wrapped);
        }

        /// <summary>
        /// Verifies that wrapping a <see cref="DbDataReader"/> instance returns
        /// <see cref="CommandOwningDbDataReader"/>.
        /// </summary>
        [Fact]
        public void Wrap_ReturnsDbWrapper_ForDbDataReader()
        {
            var wrapped = CommandOwningReader.Wrap(new TrackingCommand(), new TrackingDbDataReader());

            Assert.IsType<CommandOwningDbDataReader>(wrapped);
        }

        /// <summary>
        /// Verifies that <see cref="CommandOwningDataReader"/> forwards member calls to the inner reader and disposes
        /// the associated command only once.
        /// </summary>
        [Fact]
        public void CommandOwningDataReader_ForwardsMembers_AndDisposesCommandOnlyOnce()
        {
            var command = new TrackingCommand();
            var inner = new TrackingDataReader();
            var wrapped = new CommandOwningDataReader(command, inner);
            var byteBuffer = new byte[4];
            var charBuffer = new char[4];
            var values = new object[2];

            Assert.Equal(123, wrapped[0]);
            Assert.Equal("value", wrapped["Name"]);
            Assert.Equal(2, wrapped.Depth);
            Assert.False(wrapped.IsClosed);
            Assert.Equal(7, wrapped.RecordsAffected);
            Assert.Equal(2, wrapped.FieldCount);
            Assert.True(wrapped.GetBoolean(0));
            Assert.Equal((byte)2, wrapped.GetByte(0));
            Assert.Equal(3, wrapped.GetBytes(0, 0, byteBuffer, 0, byteBuffer.Length));
            Assert.Equal(1, byteBuffer[0]);
            Assert.Equal('x', wrapped.GetChar(0));
            Assert.Equal(3, wrapped.GetChars(0, 0, charBuffer, 0, charBuffer.Length));
            Assert.Equal('a', charBuffer[0]);
            Assert.Same(inner, wrapped.GetData(0));
            Assert.Equal("Int32", wrapped.GetDataTypeName(0));
            Assert.Equal(new DateTime(2020, 1, 2), wrapped.GetDateTime(0));
            Assert.Equal(1.23m, wrapped.GetDecimal(0));
            Assert.Equal(2.34d, wrapped.GetDouble(0));
            Assert.Equal(typeof(int), wrapped.GetFieldType(0));
            Assert.Equal(3.45f, wrapped.GetFloat(0));
            Assert.Equal(TrackingDataReader.GuidValue, wrapped.GetGuid(0));
            Assert.Equal((short)4, wrapped.GetInt16(0));
            Assert.Equal(123, wrapped.GetInt32(0));
            Assert.Equal(456L, wrapped.GetInt64(0));
            Assert.Equal("Id", wrapped.GetName(0));
            Assert.Equal(1, wrapped.GetOrdinal("Name"));
            Assert.NotNull(wrapped.GetSchemaTable());
            Assert.Equal("value", wrapped.GetString(1));
            Assert.Equal("value", wrapped.GetValue(1));
            Assert.Equal(2, wrapped.GetValues(values));
            Assert.Equal("value", values[1]);
            Assert.False(wrapped.IsDBNull(0));
            Assert.True(wrapped.NextResult());
            Assert.True(wrapped.Read());
            Assert.NotNull(wrapped.GetEnumerator());

            wrapped.Close();
            wrapped.Close();
            wrapped.Dispose();

            Assert.Equal(2, inner.CloseCount);
            Assert.Equal(0, inner.DisposeCount);
            Assert.Equal(1, command.DisposeCount);
        }

        /// <summary>
        /// Verifies that disposing <see cref="CommandOwningDataReader"/> multiple times disposes the inner reader and
        /// command exactly once.
        /// </summary>
        [Fact]
        public void CommandOwningDataReader_Dispose_DisposesReaderAndCommandOnlyOnce()
        {
            var command = new TrackingCommand();
            var inner = new TrackingDataReader();
            var wrapped = new CommandOwningDataReader(command, inner);

            wrapped.Dispose();
            wrapped.Dispose();

            Assert.Equal(1, inner.DisposeCount);
            Assert.Equal(1, command.DisposeCount);
        }

        /// <summary>
        /// Verifies that <see cref="CommandOwningDbDataReader"/> forwards members correctly and enforces single
        /// ownership disposal semantics for the associated command.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task CommandOwningDbDataReader_ForwardsMembers_AndKeepsCommandSingleOwned()
        {
            var command = new TrackingCommand();
            var inner = new TrackingDbDataReader();
            var wrapped = new CommandOwningDbDataReader(command, inner);
            var byteBuffer = new byte[4];
            var charBuffer = new char[4];
            var values = new object[2];

            Assert.Equal(123, wrapped[0]);
            Assert.Equal("value", wrapped["Name"]);
            Assert.Equal(1, wrapped.Depth);
            Assert.Equal(2, wrapped.FieldCount);
            Assert.True(wrapped.HasRows);
            Assert.False(wrapped.IsClosed);
            Assert.Equal(5, wrapped.RecordsAffected);
            Assert.Equal(2, wrapped.VisibleFieldCount);
            Assert.True(wrapped.GetBoolean(0));
            Assert.Equal((byte)2, wrapped.GetByte(0));
            Assert.Equal(3, wrapped.GetBytes(0, 0, byteBuffer, 0, byteBuffer.Length));
            Assert.Equal('x', wrapped.GetChar(0));
            Assert.Equal(3, wrapped.GetChars(0, 0, charBuffer, 0, charBuffer.Length));
            Assert.Equal("Int32", wrapped.GetDataTypeName(0));
            Assert.Equal(new DateTime(2020, 1, 2), wrapped.GetDateTime(0));
            Assert.Equal(1.23m, wrapped.GetDecimal(0));
            Assert.Equal(2.34d, wrapped.GetDouble(0));
            Assert.NotNull(wrapped.GetEnumerator());
            Assert.Equal(typeof(int), wrapped.GetFieldType(0));
            Assert.Equal(3.45f, wrapped.GetFloat(0));
            Assert.Equal(TrackingDbDataReader.GuidValue, wrapped.GetGuid(0));
            Assert.Equal((short)4, wrapped.GetInt16(0));
            Assert.Equal(123, wrapped.GetInt32(0));
            Assert.Equal(456L, wrapped.GetInt64(0));
            Assert.Equal("Id", wrapped.GetName(0));
            Assert.Equal(1, wrapped.GetOrdinal("Name"));
            Assert.NotNull(wrapped.GetSchemaTable());
            Assert.Equal("value", wrapped.GetString(1));
            Assert.Equal("value", wrapped.GetValue(1));
            Assert.Equal(2, wrapped.GetValues(values));
            Assert.False(wrapped.IsDBNull(0));
            Assert.True(wrapped.NextResult());
            Assert.Equal(123, wrapped.GetFieldValue<int>(0));
            Assert.Equal(123, await wrapped.GetFieldValueAsync<int>(0, CancellationToken.None));
            Assert.False(await wrapped.NextResultAsync(CancellationToken.None));
            Assert.True(await wrapped.ReadAsync(CancellationToken.None));
            Assert.False(await wrapped.IsDBNullAsync(0, CancellationToken.None));

            wrapped.Close();
            wrapped.Close();
            wrapped.Dispose();

            Assert.Equal(2, inner.CloseCount);
            Assert.Equal(0, inner.DisposeCount);
            Assert.Equal(1, command.DisposeCount);
        }

        /// <summary>
        /// Verifies that calling protected dispose with <c>disposing = false</c> does not dispose the inner reader or
        /// command.
        /// </summary>
        [Fact]
        public void CommandOwningDbDataReader_DisposeFalse_DoesNotDisposeInnerReaderOrCommand()
        {
            var command = new TrackingCommand();
            var inner = new TrackingDbDataReader();
            var wrapped = new CommandOwningDbDataReader(command, inner);
            var disposeMethod = typeof(CommandOwningDbDataReader).GetMethod(
                "Dispose",
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(bool) },
                null);

            disposeMethod.Invoke(wrapped, new object[] { false });

            Assert.Equal(0, inner.DisposeCount);
            Assert.Equal(0, command.DisposeCount);
        }

        /// <summary>
        /// Verifies that asynchronous disposal of <see cref="CommandOwningDbDataReader"/> disposes the inner reader
        /// and command exactly once.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task CommandOwningDbDataReader_DisposeAsync_DisposesReaderAndCommandOnlyOnce()
        {
            var command = new TrackingCommand();
            var inner = new TrackingDbDataReader();
            var wrapped = new CommandOwningDbDataReader(command, inner);

            await wrapped.DisposeAsync();
            await wrapped.DisposeAsync();

            Assert.Equal(1, inner.DisposeAsyncCount);
            Assert.Equal(1, command.DisposeCount);
        }

        /// <summary>
        /// Represents a lightweight tracking implementation of <see cref="IDbCommand"/> used by tests to verify
        /// disposal and wrapper behavior.
        /// </summary>
        private sealed class TrackingCommand : IDbCommand
        {
            /// <summary>
            /// Gets or sets the SQL statement to execute.
            /// </summary>
            public string CommandText { get; set; }

            /// <summary>
            /// Gets or sets the wait time before terminating command execution.
            /// </summary>
            public int CommandTimeout { get; set; }

            /// <summary>
            /// Gets or sets how <see cref="CommandText"/> is interpreted.
            /// </summary>
            public CommandType CommandType { get; set; }

            /// <summary>
            /// Gets or sets the connection used by the command.
            /// </summary>
            public IDbConnection Connection { get; set; }

            /// <summary>
            /// Gets the parameter collection associated with the command.
            /// </summary>
            public IDataParameterCollection Parameters { get; } = new TrackingParameterCollection();

            /// <summary>
            /// Gets or sets the transaction within which the command executes.
            /// </summary>
            public IDbTransaction Transaction { get; set; }

            /// <summary>
            /// Gets or sets how command results are applied to a <see cref="DataRow"/>.
            /// </summary>
            public UpdateRowSource UpdatedRowSource { get; set; }

            /// <summary>
            /// Gets the number of times <see cref="Dispose"/> was called.
            /// </summary>
            public int DisposeCount { get; private set; }

            /// <summary>
            /// Attempts to cancel command execution.
            /// </summary>
            public void Cancel()
            { }

            /// <summary>
            /// Creates a new parameter instance.
            /// </summary>
            /// <returns>An <see cref="IDbDataParameter"/> instance.</returns>
            /// <exception cref="NotSupportedException">Always thrown by this test implementation.</exception>
            public IDbDataParameter CreateParameter() => throw new NotSupportedException();

            /// <summary>
            /// Releases resources associated with this command and increments the disposal counter.
            /// </summary>
            public void Dispose() => DisposeCount++;

            /// <summary>
            /// Executes a SQL statement and returns affected rows.
            /// </summary>
            /// <returns>The number of affected rows.</returns>
            /// <exception cref="NotSupportedException">Always thrown by this test implementation.</exception>
            public int ExecuteNonQuery() => throw new NotSupportedException();

            /// <summary>
            /// Executes the command and returns a data reader.
            /// </summary>
            /// <returns>An <see cref="IDataReader"/> instance.</returns>
            /// <exception cref="NotSupportedException">Always thrown by this test implementation.</exception>
            public IDataReader ExecuteReader() => throw new NotSupportedException();

            /// <summary>
            /// Executes the command with the specified behavior and returns a data reader.
            /// </summary>
            /// <param name="behavior">A description of the command execution behavior.</param>
            /// <returns>An <see cref="IDataReader"/> instance.</returns>
            /// <exception cref="NotSupportedException">Always thrown by this test implementation.</exception>
            public IDataReader ExecuteReader(CommandBehavior behavior) => throw new NotSupportedException();

            /// <summary>
            /// Executes the query and returns the first column of the first row.
            /// </summary>
            /// <returns>The first column of the first row in the result set.</returns>
            /// <exception cref="NotSupportedException">Always thrown by this test implementation.</exception>
            public object ExecuteScalar() => throw new NotSupportedException();

            /// <summary>
            /// Creates a prepared version of the command.
            /// </summary>
            public void Prepare()
            { }
        }

        /// <summary>
        /// Represents a minimal parameter collection used in command wrapper tests.
        /// </summary>
        private sealed class TrackingParameterCollection : List<object>, IDataParameterCollection
        {
            /// <summary>
            /// Gets or sets the parameter with the specified name.
            /// </summary>
            /// <param name="parameterName">The parameter name.</param>
            /// <returns>The parameter value associated with the name.</returns>
            public object this[string parameterName]
            {
                get => null;
                set { }
            }

            /// <summary>
            /// Determines whether a parameter with the specified name exists in the collection.
            /// </summary>
            /// <param name="parameterName">The parameter name to find.</param>
            /// <returns><see langword="true"/> if the parameter exists; otherwise, <see langword="false"/>.</returns>
            public bool Contains(string parameterName) => false;

            /// <summary>
            /// Returns the index of the parameter with the specified name.
            /// </summary>
            /// <param name="parameterName">The parameter name to locate.</param>
            /// <returns>The zero-based index if found; otherwise, -1.</returns>
            public int IndexOf(string parameterName) => -1;

            /// <summary>
            /// Removes the parameter with the specified name from the collection.
            /// </summary>
            /// <param name="parameterName">The parameter name to remove.</param>
            public void RemoveAt(string parameterName)
            { }
        }

        /// <summary>
        /// Represents a lightweight <see cref="IDataReader"/> test double with deterministic values and counters.
        /// </summary>
        private sealed class TrackingDataReader : IDataReader, IEnumerable
        {
            /// <summary>
            /// Gets a deterministic Guid value used by tests.
            /// </summary>
            public static readonly Guid GuidValue = Guid.Parse("11111111-1111-1111-1111-111111111111");

            /// <summary>
            /// Maps column names to their zero-based ordinals.
            /// </summary>
            private readonly Dictionary<string, int> _ordinals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Id"] = 0,
                ["Name"] = 1
            };

            /// <summary>
            /// Stores deterministic row values used by the test reader.
            /// </summary>
            private readonly object[] _values = { 123, "value" };

            /// <summary>
            /// Stores deterministic byte payload used in byte-read operations.
            /// </summary>
            private readonly byte[] _bytes = { 1, 2, 3 };

            /// <summary>
            /// Stores deterministic character payload used in char-read operations.
            /// </summary>
            private readonly char[] _chars = { 'a', 'b', 'c' };

            /// <summary>
            /// Represents schema metadata returned by <see cref="GetSchemaTable"/>.
            /// </summary>
            private readonly DataTable _schemaTable = new DataTable("Schema");

            /// <summary>
            /// Indicates whether the reader has been closed or disposed.
            /// </summary>
            private bool _isClosed;

            /// <summary>
            /// Tracks how many times <see cref="NextResult"/> was called.
            /// </summary>
            private int _nextResultCalls;

            /// <summary>
            /// Tracks how many times <see cref="Read"/> was called.
            /// </summary>
            private int _readCalls;

            /// <summary>
            /// Gets the number of times <see cref="Close"/> was called.
            /// </summary>
            public int CloseCount { get; private set; }

            /// <summary>
            /// Gets the number of times <see cref="Dispose"/> was called.
            /// </summary>
            public int DisposeCount { get; private set; }

            /// <summary>
            /// Gets the value of the specified column by ordinal.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The column value.</returns>
            public object this[int i] => _values[i];

            /// <summary>
            /// Gets the value of the specified column by name.
            /// </summary>
            /// <param name="name">The column name.</param>
            /// <returns>The column value.</returns>
            public object this[string name] => _values[_ordinals[name]];

            /// <summary>
            /// Gets the depth of nesting for the current row.
            /// </summary>
            public int Depth => 2;

            /// <summary>
            /// Gets a value indicating whether the reader is closed.
            /// </summary>
            public bool IsClosed => _isClosed;

            /// <summary>
            /// Gets the number of rows affected.
            /// </summary>
            public int RecordsAffected => 7;

            /// <summary>
            /// Gets the number of fields in the current row.
            /// </summary>
            public int FieldCount => 2;

            /// <summary>
            /// Closes the data reader.
            /// </summary>
            public void Close()
            {
                CloseCount++;
                _isClosed = true;
            }

            /// <summary>
            /// Releases resources used by the reader.
            /// </summary>
            public void Dispose()
            {
                DisposeCount++;
                _isClosed = true;
            }

            /// <summary>
            /// Gets the value of the specified column as a Boolean.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The Boolean value.</returns>
            public bool GetBoolean(int i) => true;

            /// <summary>
            /// Gets the value of the specified column as a byte.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The byte value.</returns>
            public byte GetByte(int i) => 2;

            /// <summary>
            /// Reads a stream of bytes from the specified column into a buffer.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <param name="fieldOffset">The index within the field from which to start reading.</param>
            /// <param name="buffer">The destination buffer.</param>
            /// <param name="bufferoffset">The index in the buffer where writing starts.</param>
            /// <param name="length">The maximum number of bytes to read.</param>
            /// <returns>The number of bytes read.</returns>
            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {
                Array.Copy(_bytes, 0, buffer, bufferoffset, Math.Min(length, _bytes.Length));
                return _bytes.Length;
            }

            /// <summary>
            /// Gets the value of the specified column as a character.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The character value.</returns>
            public char GetChar(int i) => 'x';

            /// <summary>
            /// Reads a stream of characters from the specified column into a buffer.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <param name="fieldoffset">The index within the field from which to start reading.</param>
            /// <param name="buffer">The destination buffer.</param>
            /// <param name="bufferoffset">The index in the buffer where writing starts.</param>
            /// <param name="length">The maximum number of characters to read.</param>
            /// <returns>The number of characters read.</returns>
            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            {
                Array.Copy(_chars, 0, buffer, bufferoffset, Math.Min(length, _chars.Length));
                return _chars.Length;
            }

            /// <summary>
            /// Returns an <see cref="IDataReader"/> for the specified column.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>An <see cref="IDataReader"/> instance.</returns>
            public IDataReader GetData(int i) => this;

            /// <summary>
            /// Gets the provider-specific type name of the specified column.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The provider-specific type name.</returns>
            public string GetDataTypeName(int i) => "Int32";

            /// <summary>
            /// Gets the value of the specified column as a <see cref="DateTime"/>.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The date and time value.</returns>
            public DateTime GetDateTime(int i) => new DateTime(2020, 1, 2);

            /// <summary>
            /// Gets the value of the specified column as a decimal.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The decimal value.</returns>
            public decimal GetDecimal(int i) => 1.23m;

            /// <summary>
            /// Gets the value of the specified column as a double.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The double value.</returns>
            public double GetDouble(int i) => 2.34d;

            /// <summary>
            /// Gets the CLR type of the specified column.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The CLR type.</returns>
            public Type GetFieldType(int i) => typeof(int);

            /// <summary>
            /// Gets the value of the specified column as a single-precision floating-point number.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The float value.</returns>
            public float GetFloat(int i) => 3.45f;

            /// <summary>
            /// Gets the value of the specified column as a Guid.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The Guid value.</returns>
            public Guid GetGuid(int i) => GuidValue;

            /// <summary>
            /// Gets the value of the specified column as a 16-bit integer.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The 16-bit integer value.</returns>
            public short GetInt16(int i) => 4;

            /// <summary>
            /// Gets the value of the specified column as a 32-bit integer.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The 32-bit integer value.</returns>
            public int GetInt32(int i) => 123;

            /// <summary>
            /// Gets the value of the specified column as a 64-bit integer.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The 64-bit integer value.</returns>
            public long GetInt64(int i) => 456L;

            /// <summary>
            /// Gets the name of the specified column.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The column name.</returns>
            public string GetName(int i) => i == 0 ? "Id" : "Name";

            /// <summary>
            /// Gets the column ordinal for the specified column name.
            /// </summary>
            /// <param name="name">The column name.</param>
            /// <returns>The zero-based column ordinal.</returns>
            public int GetOrdinal(string name) => _ordinals[name];

            /// <summary>
            /// Gets schema information for the current result set.
            /// </summary>
            /// <returns>A <see cref="DataTable"/> containing schema metadata.</returns>
            public DataTable GetSchemaTable() => _schemaTable;

            /// <summary>
            /// Gets the value of the specified column as a string.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The string value.</returns>
            public string GetString(int i) => (string)_values[i];

            /// <summary>
            /// Gets the value of the specified column.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns>The value as an object.</returns>
            public object GetValue(int i) => _values[i];

            /// <summary>
            /// Copies the column values of the current row into the provided array.
            /// </summary>
            /// <param name="values">The destination array.</param>
            /// <returns>The number of copied values.</returns>
            public int GetValues(object[] values)
            {
                _values.CopyTo(values, 0);
                return _values.Length;
            }

            /// <summary>
            /// Determines whether the specified column contains a database null value.
            /// </summary>
            /// <param name="i">The zero-based column ordinal.</param>
            /// <returns><see langword="true"/> if the column contains <see cref="DBNull"/>; otherwise, <see langword="false"/>.</returns>
            public bool IsDBNull(int i) => false;

            /// <summary>
            /// Advances to the next result set.
            /// </summary>
            /// <returns><see langword="true"/> for the first call; otherwise, <see langword="false"/>.</returns>
            public bool NextResult() => _nextResultCalls++ == 0;

            /// <summary>
            /// Advances to the next record in the current result set.
            /// </summary>
            /// <returns><see langword="true"/> for the first call; otherwise, <see langword="false"/>.</returns>
            public bool Read() => _readCalls++ == 0;

            /// <summary>
            /// Returns an enumerator over the current value array.
            /// </summary>
            /// <returns>An <see cref="IEnumerator"/> instance.</returns>
            public IEnumerator GetEnumerator() => ((IEnumerable)_values).GetEnumerator();
        }

        /// <summary>
        /// Represents a deterministic <see cref="DbDataReader"/> test double with tracking counters.
        /// </summary>
        private sealed class TrackingDbDataReader : DbDataReader
        {
            /// <summary>
            /// Gets a deterministic Guid value used by tests.
            /// </summary>
            public static readonly Guid GuidValue = Guid.Parse("22222222-2222-2222-2222-222222222222");

            /// <summary>
            /// Maps column names to their zero-based ordinals.
            /// </summary>
            private readonly Dictionary<string, int> _ordinals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Id"] = 0,
                ["Name"] = 1
            };

            /// <summary>
            /// Stores deterministic row values used by the test reader.
            /// </summary>
            private readonly object[] _values = { 123, "value" };

            /// <summary>
            /// Stores deterministic byte payload used in byte-read operations.
            /// </summary>
            private readonly byte[] _bytes = { 1, 2, 3 };

            /// <summary>
            /// Stores deterministic character payload used in char-read operations.
            /// </summary>
            private readonly char[] _chars = { 'a', 'b', 'c' };

            /// <summary>
            /// Represents schema metadata returned by <see cref="GetSchemaTable"/>.
            /// </summary>
            private readonly DataTable _schemaTable = new DataTable("Schema");

            /// <summary>
            /// Indicates whether the reader has been closed or disposed.
            /// </summary>
            private bool _isClosed;

            /// <summary>
            /// Tracks how many times <see cref="NextResult"/> was called.
            /// </summary>
            private int _nextResultCalls;

            /// <summary>
            /// Tracks how many times <see cref="Read"/> was called.
            /// </summary>
            private int _readCalls;

            /// <summary>
            /// Gets the number of times <see cref="Close"/> was called.
            /// </summary>
            public int CloseCount { get; private set; }

            /// <summary>
            /// Gets the number of times synchronous dispose was called.
            /// </summary>
            public int DisposeCount { get; private set; }

            /// <summary>
            /// Gets the number of times asynchronous dispose was called.
            /// </summary>
            public int DisposeAsyncCount { get; private set; }

            /// <summary>
            /// Gets the value of the specified column by ordinal.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The column value.</returns>
            public override object this[int ordinal] => _values[ordinal];

            /// <summary>
            /// Gets the value of the specified column by name.
            /// </summary>
            /// <param name="name">The column name.</param>
            /// <returns>The column value.</returns>
            public override object this[string name] => _values[_ordinals[name]];

            /// <summary>
            /// Gets the nesting depth for the current row.
            /// </summary>
            public override int Depth => 1;

            /// <summary>
            /// Gets the number of fields in the current row.
            /// </summary>
            public override int FieldCount => 2;

            /// <summary>
            /// Gets a value indicating whether the reader contains one or more rows.
            /// </summary>
            public override bool HasRows => true;

            /// <summary>
            /// Gets a value indicating whether the reader is closed.
            /// </summary>
            public override bool IsClosed => _isClosed;

            /// <summary>
            /// Gets the number of rows affected by the command.
            /// </summary>
            public override int RecordsAffected => 5;

            /// <summary>
            /// Gets the number of visible fields in the current record.
            /// </summary>
            public override int VisibleFieldCount => 2;

            /// <summary>
            /// Closes the reader.
            /// </summary>
            public override void Close()
            {
                CloseCount++;
                _isClosed = true;
            }

            /// <summary>
            /// Returns schema information for the current result set.
            /// </summary>
            /// <returns>A <see cref="DataTable"/> containing schema metadata.</returns>
            public override DataTable GetSchemaTable() => _schemaTable;

            /// <summary>
            /// Advances to the next result set.
            /// </summary>
            /// <returns><see langword="true"/> for the first call; otherwise, <see langword="false"/>.</returns>
            public override bool NextResult() => _nextResultCalls++ == 0;

            /// <summary>
            /// Advances to the next row.
            /// </summary>
            /// <returns><see langword="true"/> for the first call; otherwise, <see langword="false"/>.</returns>
            public override bool Read() => _readCalls++ == 0;

            /// <summary>
            /// Gets the value of the specified column as a Boolean.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The Boolean value.</returns>
            public override bool GetBoolean(int ordinal) => true;

            /// <summary>
            /// Gets the value of the specified column as a byte.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The byte value.</returns>
            public override byte GetByte(int ordinal) => 2;

            /// <summary>
            /// Reads a stream of bytes from the specified column into a buffer.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <param name="dataOffset">The index in the field from which to begin the read.</param>
            /// <param name="buffer">The destination buffer.</param>
            /// <param name="bufferOffset">The index in the buffer at which writing begins.</param>
            /// <param name="length">The maximum number of bytes to read.</param>
            /// <returns>The number of bytes read.</returns>
            public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
            {
                Array.Copy(_bytes, 0, buffer, bufferOffset, Math.Min(length, _bytes.Length));
                return _bytes.Length;
            }

            /// <summary>
            /// Gets the value of the specified column as a character.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The character value.</returns>
            public override char GetChar(int ordinal) => 'x';

            /// <summary>
            /// Reads a stream of characters from the specified column into a buffer.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <param name="dataOffset">The index in the field from which to begin the read.</param>
            /// <param name="buffer">The destination buffer.</param>
            /// <param name="bufferOffset">The index in the buffer at which writing begins.</param>
            /// <param name="length">The maximum number of characters to read.</param>
            /// <returns>The number of characters read.</returns>
            public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
            {
                Array.Copy(_chars, 0, buffer, bufferOffset, Math.Min(length, _chars.Length));
                return _chars.Length;
            }

            /// <summary>
            /// Gets the provider-specific type name for the specified column.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The provider-specific type name.</returns>
            public override string GetDataTypeName(int ordinal) => "Int32";

            /// <summary>
            /// Gets the value of the specified column as a <see cref="DateTime"/>.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The date and time value.</returns>
            public override DateTime GetDateTime(int ordinal) => new DateTime(2020, 1, 2);

            /// <summary>
            /// Gets the value of the specified column as a decimal.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The decimal value.</returns>
            public override decimal GetDecimal(int ordinal) => 1.23m;

            /// <summary>
            /// Gets the value of the specified column as a double.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The double value.</returns>
            public override double GetDouble(int ordinal) => 2.34d;

            /// <summary>
            /// Returns an enumerator for the row values.
            /// </summary>
            /// <returns>An <see cref="IEnumerator"/> instance.</returns>
            public override IEnumerator GetEnumerator() => ((IEnumerable)_values).GetEnumerator();

            /// <summary>
            /// Gets the CLR type of the specified column.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The CLR type.</returns>
            public override Type GetFieldType(int ordinal) => typeof(int);

            /// <summary>
            /// Gets the value of the specified column as a single-precision floating-point number.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The float value.</returns>
            public override float GetFloat(int ordinal) => 3.45f;

            /// <summary>
            /// Gets the value of the specified column as a Guid.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The Guid value.</returns>
            public override Guid GetGuid(int ordinal) => GuidValue;

            /// <summary>
            /// Gets the value of the specified column as a 16-bit integer.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The 16-bit integer value.</returns>
            public override short GetInt16(int ordinal) => 4;

            /// <summary>
            /// Gets the value of the specified column as a 32-bit integer.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The 32-bit integer value.</returns>
            public override int GetInt32(int ordinal) => 123;

            /// <summary>
            /// Gets the value of the specified column as a 64-bit integer.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The 64-bit integer value.</returns>
            public override long GetInt64(int ordinal) => 456L;

            /// <summary>
            /// Gets the name of the specified column.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The column name.</returns>
            public override string GetName(int ordinal) => ordinal == 0 ? "Id" : "Name";

            /// <summary>
            /// Gets the column ordinal for the specified column name.
            /// </summary>
            /// <param name="name">The column name.</param>
            /// <returns>The zero-based ordinal.</returns>
            public override int GetOrdinal(string name) => _ordinals[name];

            /// <summary>
            /// Gets the value of the specified column as a string.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The string value.</returns>
            public override string GetString(int ordinal) => (string)_values[ordinal];

            /// <summary>
            /// Gets the value of the specified column.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The column value.</returns>
            public override object GetValue(int ordinal) => _values[ordinal];

            /// <summary>
            /// Copies all column values for the current row into the provided array.
            /// </summary>
            /// <param name="values">The destination array.</param>
            /// <returns>The number of copied values.</returns>
            public override int GetValues(object[] values)
            {
                _values.CopyTo(values, 0);
                return _values.Length;
            }

            /// <summary>
            /// Determines whether the specified column contains a database null value.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns><see langword="true"/> if the value is <see cref="DBNull"/>; otherwise, <see langword="false"/>.</returns>
            public override bool IsDBNull(int ordinal) => false;

            /// <summary>
            /// Gets the value of the specified column as the requested type.
            /// </summary>
            /// <typeparam name="T">The target type.</typeparam>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <returns>The converted value.</returns>
            public override T GetFieldValue<T>(int ordinal) => (T)Convert.ChangeType(_values[ordinal], typeof(T));

            /// <summary>
            /// Asynchronously gets the value of the specified column as the requested type.
            /// </summary>
            /// <typeparam name="T">The target type.</typeparam>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
            /// <returns>A task that returns the converted value.</returns>
            public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken) => Task.FromResult(GetFieldValue<T>(ordinal));

            /// <summary>
            /// Asynchronously advances to the next result set.
            /// </summary>
            /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
            /// <returns>A task whose result indicates whether another result set exists.</returns>
            public override Task<bool> NextResultAsync(CancellationToken cancellationToken) => Task.FromResult(NextResult());

            /// <summary>
            /// Asynchronously advances to the next row.
            /// </summary>
            /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
            /// <returns>A task whose result indicates whether another row exists.</returns>
            public override Task<bool> ReadAsync(CancellationToken cancellationToken) => Task.FromResult(Read());

            /// <summary>
            /// Asynchronously determines whether the specified column contains a database null value.
            /// </summary>
            /// <param name="ordinal">The zero-based column ordinal.</param>
            /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
            /// <returns>A task whose result indicates whether the column value is <see cref="DBNull"/>.</returns>
            public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken) => Task.FromResult(IsDBNull(ordinal));

            /// <summary>
            /// Releases resources used by the reader.
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to dispose managed resources; otherwise, <see langword="false"/>.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    DisposeCount++;
                    _isClosed = true;
                }

                base.Dispose(disposing);
            }

            /// <summary>
            /// Asynchronously releases resources used by the reader.
            /// </summary>
            /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
            public override ValueTask DisposeAsync()
            {
                DisposeAsyncCount++;
                _isClosed = true;
                return default;
            }
        }
    }
}
