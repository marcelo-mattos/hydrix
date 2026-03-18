using Hydrix.Engines;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Hydrix.UnitTests.Engines
{
    /// <summary>
    /// Contains unit tests for the ParameterEngine class, verifying parameter handling and formatting behaviors.
    /// </summary>
    /// <remarks>This test class uses mock implementations of database command and parameter interfaces to
    /// simulate database operations without requiring a real data source. The tests cover scenarios such as adding
    /// parameters, handling null values, formatting parameter values for various types, and binding parameters from
    /// objects. These tests help ensure the correctness and robustness of the ParameterEngine's parameter management
    /// logic.</remarks>
    public class ParameterEngineTests
    {
        /// <summary>
        /// Represents a non-formattable value used to validate fallback string formatting behavior.
        /// </summary>
        private class NonFormattableValue
        {
            /// <summary>
            /// Returns the string representation for test assertions.
            /// </summary>
            /// <returns>A deterministic string value.</returns>
            public override string ToString() => "non-formattable";
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
        /// Verifies that the AddParameter method correctly adds a parameter with the specified name and value to the
        /// command.
        /// </summary>
        /// <remarks>This test ensures that the AddParameter method assigns both the parameter name and
        /// value as expected when invoked via reflection. It is intended to validate the behavior of the internal
        /// AddParameter logic.</remarks>
        [Fact]
        public void AddParameter_AddsParameterWithCorrectNameAndValue()
        {
            var cmd = new FakeDbCommand();
            var name = "@foo";
            var value = 123;
            var addParamMethod = typeof(ParameterEngine).GetMethod("AddParameter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            addParamMethod.Invoke(null, new object[] { cmd, name, value });
            var param = (IDbDataParameter)cmd.Parameters[0];
            Assert.Equal(name, param.ParameterName);
            Assert.Equal(value, param.Value);
        }

        /// <summary>
        /// Verifies that adding a parameter with a null value results in the parameter's value being set to
        /// DBNull.Value.
        /// </summary>
        /// <remarks>This test ensures that the AddParameter method correctly handles null values by
        /// converting them to DBNull.Value, which is required for database operations that do not accept null
        /// references. This behavior is important for compatibility with ADO.NET data providers.</remarks>
        [Fact]
        public void AddParameter_NullValue_UsesDBNull()
        {
            var cmd = new FakeDbCommand();
            var name = "@bar";
            object value = null;
            var addParamMethod = typeof(ParameterEngine).GetMethod("AddParameter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            addParamMethod.Invoke(null, new object[] { cmd, name, value });
            var param = (IDbDataParameter)cmd.Parameters[0];
            Assert.Equal(DBNull.Value, param.Value);
        }

        /// <summary>
        /// Verifies that the FormatParameterValue method correctly formats common parameter types to their expected
        /// string representations.
        /// </summary>
        /// <param name="value">The input value to be formatted. Can be null, a string, an integer, or a boolean.</param>
        /// <param name="expected">The expected string representation of the formatted value.</param>
        [Theory]
        [InlineData(null, "NULL")]
        [InlineData("abc", "'abc'")]
        [InlineData(123, "123")]
        [InlineData(true, "1")]
        [InlineData(false, "0")]
        public void FormatParameterValue_HandlesCommonTypes(object value, string expected)
        {
            var result = ParameterEngine.FormatParameterValue(value);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Verifies that the FormatParameterValue method correctly formats a DateTime value as a string with
        /// millisecond precision.
        /// </summary>
        /// <remarks>This test ensures that DateTime values are formatted in the expected SQL-compatible
        /// string format, including milliseconds, when passed to the FormatParameterValue method.</remarks>
        [Fact]
        public void FormatParameterValue_FormatsDateTime()
        {
            var dt = new DateTime(2023, 1, 2, 3, 4, 5, 678);
            var expected = "'2023-01-02 03:04:05.678'";
            var result = ParameterEngine.FormatParameterValue(dt);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Verifies that the FormatParameterValue method correctly formats a Guid value by enclosing it in single
        /// quotes.
        /// </summary>
        /// <remarks>This test ensures that when a Guid is passed to FormatParameterValue, the resulting
        /// string matches the expected format used for parameterized queries or logging scenarios.</remarks>
        [Fact]
        public void FormatParameterValue_FormatsGuid()
        {
            var guid = Guid.NewGuid();
            var expected = $"'{guid}'";
            var result = ParameterEngine.FormatParameterValue(guid);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Verifies that string values are SQL-escaped when formatted for logging.
        /// </summary>
        [Fact]
        public void FormatParameterValue_EscapesSingleQuotes_InStringValues()
        {
            var value = "O'Reilly";

            var result = ParameterEngine.FormatParameterValue(value);

            Assert.Equal("'O''Reilly'", result);
        }

        /// <summary>
        /// Verifies that empty string values are formatted as quoted empty literals.
        /// </summary>
        [Fact]
        public void FormatParameterValue_FormatsEmptyString_AsQuotedEmptyLiteral()
        {
            var result = ParameterEngine.FormatParameterValue(string.Empty);

            Assert.Equal("''", result);
        }

        /// <summary>
        /// Verifies that DateTimeOffset values are formatted with offset information.
        /// </summary>
        [Fact]
        public void FormatParameterValue_FormatsDateTimeOffset()
        {
            var value = new DateTimeOffset(2023, 1, 2, 3, 4, 5, 678, TimeSpan.FromHours(2));

            var result = ParameterEngine.FormatParameterValue(value);

            Assert.Equal("'2023-01-02 03:04:05.678 +02:00'", result);
        }

        /// <summary>
        /// Verifies that numeric formattable values use invariant culture when formatted.
        /// </summary>
        [Fact]
        public void FormatParameterValue_UsesInvariantCulture_ForFormattableValues()
        {
            var originalCulture = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
                object value = 1234.56m;

                var result = ParameterEngine.FormatParameterValue(value);

                Assert.Equal("1234.56", result);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        /// <summary>
        /// Verifies that non-formattable values use the fallback ToString formatting branch.
        /// </summary>
        [Fact]
        public void FormatParameterValue_UsesToStringFallback_ForNonFormattableValues()
        {
            object value = new NonFormattableValue();

            var result = ParameterEngine.FormatParameterValue(value);

            Assert.Equal("non-formattable", result);
        }

        /// <summary>
        /// Verifies that the FormatParameterValue method returns the string "NULL" when provided with a DBNull value.
        /// </summary>
        /// <remarks>This test ensures that database null values are correctly formatted as the string
        /// "NULL" for parameter handling scenarios.</remarks>
        [Fact]
        public void FormatParameterValue_DBNull()
        {
            var result = ParameterEngine.FormatParameterValue(DBNull.Value);
            Assert.Equal("NULL", result);
        }

        /// <summary>
        /// Verifies that calling BindParametersFromObject with a null parameters object does not add any parameters to
        /// the command.
        /// </summary>
        /// <remarks>This test ensures that the BindParametersFromObject method handles null parameter
        /// objects gracefully by leaving the command's parameter collection unchanged.</remarks>
        [Fact]
        public void BindParametersFromObject_NullParameters_DoesNothing()
        {
            var cmd = new FakeDbCommand();
            ParameterEngine.BindParametersFromObject(cmd, null, "@");
            Assert.Empty(cmd.Parameters);
        }
    }
}