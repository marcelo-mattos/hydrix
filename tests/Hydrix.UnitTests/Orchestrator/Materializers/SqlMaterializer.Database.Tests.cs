using Moq;
using System;
using System.Data;
using System.Data.Common;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for the SQL materializer component, verifying correct behavior of connection and transaction
    /// management methods.
    /// </summary>
    /// <remarks>These tests ensure that the SQL materializer correctly handles opening and closing database
    /// connections, as well as beginning, committing, and rolling back transactions under various conditions, including
    /// proper exception handling when disposed or in invalid states. The tests use mock objects to simulate database
    /// connections and transactions.</remarks>
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the OpenConnection method opens the database connection when it is in the Closed state.
        /// </summary>
        /// <remarks>This unit test ensures that OpenConnection calls the Open method on the underlying
        /// DbConnection if the connection is currently closed. It uses a mock DbConnection to validate the expected
        /// behavior.</remarks>
        [Fact]
        public void OpenConnection_Opens_WhenClosed()
        {
            var connMock = new Mock<DbConnection>();
            connMock.SetupGet(c => c.State).Returns(ConnectionState.Closed);
            var mat = new TestSqlMaterializer();
            mat.SetDbConnection(connMock.Object);

            mat.OpenConnection();

            connMock.Verify(c => c.Open(), Times.Once);
        }

        /// <summary>
        /// Verifies that calling OpenConnection does not attempt to open the database connection if it is already open.
        /// </summary>
        /// <remarks>This test ensures that OpenConnection does not redundantly call Open on a
        /// DbConnection whose State is already Open, preserving expected connection management behavior.</remarks>
        [Fact]
        public void OpenConnection_DoesNotOpen_WhenAlreadyOpen()
        {
            var connMock = new Mock<DbConnection>();
            connMock.SetupGet(c => c.State).Returns(ConnectionState.Open);
            var mat = new TestSqlMaterializer();
            mat.SetDbConnection(connMock.Object);

            mat.OpenConnection();

            connMock.Verify(c => c.Open(), Times.Never);
        }

        /// <summary>
        /// Verifies that calling OpenConnection on a disposed TestSqlMaterializer instance throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <remarks>This test ensures that the OpenConnection method enforces correct object lifetime
        /// management by throwing an exception when invoked after disposal.</remarks>
        [Fact]
        public void OpenConnection_Throws_WhenDisposed()
        {
            var mat = new TestSqlMaterializer();
            mat.SetDisposed(true);

            Assert.Throws<ObjectDisposedException>(() => mat.OpenConnection());
        }

        /// <summary>
        /// Verifies that the CloseConnection method closes the database connection when it is open.
        /// </summary>
        /// <remarks>This unit test ensures that calling CloseConnection on the TestSqlMaterializer
        /// instance results in the Close method being invoked on the underlying DbConnection if its state is
        /// Open.</remarks>
        [Fact]
        public void CloseConnection_Closes_WhenOpen()
        {
            var connMock = new Mock<DbConnection>();
            connMock.SetupGet(c => c.State).Returns(ConnectionState.Open);
            var mat = new TestSqlMaterializer();
            mat.SetDbConnection(connMock.Object);

            mat.CloseConnection();

            connMock.Verify(c => c.Close(), Times.Once);
        }

        /// <summary>
        /// Verifies that calling CloseConnection on a TestSqlMaterializer instance does not attempt to close the
        /// underlying database connection if it is already closed.
        /// </summary>
        /// <remarks>This test ensures that CloseConnection does not call Close on the DbConnection when
        /// its State is Closed, preventing unnecessary operations or exceptions.</remarks>
        [Fact]
        public void CloseConnection_DoesNothing_WhenClosed()
        {
            var connMock = new Mock<DbConnection>();
            connMock.SetupGet(c => c.State).Returns(ConnectionState.Closed);
            var mat = new TestSqlMaterializer();
            mat.SetDbConnection(connMock.Object);

            mat.CloseConnection();

            connMock.Verify(c => c.Close(), Times.Never);
        }

        /// <summary>
        /// Verifies that calling CloseConnection on a disposed TestSqlMaterializer instance throws an
        /// ObjectDisposedException.
        /// </summary>
        [Fact]
        public void CloseConnection_Throws_WhenDisposed()
        {
            var mat = new TestSqlMaterializer();
            mat.SetDisposed(true);

            Assert.Throws<ObjectDisposedException>(() => mat.CloseConnection());
        }

        /// <summary>
        /// Verifies that calling BeginTransaction with no active transaction starts a new transaction on the underlying
        /// database connection.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly initiates a transaction when none
        /// is currently active, and that the transaction is started with the specified isolation level.</remarks>
        [Fact]
        public void BeginTransaction_Starts_WhenNoneActive()
        {
            var connMock = new Mock<IDbConnection>();
            var tranMock = new Mock<IDbTransaction>();
            connMock.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(tranMock.Object);
            var mat = new TestSqlMaterializer();
            mat.SetDbConnection(connMock.Object);

            mat.BeginTransaction(IsolationLevel.ReadCommitted);

            Assert.NotNull(mat.DbTransaction);
            connMock.Verify(c => c.BeginTransaction(IsolationLevel.ReadCommitted), Times.Once);
        }

        /// <summary>
        /// Verifies that calling BeginTransaction when an active transaction exists throws an
        /// InvalidOperationException.
        /// </summary>
        /// <remarks>This test ensures that the materializer enforces the rule that only one transaction
        /// can be active at a time. Attempting to begin a new transaction while another is active should result in an
        /// exception.</remarks>
        [Fact]
        public void BeginTransaction_Throws_WhenActiveTransaction()
        {
            var mat = new TestSqlMaterializer();
            var tranMock = new Mock<DbTransaction>();
            mat.SetDbTransaction(tranMock.Object);

            Assert.Throws<InvalidOperationException>(() => mat.BeginTransaction(IsolationLevel.ReadCommitted));
        }

        /// <summary>
        /// Verifies that calling BeginTransaction on a disposed TestSqlMaterializer instance throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <remarks>This test ensures that the materializer enforces correct object lifetime management
        /// by throwing the appropriate exception when operations are attempted after disposal.</remarks>
        [Fact]
        public void BeginTransaction_Throws_WhenDisposed()
        {
            var mat = new TestSqlMaterializer();
            mat.SetDisposed(true);

            Assert.Throws<ObjectDisposedException>(() => mat.BeginTransaction(IsolationLevel.ReadCommitted));
        }

        /// <summary>
        /// Verifies that the CommitTransaction method commits the current transaction and disposes of the transaction
        /// object.
        /// </summary>
        /// <remarks>This unit test ensures that after calling CommitTransaction, the transaction's Commit
        /// and Dispose methods are each called exactly once, and that the DbTransaction property is set to
        /// null.</remarks>
        [Fact]
        public void CommitTransaction_Commits_AndDisposes()
        {
            var tranMock = new Mock<IDbTransaction>();
            var mat = new TestSqlMaterializer();
            mat.SetDbTransaction(tranMock.Object);

            mat.CommitTransaction();

            tranMock.Verify(t => t.Commit(), Times.Once);
            tranMock.Verify(t => t.Dispose(), Times.Once);
            Assert.Null(mat.DbTransaction);
        }

        /// <summary>
        /// Verifies that calling CommitTransaction without an active transaction throws an InvalidOperationException.
        /// </summary>
        /// <remarks>This test ensures that the CommitTransaction method enforces correct usage by
        /// requiring an active transaction before committing. It helps validate the error handling behavior of the
        /// TestSqlMaterializer class.</remarks>
        [Fact]
        public void CommitTransaction_Throws_WhenNoTransaction()
        {
            var mat = new TestSqlMaterializer();

            Assert.Throws<InvalidOperationException>(() => mat.CommitTransaction());
        }

        /// <summary>
        /// Verifies that calling CommitTransaction on a disposed TestSqlMaterializer instance throws an
        /// ObjectDisposedException.
        /// </summary>
        [Fact]
        public void CommitTransaction_Throws_WhenDisposed()
        {
            var mat = new TestSqlMaterializer();
            mat.SetDisposed(true);

            Assert.Throws<ObjectDisposedException>(() => mat.CommitTransaction());
        }

        /// <summary>
        /// Verifies that the RollbackTransaction method rolls back the current transaction and disposes of the
        /// underlying DbTransaction object.
        /// </summary>
        /// <remarks>This test ensures that after calling RollbackTransaction, the transaction is properly
        /// rolled back, disposed, and the DbTransaction property is set to null.</remarks>
        [Fact]
        public void RollbackTransaction_RollsBack_AndDisposes()
        {
            var tranMock = new Mock<IDbTransaction>();
            var mat = new TestSqlMaterializer();
            mat.SetDbTransaction(tranMock.Object);

            mat.RollbackTransaction();

            tranMock.Verify(t => t.Rollback(), Times.Once);
            tranMock.Verify(t => t.Dispose(), Times.Once);
            Assert.Null(mat.DbTransaction);
        }

        /// <summary>
        /// Verifies that calling RollbackTransaction when no transaction is active and the object is not disposing
        /// throws an InvalidOperationException.
        /// </summary>
        /// <remarks>This test ensures that RollbackTransaction enforces correct usage by throwing an
        /// exception if invoked without an active transaction and outside of a disposal context.</remarks>
        [Fact]
        public void RollbackTransaction_Throws_WhenNoTransaction_AndNotDisposing()
        {
            var mat = new TestSqlMaterializer();
            mat.SetDisposing(false);

            Assert.Throws<InvalidOperationException>(() => mat.RollbackTransaction());
        }

        /// <summary>
        /// Verifies that calling RollbackTransaction does not throw an exception when no transaction is active and the
        /// object is disposing.
        /// </summary>
        /// <remarks>This test ensures that RollbackTransaction is safe to call during disposal, even if
        /// no transaction has been started. It is intended to validate that the method is resilient to being called in
        /// this state and does not result in unexpected exceptions.</remarks>
        [Fact]
        public void RollbackTransaction_DoesNothing_WhenNoTransaction_AndDisposing()
        {
            var mat = new TestSqlMaterializer();
            mat.SetDisposing(true);

            var ex = Record.Exception(() => mat.RollbackTransaction());
            Assert.Null(ex);
        }

        /// <summary>
        /// Verifies that calling RollbackTransaction on a disposed TestSqlMaterializer instance throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <remarks>This test ensures that the RollbackTransaction method enforces correct object
        /// lifetime management by throwing an exception when invoked after disposal.</remarks>
        [Fact]
        public void RollbackTransaction_Throws_WhenDisposed()
        {
            var mat = new TestSqlMaterializer();
            mat.SetDisposed(true);

            Assert.Throws<ObjectDisposedException>(() => mat.RollbackTransaction());
        }
    }
}