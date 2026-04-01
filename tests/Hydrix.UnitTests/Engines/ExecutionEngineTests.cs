using Hydrix.Attributes.Schemas;
using Hydrix.Engines;
using Hydrix.Schemas.Contract;
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
    /// Contains unit tests and supporting mock implementations for verifying the behavior of the execution engine when
    /// interacting with database commands, parameters, and connections.
    /// </summary>
    /// <remarks>This class provides a comprehensive suite of tests for both synchronous and asynchronous
    /// database operations, including command execution, parameter binding, and cancellation scenarios. It includes
    /// reusable mock classes for simulating IDbCommand, DbCommand, and parameter collections, ensuring consistent and
    /// maintainable test setups. The tests validate correct handling of command types, result values, and exception
    /// conditions, helping to ensure robust data access code.</remarks>
    public class ExecutionEngineTests
    {
        /// <summary>
        /// Represents a mock implementation of the IDbCommand interface for testing database command operations.
        /// </summary>
        /// <remarks>This class simulates the behavior of a database command and can be used in unit tests
        /// to verify interactions with data access code. It provides configurable properties and result values to mimic
        /// various command scenarios. All execution methods throw NotImplementedException to indicate that actual
        /// database operations are not performed.</remarks>
        private sealed class TestCommand :
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
            public IDataParameterCollection Parameters { get; } = new TestParameterCollection();

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
            /// Gets or sets the result value returned by a non-query database operation.
            /// </summary>
            public int NonQueryResult { get; set; } = 1;

            /// <summary>
            /// Gets or sets the result value produced by a scalar operation.
            /// </summary>
            public object ScalarResult { get; set; } = 42;

            /// <summary>
            /// Gets or sets the result of a database query as an IDataReader instance.
            /// </summary>
            /// <remarks>Use this property to access or assign the data reader representing the
            /// outcome of a database operation. The assigned IDataReader should be properly disposed after use to
            /// release resources.</remarks>
            public IDataReader ReaderResult { get; set; } = new Mock<IDataReader>().Object;

            /// <summary>
            /// Gets a value indicating whether the command has been disposed.
            /// </summary>
            public bool IsDisposed { get; private set; }

            /// <summary>
            /// Requests cancellation of the current operation, if supported.
            /// </summary>
            public void Cancel()
            { }

            /// <summary>
            /// Creates a new instance of a parameter object for use with database commands.
            /// </summary>
            /// <remarks>The returned parameter is not associated with any specific command or
            /// connection until it is added to a command object. Configure the parameter's properties as needed before
            /// use.</remarks>
            /// <returns>An <see cref="IDbDataParameter"/> representing a parameter that can be added to a command for data
            /// operations.</returns>
            public IDbDataParameter CreateParameter() => new TestParameter();

            /// <summary>
            /// Executes a SQL command that does not return any result sets, such as an INSERT, UPDATE, or DELETE
            /// statement.
            /// </summary>
            /// <returns>The number of rows affected by the command. Returns -1 for statements that do not affect rows, such as
            /// DDL commands.</returns>
            /// <exception cref="NotImplementedException">Thrown in all cases, as this method is not implemented.</exception>
            public int ExecuteNonQuery() => NonQueryResult;

            /// <summary>
            /// Executes the command and returns a data reader for retrieving the results of the query.
            /// </summary>
            /// <returns>An <see cref="IDataReader"/> that can be used to read the results of the command in a forward-only,
            /// read-only manner.</returns>
            /// <exception cref="NotImplementedException">Thrown in all cases, as this method is not implemented.</exception>
            public IDataReader ExecuteReader() => ReaderResult;

            /// <summary>
            /// Executes the command and returns a data reader for retrieving the results, using the specified command
            /// behavior.
            /// </summary>
            /// <param name="behavior">A value that specifies the behavior of the command and the data reader. This can influence how results
            /// are retrieved, such as whether the connection is closed when the reader is closed, or whether single-row
            /// or schema-only results are returned.</param>
            /// <returns>An <see cref="IDataReader"/> that can be used to read the results of the command.</returns>
            /// <exception cref="NotImplementedException">Thrown in all cases, as this method is not implemented.</exception>
            public IDataReader ExecuteReader(CommandBehavior behavior) => ReaderResult;

            /// <summary>
            /// Executes the query and returns the first column of the first row in the result set.
            /// </summary>
            /// <returns>An object representing the value of the first column of the first row in the result set. Returns null if
            /// the result set is empty.</returns>
            /// <exception cref="NotImplementedException">Thrown in all cases, as this method is not implemented.</exception>
            public object ExecuteScalar() => ScalarResult;

            /// <summary>
            /// Prepares the current instance for execution or use. This method should be called before performing
            /// operations that depend on the instance being initialized.
            /// </summary>
            /// <exception cref="NotImplementedException">Thrown in all cases, as this method is not yet implemented.</exception>
            public void Prepare()
            { }

            /// <summary>
            /// Releases all resources used by the current instance of the class.
            /// </summary>
            /// <remarks>Call this method when you are finished using the object to free unmanaged
            /// resources and perform other cleanup operations. After calling <see cref="Dispose"/>, the object should
            /// not be used further.</remarks>
            public void Dispose()
                => IsDisposed = true;
        }

        /// <summary>
        /// Represents a mock implementation of a database parameter used for testing purposes. Provides properties for
        /// configuring parameter metadata, value, and mapping to database fields.
        /// </summary>
        /// <remarks>This class is intended for use in unit tests and mock scenarios where a database
        /// parameter is required. It implements the IDbDataParameter interface, allowing it to be used in contexts that
        /// expect standard database parameter behavior. Properties such as DbType, Direction, and SourceVersion can be
        /// set to simulate various parameter configurations. The class does not support nullable types, and IsNullable
        /// always returns false.</remarks>
        private sealed class TestParameter :
            IDbDataParameter
        {
            /// <summary>
            /// Gets or sets the number of decimal places used for numeric values.
            /// </summary>
            public Byte Precision { get; set; }

            /// <summary>
            /// Gets or sets the number of decimal places to which numeric values are scaled.
            /// </summary>
            /// <remarks>Use this property to specify the precision of fractional values when working
            /// with numeric data types that support scaling. The valid range and effect of this property may depend on
            /// the underlying data type or context in which it is used.</remarks>
            public Byte Scale { get; set; }

            /// <summary>
            /// Gets or sets the size of the object.
            /// </summary>
            public Int32 Size { get; set; }

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
        private sealed class TestDbParameter :
            DbParameter
        {
            /// <summary>
            /// Gets or sets the database type of the parameter.
            /// </summary>
            /// <remarks>Use this property to specify the type of data that the parameter represents
            /// when interacting with the database. Setting the correct database type ensures proper value conversion
            /// and compatibility with the underlying data provider.</remarks>
            public override DbType DbType { get; set; }

            /// <summary>
            /// Gets or sets the direction of the parameter for a command.
            /// </summary>
            /// <remarks>Use this property to specify whether the parameter is an input, output,
            /// bidirectional, or a return value when executing a command. The value should be set according to the
            /// intended use of the parameter in the command context.</remarks>
            public override ParameterDirection Direction { get; set; }

            /// <summary>
            /// Gets a value indicating whether the current type allows null values.
            /// </summary>
            public override bool IsNullable { get; set; }

            /// <summary>
            /// Gets or sets the name of the parameter associated with the current context.
            /// </summary>
            public override string ParameterName { get; set; }

            /// <summary>
            /// Gets or sets the name of the source column mapped to a data field.
            /// </summary>
            public override string SourceColumn { get; set; }

            /// <summary>
            /// Gets or sets the version of the data in a DataRow to use when retrieving parameter values.
            /// </summary>
            /// <remarks>Use this property to specify which version of the DataRow's data should be
            /// used, such as Original, Current, or Proposed, when working with data-bound parameters. This is commonly
            /// used in scenarios involving updates or concurrency control in data operations.</remarks>
            public override DataRowVersion SourceVersion { get; set; }

            /// <summary>
            /// Gets or sets the value associated with this instance.
            /// </summary>
            public override object Value { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the source column is mapped to a DataSet column that allows null
            /// values.
            /// </summary>
            /// <remarks>This property is used during data operations to determine if null values from
            /// the source column should be handled when populating a DataSet. It is relevant for scenarios where schema
            /// mapping and nullability need to be considered.</remarks>
            public override bool SourceColumnNullMapping { get; set; }

            /// <summary>
            /// Gets or sets the size, in bytes, of the parameter value.
            /// </summary>
            public override int Size { get; set; }

            /// <summary>
            /// Resets the type associated with the parameter to its default value.
            /// </summary>
            /// <remarks>Use this method to clear any custom database type previously set for the
            /// parameter, restoring it to its initial state. This is useful when reusing parameter instances for
            /// different commands or queries.</remarks>
            public override void ResetDbType()
            { }
        }

        /// <summary>
        /// Represents a collection of database parameters for use with test database command objects. Provides methods
        /// to add, remove, and access parameters by index or name.
        /// </summary>
        /// <remarks>This collection is intended for use in unit tests and mock scenarios where a concrete
        /// implementation of DbParameterCollection is required. It supports typical collection operations and allows
        /// parameters to be managed by their names or positions. Thread safety is not guaranteed; synchronization
        /// should be handled externally if needed.</remarks>
        private sealed class TestDbParameterCollection :
            DbParameterCollection
        {
            /// <summary>
            /// Represents the collection of database parameters contained in this instance.
            /// </summary>
            private readonly List<DbParameter> _items = new List<DbParameter>();

            /// <summary>
            /// Gets the number of elements contained in the collection.
            /// </summary>
            public override int Count => _items.Count;

            /// <summary>
            /// Gets an object that can be used to synchronize access to the collection.
            /// </summary>
            /// <remarks>Use the object returned by this property to lock the collection during
            /// multithreaded operations. This ensures thread safety when accessing or modifying the collection
            /// concurrently.</remarks>
            public override object SyncRoot => ((ICollection)_items).SyncRoot;

            /// <summary>
            /// Adds a parameter to the collection and returns the index at which it was added.
            /// </summary>
            /// <remarks>The collection increases in size by one after the parameter is added. If the
            /// value is not of type DbParameter, an exception may be thrown.</remarks>
            /// <param name="value">The parameter to add to the collection. Must be of type DbParameter.</param>
            /// <returns>The zero-based index of the added parameter within the collection.</returns>
            public override int Add(object value)
            {
                _items.Add((DbParameter)value);
                return _items.Count - 1;
            }

            /// <summary>
            /// Adds the elements of the specified array to the collection.
            /// </summary>
            /// <remarks>The collection is updated to include all parameters from the specified array.
            /// If any element in the array is not of type DbParameter, an exception will be thrown.</remarks>
            /// <param name="values">An array containing the parameters to add to the collection. Each element must be of type DbParameter.</param>
            public override void AddRange(Array values)
            {
                foreach (var value in values)
                    _items.Add((DbParameter)value);
            }

            /// <summary>
            /// Removes all items from the collection.
            /// </summary>
            public override void Clear() => _items.Clear();

            /// <summary>
            /// Determines whether the collection contains a specific parameter object.
            /// </summary>
            /// <param name="value">The parameter object to locate in the collection. Must be of type DbParameter.</param>
            /// <returns>true if the parameter object is found in the collection; otherwise, false.</returns>
            public override bool Contains(object value) => _items.Contains((DbParameter)value);

            /// <summary>
            /// Determines whether the collection contains a parameter with the specified name.
            /// </summary>
            /// <param name="value">The name of the parameter to locate in the collection. Cannot be null.</param>
            /// <returns>true if a parameter with the specified name exists in the collection; otherwise, false.</returns>
            public override bool Contains(string value) => _items.Any(p => p.ParameterName == value);

            /// <summary>
            /// Copies the elements of the collection to a specified array, starting at the given array index.
            /// </summary>
            /// <param name="array">The destination array that will receive the copied elements. Must be of a compatible type and have
            /// sufficient space to accommodate the copied elements.</param>
            /// <param name="index">The zero-based index in the destination array at which copying begins. Must be within the bounds of the
            /// array.</param>
            public override void CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            public override IEnumerator GetEnumerator() => _items.GetEnumerator();

            /// <summary>
            /// Retrieves the parameter at the specified index from the collection.
            /// </summary>
            /// <param name="index">The zero-based index of the parameter to retrieve. Must be within the bounds of the collection.</param>
            /// <returns>The parameter at the specified index as a DbParameter.</returns>
            protected override DbParameter GetParameter(int index) => _items[index];

            /// <summary>
            /// Retrieves the parameter object with the specified name from the collection.
            /// </summary>
            /// <remarks>Throws an exception if no parameter with the specified name exists in the
            /// collection.</remarks>
            /// <param name="parameterName">The name of the parameter to locate. Cannot be null or empty.</param>
            /// <returns>The parameter object that matches the specified name.</returns>
            protected override DbParameter GetParameter(string parameterName) => _items.First(p => p.ParameterName == parameterName);

            /// <summary>
            /// Returns the zero-based index of the specified parameter within the collection.
            /// </summary>
            /// <remarks>If the specified value is not present in the collection, the method returns
            /// -1. The search is performed using reference equality for DbParameter objects.</remarks>
            /// <param name="value">The parameter object to locate in the collection. Must be of type DbParameter.</param>
            /// <returns>The zero-based index of the parameter if found; otherwise, -1.</returns>
            public override int IndexOf(object value) => _items.IndexOf((DbParameter)value);

            /// <summary>
            /// Returns the zero-based index of the parameter with the specified name within the collection.
            /// </summary>
            /// <remarks>If multiple parameters have the same name, the index of the first occurrence
            /// is returned. The search may be case-sensitive or case-insensitive depending on the collection's
            /// implementation.</remarks>
            /// <param name="parameterName">The name of the parameter to locate in the collection. Case sensitivity depends on the implementation.</param>
            /// <returns>The zero-based index of the parameter if found; otherwise, -1.</returns>
            public override int IndexOf(string parameterName) => _items.FindIndex(p => p.ParameterName == parameterName);

            /// <summary>
            /// Inserts a parameter into the collection at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index at which the parameter should be inserted. Must be within the bounds of the
            /// collection.</param>
            /// <param name="value">The parameter to insert. Must be of type DbParameter.</param>
            public override void Insert(int index, object value) => _items.Insert(index, (DbParameter)value);

            /// <summary>
            /// Removes the specified parameter from the collection.
            /// </summary>
            /// <remarks>If the specified parameter is not found in the collection, no action is
            /// taken. The method expects the value to be a DbParameter; passing an object of a different type may
            /// result in an exception.</remarks>
            /// <param name="value">The parameter object to remove from the collection. Must be of type DbParameter.</param>
            public override void Remove(object value) => _items.Remove((DbParameter)value);

            /// <summary>
            /// Removes the element at the specified index from the collection.
            /// </summary>
            /// <param name="index">The zero-based index of the element to remove. Must be greater than or equal to 0 and less than the
            /// number of elements in the collection.</param>
            public override void RemoveAt(int index) => _items.RemoveAt(index);

            /// <summary>
            /// Removes the parameter with the specified name from the collection.
            /// </summary>
            /// <remarks>If the parameter with the specified name does not exist in the collection, no
            /// action is taken.</remarks>
            /// <param name="parameterName">The name of the parameter to remove. Cannot be null.</param>
            public override void RemoveAt(string parameterName)
            {
                var index = IndexOf(parameterName);
                if (index >= 0)
                    RemoveAt(index);
            }

            /// <summary>
            /// Sets the parameter at the specified index in the collection to the given value.
            /// </summary>
            /// <param name="index">The zero-based index of the parameter to set. Must be within the bounds of the collection.</param>
            /// <param name="value">The parameter to assign at the specified index. Cannot be null.</param>
            protected override void SetParameter(int index, DbParameter value) => _items[index] = value;

            /// <summary>
            /// Sets the parameter with the specified name to the given value, adding it if it does not already exist.
            /// </summary>
            /// <param name="parameterName">The name of the parameter to set or add. Cannot be null.</param>
            /// <param name="value">The parameter object to assign. Cannot be null.</param>
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
        /// Represents a mock implementation of an asynchronous database command for testing purposes.
        /// </summary>
        /// <remarks>This class simulates the behavior of a database command and provides configurable
        /// asynchronous results for unit tests. It is intended for use in scenarios where database operations need to
        /// be mocked without connecting to a real database. The class reuses supporting types from the project’s mock
        /// infrastructure, such as TestDbParameter and TestDbParameterCollection.</remarks>
        private sealed class TestAsyncDbCommand :
            DbCommand
        {
            /// <summary>
            /// Represents the collection of parameters associated with the test database command.
            /// </summary>
            /// <remarks>This collection is used to manage and access parameters for mock database
            /// operations in unit tests. It is intended for internal use within test infrastructure and is not exposed
            /// to consumers of the API.</remarks>
            private readonly TestDbParameterCollection _parameters = new TestDbParameterCollection();

            /// <summary>
            /// Gets or sets the result of an asynchronous non-query database operation.
            /// </summary>
            public int NonQueryAsyncResult { get; set; }

            /// <summary>
            /// Gets or sets the result of an asynchronous scalar operation.
            /// </summary>
            public object ScalarAsyncResult { get; set; }

            /// <summary>
            /// Gets or sets the asynchronous result of a database reader operation.
            /// </summary>
            public DbDataReader ReaderAsyncResult { get; set; }

            /// <summary>
            /// Gets a value indicating whether the command has been disposed.
            /// </summary>
            public bool IsDisposed { get; private set; }

            /// <summary>
            /// Gets or sets the SQL statement to execute at the data source.
            /// </summary>
            public override string CommandText { get; set; }

            /// <summary>
            /// Gets or sets the wait time, in seconds, before terminating an attempt to execute a command.
            /// </summary>
            public override int CommandTimeout { get; set; }

            /// <summary>
            /// Gets or sets the type of the command to execute against the data source.
            /// </summary>
            public override CommandType CommandType { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the component should be visible at design time.
            /// </summary>
            /// <remarks>This property is typically used by design tools to determine whether the
            /// component appears in a designer environment. Setting this property to <see langword="false"/> may hide
            /// the component from design-time tools, but does not affect its runtime visibility.</remarks>
            public override bool DesignTimeVisible { get; set; }

            /// <summary>
            /// Gets or sets a value that determines how command results are applied to the DataRow when performing an
            /// update operation.
            /// </summary>
            /// <remarks>Use this property to specify whether output parameters, first returned rows,
            /// or both are used to update the DataRow after executing a command. The value affects how changes from the
            /// data source are propagated back to the DataRow during update operations.</remarks>
            public override UpdateRowSource UpdatedRowSource { get; set; }

            /// <summary>
            /// Gets or sets the database connection used by the provider.
            /// </summary>
            protected override DbConnection DbConnection { get; set; }

            /// <summary>
            /// Gets the collection of parameters associated with the command.
            /// </summary>
            protected override DbParameterCollection DbParameterCollection => _parameters;

            /// <summary>
            /// Gets or sets the database transaction associated with the current operation.
            /// </summary>
            /// <remarks>Use this property to manage transactional operations within the database
            /// context. Assigning a transaction enables coordinated commit or rollback of multiple commands. The
            /// property may be null if no transaction is active.</remarks>
            protected override DbTransaction DbTransaction { get; set; }

            /// <summary>
            /// Attempts to cancel the execution of the command associated with this instance.
            /// </summary>
            /// <remarks>If the command is already executing, this method requests cancellation. The
            /// actual cancellation behavior depends on the underlying data provider and may not be immediate or
            /// guaranteed. Not all providers support command cancellation; consult provider documentation for
            /// details.</remarks>
            public override void Cancel()
            { }

            /// <summary>
            /// Executes the SQL statement against the connection and returns the number of rows affected.
            /// </summary>
            /// <returns>The number of rows affected by the execution of the SQL statement. Returns 0 for statements that do not
            /// affect rows.</returns>
            public override int ExecuteNonQuery() => NonQueryAsyncResult;

            /// <summary>
            /// Executes the query and returns the first column of the first row in the result set.
            /// </summary>
            /// <returns>An object representing the value of the first column of the first row in the result set. Returns null if
            /// the result set is empty.</returns>
            public override object ExecuteScalar() => ScalarAsyncResult;

            /// <summary>
            /// Creates a prepared version of the command on the data source, optimizing execution for repeated use.
            /// </summary>
            /// <remarks>Call this method before executing the command multiple times to improve
            /// performance. The exact effect depends on the underlying data provider; some providers may not support
            /// command preparation and will ignore this call.</remarks>
            public override void Prepare()
            { }

            /// <summary>
            /// Creates a new instance of a test database parameter for use in mock database operations.
            /// </summary>
            /// <remarks>This method is intended for use in unit tests and mock scenarios where a real
            /// database parameter is not required. The returned parameter mimics the behavior of a standard database
            /// parameter for testing purposes.</remarks>
            /// <returns>A new instance of a test database parameter that can be used to simulate parameter behavior in database
            /// command tests.</returns>
            protected override DbParameter CreateDbParameter() => new TestDbParameter();

            /// <summary>
            /// Executes the command and returns a data reader for reading the results, using the specified command
            /// behavior.
            /// </summary>
            /// <param name="behavior">A value that specifies the command behavior options for the data reader, such as single result or
            /// schema-only.</param>
            /// <returns>A data reader that can be used to read the results of the command execution.</returns>
            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => ReaderAsyncResult;

            /// <summary>
            /// Executes a SQL statement asynchronously against the database and returns the number of rows affected.
            /// </summary>
            /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
            /// <returns>A task representing the asynchronous operation. The task result contains the number of rows affected by
            /// the SQL statement.</returns>
            public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken) => Task.FromResult(NonQueryAsyncResult);

            /// <summary>
            /// Asynchronously executes the query and returns the first column of the first row in the result set.
            /// </summary>
            /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
            /// <returns>A task representing the asynchronous operation. The task result contains the value of the first column
            /// of the first row in the result set, or null if the result set is empty.</returns>
            public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken) => Task.FromResult(ScalarAsyncResult);

            /// <summary>
            /// Asynchronously executes the command and returns a data reader for reading the results, using the
            /// specified command behavior and cancellation token.
            /// </summary>
            /// <param name="behavior">A value that specifies the command behavior options for the data reader, such as single result or schema
            /// only.</param>
            /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
            /// <returns>A task representing the asynchronous operation. The task result contains a data reader for accessing the
            /// command results.</returns>
            protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
                => Task.FromResult(ReaderAsyncResult);

            /// <summary>
            /// Releases the unmanaged resources used by the object and optionally releases the managed resources.
            /// </summary>
            /// <remarks>This method is called by the public Dispose method and the finalizer. When
            /// disposing is true, this method releases all resources held by managed objects. When disposing is false,
            /// only unmanaged resources are released.</remarks>
            /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    IsDisposed = true;

                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Represents a test stored procedure descriptor used by ExecutionEngine procedure overload tests.
        /// </summary>
        [Procedure("usp_TestProcedure")]
        private sealed class TestProcedure :
            IProcedure<TestParameter>
        { }

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
        /// Verifies ExecuteNonQuery procedure overload returns affected rows.
        /// </summary>
        [Fact]
        public void ExecuteNonQuery_Procedure_ReturnsAffectedRows()
        {
            var command = new TestCommand { NonQueryResult = 8 };
            var connection = CreateOpenConnection(command);

            var result = ExecutionEngine.ExecuteNonQuery(
                connection.Object,
                new TestProcedure());

            Assert.Equal(8, result);
            Assert.Equal(CommandType.StoredProcedure, command.CommandType);
        }

        /// <summary>
        /// Verifies async procedure non-query uses DbCommand async execution path.
        /// </summary>
        [Fact]
        public async Task ExecuteNonQueryAsync_Procedure_DbCommand_UsesAsyncPath()
        {
            var dbCommand = new Mock<DbCommand>();
            dbCommand.SetupAllProperties();
            dbCommand.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(12);
            var connection = CreateOpenConnection(dbCommand.Object);

            var result = await ExecutionEngine.ExecuteNonQueryAsync(
                connection.Object,
                new TestProcedure());

            Assert.Equal(12, result);
        }

        /// <summary>
        /// Verifies async procedure non-query fallback path observes cancellation.
        /// </summary>
        [Fact]
        public async Task ExecuteNonQueryAsync_Procedure_NonDbCommand_Canceled_ThrowsTaskCanceledException()
        {
            var command = new TestCommand();
            var connection = CreateOpenConnection(command);
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await ExecutionEngine.ExecuteNonQueryAsync(
                    connection.Object,
                    new TestProcedure(),
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
        /// Verifies ExecuteScalar procedure overload returns scalar value.
        /// </summary>
        [Fact]
        public void ExecuteScalar_Procedure_ReturnsScalarValue()
        {
            var command = new TestCommand { ScalarResult = 456 };
            var connection = CreateOpenConnection(command);

            var result = ExecutionEngine.ExecuteScalar(
                connection.Object,
                new TestProcedure());

            Assert.Equal(456, result);
            Assert.Equal(CommandType.StoredProcedure, command.CommandType);
        }

        /// <summary>
        /// Verifies async procedure scalar uses DbCommand async execution path.
        /// </summary>
        [Fact]
        public async Task ExecuteScalarAsync_Procedure_DbCommand_UsesAsyncPath()
        {
            var dbCommand = new Mock<DbCommand>();
            dbCommand.SetupAllProperties();
            dbCommand.Setup(c => c.ExecuteScalarAsync(It.IsAny<CancellationToken>())).ReturnsAsync("proc-ok");
            var connection = CreateOpenConnection(dbCommand.Object);

            var result = await ExecutionEngine.ExecuteScalarAsync(
                connection.Object,
                new TestProcedure());

            Assert.Equal("proc-ok", result);
        }

        /// <summary>
        /// Verifies async procedure scalar fallback path observes cancellation.
        /// </summary>
        [Fact]
        public async Task ExecuteScalarAsync_Procedure_NonDbCommand_Canceled_ThrowsTaskCanceledException()
        {
            var command = new TestCommand();
            var connection = CreateOpenConnection(command);
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await ExecutionEngine.ExecuteScalarAsync(
                    connection.Object,
                    new TestProcedure(),
                    cancellationToken: cancellationTokenSource.Token));
        }

        /// <summary>
        /// Verifies ExecuteReader keeps the command alive until the returned reader is disposed.
        /// </summary>
        [Fact]
        public void ExecuteReader_DisposesCommandWhenReturnedReaderIsDisposed()
        {
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Dispose());
            var command = new TestCommand { ReaderResult = reader.Object };
            var connection = CreateOpenConnection(command);

            var result = ExecutionEngine.ExecuteReader(connection.Object, "select 1");

            Assert.NotSame(reader.Object, result);
            Assert.False(command.IsDisposed);

            result.Dispose();

            Assert.True(command.IsDisposed);
            reader.Verify(r => r.Dispose(), Times.Once);
        }

        /// <summary>
        /// Verifies async reader keeps the DbCommand alive until the returned reader is disposed.
        /// </summary>
        [Fact]
        public async Task ExecuteReaderAsync_DbCommand_DisposesCommandWhenReturnedReaderIsDisposed()
        {
            var dataReader = new Mock<DbDataReader>().Object;
            var dbCommand = new TestAsyncDbCommand { ReaderAsyncResult = dataReader };
            var connection = CreateOpenConnection(dbCommand);

            var result = await ExecutionEngine.ExecuteReaderAsync(connection.Object, "select 1");

            Assert.IsAssignableFrom<DbDataReader>(result);
            Assert.False(dbCommand.IsDisposed);

            result.Dispose();

            Assert.True(dbCommand.IsDisposed);
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
        /// Verifies ExecuteReader procedure overload also disposes the command when the reader is disposed.
        /// </summary>
        [Fact]
        public void ExecuteReader_Procedure_DisposesCommandWhenReturnedReaderIsDisposed()
        {
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Dispose());
            var command = new TestCommand { ReaderResult = reader.Object };
            var connection = CreateOpenConnection(command);

            var result = ExecutionEngine.ExecuteReader(
                connection.Object,
                new TestProcedure());

            Assert.Equal(CommandType.StoredProcedure, command.CommandType);
            Assert.False(command.IsDisposed);

            result.Dispose();

            Assert.True(command.IsDisposed);
            reader.Verify(r => r.Dispose(), Times.Once);
        }

        /// <summary>
        /// Verifies async procedure reader also keeps the command alive until reader disposal.
        /// </summary>
        [Fact]
        public async Task ExecuteReaderAsync_Procedure_DbCommand_DisposesCommandWhenReturnedReaderIsDisposed()
        {
            var dataReader = new Mock<DbDataReader>().Object;
            var dbCommand = new TestAsyncDbCommand { ReaderAsyncResult = dataReader };
            var connection = CreateOpenConnection(dbCommand);

            var result = await ExecutionEngine.ExecuteReaderAsync(
                connection.Object,
                new TestProcedure());

            Assert.IsAssignableFrom<DbDataReader>(result);
            Assert.False(dbCommand.IsDisposed);

            result.Dispose();

            Assert.True(dbCommand.IsDisposed);
        }

        /// <summary>
        /// Verifies async procedure reader fallback path observes cancellation.
        /// </summary>
        [Fact]
        public async Task ExecuteReaderAsync_Procedure_NonDbCommand_Canceled_ThrowsTaskCanceledException()
        {
            var command = new TestCommand();
            var connection = CreateOpenConnection(command);
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await ExecutionEngine.ExecuteReaderAsync(
                    connection.Object,
                    new TestProcedure(),
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
        /// Verifies that when command execution throws an exception, the command is disposed and the exception is
        /// rethrown.
        /// </summary>
        /// <remarks>This test ensures that the execution engine properly disposes the command object even
        /// if an exception occurs during execution, and that the original exception is propagated to the
        /// caller.</remarks>
        [Fact]
        public void ExecuteReader_WhenCommandExecutionThrows_DisposesCommandAndRethrows()
        {
            var command = new Mock<IDbCommand>();
            command.SetupAllProperties();
            command.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
            command.Setup(c => c.ExecuteReader(CommandBehavior.SingleResult)).Throws(new InvalidOperationException("boom"));
            var disposed = 0;
            command.Setup(c => c.Dispose()).Callback(() => disposed++);

            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.State).Returns(ConnectionState.Open);
            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var exception = Assert.Throws<InvalidOperationException>(() =>
                ExecutionEngine.ExecuteReader(
                    connection.Object,
                    "select 1",
                    behavior: CommandBehavior.SingleResult));

            Assert.Equal("boom", exception.Message);
            Assert.Equal(1, disposed);
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
