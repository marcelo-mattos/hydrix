using Hydrix.Orchestrator.Materializers;
using Moq;
using System;
using System.Data;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for the SqlMaterializer class, verifying its public properties and behaviors.
    /// </summary>
    /// <remarks>These tests ensure that SqlMaterializer correctly exposes its connection, transaction, state,
    /// timeout, and logging properties, and that it enforces expected preconditions such as argument validation and
    /// object disposal. The tests use mock objects to simulate database connections and transactions.</remarks>
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the DbConnection property of the materializer returns the injected database connection
        /// instance.
        /// </summary>
        /// <remarks>This test ensures that when a specific IDbConnection is provided to the materializer,
        /// it is correctly stored and accessible via the DbConnection property. This behavior is important for
        /// scenarios where connection management or dependency injection is required.</remarks>
        [Fact]
        public void DbConnection_ReturnsInjectedConnection()
        {
            var mockConn = new Mock<IDbConnection>().Object;
            var mat = CreateMaterializer(dbConnection: mockConn);
            Assert.Same(mockConn, typeof(SqlMaterializer)
                .GetProperty("DbConnection", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(mat));
        }

        /// <summary>
        /// Verifies that the DbTransaction property returns the injected transaction instance.
        /// </summary>
        /// <remarks>This test ensures that when a transaction is provided during materializer creation,
        /// the DbTransaction property exposes the same instance. This helps confirm correct dependency injection
        /// behavior for transaction management.</remarks>
        [Fact]
        public void DbTransaction_ReturnsInjectedTransaction()
        {
            var mockTran = new Mock<IDbTransaction>().Object;
            var mat = CreateMaterializer(dbTransaction: mockTran);
            Assert.Same(mockTran, typeof(SqlMaterializer)
                .GetProperty("DbTransaction", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(mat));
        }

        /// <summary>
        /// Verifies that the IsTransactionActive property returns true when the underlying transaction is not null.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly identifies an active transaction
        /// when a non-null IDbTransaction is provided.</remarks>
        [Fact]
        public void IsTransactionActive_TrueIfTransactionNotNull()
        {
            var mockTran = new Mock<IDbTransaction>().Object;
            var mat = CreateMaterializer(dbTransaction: mockTran);
            Assert.True(mat.IsTransactionActive);
        }

        /// <summary>
        /// Verifies that the IsTransactionActive property returns false when the underlying transaction is null.
        /// </summary>
        [Fact]
        public void IsTransactionActive_FalseIfTransactionNull()
        {
            var mat = CreateMaterializer(dbTransaction: null);
            Assert.False(mat.IsTransactionActive);
        }

        /// <summary>
        /// Verifies that accessing the ConnectionString property throws an ObjectDisposedException when the
        /// materializer has been disposed.
        /// </summary>
        [Fact]
        public void ConnectionString_ThrowsIfDisposed()
        {
            var mat = CreateMaterializer(isDisposed: true);
            Assert.Throws<ObjectDisposedException>(() => _ = mat.ConnectionString);
        }

        /// <summary>
        /// Verifies that the ConnectionString property returns the underlying connection string from the associated
        /// IDbConnection instance.
        /// </summary>
        [Fact]
        public void ConnectionString_ReturnsUnderlyingConnectionString()
        {
            var mockConn = new Mock<IDbConnection>();
            mockConn.SetupGet(c => c.ConnectionString).Returns("TestConn");
            var mat = CreateMaterializer(dbConnection: mockConn.Object);
            Assert.Equal("TestConn", mat.ConnectionString);
        }

        /// <summary>
        /// Verifies that accessing the State property throws an ObjectDisposedException when the materializer has been
        /// disposed.
        /// </summary>
        [Fact]
        public void State_ThrowsIfDisposed()
        {
            var mat = CreateMaterializer(isDisposed: true);
            Assert.Throws<ObjectDisposedException>(() => _ = mat.State);
        }

        /// <summary>
        /// Verifies that the State property returns the current state of the underlying database connection.
        /// </summary>
        [Fact]
        public void State_ReturnsUnderlyingState()
        {
            var mockConn = new Mock<IDbConnection>();
            mockConn.SetupGet(c => c.State).Returns(ConnectionState.Open);
            var mat = CreateMaterializer(dbConnection: mockConn.Object);
            Assert.Equal(ConnectionState.Open, mat.State);
        }

        /// <summary>
        /// Verifies that the Timeout property returns the value specified during materializer creation.
        /// </summary>
        [Fact]
        public void Timeout_GetReturnsValue()
        {
            var mat = CreateMaterializer(timeout: 42);
            Assert.Equal(42, mat.Timeout);
        }

        /// <summary>
        /// Verifies that setting the Timeout property to zero or a negative value throws an ArgumentException.
        /// </summary>
        /// <remarks>This test ensures that the Timeout property enforces its requirement for positive
        /// values. Attempting to set Timeout to zero or a negative number should result in an ArgumentException being
        /// thrown.</remarks>
        [Fact]
        public void Timeout_SetThrowsIfZeroOrNegative()
        {
            var mat = CreateMaterializer();
            Assert.Throws<ArgumentException>(() => mat.Timeout = 0);
            Assert.Throws<ArgumentException>(() => mat.Timeout = -1);
        }

        /// <summary>
        /// Verifies that setting the Timeout property updates its value as expected.
        /// </summary>
        [Fact]
        public void Timeout_SetUpdatesValue()
        {
            var mat = CreateMaterializer();
            mat.Timeout = 99;
            Assert.Equal(99, mat.Timeout);
        }

        /// <summary>
        /// Verifies that the EnableSqlLogging property is set to true by default.
        /// </summary>
        [Fact]
        public void EnableSqlLogging_DefaultsTrue()
        {
            var mat = CreateMaterializer();
            Assert.True(mat.EnableSqlLogging);
        }

        /// <summary>
        /// Verifies that the EnableSqlLogging property can be set to false.
        /// </summary>
        [Fact]
        public void EnableSqlLogging_CanSetFalse()
        {
            var mat = CreateMaterializer();
            mat.EnableSqlLogging = false;
            Assert.False(mat.EnableSqlLogging);
        }

        /// <summary>
        /// Verifies that a newly created materializer instance is not disposed by default.
        /// </summary>
        [Fact]
        public void IsDisposed_DefaultsFalse()
        {
            var mat = CreateMaterializer();
            Assert.False(mat.IsDisposed);
        }

        /// <summary>
        /// Verifies that the IsDisposing property of a newly created materializer instance is false by default.
        /// </summary>
        [Fact]
        public void IsDisposing_DefaultsFalse()
        {
            var mat = CreateMaterializer();
            Assert.False(mat.IsDisposing);
        }
    }
}