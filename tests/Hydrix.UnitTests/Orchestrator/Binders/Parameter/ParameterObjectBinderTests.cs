using Hydrix.Attributes.Parameters;
using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Binders.Parameter;
using Hydrix.Orchestrator.Caching;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
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
            public void Cancel()
            { }

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
            public void Dispose()
            { }

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
            public void Prepare()
            { }
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
        /// Represents a stored procedure that accepts an input parameter and provides a write-only property for input
        /// values.
        /// </summary>
        /// <remarks>The write-only property is intended for scenarios where a value must be supplied to
        /// the procedure without allowing retrieval of that value. This can be useful for sensitive data or when only
        /// input is required.</remarks>
        [Procedure("sp_test")]
        private class ProcedureWithWriteOnlyProperty
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            /// <remarks>This property is used to uniquely identify an instance of the entity in the
            /// database. It is required for operations that involve data retrieval or manipulation.</remarks>
            [Parameter("Id", DbType.Int32, Direction = ParameterDirection.Input)]
            public int Id { get; set; }

            /// <summary>
            /// Sets the value for the WriteOnly property. This property does not provide a getter, making it
            /// write-only.
            /// </summary>
            /// <remarks>This property is intended for scenarios where only setting a value is
            /// required, and reading the value is not necessary.</remarks>
            public int WriteOnly
            { set { } }
        }

        /// <summary>
        /// Represents a stored procedure that exposes an indexer for retrieving values based on an integer index.
        /// </summary>
        /// <remarks>This class encapsulates the parameters and behavior for the 'sp_test' stored
        /// procedure. The indexer allows callers to access a value associated with a specified index, which may be
        /// useful for scenarios where multiple result values are exposed in an indexed manner.</remarks>
        [Procedure("sp_test")]
        private class ProcedureWithIndexer
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            /// <remarks>This property is used to uniquely identify an instance of the entity in the
            /// database. It is required for operations that involve data retrieval or manipulation.</remarks>
            [Parameter("Id", DbType.Int32, Direction = ParameterDirection.Input)]
            public int Id { get; set; }

            /// <summary>
            /// Gets the value associated with the specified index.
            /// </summary>
            /// <param name="i">The zero-based index of the value to retrieve. Must be a non-negative integer.</param>
            /// <returns>The integer value associated with the specified index, which is always 42.</returns>
            public int this[int i] => 42;
        }

        /// <summary>
        /// Represents a stored procedure mapping for 'sp_test' that does not require any parameters for execution.
        /// </summary>
        /// <remarks>This class is decorated with the <see langword="Procedure"/> attribute, indicating
        /// its association with a stored procedure named 'sp_test'. The class contains a property 'Id' which is marked
        /// as a parameter, while 'NotAParameter' is a regular property and does not participate in the procedure's
        /// execution.</remarks>
        [Procedure("sp_test")]
        private class ProcedureWithNoParameterAttribute
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            /// <remarks>This property is used to uniquely identify an instance of the entity in the
            /// database. It is required for operations that involve data retrieval or manipulation.</remarks>
            [Parameter("Id", DbType.Int32, Direction = ParameterDirection.Input)]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the value associated with the NotAParameter property.
            /// </summary>
            public string NotAParameter { get; set; }
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

        /// <summary>
        /// Verifies that the binder excludes write-only properties when binding parameters to a command.
        /// </summary>
        /// <remarks>This test ensures that only properties with accessible getters are included as
        /// parameters during the binding process. Write-only properties are intentionally omitted to prevent unintended
        /// or unsupported parameter bindings.</remarks>
        [Fact]
        public void Binder_DoesNotIncludeWriteOnlyProperty()
        {
            var binder = ProcedureBinderCache.GetOrAdd(typeof(ProcedureWithWriteOnlyProperty));
            var command = new DummyCommand();
            var proc = new ProcedureWithWriteOnlyProperty { Id = 42 };
            var added = new List<string>();
            binder.BindParameters(command, proc, "@", (cmd, name, value, dir, dbType) => added.Add(name));
            Assert.Contains("@Id", added);
            Assert.DoesNotContain("@WriteOnly", added);
        }

        /// <summary>
        /// Verifies that the parameter binder excludes indexer properties when binding parameters for a procedure
        /// object.
        /// </summary>
        /// <remarks>This test ensures that only explicitly defined properties, and not indexers, are
        /// included in the parameter binding process. This helps prevent unintended parameters from being bound when
        /// objects with indexer properties are used.</remarks>
        [Fact]
        public void Binder_DoesNotIncludeIndexerProperty()
        {
            var binder = ProcedureBinderCache.GetOrAdd(typeof(ProcedureWithIndexer));
            var command = new DummyCommand();
            var proc = new ProcedureWithIndexer { Id = 42 };
            var added = new List<string>();
            binder.BindParameters(command, proc, "@", (cmd, name, value, dir, dbType) => added.Add(name));
            Assert.Contains("@Id", added);
            Assert.True(added.Count == 1);
        }

        /// <summary>
        /// Verifies that properties without the ParameterAttribute are not included in the binding process.
        /// </summary>
        /// <remarks>This test ensures that only properties marked with the ParameterAttribute are
        /// considered for parameter binding, preventing unintended properties from being included.</remarks>
        [Fact]
        public void Binder_DoesNotIncludePropertyWithoutParameterAttribute()
        {
            var binder = ProcedureBinderCache.GetOrAdd(typeof(ProcedureWithNoParameterAttribute));
            var command = new DummyCommand();
            var proc = new ProcedureWithNoParameterAttribute { Id = 42, NotAParameter = "ignore" };
            var added = new List<string>();
            binder.BindParameters(command, proc, "@", (cmd, name, value, dir, dbType) => added.Add(name));
            Assert.Contains("@Id", added);
            Assert.DoesNotContain("@NotAParameter", added);
        }

        /// <summary>
        /// Verifies that the parameter token replacement logic in SQL query strings behaves correctly across a variety
        /// of scenarios.
        /// </summary>
        /// <remarks>This test method covers multiple edge cases, including parameter tokens within string
        /// literals, comments, and as substrings of other identifiers, to ensure that only valid occurrences are
        /// replaced and SQL syntax is preserved.</remarks>
        /// <param name="sql">The SQL query string in which the parameter token is to be replaced.</param>
        /// <param name="token">The parameter token to search for and replace within the SQL query string.</param>
        /// <param name="replacement">The new token that will replace the specified parameter token in the SQL query string.</param>
        /// <param name="expected">The expected SQL query string result after the parameter token has been replaced.</param>
        [Theory]
        [InlineData("SELECT * FROM t WHERE @X", "@X", "@Y", "SELECT * FROM t WHERE @Y")]
        [InlineData("SELECT '@X' -- @X\nWHERE @X", "@X", "@Y", "SELECT '@X' -- @X\nWHERE @Y")]
        [InlineData("SELECT '@X''@X' WHERE @X", "@X", "@Y", "SELECT '@X''@X' WHERE @Y")]
        [InlineData("SELECT \"@X\" /* @X */ WHERE @X", "@X", "@Y", "SELECT \"@X\" /* @X */ WHERE @Y")]
        [InlineData("SELECT * FROM t WHERE a_@X = 1", "@X", "@Y", "SELECT * FROM t WHERE a_@X = 1")]
        [InlineData("SELECT * FROM t WHERE @X1 = 1", "@X", "@Y", "SELECT * FROM t WHERE @X1 = 1")]
        [InlineData("SELECT * FROM t WHERE @X = 1", "", "@Y", "SELECT * FROM t WHERE @X = 1")]
        [InlineData("", "@X", "@Y", "")]
        [InlineData("@X", "@X", "@Y", "@Y")]
        [InlineData("@X @X", "@X", "@Y", "@Y @Y")]
        [InlineData("SELECT * FROM t WHERE @X", "@Z", "@Y", "SELECT * FROM t WHERE @X")]
        [InlineData("SELECT * FROM t WHERE @X", "@X", "", "SELECT * FROM t WHERE ")]
        [InlineData("SELECT * FROM t WHERE @X AND @X", "@X", "@Y", "SELECT * FROM t WHERE @Y AND @Y")]
        public void ReplaceParameterToken_CoversAllBranches(
            string sql,
            string token,
            string replacement,
            string expected)
        {
            var result = InvokeReplaceParameterToken(sql, token, replacement);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Verifies that the parameter token is not replaced when it appears inside a block comment in the SQL query.
        /// </summary>
        /// <remarks>This test ensures that the replacement logic correctly ignores tokens within
        /// comments, preserving the original comment content while replacing tokens outside of comments.</remarks>
        [Fact]
        public void ReplaceParameterToken_DoesNotReplaceInsideBlockComment()
        {
            var sql = "SELECT /* @X */ @X";
            var result = InvokeReplaceParameterToken(sql, "@X", "@Y");
            Assert.Equal("SELECT /* @X */ @Y", result);
        }

        /// <summary>
        /// Verifies that the parameter token is not replaced when it appears inside a line comment in the SQL query.
        /// </summary>
        /// <remarks>This test ensures that the replacement logic correctly ignores tokens that are
        /// commented out, preserving the original comment content.</remarks>
        [Fact]
        public void ReplaceParameterToken_DoesNotReplaceInsideLineComment()
        {
            var sql = "SELECT -- @X\n@X";
            var result = InvokeReplaceParameterToken(sql, "@X", "@Y");
            Assert.Equal("SELECT -- @X\n@Y", result);
        }

        /// <summary>
        /// Verifies that an InvalidOperationException is thrown when an empty enumerable parameter is passed to the
        /// command.
        /// </summary>
        /// <remarks>This test ensures that the command correctly handles the case where the enumerable
        /// parameter is empty, providing a clear error message to the user.</remarks>
        [Fact]
        public void ExpandEnumerableParameter_ThrowsIfEmpty()
        {
            var command = new DummyCommand { CommandText = "WHERE @X" };
            var ex = Assert.Throws<TargetInvocationException>(() =>
                InvokeExpandEnumerableParameter(
                    command,
                    "X",
                    "@",
                    new List<object>()
                )
            );
            Assert.IsType<InvalidOperationException>(ex.InnerException);
            Assert.Contains("Enumerable parameter 'X' is empty.", ex.InnerException.Message);
        }

        /// <summary>
        /// Verifies that parameter tokens within single quotes are not replaced when escaped in the SQL query.
        /// </summary>
        /// <remarks>This test ensures that the parameter replacement logic preserves parameter tokens
        /// inside single-quoted strings, even when escape sequences are present. Maintaining the original token format
        /// within quoted sections is important for SQL statement integrity.</remarks>
        [Fact]
        public void ReplaceParameterToken_DoesNotReplaceInsideSingleQuoteWithEscape()
        {
            var sql = "SELECT '@X''@X' @X";
            var result = InvokeReplaceParameterToken(sql, "@X", "@Y");
            Assert.Equal("SELECT '@X''@X' @Y", result);
        }

        /// <summary>
        /// Verifies that the parameter token replacement method does not alter tokens enclosed within double quotes in
        /// a SQL string.
        /// </summary>
        /// <remarks>This test ensures that the parameter token '@X' is only replaced when it is outside
        /// of double quotes, preserving quoted SQL syntax. Use this test to confirm correct handling of parameter
        /// tokens in scenarios where quoted identifiers or literals are present.</remarks>
        [Fact]
        public void ReplaceParameterToken_DoesNotReplaceInsideDoubleQuote()
        {
            var sql = "SELECT \"@X\" @X";
            var result = InvokeReplaceParameterToken(sql, "@X", "@Y");
            Assert.Equal("SELECT \"@X\" @Y", result);
        }

        /// <summary>
        /// Verifies that the parameter token is not replaced when it is part of an identifier in the SQL query.
        /// </summary>
        /// <remarks>This test ensures that the replacement logic correctly identifies tokens that are
        /// part of identifiers and does not alter them, maintaining the integrity of the SQL statement.</remarks>
        [Fact]
        public void ReplaceParameterToken_DoesNotReplaceIfTokenIsPartOfIdentifier()
        {
            var sql = "SELECT * FROM t WHERE a_@X = 1";
            var result = InvokeReplaceParameterToken(sql, "@X", "@Y");
            Assert.Equal("SELECT * FROM t WHERE a_@X = 1", result);
        }

        /// <summary>
        /// Verifies that the SQL query remains unchanged when attempting to replace a parameter token that does not
        /// exist in the query.
        /// </summary>
        /// <remarks>This test ensures that the method correctly handles cases where the specified token
        /// is not present, maintaining the integrity of the original SQL query.</remarks>
        [Fact]
        public void ReplaceParameterToken_DoesNotReplaceIfTokenDoesNotExist()
        {
            var sql = "SELECT * FROM t WHERE @Z = 1";
            var result = InvokeReplaceParameterToken(sql, "@X", "@Y");
            Assert.Equal("SELECT * FROM t WHERE @Z = 1", result);
        }

        /// <summary>
        /// Verifies that the parameter token replacement method correctly substitutes all occurrences of a specified
        /// token at both the start and end of a SQL string.
        /// </summary>
        /// <remarks>This test ensures that the replacement logic handles tokens appearing at the
        /// boundaries of the input string, confirming comprehensive coverage for parameter token updates in SQL
        /// queries.</remarks>
        [Fact]
        public void ReplaceParameterToken_ReplacesTokenAtStartAndEnd()
        {
            var sql = "@X something @X";
            var result = InvokeReplaceParameterToken(sql, "@X", "@Y");
            Assert.Equal("@Y something @Y", result);
        }

        /// <summary>
        /// Verifies that the parameter token replacement logic correctly handles tokens containing special characters
        /// in a SQL query string.
        /// </summary>
        /// <remarks>This test ensures that the method responsible for replacing parameter tokens can
        /// accommodate tokens with special characters, maintaining the integrity of the SQL syntax during
        /// replacement.</remarks>
        [Fact]
        public void ReplaceParameterToken_ReplacesTokenWithSpecialCharacters()
        {
            var sql = "SELECT * FROM t WHERE @X$ = 1";
            var result = InvokeReplaceParameterToken(sql, "@X$", "@Y$");
            Assert.Equal("SELECT * FROM t WHERE @Y$ = 1", result);
        }

        /// <summary>
        /// Verifies that the ReplaceParameterToken method replaces all occurrences of a specified parameter token in a
        /// SQL string with a new token.
        /// </summary>
        /// <remarks>This test ensures that when multiple instances of the original token are present in
        /// the input SQL string, each is correctly replaced by the new token. It is useful for validating scenarios
        /// where parameterized SQL queries require dynamic token substitution.</remarks>
        [Fact]
        public void ReplaceParameterToken_ReplacesMultipleTokens()
        {
            var sql = "@X @X @X";
            var result = InvokeReplaceParameterToken(sql, "@X", "@Y");
            Assert.Equal("@Y @Y @Y", result);
        }

        /// <summary>
        /// Verifies that a SQL line comment ending with a newline character is correctly processed and that the parser
        /// transitions to the expected state.
        /// </summary>
        /// <remarks>This test ensures that when a line comment in SQL ends with a newline, the comment is
        /// recognized and the parsing state returns to normal. It also checks that the output begins with a hyphen,
        /// indicating correct comment handling.</remarks>
        [Fact]
        public void LineComment_EndsWithNewline()
        {
            var sql = "-- comment\n";
            var builder = new StringBuilder();
            object state = Enum.Parse(typeof(ParameterObjectBinder).GetNestedType("SqlScanState", BindingFlags.NonPublic), "LineComment");
            int index = 0;
            var result = InvokeTryHandleCommentOrString(sql, builder, ref state, ref index);
            Assert.True(result);
            Assert.Equal("-", builder.ToString()[0].ToString());
            Assert.Equal("LineComment", state.ToString());
        }

        /// <summary>
        /// Verifies that a SQL line comment without a terminating newline is correctly recognized and processed by the
        /// comment handler.
        /// </summary>
        /// <remarks>This test ensures that the handler transitions to the 'LineComment' state and that
        /// the output begins with a hyphen, indicating the start of a line comment. It validates correct state
        /// management and output for SQL comments that do not end with a newline character.</remarks>
        [Fact]
        public void LineComment_WithoutNewline()
        {
            var sql = "-- comment";
            var builder = new StringBuilder();
            object state = Enum.Parse(typeof(ParameterObjectBinder).GetNestedType("SqlScanState", BindingFlags.NonPublic), "LineComment");
            int index = 0;
            var result = InvokeTryHandleCommentOrString(sql, builder, ref state, ref index);
            Assert.True(result);
            Assert.Equal("-", builder.ToString()[0].ToString());
            Assert.Equal("LineComment", state.ToString());
        }

        /// <summary>
        /// Verifies that a block comment ending with a star and a slash ("*/") is correctly handled by the parser.
        /// </summary>
        /// <remarks>This test ensures that the parser transitions from the block comment state to the
        /// normal state after processing a comment that ends with "*/". It also checks that the comment content is
        /// accurately captured.</remarks>
        [Fact]
        public void BlockComment_EndsWithStarSlash()
        {
            var sql = "* /".Replace(" ", "/"); // "*/"
            var builder = new StringBuilder();
            object state = Enum.Parse(typeof(ParameterObjectBinder).GetNestedType("SqlScanState", BindingFlags.NonPublic), "BlockComment");
            int index = 0;
            var result = InvokeTryHandleCommentOrString(sql, builder, ref state, ref index);
            Assert.True(result);
            Assert.Equal("*/", builder.ToString());
            Assert.Equal("Normal", state.ToString());
        }

        /// <summary>
        /// Verifies that a block comment without a closing star-slash sequence is correctly identified and processed by
        /// the comment handling logic.
        /// </summary>
        /// <remarks>This test ensures that the method under test recognizes the start of a block comment
        /// and updates the parsing state accordingly, even when the comment is not properly terminated. It also checks
        /// that the output matches the expected result and that the state reflects the block comment type after
        /// processing.</remarks>
        [Fact]
        public void BlockComment_WithoutStarSlash()
        {
            var sql = "*abc";
            var builder = new StringBuilder();
            object state = Enum.Parse(typeof(ParameterObjectBinder).GetNestedType("SqlScanState", BindingFlags.NonPublic), "BlockComment");
            int index = 0;
            var result = InvokeTryHandleCommentOrString(sql, builder, ref state, ref index);
            Assert.True(result);
            Assert.Equal("*", builder.ToString());
            Assert.Equal("BlockComment", state.ToString());
        }

        /// <summary>
        /// Verifies that a single quote character at the end of a string is handled correctly without requiring escape
        /// characters.
        /// </summary>
        /// <remarks>This test ensures that the method under test processes a string containing only a
        /// single quote, produces the expected output, and transitions the internal state to 'Normal'. It validates
        /// correct handling of edge cases involving single quotes in SQL-like parsing scenarios.</remarks>
        [Fact]
        public void SingleQuote_EndsWithSingleQuote_NoEscape()
        {
            var sql = "'";
            var builder = new StringBuilder();
            object state = Enum.Parse(typeof(ParameterObjectBinder).GetNestedType("SqlScanState", BindingFlags.NonPublic), "SingleQuote");
            int index = 0;
            var result = InvokeTryHandleCommentOrString(sql, builder, ref state, ref index);
            Assert.True(result);
            Assert.Equal("'", builder.ToString());
            Assert.Equal("Normal", state.ToString());
        }

        /// <summary>
        /// Verifies that single quotes within SQL strings are correctly escaped and that the parameter object binder
        /// transitions to the expected 'SingleQuote' state.
        /// </summary>
        /// <remarks>This test ensures that when processing a SQL string containing escaped single quotes,
        /// the output matches the expected format and the internal state reflects the correct parsing state. It is
        /// useful for validating the handling of SQL string literals that require escaping.</remarks>
        [Fact]
        public void SingleQuote_WithEscape()
        {
            var sql = "''";
            var builder = new StringBuilder();
            object state = Enum.Parse(typeof(ParameterObjectBinder).GetNestedType("SqlScanState", BindingFlags.NonPublic), "SingleQuote");
            int index = 0;
            var result = InvokeTryHandleCommentOrString(sql, builder, ref state, ref index);
            Assert.True(result);
            Assert.Equal("''", builder.ToString());
            Assert.Equal("SingleQuote", state.ToString());
        }

        /// <summary>
        /// Verifies that the SQL parameter binder correctly processes a string input containing no single quotes when
        /// in the SingleQuote state.
        /// </summary>
        /// <remarks>This test ensures that the input string is handled without introducing additional
        /// single quotes and that the binder's state remains unchanged after processing. It is intended to validate the
        /// correct behavior of the parameter binder when handling simple string values within single-quoted SQL
        /// contexts.</remarks>
        [Fact]
        public void SingleQuote_WithoutSingleQuote()
        {
            var sql = "a";
            var builder = new StringBuilder();
            object state = Enum.Parse(typeof(ParameterObjectBinder).GetNestedType("SqlScanState", BindingFlags.NonPublic), "SingleQuote");
            int index = 0;
            var result = InvokeTryHandleCommentOrString(sql, builder, ref state, ref index);
            Assert.True(result);
            Assert.Equal("a", builder.ToString());
            Assert.Equal("SingleQuote", state.ToString());
        }

        /// <summary>
        /// Verifies that a string consisting of a single double quote character is correctly handled by the parameter
        /// object binder, ensuring the double quote is retained and the state transitions as expected.
        /// </summary>
        /// <remarks>This test confirms that when the input is a single double quote, the binder processes
        /// it without removing the quote and updates the internal state to 'Normal'. This scenario helps ensure correct
        /// handling of edge cases involving quoted strings.</remarks>
        [Fact]
        public void DoubleQuote_EndsWithDoubleQuote()
        {
            var sql = "\"";
            var builder = new StringBuilder();
            object state = Enum.Parse(typeof(ParameterObjectBinder).GetNestedType("SqlScanState", BindingFlags.NonPublic), "DoubleQuote");
            int index = 0;
            var result = InvokeTryHandleCommentOrString(sql, builder, ref state, ref index);
            Assert.True(result);
            Assert.Equal("\"", builder.ToString());
            Assert.Equal("Normal", state.ToString());
        }

        /// <summary>
        /// Verifies that the method correctly processes a string without double quotes when handling SQL string parsing
        /// in double-quote mode.
        /// </summary>
        /// <remarks>This test ensures that the input string is appended to the builder as expected and
        /// that the parsing state remains in double-quote mode. It confirms that the method can handle simple, unquoted
        /// strings without introducing additional quotes or altering the parsing state.</remarks>
        [Fact]
        public void DoubleQuote_WithoutDoubleQuote()
        {
            var sql = "a";
            var builder = new StringBuilder();
            object state = Enum.Parse(typeof(ParameterObjectBinder).GetNestedType("SqlScanState", BindingFlags.NonPublic), "DoubleQuote");
            int index = 0;
            var result = InvokeTryHandleCommentOrString(sql, builder, ref state, ref index);
            Assert.True(result);
            Assert.Equal("a", builder.ToString());
            Assert.Equal("DoubleQuote", state.ToString());
        }

        /// <summary>
        /// Verifies that a line comment in SQL syntax is correctly identified and processed by the comment or string
        /// handler.
        /// </summary>
        /// <remarks>This test ensures that when a line comment is encountered, it is properly recognized,
        /// appended to the builder, and the parser state transitions to indicate a line comment. It validates the
        /// expected behavior of the comment handling logic in the parameter object binder.</remarks>
        [Fact]
        public void Normal_EntersLineComment()
        {
            var sql = "--";
            var builder = new StringBuilder();
            object state = Enum.Parse(typeof(ParameterObjectBinder).GetNestedType("SqlScanState", BindingFlags.NonPublic), "Normal");
            int index = 0;
            var result = InvokeTryHandleCommentOrString(sql, builder, ref state, ref index);
            Assert.True(result);
            Assert.Equal("--", builder.ToString());
            Assert.Equal("LineComment", state.ToString());
        }

        /// <summary>
        /// Verifies that the parser correctly enters the block comment state when encountering the start of a SQL block
        /// comment.
        /// </summary>
        /// <remarks>This test ensures that when the input SQL string contains the opening block comment
        /// sequence ("/*"), the parser updates its state to indicate a block comment and appends the sequence to the
        /// output builder. This behavior is essential for accurate parsing of SQL statements that include
        /// comments.</remarks>
        [Fact]
        public void Normal_EntersBlockComment()
        {
            var sql = "/*";
            var builder = new StringBuilder();
            object state = Enum.Parse(typeof(ParameterObjectBinder).GetNestedType("SqlScanState", BindingFlags.NonPublic), "Normal");
            int index = 0;
            var result = InvokeTryHandleCommentOrString(sql, builder, ref state, ref index);
            Assert.True(result);
            Assert.Equal("/*", builder.ToString());
            Assert.Equal("BlockComment", state.ToString());
        }

        /// <summary>
        /// Verifies that entering a single quote character in a SQL context transitions the state to 'SingleQuote' and
        /// appends the character to the output.
        /// </summary>
        /// <remarks>This test ensures that the method under test correctly processes a single quote
        /// character by updating the parsing state and output as expected. It is intended to validate proper handling
        /// of string delimiters in SQL parsing scenarios.</remarks>
        [Fact]
        public void Normal_EntersSingleQuote()
        {
            var sql = "'";
            var builder = new StringBuilder();
            object state = Enum.Parse(typeof(ParameterObjectBinder).GetNestedType("SqlScanState", BindingFlags.NonPublic), "Normal");
            int index = 0;
            var result = InvokeTryHandleCommentOrString(sql, builder, ref state, ref index);
            Assert.True(result);
            Assert.Equal("'", builder.ToString());
            Assert.Equal("SingleQuote", state.ToString());
        }

        /// <summary>
        /// Verifies that entering a double quote character in a SQL string is correctly handled by the SQL parser.
        /// </summary>
        /// <remarks>This test ensures that the parser transitions to the expected 'DoubleQuote' state and
        /// that the double quote character is properly appended to the output. It validates correct handling of quoted
        /// strings in SQL parsing scenarios.</remarks>
        [Fact]
        public void Normal_EntersDoubleQuote()
        {
            var sql = "\"";
            var builder = new StringBuilder();
            object state = Enum.Parse(typeof(ParameterObjectBinder).GetNestedType("SqlScanState", BindingFlags.NonPublic), "Normal");
            int index = 0;
            var result = InvokeTryHandleCommentOrString(sql, builder, ref state, ref index);
            Assert.True(result);
            Assert.Equal("\"", builder.ToString());
            Assert.Equal("DoubleQuote", state.ToString());
        }

        /// <summary>
        /// Verifies that the method under test returns false when provided with characters that are not valid SQL
        /// parameters and does not alter the output builder or state.
        /// </summary>
        /// <remarks>This test ensures that non-SQL parameter characters are correctly identified and
        /// ignored, maintaining the integrity of the builder and state. It is useful for confirming that the method
        /// does not produce side effects when encountering irrelevant input.</remarks>
        [Fact]
        public void Normal_ReturnsFalseForOtherChars()
        {
            var sql = "a";
            var builder = new StringBuilder();
            object state = Enum.Parse(typeof(ParameterObjectBinder).GetNestedType("SqlScanState", BindingFlags.NonPublic), "Normal");
            int index = 0;
            var result = InvokeTryHandleCommentOrString(sql, builder, ref state, ref index);
            Assert.False(result);
            Assert.Equal("", builder.ToString());
            Assert.Equal("Normal", state.ToString());
        }

        /// <summary>
        /// Verifies that the method returns false when the input string contains characters other than valid SQL
        /// comment or string delimiters.
        /// </summary>
        /// <remarks>This test ensures that the method correctly identifies unsupported input and does not
        /// modify the output builder or the scan state when encountering invalid characters. It is useful for
        /// confirming that only recognized SQL comment or string delimiters are processed.</remarks>
        [Fact]
        public void Unknown_ReturnsFalseForOtherChars()
        {
            var sql = "a";
            var builder = new StringBuilder();
            var enumType = typeof(ParameterObjectBinder).GetNestedType("SqlScanState", BindingFlags.NonPublic);
            object state = Enum.ToObject(enumType, -1);
            int index = 0;
            var result = InvokeTryHandleCommentOrString(sql, builder, ref state, ref index);
            Assert.False(result);
            Assert.Equal("", builder.ToString());
            Assert.Equal("-1", state.ToString());
        }

        /// <summary>
        /// Verifies that the parameter token replacement method correctly handles an unknown enum state by leaving the
        /// output unchanged and preserving the original state value.
        /// </summary>
        /// <remarks>This test ensures that when an invalid enum value is provided to the method, the
        /// output builder remains unmodified and the state is retained as its string representation. This behavior is
        /// important for maintaining predictable results when encountering unexpected enum values.</remarks>
        [Fact]
        public void ReplaceParameterToken_HandlesUnknownEnumState()
        {
            var sql = "abcdefghijklmnopqrstuvwxyz";
            var token = "abcdefghijklmnopqrstuvwxyz";
            var replacement = "@Y";
            object state = Enum.Parse(typeof(ParameterObjectBinder).GetNestedType("SqlScanState", BindingFlags.NonPublic), "Normal");
            var builder = new StringBuilder();

            var method = typeof(ParameterObjectBinder).GetMethod("ReplaceParameterToken", BindingFlags.NonPublic | BindingFlags.Static);
            method.Invoke(null, new object[] { sql, token, replacement });

            Assert.Equal("", builder.ToString());
            Assert.Equal("Normal", state.ToString());
        }

        /// <summary>
        /// Invokes a non-public method to process a SQL string for comments or string literals, updating the provided
        /// state and index as needed.
        /// </summary>
        /// <remarks>This method uses reflection to invoke a non-public static method, which may have
        /// performance implications. Ensure that the parameters are properly initialized before calling this
        /// method.</remarks>
        /// <param name="sql">The SQL string to be analyzed for comments or string literals.</param>
        /// <param name="builder">A StringBuilder instance used to accumulate the processed output.</param>
        /// <param name="state">A reference to an object representing the current parsing state. This value may be modified by the method.</param>
        /// <param name="index">A reference to the current position within the SQL string. This value will be updated to reflect the new
        /// position after processing.</param>
        /// <returns>true if a comment or string literal was successfully handled; otherwise, false.</returns>
        private static bool InvokeTryHandleCommentOrString(
            string sql,
            StringBuilder builder,
            ref object state,
            ref int index)
        {
            var method = typeof(ParameterObjectBinder).GetMethod("TryHandleCommentOrString", BindingFlags.NonPublic | BindingFlags.Static);
            object[] parameters = new object[] { sql, builder, state, index };
            var result = (bool)method.Invoke(null, parameters);
            state = parameters[2];
            index = (int)parameters[3];
            return result;
        }

        /// <summary>
        /// Replaces the specified parameter token in the provided SQL string with the given replacement value.
        /// </summary>
        /// <remarks>This method uses reflection to invoke a non-public static method for parameter token
        /// replacement. It is intended for scenarios where direct access to the underlying method is not
        /// available.</remarks>
        /// <param name="sql">The SQL string in which to replace the parameter token.</param>
        /// <param name="token">The parameter token to be replaced within the SQL string.</param>
        /// <param name="replacement">The value to substitute for the specified parameter token.</param>
        /// <returns>A new SQL string with the specified parameter token replaced by the provided replacement value.</returns>
        private static string InvokeReplaceParameterToken(
            string sql,
            string token,
            string replacement)
        {
            var method = typeof(ParameterObjectBinder).GetMethod("ReplaceParameterToken", BindingFlags.NonPublic | BindingFlags.Static);
            return (string)method.Invoke(null, new object[] { sql, token, replacement });
        }

        /// <summary>
        /// Invokes the non-public static ExpandEnumerableParameter method to bind a collection of values to a parameter
        /// in the specified database command.
        /// </summary>
        /// <remarks>This method uses reflection to invoke a non-public method, which may have performance
        /// implications. Ensure that the values collection is not null before calling this method.</remarks>
        /// <param name="command">The database command to which the enumerable parameter values will be bound. Cannot be null.</param>
        /// <param name="name">The name of the parameter in the command that will receive the enumerable values. Cannot be null or empty.</param>
        /// <param name="prefix">A prefix to apply to the parameter names when binding the values.</param>
        /// <param name="values">An enumerable collection of values to bind to the command parameter. Cannot be null.</param>
        private static void InvokeExpandEnumerableParameter(IDbCommand command, string name, string prefix, System.Collections.IEnumerable values)
        {
            var method = typeof(ParameterObjectBinder).GetMethod("ExpandEnumerableParameter", BindingFlags.NonPublic | BindingFlags.Static);
            method.Invoke(null, new object[] { command, name, prefix, values });
        }
    }
}