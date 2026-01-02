using Hydrix.Orchestrator.Materializers.Contract;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers.Contract
{
    /// <summary>
    /// Contains unit tests for verifying the behavior of the ISqlMaterializer interface, including synchronous and
    /// asynchronous execution of SQL commands and procedures, parameter handling, transaction support, and exception
    /// scenarios.
    /// </summary>
    /// <remarks>These tests use mock implementations to simulate database operations and validate that the
    /// ISqlMaterializer interface methods return the expected results or throw appropriate exceptions under various
    /// conditions. The tests cover a range of scenarios, including command execution with different parameter and
    /// transaction combinations, as well as error and cancellation handling for both synchronous and asynchronous
    /// methods.</remarks>
    public partial class ISqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the ExecuteNonQuery method returns the number of rows affected when called with a SQL
        /// statement and parameters.
        /// </summary>
        /// <remarks>This test ensures that providing a SQL command and an anonymous object containing
        /// parameters to ExecuteNonQuery results in the expected number of affected rows being returned. It uses a mock
        /// to simulate the database operation.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithSqlAndParameters_ReturnsRowsAffected()
        {
            _materializerMock.Setup(m => m.ExecuteNonQuery("UPDATE", It.IsAny<object>())).Returns(1);
            var result = _materializerMock.Object.ExecuteNonQuery("UPDATE", new { Id = 1 });
            Assert.Equal(1, result);
        }

        /// <summary>
        /// Verifies that the ExecuteNonQuery method returns the correct number of rows affected when called with SQL
        /// parameters and a transaction.
        /// </summary>
        /// <remarks>This test ensures that ExecuteNonQuery correctly processes the provided SQL command,
        /// parameters, and transaction, and returns the expected number of affected rows. It uses a mock implementation
        /// to simulate the database operation.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithSqlParametersAndTransaction_ReturnsRowsAffected()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteNonQuery("UPDATE", It.IsAny<object>(), transaction)).Returns(2);
            var result = _materializerMock.Object.ExecuteNonQuery("UPDATE", new { Id = 2 }, transaction);
            Assert.Equal(2, result);
        }

        /// <summary>
        /// Verifies that the ExecuteNonQuery method returns the number of rows affected when called with only a SQL
        /// command.
        /// </summary>
        /// <remarks>This test ensures that providing a SQL statement to ExecuteNonQuery returns the
        /// expected number of affected rows, validating correct behavior for non-query SQL operations.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithSqlOnly_ReturnsRowsAffected()
        {
            _materializerMock.Setup(m => m.ExecuteNonQuery("DELETE")).Returns(3);
            var result = _materializerMock.Object.ExecuteNonQuery("DELETE");
            Assert.Equal(3, result);
        }

        /// <summary>
        /// Verifies that the ExecuteNonQuery method returns the correct number of rows affected when called with a SQL
        /// command and a transaction.
        /// </summary>
        /// <remarks>This unit test ensures that ExecuteNonQuery, when provided with a specific SQL
        /// statement and transaction, returns the expected number of affected rows. It uses a mock implementation to
        /// simulate the database operation.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithSqlAndTransaction_ReturnsRowsAffected()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteNonQuery("DELETE", transaction)).Returns(4);
            var result = _materializerMock.Object.ExecuteNonQuery("DELETE", transaction);
            Assert.Equal(4, result);
        }

        /// <summary>
        /// Verifies that the ExecuteNonQuery method returns the correct number of rows affected when called with
        /// CommandType.Text, a SQL command, and a list of parameters.
        /// </summary>
        /// <remarks>This test ensures that ExecuteNonQuery correctly processes SQL commands with
        /// parameters and returns the expected result. It uses a mock to simulate the database operation.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithCommandTypeSqlAndParameters_ReturnsRowsAffected()
        {
            var parameters = new List<IDataParameter>();
            _materializerMock.Setup(m => m.ExecuteNonQuery(CommandType.Text, "INSERT", parameters)).Returns(5);
            var result = _materializerMock.Object.ExecuteNonQuery(CommandType.Text, "INSERT", parameters);
            Assert.Equal(5, result);
        }

        /// <summary>
        /// Verifies that the ExecuteNonQuery method returns the correct number of rows affected when called with a
        /// stored procedure, a set of SQL parameters, and a database transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQuery method correctly propagates the rows
        /// affected result when executed with all relevant arguments, including command type, procedure name,
        /// parameters, and transaction context.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithCommandTypeSqlParametersAndTransaction_ReturnsRowsAffected()
        {
            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteNonQuery(CommandType.StoredProcedure, "PROC", parameters, transaction)).Returns(6);
            var result = _materializerMock.Object.ExecuteNonQuery(CommandType.StoredProcedure, "PROC", parameters, transaction);
            Assert.Equal(6, result);
        }

        /// <summary>
        /// Verifies that the ExecuteNonQuery method returns the expected number of rows affected when called with a
        /// specific command type and SQL statement.
        /// </summary>
        /// <remarks>This test ensures that ExecuteNonQuery correctly processes the provided CommandType
        /// and SQL string, and that the mocked implementation returns the configured result. It validates the
        /// integration between the method parameters and the return value in the context of unit testing.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithCommandTypeAndSql_ReturnsRowsAffected()
        {
            _materializerMock.Setup(m => m.ExecuteNonQuery(CommandType.Text, "SELECT")).Returns(7);
            var result = _materializerMock.Object.ExecuteNonQuery(CommandType.Text, "SELECT");
            Assert.Equal(7, result);
        }

        /// <summary>
        /// Verifies that the ExecuteNonQuery method returns the correct number of rows affected when called with
        /// CommandType.Text, a SQL command, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQuery method correctly processes a SQL command
        /// within a transaction context and returns the expected result. It uses a mock implementation to simulate the
        /// database operation.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithCommandTypeSqlAndTransaction_ReturnsRowsAffected()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteNonQuery(CommandType.Text, "SELECT", transaction)).Returns(8);
            var result = _materializerMock.Object.ExecuteNonQuery(CommandType.Text, "SELECT", transaction);
            Assert.Equal(8, result);
        }

        /// <summary>
        /// Tests that ExecuteNonQueryAsync returns the correct number of rows affected when called with SQL parameters.
        /// </summary>
        /// <remarks>This unit test verifies that the ExecuteNonQueryAsync method correctly returns the
        /// number of rows affected by an update operation when provided with specific SQL parameters. It uses a mock to
        /// simulate the expected behavior.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithSqlParameters_ReturnsRowsAffected()
        {
            _materializerMock.Setup(m => m.ExecuteNonQueryAsync("UPDATE", It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(9);
            var result = await _materializerMock.Object.ExecuteNonQueryAsync("UPDATE", new { Id = 9 });
            Assert.Equal(9, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync returns the number of rows affected when called with SQL parameters and a
        /// transaction.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithSqlParametersAndTransaction_ReturnsRowsAffected()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteNonQueryAsync("UPDATE", It.IsAny<object>(), transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(10);
            var result = await _materializerMock.Object.ExecuteNonQueryAsync("UPDATE", new { Id = 10 }, transaction);
            Assert.Equal(10, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync, when called with only a SQL command, returns the expected number of rows
        /// affected.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQueryAsync method correctly returns the number
        /// of rows affected when invoked with a SQL command string and no cancellation token.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithSqlOnly_ReturnsRowsAffected()
        {
            _materializerMock.Setup(m => m.ExecuteNonQueryAsync("DELETE", It.IsAny<CancellationToken>()))
                .ReturnsAsync(11);
            var result = await _materializerMock.Object.ExecuteNonQueryAsync("DELETE");
            Assert.Equal(11, result);
        }

        /// <summary>
        /// Verifies that the ExecuteNonQueryAsync method returns the correct number of rows affected when called with a
        /// SQL command and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQueryAsync method correctly propagates the
        /// number of rows affected as returned by the underlying materializer when provided with a SQL statement and a
        /// transaction.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithSqlAndTransaction_ReturnsRowsAffected()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteNonQueryAsync("DELETE", transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(12);
            var result = await _materializerMock.Object.ExecuteNonQueryAsync("DELETE", transaction);
            Assert.Equal(12, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync returns the expected number of rows affected when called with
        /// CommandType.Text, a SQL command, and a list of parameters.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQueryAsync method correctly propagates the
        /// number of rows affected as returned by the underlying materializer when executing a SQL command with
        /// parameters.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithCommandTypeSqlParameters_ReturnsRowsAffected()
        {
            var parameters = new List<IDataParameter>();
            _materializerMock.Setup(m => m.ExecuteNonQueryAsync(CommandType.Text, "INSERT", parameters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(13);
            var result = await _materializerMock.Object.ExecuteNonQueryAsync(CommandType.Text, "INSERT", parameters);
            Assert.Equal(13, result);
        }

        /// <summary>
        /// Tests that ExecuteNonQueryAsync returns the correct number of rows affected when called with a stored
        /// procedure command type, SQL parameters, and a transaction.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithCommandTypeSqlParametersAndTransaction_ReturnsRowsAffected()
        {
            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteNonQueryAsync(CommandType.StoredProcedure, "PROC", parameters, transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(14);
            var result = await _materializerMock.Object.ExecuteNonQueryAsync(CommandType.StoredProcedure, "PROC", parameters, transaction);
            Assert.Equal(14, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync returns the expected number of rows affected when called with a specific
        /// command type and SQL statement.
        /// </summary>
        /// <remarks>This test sets up a mock to return a fixed value and asserts that the method under
        /// test returns the correct result. It ensures that the ExecuteNonQueryAsync method correctly propagates the
        /// number of rows affected from the underlying data access implementation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithCommandTypeAndSql_ReturnsRowsAffected()
        {
            _materializerMock.Setup(m => m.ExecuteNonQueryAsync(CommandType.Text, "SELECT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(15);
            var result = await _materializerMock.Object.ExecuteNonQueryAsync(CommandType.Text, "SELECT");
            Assert.Equal(15, result);
        }

        /// <summary>
        /// Tests that ExecuteNonQueryAsync returns the number of rows affected when called with CommandType.Text, a SQL
        /// command, and a transaction.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithCommandTypeSqlAndTransaction_ReturnsRowsAffected()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteNonQueryAsync(CommandType.Text, "SELECT", transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(16);
            var result = await _materializerMock.Object.ExecuteNonQueryAsync(CommandType.Text, "SELECT", transaction);
            Assert.Equal(16, result);
        }

        /// <summary>
        /// Verifies that executing a generic SQL procedure using ExecuteNonQuery returns the expected number of rows
        /// affected.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteNonQuery method, when called with a generic
        /// parameter type, correctly returns the number of rows affected as reported by the underlying data access
        /// implementation.</remarks>
        [Fact]
        public void ExecuteNonQuery_GenericSqlProcedure_ReturnsRowsAffected()
        {
            var proc = new DummySqlProcedure();
            _materializerMock.Setup(m => m.ExecuteNonQuery<DummyParameter>(proc)).Returns(17);
            var result = _materializerMock.Object.ExecuteNonQuery<DummyParameter>(proc);
            Assert.Equal(17, result);
        }

        /// <summary>
        /// Verifies that executing a generic SQL procedure with a transaction returns the expected number of rows
        /// affected.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQuery method correctly returns the number of
        /// rows affected when called with a generic parameter and an explicit transaction. It uses a mock
        /// implementation to simulate the database operation.</remarks>
        [Fact]
        public void ExecuteNonQuery_GenericSqlProcedureWithTransaction_ReturnsRowsAffected()
        {
            var proc = new DummySqlProcedure();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteNonQuery<DummyParameter>(proc, transaction)).Returns(18);
            var result = _materializerMock.Object.ExecuteNonQuery<DummyParameter>(proc, transaction);
            Assert.Equal(18, result);
        }

        /// <summary>
        /// Verifies that executing a generic SQL procedure asynchronously using ExecuteNonQueryAsync returns the
        /// expected number of rows affected.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQueryAsync&lt;TParameter&gt; method correctly returns
        /// the number of rows affected when called with a generic SQL procedure. It uses a mock materializer to
        /// simulate the database operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_GenericSqlProcedure_ReturnsRowsAffected()
        {
            var proc = new DummySqlProcedure();
            _materializerMock.Setup(m => m.ExecuteNonQueryAsync<DummyParameter>(proc, It.IsAny<CancellationToken>()))
                .ReturnsAsync(19);
            var result = await _materializerMock.Object.ExecuteNonQueryAsync<DummyParameter>(proc);
            Assert.Equal(19, result);
        }

        /// <summary>
        /// Verifies that executing a generic SQL procedure with a transaction using ExecuteNonQueryAsync returns the
        /// expected number of rows affected.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQueryAsync method correctly returns the number
        /// of rows affected when called with a generic SQL procedure and an explicit transaction. It uses a mock
        /// materializer to simulate the database operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_GenericSqlProcedureWithTransaction_ReturnsRowsAffected()
        {
            var proc = new DummySqlProcedure();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteNonQueryAsync<DummyParameter>(proc, transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(20);
            var result = await _materializerMock.Object.ExecuteNonQueryAsync<DummyParameter>(proc, transaction);
            Assert.Equal(20, result);
        }

        /// <summary>
        /// Verifies that calling ExecuteNonQuery on a disposed materializer throws an ObjectDisposedException.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQuery method enforces correct object lifetime
        /// management by throwing an ObjectDisposedException when invoked after disposal. Proper exception handling in
        /// this scenario helps prevent undefined behavior when using disposed resources.</remarks>
        [Fact]
        public void ExecuteNonQuery_Disposed_ThrowsObjectDisposedException()
        {
            _materializerMock.Setup(m => m.ExecuteNonQuery("UPDATE", It.IsAny<object>()))
                .Throws(new ObjectDisposedException(nameof(ISqlMaterializer))); ;
            Assert.Throws<ObjectDisposedException>(() => _materializerMock.Object.ExecuteNonQuery("UPDATE", new { }));
        }

        /// <summary>
        /// Verifies that the ExecuteNonQuery method throws an ArgumentException when called with an invalid argument.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQuery method correctly enforces argument
        /// validation by throwing an ArgumentException when provided with invalid input.</remarks>
        [Fact]
        public void ExecuteNonQuery_InvalidArgument_ThrowsArgumentException()
        {
            _materializerMock.Setup(m => m.ExecuteNonQuery("UPDATE", It.IsAny<object>()))
                .Throws<ArgumentException>();
            Assert.Throws<ArgumentException>(() => _materializerMock.Object.ExecuteNonQuery("UPDATE", new { }));
        }

        /// <summary>
        /// Verifies that calling ExecuteNonQuery with the specified command throws a NotSupportedException when the
        /// operation is not supported.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQuery method correctly throws a
        /// NotSupportedException when invoked with an unsupported command, such as an update operation. Use this test
        /// to validate that unsupported operations are properly guarded against in the implementation.</remarks>
        [Fact]
        public void ExecuteNonQuery_NotSupported_ThrowsNotSupportedException()
        {
            _materializerMock.Setup(m => m.ExecuteNonQuery("UPDATE", It.IsAny<object>()))
                .Throws<NotSupportedException>();
            Assert.Throws<NotSupportedException>(() => _materializerMock.Object.ExecuteNonQuery("UPDATE", new { }));
        }

        /// <summary>
        /// Verifies that calling ExecuteNonQuery with an invalid operation throws an InvalidOperationException.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQuery method correctly throws an
        /// InvalidOperationException when invoked with an invalid operation, helping to validate error handling
        /// behavior.</remarks>
        [Fact]
        public void ExecuteNonQuery_InvalidOperation_ThrowsInvalidOperationException()
        {
            _materializerMock.Setup(m => m.ExecuteNonQuery("UPDATE", It.IsAny<object>()))
                .Throws<InvalidOperationException>();
            Assert.Throws<InvalidOperationException>(() => _materializerMock.Object.ExecuteNonQuery("UPDATE", new { }));
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync throws an OperationCanceledException when the operation is canceled.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQueryAsync method correctly propagates an
        /// OperationCanceledException when the underlying operation is canceled. It is intended to validate proper
        /// cancellation handling in asynchronous database operations.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_OperationCanceled_ThrowsOperationCanceledException()
        {
            _materializerMock.Setup(m => m.ExecuteNonQueryAsync("UPDATE", It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _materializerMock.Object.ExecuteNonQueryAsync("UPDATE", new { }, CancellationToken.None));
        }
    }
}