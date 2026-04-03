using Hydrix.Binders.Procedure;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Binders.Procedure
{
    /// <summary>
    /// Contains unit tests for the ProcedureBinder class, validating its behavior when applying commands and binding
    /// parameters.
    /// </summary>
    /// <remarks>These tests ensure that the ProcedureBinder correctly sets command types and texts, binds
    /// parameters with prefixes, handles null values by using DBNull, and does nothing when no parameters are
    /// provided.</remarks>
    public class ProcedureBinderTests
    {
        /// <summary>
        /// Represents a database command that can be executed against a data source, providing properties and methods
        /// to configure and run SQL statements or stored procedures.
        /// </summary>
        /// <remarks>Implements the IDbCommand interface, allowing for the execution of commands within a
        /// database context. The class exposes properties for command text, timeout, type, connection, parameters,
        /// transaction, and updated row source. Methods are provided to execute commands, retrieve results, and manage
        /// command preparation and cancellation. Use this type to configure and execute database operations in a manner
        /// consistent with ADO.NET patterns.</remarks>
        private class DummyCommand :
            IDbCommand
        {
            /// <summary>
            /// Gets or sets the SQL command text to be executed against the database.
            /// </summary>
            /// <remarks>The command text can include parameters that need to be defined separately.
            /// Ensure that the command text is properly formatted to avoid SQL injection vulnerabilities.</remarks>
            public string CommandText { get; set; }

            /// <summary>
            /// Gets or sets the maximum time, in seconds, before a command is considered to have timed out.
            /// </summary>
            /// <remarks>If the command does not complete within the specified time, an exception is
            /// thrown. The default value is 30 seconds.</remarks>
            public int CommandTimeout { get; set; }

            /// <summary>
            /// Gets or sets the type of command to execute.
            /// </summary>
            /// <remarks>Setting the appropriate command type determines how the command is processed
            /// when executed. Use this property to specify whether the command is a text command, a stored procedure,
            /// or a table direct operation.</remarks>
            public CommandType CommandType { get; set; }

            /// <summary>
            /// Gets or sets the connection to the data source that the command will be executed against.
            /// </summary>
            public IDbConnection Connection { get; set; }

            /// <summary>
            /// Gets the collection of parameters associated with the command.
            /// </summary>
            /// <remarks>This property provides access to the parameters that can be used in the
            /// command execution. It is essential for setting up command parameters dynamically before executing a
            /// command against a data source.</remarks>
            public IDataParameterCollection Parameters { get; } = new DummyParameterCollection();

            /// <summary>
            /// Gets or sets the database transaction associated with the current context.
            /// </summary>
            /// <remarks>This property allows for managing transactions within the database context.
            /// Ensure that the transaction is properly initialized before use, and be aware that operations performed
            /// without an active transaction may not be committed to the database.</remarks>
            public IDbTransaction Transaction { get; set; }

            /// <summary>
            /// Gets or sets a value that determines how the results of a command are applied to the data source after
            /// an insert, update, or delete operation.
            /// </summary>
            /// <remarks>The value of this property specifies whether the updated row is included in
            /// the result set and how output parameters are handled. Set this property to a member of the
            /// UpdateRowSource enumeration to control the behavior. Changing this property affects how the command
            /// updates the data source and may impact performance or result set contents.</remarks>
            public UpdateRowSource UpdatedRowSource { get; set; }

            /// <summary>
            /// Cancels the execution of a command. If the command is currently executing, it attempts to stop the execution
            /// </summary>
            public void Cancel()
            { }

            /// <summary>
            /// Creates a new instance of a database parameter suitable for use with database commands.
            /// </summary>
            /// <returns>An instance of <see cref="IDbDataParameter"/> representing the database parameter.</returns>
            public IDbDataParameter CreateParameter() => new DummyParameter();

            /// <summary>
            /// Disposes of the command object, releasing any resources associated with it. After calling this method, the
            /// command object should not be used.
            /// </summary>
            public void Dispose()
            { }

            /// <summary>
            /// Executes a command against the data source and returns the number of rows affected.
            /// </summary>
            /// <remarks>This method is typically used for executing SQL statements that do not return
            /// any result set, such as INSERT, UPDATE, or DELETE commands.</remarks>
            /// <returns>The number of rows affected by the command execution. Returns 0 if no rows are affected.</returns>
            public int ExecuteNonQuery() => 0;

            /// <summary>
            /// Executes the command and returns an IDataReader for reading data from the result set.
            /// </summary>
            /// <remarks>The caller is responsible for closing the IDataReader when it is no longer
            /// needed to free up resources. This method may throw exceptions if the command is not properly configured
            /// or if there are issues with the database connection.</remarks>
            /// <returns>An IDataReader that provides access to the data returned by the command. The reader is positioned before
            /// the first record.</returns>
            public IDataReader ExecuteReader() => null;

            /// <summary>
            /// Executes the command and returns an IDataReader for reading data from the result set.
            /// </summary>
            /// <remarks>This method is typically used when executing commands that return rows, such
            /// as SELECT statements. Ensure that the command is properly configured before calling this
            /// method.</remarks>
            /// <param name="behavior">Specifies the behavior of the command execution, which influences how the data is retrieved and how the
            /// connection is managed.</param>
            /// <returns>An IDataReader that provides a way to read a forward-only stream of rows from the result set.</returns>
            public IDataReader ExecuteReader(CommandBehavior behavior) => null;

            /// <summary>
            /// Executes the command and returns the value of the first column in the first row of the result set
            /// produced by the command.
            /// </summary>
            /// <remarks>Use this method when the command is expected to return a single value, such
            /// as an aggregate result. If the result set contains additional columns or rows, they are
            /// ignored.</remarks>
            /// <returns>An object representing the value of the first column of the first row in the result set, or null if the
            /// result set contains no rows.</returns>
            public object ExecuteScalar() => null;

            /// <summary>
            /// Prepares the system for operation by initializing necessary components.
            /// </summary>
            /// <remarks>This method should be called before any operations are performed to ensure
            /// that the system is in a ready state.</remarks>
            public void Prepare()
            { }
        }

        /// <summary>
        /// Represents a parameter to a command that is executed against a database, encapsulating its properties and
        /// behavior.
        /// </summary>
        /// <remarks>This class implements the IDbDataParameter interface, providing properties to define
        /// the parameter's characteristics such as its data type, size, and direction. It is designed for use in
        /// database operations where parameters are required for commands.</remarks>
        private class DummyParameter :
            IDbDataParameter
        {
            /// <summary>
            /// Gets or sets the precision of the numeric value, indicating the total number of digits that can be
            /// stored.
            /// </summary>
            /// <remarks>The precision value must be a positive integer. It defines the maximum number
            /// of significant digits that can be represented, which is important for ensuring accurate calculations and
            /// data representation.</remarks>
            public byte Precision { get; set; }

            /// <summary>
            /// Gets or sets the scale of the numeric value, indicating the number of decimal places to which the value is
            /// rounded.
            /// </summary>
            public byte Scale { get; set; }

            /// <summary>
            /// Gets or sets the maximum size of the data within the column. For string data types, this represents the maximum
            /// number of characters that can be stored. For binary data types, this represents the maximum number of bytes.
            /// </summary>
            public int Size { get; set; }

            /// <summary>
            /// Gets or sets the database type associated with the current instance.
            /// </summary>
            /// <remarks>This property is used to specify the type of database that the instance is
            /// interacting with, which can affect how data is processed and stored.</remarks>
            public DbType DbType { get; set; }

            /// <summary>
            /// Gets or sets the direction of the parameter, indicating whether it is used as an input, output, or
            /// input/output parameter in a database operation.
            /// </summary>
            /// <remarks>Use this property to specify how the parameter interacts with a command or
            /// stored procedure. Setting the correct direction is essential when executing database operations that
            /// require parameters to be passed in, returned, or both. The value should correspond to the intended usage
            /// in the database context, such as input for values sent to the database, output for values returned, or
            /// input/output for parameters that are both sent and received.</remarks>
            public ParameterDirection Direction { get; set; }

            /// <summary>
            /// Returns a boolean value indicating whether the parameter accepts null values.
            /// </summary>
            public bool IsNullable => false;

            /// <summary>
            /// Gets or sets the name of the parameter, which is used to identify it in a command or stored procedure.
            /// </summary>
            public string ParameterName { get; set; }

            /// <summary>
            /// Gets or sets the name of the source column that is mapped to the parameter. This property is used when
            /// performing data operations that involve mapping between a data source and a parameter.
            /// </summary>
            public string SourceColumn { get; set; }

            /// <summary>
            /// Gets or sets the version of the data row to use when updating the data source.
            /// </summary>
            /// <remarks>This property specifies which version of the data row is considered during
            /// update operations. Valid values are defined by the DataRowVersion enumeration, such as Current,
            /// Original, or Proposed. Setting this property allows control over whether updates use the original
            /// values, current values, or proposed values in the data row.</remarks>
            public DataRowVersion SourceVersion { get; set; }

            /// <summary>
            /// Gets or sets the value of the parameter, which is used when executing a command against a database.
            /// The value can be of any type that is compatible with the database column.
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether null values in the source column are mapped during data
            /// operations.
            /// </summary>
            /// <remarks>This property is useful when working with data sources that may contain null
            /// values. Setting this property to <see langword="true"/> ensures that null values in the source column
            /// are handled appropriately during mapping, which can affect how data is transferred or
            /// processed.</remarks>
            public bool SourceColumnNullMapping { get; set; }
        }

        /// <summary>
        /// Represents a collection of parameters that supports access and manipulation by parameter name, facilitating
        /// parameter management in data operations.
        /// </summary>
        /// <remarks>This collection implements the IDataParameterCollection interface, providing methods
        /// to locate, check for existence, and remove parameters using their names. It is intended for scenarios where
        /// parameters must be managed dynamically by name, such as in data binding or command execution
        /// contexts.</remarks>
        private class DummyParameterCollection :
            List<object>, IDataParameterCollection
        {
            /// <summary>
            /// Gets or sets the parameter associated with the specified name.
            /// </summary>
            /// <remarks>This indexer allows for dynamic access to parameters by name. It is important
            /// to ensure that the provided name corresponds to a valid parameter to avoid unexpected results.</remarks>
            /// <param name="parameterName">The name of the parameter to retrieve or set. This must match the name of an existing parameter.</param>
            /// <returns>An object representing the parameter associated with the specified name, or null if no such parameter
            /// exists.</returns>
            public object this[string parameterName]
            {
                get => this.Find(p => ((DummyParameter)p).ParameterName == parameterName);
                set { }
            }

            /// <summary>
            /// Determines whether a parameter with the specified name exists in the collection.
            /// </summary>
            /// <remarks>This method performs a case-sensitive comparison when checking for the
            /// existence of the parameter name.</remarks>
            /// <param name="parameterName">The name of the parameter to search for within the collection.</param>
            /// <returns>true if a parameter with the specified name exists; otherwise, false.</returns>
            public bool Contains(string parameterName)
                => this.Exists(p => ((DummyParameter)p).ParameterName == parameterName);

            /// <summary>
            /// Returns the zero-based index of the first occurrence of a parameter with the specified name.
            /// </summary>
            /// <remarks>This method performs a linear search and may not be optimal for large
            /// collections. Consider using a different approach if performance is a concern.</remarks>
            /// <param name="parameterName">The name of the parameter to locate in the collection. This value cannot be null or empty.</param>
            /// <returns>The zero-based index of the first occurrence of the specified parameter name, or -1 if the parameter is
            /// not found.</returns>
            public int IndexOf(string parameterName)
                => this.FindIndex(p => ((DummyParameter)p).ParameterName == parameterName);

            /// <summary>
            /// Removes all parameters with the specified name from the collection.
            /// </summary>
            /// <param name="parameterName">The name of the parameter to remove from the collection. This value cannot be null or empty.</param>
            public void RemoveAt(string parameterName) =>
                this.RemoveAll(p => ((DummyParameter)p).ParameterName == parameterName);
        }

        /// <summary>
        /// Encapsulates the details of a test procedure, including its identifier, name, and an optional associated
        /// object.
        /// </summary>
        /// <remarks>The NullObj property can be used to store additional data related to the test
        /// procedure. This class is intended for scenarios where test procedures need to be represented with flexible
        /// metadata.</remarks>
        private class TestProcedure
        {
            /// <summary>
            /// Gets or sets the unique identifier for the test procedure, which can be used to distinguish it from other
            /// procedures.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name of the test procedure, providing a human-readable identifier for the procedure.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets an optional object associated with the test procedure, which can be used to store additional
            /// data related to the procedure.
            /// </summary>
            public object NullObj { get; set; }
        }

        /// <summary>
        /// Verifies that the ApplyCommand method sets the command type and command text properties on the provided
        /// command object as expected.
        /// </summary>
        /// <remarks>This unit test ensures that ProcedureBinder correctly configures a command object by
        /// assigning the specified command type and command text. It validates that the binder's ApplyCommand method
        /// behaves as intended when used with a stored procedure and a given command text.</remarks>
        [Fact]
        public void ApplyCommand_SetsCommandTypeAndText()
        {
            var binder = new ProcedureBinder(CommandType.StoredProcedure, "sp_test", new ProcedureParameterBinding[0]);
            var command = new DummyCommand();
            binder.ApplyCommand(command);
            Assert.Equal(CommandType.StoredProcedure, command.CommandType);
            Assert.Equal("sp_test", command.CommandText);
        }

        /// <summary>
        /// Verifies that the BindParameters method adds all specified parameters to the command with the correct
        /// prefix, direction, and data type.
        /// </summary>
        /// <remarks>This test ensures that both input and output parameters are bound correctly when
        /// using a stored procedure, and that the parameter names are prefixed as expected. It validates that the
        /// values, directions, and types of the parameters match the bindings provided.</remarks>
        [Fact]
        public void BindParameters_AddsAllParametersWithPrefix()
        {
            var bindings = new[] {
                new ProcedureParameterBinding("Id", ParameterDirection.Input, (int)DbType.Int32, o => ((TestProcedure)o).Id),
                new ProcedureParameterBinding("Name", ParameterDirection.Output, (int)DbType.String, o => ((TestProcedure)o).Name)
            };
            var binder = new ProcedureBinder(CommandType.StoredProcedure, "sp_test", bindings);
            var command = new DummyCommand();
            var proc = new TestProcedure { Id = 42, Name = "abc" };
            var added = new List<(string, object, ParameterDirection, int)>();
            binder.BindParameters(command, proc, "@", (cmd, name, value, dir, dbType) =>
                added.Add((name, value, dir, dbType)));
            Assert.Contains(added, x => x.Item1 == "@Id" && (int)x.Item2 == 42 && x.Item3 == ParameterDirection.Input && x.Item4 == (int)DbType.Int32);
            Assert.Contains(added, x => x.Item1 == "@Name" && (string)x.Item2 == "abc" && x.Item3 == ParameterDirection.Output && x.Item4 == (int)DbType.String);
        }

        /// <summary>
        /// Verifies that a null value is correctly bound as DBNull when binding parameters to a database command using
        /// ProcedureBinder.
        /// </summary>
        /// <remarks>This test ensures that when a procedure parameter with a null value is provided, the
        /// ProcedureBinder converts it to DBNull. This behavior is necessary for compatibility with database operations
        /// that require DBNull to represent null values in parameters.</remarks>
        [Fact]
        public void BindParameters_NullValue_UsesDBNull()
        {
            var bindings = new[] {
                new ProcedureParameterBinding("NullObj", ParameterDirection.Input, (int)DbType.Object, o => ((TestProcedure)o).NullObj)
            };
            var binder = new ProcedureBinder(CommandType.StoredProcedure, "sp_test", bindings);
            var command = new DummyCommand();
            var proc = new TestProcedure { NullObj = null };
            var added = new List<object>();
            binder.BindParameters(command, proc, "@", (cmd, name, value, dir, dbType) => added.Add(value));
            Assert.Contains(added, v => v == DBNull.Value);
        }

        /// <summary>
        /// Verifies that binding parameters for a stored procedure command with an empty parameter list results in no
        /// actions being performed.
        /// </summary>
        /// <remarks>This test ensures that the parameter binding process correctly handles cases where no
        /// parameters are specified, confirming that no values are added and no errors occur. It is useful for
        /// validating the robustness of the binding logic when faced with empty input.</remarks>
        [Fact]
        public void BindParameters_EmptyParameters_DoesNothing()
        {
            var binder = new ProcedureBinder(CommandType.StoredProcedure, "sp_test", new ProcedureParameterBinding[0]);
            var command = new DummyCommand();
            var proc = new TestProcedure();
            var added = new List<object>();
            binder.BindParameters(command, proc, "@", (cmd, name, value, dir, dbType) => added.Add(value));
            Assert.Empty(added);
        }
    }
}
