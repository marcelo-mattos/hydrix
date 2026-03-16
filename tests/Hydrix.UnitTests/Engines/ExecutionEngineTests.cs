using Hydrix.Engines;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Hydrix.UnitTests.Engines
{
    /// <summary>
    /// Contains unit tests for the ExecutionEngine class, covering command creation and sync/async execution paths.
    /// </summary>
    public class ExecutionEngineTests
    {
        /// <summary>
        /// Provides a simple non-DbCommand command implementation for fallback async path tests.
        /// </summary>
        private sealed class TestCommand : IDbCommand
        {
            public string CommandText { get; set; }
            public int CommandTimeout { get; set; }
            public CommandType CommandType { get; set; }
            public IDbConnection Connection { get; set; }
            public IDataParameterCollection Parameters { get; } = new TestParameterCollection();
            public IDbTransaction Transaction { get; set; }
            public UpdateRowSource UpdatedRowSource { get; set; }
            public int NonQueryResult { get; set; } = 1;
            public object ScalarResult { get; set; } = 42;
            public IDataReader ReaderResult { get; set; } = new Mock<IDataReader>().Object;

            public void Cancel() { }
            public IDbDataParameter CreateParameter() => new TestParameter();
            public void Dispose() { }
            public int ExecuteNonQuery() => NonQueryResult;
            public IDataReader ExecuteReader() => ReaderResult;
            public IDataReader ExecuteReader(CommandBehavior behavior) => ReaderResult;
            public object ExecuteScalar() => ScalarResult;
            public void Prepare() { }
        }

        /// <summary>
        /// Represents a simple test data parameter implementation.
        /// </summary>
        private sealed class TestParameter : IDbDataParameter
        {
            public byte Precision { get; set; }
            public byte Scale { get; set; }
            public int Size { get; set; }
            public DbType DbType { get; set; }
            public ParameterDirection Direction { get; set; }
            public bool IsNullable => true;
            public string ParameterName { get; set; }
            public string SourceColumn { get; set; }
            public DataRowVersion SourceVersion { get; set; }
            public object Value { get; set; }
        }

        /// <summary>
        /// Provides an IDataParameterCollection backed by a list.
        /// </summary>
        private sealed class TestParameterCollection : List<object>, IDataParameterCollection
        {
            public object this[string parameterName]
            {
                get => this.FirstOrDefault(x => x is IDataParameter p && p.ParameterName == parameterName);
                set { }
            }

            public bool Contains(string parameterName)
                => this.Any(x => x is IDataParameter p && p.ParameterName == parameterName);

            public int IndexOf(string parameterName)
                => this.FindIndex(x => x is IDataParameter p && p.ParameterName == parameterName);

            public void RemoveAt(string parameterName)
            {
                var index = IndexOf(parameterName);
                if (index >= 0)
                    RemoveAt(index);
            }
        }

        /// <summary>
        /// Provides a DbParameter implementation for DbCommand-based tests.
        /// </summary>
        private sealed class TestDbParameter : DbParameter
        {
            public override DbType DbType { get; set; }
            public override ParameterDirection Direction { get; set; }
            public override bool IsNullable { get; set; }
            public override string ParameterName { get; set; }
            public override string SourceColumn { get; set; }
            public override object Value { get; set; }
            public override bool SourceColumnNullMapping { get; set; }
            public override int Size { get; set; }
            public override void ResetDbType() { }
        }

        /// <summary>
        /// Provides a DbParameterCollection implementation for DbCommand-based tests.
        /// </summary>
        private sealed class TestDbParameterCollection : DbParameterCollection
        {
            private readonly List<DbParameter> _items = new List<DbParameter>();

            public override int Count => _items.Count;
            public override object SyncRoot => ((ICollection)_items).SyncRoot;
            public override int Add(object value)
            {
                _items.Add((DbParameter)value);
                return _items.Count - 1;
            }

            public override void AddRange(Array values)
            {
                foreach (var value in values)
                    _items.Add((DbParameter)value);
            }

            public override void Clear() => _items.Clear();
            public override bool Contains(object value) => _items.Contains((DbParameter)value);
            public override bool Contains(string value) => _items.Any(p => p.ParameterName == value);
            public override void CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);
            public override IEnumerator GetEnumerator() => _items.GetEnumerator();
            protected override DbParameter GetParameter(int index) => _items[index];
            protected override DbParameter GetParameter(string parameterName) => _items.First(p => p.ParameterName == parameterName);
            public override int IndexOf(object value) => _items.IndexOf((DbParameter)value);
            public override int IndexOf(string parameterName) => _items.FindIndex(p => p.ParameterName == parameterName);
            public override void Insert(int index, object value) => _items.Insert(index, (DbParameter)value);
            public override void Remove(object value) => _items.Remove((DbParameter)value);
            public override void RemoveAt(int index) => _items.RemoveAt(index);
            public override void RemoveAt(string parameterName)
            {
                var index = IndexOf(parameterName);
                if (index >= 0)
                    RemoveAt(index);
            }

            protected override void SetParameter(int index, DbParameter value) => _items[index] = value;
            protected override void SetParameter(string parameterName, DbParameter value)
            {
                var index = IndexOf(parameterName);
                if (index >= 0)
                    _items[index] = value;
                else
                    _items.Add(value);
            }
        }

        /// <summary>
        /// Provides a DbCommand implementation to exercise DbCommand async execution branches.
        /// </summary>
        private sealed class TestAsyncDbCommand : DbCommand
        {
            private readonly TestDbParameterCollection _parameters = new TestDbParameterCollection();

            public int NonQueryAsyncResult { get; set; }
            public object ScalarAsyncResult { get; set; }
            public DbDataReader ReaderAsyncResult { get; set; }

            public override string CommandText { get; set; }
            public override int CommandTimeout { get; set; }
            public override CommandType CommandType { get; set; }
            public override bool DesignTimeVisible { get; set; }
            public override UpdateRowSource UpdatedRowSource { get; set; }
            protected override DbConnection DbConnection { get; set; }
            protected override DbParameterCollection DbParameterCollection => _parameters;
            protected override DbTransaction DbTransaction { get; set; }

            public override void Cancel() { }
            public override int ExecuteNonQuery() => NonQueryAsyncResult;
            public override object ExecuteScalar() => ScalarAsyncResult;
            public override void Prepare() { }
            protected override DbParameter CreateDbParameter() => new TestDbParameter();
            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => ReaderAsyncResult;
            public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken) => Task.FromResult(NonQueryAsyncResult);
            public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken) => Task.FromResult(ScalarAsyncResult);
            protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
                => Task.FromResult(ReaderAsyncResult);
        }

        /// <summary>
        /// Verifies ExecuteNonQuery with text command and object parameters.
        /// </summary>
        [Fact]
        public void ExecuteNonQuery_TextCommand_ReturnsAffectedRows()
        {
            var command = new TestCommand { NonQueryResult = 7 };
            var connection = CreateOpenConnection(command);

            var result = ExecutionEngine.ExecuteNonQuery(
                connection.Object,
                "update x set y = @Id",
                new { Id = 1 });

            Assert.Equal(7, result);
            Assert.Equal(CommandType.Text, command.CommandType);
            Assert.Equal("update x set y = @Id", command.CommandText);
            Assert.Single(command.Parameters);
        }

        /// <summary>
        /// Verifies ExecuteNonQuery with non-text command and IDataParameter input.
        /// </summary>
        [Fact]
        public void ExecuteNonQuery_NonTextCommand_BindsIDataParameter()
        {
            var command = new TestCommand { NonQueryResult = 3 };
            var connection = CreateOpenConnection(command);
            var parameter = new TestParameter { ParameterName = "@p1", Value = 10 };

            var result = ExecutionEngine.ExecuteNonQuery(
                connection.Object,
                "sp_test",
                parameter,
                commandType: CommandType.StoredProcedure);

            Assert.Equal(3, result);
            Assert.Equal(CommandType.StoredProcedure, command.CommandType);
            Assert.Single(command.Parameters);
        }

        /// <summary>
        /// Verifies that unsupported non-text parameter input throws ArgumentException.
        /// </summary>
        [Fact]
        public void ExecuteNonQuery_NonTextCommand_UnsupportedParameters_Throws()
        {
            var command = new TestCommand();
            var connection = CreateOpenConnection(command);

            Assert.Throws<ArgumentException>(() => ExecutionEngine.ExecuteNonQuery(
                connection.Object,
                "sp_test",
                new { Id = 1 },
                commandType: CommandType.StoredProcedure));
        }

        /// <summary>
        /// Verifies async non-query uses DbCommand async execution path.
        /// </summary>
        [Fact]
        public async Task ExecuteNonQueryAsync_DbCommand_UsesAsyncPath()
        {
            var dbCommand = new Mock<DbCommand>();
            dbCommand.SetupAllProperties();
            dbCommand.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(11);

            var connection = CreateOpenConnection(dbCommand.Object);
            var result = await ExecutionEngine.ExecuteNonQueryAsync(connection.Object, "update x set y = 1");

            Assert.Equal(11, result);
        }

        /// <summary>
        /// Verifies async non-query fallback path observes cancellation.
        /// </summary>
        [Fact]
        public async Task ExecuteNonQueryAsync_NonDbCommand_Canceled_ThrowsTaskCanceledException()
        {
            var command = new TestCommand();
            var connection = CreateOpenConnection(command);
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await ExecutionEngine.ExecuteNonQueryAsync(
                    connection.Object,
                    "update x set y = 1",
                    cancellationToken: cancellationTokenSource.Token));
        }

        /// <summary>
        /// Verifies ExecuteScalar returns the command scalar result.
        /// </summary>
        [Fact]
        public void ExecuteScalar_ReturnsScalarValue()
        {
            var command = new TestCommand { ScalarResult = 123 };
            var connection = CreateOpenConnection(command);

            var result = ExecutionEngine.ExecuteScalar(connection.Object, "select 123");

            Assert.Equal(123, result);
        }

        /// <summary>
        /// Verifies async scalar uses DbCommand async execution path.
        /// </summary>
        [Fact]
        public async Task ExecuteScalarAsync_DbCommand_UsesAsyncPath()
        {
            var dbCommand = new Mock<DbCommand>();
            dbCommand.SetupAllProperties();
            dbCommand.Setup(c => c.ExecuteScalarAsync(It.IsAny<CancellationToken>())).ReturnsAsync("ok");
            var connection = CreateOpenConnection(dbCommand.Object);

            var result = await ExecutionEngine.ExecuteScalarAsync(connection.Object, "select 'ok'");

            Assert.Equal("ok", result);
        }

        /// <summary>
        /// Verifies async scalar fallback path observes cancellation.
        /// </summary>
        [Fact]
        public async Task ExecuteScalarAsync_NonDbCommand_Canceled_ThrowsTaskCanceledException()
        {
            var command = new TestCommand();
            var connection = CreateOpenConnection(command);
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await ExecutionEngine.ExecuteScalarAsync(
                    connection.Object,
                    "select 1",
                    cancellationToken: cancellationTokenSource.Token));
        }

        /// <summary>
        /// Verifies ExecuteReader returns the reader provided by command execution.
        /// </summary>
        [Fact]
        public void ExecuteReader_ReturnsReader()
        {
            var reader = new Mock<IDataReader>().Object;
            var command = new TestCommand { ReaderResult = reader };
            var connection = CreateOpenConnection(command);

            var result = ExecutionEngine.ExecuteReader(connection.Object, "select 1");

            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies async reader uses DbCommand async execution path.
        /// </summary>
        [Fact]
        public async Task ExecuteReaderAsync_DbCommand_UsesAsyncPath()
        {
            var dataReader = new Mock<DbDataReader>().Object;
            var dbCommand = new TestAsyncDbCommand { ReaderAsyncResult = dataReader };
            var connection = CreateOpenConnection(dbCommand);

            var result = await ExecutionEngine.ExecuteReaderAsync(connection.Object, "select 1");

            Assert.Same(dataReader, result);
        }

        /// <summary>
        /// Verifies async reader fallback path observes cancellation.
        /// </summary>
        [Fact]
        public async Task ExecuteReaderAsync_NonDbCommand_Canceled_ThrowsTaskCanceledException()
        {
            var command = new TestCommand();
            var connection = CreateOpenConnection(command);
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await ExecutionEngine.ExecuteReaderAsync(
                    connection.Object,
                    "select 1",
                    cancellationToken: cancellationTokenSource.Token));
        }

        /// <summary>
        /// Verifies execution throws when connection is not open.
        /// </summary>
        [Fact]
        public void ExecuteNonQuery_ClosedConnection_ThrowsInvalidOperationException()
        {
            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.State).Returns(ConnectionState.Closed);

            Assert.Throws<InvalidOperationException>(() =>
                ExecutionEngine.ExecuteNonQuery(connection.Object, "update x set y = 1"));
        }

        /// <summary>
        /// Creates an open connection mock that returns the provided command.
        /// </summary>
        private static Mock<IDbConnection> CreateOpenConnection(IDbCommand command)
        {
            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.State).Returns(ConnectionState.Open);
            connection.Setup(c => c.CreateCommand()).Returns(command);
            return connection;
        }
    }
}
