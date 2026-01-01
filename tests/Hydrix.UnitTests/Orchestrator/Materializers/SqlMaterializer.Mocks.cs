using Hydrix.Attributes.Parameters;
using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Materializers;
using Hydrix.Schemas;
using Microsoft.Data.SqlClient;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
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
        /// Represents a non-functional, placeholder implementation of the IDbConnection interface for testing or
        /// design-time scenarios.
        /// </summary>
        /// <remarks>This class simulates a database connection without providing any real connectivity or
        /// data operations. All methods and properties return default or fixed values, and no actual database actions
        /// are performed. Use this class when a mock or stub implementation of IDbConnection is required, such as in
        /// unit tests or design-time environments.</remarks>
        private class DummyDbConnection : IDbConnection
        {
            /// <summary>
            /// Gets or sets the connection string used to establish a connection to the data source.
            /// </summary>
            public string ConnectionString { get; set; }

            /// <summary>
            /// Gets the time, in seconds, to wait while trying to establish a connection before terminating the attempt
            /// and generating an error.
            /// </summary>
            public int ConnectionTimeout => 0;

            /// <summary>
            /// Gets the name of the database associated with the current connection.
            /// </summary>
            public string Database => "Dummy";

            /// <summary>
            /// Gets the current state of the connection.
            /// </summary>
            public ConnectionState State => ConnectionState.Closed;

            /// <summary>
            /// Changes the current database for an open connection to the database specified by name.
            /// </summary>
            /// <remarks>The connection must be open before calling this method. The method does not
            /// validate whether the specified database exists; an exception may be thrown if the database name is
            /// invalid or inaccessible.</remarks>
            /// <param name="databaseName">The name of the database to use for the current connection. Cannot be null, empty, or contain only
            /// whitespace.</param>
            public void ChangeDatabase(string databaseName)
            { }

            /// <summary>
            /// Closes the current resource and releases any associated resources.
            /// </summary>
            public void Close()
            { }

            /// <summary>
            /// Creates and returns a new command associated with the current database connection.
            /// </summary>
            /// <returns>An <see cref="IDbCommand"/> object that can be used to execute queries or commands against the data
            /// source.</returns>
            public IDbCommand CreateCommand() => null;

            /// <summary>
            /// Opens the resource or connection for use.
            /// </summary>
            public void Open()
            { }

            /// <summary>
            /// Releases all resources used by the current instance.
            /// </summary>
            /// <remarks>Call this method when you are finished using the object to free unmanaged
            /// resources and perform other cleanup operations. After calling Dispose, the object should not be
            /// used.</remarks>
            public void Dispose()
            { }

            /// <summary>
            /// Begins a database transaction.
            /// </summary>
            /// <returns>An object representing the new database transaction.</returns>
            /// <exception cref="NotImplementedException">The method is not implemented.</exception>
            public IDbTransaction BeginTransaction() => throw new NotImplementedException();

            /// <summary>
            /// Begins a database transaction with the specified isolation level.
            /// </summary>
            /// <param name="il">The isolation level under which the transaction should operate. Determines the locking and row
            /// versioning behavior for the transaction.</param>
            /// <returns>An object representing the new database transaction. The caller is responsible for committing or rolling
            /// back the transaction.</returns>
            /// <exception cref="NotImplementedException">The method is not implemented.</exception>
            public IDbTransaction BeginTransaction(IsolationLevel il) => throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new instance of the SqlMaterializer class using its parameterless constructor.
        /// </summary>
        /// <returns>A new instance of SqlMaterializer.</returns>
        private static SqlMaterializer CreateInstance(
            int? timeout = null,
            string parameterPrefix = null)
        {
            var ctor = typeof(SqlMaterializer).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new Type[] { typeof(IDbConnection), typeof(int), typeof(string) },
                null);

            Assert.NotNull(ctor);
            return (SqlMaterializer)ctor.Invoke(new object[] {
                null,
                timeout ?? 60,
                parameterPrefix ?? ":"
            });
        }

        /// <summary>
        /// Creates a new instance of the SqlMaterializer class with the specified database connection, transaction,
        /// timeout, and disposal state.
        /// </summary>
        /// <param name="dbConnection">The database connection to associate with the materializer, or null to leave uninitialized.</param>
        /// <param name="dbTransaction">The database transaction to associate with the materializer, or null if no transaction is required.</param>
        /// <param name="timeout">The command timeout, in seconds, to use for database operations. Must be a non-negative value.</param>
        /// <param name="isDisposed">true if the materializer should be marked as disposed; otherwise, false.</param>
        /// <param name="isDisposing">true if the materializer should be marked as disposing; otherwise, false.</param>
        /// <returns>A new SqlMaterializer instance configured with the specified connection, transaction, timeout, and disposal
        /// state.</returns>
        private SqlMaterializer CreateMaterializer(
            IDbConnection dbConnection = null,
            IDbTransaction dbTransaction = null,
            int timeout = 30,
            bool isDisposed = false,
            bool isDisposing = false)
        {
            var mat = CreateInstance();

            // Set private fields via reflection
            typeof(SqlMaterializer).GetField("_dbConnection", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(mat, dbConnection);
            typeof(SqlMaterializer).GetField("_dbTransaction", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(mat, dbTransaction);
            typeof(SqlMaterializer).GetField("_timeout", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(mat, timeout);
            typeof(SqlMaterializer).GetField("_lockConnection", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(mat, new object());
            typeof(SqlMaterializer).GetField("_lockTransaction", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(mat, new object());
            typeof(SqlMaterializer).GetProperty("IsDisposed", BindingFlags.Instance | BindingFlags.Public)
                .SetValue(mat, isDisposed);
            typeof(SqlMaterializer).GetProperty("IsDisposing", BindingFlags.Instance | BindingFlags.Public)
                .SetValue(mat, isDisposing);

            return mat;
        }

        /// <summary>
        /// Retrieves the private parameter prefix used by the specified SqlMaterializer instance.
        /// </summary>
        /// <remarks>This method accesses a non-public member of the SqlMaterializer class. It is intended
        /// for advanced scenarios where direct access to the internal parameter prefix is required.</remarks>
        /// <param name="materializer">The SqlMaterializer instance from which to obtain the parameter prefix. Cannot be null.</param>
        /// <returns>A string containing the parameter prefix used internally by the specified SqlMaterializer instance.</returns>
        private static string GetPrivateParameterPrefix(SqlMaterializer materializer)
        {
            var field = typeof(SqlMaterializer).GetField("_parameterPrefix", BindingFlags.Instance | BindingFlags.NonPublic);
            return (string)field.GetValue(materializer);
        }

        /// <summary>
        /// Represents a test SQL stored procedure definition for use with the ISqlProcedure interface and
        /// FakeDataParameter parameters.
        /// </summary>
        /// <remarks>This class is intended for testing scenarios and provides metadata for the
        /// 'dbo.TestProc' stored procedure, including its parameter definitions. It is not intended for production
        /// use.</remarks>
        [SqlProcedure("dbo", "TestProc")]
        private class TestSqlProcedure :
            ISqlProcedure<FakeDataParameter>
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [SqlParameter("Id", DbType.Int32)]
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents an entity mapped to a SQL table with fields for identifier and name.
        /// </summary>
        [SqlEntity]
        private class TestEntity : ISqlEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [SqlField("Id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with the entity.
            /// </summary>
            [SqlField("Name")]
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents a SQL entity with an integer identifier.
        /// </summary>
        [SqlEntity]
        private class NoFieldEntity : ISqlEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents an entity without additional attributes for use with SQL mapping.
        /// </summary>
        /// <remarks>This class is intended for scenarios where an entity requires only the default SQL
        /// mapping behavior provided by the ISqlEntity interface. It does not define any custom attributes beyond the
        /// required fields.</remarks>
        private class NoAttributeEntity : ISqlEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [SqlField]
            public int Id { get; set; }
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

        /// <summary>
        /// Provides a testable subclass of <see cref="SqlMaterializer"/> for unit testing purposes, allowing controlled
        /// access to internal state and behavior.
        /// </summary>
        /// <remarks>This class exposes setters for internal properties and fields of <see
        /// cref="SqlMaterializer"/> to facilitate testing scenarios. It should only be used in test contexts and is not
        /// intended for production use.</remarks>
        private class SqlMaterializerTestable :
            SqlMaterializer
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
        /// Represents a test implementation of a database connection for use in unit tests or scenarios where a real
        /// database connection is not required.
        /// </summary>
        /// <remarks>This class provides a minimal, non-functional implementation of the DbConnection
        /// abstract class. All members return default or empty values, and no actual database operations are performed.
        /// Use this class to mock or stub database connections in testing environments without connecting to a real
        /// database.</remarks>
        private class TestDbConnection :
            DbConnection
        {
            /// <summary>
            /// Gets a value indicating whether the object has been disposed.
            /// </summary>
            /// <remarks>Use this property to determine if the object is no longer usable due to
            /// disposal. Once disposed, further operations on the object may throw exceptions or have no
            /// effect.</remarks>
            public new bool Disposed { get; private set; }

            /// <summary>
            /// Gets or sets the string used to open a database connection.
            /// </summary>
            public override string ConnectionString { get; set; }

            /// <summary>
            /// Gets the name of the current database for the connection.
            /// </summary>
            public override string Database => string.Empty;

            /// <summary>
            /// Gets the name of the data source associated with the connection.
            /// </summary>
            public override string DataSource => string.Empty;

            /// <summary>
            /// Gets a string that represents the version of the database server to which the connection is established.
            /// </summary>
            public override string ServerVersion => string.Empty;

            /// <summary>
            /// Gets the current state of the connection.
            /// </summary>
            public override ConnectionState State => ConnectionState.Closed;

            /// <summary>
            /// Begins a database transaction with the specified isolation level for the underlying data source.
            /// </summary>
            /// <remarks>Override this method in a derived class to provide transaction support for a
            /// custom database provider. The default implementation may not support transactions and can return
            /// null.</remarks>
            /// <param name="isolationLevel">The isolation level under which the transaction should run. Determines the locking and row versioning
            /// behavior for the transaction.</param>
            /// <returns>A <see cref="DbTransaction"/> object representing the new transaction. The specific implementation may
            /// return null if transactions are not supported.</returns>
            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => null;

            /// <summary>
            /// Changes the current database for an open connection to the database specified by name.
            /// </summary>
            /// <remarks>The connection must be open before calling this method. The behavior of this
            /// method may vary depending on the underlying data provider. If the specified database does not exist or
            /// cannot be accessed, an exception may be thrown.</remarks>
            /// <param name="databaseName">The name of the database to use in place of the current database. Cannot be null, empty, or contain only
            /// whitespace.</param>
            public override void ChangeDatabase(string databaseName)
            { }

            /// <summary>
            /// Closes the current stream and releases any resources associated with it.
            /// </summary>
            /// <remarks>After calling this method, attempts to access the stream may result in an
            /// exception. This method is typically called when the stream is no longer needed to ensure that all
            /// resources are properly released.</remarks>
            public override void Close()
            { }

            /// <summary>
            /// Opens the connection to the underlying data source.
            /// </summary>
            /// <remarks>If the connection is already open, calling this method has no effect. This
            /// method must be called before executing commands that require an open connection.</remarks>
            public override void Open()
            { }

            /// <summary>
            /// Creates and returns a new instance of the database command associated with the current connection.
            /// </summary>
            /// <returns>A <see cref="DbCommand"/> object representing the database command for the connection, or
            /// <see langword="null"/> if no command can be created.</returns>
            protected override DbCommand CreateDbCommand() => null;

            /// <summary>
            /// Releases the unmanaged resources used by the object and optionally releases the managed resources.
            /// </summary>
            /// <remarks>This method is called by both the public Dispose() method and the finalizer.
            /// When disposing is true, this method can dispose managed resources in addition to unmanaged resources.
            /// Override this method to provide custom cleanup logic for derived classes.</remarks>
            /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
            protected override void Dispose(bool disposing) => Disposed = true;
        }

        /// <summary>
        /// Provides a test implementation of the SqlMaterializer class for use in unit tests and test scenarios.
        /// </summary>
        /// <remarks>TestSqlMaterializer exposes additional properties and methods to facilitate testing
        /// of transaction rollback and connection management behaviors. It allows inspection and manipulation of
        /// internal state relevant to connection and transaction handling.</remarks>
        private class TestSqlMaterializerDispose :
            SqlMaterializer
        {
            /// <summary>
            /// Gets or sets a value indicating whether the rollback operation has been called.
            /// </summary>
            public bool RollbackCalled { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the Close method has been called.
            /// </summary>
            public bool CloseCalled { get; set; }

            /// <summary>
            /// Gets the synchronization object used to coordinate access to the underlying database connection.
            /// </summary>
            /// <remarks>This object can be used to implement thread-safe operations involving the
            /// connection. The returned object is intended for internal synchronization and should not be modified or
            /// replaced.</remarks>
            public object LockConnection => typeof(SqlMaterializer).GetField("_lockConnection", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(this);

            /// <summary>
            /// Initializes a new instance of the TestSqlMaterializer class for use in testing scenarios.
            /// </summary>
            /// <remarks>This constructor configures the base SqlMaterializer with test-specific
            /// dependencies, allowing for isolated unit testing without requiring a real database connection. It is
            /// intended for use in test environments only.</remarks>
            public TestSqlMaterializerDispose() : base(null)
            {
                typeof(SqlMaterializer).GetProperty("DbConnection", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(this, new TestDbConnection());
                typeof(SqlMaterializer).GetField("_lockConnection", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(this, new object());
                typeof(SqlMaterializer).GetProperty("IsDisposed", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(this, false);
                typeof(SqlMaterializer).GetProperty("IsDisposing", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(this, false);
            }

            /// <summary>
            /// Rolls back the current transaction, undoing all changes made since the transaction began.
            /// </summary>
            public override void RollbackTransaction()
            {
                base.RollbackTransaction();
                RollbackCalled = true;
            }

            /// <summary>
            /// Closes the current connection and marks it as closed.
            /// </summary>
            /// <remarks>After calling this method, the connection is considered closed and cannot be
            /// used for further operations until it is reopened. This method is virtual and can be overridden in a
            /// derived class to provide custom close behavior.</remarks>
            public override void CloseConnection()
            {
                base.CloseConnection();
                CloseCalled = true;
            }

            /// <summary>
            /// Sets the underlying database connection to null for this instance.
            /// </summary>
            /// <remarks>This method is intended for advanced scenarios where direct manipulation of
            /// the internal database connection is required. Use with caution, as setting the connection to null may
            /// render the instance unusable for further database operations until a new connection is
            /// assigned.</remarks>
            public void SetDbConnectionNull()
                => typeof(SqlMaterializer).GetProperty("DbConnection", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(this, null);

            /// <summary>
            /// Sets the database connection to be used by this instance.
            /// </summary>
            /// <remarks>This method is intended for advanced scenarios where direct control over the
            /// underlying database connection is required. Changing the connection may affect the state of ongoing
            /// operations.</remarks>
            /// <param name="conn">The database connection to associate with this instance. Cannot be null.</param>
            public void SetDbConnection(IDbConnection conn)
                => typeof(SqlMaterializer).GetProperty("DbConnection", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(this, conn);

            /// <summary>
            /// Associates the specified database transaction with the current materializer instance.
            /// </summary>
            /// <remarks>Use this method to ensure that database operations performed by this
            /// materializer participate in the specified transaction. The transaction must be compatible with the
            /// underlying database connection.</remarks>
            /// <param name="tran">The database transaction to associate with this materializer. Can be null to clear the current
            /// transaction.</param>
            public void SetDbTransaction(IDbTransaction tran)
                => typeof(SqlMaterializer).GetProperty("DbTransaction", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(this, tran);

            /// <summary>
            /// Sets the disposed state of the current instance.
            /// </summary>
            /// <remarks>This method is intended for advanced scenarios where manual control of the
            /// disposed state is required. Improper use may lead to inconsistent object state or resource
            /// leaks.</remarks>
            /// <param name="value">true to mark the instance as disposed; otherwise, false.</param>
            public void SetDisposed(bool value)
                => typeof(SqlMaterializer).GetProperty("IsDisposed")?.SetValue(this, value);

            /// <summary>
            /// Sets the disposing state of the current instance.
            /// </summary>
            /// <remarks>This method is intended for advanced scenarios where manual control of the
            /// disposing state is required. Changing the disposing state may affect resource cleanup and object
            /// lifecycle management.</remarks>
            /// <param name="value">true to indicate that the instance is disposing; otherwise, false.</param>
            public void SetDisposing(bool value)
                => typeof(SqlMaterializer).GetProperty("IsDisposing")?.SetValue(this, value);
        }

        /// <summary>
        /// A test implementation of the SQL materializer that throws exceptions when certain operations are performed.
        /// </summary>
        /// <remarks>This class is intended for use in testing scenarios where it is necessary to simulate
        /// failures during transaction rollback or connection closure. It overrides specific methods to throw
        /// exceptions after invoking the base implementation, allowing tests to verify error handling logic.</remarks>
        private class ExceptionThrowingSqlMaterializer :
            TestSqlMaterializerDispose
        {
            /// <summary>
            /// Rolls back the current transaction.
            /// </summary>
            /// <exception cref="Exception">Thrown to simulate a rollback failure.</exception>
            public override void RollbackTransaction()
            {
                base.RollbackTransaction();
                throw new Exception("Simulated rollback exception");
            }

            /// <summary>
            /// Closes the current connection and releases any associated resources.
            /// </summary>
            /// <exception cref="Exception">Thrown to simulate a failure when closing the connection.</exception>
            public override void CloseConnection()
            {
                base.CloseConnection();
                throw new Exception("Simulated close exception");
            }
        }

        /// <summary>
        /// Provides test access to internal parameter-related functionality of the SqlMaterializer class.
        /// </summary>
        /// <remarks>This class is intended for testing scenarios where direct invocation of internal
        /// methods of SqlMaterializer is required. It exposes static and instance methods that allow tests to interact
        /// with parameter handling logic, such as adding parameters to commands or formatting parameter values. This
        /// class should not be used in production code.</remarks>
        private class TestSqlMaterializerParameter :
            SqlMaterializer
        {
            /// <summary>
            /// Initializes a new instance of the TestSqlMaterializerParameter class with the specified parameter
            /// prefix.
            /// </summary>
            /// <remarks>Use this constructor to customize the prefix used for SQL parameters when
            /// materializing queries. The prefix is applied to all parameters generated by this instance.</remarks>
            /// <param name="prefix">The string to use as the prefix for SQL parameters. Defaults to "@" if not specified.</param>
            public TestSqlMaterializerParameter(string prefix = "@") :
                base(null, parameterPrefix: prefix)
            {
                typeof(SqlMaterializer)
                    .GetField("_parameterPrefix", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.SetValue(this, prefix);
            }

            /// <summary>
            /// Determines whether the specified value is considered an enumerable parameter for SQL materialization
            /// purposes.
            /// </summary>
            /// <remarks>This method is intended for advanced scenarios where it is necessary to check
            /// if a value will be treated as an enumerable parameter by the SQL materializer. The criteria for what
            /// constitutes an enumerable parameter are defined internally and may change in future versions.</remarks>
            /// <param name="value">The value to evaluate as a potential enumerable parameter. Can be any object, including null.</param>
            /// <returns>true if the value is recognized as an enumerable parameter; otherwise, false.</returns>
            public static bool CallIsEnumerableParameter(object value) =>
                typeof(SqlMaterializer)
                    .GetMethod("IsEnumerableParameter", BindingFlags.Static | BindingFlags.NonPublic)
                    .Invoke(null, new[] { value }) is bool b && b;

            /// <summary>
            /// Adds a scalar parameter with the specified name and value to the given database command.
            /// </summary>
            /// <param name="cmd">The database command to which the parameter will be added. Must not be null.</param>
            /// <param name="name">The name of the parameter to add. Cannot be null or empty.</param>
            /// <param name="value">The value to assign to the parameter. May be null to represent a database null value.</param>
            public void CallAddScalarParameter(IDbCommand cmd, string name, object value) =>
                typeof(SqlMaterializer)
                    .GetMethod("AddScalarParameter", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(this, new object[] { cmd, name, value });

            /// <summary>
            /// Expands an enumerable parameter for use in a database command, enabling the command to handle a
            /// collection of values as a single parameter.
            /// </summary>
            /// <remarks>This method is typically used to support SQL 'IN' clauses or similar
            /// scenarios where a parameterized query needs to accept multiple values. The command object is modified to
            /// include the expanded parameters as required by the underlying database provider.</remarks>
            /// <param name="cmd">The database command to which the enumerable parameter will be added. Must not be null.</param>
            /// <param name="name">The name of the parameter to expand within the command. Cannot be null or empty.</param>
            /// <param name="values">The collection of values to be expanded into the parameter. Can be any enumerable; must not be null.</param>
            public void CallExpandEnumerableParameter(IDbCommand cmd, string name, IEnumerable values) =>
                typeof(SqlMaterializer)
                    .GetMethod("ExpandEnumerableParameter", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(this, new object[] { cmd, name, values });

            /// <summary>
            /// Adds a parameter with the specified name and value to the given database command.
            /// </summary>
            /// <param name="cmd">The database command to which the parameter will be added. Cannot be null.</param>
            /// <param name="name">The name of the parameter to add. Cannot be null or empty.</param>
            /// <param name="value">The value to assign to the parameter. May be null to represent a database null value.</param>
            public void CallAddParameter(IDbCommand cmd, string name, object value) =>
                typeof(SqlMaterializer)
                    .GetMethod("AddParameter", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(this, new object[] { cmd, name, value });

            /// <summary>
            /// Binds the specified parameters object to the given database command by mapping its properties to command
            /// parameters.
            /// </summary>
            /// <remarks>This method uses reflection to map the properties of the parameters object to
            /// the parameters of the database command. Property names should match the expected parameter names of the
            /// command. If a property value is null, the corresponding parameter will be set to DBNull.Value.</remarks>
            /// <param name="cmd">The database command to which parameters will be bound. Must not be null.</param>
            /// <param name="parameters">An object whose public properties represent the parameter names and values to bind to the command. Can
            /// be null if no parameters are required.</param>
            public void CallBindParametersFromObject(IDbCommand cmd, object parameters) =>
                typeof(SqlMaterializer)
                    .GetMethod("BindParametersFromObject", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(this, new object[] { cmd, parameters });

            /// <summary>
            /// Formats the specified value as a string suitable for use as a SQL parameter.
            /// </summary>
            /// <remarks>This method is intended for advanced scenarios where direct access to the
            /// internal SQL parameter formatting logic is required. It uses reflection to invoke a non-public method
            /// and may be subject to change in future versions. Use with caution.</remarks>
            /// <param name="value">The value to format for SQL parameterization. Can be null.</param>
            /// <returns>A string representation of the value formatted for SQL parameter usage, or null if the value cannot be
            /// formatted.</returns>
            public static string CallFormatParameterValue(object value) =>
                typeof(SqlMaterializer)
                    .GetMethod("FormatParameterValue", BindingFlags.Static | BindingFlags.NonPublic)
                    .Invoke(null, new[] { value }) as string;
        }

        /// <summary>
        /// Represents a parameter to a command object, such as a SQL query or stored procedure, for use with a mock
        /// database implementation.
        /// </summary>
        /// <remarks>This class implements the IDbDataParameter interface to provide parameter information
        /// for database commands in testing or mock scenarios. It is not intended for use with actual database
        /// connections.</remarks>
        private class MockDbParameter :
            IDbDataParameter
        {
            /// <summary>
            /// Gets or sets the name of the parameter associated with this instance.
            /// </summary>
            public string ParameterName { get; set; }

            /// <summary>
            /// Gets or sets the value associated with this instance.
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// Gets or sets the database type of the parameter.
            /// </summary>
            public DbType DbType { get; set; }

            /// <summary>
            /// Gets or sets the direction of the parameter within a command or stored procedure.
            /// </summary>
            /// <remarks>Use this property to specify whether the parameter is an input, output,
            /// bidirectional, or a return value parameter. The default is typically Input. Setting the correct
            /// direction is important for commands that expect output or return values from stored
            /// procedures.</remarks>
            public ParameterDirection Direction { get; set; }

            /// <summary>
            /// Gets a value indicating whether the current type allows null values.
            /// </summary>
            public bool IsNullable => false;

            /// <summary>
            /// Gets or sets the name of the source column mapped to the data field.
            /// </summary>
            public string SourceColumn { get; set; }

            /// <summary>
            /// Gets or sets the version of data in a DataRow to use when loading parameter values.
            /// </summary>
            /// <remarks>This property determines which version of the DataRow's data is used when
            /// retrieving parameter values, such as Original, Current, or Proposed. It is commonly used in data access
            /// scenarios where parameters are populated from DataRow objects, for example, during database updates or
            /// inserts.</remarks>
            public DataRowVersion SourceVersion { get; set; }

            /// <summary>
            /// Gets or sets the number of significant digits used for numeric values.
            /// </summary>
            public byte Precision { get; set; }

            /// <summary>
            /// Gets or sets the number of decimal places to which a value is scaled.
            /// </summary>
            public byte Scale { get; set; }

            /// <summary>
            /// Gets or sets the size value.
            /// </summary>
            public int Size { get; set; }
        }

        /// <summary>
        /// Represents a collection of database parameters for a command, supporting access by parameter name and index.
        /// </summary>
        /// <remarks>This collection is intended for use with mock or test implementations of database
        /// commands that require parameter management. It implements both IList and IDataParameterCollection
        /// interfaces, allowing parameters to be added, removed, or accessed by name or ordinal position. The
        /// collection is not thread-safe.</remarks>
        private class MockDataParameterCollection :
            List<IDbDataParameter>, IDataParameterCollection
        {
            /// <summary>
            /// Gets or sets the parameter with the specified name.
            /// </summary>
            /// <param name="parameterName">The name of the parameter to retrieve or set. The comparison is typically case-sensitive. Cannot be
            /// null.</param>
            /// <returns>The parameter object associated with the specified name, or null if no parameter with that name exists.</returns>
            public object this[string parameterName]
            {
                get => this.FirstOrDefault(p => p.ParameterName == parameterName);
                set { }
            }

            /// <summary>
            /// Adds an object to the collection as a parameter and returns the index at which the parameter was added.
            /// </summary>
            /// <param name="value">The parameter object to add to the collection. Must implement the IDbDataParameter interface. Cannot be
            /// null.</param>
            /// <returns>The zero-based index at which the parameter was added to the collection.</returns>
            public int Add(object value)
            {
                base.Add((IDbDataParameter)value);
                return Count - 1;
            }

            /// <summary>
            /// Determines whether a parameter with the specified name exists in the collection.
            /// </summary>
            /// <param name="parameterName">The name of the parameter to locate. The comparison is case-sensitive.</param>
            /// <returns>true if a parameter with the specified name exists in the collection; otherwise, false.</returns>
            public bool Contains(string parameterName)
                => this.Any(p => p.ParameterName == parameterName);

            /// <summary>
            /// Returns the zero-based index of the parameter with the specified name.
            /// </summary>
            /// <param name="parameterName">The name of the parameter to locate. The comparison is case-sensitive.</param>
            /// <returns>The zero-based index of the parameter if found; otherwise, -1.</returns>
            public int IndexOf(string parameterName)
                => this.FindIndex(p => p.ParameterName == parameterName);

            /// <summary>
            /// Removes the parameter with the specified name from the collection.
            /// </summary>
            /// <remarks>If the parameter with the specified name does not exist in the collection, no
            /// action is taken.</remarks>
            /// <param name="parameterName">The name of the parameter to remove. The comparison is typically case-sensitive, depending on the
            /// implementation.</param>
            public void RemoveAt(string parameterName)
            {
                var idx = IndexOf(parameterName);
                if (idx >= 0) RemoveAt(idx);
            }

            /// <summary>
            /// Determines whether the collection contains a parameter with the specified value.
            /// </summary>
            /// <param name="value">The parameter to locate in the collection. Must be an object that implements the IDbDataParameter
            /// interface.</param>
            /// <returns>true if the parameter is found in the collection; otherwise, false.</returns>
            public bool Contains(object value)
                => base.Contains((IDbDataParameter)value);

            /// <summary>
            /// Returns the zero-based index of the first occurrence of the specified parameter in the collection.
            /// </summary>
            /// <param name="value">The parameter to locate in the collection. Must be an object that implements the IDbDataParameter
            /// interface.</param>
            /// <returns>The zero-based index of the first occurrence of the specified parameter in the collection; otherwise, -1
            /// if the parameter is not found.</returns>
            public int IndexOf(object value)
                => base.IndexOf((IDbDataParameter)value);

            /// <summary>
            /// Inserts an object into the collection at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index at which the value should be inserted. Must be greater than or equal to 0 and less
            /// than or equal to the number of items in the collection.</param>
            /// <param name="value">The object to insert into the collection. Must implement the IDbDataParameter interface and cannot be
            /// null.</param>
            public void Insert(int index, object value)
                => base.Insert(index, (IDbDataParameter)value);

            /// <summary>
            /// Removes the specified parameter from the collection.
            /// </summary>
            /// <remarks>If the specified object is not found in the collection, no action is taken.
            /// The method expects the parameter to be of type <see cref="IDbDataParameter"/>; passing an object of a
            /// different type will result in an exception.</remarks>
            /// <param name="value">The parameter object to remove from the collection. Must be an instance of <see
            /// cref="IDbDataParameter"/>.</param>
            public void Remove(object value)
                => base.Remove((IDbDataParameter)value);

            /// <summary>
            /// Copies the elements of the collection to a specified array, starting at a particular index.
            /// </summary>
            /// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array
            /// must have zero-based indexing.</param>
            /// <param name="index">The zero-based index in the destination array at which copying begins.</param>
            public void CopyTo(Array array, int index)
                => ((ICollection)this).CopyTo(array, index);

            /// <summary>
            /// Gets a value indicating whether access to the collection is synchronized (thread safe).
            /// </summary>
            /// <remarks>This property always returns <see langword="false"/>. Access to the
            /// collection is not synchronized; callers must implement their own synchronization if thread safety is
            /// required.</remarks>
            public bool IsSynchronized => false;

            /// <summary>
            /// Gets an object that can be used to synchronize access to the collection.
            /// </summary>
            /// <remarks>Use this object to lock the collection during multithreaded operations to
            /// ensure thread safety. Synchronizing access using the SyncRoot property is necessary when multiple
            /// threads might access the collection concurrently.</remarks>
            public object SyncRoot => this;

            /// <summary>
            /// Gets a value indicating whether the collection has a fixed size.
            /// </summary>
            public bool IsFixedSize => false;

            /// <summary>
            /// Gets a value indicating whether the collection is read-only.
            /// </summary>
            public bool IsReadOnly => false;
        }

        /// <summary>
        /// Provides a mock implementation of the <see cref="IDbCommand"/> interface for use in testing database-related
        /// code without requiring a real database connection.
        /// </summary>
        /// <remarks>This class is intended for use in unit tests or scenarios where a lightweight,
        /// non-functional database command object is needed. All methods and properties are implemented with minimal or
        /// no behavior, and do not interact with an actual database. Return values are typically default values or
        /// null, and no exceptions are thrown for unsupported operations.</remarks>
        private class MockDbCommand :
            IDbCommand
        {
            /// <summary>
            /// Gets or sets the SQL statement or command to execute against the data source.
            /// </summary>
            public string CommandText { get; set; }

            /// <summary>
            /// Gets the collection of parameters associated with the command.
            /// </summary>
            /// <remarks>Use this collection to add, remove, or access parameters that are sent to the
            /// data source with the command. The collection is empty if no parameters have been added.</remarks>
            public IDataParameterCollection Parameters { get; } = new MockDataParameterCollection();

            /// <summary>
            /// Creates a new instance of a parameter object for use with a database command.
            /// </summary>
            /// <returns>An <see cref="IDbDataParameter"/> instance that can be used to represent a parameter in a database
            /// command.</returns>
            public IDbDataParameter CreateParameter()
                => new MockDbParameter();

            /// <summary>
            /// Gets or sets the wait time, in seconds, before terminating an attempt to execute a command and
            /// generating an error.
            /// </summary>
            /// <remarks>A value of 0 indicates no limit, and the command will wait indefinitely.
            /// Setting this property to a negative value will throw an exception. The default value is typically 30
            /// seconds, but may vary depending on the provider.</remarks>
            public int CommandTimeout { get; set; }

            /// <summary>
            /// Gets or sets a value indicating how the command string is interpreted by the data provider.
            /// </summary>
            /// <remarks>Set this property to specify whether the command text represents a raw SQL
            /// statement, a stored procedure, or a table name. The default is typically CommandType.Text, which treats
            /// the command as a SQL query. Changing this property may affect how parameters are handled and how the
            /// command is executed by the underlying data provider.</remarks>
            public CommandType CommandType { get; set; }

            /// <summary>
            /// Gets or sets the database connection used to execute commands.
            /// </summary>
            public IDbConnection Connection { get; set; }

            /// <summary>
            /// Executes the command and returns a data reader for reading the results.
            /// </summary>
            /// <returns>An <see cref="IDataReader"/> object that can be used to read the results of the command. The caller is
            /// responsible for closing the data reader when finished.</returns>
            public IDataReader ExecuteReader()
                => null;

            /// <summary>
            /// Executes the command and returns a data reader for reading the results, using the specified command
            /// behavior.
            /// </summary>
            /// <param name="behavior">A bitwise combination of CommandBehavior values that determines how the command results are read and how
            /// the connection is managed.</param>
            /// <returns>An IDataReader object for reading the results of the command. The caller is responsible for closing the
            /// data reader when finished.</returns>
            public IDataReader ExecuteReader(CommandBehavior behavior)
                => null;

            /// <summary>
            /// Executes the query and returns the first column of the first row in the result set.
            /// </summary>
            /// <returns>An object representing the value of the first column of the first row in the result set, or null if the
            /// result set is empty.</returns>
            public object ExecuteScalar()
                => null;

            /// <summary>
            /// Requests cancellation of the current operation, if one is in progress.
            /// </summary>
            /// <remarks>Calling this method signals that the ongoing operation should be canceled as
            /// soon as possible. The exact timing and effect of cancellation depend on the implementation. If no
            /// operation is in progress, this method has no effect.</remarks>
            public void Cancel()
            { }

            /// <summary>
            /// Gets an array containing all parameters as <see cref="IDbDataParameter"/> objects.
            /// </summary>
            public IDbDataParameter[] ParametersArray
                => Parameters.Cast<IDbDataParameter>().ToArray();

            /// <summary>
            /// Gets or sets the database transaction to be used for executing commands.
            /// </summary>
            /// <remarks>Set this property to associate database operations with a specific
            /// transaction. If not set, commands will execute outside of any explicit transaction context. The caller
            /// is responsible for managing the lifetime and disposal of the transaction object.</remarks>
            public IDbTransaction Transaction { get; set; }

            /// <summary>
            /// Gets or sets a value that determines how command results are applied to the DataRow when used with an
            /// update operation.
            /// </summary>
            /// <remarks>Use this property to control whether output parameters, first returned rows,
            /// or both are mapped back to the DataRow during an update. The default value and supported options may
            /// vary depending on the specific data provider implementation.</remarks>
            public UpdateRowSource UpdatedRowSource { get; set; }

            /// <summary>
            /// Releases all resources used by the current instance.
            /// </summary>
            /// <remarks>Call this method when you are finished using the object to free unmanaged
            /// resources and perform other cleanup operations. After calling Dispose, the object should not be used
            /// further.</remarks>
            public void Dispose()
            { }

            /// <summary>
            /// Executes a SQL statement against the connection and returns the number of rows affected.
            /// </summary>
            /// <returns>The number of rows affected by the SQL statement. Returns -1 for statements that do not affect rows,
            /// such as DDL statements.</returns>
            public int ExecuteNonQuery() => 0;

            /// <summary>
            /// Performs any necessary setup or initialization required before executing operations on this instance.
            /// </summary>
            public void Prepare()
            { }
        }

        /// <summary>
        /// Creates a new instance of the SqlMaterializer class using the specified command mock for testing purposes.
        /// </summary>
        /// <param name="commandMock">A mock implementation of the IDbCommand interface to be used by the materializer.</param>
        /// <returns>A SqlMaterializer instance configured to use the provided command mock and a mock database connection.</returns>
        private static SqlMaterializer CreateMaterializerWithCommand(
            Mock<IDbCommand> commandMock)
        {
            var connectionMock = new Mock<IDbConnection>();
            connectionMock
                .Setup(c => c.CreateCommand())
                .Returns(commandMock.Object);
            connectionMock
                .Setup(c => c.State)
                .Returns(ConnectionState.Open);

            commandMock
                .Setup(c => c.Parameters)
                .Returns(new MockDataParameterCollection());
            commandMock
                .Setup(c => c.CreateParameter())
                .Returns(new MockDbParameter());

            var materializer = new SqlMaterializer(
                connectionMock.Object,
                timeout: 30,
                parameterPrefix: "@");

            return materializer;
        }
    }
}