using Hydrix.Orchestrator.Materializers;
using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for the SqlMaterializer.Dispose logic, verifying correct resource cleanup and state management.
    /// </summary>
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Represents a test implementation of a database connection for use in unit tests or scenarios where a real
        /// database connection is not required.
        /// </summary>
        /// <remarks>This class provides a minimal, non-functional implementation of the DbConnection
        /// abstract class. All members return default or empty values, and no actual database operations are performed.
        /// Use this class to mock or stub database connections in testing environments without connecting to a real
        /// database.</remarks>
        private class TestDbConnection : DbConnection
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
        private class TestSqlMaterializer : SqlMaterializer
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
            public TestSqlMaterializer() : base(null)
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
            /// Sets the disposed state of the current instance.
            /// </summary>
            /// <remarks>This method is intended for advanced scenarios where manual control of the
            /// disposed state is required. Improper use may lead to inconsistent object state or resource
            /// leaks.</remarks>
            /// <param name="value">true to mark the instance as disposed; otherwise, false.</param>
            public void SetDisposed(bool value)
                => typeof(SqlMaterializer).GetProperty("IsDisposed", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(this, value);
        }

        /// <summary>
        /// A test implementation of the SQL materializer that throws exceptions when certain operations are performed.
        /// </summary>
        /// <remarks>This class is intended for use in testing scenarios where it is necessary to simulate
        /// failures during transaction rollback or connection closure. It overrides specific methods to throw
        /// exceptions after invoking the base implementation, allowing tests to verify error handling logic.</remarks>
        private class ExceptionThrowingSqlMaterializer : TestSqlMaterializer
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
        /// Verifies that calling Dispose sets the IsDisposed and IsDisposing properties to true.
        /// </summary>
        /// <remarks>This unit test ensures that the TestSqlMaterializer correctly updates its disposal
        /// state when Dispose is invoked. It is intended to validate the object's resource management
        /// behavior.</remarks>
        [Fact]
        public void Dispose_SetsIsDisposedAndIsDisposing()
        {
            var mat = new TestSqlMaterializer();
            mat.Dispose();
            Assert.True(mat.IsDisposed);
            Assert.True(mat.IsDisposing);
        }

        /// <summary>
        /// Verifies that disposing the TestSqlMaterializer instance calls both Rollback and Close operations.
        /// </summary>
        /// <remarks>This test ensures that the Dispose method triggers the expected cleanup actions by
        /// asserting that both RollbackCalled and CloseCalled are set to true after disposal.</remarks>
        [Fact]
        public void Dispose_CallsRollbackAndClose()
        {
            var mat = new TestSqlMaterializer();
            mat.Dispose();
            Assert.True(mat.RollbackCalled);
            Assert.True(mat.CloseCalled);
        }

        /// <summary>
        /// Verifies that disposing the materializer disposes the underlying database connection and sets the
        /// DbConnection property to null.
        /// </summary>
        /// <remarks>This test ensures that resource cleanup is performed correctly when the materializer
        /// is disposed. It checks that the associated database connection is properly disposed and that the reference
        /// to the connection is cleared.</remarks>
        [Fact]
        public void Dispose_DisposesDbConnectionAndSetsNull()
        {
            var mat = new TestSqlMaterializer();
            var conn = (TestDbConnection)mat.DbConnection;
            mat.Dispose();
            Assert.True(conn.Disposed);
            Assert.Null(mat.DbConnection);
        }

        /// <summary>
        /// Verifies that calling Dispose multiple times on a TestSqlMaterializer instance does not perform disposal
        /// actions more than once.
        /// </summary>
        /// <remarks>This test ensures that repeated calls to Dispose do not result in multiple rollbacks
        /// or closures, and that the disposed state remains consistent after the first call.</remarks>
        [Fact]
        public void Dispose_DoesNotDisposeTwice()
        {
            var mat = new TestSqlMaterializer();
            mat.Dispose();
            var disposedState = mat.IsDisposed;
            mat.RollbackCalled = false;
            mat.CloseCalled = false;
            mat.Dispose();
            Assert.True(mat.IsDisposed);
            Assert.True(disposedState);
            Assert.False(mat.RollbackCalled);
            Assert.False(mat.CloseCalled);
        }

        /// <summary>
        /// Verifies that the Dispose method correctly handles exceptions thrown during transaction rollback and
        /// connection close operations.
        /// </summary>
        /// <remarks>This test ensures that Dispose sets the object's disposal state even if exceptions
        /// occur in RollbackTransaction or CloseConnection. It validates that IsDisposed and IsDisposing are set to
        /// true after disposal, regardless of exceptions.</remarks>
        [Fact]
        public void Dispose_HandlesExceptionsInRollbackAndClose()
        {
            var matEx = new ExceptionThrowingSqlMaterializer();
            matEx.SetDisposed(false);

            Exception ex = Record.Exception(() => matEx.Dispose());
            Assert.True(matEx.IsDisposed);
            Assert.True(matEx.IsDisposing);
        }
    }
}