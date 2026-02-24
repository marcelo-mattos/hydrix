using Hydrix.Orchestrator.Binders.Parameter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Binders.Parameter
{
    /// <summary>
    /// Provides unit tests for the ParameterObjectBinder class to verify correct binding of parameters to database
    /// commands.
    /// </summary>
    /// <remarks>This class includes test cases for various scenarios, such as binding simple parameters,
    /// handling enumerable parameters, managing null values, and ensuring that string and byte array parameters are not
    /// expanded. The tests help ensure that ParameterObjectBinder behaves as expected when interacting with database
    /// command objects.</remarks>
    public class ParameterObjectBinderTests
    {
        /// <summary>
        /// Represents a command to be executed against a database, providing properties and methods to configure and
        /// execute the command.
        /// </summary>
        /// <remarks>This class implements the IDbCommand interface, allowing for the execution of SQL
        /// commands against a database connection. It provides properties to set the command text, timeout, type, and
        /// associated connection and transaction. The ExecuteNonQuery, ExecuteReader, and ExecuteScalar methods are
        /// available for executing commands and retrieving results.</remarks>
        private class DummyCommand : IDbCommand
        {
            /// <summary>
            /// Gets or sets the SQL command text to be executed against the database.
            /// </summary>
            /// <remarks>The command text can include parameters that need to be defined before
            /// execution. Ensure that the command text is properly formatted to avoid SQL injection
            /// vulnerabilities.</remarks>
            public string CommandText { get; set; }

            /// <summary>
            /// Gets or sets the maximum time, in seconds, before a command is considered to have timed out.
            /// </summary>
            /// <remarks>If the command does not complete within the specified time, an exception is
            /// thrown. The default value is 30 seconds.</remarks>
            public int CommandTimeout { get; set; }

            /// <summary>
            /// Gets or sets the type of command to be executed.
            /// </summary>
            /// <remarks>This property determines the nature of the command, which can affect how it
            /// is processed by the underlying system. Valid command types include text commands, stored procedures, and
            /// others as defined by the CommandType enumeration.</remarks>
            public CommandType CommandType { get; set; }

            /// <summary>
            /// Gets or sets the database connection used to execute commands against the database.
            /// </summary>
            /// <remarks>Assign a valid IDbConnection instance before performing any database
            /// operations. Ensure that the connection is properly opened and closed to prevent resource
            /// leaks.</remarks>
            public IDbConnection Connection { get; set; }

            /// <summary>
            /// Gets the collection of parameters associated with the command.
            /// </summary>
            /// <remarks>Use this property to access or configure the parameters required for command
            /// execution. The collection allows you to add, remove, or modify parameters dynamically before executing
            /// the command. This is essential for commands that require input values or output parameters.</remarks>
            public IDataParameterCollection Parameters { get; } = new DummyParameterCollection();

            /// <summary>
            /// Gets or sets the database transaction associated with the current connection.
            /// </summary>
            /// <remarks>This property enables transactional operations within the database context.
            /// Ensure that the transaction is properly initialized before use, and be aware that all commands executed
            /// while this transaction is active will be committed or rolled back according to the transaction's
            /// state.</remarks>
            public IDbTransaction Transaction { get; set; }

            /// <summary>
            /// Gets or sets the behavior for updating the row source after an insert, update, or delete operation.
            /// </summary>
            /// <remarks>This property determines how the data adapter updates the data source after
            /// executing commands. It can be set to values from the UpdateRowSource enumeration, which specifies
            /// whether to include the parameters used in the command, the updated row, or both.</remarks>
            public UpdateRowSource UpdatedRowSource { get; set; }

            /// <summary>
            /// Cancels the current operation.
            /// </summary>
            /// <remarks>This method can be called to halt any ongoing processes initiated by the
            /// associated class. It is important to note that calling this method may not guarantee immediate
            /// termination of the operation, depending on its current state.</remarks>
            public void Cancel() { }

            /// <summary>
            /// Creates a new database parameter instance for use in command execution.
            /// </summary>
            /// <returns>An object that implements <see cref="IDbDataParameter"/> representing the database parameter.</returns>
            public IDbDataParameter CreateParameter() => new DummyParameter();

            /// <summary>
            /// Releases all resources used by the current instance of the class.
            /// </summary>
            /// <remarks>Call this method when the object is no longer needed to ensure that any
            /// unmanaged resources are properly released. After calling Dispose, the object should not be used
            /// further.</remarks>
            public void Dispose() { }

            /// <summary>
            /// Executes a command against the database and returns the number of rows affected.
            /// </summary>
            /// <remarks>This method is typically used for executing SQL statements that do not return
            /// result sets, such as INSERT, UPDATE, or DELETE commands.</remarks>
            /// <returns>The number of rows affected by the command execution.</returns>
            public int ExecuteNonQuery() => 0;

            /// <summary>
            /// Executes the command and returns an IDataReader for reading data from the result set.
            /// </summary>
            /// <remarks>This method should be called when the command is expected to return rows.
            /// Ensure that the command is properly configured before calling this method.</remarks>
            /// <returns>An IDataReader that provides access to the data returned by the command. The reader is positioned before
            /// the first record.</returns>
            public IDataReader ExecuteReader() => null;

            /// <summary>
            /// Executes the command and returns an IDataReader for reading the results.
            /// </summary>
            /// <remarks>This method allows for efficient retrieval of data in a forward-only,
            /// read-only manner. Ensure that the command is properly configured before calling this method.</remarks>
            /// <param name="behavior">Specifies the behavior of the command execution, which influences how the data is retrieved and
            /// processed.</param>
            /// <returns>An IDataReader that provides a way to read the results of the command. The reader is positioned before
            /// the first record.</returns>
            public IDataReader ExecuteReader(CommandBehavior behavior) => null;

            /// <summary>
            /// Executes the command and returns the first column of the first row in the result set produced by the
            /// command. Additional columns or rows are ignored.
            /// </summary>
            /// <remarks>This method is typically used when the command is expected to return a single
            /// value, such as an aggregate result. If the command does not return any rows, the method returns
            /// null.</remarks>
            /// <returns>An object representing the value of the first column of the first row in the result set, or null if the
            /// result set is empty.</returns>
            public object ExecuteScalar() => null;

            /// <summary>
            /// Prepares the system for operation by initializing necessary components.
            /// </summary>
            /// <remarks>This method should be called before any operations are performed to ensure
            /// that all required resources are set up correctly.</remarks>
            public void Prepare() { }
        }

        /// <summary>
        /// Represents a database command parameter, providing properties to define its data type, direction, and value
        /// for use in parameterized queries.
        /// </summary>
        /// <remarks>This class implements the IDbDataParameter interface, allowing configuration of
        /// parameter characteristics such as precision, scale, size, and nullability. It is intended for scenarios
        /// where parameters must be supplied to database commands, such as executing stored procedures or parameterized
        /// SQL statements.</remarks>
        private class DummyParameter : IDbDataParameter
        {
            /// <summary>
            /// Gets or sets the precision of the numeric value, indicating the total number of digits that can be
            /// stored.
            /// </summary>
            /// <remarks>The precision value must be a positive integer. It defines the maximum number
            /// of significant digits that can be represented, which is crucial for ensuring accurate calculations and
            /// data representation.</remarks>
            public byte Precision { get; set; }

            /// <summary>
            /// Gets or sets the scale factor used for calculations.
            /// </summary>
            /// <remarks>The scale value influences the precision of the calculations performed. It
            /// should be a positive value, typically ranging from 0 to 255, where higher values indicate greater
            /// precision.</remarks>
            public byte Scale { get; set; }

            /// <summary>
            /// Gets or sets the size of the object, in bytes.
            /// </summary>
            /// <remarks>The size property indicates the memory footprint of the object. The value may
            /// vary depending on the object's state and the data it contains.</remarks>
            public int Size { get; set; }

            /// <summary>
            /// Gets or sets the database type associated with the current instance.
            /// </summary>
            /// <remarks>This property is used to specify the type of database that the instance
            /// interacts with, which can affect how data is processed and stored. Ensure that the value assigned is
            /// compatible with the underlying database system being used.</remarks>
            public DbType DbType { get; set; }

            /// <summary>
            /// Gets or sets the direction of the parameter, indicating whether it is used for input, output, or both in
            /// a database operation.
            /// </summary>
            /// <remarks>This property is essential for specifying how the parameter interacts with
            /// database commands, such as stored procedures. The direction determines whether the parameter supplies
            /// data to the command, receives data from it, or does both. Setting the correct direction is important for
            /// ensuring proper execution and data flow in database operations.</remarks>
            public ParameterDirection Direction { get; set; }

            /// <summary>
            /// Gets a value indicating whether the associated value can be null.
            /// </summary>
            /// <remarks>This property always returns <see langword="false"/>, indicating that the
            /// value is not nullable.</remarks>
            public bool IsNullable => false;

            /// <summary>
            /// Gets or sets the name of the parameter.
            /// </summary>
            /// <remarks>This property is used to identify the parameter in various contexts, such as
            /// logging or validation.</remarks>
            public string ParameterName { get; set; }

            /// <summary>
            /// Gets or sets the name of the source column from which data is retrieved.
            /// </summary>
            /// <remarks>This property is typically used in data binding scenarios to specify the
            /// column name in the data source that corresponds to the property.</remarks>
            public string SourceColumn { get; set; }

            /// <summary>
            /// Gets or sets the version of the data row to use when updating the data source.
            /// </summary>
            /// <remarks>This property determines which version of the data row is referenced during
            /// update operations. Setting the correct version is essential to ensure that the intended data is sent to
            /// the data source, especially when handling modified, original, or current values.</remarks>
            public DataRowVersion SourceVersion { get; set; }

            /// <summary>
            /// Gets or sets the value associated with the property.
            /// </summary>
            /// <remarks>This property can hold any object type, allowing for flexible data storage.
            /// Ensure to check the type of the object before using it to avoid runtime errors.</remarks>
            public object Value { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether null values in the source column are mapped during data
            /// operations.
            /// </summary>
            /// <remarks>This property is useful when working with data sources that may contain null
            /// values, allowing for explicit control over how nulls are handled in mapping scenarios.</remarks>
            public bool SourceColumnNullMapping { get; set; }
        }

        /// <summary>
        /// Represents a collection of parameters that supports access and manipulation by parameter name, facilitating
        /// parameter management in data operations.
        /// </summary>
        /// <remarks>This collection implements the IDataParameterCollection interface, enabling users to
        /// check for the existence of parameters, retrieve their indices, and remove them by name. It is intended for
        /// scenarios where parameters must be managed dynamically, such as in data binding or command execution
        /// contexts.</remarks>
        private class DummyParameterCollection : List<object>, IDataParameterCollection
        {
            /// <summary>
            /// Gets or sets the first parameter that matches the specified parameter name.
            /// </summary>
            /// <remarks>This indexer allows for easy access to parameters by their name. If multiple
            /// parameters share the same name, only the first matching parameter is returned.</remarks>
            /// <param name="parameterName">The name of the parameter to retrieve or set. This must not be null or empty.</param>
            /// <returns>An object representing the parameter that matches the specified name, or null if no such parameter
            /// exists.</returns>
            public object this[string parameterName]
            {
                get => this.FirstOrDefault(p => ((DummyParameter)p).ParameterName == parameterName);
                set { }
            }

            /// <summary>
            /// Determines whether a parameter with the specified name exists in the collection.
            /// </summary>
            /// <remarks>This method performs a case-sensitive comparison when checking for the
            /// existence of the parameter name.</remarks>
            /// <param name="parameterName">The name of the parameter to search for within the collection. This value cannot be null or empty.</param>
            /// <returns>true if a parameter with the specified name exists; otherwise, false.</returns>
            public bool Contains(string parameterName) => this.Any(p => ((DummyParameter)p).ParameterName == parameterName);

            /// <summary>
            /// Returns the zero-based index of the first parameter in the collection that matches the specified name.
            /// </summary>
            /// <remarks>This method performs a linear search. For large collections, consider the
            /// potential performance impact.</remarks>
            /// <param name="parameterName">The name of the parameter to locate within the collection. This value cannot be null or empty.</param>
            /// <returns>The zero-based index of the first occurrence of the specified parameter name, or -1 if the parameter is
            /// not found.</returns>
            public int IndexOf(string parameterName) => this.FindIndex(p => ((DummyParameter)p).ParameterName == parameterName);

            /// <summary>
            /// Removes all parameters with the specified name from the collection.
            /// </summary>
            /// <remarks>This method will remove all instances of parameters that match the provided
            /// name. If no parameters match, the collection remains unchanged.</remarks>
            /// <param name="parameterName">The name of the parameter to remove. This value is used to identify which parameters should be deleted
            /// from the collection.</param>
            public void RemoveAt(string parameterName) => this.RemoveAll(p => ((DummyParameter)p).ParameterName == parameterName);
        }

        /// <summary>
        /// Represents a set of parameters used for testing scenarios, including identifiers, names, values, text,
        /// binary data, and a nullable object.
        /// </summary>
        /// <remarks>This class encapsulates various data types commonly required in test cases. The
        /// 'Values' property allows for an array of nullable integers, supporting flexible test data representation.
        /// The 'NullObj' property can be used to simulate null or optional values in tests.</remarks>
        private class TestParams
        {
            /// <summary>
            /// Gets or sets the identifier for the test parameters.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with the test parameters.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets an array of nullable integers representing the values associated with the object.
            /// </summary>
            /// <remarks>This property can hold a collection of integers, where each integer can be
            /// null. It is useful for scenarios where the presence of a value is optional.</remarks>
            public int?[] Values { get; set; }

            /// <summary>
            /// Gets or sets the text associated with the object.
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// Gets or sets the raw binary data associated with the object.
            /// </summary>
            /// <remarks>This property holds the data in a byte array format, which can be used for
            /// various purposes such as file storage or transmission. Ensure that the data is properly initialized
            /// before use.</remarks>
            public byte[] Data { get; set; }

            /// <summary>
            /// Gets or sets the object that represents a null value in the context of this property.
            /// </summary>
            /// <remarks>This property can be used to define a placeholder for null values, allowing
            /// for more flexible handling of null scenarios in the application.</remarks>
            public object NullObj { get; set; }
        }

        /// <summary>
        /// Verifies that binding simple parameters to a command adds them with the specified prefix to their names.
        /// </summary>
        /// <remarks>This test ensures that the ParameterObjectBinder correctly applies the given prefix
        /// to parameter names when binding values from a parameter object. It validates that the resulting command
        /// parameters are named and valued as expected, supporting scenarios where parameter name uniqueness is
        /// required.</remarks>
        [Fact]
        public void Bind_SimpleParameters_AddsParametersWithPrefix()
        {
            var bindings = new[] {
                new ParameterObjectBinding("Id", p => ((TestParams)p).Id),
                new ParameterObjectBinding("Name", p => ((TestParams)p).Name)
            };
            var binder = new ParameterObjectBinder(bindings);
            var command = new DummyCommand { CommandText = "SELECT * FROM t WHERE @prefixId = @prefixName" };
            var parameters = new TestParams { Id = 42, Name = "abc" };
            var added = new List<(string, object)>();
            binder.Bind(command, parameters, "@prefix", (cmd, name, value) => added.Add((name, value)));
            Assert.Contains(added, x => x.Item1 == "@prefixId" && (int)x.Item2 == 42);
            Assert.Contains(added, x => x.Item1 == "@prefixName" && (string)x.Item2 == "abc");
        }

        /// <summary>
        /// Verifies that binding an enumerable parameter expands it into multiple parameters and updates the command
        /// text accordingly.
        /// </summary>
        /// <remarks>This test ensures that when an enumerable property is bound to a command, each
        /// element is assigned a unique parameter name and the command text is modified to reference all generated
        /// parameters. It validates correct parameter naming and command text replacement for scenarios such as SQL IN
        /// clauses.</remarks>
        [Fact]
        public void Bind_EnumerableParameter_ExpandsAndReplacesCommandText()
        {
            var bindings = new[] {
                new ParameterObjectBinding("Values", p => ((TestParams)p).Values)
            };
            var binder = new ParameterObjectBinder(bindings);
            var command = new DummyCommand { CommandText = "WHERE @Values" };
            var parameters = new TestParams { Values = new int?[] { 1, 2, 3 } };
            binder.Bind(command, parameters, "@", (cmd, name, value) =>
            {
                var param = cmd.CreateParameter();
                param.ParameterName = name;
                param.Value = value;
                cmd.Parameters.Add(param);
            });
            Assert.Contains(command.Parameters.Cast<DummyParameter>(), p => p.ParameterName == "@Values_0" && (int)p.Value == 1);
            Assert.Contains(command.Parameters.Cast<DummyParameter>(), p => p.ParameterName == "@Values_1" && (int)p.Value == 2);
            Assert.Contains(command.Parameters.Cast<DummyParameter>(), p => p.ParameterName == "@Values_2" && (int)p.Value == 3);
            Assert.Equal("WHERE @Values_0, @Values_1, @Values_2", command.CommandText);
        }

        /// <summary>
        /// Verifies that binding an enumerable parameter containing null values replaces those nulls with DBNull when
        /// adding parameters to a command.
        /// </summary>
        /// <remarks>This test ensures that the parameter binding logic correctly handles null values in
        /// enumerables by converting them to DBNull, which is required for proper database interaction. It is useful
        /// for scenarios where SQL commands must distinguish between actual values and database nulls.</remarks>
        [Fact]
        public void Bind_EnumerableParameter_WithNulls_UsesDBNull()
        {
            var bindings = new[] {
                new ParameterObjectBinding("Values", p => ((TestParams)p).Values)
            };
            var binder = new ParameterObjectBinder(bindings);
            var command = new DummyCommand { CommandText = "WHERE @Values" };
            var parameters = new TestParams { Values = new int?[] { 1, null, 3 }.Select(x => x.HasValue ? x.Value : (int?)null).ToArray() };
            binder.Bind(command, parameters, "@", (cmd, name, value) =>
            {
                var param = cmd.CreateParameter();
                param.ParameterName = name;
                param.Value = value;
                cmd.Parameters.Add(param);
            });
            Assert.Equal(DBNull.Value, ((DummyParameter)command.Parameters.Cast<object>().ToArray()[1]).Value);
        }

        /// <summary>
        /// Verifies that the parameter binder does not expand string or byte array parameters when binding them to a
        /// command.
        /// </summary>
        /// <remarks>This test ensures that string and byte array parameters are bound as single values,
        /// preserving their original types and values. It confirms that the command receives the expected parameter
        /// names and types without modification.</remarks>
        [Fact]
        public void Bind_DoesNotExpand_StringOrByteArray()
        {
            var bindings = new[] {
                new ParameterObjectBinding("Text", p => ((TestParams)p).Text),
                new ParameterObjectBinding("Data", p => ((TestParams)p).Data)
            };
            var binder = new ParameterObjectBinder(bindings);
            var command = new DummyCommand { CommandText = "WHERE @Text AND @Data" };
            var parameters = new TestParams { Text = "hello", Data = new byte[] { 1, 2 } };
            var added = new List<(string, object)>();
            binder.Bind(command, parameters, "@", (cmd, name, value) => added.Add((name, value)));
            Assert.Contains(added, x => x.Item1 == "@Text" && (string)x.Item2 == "hello");
            Assert.Contains(added, x => x.Item1 == "@Data" && x.Item2 is byte[]);
        }

        /// <summary>
        /// Verifies that a null parameter is correctly added to the command's parameters during binding.
        /// </summary>
        /// <remarks>This test ensures that when a parameter is set to null in the provided parameters, it
        /// is added to the command with the expected name and null value. It is important for scenarios where null
        /// values need to be explicitly handled in SQL commands.</remarks>
        [Fact]
        public void Bind_NullParameter_AddsNull()
        {
            var bindings = new[] {
                new ParameterObjectBinding("NullObj", p => ((TestParams)p).NullObj)
            };
            var binder = new ParameterObjectBinder(bindings);
            var command = new DummyCommand { CommandText = "WHERE @NullObj" };
            var parameters = new TestParams { NullObj = null };
            var added = new List<(string, object)>();
            binder.Bind(command, parameters, "@", (cmd, name, value) => added.Add((name, value)));
            Assert.Contains(added, x => x.Item1 == "@NullObj" && x.Item2 == null);
        }
    }
}
