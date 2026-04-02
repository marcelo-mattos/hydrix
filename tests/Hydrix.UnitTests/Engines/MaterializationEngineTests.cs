using Hydrix.Attributes.Schemas;
using Hydrix.Engines;
using Hydrix.Schemas.Contract;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Hydrix.UnitTests.Engines
{
    /// <summary>
    /// Contains unit tests for the MaterializationEngine class, validating query materialization behavior.
    /// </summary>
    public class MaterializationEngineTests
    {
        /// <summary>
        /// Represents a valid stored procedure descriptor for procedure materialization scenarios.
        /// </summary>
        [Procedure("usp_TestEntity")]
        private sealed class TestProcedure :
            IProcedure<TestParameter>
        { }

        /// <summary>
        /// Represents a valid test entity for materialization scenarios.
        /// </summary>
        [Table("Entity")]
        private class TestEntity : ITable
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            [Column]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            [Column]
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents an invalid entity missing table metadata.
        /// </summary>
        private class InvalidEntity : ITable
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            [Column]
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents a test implementation of DbParameter used by the test command.
        /// </summary>
        private sealed class TestDbParameter : DbParameter
        {
            /// <summary>
            /// Gets or sets the database type.
            /// </summary>
            public override DbType DbType { get; set; }

            /// <summary>
            /// Gets or sets the parameter direction.
            /// </summary>
            public override ParameterDirection Direction { get; set; }

            /// <summary>
            /// Gets or sets whether the parameter is nullable.
            /// </summary>
            public override bool IsNullable { get; set; }

            /// <summary>
            /// Gets or sets the parameter name.
            /// </summary>
            public override string ParameterName { get; set; }

            /// <summary>
            /// Gets or sets the source column.
            /// </summary>
            public override string SourceColumn { get; set; }

            /// <summary>
            /// Gets or sets the parameter value.
            /// </summary>
            public override object Value { get; set; }

            /// <summary>
            /// Gets or sets source-column null mapping.
            /// </summary>
            public override bool SourceColumnNullMapping { get; set; }

            /// <summary>
            /// Gets or sets the parameter size.
            /// </summary>
            public override int Size { get; set; }

            /// <summary>
            /// Resets the parameter type.
            /// </summary>
            public override void ResetDbType()
            { }
        }

        /// <summary>
        /// Represents a test DbParameterCollection implementation.
        /// </summary>
        private sealed class TestDbParameterCollection : DbParameterCollection
        {
            /// <summary>
            /// Stores the internal parameter list.
            /// </summary>
            private readonly List<DbParameter> _items = new List<DbParameter>();

            /// <summary>
            /// Gets the parameter count.
            /// </summary>
            public override int Count => _items.Count;

            /// <summary>
            /// Gets the synchronization root.
            /// </summary>
            public override object SyncRoot => ((ICollection)_items).SyncRoot;

            /// <summary>
            /// Adds a parameter to the collection.
            /// </summary>
            public override int Add(object value)
            {
                _items.Add((DbParameter)value);
                return _items.Count - 1;
            }

            /// <summary>
            /// Adds multiple parameters to the collection.
            /// </summary>
            public override void AddRange(Array values)
            {
                foreach (var value in values)
                    _items.Add((DbParameter)value);
            }

            /// <summary>
            /// Clears all parameters.
            /// </summary>
            public override void Clear() => _items.Clear();

            /// <summary>
            /// Determines whether a parameter exists in the collection.
            /// </summary>
            public override bool Contains(object value) => _items.Contains((DbParameter)value);

            /// <summary>
            /// Determines whether a parameter exists by name.
            /// </summary>
            public override bool Contains(string value) => _items.Any(p => p.ParameterName == value);

            /// <summary>
            /// Copies parameters to an array.
            /// </summary>
            public override void CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);

            /// <summary>
            /// Gets a collection enumerator.
            /// </summary>
            public override IEnumerator GetEnumerator() => _items.GetEnumerator();

            /// <summary>
            /// Gets a parameter by index.
            /// </summary>
            protected override DbParameter GetParameter(int index) => _items[index];

            /// <summary>
            /// Gets a parameter by name.
            /// </summary>
            protected override DbParameter GetParameter(string parameterName) => _items.First(p => p.ParameterName == parameterName);

            /// <summary>
            /// Gets the index of a parameter instance.
            /// </summary>
            public override int IndexOf(object value) => _items.IndexOf((DbParameter)value);

            /// <summary>
            /// Gets the index of a parameter by name.
            /// </summary>
            public override int IndexOf(string parameterName) => _items.FindIndex(p => p.ParameterName == parameterName);

            /// <summary>
            /// Inserts a parameter at the specified index.
            /// </summary>
            public override void Insert(int index, object value) => _items.Insert(index, (DbParameter)value);

            /// <summary>
            /// Removes a parameter instance.
            /// </summary>
            public override void Remove(object value) => _items.Remove((DbParameter)value);

            /// <summary>
            /// Removes a parameter at the specified index.
            /// </summary>
            public override void RemoveAt(int index) => _items.RemoveAt(index);

            /// <summary>
            /// Removes a parameter by name.
            /// </summary>
            public override void RemoveAt(string parameterName)
            {
                var index = IndexOf(parameterName);
                if (index >= 0)
                    _items.RemoveAt(index);
            }

            /// <summary>
            /// Sets a parameter by index.
            /// </summary>
            protected override void SetParameter(int index, DbParameter value) => _items[index] = value;

            /// <summary>
            /// Sets a parameter by name.
            /// </summary>
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
        /// Represents a simple IDbDataParameter implementation for non-DbCommand fallback tests.
        /// </summary>
        private sealed class TestParameter : IDbDataParameter
        {
            /// <summary>
            /// Gets or sets precision.
            /// </summary>
            public byte Precision { get; set; }

            /// <summary>
            /// Gets or sets scale.
            /// </summary>
            public byte Scale { get; set; }

            /// <summary>
            /// Gets or sets parameter size.
            /// </summary>
            public int Size { get; set; }

            /// <summary>
            /// Gets or sets DbType.
            /// </summary>
            public DbType DbType { get; set; }

            /// <summary>
            /// Gets or sets direction.
            /// </summary>
            public ParameterDirection Direction { get; set; }

            /// <summary>
            /// Gets whether nullable.
            /// </summary>
            public bool IsNullable => true;

            /// <summary>
            /// Gets or sets parameter name.
            /// </summary>
            public string ParameterName { get; set; }

            /// <summary>
            /// Gets or sets source column.
            /// </summary>
            public string SourceColumn { get; set; }

            /// <summary>
            /// Gets or sets source version.
            /// </summary>
            public DataRowVersion SourceVersion { get; set; }

            /// <summary>
            /// Gets or sets value.
            /// </summary>
            public object Value { get; set; }
        }

        /// <summary>
        /// Represents a simple IDataParameterCollection implementation for fallback tests.
        /// </summary>
        private sealed class TestParameterCollection : List<object>, IDataParameterCollection
        {
            /// <summary>
            /// Gets or sets parameter by name.
            /// </summary>
            public object this[string parameterName]
            {
                get => this.FirstOrDefault(x => x is IDataParameter p && p.ParameterName == parameterName);
                set { }
            }

            /// <summary>
            /// Determines whether a parameter exists by name.
            /// </summary>
            public bool Contains(string parameterName)
                => this.Any(x => x is IDataParameter p && p.ParameterName == parameterName);

            /// <summary>
            /// Gets parameter index by name.
            /// </summary>
            public int IndexOf(string parameterName)
                => this.FindIndex(x => x is IDataParameter p && p.ParameterName == parameterName);

            /// <summary>
            /// Removes parameter by name.
            /// </summary>
            public void RemoveAt(string parameterName)
            {
                var index = IndexOf(parameterName);
                if (index >= 0)
                    RemoveAt(index);
            }
        }

        /// <summary>
        /// Represents a DbCommand test implementation for reader execution.
        /// </summary>
        private sealed class TestReaderDbCommand : DbCommand
        {
            /// <summary>
            /// Stores parameter instances associated with the command.
            /// </summary>
            private readonly TestDbParameterCollection _parameters = new TestDbParameterCollection();

            /// <summary>
            /// Gets or sets the reader to return on execution.
            /// </summary>
            public DbDataReader ReaderResult { get; set; }

            /// <summary>
            /// Gets or sets command text.
            /// </summary>
            public override string CommandText { get; set; }

            /// <summary>
            /// Gets or sets command timeout.
            /// </summary>
            public override int CommandTimeout { get; set; }

            /// <summary>
            /// Gets or sets command type.
            /// </summary>
            public override CommandType CommandType { get; set; }

            /// <summary>
            /// Gets or sets design-time visibility.
            /// </summary>
            public override bool DesignTimeVisible { get; set; }

            /// <summary>
            /// Gets or sets updated row source behavior.
            /// </summary>
            public override UpdateRowSource UpdatedRowSource { get; set; }

            /// <summary>
            /// Gets or sets connection.
            /// </summary>
            protected override DbConnection DbConnection { get; set; }

            /// <summary>
            /// Gets the parameter collection.
            /// </summary>
            protected override DbParameterCollection DbParameterCollection => _parameters;

            /// <summary>
            /// Gets or sets transaction.
            /// </summary>
            protected override DbTransaction DbTransaction { get; set; }

            /// <summary>
            /// Cancels execution.
            /// </summary>
            public override void Cancel()
            { }

            /// <summary>
            /// Executes non-query command.
            /// </summary>
            public override int ExecuteNonQuery() => 0;

            /// <summary>
            /// Executes scalar command.
            /// </summary>
            public override object ExecuteScalar() => null;

            /// <summary>
            /// Prepares command.
            /// </summary>
            public override void Prepare()
            { }

            /// <summary>
            /// Creates a parameter instance.
            /// </summary>
            protected override DbParameter CreateDbParameter() => new TestDbParameter();

            /// <summary>
            /// Executes and returns a reader.
            /// </summary>
            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
                => ReaderResult;

            /// <summary>
            /// Asynchronously executes and returns a reader.
            /// </summary>
            protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
                => Task.FromResult(ReaderResult);
        }

        /// <summary>
        /// Verifies that AddRange in TestDbParameterCollection covers both loop branches
        /// (with values and with empty input).
        /// </summary>
        [Fact]
        public void TestDbParameterCollection_AddRange_CoversEmptyAndNonEmpty()
        {
            var collection = new TestDbParameterCollection();
            var parameter = new TestDbParameter { ParameterName = "@Id", Value = 1 };

            collection.AddRange(new object[] { parameter });
            Assert.Equal(1, collection.Count);

            collection.AddRange(Array.Empty<object>());
            Assert.Equal(1, collection.Count);
        }

        /// <summary>
        /// Verifies Query maps rows to entities using text command mode.
        /// </summary>
        [Fact]
        public void Query_Text_ReturnsMappedEntities()
        {
            var command = new TestReaderDbCommand { ReaderResult = CreateReader((1, "Alice"), (2, "Bob")) };
            var connection = CreateOpenConnection(command);

            var result = MaterializationEngine.Query<TestEntity>(
                connection.Object,
                "select Id, Name from t");

            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
            Assert.Equal(CommandType.Text, command.CommandType);
        }

        /// <summary>
        /// Verifies Query maps rows in non-text mode with IDataParameter input.
        /// </summary>
        [Fact]
        public void Query_StoredProcedure_WithParameters_ReturnsMappedEntities()
        {
            var command = new TestReaderDbCommand { ReaderResult = CreateReader((7, "Proc")) };
            var connection = CreateOpenConnection(command);
            var parameter = new TestDbParameter { ParameterName = "@Id", Value = 7 };

            var result = MaterializationEngine.Query<TestEntity>(
                connection.Object,
                "sp_test",
                parameter,
                commandType: CommandType.StoredProcedure,
                parameterPrefix: "@");

            Assert.Single(result);
            Assert.Equal(7, result[0].Id);
            Assert.Equal(CommandType.StoredProcedure, command.CommandType);
            Assert.Single(command.Parameters);
        }

        /// <summary>
        /// Verifies QueryAsync maps rows to entities.
        /// </summary>
        [Fact]
        public async Task QueryAsync_Text_ReturnsMappedEntities()
        {
            var command = new TestReaderDbCommand { ReaderResult = CreateReader((3, "Async")) };
            var connection = CreateOpenConnection(command);

            var result = await MaterializationEngine.QueryAsync<TestEntity>(
                connection.Object,
                "select Id, Name from t");

            Assert.Single(result);
            Assert.Equal(3, result[0].Id);
            Assert.Equal("Async", result[0].Name);
        }

        /// <summary>
        /// Verifies QueryAsync propagates cancellation for non-DbCommand fallback path.
        /// </summary>
        [Fact]
        public async Task QueryAsync_Canceled_ThrowsTaskCanceledException()
        {
            var command = new FallbackReaderCommand();
            var connection = CreateOpenConnection(command);
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await MaterializationEngine.QueryAsync<TestEntity>(
                    connection.Object,
                    "select Id from t",
                    cancellationToken: cancellationTokenSource.Token));
        }

        /// <summary>
        /// Verifies QueryAsync maps entities when using the non-DbCommand fallback execution path.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithFallbackReader_ReturnsMappedEntities()
        {
            var command = new FallbackReaderCommand();
            var connection = CreateOpenConnection(command);

            var result = await MaterializationEngine.QueryAsync<TestEntity>(
                connection.Object,
                "select Id from t");

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("fallback", result[0].Name);
        }

        /// <summary>
        /// Verifies procedure QueryAsync throws when the fallback path returns a non-DbDataReader instance.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_Procedure_WithFallbackNonDbReader_ThrowsInvalidOperationException()
        {
            var command = new FallbackReaderCommand
            {
                ReaderFactory = () => new Mock<IDataReader>().Object
            };
            var connection = CreateOpenConnection(command);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await MaterializationEngine.QueryAsync<TestEntity, TestParameter>(
                    connection.Object,
                    new TestProcedure()));
        }

        /// <summary>
        /// Verifies QueryAsync throws when the fallback path returns a non-DbDataReader instance.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithFallbackNonDbReader_ThrowsInvalidOperationException()
        {
            var command = new FallbackReaderCommand
            {
                ReaderFactory = () => new Mock<IDataReader>().Object
            };
            var connection = CreateOpenConnection(command);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await MaterializationEngine.QueryAsync<TestEntity>(
                    connection.Object,
                    "select Id from t"));
        }

        /// <summary>
        /// Verifies procedure Query overload maps rows to entities.
        /// </summary>
        [Fact]
        public void Query_Procedure_ReturnsMappedEntities()
        {
            var command = new TestReaderDbCommand { ReaderResult = CreateReader((9, "ProcEntity")) };
            var connection = CreateOpenConnection(command);

            var result = MaterializationEngine.Query<TestEntity, TestParameter>(
                connection.Object,
                new TestProcedure(),
                commandTimeout: 15,
                parameterPrefix: "@");

            Assert.Single(result);
            Assert.Equal(9, result[0].Id);
            Assert.Equal("ProcEntity", result[0].Name);
            Assert.Equal(CommandType.StoredProcedure, command.CommandType);
        }

        /// <summary>
        /// Verifies procedure Query overload maps rows to entities when parameterPrefix is null,
        /// covering the default prefix branch.
        /// </summary>
        [Fact]
        public void Query_Procedure_WithNullParameterPrefix_ReturnsMappedEntities()
        {
            var command = new TestReaderDbCommand { ReaderResult = CreateReader((11, "ProcDefaultPrefix")) };
            var connection = CreateOpenConnection(command);

            var result = MaterializationEngine.Query<TestEntity, TestParameter>(
                connection.Object,
                new TestProcedure());

            Assert.Single(result);
            Assert.Equal(11, result[0].Id);
            Assert.Equal("ProcDefaultPrefix", result[0].Name);
            Assert.Equal(CommandType.StoredProcedure, command.CommandType);
        }

        /// <summary>
        /// Verifies procedure QueryAsync overload maps rows to entities.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_Procedure_ReturnsMappedEntities()
        {
            var command = new TestReaderDbCommand { ReaderResult = CreateReader((10, "ProcAsync")) };
            var connection = CreateOpenConnection(command);

            var result = await MaterializationEngine.QueryAsync<TestEntity, TestParameter>(
                connection.Object,
                new TestProcedure(),
                commandTimeout: 15,
                parameterPrefix: "@");

            Assert.Single(result);
            Assert.Equal(10, result[0].Id);
            Assert.Equal("ProcAsync", result[0].Name);
            Assert.Equal(CommandType.StoredProcedure, command.CommandType);
        }

        /// <summary>
        /// Verifies procedure QueryAsync overload propagates cancellation for non-DbCommand fallback path.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_Procedure_Canceled_ThrowsTaskCanceledException()
        {
            var command = new FallbackReaderCommand();
            var connection = CreateOpenConnection(command);
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await MaterializationEngine.QueryAsync<TestEntity, TestParameter>(
                    connection.Object,
                    new TestProcedure(),
                    cancellationToken: cancellationTokenSource.Token));
        }

        /// <summary>
        /// Verifies procedure QueryAsync maps entities when using the non-DbCommand fallback execution path.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_Procedure_WithFallbackReader_ReturnsMappedEntities()
        {
            var command = new FallbackReaderCommand();
            var connection = CreateOpenConnection(command);

            var result = await MaterializationEngine.QueryAsync<TestEntity, TestParameter>(
                connection.Object,
                new TestProcedure());

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("fallback", result[0].Name);
        }

        /// <summary>
        /// Verifies invalid entity requests throw missing metadata exceptions.
        /// </summary>
        [Fact]
        public void Query_InvalidEntity_ThrowsMissingMemberException()
        {
            var command = new TestReaderDbCommand { ReaderResult = CreateReader((1, "x")) };
            var connection = CreateOpenConnection(command);

            Assert.Throws<MissingMemberException>(() => MaterializationEngine.Query<InvalidEntity>(
                connection.Object,
                "select Id from t"));
        }

        /// <summary>
        /// Verifies invalid entity requests throw missing metadata exceptions for procedure Query overload.
        /// </summary>
        [Fact]
        public void Query_Procedure_InvalidEntity_ThrowsMissingMemberException()
        {
            var command = new TestReaderDbCommand { ReaderResult = CreateReader((1, "x")) };
            var connection = CreateOpenConnection(command);

            Assert.Throws<MissingMemberException>(() => MaterializationEngine.Query<InvalidEntity, TestParameter>(
                connection.Object,
                new TestProcedure()));
        }

        /// <summary>
        /// Represents an IDbCommand implementation (non-DbCommand) for async fallback branch tests.
        /// </summary>
        private sealed class FallbackReaderCommand : IDbCommand
        {
            /// <summary>
            /// Gets or sets the factory used to create readers returned by this command.
            /// </summary>
            public Func<IDataReader> ReaderFactory { get; set; } =
                () => CreateReader((1, "fallback"));

            /// <summary>
            /// Gets or sets the SQL text executed by the command.
            /// </summary>
            public string CommandText { get; set; }

            /// <summary>
            /// Gets or sets the command timeout, in seconds.
            /// </summary>
            public int CommandTimeout { get; set; }

            /// <summary>
            /// Gets or sets the command type.
            /// </summary>
            public CommandType CommandType { get; set; }

            /// <summary>
            /// Gets or sets the associated database connection.
            /// </summary>
            public IDbConnection Connection { get; set; }

            /// <summary>
            /// Gets the command parameter collection.
            /// </summary>
            public IDataParameterCollection Parameters { get; } = new TestParameterCollection();

            /// <summary>
            /// Gets or sets the associated transaction.
            /// </summary>
            public IDbTransaction Transaction { get; set; }

            /// <summary>
            /// Gets or sets how command results are applied to a DataRow.
            /// </summary>
            public UpdateRowSource UpdatedRowSource { get; set; }

            /// <summary>
            /// Cancels the command execution.
            /// </summary>
            public void Cancel()
            { }

            /// <summary>
            /// Creates a parameter object for this command.
            /// </summary>
            /// <returns>A new <see cref="IDbDataParameter"/> instance.</returns>
            public IDbDataParameter CreateParameter() => new TestParameter();

            /// <summary>
            /// Releases resources used by this command instance.
            /// </summary>
            public void Dispose()
            { }

            /// <summary>
            /// Executes the command and returns the number of affected rows.
            /// </summary>
            /// <returns>Always returns <c>0</c> in this test implementation.</returns>
            public int ExecuteNonQuery() => 0;

            /// <summary>
            /// Executes the command and returns a data reader.
            /// </summary>
            /// <returns>An <see cref="IDataReader"/> containing fallback test data.</returns>
            public IDataReader ExecuteReader() => ReaderFactory();

            /// <summary>
            /// Executes the command with the specified behavior and returns a data reader.
            /// </summary>
            /// <param name="behavior">The requested command behavior.</param>
            /// <returns>An <see cref="IDataReader"/> containing fallback test data.</returns>
            public IDataReader ExecuteReader(CommandBehavior behavior) => ReaderFactory();

            /// <summary>
            /// Executes the command and returns the first column of the first row.
            /// </summary>
            /// <returns>Always returns <see langword="null"/> in this test implementation.</returns>
            public object ExecuteScalar() => null;

            /// <summary>
            /// Prepares the command for execution.
            /// </summary>
            public void Prepare()
            { }
        }

        /// <summary>
        /// Creates an open connection mock that returns the specified command instance.
        /// </summary>
        /// <param name="command">The command instance returned by <see cref="IDbConnection.CreateCommand"/>.</param>
        /// <returns>A mock open connection configured with the provided command.</returns>
        private static Mock<IDbConnection> CreateOpenConnection(IDbCommand command)
        {
            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.State).Returns(ConnectionState.Open);
            connection.Setup(c => c.CreateCommand()).Returns(command);
            return connection;
        }

        /// <summary>
        /// Creates a DbDataReader from provided row tuples.
        /// </summary>
        /// <param name="rows">The rows used to populate the in-memory data table backing the reader.</param>
        /// <returns>A <see cref="DbDataReader"/> over the generated in-memory table.</returns>
        private static DbDataReader CreateReader(params (int Id, string Name)[] rows)
        {
            var table = new DataTable("Entity");
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));

            foreach (var row in rows)
                table.Rows.Add(row.Id, row.Name);

            return table.CreateDataReader();
        }
    }
}
