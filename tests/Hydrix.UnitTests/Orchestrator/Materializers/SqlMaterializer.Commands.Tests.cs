using Hydrix.Attributes.Parameters;
using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Materializers;
using Hydrix.Orchestrator.Materializers.Contract;
using Hydrix.Schemas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for the SqlMaterializer class, verifying its command creation, parameter binding,
    /// transaction handling, and error conditions.
    /// </summary>
    /// <remarks>These tests ensure that SqlMaterializer behaves correctly when interacting with
    /// database-related abstractions, including handling disposed states, open/closed connections, and parameter
    /// binding for various command scenarios. The class uses mock implementations of IDbConnection, IDbCommand, and
    /// related interfaces to isolate and validate SqlMaterializer's logic without requiring a real database
    /// connection.</remarks>
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Represents a mock implementation of the <see cref="IDbConnection"/> interface for testing or simulation
        /// purposes.
        /// </summary>
        /// <remarks>This class simulates a database connection without interacting with a real database.
        /// It can be used in unit tests or scenarios where a functional database connection is not required. Most
        /// methods and properties are either stubbed or return fixed values, and some members throw <see
        /// cref="NotImplementedException"/> if invoked.</remarks>
        private class FakeDbConnection : IDbConnection
        {
            /// <summary>
            /// Gets or sets the current state of the connection.
            /// </summary>
            /// <remarks>The value indicates whether the connection is open, closed, or in another
            /// defined state. Changing the state may affect the ability to send or receive data.</remarks>
            public ConnectionState State { get; set; } = ConnectionState.Open;

            /// <summary>
            /// Begins a database transaction for the current connection.
            /// </summary>
            /// <returns>An <see cref="IDbTransaction"/> object representing the new transaction.</returns>
            /// <exception cref="NotImplementedException">Thrown in all cases, as this method is not implemented.</exception>
            public IDbTransaction BeginTransaction() => throw new NotImplementedException();

            /// <summary>
            /// Begins a database transaction with the specified isolation level.
            /// </summary>
            /// <param name="il">The isolation level to use for the transaction. Determines how the transaction is isolated from other
            /// operations. Valid values are defined by the <see cref="IsolationLevel"/> enumeration.</param>
            /// <returns>An <see cref="IDbTransaction"/> object representing the new transaction.</returns>
            /// <exception cref="NotImplementedException">Thrown in all cases, as this method is not implemented.</exception>
            public IDbTransaction BeginTransaction(IsolationLevel il) => throw new NotImplementedException();

            /// <summary>
            /// Changes the current database for an open connection to the specified database name.
            /// </summary>
            /// <param name="databaseName">The name of the database to switch to. Cannot be null, empty, or contain invalid characters.</param>
            /// <exception cref="NotImplementedException">Thrown in all cases as this method is not implemented.</exception>
            public void ChangeDatabase(string databaseName) => throw new NotImplementedException();

            /// <summary>
            /// Closes the current resource and releases any associated resources.
            /// </summary>
            /// <exception cref="NotImplementedException">Thrown in all cases as this method is not implemented.</exception>
            public void Close() => throw new NotImplementedException();

            /// <summary>
            /// Gets or sets the connection string used to establish a connection to the database.
            /// </summary>
            public string ConnectionString { get; set; }

            /// <summary>
            /// Gets the time, in seconds, to wait for a connection to open before the attempt times out.
            /// </summary>
            public int ConnectionTimeout => 0;

            /// <summary>
            /// Creates and returns a new command object associated with the current connection.
            /// </summary>
            /// <returns>An <see cref="IDbCommand"/> instance that can be used to execute queries or commands against the data
            /// source.</returns>
            public IDbCommand CreateCommand() => new FakeDbCommand();

            /// <summary>
            /// Gets the name of the database associated with the current context.
            /// </summary>
            public string Database => "FakeDb";

            /// <summary>
            /// Opens the resource or connection for use.
            /// </summary>
            /// <exception cref="NotImplementedException">Thrown in all cases as the method is not yet implemented.</exception>
            public void Open() => throw new NotImplementedException();

            /// <summary>
            /// Releases all resources used by the current instance.
            /// </summary>
            /// <remarks>Call this method when you are finished using the object to free unmanaged
            /// resources and perform other cleanup operations. After calling <see cref="Dispose"/>, the object should
            /// not be used.</remarks>
            public void Dispose()
            { }
        }

        /// <summary>
        /// Represents a mock implementation of the <see cref="IDbCommand"/> interface for testing or simulation
        /// purposes.
        /// </summary>
        /// <remarks>This class provides a non-functional, in-memory substitute for database command
        /// operations. It can be used in unit tests or scenarios where a real database connection is not required. All
        /// execution methods throw <see cref="NotImplementedException"/> and do not interact with any data
        /// source.</remarks>
        private class FakeDbCommand : IDbCommand
        {
            /// <summary>
            /// Gets or sets the SQL statement or command to execute against the data source.
            /// </summary>
            /// <remarks>The command text can be a SQL query, stored procedure name, or other command
            /// recognized by the data provider. Ensure that the syntax is valid for the target database. Changing this
            /// property after preparing or executing the command may require re-preparation or re-execution, depending
            /// on the provider.</remarks>
            public string CommandText { get; set; }

            /// <summary>
            /// Gets or sets the wait time, in seconds, before terminating an attempt to execute a command and
            /// generating an error.
            /// </summary>
            /// <remarks>A value of 0 indicates that the command will wait indefinitely. Setting a
            /// timeout can help prevent applications from hanging if a command takes too long to execute.</remarks>
            public int CommandTimeout { get; set; }

            /// <summary>
            /// Gets or sets a value indicating how the command string is interpreted by the data provider.
            /// </summary>
            /// <remarks>Set this property to specify whether the command is a text command, a stored
            /// procedure, or a direct table access. The default value is provider-specific and may affect how
            /// parameters are handled.</remarks>
            public CommandType CommandType { get; set; }

            /// <summary>
            /// Gets or sets the database connection used to execute commands.
            /// </summary>
            /// <remarks>The caller is responsible for managing the lifetime of the connection. Ensure
            /// that the connection is open before executing operations that require an active database
            /// session.</remarks>
            public IDbConnection Connection { get; set; }

            /// <summary>
            /// Gets the collection of parameters associated with the command.
            /// </summary>
            /// <remarks>Use this collection to add, remove, or access parameters that are sent to the
            /// data source when executing the command. The collection is read-only; however, its contents can be
            /// modified. Parameter names must match those expected by the command text.</remarks>
            public IDataParameterCollection Parameters { get; } = new FakeParameterCollection();

            /// <summary>
            /// Gets or sets the database transaction associated with the current operation.
            /// </summary>
            /// <remarks>Assigning a transaction enables operations to participate in the specified
            /// transaction context. If set to null, operations will not be executed within a transaction.</remarks>
            public IDbTransaction Transaction { get; set; }

            /// <summary>
            /// Gets or sets a value that determines how command results are applied to the DataRow when used with a
            /// data adapter.
            /// </summary>
            /// <remarks>Use this property to specify whether output parameters, first returned rows,
            /// or both are mapped to the DataRow after a command is executed. The value affects how updates are
            /// performed when using a data adapter to update a DataSet.</remarks>
            public UpdateRowSource UpdatedRowSource { get; set; }

            /// <summary>
            /// Requests cancellation of the current operation, if supported.
            /// </summary>
            /// <exception cref="NotImplementedException">Thrown in all cases, as cancellation is not currently implemented.</exception>
            public void Cancel() => throw new NotImplementedException();

            /// <summary>
            /// Creates a new instance of a parameter object for use with database commands.
            /// </summary>
            /// <remarks>The returned parameter is not associated with any specific command or
            /// connection until it is added to a command object. Configure the parameter's properties as needed before
            /// use.</remarks>
            /// <returns>An <see cref="IDbDataParameter"/> representing a parameter that can be added to a command for data
            /// operations.</returns>
            public IDbDataParameter CreateParameter() => new FakeDataParameter();

            /// <summary>
            /// Executes a SQL command that does not return any result sets, such as an INSERT, UPDATE, or DELETE
            /// statement.
            /// </summary>
            /// <returns>The number of rows affected by the command. Returns -1 for statements that do not affect rows, such as
            /// DDL commands.</returns>
            /// <exception cref="NotImplementedException">Thrown in all cases, as this method is not implemented.</exception>
            public int ExecuteNonQuery() => throw new NotImplementedException();

            /// <summary>
            /// Executes the command and returns a data reader for retrieving the results of the query.
            /// </summary>
            /// <returns>An <see cref="IDataReader"/> that can be used to read the results of the command in a forward-only,
            /// read-only manner.</returns>
            /// <exception cref="NotImplementedException">Thrown in all cases, as this method is not implemented.</exception>
            public IDataReader ExecuteReader() => throw new NotImplementedException();

            /// <summary>
            /// Executes the command and returns a data reader for retrieving the results, using the specified command
            /// behavior.
            /// </summary>
            /// <param name="behavior">A value that specifies the behavior of the command and the data reader. This can influence how results
            /// are retrieved, such as whether the connection is closed when the reader is closed, or whether single-row
            /// or schema-only results are returned.</param>
            /// <returns>An <see cref="IDataReader"/> that can be used to read the results of the command.</returns>
            /// <exception cref="NotImplementedException">Thrown in all cases, as this method is not implemented.</exception>
            public IDataReader ExecuteReader(CommandBehavior behavior) => throw new NotImplementedException();

            /// <summary>
            /// Executes the query and returns the first column of the first row in the result set.
            /// </summary>
            /// <returns>An object representing the value of the first column of the first row in the result set. Returns null if
            /// the result set is empty.</returns>
            /// <exception cref="NotImplementedException">Thrown in all cases, as this method is not implemented.</exception>
            public object ExecuteScalar() => throw new NotImplementedException();

            /// <summary>
            /// Prepares the current instance for execution or use. This method should be called before performing
            /// operations that depend on the instance being initialized.
            /// </summary>
            /// <exception cref="NotImplementedException">Thrown in all cases, as this method is not yet implemented.</exception>
            public void Prepare() => throw new NotImplementedException();

            /// <summary>
            /// Releases all resources used by the current instance of the class.
            /// </summary>
            /// <remarks>Call this method when you are finished using the object to free unmanaged
            /// resources and perform other cleanup operations. After calling <see cref="Dispose"/>, the object should
            /// not be used further.</remarks>
            public void Dispose()
            { }
        }

        /// <summary>
        /// Represents a mock implementation of the <see cref="IDbTransaction"/> interface for testing or placeholder
        /// scenarios.
        /// </summary>
        /// <remarks>This class provides stubbed members that do not perform any actual transaction
        /// operations. All methods throw <see cref="NotImplementedException"/> when called. Use this type only in
        /// contexts where a non-functional transaction object is required, such as unit tests or design-time
        /// scenarios.</remarks>
        private class FakeDbTransaction : IDbTransaction
        {
            /// <summary>
            /// Gets the database connection associated with the current context.
            /// </summary>
            public IDbConnection Connection => null;

            /// <summary>
            /// Gets the isolation level used for database transactions.
            /// </summary>
            /// <remarks>The isolation level determines how transaction integrity is maintained and
            /// how changes made by concurrent transactions are visible to each other. This property is typically used
            /// to control concurrency and consistency behavior when executing database operations.</remarks>
            public IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;

            /// <summary>
            /// Commits all pending changes to the underlying data store or transaction.
            /// </summary>
            /// <exception cref="NotImplementedException">Thrown in all cases, as this method is not yet implemented.</exception>
            public void Commit() => throw new NotImplementedException();

            /// <summary>
            /// Reverts all changes made during the current transaction.
            /// </summary>
            /// <exception cref="NotImplementedException">Thrown in all cases as this method is not yet implemented.</exception>
            public void Rollback() => throw new NotImplementedException();

            /// <summary>
            /// Releases all resources used by the current instance of the class.
            /// </summary>
            /// <remarks>Call this method when you are finished using the object to free unmanaged
            /// resources and perform other cleanup operations. After calling <see cref="Dispose"/>, the object should
            /// not be used further.</remarks>
            public void Dispose()
            { }
        }

        /// <summary>
        /// Represents a mock implementation of the <see cref="IDbDataParameter"/> interface for simulating database
        /// parameter behavior in testing scenarios.
        /// </summary>
        /// <remarks>This class is intended for use in unit tests or other scenarios where a real database
        /// parameter is not required. It provides basic property implementations to mimic the behavior of a data
        /// parameter, but does not interact with any actual database provider. Some members throw <see
        /// cref="NotImplementedException"/> if accessed, as they are not supported in this mock
        /// implementation.</remarks>
        private class FakeDataParameter : IDbDataParameter
        {
            /// <summary>
            /// Gets or sets the database type of the parameter.
            /// </summary>
            /// <remarks>Use this property to specify the type of data that the parameter represents
            /// when interacting with the database. Setting the correct database type ensures proper value conversion
            /// and compatibility with the underlying data provider.</remarks>
            public DbType DbType { get; set; }

            /// <summary>
            /// Gets or sets the direction of the parameter for a command.
            /// </summary>
            /// <remarks>Use this property to specify whether the parameter is an input, output,
            /// bidirectional, or a return value when executing a command. The value should be set according to the
            /// intended use of the parameter in the command context.</remarks>
            public ParameterDirection Direction { get; set; }

            /// <summary>
            /// Gets a value indicating whether the current type allows null values.
            /// </summary>
            public bool IsNullable => false;

            /// <summary>
            /// Gets or sets the name of the parameter associated with the current context.
            /// </summary>
            public string ParameterName { get; set; }

            /// <summary>
            /// Gets or sets the name of the source column mapped to a data field.
            /// </summary>
            public string SourceColumn { get; set; }

            /// <summary>
            /// Gets or sets the version of the data in a DataRow to use when retrieving parameter values.
            /// </summary>
            /// <remarks>Use this property to specify which version of the DataRow's data should be
            /// used, such as Original, Current, or Proposed, when working with data-bound parameters. This is commonly
            /// used in scenarios involving updates or concurrency control in data operations.</remarks>
            public DataRowVersion SourceVersion { get; set; }

            /// <summary>
            /// Gets or sets the value associated with this instance.
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// Gets or sets the number of decimal places used for numeric values.
            /// </summary>
            public Byte Precision { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            /// <summary>
            /// Gets or sets the number of decimal places to which numeric values are scaled.
            /// </summary>
            /// <remarks>Use this property to specify the precision of fractional values when working
            /// with numeric data types that support scaling. The valid range and effect of this property may depend on
            /// the underlying data type or context in which it is used.</remarks>
            public Byte Scale { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            /// <summary>
            /// Gets or sets the size of the object.
            /// </summary>
            public Int32 Size { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }

        /// <summary>
        /// Represents a collection of <see cref="IDataParameter"/> objects that can be accessed by parameter name or
        /// index.
        /// </summary>
        /// <remarks>This collection provides methods to locate, access, and remove parameters by their
        /// name, in addition to standard list operations. It is intended for scenarios where parameters are managed by
        /// name, such as in data access or command execution contexts.</remarks>
        private class FakeParameterCollection : List<IDataParameter>, IDataParameterCollection
        {
            /// <summary>
            /// Gets or sets the value of the parameter with the specified name.
            /// </summary>
            /// <param name="parameterName">The name of the parameter to retrieve or assign a value for. Cannot be null.</param>
            /// <returns>The value associated with the specified parameter name, or null if the parameter does not exist.</returns>
            public object this[string parameterName]
            {
                get => this.FirstOrDefault(p => p.ParameterName == parameterName);
                set { }
            }

            /// <summary>
            /// Determines whether a parameter with the specified name exists in the collection.
            /// </summary>
            /// <param name="parameterName">The name of the parameter to locate. Comparison is case-sensitive.</param>
            /// <returns>true if a parameter with the specified name is found; otherwise, false.</returns>
            public bool Contains(string parameterName) => this.Any(p => p.ParameterName == parameterName);

            /// <summary>
            /// Returns the zero-based index of the parameter with the specified name.
            /// </summary>
            /// <remarks>If multiple parameters have the same name, the index of the first occurrence
            /// is returned.</remarks>
            /// <param name="parameterName">The name of the parameter to locate. Comparison is case-sensitive.</param>
            /// <returns>The zero-based index of the parameter if found; otherwise, –1.</returns>
            public int IndexOf(string parameterName) => this.FindIndex(p => p.ParameterName == parameterName);

            /// <summary>
            /// Removes all parameters with the specified name from the collection.
            /// </summary>
            /// <remarks>If multiple parameters share the same name, all matching parameters are
            /// removed. The comparison is case-sensitive.</remarks>
            /// <param name="parameterName">The name of the parameter to remove. Cannot be null.</param>
            public void RemoveAt(string parameterName) => RemoveAll(p => p.ParameterName == parameterName);
        }

        /// <summary>
        /// Represents a test SQL stored procedure definition for use with the ISqlProcedure interface and
        /// FakeDataParameter parameters.
        /// </summary>
        /// <remarks>This class is intended for testing scenarios and provides metadata for the
        /// 'dbo.TestProc' stored procedure, including its parameter definitions. It is not intended for production
        /// use.</remarks>
        private class TestSqlProcedure : ISqlProcedure<FakeDataParameter>
        {
            /// <summary>
            /// Represents a SQL stored procedure mapped to the 'TestProc' procedure in the 'dbo' schema.
            /// </summary>
            /// <remarks>Use this type to invoke or interact with the 'TestProc' stored procedure in
            /// the database. The class is typically used in data access scenarios where stored procedures are exposed
            /// as .NET types for type-safe operations.</remarks>
            [SqlProcedure("dbo", "TestProc")]
            public class Procedure
            { }

            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [SqlParameter("Id", DbType.Int32)]
            public int Id { get; set; }
        }

        /// <summary>
        /// Provides a testable subclass of <see cref="SqlMaterializer"/> for unit testing purposes, allowing controlled
        /// access to internal state and behavior.
        /// </summary>
        /// <remarks>This class exposes setters for internal properties and fields of <see
        /// cref="SqlMaterializer"/> to facilitate testing scenarios. It should only be used in test contexts and is not
        /// intended for production use.</remarks>
        private class SqlMaterializerTestable : SqlMaterializer
        {
            /// <summary>
            /// Sets the disposed state of the current instance.
            /// </summary>
            /// <remarks>This property is intended for advanced scenarios where direct control over
            /// the disposed state is required. Changing the disposed state may affect the ability to use the instance
            /// and can lead to unexpected behavior if not managed carefully.</remarks>
            public bool IsDisposedSet
            {
                set =>
                    typeof(SqlMaterializer).GetProperty("IsDisposed", BindingFlags.Public | BindingFlags.Instance)?.SetValue(this, value);
            }

            /// <summary>
            /// Sets the transaction state for the current instance to indicate that a transaction is active.
            /// </summary>
            /// <remarks>This property is intended for internal use to simulate an active database
            /// transaction. Setting this property may affect the transactional behavior of subsequent operations on the
            /// instance.</remarks>
            public bool IsTransactionActiveSet
            {
                set =>
                    typeof(SqlMaterializer).GetProperty("DbTransaction", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(this, new FakeDbTransaction());
            }

            /// <summary>
            /// Sets the database connection used by the materializer instance.
            /// </summary>
            /// <remarks>This property is intended for advanced scenarios where direct control over
            /// the underlying database connection is required. Setting this property replaces the internal connection
            /// used for database operations. Use with caution, as changing the connection may affect ongoing or future
            /// operations.</remarks>
            public IDbConnection DbConnectionSet
            {
                set =>
                    typeof(SqlMaterializer).GetField("_dbConnection", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(this, value);
            }

            /// <summary>
            /// Sets the database transaction to be used by the materializer.
            /// </summary>
            /// <remarks>This property is intended for internal use when configuring the transaction
            /// context for database operations. Setting this property replaces the current transaction with a new
            /// instance. This property does not retrieve the current transaction.</remarks>
            public IDbTransaction DbTransactionSet
            {
                set =>
                    typeof(SqlMaterializer).GetProperty("DbTransaction", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(this, new FakeDbTransaction());
            }

            /// <summary>
            /// Initializes a new instance of the SqlMaterializerTestable class for testing purposes using a fake
            /// database connection.
            /// </summary>
            /// <remarks>This constructor is intended for use in unit tests or scenarios where a real
            /// database connection is not required. The instance is initialized with a mock connection to facilitate
            /// isolated testing of materialization logic.</remarks>
            public SqlMaterializerTestable()
                : base(new FakeDbConnection())
            { }

            /// <summary>
            /// Creates and returns a database command configured with the specified command type, SQL statement,
            /// parameters, and transaction context.
            /// </summary>
            /// <param name="commandType">The type of command to execute, such as Text, StoredProcedure, or TableDirect. Determines how the SQL
            /// statement is interpreted by the database.</param>
            /// <param name="sql">The SQL statement or stored procedure to execute. Cannot be null or empty.</param>
            /// <param name="parameterBinder">An action that binds parameters to the command. This delegate is invoked to configure the command's
            /// parameters before execution. Can be null if no parameters are required.</param>
            /// <param name="transaction">The transaction context in which the command will execute. Can be null if the command should execute
            /// outside of a transaction.</param>
            /// <returns>An <see cref="IDbCommand"/> instance configured with the specified command type, SQL statement,
            /// parameters, and transaction. The caller is responsible for disposing the returned command when it is no
            /// longer needed.</returns>
            public IDbCommand CallCreateCommandCore(
                CommandType commandType,
                string sql,
                Action<IDbCommand> parameterBinder,
                IDbTransaction transaction)
            => CreateCommandCore(
                    commandType,
                    sql,
                    parameterBinder,
                    transaction);
        }

        /// <summary>
        /// Verifies that calling CreateCommandCore on a disposed SqlMaterializerTestable instance throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <remarks>This test ensures that the CreateCommandCore method enforces correct disposal
        /// semantics by throwing an ObjectDisposedException when invoked after the object has been disposed. This
        /// behavior helps prevent usage of resources that have already been released.</remarks>
        [Fact]
        public void CreateCommandCore_Throws_ObjectDisposedException_WhenDisposed()
        {
            var mat = new SqlMaterializerTestable
            {
                IsDisposedSet = true,
                DbConnectionSet = new FakeDbConnection()
            };
            Assert.Throws<ObjectDisposedException>(() =>
                mat.CallCreateCommandCore(CommandType.Text, "SELECT 1", null, null));
        }

        /// <summary>
        /// Verifies that calling CreateCommandCore throws an InvalidOperationException when the database connection is
        /// not open.
        /// </summary>
        /// <remarks>This test ensures that CreateCommandCore enforces the requirement for an open
        /// connection before executing a command. The method simulates a closed connection and asserts that the
        /// expected exception is thrown, validating correct error handling in this scenario.</remarks>
        [Fact]
        public void CreateCommandCore_Throws_InvalidOperationException_WhenConnectionNotOpen()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection { State = ConnectionState.Closed }
            };
            Assert.Throws<InvalidOperationException>(() =>
                mat.CallCreateCommandCore(CommandType.Text, "SELECT 1", null, null));
        }

        /// <summary>
        /// Verifies that the CreateCommandCore method correctly sets command properties and binds parameters as
        /// expected.
        /// </summary>
        /// <remarks>This test ensures that the command type, command text, and command timeout are
        /// properly assigned, and that the parameter binder delegate is invoked when creating a command using
        /// CreateCommandCore. It also checks that the command is associated with the specified transaction.</remarks>
        [Fact]
        public void CreateCommandCore_Sets_Command_Properties_And_Binds_Parameters()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            bool binderCalled = false;
            var cmd = mat.CallCreateCommandCore(
                CommandType.StoredProcedure,
                "spTest",
                c => { binderCalled = true; },
                new FakeDbTransaction());
            Assert.Equal(CommandType.StoredProcedure, cmd.CommandType);
            Assert.Equal("spTest", cmd.CommandText);
            Assert.Equal(mat.Timeout, cmd.CommandTimeout);
            Assert.True(binderCalled);
        }

        /// <summary>
        /// Verifies that the CreateCommandCore method assigns the specified transaction to the created command when a
        /// transaction parameter is provided.
        /// </summary>
        /// <remarks>This test ensures that passing a transaction to CreateCommandCore results in the
        /// command's Transaction property being set to the same instance. It validates correct transaction propagation
        /// for command creation scenarios.</remarks>
        [Fact]
        public void CreateCommandCore_Sets_Transaction_From_Parameter()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection(),
                DbTransactionSet = new FakeDbTransaction(),
                IsTransactionActiveSet = true
            };
            var transaction = new FakeDbTransaction();
            var cmd = mat.CallCreateCommandCore(
                CommandType.Text,
                "SELECT 1",
                null,
                transaction);
            Assert.Equal(transaction, cmd.Transaction);
        }

        /// <summary>
        /// Verifies that the CreateCommandCore method sets the transaction property of the created command to the
        /// active transaction when a transaction is active.
        /// </summary>
        /// <remarks>This test ensures that when an active transaction is present, the command created by
        /// CreateCommandCore is associated with that transaction. This behavior is important for maintaining
        /// transactional consistency when executing database commands.</remarks>
        [Fact]
        public void CreateCommandCore_Sets_Transaction_From_ActiveTransaction()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection(),
                DbTransactionSet = new FakeDbTransaction(),
                IsTransactionActiveSet = true
            };
            var cmd = mat.CallCreateCommandCore(
                CommandType.Text,
                "SELECT 1",
                null,
                mat.DbTransaction);
            Assert.Equal(mat.DbTransaction, cmd.Transaction);
        }

        /// <summary>
        /// Verifies that the CreateCommandCore method calls LogCommand when SQL logging is enabled.
        /// </summary>
        /// <remarks>This test ensures that enabling SQL logging on the SqlMaterializerTestable instance
        /// results in LogCommand being executed during command creation. The absence of exceptions indicates that
        /// LogCommand was called successfully.</remarks>
        [Fact]
        public void CreateCommandCore_Calls_LogCommand_When_Enabled()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection(),
                EnableSqlLogging = true
            };
            // No exception means LogCommand executed (writes to console)
            var cmd = mat.CallCreateCommandCore(
                CommandType.Text,
                "SELECT 1",
                null,
                null);
            Assert.NotNull(cmd);
        }

        /// <summary>
        /// Verifies that the CreateCommand method correctly binds parameters when provided with an object containing
        /// parameter values.
        /// </summary>
        /// <remarks>This test ensures that named parameters supplied via an anonymous object are properly
        /// mapped to the corresponding placeholders in the SQL command text. It checks that the command text remains
        /// unchanged and that parameter binding occurs as expected.</remarks>
        [Fact]
        public void CreateCommand_Object_Parameters_Binds_Parameters()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (ISqlMaterializer)mat;
            var parameters = new { Id = 42 };
            var cmd = sqlMat.CreateCommand("SELECT * FROM T WHERE Id=@Id", parameters, null);
            Assert.Equal("SELECT * FROM T WHERE Id=@Id", cmd.CommandText);
        }

        /// <summary>
        /// Verifies that the CreateCommand method correctly binds enumerable parameters to the resulting command.
        /// </summary>
        /// <remarks>This test ensures that when an enumerable of parameters is provided to CreateCommand,
        /// each parameter is properly added to the command's Parameters collection. It specifically checks that a
        /// parameter with the name '@Id' is present exactly once, confirming correct binding behavior.</remarks>
        [Fact]
        public void CreateCommand_Enumerable_Parameters_Binds_Parameters()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (ISqlMaterializer)mat;
            var param = new FakeDataParameter { ParameterName = "@Id", Value = 1, DbType = DbType.Int32 };
            var cmd = sqlMat.CreateCommand(CommandType.Text, "SELECT * FROM T WHERE Id=@Id", new[] { param }, null);
            Assert.Single(cmd.Parameters.Cast<IDataParameter>().Where(p => p.ParameterName == "@Id"));
        }

        /// <summary>
        /// Verifies that the CreateCommand&lt;T&gt; method throws a MissingMemberException when the specified procedure type
        /// does not have a SqlProcedureAttribute.
        /// </summary>
        /// <remarks>This test ensures that attempting to create a command for a procedure type lacking
        /// the required SqlProcedureAttribute results in an exception, enforcing correct usage of the API.</remarks>
        [Fact]
        public void CreateCommand_Generic_Throws_If_No_SqlProcedureAttribute()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (ISqlMaterializer)mat;
            var fakeProc = new NoAttributeProcedure();
            Assert.Throws<MissingMemberException>(() =>
                sqlMat.CreateCommand<NoAttributeParameter>(fakeProc, null));
        }

        /// <summary>
        /// Represents a SQL procedure that operates on parameters of type NoAttributeParameter.
        /// </summary>
        private class NoAttributeProcedure :
            ISqlProcedure<NoAttributeParameter>
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents a data parameter without attribute-based configuration, implementing the <see
        /// cref="IDataParameter"/> interface for use in data operations.
        /// </summary>
        /// <remarks>This class provides a basic implementation of <see cref="IDataParameter"/> for
        /// scenarios where parameter attributes are not required or supported. It can be used to supply parameter
        /// information to data providers in custom or testing contexts. All properties must be set explicitly before
        /// use.</remarks>
        private class NoAttributeParameter :
            IDataParameter
        {
            /// <summary>
            /// Gets or sets the database type of the parameter.
            /// </summary>
            /// <remarks>The database type determines how the value of the parameter is interpreted
            /// and sent to the database. Setting this property explicitly can be useful when the default type inference
            /// does not match the intended database type.</remarks>
            public DbType DbType { get; set; }

            /// <summary>
            /// Gets or sets a value that indicates whether the parameter is an input, output, bidirectional, or a
            /// return value parameter for a command.
            /// </summary>
            /// <remarks>Use this property to specify how the parameter will be used when executing a
            /// command against a data source. The default is typically Input, but other values may be required
            /// depending on the command and stored procedure signature.</remarks>
            public ParameterDirection Direction { get; set; }

            /// <summary>
            /// Gets a value indicating whether the current type allows null values.
            /// </summary>
            public bool IsNullable => false;

            /// <summary>
            /// Gets or sets the name of the parameter associated with the current context.
            /// </summary>
            public string ParameterName { get; set; }

            /// <summary>
            /// Gets or sets the name of the source column mapped to the property or parameter.
            /// </summary>
            /// <remarks>Set this property to specify which column in the data source is associated
            /// with this member. This is commonly used in data binding and parameter mapping scenarios.</remarks>
            public string SourceColumn { get; set; }

            /// <summary>
            /// Gets or sets the version of the data in a DataRow to use when retrieving parameter values.
            /// </summary>
            /// <remarks>Use this property to specify which version of the DataRow's data should be
            /// used, such as Original, Current, or Proposed, when working with data-bound parameters. This is commonly
            /// used in scenarios involving updates or concurrency control in data operations.</remarks>
            public DataRowVersion SourceVersion { get; set; }

            /// <summary>
            /// Gets or sets the value associated with the current instance.
            /// </summary>
            public object Value { get; set; }
        }

        /// <summary>
        /// Verifies that the generic CreateCommand method binds parameters based on attribute metadata applied to the
        /// input object.
        /// </summary>
        /// <remarks>This test ensures that when an object with parameter attributes is passed to
        /// CreateCommand, the resulting command contains parameters matching the object's properties and their values,
        /// as defined by the attributes. It checks that the command text, command type, and parameter values are set
        /// correctly according to the attribute configuration.</remarks>
        [Fact]
        public void CreateCommand_Generic_Binds_Parameters_From_Attributes()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (ISqlMaterializer)mat;
            var proc = new ProcedureWithAttributes { Id = 7, Name = "abc" };
            var cmd = sqlMat.CreateCommand<AttributeParameter>(proc, null);
            Assert.Equal("dbo.TestProc", cmd.CommandText);
            Assert.Equal(CommandType.StoredProcedure, cmd.CommandType);
            Assert.Equal(2, cmd.Parameters.Count);
            Assert.Contains(cmd.Parameters.Cast<IDataParameter>(), p => p.ParameterName == "@Id" && (int)p.Value == 7);
            Assert.Contains(cmd.Parameters.Cast<IDataParameter>(), p => p.ParameterName == "@Name" && (string)p.Value == "abc");
        }

        /// <summary>
        /// Represents a SQL stored procedure definition with parameter attributes for use in database operations.
        /// </summary>
        /// <remarks>This class is typically used to map .NET properties to SQL procedure parameters via
        /// attributes, enabling type-safe invocation and parameter binding. It is intended for scenarios where
        /// procedures are invoked programmatically and parameter metadata is required for correct execution.</remarks>
        [SqlProcedure("dbo", "TestProc")]
        private class ProcedureWithAttributes :
            ISqlProcedure<AttributeParameter>
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [SqlParameter("@Id", DbType.Int32)]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with this entity.
            /// </summary>
            [SqlParameter("@Name", DbType.String)]
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents a parameter for a database command or query, providing metadata such as type, direction, and
        /// value.
        /// </summary>
        /// <remarks>This class implements the IDataParameter interface to encapsulate information about a
        /// single parameter used in database operations. It includes properties for specifying the parameter's data
        /// type, direction, name, source column, source version, and value. The IsNullable property always returns
        /// false, indicating that the parameter does not support null values.</remarks>
        private class AttributeParameter :
            IDataParameter
        {
            /// <summary>
            /// Gets or sets the database type of the parameter.
            /// </summary>
            /// <remarks>The database type determines how the parameter value is interpreted and sent
            /// to the database. Setting this property explicitly can be useful when the default type mapping does not
            /// match the desired database type.</remarks>
            public DbType DbType { get; set; }

            /// <summary>
            /// Gets or sets the direction of the parameter for a command.
            /// </summary>
            /// <remarks>Use this property to specify whether the parameter is an input, output,
            /// bidirectional, or a return value when executing a command against a data source.</remarks>
            public ParameterDirection Direction { get; set; }

            /// <summary>
            /// Gets a value indicating whether the type allows null values.
            /// </summary>
            public bool IsNullable => false;

            /// <summary>
            /// Gets or sets the name of the parameter associated with the current context.
            /// </summary>
            public string ParameterName { get; set; }

            /// <summary>
            /// Gets or sets the name of the source column mapped to the property or parameter.
            /// </summary>
            public string SourceColumn { get; set; }

            /// <summary>
            /// Gets or sets the version of the data in a DataRow to use when retrieving parameter values.
            /// </summary>
            /// <remarks>Use this property to specify which version of the DataRow's data should be
            /// used, such as Original, Current, or Proposed, when working with data-bound parameters. This is commonly
            /// used in scenarios involving updates or concurrency control in data operations.</remarks>
            public DataRowVersion SourceVersion { get; set; }

            /// <summary>
            /// Gets or sets the value associated with this instance.
            /// </summary>
            public object Value { get; set; }
        }
    }
}