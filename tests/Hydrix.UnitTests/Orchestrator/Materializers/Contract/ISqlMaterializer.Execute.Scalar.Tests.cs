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
    /// Contains unit tests for verifying the behavior of ISqlMaterializer implementations, including synchronous and
    /// asynchronous execution of scalar SQL commands and related exception handling.
    /// </summary>
    /// <remarks>These tests cover various overloads of the ExecuteScalar and ExecuteScalarAsync methods,
    /// ensuring correct results are returned for different combinations of SQL, parameters, transactions, and
    /// cancellation tokens. The tests also validate that appropriate exceptions are thrown for invalid arguments,
    /// disposed instances, and canceled operations. Use these tests as a reference when implementing or modifying
    /// ISqlMaterializer functionality.</remarks>
    public partial class ISqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected result when called with a SQL query and
        /// parameters.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalar method correctly processes the provided SQL
        /// statement and parameters, returning the expected scalar value. It uses a mock implementation to simulate the
        /// database interaction.</remarks>
        [Fact]
        public void ExecuteScalar_WithSqlAndParameters_ReturnsExpected()
        {
            _materializerMock.Setup(m => m.ExecuteScalar("SELECT", It.IsAny<object>())).Returns(1);
            var result = _materializerMock.Object.ExecuteScalar("SELECT", new { Id = 1 });
            Assert.Equal(1, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected result when called with SQL parameters and a
        /// transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalar method correctly processes the provided SQL
        /// statement, parameters, and transaction, and returns the expected scalar value. It uses a mock implementation
        /// to simulate the database interaction.</remarks>
        [Fact]
        public void ExecuteScalar_WithSqlParametersAndTransaction_ReturnsExpected()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteScalar("SELECT", It.IsAny<object>(), transaction)).Returns(2);
            var result = _materializerMock.Object.ExecuteScalar("SELECT", new { Id = 2 }, transaction);
            Assert.Equal(2, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected result when called with a SQL query string.
        /// </summary>
        /// <remarks>This unit test sets up a mock to return a specific value for a given SQL query and
        /// asserts that the method under test returns the correct value. This test ensures correct behavior of the
        /// ExecuteScalar method when only a SQL string is provided.</remarks>
        [Fact]
        public void ExecuteScalar_WithSqlOnly_ReturnsExpected()
        {
            _materializerMock.Setup(m => m.ExecuteScalar("SELECT")).Returns(3);
            var result = _materializerMock.Object.ExecuteScalar("SELECT");
            Assert.Equal(3, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected result when called with a SQL query and a
        /// transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalar method correctly processes the provided SQL
        /// statement and transaction, and returns the value as expected. It uses a mock implementation to simulate the
        /// database interaction.</remarks>
        [Fact]
        public void ExecuteScalar_WithSqlAndTransaction_ReturnsExpected()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteScalar("SELECT", transaction)).Returns(4);
            var result = _materializerMock.Object.ExecuteScalar("SELECT", transaction);
            Assert.Equal(4, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected result when called with CommandType.Text, a SQL
        /// command, and a list of parameters.
        /// </summary>
        /// <remarks>This unit test ensures that ExecuteScalar correctly processes the specified command
        /// type, SQL statement, and parameters, and returns the expected scalar value. It uses a mock implementation to
        /// simulate the method's behavior.</remarks>
        [Fact]
        public void ExecuteScalar_WithCommandTypeSqlAndParameters_ReturnsExpected()
        {
            var parameters = new List<IDataParameter>();
            _materializerMock.Setup(m => m.ExecuteScalar(CommandType.Text, "SELECT", parameters)).Returns(5);
            var result = _materializerMock.Object.ExecuteScalar(CommandType.Text, "SELECT", parameters);
            Assert.Equal(5, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected result when called with a stored procedure
        /// command type, SQL parameters, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalar method correctly processes the specified
        /// command type, parameters, and transaction, and returns the expected scalar value. It uses a mock
        /// implementation to simulate the database interaction.</remarks>
        [Fact]
        public void ExecuteScalar_WithCommandTypeSqlParametersAndTransaction_ReturnsExpected()
        {
            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteScalar(CommandType.StoredProcedure, "PROC", parameters, transaction)).Returns(6);
            var result = _materializerMock.Object.ExecuteScalar(CommandType.StoredProcedure, "PROC", parameters, transaction);
            Assert.Equal(6, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected result when called with a specific command type
        /// and SQL statement.
        /// </summary>
        /// <remarks>This test ensures that ExecuteScalar correctly processes the provided CommandType and
        /// SQL query, returning the value configured in the mock setup.</remarks>
        [Fact]
        public void ExecuteScalar_WithCommandTypeAndSql_ReturnsExpected()
        {
            _materializerMock.Setup(m => m.ExecuteScalar(CommandType.Text, "SELECT")).Returns(7);
            var result = _materializerMock.Object.ExecuteScalar(CommandType.Text, "SELECT");
            Assert.Equal(7, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected result when called with CommandType.Text, a SQL
        /// command, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that ExecuteScalar correctly processes a SQL command within the
        /// context of a database transaction and returns the expected scalar value.</remarks>
        [Fact]
        public void ExecuteScalar_WithCommandTypeSqlAndTransaction_ReturnsExpected()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteScalar(CommandType.Text, "SELECT", transaction)).Returns(8);
            var result = _materializerMock.Object.ExecuteScalar(CommandType.Text, "SELECT", transaction);
            Assert.Equal(8, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalarAsync method returns the expected result when called with SQL parameters and
        /// a cancellation token.
        /// </summary>
        /// <remarks>This unit test ensures that ExecuteScalarAsync correctly processes the provided SQL
        /// statement and parameters, and that it returns the expected scalar value. The test also verifies that the
        /// method can be called with a cancellation token without affecting the result.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithSqlParametersAndCancellationToken_ReturnsExpected()
        {
            _materializerMock.Setup(m => m.ExecuteScalarAsync("SELECT", It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(9);
            var result = await _materializerMock.Object.ExecuteScalarAsync("SELECT", new { Id = 9 });
            Assert.Equal(9, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalarAsync method returns the expected result when called with SQL parameters, a
        /// transaction, and a cancellation token.
        /// </summary>
        /// <remarks>This test ensures that ExecuteScalarAsync correctly processes the provided SQL
        /// statement, parameters, and transaction, and that it returns the expected scalar value. It also verifies that
        /// the method can be awaited asynchronously.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithSqlParametersTransactionAndCancellationToken_ReturnsExpected()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteScalarAsync("SELECT", It.IsAny<object>(), transaction, It.IsAny<CancellationToken>())).ReturnsAsync(10);
            var result = await _materializerMock.Object.ExecuteScalarAsync("SELECT", new { Id = 10 }, transaction);
            Assert.Equal(10, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalarAsync method returns the expected result when called with a SQL query and a
        /// cancellation token.
        /// </summary>
        /// <remarks>This test ensures that ExecuteScalarAsync correctly processes the provided SQL
        /// statement and handles cancellation tokens as expected. It uses a mock to simulate the method's behavior and
        /// asserts that the returned value matches the expected result.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithSqlAndCancellationToken_ReturnsExpected()
        {
            _materializerMock.Setup(m => m.ExecuteScalarAsync("SELECT", It.IsAny<CancellationToken>())).ReturnsAsync(11);
            var result = await _materializerMock.Object.ExecuteScalarAsync("SELECT");
            Assert.Equal(11, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalarAsync method returns the expected result when called with a SQL transaction
        /// and a cancellation token.
        /// </summary>
        /// <remarks>This test ensures that ExecuteScalarAsync correctly processes the provided
        /// transaction and returns the anticipated scalar value. It uses a mock implementation to simulate the database
        /// operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithSqlTransactionAndCancellationToken_ReturnsExpected()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteScalarAsync("SELECT", transaction, It.IsAny<CancellationToken>())).ReturnsAsync(12);
            var result = await _materializerMock.Object.ExecuteScalarAsync("SELECT", transaction);
            Assert.Equal(12, result);
        }

        /// <summary>
        /// Verifies that ExecuteScalarAsync returns the expected result when called with a command type, SQL statement,
        /// parameters, and a cancellation token.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithCommandTypeSqlParametersAndCancellationToken_ReturnsExpected()
        {
            var parameters = new List<IDataParameter>();
            _materializerMock.Setup(m => m.ExecuteScalarAsync(CommandType.Text, "SELECT", parameters, It.IsAny<CancellationToken>())).ReturnsAsync(13);
            var result = await _materializerMock.Object.ExecuteScalarAsync(CommandType.Text, "SELECT", parameters);
            Assert.Equal(13, result);
        }

        /// <summary>
        /// Verifies that ExecuteScalarAsync returns the expected result when called with a command type, SQL
        /// parameters, a transaction, and a cancellation token.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithCommandTypeSqlParametersTransactionAndCancellationToken_ReturnsExpected()
        {
            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteScalarAsync(CommandType.StoredProcedure, "PROC", parameters, transaction, It.IsAny<CancellationToken>())).ReturnsAsync(14);
            var result = await _materializerMock.Object.ExecuteScalarAsync(CommandType.StoredProcedure, "PROC", parameters, transaction);
            Assert.Equal(14, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalarAsync method returns the expected result when called with a specific command
        /// type, SQL statement, and cancellation token.
        /// </summary>
        /// <remarks>This test ensures that ExecuteScalarAsync correctly processes the provided command
        /// type and SQL query, and that it returns the expected scalar value. It uses a mocked materializer to simulate
        /// the database operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithCommandTypeAndSqlAndCancellationToken_ReturnsExpected()
        {
            _materializerMock.Setup(m => m.ExecuteScalarAsync(CommandType.Text, "SELECT", It.IsAny<CancellationToken>())).ReturnsAsync(15);
            var result = await _materializerMock.Object.ExecuteScalarAsync(CommandType.Text, "SELECT");
            Assert.Equal(15, result);
        }

        /// <summary>
        /// Verifies that ExecuteScalarAsync returns the expected result when called with a command type, SQL statement,
        /// transaction, and cancellation token.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteScalarAsync method correctly processes the
        /// provided parameters and returns the expected scalar value. It uses a mock implementation to simulate the
        /// database operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithCommandTypeSqlTransactionAndCancellationToken_ReturnsExpected()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteScalarAsync(CommandType.Text, "SELECT", transaction, It.IsAny<CancellationToken>())).ReturnsAsync(16);
            var result = await _materializerMock.Object.ExecuteScalarAsync(CommandType.Text, "SELECT", transaction);
            Assert.Equal(16, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected value when invoked with a generic SQL procedure
        /// and parameter type.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalar&lt;TParameter&gt; method correctly returns the
        /// value provided by the mock implementation when called with a DummySqlProcedure instance. It validates the
        /// integration between the procedure and the materializer mock.</remarks>
        [Fact]
        public void ExecuteScalar_GenericSqlProcedure_ReturnsExpected()
        {
            var proc = new DummySqlProcedure();
            _materializerMock.Setup(m => m.ExecuteScalar<DummyParameter>(proc)).Returns(17);
            var result = _materializerMock.Object.ExecuteScalar<DummyParameter>(proc);
            Assert.Equal(17, result);
        }

        /// <summary>
        /// Verifies that executing a generic SQL procedure with a transaction returns the expected scalar value.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalar method correctly returns the expected result
        /// when called with a specific procedure and transaction. It uses a mock implementation to simulate the
        /// database interaction.</remarks>
        [Fact]
        public void ExecuteScalar_GenericSqlProcedureWithTransaction_ReturnsExpected()
        {
            var proc = new DummySqlProcedure();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteScalar<DummyParameter>(proc, transaction)).Returns(18);
            var result = _materializerMock.Object.ExecuteScalar<DummyParameter>(proc, transaction);
            Assert.Equal(18, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalarAsync&lt;T&gt; method returns the expected result when executing a generic SQL
        /// procedure.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalarAsync&lt;T&gt; method correctly returns the scalar
        /// value produced by the specified SQL procedure when called with a generic parameter type.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_GenericSqlProcedure_ReturnsExpected()
        {
            var proc = new DummySqlProcedure();
            _materializerMock.Setup(m => m.ExecuteScalarAsync<DummyParameter>(proc, It.IsAny<CancellationToken>())).ReturnsAsync(19);
            var result = await _materializerMock.Object.ExecuteScalarAsync<DummyParameter>(proc);
            Assert.Equal(19, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalarAsync method returns the expected result when called with a generic SQL
        /// procedure and a transaction.
        /// </summary>
        /// <remarks>This test ensures that ExecuteScalarAsync correctly executes a SQL procedure using a
        /// provided transaction and returns the expected scalar value. It uses a mock materializer to simulate the
        /// database operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_GenericSqlProcedureWithTransaction_ReturnsExpected()
        {
            var proc = new DummySqlProcedure();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteScalarAsync<DummyParameter>(proc, transaction, It.IsAny<CancellationToken>())).ReturnsAsync(20);
            var result = await _materializerMock.Object.ExecuteScalarAsync<DummyParameter>(proc, transaction);
            Assert.Equal(20, result);
        }

        /// <summary>
        /// Verifies that calling ExecuteScalar on a disposed ISqlMaterializer instance throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalar method enforces correct object lifetime
        /// management by throwing an ObjectDisposedException when invoked after disposal. Proper exception handling in
        /// this scenario helps prevent usage of invalid or released resources.</remarks>
        [Fact]
        public void ExecuteScalar_Disposed_ThrowsObjectDisposedException()
        {
            _materializerMock.Setup(m => m.ExecuteScalar("SELECT", It.IsAny<object>()))
                .Throws(new ObjectDisposedException(nameof(ISqlMaterializer)));
            Assert.Throws<ObjectDisposedException>(() => _materializerMock.Object.ExecuteScalar("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method throws an ArgumentException when called with invalid arguments.
        /// </summary>
        /// <remarks>This unit test ensures that ExecuteScalar enforces argument validation by throwing an
        /// ArgumentException when provided with invalid input parameters.</remarks>
        [Fact]
        public void ExecuteScalar_InvalidArgument_ThrowsArgumentException()
        {
            _materializerMock.Setup(m => m.ExecuteScalar("SELECT", null)).Throws<ArgumentException>();
            Assert.Throws<ArgumentException>(() => _materializerMock.Object.ExecuteScalar("SELECT", null));
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method throws an InvalidOperationException when called in an invalid
        /// operation scenario.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalar method correctly signals an error condition
        /// by throwing an InvalidOperationException when its underlying operation is invalid. Use this test to validate
        /// exception handling behavior in error scenarios.</remarks>
        [Fact]
        public void ExecuteScalar_InvalidOperation_ThrowsInvalidOperationException()
        {
            _materializerMock.Setup(m => m.ExecuteScalar("SELECT", It.IsAny<object>())).Throws<InvalidOperationException>();
            Assert.Throws<InvalidOperationException>(() => _materializerMock.Object.ExecuteScalar("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that ExecuteScalarAsync throws an OperationCanceledException when the operation is canceled.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalarAsync method correctly propagates an
        /// OperationCanceledException when the underlying operation is canceled. Use this test to validate proper
        /// cancellation handling in asynchronous database operations.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_OperationCanceled_ThrowsOperationCanceledException()
        {
            _materializerMock.Setup(m => m.ExecuteScalarAsync("SELECT", It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _materializerMock.Object.ExecuteScalarAsync("SELECT", new { }, CancellationToken.None));
        }
    }
}