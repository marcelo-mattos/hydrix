using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Materializers;
using Hydrix.Orchestrator.Materializers.Contract;
using Hydrix.Schemas.Contract;
using Microsoft.Data.SqlClient;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Hydrix.UnitTests
{
    /// <summary>
    /// Provides unit tests for the HydrixDataCore class, verifying the correct behavior of its extension
    /// methods under various scenarios.
    /// </summary>
    /// <remarks>This test class covers both synchronous and asynchronous execution paths, ensuring that all
    /// parameter combinations and exception cases are validated. The tests help maintain the reliability and
    /// correctness of the extension methods by checking their responses to valid and invalid inputs.</remarks>
    public class HydrixDataCoreTests
    {
        /// <summary>
        /// Represents a placeholder implementation of the ITable interface for testing or mock scenarios.
        /// </summary>
        [Table("DummyTable")]
        private class DummyEntity :
            ITable
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with the object.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents a dummy stored procedure used for extension method tests.
        /// </summary>
        [Procedure("usp_DummyProcedure")]
        private class DummyProcedure :
            IProcedure<MockDbParameter>
        { }

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
        /// Creates a mock implementation of the IDbConnection interface with the connection state set to open.
        /// </summary>
        /// <remarks>Use this method to provide a mock database connection for unit tests that require an
        /// open connection without accessing a real database.</remarks>
        /// <returns>A Mock&lt;IDbConnection&gt; object that simulates an open database connection.</returns>
        private static Mock<IDbConnection> CreateConnectionMock(
            Mock<IDbCommand> commandMock)
        {
            var connectionMock = new Mock<IDbConnection>();
            connectionMock.Setup(c => c.State).Returns(ConnectionState.Open);
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

            return connectionMock;
        }

        /// <summary>
        /// Creates a sample DataTable containing predefined columns and rows for demonstration or testing purposes.
        /// </summary>
        /// <returns>A DataTable with two columns, "Id" and "Name", and two rows of sample data.</returns>
        private static DataTable CreateSampleTable(IList<DummyEntity> entities)
        {
            var table = new DataTable(nameof(Materializer));

            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            if (entities == null || entities.Count == 0)
                return table;

            foreach (var entity in entities)
                table.Rows.Add(entity.Id, entity.Name);
            return table;
        }

        /// <summary>
        /// Creates a mock implementation of the IDataReader interface for use in unit tests.
        /// </summary>
        /// <remarks>The returned mock IDataReader supports common data reader operations such as Read,
        /// GetValue, GetName, and IsDBNull, based on a predefined in-memory data table. This method is intended for
        /// testing scenarios where a real database connection is not required.</remarks>
        /// <returns>A Mock&lt;IDataReader&gt; instance configured to simulate reading from a sample data table.</returns>
        private static Mock<IDataReader> CreateMockReader(IList<DummyEntity> entities)
        {
            var table = CreateSampleTable(entities);
            var reader = new Mock<IDataReader>();
            int rowIndex = -1;

            reader.Setup(r => r.Read()).Returns(() =>
            {
                rowIndex++;
                return rowIndex < table.Rows.Count;
            });

            reader.Setup(r => r.FieldCount).Returns(table.Columns.Count);
            reader.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns((string columnName) =>
                columnName switch
                {
                    "Id" => 0,
                    "Name" => 1,
                    _ => throw new IndexOutOfRangeException(
                            $"Column '{columnName}' not found.")
                });
            reader.Setup(r => r.GetOrdinal("Name")).Returns(1);
            reader.Setup(r => r.GetName(It.IsAny<int>())).Returns((int i) => table.Columns[i].ColumnName);
            reader.Setup(r => r.GetValue(It.IsAny<int>())).Returns((int i) => table.Rows[rowIndex][i]);
            reader.Setup(r => r.GetFieldType(It.IsAny<int>())).Returns((int i) => table.Columns[i].DataType);
            reader.Setup(r => r[It.IsAny<int>()]).Returns((int i) => table.Rows[rowIndex][i]);
            reader.Setup(r => r[It.IsAny<string>()]).Returns((string name) => table.Rows[rowIndex][name]);
            reader.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns((int i) => table.Rows[rowIndex][i] == DBNull.Value);
            reader.Setup(r => r.GetValues(It.IsAny<object[]>()))
              .Returns((object[] values) =>
              {
                  for (var i = 0; i < table.Columns.Count; i++)
                      values[i] = table.Rows[rowIndex][i];

                  return table.Columns.Count;
              });
            reader.Setup(r => r.Dispose());
            reader.Setup(r => r.GetInt32(It.IsAny<int>())).Returns((int i) => Convert.ToInt32(table.Rows[rowIndex][i]));
            reader.Setup(r => r.GetInt64(It.IsAny<int>())).Returns((int i) => Convert.ToInt64(table.Rows[rowIndex][i]));
            reader.Setup(r => r.GetInt16(It.IsAny<int>())).Returns((int i) => Convert.ToInt16(table.Rows[rowIndex][i]));
            reader.Setup(r => r.GetByte(It.IsAny<int>())).Returns((int i) => Convert.ToByte(table.Rows[rowIndex][i]));
            reader.Setup(r => r.GetBoolean(It.IsAny<int>())).Returns((int i) => Convert.ToBoolean(table.Rows[rowIndex][i]));
            reader.Setup(r => r.GetDecimal(It.IsAny<int>())).Returns((int i) => Convert.ToDecimal(table.Rows[rowIndex][i]));
            reader.Setup(r => r.GetDouble(It.IsAny<int>())).Returns((int i) => Convert.ToDouble(table.Rows[rowIndex][i]));
            reader.Setup(r => r.GetFloat(It.IsAny<int>())).Returns((int i) => Convert.ToSingle(table.Rows[rowIndex][i]));
            reader.Setup(r => r.GetGuid(It.IsAny<int>())).Returns((int i) =>
            {
                var val = table.Rows[rowIndex][i];
                return val is Guid g ? g : Guid.Parse(val.ToString());
            });
            reader.Setup(r => r.GetDateTime(It.IsAny<int>())).Returns((int i) => Convert.ToDateTime(table.Rows[rowIndex][i]));
            reader.Setup(r => r.GetString(It.IsAny<int>())).Returns((int i) => Convert.ToString(table.Rows[rowIndex][i]));

            return reader;
        }

        /// <summary>
        /// Creates a mock implementation of the IMaterializer interface configured to return a specified result for all
        /// ExecuteNonQuery method overloads.
        /// </summary>
        /// <remarks>Use this method in unit tests to simulate database command execution without
        /// requiring a real database connection.</remarks>
        /// <param name="result">The value to be returned by the ExecuteNonQuery methods. Defaults to 1.</param>
        /// <returns>A mock IMaterializer instance that returns the specified result when any ExecuteNonQuery overload is called.</returns>
        private static Mock<IMaterializer> SetupMaterializerForExecuteNonQuery(int result = 1)
        {
            var mat = new Mock<IMaterializer>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(result);
            mat.Setup(m => m.ExecuteNonQuery(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.ExecuteNonQuery(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.ExecuteNonQuery(It.IsAny<string>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.ExecuteNonQuery(It.IsAny<string>(), It.IsAny<IDbTransaction>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.ExecuteNonQuery(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<IEnumerable<IDataParameter>>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.ExecuteNonQuery(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<IEnumerable<IDataParameter>>(), It.IsAny<IDbTransaction>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.ExecuteNonQuery(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.ExecuteNonQuery(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<IDbTransaction>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.DbConnection).Returns(CreateConnectionMock(commandMock).Object);
            return mat;
        }

        /// <summary>
        /// Creates and configures a mock implementation of the IMaterializer interface to return a specified result for
        /// asynchronous non-query execution methods.
        /// </summary>
        /// <remarks>Use this method in unit tests to simulate the behavior of IMaterializer when
        /// executing non-query commands asynchronously. This allows for consistent and controlled testing of code that
        /// depends on database write operations.</remarks>
        /// <param name="result">The number of rows affected to be returned by the mocked ExecuteNonQueryAsync methods. Defaults to 1.</param>
        /// <returns>A mock IMaterializer instance set up to return the specified result for all supported ExecuteNonQueryAsync
        /// method overloads.</returns>
        private static Mock<IMaterializer> SetupMaterializerForExecuteNonQueryAsync(int result = 1)
        {
            var mat = new Mock<IMaterializer>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(result);
            mat.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
            mat.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
            mat.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
            mat.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<IDbTransaction>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
            mat.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<IEnumerable<IDataParameter>>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
            mat.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<IEnumerable<IDataParameter>>(), It.IsAny<IDbTransaction>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
            mat.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
            mat.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<IDbTransaction>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
            mat.Setup(m => m.DbConnection).Returns(CreateConnectionMock(commandMock).Object);
            return mat;
        }

        /// <summary>
        /// Creates and configures a mock implementation of the IMaterializer interface for unit testing scenarios
        /// involving the ExecuteScalar method.
        /// </summary>
        /// <remarks>Use this method to simulate database scalar query results in tests without requiring
        /// a live database connection. This approach enables consistent and isolated testing of components that depend
        /// on IMaterializer.</remarks>
        /// <param name="result">An optional object specifying the value to be returned by all mocked ExecuteScalar method calls. If not
        /// provided, the default value is null.</param>
        /// <returns>A Mock&lt;IMaterializer&gt; instance set up to return the specified result for various ExecuteScalar method
        /// overloads.</returns>
        private static Mock<IMaterializer> SetupMaterializerForExecuteScalar(object result = null)
        {
            var mat = new Mock<IMaterializer>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(result);
            mat.Setup(m => m.ExecuteScalar(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.ExecuteScalar(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.ExecuteScalar(It.IsAny<string>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.ExecuteScalar(It.IsAny<string>(), It.IsAny<IDbTransaction>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.ExecuteScalar(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<IEnumerable<IDataParameter>>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.ExecuteScalar(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<IEnumerable<IDataParameter>>(), It.IsAny<IDbTransaction>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.ExecuteScalar(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.ExecuteScalar(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<IDbTransaction>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.DbConnection).Returns(CreateConnectionMock(commandMock).Object);
            return mat;
        }

        /// <summary>
        /// Creates a mock implementation of the IMaterializer interface that returns a predefined result for queries
        /// involving DummyEntity objects.
        /// </summary>
        /// <remarks>Use this method in unit tests to simulate database query results without requiring a
        /// real database connection.</remarks>
        /// <param name="result">An optional list of DummyEntity objects to be returned by the mock's query methods. If not specified, the
        /// default is null.</param>
        /// <returns>A mock IMaterializer instance configured to return the specified result for various query method overloads.</returns>
        private static Mock<IMaterializer> SetupMaterializerForQuery(IList<DummyEntity> result = null)
        {
            var mat = new Mock<IMaterializer>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(result).Object);
            mat.Setup(m => m.Query<DummyEntity>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.Query<DummyEntity>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.Query<DummyEntity>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.Query<DummyEntity>(It.IsAny<string>(), It.IsAny<IDbTransaction>(), It.IsAny<int>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.Query<DummyEntity>(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<IEnumerable<IDataParameter>>(), It.IsAny<int>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.Query<DummyEntity>(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<IEnumerable<IDataParameter>>(), It.IsAny<IDbTransaction>(), It.IsAny<int>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.Query<DummyEntity>(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.Query<DummyEntity>(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<IDbTransaction>(), It.IsAny<int>(), It.IsAny<int>())).Returns(result);
            mat.Setup(m => m.DbConnection).Returns(CreateConnectionMock(commandMock).Object);
            return mat;
        }

        /// <summary>
        /// Verifies that an ArgumentNullException is thrown when a null database connection is provided to the Execute
        /// method.
        /// </summary>
        /// <remarks>This test ensures that the Execute method correctly handles null connections by
        /// throwing the appropriate exception, which is crucial for maintaining robust error handling in database
        /// operations.</remarks>
        [Fact]
        public void Execute_ThrowsOnNullConnection()
        {
            Assert.Throws<ArgumentNullException>(() =>
                HydrixDataCore.Execute(null, "SELECT 1"));
        }

        /// <summary>
        /// Verifies that the Execute method throws an ArgumentException when provided with a null, empty, or
        /// whitespace-only SQL command string.
        /// </summary>
        /// <remarks>This test ensures that the Execute method enforces input validation for the SQL
        /// command string, helping to prevent invalid database operations.</remarks>
        /// <param name="sql">The SQL command string to be tested. Can be null, empty, or consist solely of whitespace to validate
        /// exception handling.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Execute_ThrowsOnNullOrEmptySql(string sql)
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(1);
            var conn = CreateConnectionMock(commandMock).Object;
            Assert.Throws<ArgumentException>(() =>
                HydrixDataCore.Execute(conn, sql));
        }

        /// <summary>
        /// Verifies that the Execute method correctly invokes the materializer for all combinations of parameters and
        /// transaction states.
        /// </summary>
        /// <remarks>This test ensures that the Execute method in HydrixDataCore handles various
        /// scenarios, including the presence or absence of parameters and transactions, as well as different command
        /// types. Each assertion checks that the expected number of affected rows is returned, confirming consistent
        /// behavior across all supported input combinations.</remarks>
        [Fact]
        public void Execute_CallsMaterializer_ForAllParameterCombinations()
        {
            var mat = SetupMaterializerForExecuteNonQuery().Object;
            var sql = "UPDATE T SET X=1";
            var param = new { Id = 1 };
            var procParam = new SqlParameter("p_Id", 1);
            var tran = new Mock<IDbTransaction>().Object;

            // Text, with params, no tran
            Assert.Equal(1, mat.DbConnection.Execute(sql, param));
            // Text, with params, with tran
            Assert.Equal(1, mat.DbConnection.Execute(sql, param, tran));
            // Text, no params, no tran
            Assert.Equal(1, mat.DbConnection.Execute(sql));
            // Text, no params, with tran
            Assert.Equal(1, mat.DbConnection.Execute(sql, null, tran));
            // Non-Text, with params, no tran
            Assert.Equal(1, mat.DbConnection.Execute(sql, procParam, null, CommandType.StoredProcedure));
            // Non-Text, with params, with tran
            Assert.Equal(1, mat.DbConnection.Execute(sql, procParam, tran, CommandType.StoredProcedure));
            // Non-Text, no params, no tran
            Assert.Equal(1, mat.DbConnection.Execute(sql, null, null, CommandType.StoredProcedure));
            // Non-Text, no params, with tran
            Assert.Equal(1, mat.DbConnection.Execute(sql, null, tran, CommandType.StoredProcedure));
        }

        /// <summary>
        /// Verifies that the ExecuteAsync method correctly invokes the materializer for all combinations of parameters
        /// and transaction states.
        /// </summary>
        /// <remarks>This test ensures that ExecuteAsync handles various scenarios, including the presence
        /// or absence of parameters and transactions, as well as different command types such as text and stored
        /// procedure. It asserts that the method returns the expected number of affected rows for each
        /// combination.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteAsync_CallsMaterializer_ForAllParameterCombinations()
        {
            var mat = SetupMaterializerForExecuteNonQueryAsync().Object;
            var sql = "UPDATE T SET X=1";
            var param = new { Id = 1 };
            var procParam = new SqlParameter("p_Id", 1);
            var tran = new Mock<IDbTransaction>().Object;
            var token = CancellationToken.None;

            // Text, with params, no tran
            Assert.Equal(1, await mat.DbConnection.ExecuteAsync(sql, param));
            // Text, with params, with tran
            Assert.Equal(1, await mat.DbConnection.ExecuteAsync(sql, param, tran));
            // Text, no params, no tran
            Assert.Equal(1, await mat.DbConnection.ExecuteAsync(sql));
            // Text, no params, with tran
            Assert.Equal(1, await mat.DbConnection.ExecuteAsync(sql, null, tran));
            // Non-Text, with params, no tran
            Assert.Equal(1, await mat.DbConnection.ExecuteAsync(sql, procParam, null, CommandType.StoredProcedure));
            // Non-Text, with params, with tran
            Assert.Equal(1, await mat.DbConnection.ExecuteAsync(sql, procParam, tran, CommandType.StoredProcedure));
            // Non-Text, no params, no tran
            Assert.Equal(1, await mat.DbConnection.ExecuteAsync(sql, null, null, CommandType.StoredProcedure));
            // Non-Text, no params, with tran
            Assert.Equal(1, await mat.DbConnection.ExecuteAsync(sql, null, tran, CommandType.StoredProcedure));
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method calls the materializer for all combinations of parameters and
        /// transaction states.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalar method behaves correctly with various
        /// combinations of SQL command types, parameters, and transaction objects, confirming that the expected value
        /// is returned in each case.</remarks>
        [Fact]
        public void ExecuteScalar_CallsMaterializer_ForAllParameterCombinations()
        {
            var mat = SetupMaterializerForExecuteScalar(42).Object;
            var sql = "SELECT COUNT(*) FROM T";
            var param = new { Id = 1 };
            var procParam = new SqlParameter("p_Id", 1);
            var tran = new Mock<IDbTransaction>().Object;

            // Text, with params, no tran
            Assert.Equal(42, mat.DbConnection.ExecuteScalar<int>(sql, param));
            // Text, with params, with tran
            Assert.Equal(42, mat.DbConnection.ExecuteScalar<int>(sql, param, tran));
            // Text, no params, no tran
            Assert.Equal(42, mat.DbConnection.ExecuteScalar<int>(sql));
            // Text, no params, with tran
            Assert.Equal(42, mat.DbConnection.ExecuteScalar<int>(sql, null, tran));
            // Non-Text, with params, no tran
            Assert.Equal(42, mat.DbConnection.ExecuteScalar<int>(sql, procParam, null, CommandType.StoredProcedure));
            // Non-Text, with params, with tran
            Assert.Equal(42, mat.DbConnection.ExecuteScalar<int>(sql, procParam, tran, CommandType.StoredProcedure));
            // Non-Text, no params, no tran
            Assert.Equal(42, mat.DbConnection.ExecuteScalar<int>(sql, null, null, CommandType.StoredProcedure));
            // Non-Text, no params, with tran
            Assert.Equal(42, mat.DbConnection.ExecuteScalar<int>(sql, null, tran, CommandType.StoredProcedure));
        }

        /// <summary>
        /// Verifies that the ExecuteScalarAsync method correctly invokes the materializer for all combinations of
        /// parameters and transaction states.
        /// </summary>
        /// <remarks>This test ensures that ExecuteScalarAsync handles various scenarios, including text
        /// and stored procedure commands, with and without parameters and transactions, and returns the expected result
        /// in each case.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_CallsMaterializer_ForAllParameterCombinations()
        {
            var mat = SetupMaterializerForExecuteScalar(42).Object;
            var sql = "SELECT COUNT(*) FROM T";
            var param = new { Id = 1 };
            var procParam = new SqlParameter("p_Id", 1);
            var tran = new Mock<IDbTransaction>().Object;

            // Text, with params, no tran
            Assert.Equal(42, await mat.DbConnection.ExecuteScalarAsync<int>(sql, param));
            // Text, with params, with tran
            Assert.Equal(42, await mat.DbConnection.ExecuteScalarAsync<int>(sql, param, tran));
            // Text, no params, no tran
            Assert.Equal(42, await mat.DbConnection.ExecuteScalarAsync<int>(sql));
            // Text, no params, with tran
            Assert.Equal(42, await mat.DbConnection.ExecuteScalarAsync<int>(sql, null, tran));
            // Non-Text, with params, no tran
            Assert.Equal(42, await mat.DbConnection.ExecuteScalarAsync<int>(sql, procParam, null, CommandType.StoredProcedure));
            // Non-Text, with params, with tran
            Assert.Equal(42, await mat.DbConnection.ExecuteScalarAsync<int>(sql, procParam, tran, CommandType.StoredProcedure));
            // Non-Text, no params, no tran
            Assert.Equal(42, await mat.DbConnection.ExecuteScalarAsync<int>(sql, null, null, CommandType.StoredProcedure));
            // Non-Text, no params, with tran
            Assert.Equal(42, await mat.DbConnection.ExecuteScalarAsync<int>(sql, null, tran, CommandType.StoredProcedure));
        }

        /// <summary>
        /// Verifies that the Query method correctly calls the materializer for all combinations of parameters and
        /// transaction states.
        /// </summary>
        /// <remarks>This test ensures that the Query method behaves as expected when invoked with various
        /// combinations of SQL command types, parameters, and transaction objects. It checks both text-based and stored
        /// procedure queries, confirming that the results match the expected dummy list.</remarks>
        [Fact]
        public void Query_CallsMaterializer_ForAllParameterCombinations()
        {
            var dummyList = new List<DummyEntity> { new DummyEntity() { Id = 1, Name = "Alice" }, new DummyEntity() { Id = 2, Name = "Bob" } };
            var sql = "SELECT * FROM T";
            var param = new { Id = 1 };
            var procParam = new SqlParameter("p_Id", 1);
            var tran = new Mock<IDbTransaction>().Object;

            // Text, with params, no tran
            var mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyList, mat.DbConnection.Query<DummyEntity>(sql, param));
            // Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyList, mat.DbConnection.Query<DummyEntity>(sql, param, tran));
            // Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyList, mat.DbConnection.Query<DummyEntity>(sql));
            // Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyList, mat.DbConnection.Query<DummyEntity>(sql, null, tran));
            // Non-Text, with params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyList, mat.DbConnection.Query<DummyEntity>(sql, procParam, null, CommandType.StoredProcedure));
            // Non-Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyList, mat.DbConnection.Query<DummyEntity>(sql, procParam, tran, CommandType.StoredProcedure));
            // Non-Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyList, mat.DbConnection.Query<DummyEntity>(sql, null, null, CommandType.StoredProcedure));
            // Non-Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyList, mat.DbConnection.Query<DummyEntity>(sql, null, tran, CommandType.StoredProcedure));
        }

        /// <summary>
        /// Verifies that the asynchronous query method correctly invokes the materializer for all combinations of SQL
        /// command text, parameters, and transaction states.
        /// </summary>
        /// <remarks>This test ensures that the materializer is called as expected when executing queries
        /// with and without parameters, with and without transactions, and for both text and stored procedure command
        /// types. It helps validate that the query extension method handles various input scenarios
        /// consistently.</remarks>
        /// <returns>This method does not return a value.</returns>
        [Fact]
        public async Task QueryAsync_CallsMaterializer_ForAllParameterCombinations()
        {
            var dummyList = new List<DummyEntity> { new DummyEntity() { Id = 1, Name = "Alice" }, new DummyEntity() { Id = 2, Name = "Bob" } };
            var sql = "SELECT * FROM T";
            var param = new { Id = 1 };
            var procParam = new SqlParameter("p_Id", 1);
            var tran = new Mock<IDbTransaction>().Object;

            // Text, with params, no tran
            var mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyList, await mat.DbConnection.QueryAsync<DummyEntity>(sql, param));
            // Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyList, await mat.DbConnection.QueryAsync<DummyEntity>(sql, param, tran));
            // Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyList, await mat.DbConnection.QueryAsync<DummyEntity>(sql));
            // Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyList, await mat.DbConnection.QueryAsync<DummyEntity>(sql, null, tran));
            // Non-Text, with params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyList, await mat.DbConnection.QueryAsync<DummyEntity>(sql, procParam, null, CommandType.StoredProcedure));
            // Non-Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyList, await mat.DbConnection.QueryAsync<DummyEntity>(sql, procParam, tran, CommandType.StoredProcedure));
            // Non-Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyList, await mat.DbConnection.QueryAsync<DummyEntity>(sql, null, null, CommandType.StoredProcedure));
            // Non-Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyList, await mat.DbConnection.QueryAsync<DummyEntity>(sql, null, tran, CommandType.StoredProcedure));
        }

        /// <summary>
        /// Verifies that the QueryFirst method returns the first element of the query result or throws an
        /// InvalidOperationException if the result is empty.
        /// </summary>
        /// <remarks>This test ensures that QueryFirst retrieves the first item from the result set when
        /// available, and throws an exception when the result set contains no elements. It covers both the successful
        /// retrieval and error scenarios for the method.</remarks>
        [Fact]
        public void QueryFirst_ReturnsFirstElement_OrThrows()
        {
            var dummyList = new List<DummyEntity> { new DummyEntity() { Id = 1, Name = "Alice" } };
            var dummyEntity = dummyList[0];
            var sql = "SELECT * FROM T";
            var param = new { Id = 1 };
            var procParam = new SqlParameter("p_Id", 1);
            var tran = new Mock<IDbTransaction>().Object;

            // Text, with params, no tran
            var mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QueryFirst<DummyEntity>(sql, param));
            // Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QueryFirst<DummyEntity>(sql, param, tran));
            // Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QueryFirst<DummyEntity>(sql));
            // Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QueryFirst<DummyEntity>(sql, null, tran));
            // Non-Text, with params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QueryFirst<DummyEntity>(sql, procParam, null, CommandType.StoredProcedure));
            // Non-Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QueryFirst<DummyEntity>(sql, procParam, tran, CommandType.StoredProcedure));
            // Non-Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QueryFirst<DummyEntity>(sql, null, null, CommandType.StoredProcedure));
            // Non-Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QueryFirst<DummyEntity>(sql, null, tran, CommandType.StoredProcedure));
            // Throws if empty
            var emptyList = new List<DummyEntity>();
            var matEmpty = SetupMaterializerForQuery(emptyList).Object;
            Assert.Throws<InvalidOperationException>(() => matEmpty.DbConnection.QueryFirst<DummyEntity>(sql));
        }

        /// <summary>
        /// Verifies that the QueryFirstAsync method returns the first element of the query result or throws an
        /// exception if no elements are found.
        /// </summary>
        /// <remarks>This test covers multiple scenarios for the QueryFirstAsync method, including text
        /// and stored procedure queries, with and without parameters and transactions. It also verifies that an
        /// InvalidOperationException is thrown when the query result is empty.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryFirstAsync_ReturnsFirstElement_OrThrows()
        {
            var dummyList = new List<DummyEntity> { new DummyEntity() { Id = 1, Name = "Alice" } };
            var dummyEntity = dummyList[0];
            var sql = "SELECT * FROM T";
            var param = new { Id = 1 };
            var procParam = new SqlParameter("p_Id", 1);
            var tran = new Mock<IDbTransaction>().Object;

            // Text, with params, no tran
            var mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QueryFirstAsync<DummyEntity>(sql, param));
            // Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QueryFirstAsync<DummyEntity>(sql, param, tran));
            // Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QueryFirstAsync<DummyEntity>(sql));
            // Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QueryFirstAsync<DummyEntity>(sql, null, tran));
            // Non-Text, with params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QueryFirstAsync<DummyEntity>(sql, procParam, null, CommandType.StoredProcedure));
            // Non-Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QueryFirstAsync<DummyEntity>(sql, procParam, tran, CommandType.StoredProcedure));
            // Non-Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QueryFirstAsync<DummyEntity>(sql, null, null, CommandType.StoredProcedure));
            // Non-Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QueryFirstAsync<DummyEntity>(sql, null, tran, CommandType.StoredProcedure));
            // Throws if empty
            var emptyList = new List<DummyEntity>();
            var matEmpty = SetupMaterializerForQuery(emptyList).Object;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await matEmpty.DbConnection.QueryFirstAsync<DummyEntity>(sql));
        }

        /// <summary>
        /// Verifies that the QueryFirstOrDefault method returns the first result of the specified type when available,
        /// or the default value if no results are found.
        /// </summary>
        /// <remarks>This test ensures that QueryFirstOrDefault returns the expected entity when the query
        /// yields results and returns null when the result set is empty. It covers both the case where a matching
        /// entity exists and where no entities are returned by the query.</remarks>
        [Fact]
        public void QueryFirstOrDefault_ReturnsFirstOrDefault()
        {
            var dummyList = new List<DummyEntity> { new DummyEntity() { Id = 1, Name = "Alice" } };
            var dummyEntity = dummyList[0];
            var sql = "SELECT * FROM T";
            var param = new { Id = 1 };
            var procParam = new SqlParameter("p_Id", 1);
            var tran = new Mock<IDbTransaction>().Object;

            // Text, with params, no tran
            var mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QueryFirstOrDefault<DummyEntity>(sql, param));
            // Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QueryFirstOrDefault<DummyEntity>(sql, param, tran));
            // Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QueryFirstOrDefault<DummyEntity>(sql));
            // Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QueryFirstOrDefault<DummyEntity>(sql, null, tran));
            // Non-Text, with params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QueryFirstOrDefault<DummyEntity>(sql, procParam, null, CommandType.StoredProcedure));
            // Non-Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QueryFirstOrDefault<DummyEntity>(sql, procParam, tran, CommandType.StoredProcedure));
            // Non-Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QueryFirstOrDefault<DummyEntity>(sql, null, null, CommandType.StoredProcedure));
            // Non-Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QueryFirstOrDefault<DummyEntity>(sql, null, tran, CommandType.StoredProcedure));

            // Returns default if empty
            var emptyList = new List<DummyEntity>();
            var matEmpty = SetupMaterializerForQuery(emptyList).Object;
            Assert.Null(matEmpty.DbConnection.QueryFirstOrDefault<DummyEntity>(sql));
        }

        /// <summary>
        /// Verifies that the QueryFirstOrDefaultAsync method returns the first matching DummyEntity or the default
        /// value when no results are found.
        /// </summary>
        /// <remarks>This test covers multiple scenarios, including executing SQL queries and stored
        /// procedures with and without parameters and transactions. It also verifies that the method returns null when
        /// the result set is empty.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryFirstOrDefaultAsync_ReturnsFirstOrDefault()
        {
            var dummyList = new List<DummyEntity> { new DummyEntity() { Id = 1, Name = "Alice" } };
            var dummyEntity = dummyList[0];
            var sql = "SELECT * FROM T";
            var param = new { Id = 1 };
            var procParam = new SqlParameter("p_Id", 1);
            var tran = new Mock<IDbTransaction>().Object;

            // Text, with params, no tran
            var mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QueryFirstOrDefaultAsync<DummyEntity>(sql, param));
            // Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QueryFirstOrDefaultAsync<DummyEntity>(sql, param, tran));
            // Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QueryFirstOrDefaultAsync<DummyEntity>(sql));
            // Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QueryFirstOrDefaultAsync<DummyEntity>(sql, null, tran));
            // Non-Text, with params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QueryFirstOrDefaultAsync<DummyEntity>(sql, procParam, null, CommandType.StoredProcedure));
            // Non-Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QueryFirstOrDefaultAsync<DummyEntity>(sql, procParam, tran, CommandType.StoredProcedure));
            // Non-Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QueryFirstOrDefaultAsync<DummyEntity>(sql, null, null, CommandType.StoredProcedure));
            // Non-Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QueryFirstOrDefaultAsync<DummyEntity>(sql, null, tran, CommandType.StoredProcedure));

            // Returns default if empty
            var emptyList = new List<DummyEntity>();
            var matEmpty = SetupMaterializerForQuery(emptyList).Object;
            Assert.Null(await matEmpty.DbConnection.QueryFirstOrDefaultAsync<DummyEntity>(sql));
        }

        /// <summary>
        /// Verifies that the QuerySingle method returns the single entity from the result set or throws an
        /// InvalidOperationException if the result set is empty or contains more than one entity.
        /// </summary>
        /// <remarks>This test ensures that QuerySingle behaves as expected by returning the only entity
        /// present in the result set, and by throwing an exception when the result set is empty or contains multiple
        /// entities. It covers the typical usage scenarios and error conditions for the method.</remarks>
        [Fact]
        public void QuerySingle_ReturnsSingle_OrThrows()
        {
            var dummyList = new List<DummyEntity> { new DummyEntity() { Id = 1, Name = "Alice" } };
            var dummyEntity = dummyList[0];
            var sql = "SELECT * FROM T";
            var param = new { Id = 1 };
            var procParam = new SqlParameter("p_Id", 1);
            var tran = new Mock<IDbTransaction>().Object;

            // Text, with params, no tran
            var mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QuerySingle<DummyEntity>(sql, param));
            // Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QuerySingle<DummyEntity>(sql, param, tran));
            // Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QuerySingle<DummyEntity>(sql));
            // Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QuerySingle<DummyEntity>(sql, null, tran));
            // Non-Text, with params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QuerySingle<DummyEntity>(sql, procParam, null, CommandType.StoredProcedure));
            // Non-Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QuerySingle<DummyEntity>(sql, procParam, tran, CommandType.StoredProcedure));
            // Non-Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QuerySingle<DummyEntity>(sql, null, null, CommandType.StoredProcedure));
            // Non-Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QuerySingle<DummyEntity>(sql, null, tran, CommandType.StoredProcedure));

            // Throws if empty
            var emptyList = new List<DummyEntity>();
            var matEmpty = SetupMaterializerForQuery(emptyList).Object;
            Assert.Throws<InvalidOperationException>(() => matEmpty.DbConnection.QuerySingle<DummyEntity>(sql));

            // Throws if more than one
            var twoList = new List<DummyEntity> { new DummyEntity() { Id = 1, Name = "Alice" }, new DummyEntity() { Id = 2, Name = "Bob" } };
            var matTwo = SetupMaterializerForQuery(twoList).Object;
            Assert.Throws<InvalidOperationException>(() => matTwo.DbConnection.QuerySingle<DummyEntity>(sql));
        }

        /// <summary>
        /// Verifies that the QuerySingleAsync method returns a single entity when exactly one result is present, or
        /// throws an InvalidOperationException when the result set is empty or contains more than one entity.
        /// </summary>
        /// <remarks>This test covers various scenarios for the QuerySingleAsync method, including queries
        /// with and without parameters, with and without transactions, and using both text and stored procedure command
        /// types. It ensures that the method behaves correctly by returning the expected entity when a single result is
        /// present and throwing an exception in cases where the result set is empty or contains multiple
        /// entities.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QuerySingleAsync_ReturnsSingle_OrThrows()
        {
            var dummyList = new List<DummyEntity> { new DummyEntity() { Id = 1, Name = "Alice" } };
            var dummyEntity = dummyList[0];
            var sql = "SELECT * FROM T";
            var param = new { Id = 1 };
            var procParam = new SqlParameter("p_Id", 1);
            var tran = new Mock<IDbTransaction>().Object;

            // Text, with params, no tran
            var mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QuerySingleAsync<DummyEntity>(sql, param));
            // Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QuerySingleAsync<DummyEntity>(sql, param, tran));
            // Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QuerySingleAsync<DummyEntity>(sql));
            // Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QuerySingleAsync<DummyEntity>(sql, null, tran));
            // Non-Text, with params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QuerySingleAsync<DummyEntity>(sql, procParam, null, CommandType.StoredProcedure));
            // Non-Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QuerySingleAsync<DummyEntity>(sql, procParam, tran, CommandType.StoredProcedure));
            // Non-Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QuerySingleAsync<DummyEntity>(sql, null, null, CommandType.StoredProcedure));
            // Non-Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QuerySingleAsync<DummyEntity>(sql, null, tran, CommandType.StoredProcedure));

            // Throws if empty
            var emptyList = new List<DummyEntity>();
            var matEmpty = SetupMaterializerForQuery(emptyList).Object;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await matEmpty.DbConnection.QuerySingleAsync<DummyEntity>(sql));

            // Throws if more than one
            var twoList = new List<DummyEntity> { new DummyEntity() { Id = 1, Name = "Alice" }, new DummyEntity() { Id = 2, Name = "Bob" } };
            var matTwo = SetupMaterializerForQuery(twoList).Object;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await matTwo.DbConnection.QuerySingleAsync<DummyEntity>(sql));
        }

        /// <summary>
        /// Verifies that the QuerySingleOrDefault method returns a single entity of type DummyEntity if found, returns
        /// the default value if no entities are found, and throws an InvalidOperationException if more than one entity
        /// is found.
        /// </summary>
        /// <remarks>This test ensures that QuerySingleOrDefault behaves as expected in scenarios where
        /// the result set contains zero, one, or multiple entities. It confirms correct handling of single-result
        /// queries and proper exception throwing for data integrity issues.</remarks>
        [Fact]
        public void QuerySingleOrDefault_ReturnsSingleOrDefault_OrThrows()
        {
            var dummyList = new List<DummyEntity> { new DummyEntity() { Id = 1, Name = "Alice" } };
            var dummyEntity = dummyList[0];
            var sql = "SELECT * FROM T";
            var param = new { Id = 1 };
            var procParam = new SqlParameter("p_Id", 1);
            var tran = new Mock<IDbTransaction>().Object;

            // Text, with params, no tran
            var mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QuerySingleOrDefault<DummyEntity>(sql, param));
            // Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QuerySingleOrDefault<DummyEntity>(sql, param, tran));
            // Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QuerySingleOrDefault<DummyEntity>(sql));
            // Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QuerySingleOrDefault<DummyEntity>(sql, null, tran));
            // Non-Text, with params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QuerySingleOrDefault<DummyEntity>(sql, procParam, null, CommandType.StoredProcedure));
            // Non-Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QuerySingleOrDefault<DummyEntity>(sql, procParam, tran, CommandType.StoredProcedure));
            // Non-Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QuerySingleOrDefault<DummyEntity>(sql, null, null, CommandType.StoredProcedure));
            // Non-Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, mat.DbConnection.QuerySingleOrDefault<DummyEntity>(sql, null, tran, CommandType.StoredProcedure));

            // Returns default if empty
            var emptyList = new List<DummyEntity>();
            var matEmpty = SetupMaterializerForQuery(emptyList).Object;
            Assert.Null(matEmpty.DbConnection.QuerySingleOrDefault<DummyEntity>(sql));

            // Throws if more than one
            var twoList = new List<DummyEntity> { new DummyEntity() { Id = 1, Name = "Alice" }, new DummyEntity() { Id = 2, Name = "Bob" } };
            var matTwo = SetupMaterializerForQuery(twoList).Object;
            Assert.Throws<InvalidOperationException>(() => matTwo.DbConnection.QuerySingleOrDefault<DummyEntity>(sql));
        }

        /// <summary>
        /// Verifies that the QuerySingleOrDefaultAsync method returns a single entity of type DummyEntity when one is
        /// found, returns the default value when no entity is found, and throws an InvalidOperationException when more
        /// than one entity is found.
        /// </summary>
        /// <remarks>This test covers various scenarios for the QuerySingleOrDefaultAsync method,
        /// including queries with and without parameters, with and without transactions, and using both text and stored
        /// procedure command types. It ensures that the method behaves correctly when the result set contains zero,
        /// one, or multiple entities.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QuerySingleOrDefaultAsync_ReturnsSingleOrDefault_OrThrows()
        {
            var dummyList = new List<DummyEntity> { new DummyEntity() { Id = 1, Name = "Alice" } };
            var dummyEntity = dummyList[0];
            var sql = "SELECT * FROM T";
            var param = new { Id = 1 };
            var procParam = new SqlParameter("p_Id", 1);
            var tran = new Mock<IDbTransaction>().Object;

            // Text, with params, no tran
            var mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QuerySingleOrDefaultAsync<DummyEntity>(sql, param));
            // Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QuerySingleOrDefaultAsync<DummyEntity>(sql, param, tran));
            // Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QuerySingleOrDefaultAsync<DummyEntity>(sql));
            // Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QuerySingleOrDefaultAsync<DummyEntity>(sql, null, tran));
            // Non-Text, with params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QuerySingleOrDefaultAsync<DummyEntity>(sql, procParam, null, CommandType.StoredProcedure));
            // Non-Text, with params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QuerySingleOrDefaultAsync<DummyEntity>(sql, procParam, tran, CommandType.StoredProcedure));
            // Non-Text, no params, no tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QuerySingleOrDefaultAsync<DummyEntity>(sql, null, null, CommandType.StoredProcedure));
            // Non-Text, no params, with tran
            mat = SetupMaterializerForQuery(dummyList).Object;
            Assert.Equivalent(dummyEntity, await mat.DbConnection.QuerySingleOrDefaultAsync<DummyEntity>(sql, null, tran, CommandType.StoredProcedure));

            // Returns default if empty
            var emptyList = new List<DummyEntity>();
            var matEmpty = SetupMaterializerForQuery(emptyList).Object;
            Assert.Null(await matEmpty.DbConnection.QuerySingleOrDefaultAsync<DummyEntity>(sql));

            // Throws if more than one
            var twoList = new List<DummyEntity> { new DummyEntity() { Id = 1, Name = "Alice" }, new DummyEntity() { Id = 2, Name = "Bob" } };
            var matTwo = SetupMaterializerForQuery(twoList).Object;
            Assert.Throws<InvalidOperationException>(() => matTwo.DbConnection.QuerySingleOrDefaultAsync<DummyEntity>(sql).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Verifies that Execute with procedure overload throws an ArgumentNullException when the connection is null.
        /// </summary>
        [Fact]
        public void Execute_WithProcedure_ThrowsOnNullConnection()
        {
            Assert.Throws<ArgumentNullException>(() =>
                HydrixDataCore.Execute<MockDbParameter>(
                    null,
                    new DummyProcedure()));
        }

        /// <summary>
        /// Verifies that Execute with procedure overload throws an ArgumentNullException when the procedure is null.
        /// </summary>
        [Fact]
        public void Execute_WithProcedure_ThrowsOnNullProcedure()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(1);
            var connection = CreateConnectionMock(commandMock).Object;

            Assert.Throws<ArgumentNullException>(() =>
                connection.Execute<MockDbParameter>(null));
        }

        /// <summary>
        /// Verifies that Execute with procedure overload returns the affected rows when executing a valid procedure.
        /// </summary>
        [Fact]
        public void Execute_WithProcedure_ReturnsAffectedRows()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(7);
            var connection = CreateConnectionMock(commandMock).Object;

            var result = connection.Execute(
                new DummyProcedure(),
                null,
                30);

            Assert.Equal(7, result);
        }

        /// <summary>
        /// Verifies that ExecuteAsync with procedure overload returns the affected rows when executing a valid procedure.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteAsync_WithProcedure_ReturnsAffectedRows()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(9);
            var connection = CreateConnectionMock(commandMock).Object;
            var transaction = new Mock<IDbTransaction>().Object;

            var result = await connection.ExecuteAsync(
                new DummyProcedure(),
                transaction,
                30,
                CancellationToken.None);

            Assert.Equal(9, result);
        }

        /// <summary>
        /// Verifies that ExecuteScalar with procedure overload returns the scalar result when executing a valid procedure.
        /// </summary>
        [Fact]
        public void ExecuteScalar_WithProcedure_ReturnsScalarValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(42);
            var connection = CreateConnectionMock(commandMock).Object;

            var result = connection.ExecuteScalar<MockDbParameter, int>(
                new DummyProcedure(),
                null,
                30);

            Assert.Equal(42, result);
        }

        /// <summary>
        /// Verifies that ExecuteScalarAsync with procedure overload returns the scalar result when executing a valid procedure.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithProcedure_ReturnsScalarValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(84);
            var connection = CreateConnectionMock(commandMock).Object;
            var transaction = new Mock<IDbTransaction>().Object;

            var result = await connection.ExecuteScalarAsync<MockDbParameter, int>(
                new DummyProcedure(),
                transaction,
                30,
                CancellationToken.None);

            Assert.Equal(84, result);
        }

        /// <summary>
        /// Verifies that Query with procedure overload returns mapped entities.
        /// </summary>
        [Fact]
        public void Query_WithProcedure_ReturnsEntities()
        {
            var entities = new List<DummyEntity>
            {
                new DummyEntity { Id = 1, Name = "Alice" },
                new DummyEntity { Id = 2, Name = "Bob" }
            };
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(entities).Object);
            var connection = CreateConnectionMock(commandMock).Object;

            var result = connection.Query<DummyEntity, MockDbParameter>(new DummyProcedure());

            Assert.Equal(2, result.Count);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that QueryAsync with procedure overload returns mapped entities.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithProcedure_ReturnsEntities()
        {
            var entities = new List<DummyEntity>
            {
                new DummyEntity { Id = 1, Name = "Alice" },
                new DummyEntity { Id = 2, Name = "Bob" }
            };
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(entities).Object);
            var connection = CreateConnectionMock(commandMock).Object;

            var result = await connection.QueryAsync<DummyEntity, MockDbParameter>(new DummyProcedure());

            Assert.Equal(2, result.Count);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies procedure overloads for QueryFirst and QueryFirstAsync.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryFirst_WithProcedure_ReturnsFirst_ForSyncAndAsync()
        {
            var entities = new List<DummyEntity>
            {
                new DummyEntity { Id = 1, Name = "Alice" },
                new DummyEntity { Id = 2, Name = "Bob" }
            };

            var commandSync = new Mock<IDbCommand>();
            commandSync.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(entities).Object);
            var connectionSync = CreateConnectionMock(commandSync).Object;
            var firstSync = connectionSync.QueryFirst<DummyEntity, MockDbParameter>(new DummyProcedure());

            var commandAsync = new Mock<IDbCommand>();
            commandAsync.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(entities).Object);
            var connectionAsync = CreateConnectionMock(commandAsync).Object;
            var firstAsync = await connectionAsync.QueryFirstAsync<DummyEntity, MockDbParameter>(new DummyProcedure());

            Assert.Equal("Alice", firstSync.Name);
            Assert.Equal("Alice", firstAsync.Name);
        }

        /// <summary>
        /// Verifies procedure overloads for QueryFirstOrDefault and QueryFirstOrDefaultAsync.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryFirstOrDefault_WithProcedure_ReturnsFirstOrDefault_ForSyncAndAsync()
        {
            var entities = new List<DummyEntity> { new DummyEntity { Id = 1, Name = "Alice" } };

            var commandSync = new Mock<IDbCommand>();
            commandSync.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(entities).Object);
            var connectionSync = CreateConnectionMock(commandSync).Object;
            var firstSync = connectionSync.QueryFirstOrDefault<DummyEntity, MockDbParameter>(new DummyProcedure());

            var commandAsync = new Mock<IDbCommand>();
            commandAsync.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(new List<DummyEntity>()).Object);
            var connectionAsync = CreateConnectionMock(commandAsync).Object;
            var firstAsync = await connectionAsync.QueryFirstOrDefaultAsync<DummyEntity, MockDbParameter>(new DummyProcedure());

            Assert.Equal("Alice", firstSync.Name);
            Assert.Null(firstAsync);
        }

        /// <summary>
        /// Verifies procedure overloads for QuerySingle and QuerySingleAsync.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QuerySingle_WithProcedure_ReturnsSingle_ForSyncAndAsync()
        {
            var entities = new List<DummyEntity> { new DummyEntity { Id = 1, Name = "Alice" } };

            var commandSync = new Mock<IDbCommand>();
            commandSync.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(entities).Object);
            var connectionSync = CreateConnectionMock(commandSync).Object;
            var singleSync = connectionSync.QuerySingle<DummyEntity, MockDbParameter>(new DummyProcedure());

            var commandAsync = new Mock<IDbCommand>();
            commandAsync.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(entities).Object);
            var connectionAsync = CreateConnectionMock(commandAsync).Object;
            var singleAsync = await connectionAsync.QuerySingleAsync<DummyEntity, MockDbParameter>(new DummyProcedure());

            Assert.Equal("Alice", singleSync.Name);
            Assert.Equal("Alice", singleAsync.Name);
        }

        /// <summary>
        /// Verifies procedure overloads for QuerySingleOrDefault and QuerySingleOrDefaultAsync.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QuerySingleOrDefault_WithProcedure_ReturnsSingleOrDefault_ForSyncAndAsync()
        {
            var one = new List<DummyEntity> { new DummyEntity { Id = 1, Name = "Alice" } };

            var commandSync = new Mock<IDbCommand>();
            commandSync.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(one).Object);
            var connectionSync = CreateConnectionMock(commandSync).Object;
            var syncResult = connectionSync.QuerySingleOrDefault<DummyEntity, MockDbParameter>(new DummyProcedure());

            var commandAsync = new Mock<IDbCommand>();
            commandAsync.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(new List<DummyEntity>()).Object);
            var connectionAsync = CreateConnectionMock(commandAsync).Object;
            var asyncResult = await connectionAsync.QuerySingleOrDefaultAsync<DummyEntity, MockDbParameter>(new DummyProcedure());

            Assert.Equal("Alice", syncResult.Name);
            Assert.Null(asyncResult);
        }

        /// <summary>
        /// Verifies that Query with procedure overload throws an ArgumentNullException when the connection is null.
        /// </summary>
        [Fact]
        public void Query_WithProcedure_ThrowsOnNullConnection()
        {
            Assert.Throws<ArgumentNullException>(() =>
                HydrixDataCore.Query<DummyEntity, MockDbParameter>(
                    null,
                    new DummyProcedure()));
        }

        /// <summary>
        /// Verifies that Query with procedure overload throws an ArgumentNullException when the procedure is null.
        /// </summary>
        [Fact]
        public void Query_WithProcedure_ThrowsOnNullProcedure()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(new List<DummyEntity>()).Object);
            var connection = CreateConnectionMock(commandMock).Object;

            Assert.Throws<ArgumentNullException>(() =>
                connection.Query<DummyEntity, MockDbParameter>(null));
        }

        /// <summary>
        /// Verifies that QueryAsync with procedure overload throws an ArgumentNullException when the procedure is null.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithProcedure_ThrowsOnNullProcedure()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(new List<DummyEntity>()).Object);
            var connection = CreateConnectionMock(commandMock).Object;

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await connection.QueryAsync<DummyEntity, MockDbParameter>(null));
        }

        /// <summary>
        /// Verifies that QueryFirst with procedure overload throws an InvalidOperationException when the result is empty.
        /// </summary>
        [Fact]
        public void QueryFirst_WithProcedure_ThrowsWhenEmpty()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(new List<DummyEntity>()).Object);
            var connection = CreateConnectionMock(commandMock).Object;

            Assert.Throws<InvalidOperationException>(() =>
                connection.QueryFirst<DummyEntity, MockDbParameter>(new DummyProcedure()));
        }

        /// <summary>
        /// Verifies that QuerySingle with procedure overload throws an InvalidOperationException when the result contains more than one element.
        /// </summary>
        [Fact]
        public void QuerySingle_WithProcedure_ThrowsWhenMoreThanOne()
        {
            var entities = new List<DummyEntity>
            {
                new DummyEntity { Id = 1, Name = "Alice" },
                new DummyEntity { Id = 2, Name = "Bob" }
            };

            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(entities).Object);
            var connection = CreateConnectionMock(commandMock).Object;

            Assert.Throws<InvalidOperationException>(() =>
                connection.QuerySingle<DummyEntity, MockDbParameter>(new DummyProcedure()));
        }

        /// <summary>
        /// Verifies that QuerySingleAsync with procedure overload throws an InvalidOperationException when the result is empty.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QuerySingleAsync_WithProcedure_ThrowsWhenEmpty()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(new List<DummyEntity>()).Object);
            var connection = CreateConnectionMock(commandMock).Object;

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await connection.QuerySingleAsync<DummyEntity, MockDbParameter>(new DummyProcedure()));
        }

        /// <summary>
        /// Verifies that QuerySingleOrDefault with procedure overload throws an InvalidOperationException when the result contains more than one element.
        /// </summary>
        [Fact]
        public void QuerySingleOrDefault_WithProcedure_ThrowsWhenMoreThanOne()
        {
            var entities = new List<DummyEntity>
            {
                new DummyEntity { Id = 1, Name = "Alice" },
                new DummyEntity { Id = 2, Name = "Bob" }
            };

            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(entities).Object);
            var connection = CreateConnectionMock(commandMock).Object;

            Assert.Throws<InvalidOperationException>(() =>
                connection.QuerySingleOrDefault<DummyEntity, MockDbParameter>(new DummyProcedure()));
        }

        /// <summary>
        /// Verifies that QuerySingleOrDefaultAsync with procedure overload throws an InvalidOperationException when the result contains more than one element.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QuerySingleOrDefaultAsync_WithProcedure_ThrowsWhenMoreThanOne()
        {
            var entities = new List<DummyEntity>
            {
                new DummyEntity { Id = 1, Name = "Alice" },
                new DummyEntity { Id = 2, Name = "Bob" }
            };

            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(entities).Object);
            var connection = CreateConnectionMock(commandMock).Object;

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await connection.QuerySingleOrDefaultAsync<DummyEntity, MockDbParameter>(new DummyProcedure()));
        }

        /// <summary>
        /// Verifies that QueryFirstAsync with procedure overload throws when the result is empty.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryFirstAsync_WithProcedure_ThrowsWhenEmpty()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(new List<DummyEntity>()).Object);
            var connection = CreateConnectionMock(commandMock).Object;

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await connection.QueryFirstAsync<DummyEntity, MockDbParameter>(new DummyProcedure()));
        }

        /// <summary>
        /// Verifies that QueryFirstOrDefault with procedure overload returns default when the result is empty.
        /// </summary>
        [Fact]
        public void QueryFirstOrDefault_WithProcedure_ReturnsDefaultWhenEmpty()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(new List<DummyEntity>()).Object);
            var connection = CreateConnectionMock(commandMock).Object;

            var result = connection.QueryFirstOrDefault<DummyEntity, MockDbParameter>(new DummyProcedure());

            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that QueryFirstOrDefaultAsync with procedure overload returns first item when available.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryFirstOrDefaultAsync_WithProcedure_ReturnsFirstWhenAvailable()
        {
            var entities = new List<DummyEntity> { new DummyEntity { Id = 1, Name = "Alice" } };
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(entities).Object);
            var connection = CreateConnectionMock(commandMock).Object;

            var result = await connection.QueryFirstOrDefaultAsync<DummyEntity, MockDbParameter>(new DummyProcedure());

            Assert.NotNull(result);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that QuerySingle with procedure overload throws when the result is empty.
        /// </summary>
        [Fact]
        public void QuerySingle_WithProcedure_ThrowsWhenEmpty()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(new List<DummyEntity>()).Object);
            var connection = CreateConnectionMock(commandMock).Object;

            Assert.Throws<InvalidOperationException>(() =>
                connection.QuerySingle<DummyEntity, MockDbParameter>(new DummyProcedure()));
        }

        /// <summary>
        /// Verifies that QuerySingleAsync with procedure overload throws when the result contains more than one element.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QuerySingleAsync_WithProcedure_ThrowsWhenMoreThanOne()
        {
            var entities = new List<DummyEntity>
            {
                new DummyEntity { Id = 1, Name = "Alice" },
                new DummyEntity { Id = 2, Name = "Bob" }
            };

            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(entities).Object);
            var connection = CreateConnectionMock(commandMock).Object;

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await connection.QuerySingleAsync<DummyEntity, MockDbParameter>(new DummyProcedure()));
        }

        /// <summary>
        /// Verifies that QuerySingleOrDefault with procedure overload returns default when the result is empty.
        /// </summary>
        [Fact]
        public void QuerySingleOrDefault_WithProcedure_ReturnsDefaultWhenEmpty()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(new List<DummyEntity>()).Object);
            var connection = CreateConnectionMock(commandMock).Object;

            var result = connection.QuerySingleOrDefault<DummyEntity, MockDbParameter>(new DummyProcedure());

            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that QuerySingleOrDefaultAsync with procedure overload returns the single item when available.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QuerySingleOrDefaultAsync_WithProcedure_ReturnsSingleWhenAvailable()
        {
            var entities = new List<DummyEntity> { new DummyEntity { Id = 1, Name = "Alice" } };
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(entities).Object);
            var connection = CreateConnectionMock(commandMock).Object;

            var result = await connection.QuerySingleOrDefaultAsync<DummyEntity, MockDbParameter>(new DummyProcedure());

            Assert.NotNull(result);
            Assert.Equal("Alice", result.Name);
        }
    }
}