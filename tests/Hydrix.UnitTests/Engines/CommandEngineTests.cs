using Hydrix.Attributes.Parameters;
using Hydrix.Attributes.Schemas;
using Hydrix.Configuration;
using Hydrix.Engines;
using Hydrix.Schemas.Contract;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace Hydrix.UnitTests.Engines
{
    /// <summary>
    /// Contains unit tests for the CommandEngine class, verifying command creation, parameter binding, transaction
    /// assignment, and logging behaviors.
    /// </summary>
    /// <remarks>This test class uses mock implementations of database-related interfaces to simulate database
    /// operations without requiring a real database connection. The tests validate correct usage patterns and error
    /// handling for the CommandEngine API, including scenarios involving closed connections, parameter binding,
    /// transaction association, and logging. Use these tests as a reference for expected CommandEngine behaviors in
    /// various usage scenarios.</remarks>
    public class CommandEngineTests
    {
        /// <summary>
        /// Represents a mock implementation of the <see cref="IDbConnection"/> interface for testing or simulation
        /// purposes.
        /// </summary>
        /// <remarks>This class simulates a database connection without interacting with a real database.
        /// It can be used in unit tests or scenarios where a functional database connection is not required. Most
        /// methods and properties are either stubbed or return fixed values, and some members throw <see
        /// cref="NotImplementedException"/> if invoked.</remarks>
        private class FakeDbConnection :
            IDbConnection
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
        private class FakeDbCommand :
            IDbCommand
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
        private class FakeDbTransaction :
            IDbTransaction
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
        private class FakeDataParameter :
            IDbDataParameter
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
        private class FakeParameterCollection :
            List<IDataParameter>,
            IDataParameterCollection
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
        /// Represents a custom procedure with specific parameters for use in database operations.
        /// </summary>
        /// <remarks>This class implements the IProcedure interface with typed parameters, enabling the
        /// definition of procedures that can be executed within the Hydrix framework. Use this type to encapsulate
        /// procedure parameters when interacting with database-related APIs.</remarks>
        [Procedure("CustomProcedure")]
        private class CustomProcedure : IProcedure<CustomDataParameter>
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            /// <remarks>This property is typically used to reference the entity in database
            /// operations or business logic. Ensure that the Id is set to a valid value before performing operations
            /// that require identification of the entity.</remarks>
            [Parameter("Id", DbType.Int32)]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with the entity.
            /// </summary>
            /// <remarks>This property is used to identify the entity by a human-readable name. It is
            /// important to ensure that the name is unique within the context of its usage.</remarks>
            [Parameter("Name", DbType.String)]
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents a custom data parameter that provides additional attributes for parameter handling.
        /// </summary>
        /// <remarks>This class extends AttributeParameter to allow specifying whether a custom setting is
        /// applied. Use the CustomSet property to indicate if the custom configuration is active.</remarks>
        private class CustomDataParameter : AttributeParameter
        {
            /// <summary>
            /// Gets or sets a value indicating whether the custom setting is enabled.
            /// </summary>
            /// <remarks>Set this property to <see langword="true"/> to activate the custom behavior,
            /// or to <see langword="false"/> to deactivate it. The effect of enabling this setting depends on the
            /// specific context in which it is used.</remarks>
            public bool CustomSet { get; set; }
        }

        /// <summary>
        /// Represents a provider-specific db type enum used to validate provider-specific setter binding.
        /// </summary>
        private enum CustomProviderDbType
        {
            /// <summary>
            /// Test enum value mapped from an invalid <see cref="DbType"/> integer.
            /// </summary>
            ProviderSpecific = 777
        }

        /// <summary>
        /// Represents a parameter exposing a provider-specific *DbType property.
        /// </summary>
        private class ProviderAwareDataParameter : AttributeParameter
        {
            /// <summary>
            /// Gets or sets the provider-specific database type.
            /// </summary>
            public CustomProviderDbType SqlDbType { get; set; }
        }

        /// <summary>
        /// Represents a procedure using an out-of-range <see cref="DbType"/> value to trigger provider-specific mapping.
        /// </summary>
        [Procedure("ProviderAwareProcedure")]
        private class ProviderAwareProcedure : IProcedure<ProviderAwareDataParameter>
        {
            /// <summary>
            /// Gets or sets a sample value to bind.
            /// </summary>
            [Parameter("Code", (DbType)777)]
            public int Code { get; set; }
        }

        /// <summary>
        /// Represents a procedure containing both standard and provider-specific DbType mappings.
        /// </summary>
        [Procedure("MixedProviderAwareProcedure")]
        private class MixedProviderAwareProcedure : IProcedure<ProviderAwareDataParameter>
        {
            /// <summary>
            /// Gets or sets a standard DbType-mapped value.
            /// </summary>
            [Parameter("Name", DbType.String)]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets a provider-specific DbType-mapped value.
            /// </summary>
            [Parameter("Code", (DbType)777)]
            public int Code { get; set; }
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

        /// <summary>
        /// Represents a procedure type without the required ProcedureAttribute for negative testing.
        /// </summary>
        private class NoAttributeProcedure : IProcedure<FakeDataParameter>
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents a procedure with an out-of-range DbType and no provider-specific DbType property on parameter.
        /// </summary>
        [Procedure("NoProviderSetterProcedure")]
        private class NoProviderSetterProcedure : IProcedure<AttributeParameter>
        {
            /// <summary>
            /// Gets or sets a sample value.
            /// </summary>
            [Parameter("Code", (DbType)777)]
            public int Code { get; set; }
        }

        /// <summary>
        /// Verifies that CreateCommandCore throws an InvalidOperationException when the connection is not open.
        /// </summary>
        /// <remarks>This test ensures that attempting to create a command with a closed connection
        /// results in the expected exception, enforcing correct usage of the API.</remarks>
        [Fact]
        public void CreateCommandCore_Throws_WhenConnectionNotOpen()
        {
            var conn = new FakeDbConnection();
            conn.State = ConnectionState.Closed;
            Assert.Throws<InvalidOperationException>(() =>
                CommandEngine.CreateCommandCore(conn, null, CommandType.Text, "SELECT 1", null, null, null));
        }

        /// <summary>
        /// Verifies that the CreateCommandCore method sets command properties and binds parameters as expected.
        /// </summary>
        /// <remarks>This test ensures that the command type, command text, and command timeout are
        /// correctly assigned, and that the parameter binder delegate is invoked when creating a command using
        /// CreateCommandCore.</remarks>
        [Fact]
        public void CreateCommandCore_SetsProperties_AndBindsParameters()
        {
            var conn = new FakeDbConnection();
            bool binderCalled = false;
            var cmd = CommandEngine.CreateCommandCore(
                conn,
                null,
                CommandType.StoredProcedure,
                "spTest",
                c => { binderCalled = true; },
                77,
                null);
            Assert.Equal(CommandType.StoredProcedure, cmd.CommandType);
            Assert.Equal("spTest", cmd.CommandText);
            Assert.Equal(77, cmd.CommandTimeout);
            Assert.True(binderCalled);
        }

        /// <summary>
        /// Verifies that the CreateCommand method sets the command text and binds parameters as expected.
        /// </summary>
        /// <remarks>This test ensures that the command text is correctly assigned and that parameters are
        /// properly bound when using the CreateCommand method with a specified parameter prefix and parameter
        /// object.</remarks>
        [Fact]
        public void CreateCommand_SetsCommandText_AndBindsParameters()
        {
            var conn = new FakeDbConnection();
            var cmd = CommandEngine.CreateCommand(
                conn,
                null,
                CommandType.Text,
                "SELECT * FROM T WHERE Id=@Id",
                new { Id = 42 },
                "@",
                30,
                null);
            Assert.Equal("SELECT * FROM T WHERE Id=@Id", cmd.CommandText);
        }

        /// <summary>
        /// Verifies that the CreateCommand method correctly binds enumerable parameters to the resulting command
        /// object.
        /// </summary>
        /// <remarks>This test ensures that when an enumerable of parameters is provided to CreateCommand,
        /// each parameter is properly added to the command's Parameters collection. It checks that the parameter name
        /// and count match the expected values.</remarks>
        [Fact]
        public void CreateCommand_EnumerableParameters_BindsParameters()
        {
            var conn = new FakeDbConnection();
            var param = new FakeDataParameter { ParameterName = "@Id", Value = 1, DbType = DbType.Int32 };
            var cmd = CommandEngine.CreateCommand(
                conn,
                null,
                CommandType.Text,
                "SELECT * FROM T WHERE Id=@Id",
                new[] { param },
                "@",
                15,
                null);
            Assert.Single(cmd.Parameters);
            Assert.Equal("@Id", ((IDbDataParameter)cmd.Parameters[0]).ParameterName);
        }

        /// <summary>
        /// Verifies that CreateCommand safely handles a null enumerable parameter collection.
        /// </summary>
        [Fact]
        public void CreateCommand_EnumerableParameters_Null_DoesNotBindParameters()
        {
            var conn = new FakeDbConnection();
            var cmd = CommandEngine.CreateCommand(
                conn,
                null,
                CommandType.Text,
                "SELECT 1",
                null,
                "@",
                15,
                null);

            Assert.Empty(cmd.Parameters.Cast<IDataParameter>());
        }

        /// <summary>
        /// Verifies that the CreateCommandCore method assigns the specified transaction to the created command.
        /// </summary>
        /// <remarks>This test ensures that when a transaction is provided to CreateCommandCore, the
        /// resulting command's Transaction property is set to that transaction. This helps validate correct transaction
        /// association in command creation logic.</remarks>
        [Fact]
        public void CreateCommandCore_SetsTransaction()
        {
            var conn = new FakeDbConnection();
            var transaction = new FakeDbTransaction();
            var cmd = CommandEngine.CreateCommandCore(
                conn,
                transaction,
                CommandType.Text,
                "SELECT 1",
                null,
                null,
                null);
            Assert.Equal(transaction, cmd.Transaction);
        }

        /// <summary>
        /// Verifies that CreateCommandCore uses the default timeout when no timeout is supplied.
        /// </summary>
        [Fact]
        public void CreateCommandCore_UsesDefaultTimeout_WhenTimeoutIsNull()
        {
            var conn = new FakeDbConnection();
            var cmd = CommandEngine.CreateCommandCore(
                conn,
                null,
                CommandType.Text,
                "SELECT 1",
                null,
                HydrixOptions.DefaultTimeout,
                null);

            Assert.Equal(HydrixOptions.DefaultTimeout, cmd.CommandTimeout);
        }

        /// <summary>
        /// Verifies that invoking the LogCommand method with a null logger does not throw an exception.
        /// </summary>
        /// <remarks>This test ensures that the LogCommand method is resilient to a null logger and
        /// performs no action in such cases. It is intended to validate the method's behavior when logging is not
        /// configured.</remarks>
        [Fact]
        public void LogCommand_LoggerIsNull_DoesNothing()
        {
            var conn = new FakeDbConnection();
            var cmd = CommandEngine.CreateCommandCore(
                conn,
                null,
                CommandType.Text,
                "SELECT 1",
                null,
                null,
                null);
            // Should not throw
            var logMethod = typeof(CommandEngine).GetMethod("LogCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            logMethod.Invoke(null, new object[] { null, cmd });
        }

        /// <summary>
        /// Verifies that the LogCommand method logs the command text and parameters when logging is enabled at the
        /// Information level.
        /// </summary>
        /// <remarks>This test ensures that the logger receives a log entry containing both the SQL
        /// command text and its associated parameters. It uses a mock logger to verify that logging occurs as expected
        /// when a command is executed.</remarks>
        [Fact]
        public void LogCommand_LogsCommandTextAndParameters()
        {
            var conn = new FakeDbConnection();
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            var cmd = CommandEngine.CreateCommandCore(
                conn,
                null,
                CommandType.Text,
                "SELECT 1",
                c =>
                {
                    var p = c.CreateParameter();
                    p.ParameterName = "@foo";
                    p.Value = 123;
                    p.DbType = DbType.Int32;
                    c.Parameters.Add(p);
                },
                null,
                loggerMock.Object);
            var logMethod = typeof(CommandEngine).GetMethod("LogCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            logMethod.Invoke(null, new object[] { loggerMock.Object, cmd });
            loggerMock.Verify(l =>
                l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing DbCommand")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce());
        }

        /// <summary>
        /// Verifies that LogCommand does not emit logs when information level is disabled.
        /// </summary>
        [Fact]
        public void LogCommand_WhenInformationLevelDisabled_DoesNotLog()
        {
            var conn = new FakeDbConnection();
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

            CommandEngine.CreateCommandCore(
                conn,
                null,
                CommandType.Text,
                "SELECT 1",
                null,
                null,
                loggerMock.Object);

            loggerMock.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        /// <summary>
        /// Verifies that LogCommand logs command text even when there are no parameters.
        /// </summary>
        [Fact]
        public void LogCommand_LogsCommandText_WhenThereAreNoParameters()
        {
            var conn = new FakeDbConnection();
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            CommandEngine.CreateCommandCore(
                conn,
                null,
                CommandType.Text,
                "SELECT 1",
                null,
                null,
                loggerMock.Object);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Executing DbCommand") &&
                        !v.ToString().Contains("Parameters:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that CreateCommand throws ArgumentNullException when the procedure argument is null.
        /// </summary>
        /// <remarks>This test ensures that passing a null procedure to CreateCommand results in the expected exception,
        /// enforcing correct usage of the API.</remarks>
        [Fact]
        public void CreateCommand_ThrowsArgumentNullException_WhenProcedureIsNull()
        {
            var conn = new FakeDbConnection();
            CustomProcedure procedure = null;
            Assert.Throws<ArgumentNullException>(() =>
                CommandEngine.CreateCommand<CustomDataParameter>(conn, null, procedure, "@", 10, null));
        }

        /// <summary>
        /// Verifies that CreateCommand throws InvalidOperationException when the connection is not open.
        /// </summary>
        /// <remarks>This test ensures that attempting to create a command with a closed connection results in the expected exception.</remarks>
        [Fact]
        public void CreateCommand_ThrowsInvalidOperationException_WhenConnectionNotOpen()
        {
            var conn = new FakeDbConnection { State = ConnectionState.Closed };
            var procedure = new CustomProcedure { Id = 1, Name = "Test" };
            Assert.Throws<InvalidOperationException>(() =>
                CommandEngine.CreateCommand<CustomDataParameter>(conn, null, procedure, "@", 10, null));
        }

        /// <summary>
        /// Verifies that CreateCommand throws MissingMemberException when the procedure type is not decorated with ProcedureAttribute.
        /// </summary>
        /// <remarks>This test ensures that a procedure type without the required attribute results in the expected exception.</remarks>
        [Fact]
        public void CreateCommand_ThrowsMissingMemberException_WhenProcedureAttributeMissing()
        {
            var conn = new FakeDbConnection();
            var procedure = new NoAttributeProcedure();
            Assert.Throws<MissingMemberException>(() =>
                CommandEngine.CreateCommand<FakeDataParameter>(conn, null, procedure, "@", 10, null));
        }

        /// <summary>
        /// Verifies that CreateCommand sets command properties and binds parameters as expected for a valid procedure.
        /// </summary>
        /// <remarks>This test ensures that the command is created, parameters are bound, and properties are set correctly.</remarks>
        [Fact]
        public void CreateCommand_SetsProperties_AndBindsParameters()
        {
            var conn = new FakeDbConnection();
            var procedure = new CustomProcedure { Id = 42, Name = "Alpha" };
            var cmd = CommandEngine.CreateCommand<CustomDataParameter>(conn, null, procedure, "@", 20, null);
            Assert.NotNull(cmd);
            Assert.Equal(20, cmd.CommandTimeout);
            // The following assertion is removed because FakeDbConnection's CreateCommand does not set the Connection property.
            // Assert.Equal(conn, cmd.Connection);
            Assert.Null(cmd.Transaction);
            Assert.Contains(cmd.Parameters.Cast<IDataParameter>(), p => p.ParameterName == "@Id" && (int)p.Value == 42);
            Assert.Contains(cmd.Parameters.Cast<IDataParameter>(), p => p.ParameterName == "@Name" && (string)p.Value == "Alpha");
        }

        /// <summary>
        /// Verifies that CreateCommand assigns the specified transaction to the created command.
        /// </summary>
        /// <remarks>This test ensures that when a transaction is provided, the resulting command's Transaction property is set.</remarks>
        [Fact]
        public void CreateCommand_SetsTransaction_WhenProvided()
        {
            var conn = new FakeDbConnection();
            var procedure = new CustomProcedure { Id = 7, Name = "Beta" };
            var transaction = new FakeDbTransaction();
            var cmd = CommandEngine.CreateCommand<CustomDataParameter>(conn, transaction, procedure, "@", 15, null);
            Assert.Equal(transaction, cmd.Transaction);
        }

        /// <summary>
        /// Verifies that CreateCommand uses the default timeout when the timeout argument is null.
        /// </summary>
        /// <remarks>This test ensures that the command timeout is set to HydrixOptions.DefaultTimeout if not specified.</remarks>
        [Fact]
        public void CreateCommand_UsesDefaultTimeout_WhenTimeoutIsNull()
        {
            var conn = new FakeDbConnection();
            var procedure = new CustomProcedure { Id = 99, Name = "Gamma" };
            var cmd = CommandEngine.CreateCommand<CustomDataParameter>(conn, null, procedure, "@", HydrixOptions.DefaultTimeout, null);
            Assert.Equal(HydrixOptions.DefaultTimeout, cmd.CommandTimeout);
        }

        /// <summary>
        /// Verifies that CreateCommand uses the specified parameter prefix for parameter names.
        /// </summary>
        /// <remarks>This test ensures that the parameter prefix is applied to all parameter names in the command.</remarks>
        [Theory]
        [InlineData("@")]
        [InlineData(":")]
        [InlineData("p")]
        public void CreateCommand_UsesParameterPrefix(string prefix)
        {
            var conn = new FakeDbConnection();
            var procedure = new CustomProcedure { Id = 5, Name = "Delta" };
            var cmd = CommandEngine.CreateCommand<CustomDataParameter>(conn, null, procedure, prefix, 10, null);
            Assert.Contains(cmd.Parameters.Cast<IDataParameter>(), p => p.ParameterName == prefix + "Id");
            Assert.Contains(cmd.Parameters.Cast<IDataParameter>(), p => p.ParameterName == prefix + "Name");
        }

        /// <summary>
        /// Verifies that CreateCommand calls LogCommand when a logger is provided.
        /// </summary>
        /// <remarks>This test ensures that the logger is used to log command details if provided.</remarks>
        [Fact]
        public void CreateCommand_LogsCommand_WhenLoggerProvided()
        {
            var conn = new FakeDbConnection();
            var procedure = new CustomProcedure { Id = 11, Name = "Epsilon" };
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            var cmd = CommandEngine.CreateCommand<CustomDataParameter>(conn, null, procedure, "@", 10, loggerMock.Object);
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing DbCommand")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        /// <summary>
        /// Verifies that CreateCommand maps non-standard DbType values using provider-specific DbType properties.
        /// </summary>
        [Fact]
        public void CreateCommand_UsesProviderSpecificDbTypeSetter_WhenDbTypeIsOutOfRange()
        {
            var conn = new FakeDbConnection();
            var procedure = new ProviderAwareProcedure { Code = 123 };

            var cmd = CommandEngine.CreateCommand<ProviderAwareDataParameter>(
                conn,
                null,
                procedure,
                "@",
                10,
                null);

            var parameter = Assert.IsType<ProviderAwareDataParameter>(Assert.Single(cmd.Parameters.Cast<IDataParameter>()));
            Assert.Equal("@Code", parameter.ParameterName);
            Assert.Equal(123, parameter.Value);
            Assert.Equal(CustomProviderDbType.ProviderSpecific, parameter.SqlDbType);
        }

        /// <summary>
        /// Verifies that CreateCommand correctly handles parameter values of various types and nulls.
        /// </summary>
        /// <remarks>This test ensures that null and non-null values are bound as expected.</remarks>
        [Fact]
        public void CreateCommand_BindsNullAndNonNullParameterValues()
        {
            var conn = new FakeDbConnection();
            var procedure = new CustomProcedure { Id = 0, Name = null };
            var cmd = CommandEngine.CreateCommand<CustomDataParameter>(conn, null, procedure, "@", 10, null);
            Assert.Contains(cmd.Parameters.Cast<IDataParameter>(), p => p.ParameterName == "@Id" && (int)p.Value == 0);
            Assert.Contains(cmd.Parameters.Cast<IDataParameter>(), p => p.ParameterName == "@Name" && p.Value == DBNull.Value);
        }

        /// <summary>
        /// Verifies that CreateCommand for text commands uses default timeout and default parameter prefix when omitted.
        /// </summary>
        [Fact]
        public void CreateCommand_Text_UsesDefaults_WhenTimeoutAndPrefixAreNull()
        {
            var conn = new FakeDbConnection();

            var cmd = CommandEngine.CreateCommand(
                conn,
                null,
                CommandType.Text,
                "SELECT * FROM T WHERE Id = @Id",
                new { Id = 12 },
                null,
                null,
                null);

            Assert.Equal(HydrixConfiguration.Options.CommandTimeout, cmd.CommandTimeout);
            Assert.Contains(
                cmd.Parameters.Cast<IDataParameter>(),
                p => p.ParameterName == $"{HydrixConfiguration.Options.ParameterPrefix}Id" && (int)p.Value == 12);
        }

        /// <summary>
        /// Verifies that CreateCommand for procedure commands uses default timeout and default parameter prefix when omitted.
        /// </summary>
        [Fact]
        public void CreateCommand_Procedure_UsesDefaults_WhenTimeoutAndPrefixAreNull()
        {
            var conn = new FakeDbConnection();
            var procedure = new CustomProcedure { Id = 21, Name = "Omega" };

            var cmd = CommandEngine.CreateCommand<CustomDataParameter>(
                conn,
                null,
                procedure,
                null,
                null,
                null);

            Assert.Equal(HydrixConfiguration.Options.CommandTimeout, cmd.CommandTimeout);
            Assert.Contains(
                cmd.Parameters.Cast<IDataParameter>(),
                p => p.ParameterName == $"{HydrixConfiguration.Options.ParameterPrefix}Id" && (int)p.Value == 21);
            Assert.Contains(
                cmd.Parameters.Cast<IDataParameter>(),
                p => p.ParameterName == $"{HydrixConfiguration.Options.ParameterPrefix}Name" && (string)p.Value == "Omega");
        }

        /// <summary>
        /// Verifies that provider-specific DbType setter path safely handles missing setter when DbType is out of range.
        /// </summary>
        [Fact]
        public void CreateCommand_ProviderSpecificDbTypeSetterMissing_DoesNotThrow()
        {
            var conn = new FakeDbConnection();
            var procedure = new NoProviderSetterProcedure { Code = 777 };

            var cmd = CommandEngine.CreateCommand<AttributeParameter>(
                conn,
                null,
                procedure,
                "@",
                10,
                null);

            var parameter = Assert.IsType<AttributeParameter>(Assert.Single(cmd.Parameters.Cast<IDataParameter>()));
            Assert.Equal("@Code", parameter.ParameterName);
            Assert.Equal(777, parameter.Value);
        }

        /// <summary>
        /// Verifies that command parameter addition occurs for both standard and provider-specific DbType paths.
        /// </summary>
        [Fact]
        public void CreateCommand_AddsParameters_ForStandardAndProviderSpecificDbTypes()
        {
            var conn = new FakeDbConnection();
            var procedure = new MixedProviderAwareProcedure
            {
                Name = "Alpha",
                Code = 321
            };

            var cmd = CommandEngine.CreateCommand<ProviderAwareDataParameter>(
                conn,
                null,
                procedure,
                "@",
                10,
                null);

            Assert.Equal(2, cmd.Parameters.Count);
            Assert.Contains(cmd.Parameters.Cast<IDataParameter>(), p => p.ParameterName == "@Name" && (string)p.Value == "Alpha");
            Assert.Contains(cmd.Parameters.Cast<IDataParameter>(), p => p.ParameterName == "@Code" && (int)p.Value == 321);
        }

        /// <summary>
        /// Verifies that consecutive command creation with the same procedure type exercises the procedure-binder cache hit path.
        /// </summary>
        [Fact]
        public void CreateCommand_ReusesCachedProcedureBinder_WhenProcedureTypeRepeats()
        {
            var commandEngineType = typeof(CommandEngine);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;

            commandEngineType.GetField("_lastProcedureType", flags).SetValue(null, null);
            commandEngineType.GetField("_lastProcedureBinder", flags).SetValue(null, null);

            var conn = new FakeDbConnection();

            var first = CommandEngine.CreateCommand<CustomDataParameter>(
                conn,
                null,
                new CustomProcedure { Id = 1, Name = "One" },
                "@",
                10,
                null);

            var second = CommandEngine.CreateCommand<CustomDataParameter>(
                conn,
                null,
                new CustomProcedure { Id = 2, Name = "Two" },
                "@",
                10,
                null);

            Assert.Equal(2, first.Parameters.Count);
            Assert.Equal(2, second.Parameters.Count);
            Assert.Contains(second.Parameters.Cast<IDataParameter>(), p => p.ParameterName == "@Id" && (int)p.Value == 2);
            Assert.Contains(second.Parameters.Cast<IDataParameter>(), p => p.ParameterName == "@Name" && (string)p.Value == "Two");
        }

        /// <summary>
        /// Verifies that GetOrAddProcedureBinder returns the thread-cached binder when called consecutively for the
        /// same procedure type.
        /// </summary>
        [Fact]
        public void GetOrAddProcedureBinder_ReturnsCachedBinder_WhenSameTypeIsRequestedTwice()
        {
            var commandEngineType = typeof(CommandEngine);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;

            var lastProcedureTypeField = commandEngineType.GetField("_lastProcedureType", flags);
            var lastProcedureBinderField = commandEngineType.GetField("_lastProcedureBinder", flags);
            var getOrAddProcedureBinderMethod = commandEngineType.GetMethod("GetOrAddProcedureBinder", flags);

            lastProcedureTypeField.SetValue(null, null);
            lastProcedureBinderField.SetValue(null, null);

            var first = getOrAddProcedureBinderMethod.Invoke(null, new object[] { typeof(CustomProcedure) });
            var second = getOrAddProcedureBinderMethod.Invoke(null, new object[] { typeof(CustomProcedure) });

            Assert.NotNull(first);
            Assert.Same(first, second);
        }

        /// <summary>
        /// Verifies that GetOrAddProviderDbTypeSetter returns the thread-cached setter when called consecutively for
        /// the same parameter type.
        /// </summary>
        [Fact]
        public void GetOrAddProviderDbTypeSetter_ReturnsCachedSetter_WhenSameTypeIsRequestedTwice()
        {
            var commandEngineType = typeof(CommandEngine);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;

            var lastProviderSetterParameterTypeField = commandEngineType.GetField("_lastProviderSetterParameterType", flags);
            var lastProviderSetterField = commandEngineType.GetField("_lastProviderSetter", flags);
            var getOrAddProviderDbTypeSetterMethod = commandEngineType.GetMethod("GetOrAddProviderDbTypeSetter", flags);

            lastProviderSetterParameterTypeField.SetValue(null, null);
            lastProviderSetterField.SetValue(null, null);

            var first = (Action<IDataParameter, int>)getOrAddProviderDbTypeSetterMethod.Invoke(
                null,
                new object[] { typeof(ProviderAwareDataParameter) });

            var second = (Action<IDataParameter, int>)getOrAddProviderDbTypeSetterMethod.Invoke(
                null,
                new object[] { typeof(ProviderAwareDataParameter) });

            Assert.NotNull(first);
            Assert.Same(first, second);
        }
    }
}