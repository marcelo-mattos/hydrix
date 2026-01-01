using Hydrix.Orchestrator.Materializers;
using System;
using System.Data;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for the constructors of the SqlMaterializer class.
    /// </summary>
    /// <remarks>These tests verify that the SqlMaterializer constructors correctly assign connection and
    /// property values, apply default values when appropriate, and handle null connections as expected.</remarks>
    public partial class SqlMaterializerTests
    {
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
                timeout ?? Faker.NumberFaker.Number(30, 120),
                parameterPrefix ?? Faker.StringFaker.Alpha(100)
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
        /// Verifies that the SqlMaterializer constructor correctly assigns the provided connection, timeout, and
        /// parameter prefix to their respective properties.
        /// </summary>
        /// <remarks>This test ensures that the constructor initializes all relevant properties with the
        /// values supplied during instantiation. It checks both public and non-public members as needed to confirm
        /// correct assignment.</remarks>
        [Fact]
        public void Constructor_Assigns_Connection_And_Properties()
        {
            var connection = new DummyDbConnection();
            int timeout = 77;
            string prefix = "@@";
            var materializer = new SqlMaterializer(connection, timeout, prefix);

            // DbConnection is public or internal property
            var dbConnProp = typeof(SqlMaterializer).GetProperty("DbConnection", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var dbConn = dbConnProp.GetValue(materializer);
            Assert.Same(connection, dbConn);

            var timeoutProp = typeof(SqlMaterializer).GetProperty("Timeout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.Equal(timeout, (int)timeoutProp.GetValue(materializer));

            Assert.Equal(prefix, GetPrivateParameterPrefix(materializer));
        }

        /// <summary>
        /// Verifies that the SqlMaterializer constructor initializes the Timeout and parameter prefix properties to
        /// their default values.
        /// </summary>
        /// <remarks>This test ensures that when a new instance of SqlMaterializer is created without
        /// explicitly specifying a timeout or parameter prefix, the instance uses the class's predefined default values
        /// for these settings.</remarks>
        [Fact]
        public void Constructor_Uses_Default_Timeout_And_Prefix()
        {
            var connection = new DummyDbConnection();
            var defaultTimeoutField = typeof(SqlMaterializer).GetField("DefaultTimeout", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var defaultPrefixField = typeof(SqlMaterializer).GetField("DefaultParameterPrefix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            int defaultTimeout = (int)defaultTimeoutField.GetValue(null);
            string defaultPrefix = (string)defaultPrefixField.GetValue(null);

            var materializer = new SqlMaterializer(connection);

            var timeoutProp = typeof(SqlMaterializer).GetProperty("Timeout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.Equal(defaultTimeout, (int)timeoutProp.GetValue(materializer));
            Assert.Equal(defaultPrefix, GetPrivateParameterPrefix(materializer));
        }

        /// <summary>
        /// Verifies that the SqlMaterializer constructor allows a null DbConnection parameter without throwing an
        /// exception.
        /// </summary>
        /// <remarks>This test ensures that passing null to the SqlMaterializer constructor results in a
        /// materializer instance with a null DbConnection property.</remarks>
        [Fact]
        public void Constructor_Allows_Null_Connection()
        {
            var materializer = new SqlMaterializer(null);

            var dbConnProp = typeof(SqlMaterializer).GetProperty("DbConnection", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.Null(dbConnProp.GetValue(materializer));
        }
    }
}