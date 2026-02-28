using Hydrix.Attributes.Parameters;
using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Caching;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Caching
{
    /// <summary>
    /// Contains unit tests for the ProcedureBinderCache class, verifying correct behavior when binding procedures and
    /// handling parameters.
    /// </summary>
    /// <remarks>These tests ensure that ProcedureBinderCache correctly adds and retrieves binders for valid
    /// procedures, throws appropriate exceptions when procedure attributes are missing or invalid, and ignores
    /// non-parameter properties during parameter binding. The class helps maintain reliability and correctness in
    /// procedure binding logic.</remarks>
    public class ProcedureBinderCacheTests
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
                get => null;
                set { }
            }

            /// <summary>
            /// Determines whether a parameter with the specified name exists in the collection.
            /// </summary>
            /// <remarks>This method performs a case-sensitive comparison when checking for the
            /// existence of the parameter name.</remarks>
            /// <param name="parameterName">The name of the parameter to search for within the collection.</param>
            /// <returns>true if a parameter with the specified name exists; otherwise, false.</returns>
            public bool Contains(string parameterName) => false;

            /// <summary>
            /// Returns the zero-based index of the first occurrence of a parameter with the specified name.
            /// </summary>
            /// <remarks>This method performs a linear search and may not be optimal for large
            /// collections. Consider using a different approach if performance is a concern.</remarks>
            /// <param name="parameterName">The name of the parameter to locate in the collection. This value cannot be null or empty.</param>
            /// <returns>The zero-based index of the first occurrence of the specified parameter name, or -1 if the parameter is
            /// not found.</returns>
            public int IndexOf(string parameterName) => -1;

            /// <summary>
            /// Removes all parameters with the specified name from the collection.
            /// </summary>
            /// <param name="parameterName">The name of the parameter to remove from the collection. This value cannot be null or empty.</param>
            public void RemoveAt(string parameterName)
            { }
        }

        /// <summary>
        /// Represents a stored procedure for executing database operations with specified parameters.
        /// </summary>
        /// <remarks>This class defines parameters for the stored procedure 'sp_test', including an input
        /// parameter 'Id' and an output parameter 'Name'. The class also includes a write-only property and an indexer,
        /// which are not directly related to the stored procedure parameters.</remarks>
        [Procedure("sp_test")]
        private class ValidProcedure
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            /// <remarks>This property is used to uniquely identify an instance of the entity in the
            /// database. It is required for operations that involve data retrieval or manipulation.</remarks>
            [Parameter("Id", DbType.Int32, Direction = ParameterDirection.Input)]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with the output parameter.
            /// </summary>
            /// <remarks>This property is used to retrieve the name value from the database after
            /// executing a command. Ensure that the command is executed before accessing this property to obtain the
            /// correct value.</remarks>
            [Parameter("Name", DbType.String, Direction = ParameterDirection.Output)]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets a value that is not a parameter for the stored procedure. This property should be ignored
            /// during parameter binding.
            /// </summary>
            public string NotAParameter { get; set; }

            /// <summary>
            /// Gets the value associated with the specified index.
            /// </summary>
            /// <param name="i">The zero-based index of the value to retrieve. Must be a non-negative integer.</param>
            /// <returns>The integer value associated with the specified index, which is always 42 in this implementation.</returns>
            public int this[int i] => 42;

            /// <summary>
            /// Sets the value for internal use. This property cannot be read externally.
            /// </summary>
            /// <remarks>Use this property to assign a value without exposing it for retrieval. This
            /// is useful for scenarios where the value should be set by the user but not accessed directly, helping to
            /// encapsulate internal state and prevent unintended reads.</remarks>
            public int WriteOnly { private get; set; }
        }

        /// <summary>
        /// Represents a class that lacks the ProcedureAttribute, which is required for types to be processed by the
        /// ProcedureBinderCache.
        /// </summary>
        private class NoProcedureAttribute
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            /// <remarks>This property is used to uniquely identify an instance of the entity in the
            /// database. It is expected to be a positive integer.</remarks>
            [Parameter("Id", DbType.Int32, Direction = ParameterDirection.Input)]
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents a database procedure that does not have a specified name, enabling dynamic execution of commands
        /// without a fixed identifier.
        /// </summary>
        /// <remarks>This class is intended for scenarios where a procedure's name is not defined. It
        /// allows for flexible usage in database operations where the procedure identifier may be omitted or determined
        /// at runtime.</remarks>
        [Procedure(null)]
        private class ProcedureWithNullName
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            /// <remarks>This property is used to uniquely identify an instance of the entity in the
            /// database. It is required for operations that involve data retrieval or manipulation.</remarks>
            [Parameter("Id", DbType.Int32, Direction = ParameterDirection.Input)]
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents a stored procedure that accepts an input parameter and provides a write-only property.
        /// </summary>
        /// <remarks>The WriteOnly property is designed to accept values but does not provide a way to
        /// retrieve them. This is useful for scenarios where only input is required without exposing the internal
        /// state.</remarks>
        [Procedure("sp_test")]
        private class ProcedureWithWriteOnlyProperty
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            /// <remarks>This property is used to uniquely identify an instance of the entity in the
            /// database. It is expected to be a positive integer.</remarks>
            [Parameter("Id", DbType.Int32, Direction = ParameterDirection.Input)]
            public int Id { get; set; }

            /// <summary>
            /// Sets the value for internal use. This property cannot be read externally.
            /// </summary>
            /// <remarks>Use this property to assign a value without exposing it for retrieval. This
            /// is useful for scenarios where the value should be set by the user but not accessed directly, helping to
            /// encapsulate internal state and prevent unintended reads.</remarks>
            public int WriteOnly
            { set { } }
        }

        /// <summary>
        /// Encapsulates the parameters and behavior of the 'sp_test' stored procedure, providing an indexer for
        /// retrieving constant values.
        /// </summary>
        /// <remarks>This class represents a stored procedure with an input parameter and exposes an
        /// indexer that returns a fixed value for any given index. Use this class to interact with the 'sp_test'
        /// procedure and access its values via the indexer.</remarks>
        [Procedure("sp_test")]
        private class ProcedureWithIndexer
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            /// <remarks>The Id property is typically used to uniquely identify an instance of the
            /// entity in a database or application context.</remarks>
            [Parameter("Id", DbType.Int32, Direction = ParameterDirection.Input)]
            public int Id { get; set; }

            /// <summary>
            /// Gets the value associated with the specified index.
            /// </summary>
            /// <param name="i">The zero-based index of the value to retrieve. Must be a non-negative integer.</param>
            /// <returns>The integer value associated with the specified index, which is always 42 in this implementation.</returns>
            public int this[int i] => 42; // Indexer
        }

        /// <summary>
        /// Represents a stored procedure named 'sp_test' that does not require any parameters for execution.
        /// </summary>
        /// <remarks>This class is decorated with the <see langword="Procedure"/> attribute, indicating
        /// its association with the stored procedure named 'sp_test'. The class contains a property 'Id' which is
        /// marked as a parameter for the procedure, while 'NotAParameter' is a regular property and does not influence
        /// the procedure's execution.</remarks>
        [Procedure("sp_test")]
        private class ProcedureWithNoParameterAttribute
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            /// <remarks>This property is used to uniquely identify an instance of the entity in the
            /// database. It is expected to be a positive integer.</remarks>
            [Parameter("Id", DbType.Int32, Direction = ParameterDirection.Input)]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets a value that is not a parameter for the stored procedure. This property should be ignored
            /// during parameter binding.
            /// </summary>
            public string NotAParameter { get; set; }
        }

        /// <summary>
        /// Verifies that the ProcedureBinderCache.GetOrAdd method returns a non-null binder when provided with a valid
        /// procedure type.
        /// </summary>
        /// <remarks>This test ensures that the cache correctly handles valid procedure types and that a
        /// binder is always returned for use in subsequent operations.</remarks>
        [Fact]
        public void GetOrAdd_ReturnsBinder_ForValidProcedure()
        {
            var binder = ProcedureBinderCache.GetOrAdd(typeof(ValidProcedure));
            Assert.NotNull(binder);
        }

        /// <summary>
        /// Verifies that the binder excludes properties not defined as parameters when binding command parameters.
        /// </summary>
        /// <remarks>This test ensures that only properties marked as parameters are included during the
        /// binding process, preventing non-parameter properties from being added to the command parameters. Use this
        /// test to confirm correct parameter filtering behavior in the binder.</remarks>
        [Fact]
        public void Binder_IgnoresNonParameterProperties()
        {
            var binder = ProcedureBinderCache.GetOrAdd(typeof(ValidProcedure));
            var command = new DummyCommand();
            var proc = new ValidProcedure { Id = 42, Name = "abc", NotAParameter = "ignore" };
            var added = new List<string>();
            binder.BindParameters(command, proc, "@", (cmd, name, value, dir, dbType) => added.Add(name));
            Assert.Contains("@Id", added);
            Assert.Contains("@Name", added);
            Assert.DoesNotContain("@NotAParameter", added);
        }

        /// <summary>
        /// Verifies that the ProcedureBinderCache.GetOrAdd method throws a MissingMemberException when attempting to
        /// retrieve or add a type that does not have the ProcedureAttribute applied.
        /// </summary>
        /// <remarks>This test ensures that types lacking the required ProcedureAttribute are not
        /// processed by ProcedureBinderCache, enforcing attribute decoration as a prerequisite. Handling this exception
        /// is important to prevent runtime errors when using the cache.</remarks>
        [Fact]
        public void GetOrAdd_Throws_WhenNoProcedureAttribute()
        {
            var ex = Assert.Throws<MissingMemberException>(() =>
                ProcedureBinderCache.GetOrAdd(typeof(NoProcedureAttribute)));
            Assert.Contains("The procedure does not have a ProcedureAttribute decorating itself", ex.Message);
        }

        /// <summary>
        /// Verifies that the GetOrAdd method throws a MissingMemberException when invoked with a procedure type whose
        /// name is null.
        /// </summary>
        /// <remarks>This test ensures that ProcedureBinderCache enforces the requirement for a valid
        /// procedure name and correctly signals an error when the name is missing.</remarks>
        [Fact]
        public void GetOrAdd_Throws_WhenProcedureNameIsNull()
        {
            var ex = Assert.Throws<MissingMemberException>(() =>
                ProcedureBinderCache.GetOrAdd(typeof(ProcedureWithNullName)));
            Assert.Contains("The procedure does not have a valid name in its ProcedureAttribute", ex.Message);
        }

        /// <summary>
        /// Verifies that the ProcedureBinderCache.GetOrAdd method returns the same binder instance for repeated calls
        /// with the same procedure type.
        /// </summary>
        /// <remarks>This test ensures that the caching mechanism in ProcedureBinderCache is functioning
        /// correctly by confirming that identical types yield the same binder instance. This behavior is important for
        /// maintaining consistency and optimizing performance when binders are reused.</remarks>
        [Fact]
        public void GetOrAdd_ReturnsSameBinder_ForSameType()
        {
            var binder1 = ProcedureBinderCache.GetOrAdd(typeof(ValidProcedure));
            var binder2 = ProcedureBinderCache.GetOrAdd(typeof(ValidProcedure));
            Assert.Same(binder1, binder2);
        }

        /// <summary>
        /// Verifies that the binder excludes write-only properties when binding parameters.
        /// </summary>
        /// <remarks>This test ensures that only properties with readable access are included in the
        /// parameter binding process. Write-only properties are intentionally omitted to prevent unintended behavior
        /// when constructing command parameters.</remarks>
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
        /// Verifies that the binder excludes indexer properties when binding parameters for a procedure.
        /// </summary>
        /// <remarks>This test ensures that only explicitly defined properties are considered during
        /// parameter binding, and any indexer properties present in the procedure are ignored. Use this test to confirm
        /// that the binding logic adheres to expected behavior regarding property selection.</remarks>
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
        /// Verifies that properties without the ParameterAttribute are not included in the parameter binding process.
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
    }
}