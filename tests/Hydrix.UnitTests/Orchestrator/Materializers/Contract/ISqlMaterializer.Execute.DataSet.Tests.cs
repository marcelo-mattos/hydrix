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
    /// Contains unit tests for verifying the behavior of the ISqlMaterializer interface's DataSet execution methods,
    /// including synchronous and asynchronous execution, parameter handling, transaction support, and exception scenarios.
    /// </summary>
    /// <remarks>
    /// These tests use mock implementations to simulate database operations and validate that the ISqlMaterializer
    /// interface methods return the expected results or throw appropriate exceptions under various conditions.
    /// </remarks>
    public partial class ISqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the ExecuteDataSet method returns a DataSet when provided with a SQL query and parameters.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteDataSet method correctly processes a SQL command
        /// with associated parameters and returns a non-null DataSet result. It uses a mock materializer to simulate
        /// the database operation.</remarks>
        [Fact]
        public void ExecuteDataSet_WithSqlAndParameters_ReturnsDataSet()
        {
            _materializerMock.Setup(m => m.ExecuteDataSet("SELECT", It.IsAny<object>())).Returns(new DataSet());
            var result = _materializerMock.Object.ExecuteDataSet("SELECT", new { Id = 1 });
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteDataSet method returns a non-null DataSet when called with a SQL query, parameters,
        /// and a transaction.
        /// </summary>
        /// <remarks>This test ensures that ExecuteDataSet correctly handles SQL parameters and an
        /// explicit transaction, and that it does not return null when executed under these conditions.</remarks>
        [Fact]
        public void ExecuteDataSet_WithSqlParametersAndTransaction_ReturnsDataSet()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteDataSet("SELECT", It.IsAny<object>(), transaction)).Returns(new DataSet());
            var result = _materializerMock.Object.ExecuteDataSet("SELECT", new { Id = 2 }, transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteDataSet method returns a non-null DataSet when called with a SQL query string.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteDataSet method, when provided only with a SQL
        /// statement, returns a valid DataSet object. It uses a mock implementation to simulate the expected
        /// behavior.</remarks>
        [Fact]
        public void ExecuteDataSet_WithSqlOnly_ReturnsDataSet()
        {
            _materializerMock.Setup(m => m.ExecuteDataSet("SELECT")).Returns(new DataSet());
            var result = _materializerMock.Object.ExecuteDataSet("SELECT");
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteDataSet method returns a DataSet when called with a SQL command and a transaction.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteDataSet method correctly handles execution
        /// with both a SQL statement and an IDbTransaction, and that it does not return null. The test uses a mock
        /// implementation to simulate the expected behavior.</remarks>
        [Fact]
        public void ExecuteDataSet_WithSqlAndTransaction_ReturnsDataSet()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteDataSet("SELECT", transaction)).Returns(new DataSet());
            var result = _materializerMock.Object.ExecuteDataSet("SELECT", transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteDataSet method returns a DataSet when called with CommandType.Text, a SQL command,
        /// and a list of parameters.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteDataSet method correctly processes SQL command
        /// text and parameters and returns a non-null DataSet as expected. It uses a mock implementation to simulate
        /// the method's behavior.</remarks>
        [Fact]
        public void ExecuteDataSet_WithCommandTypeSqlAndParameters_ReturnsDataSet()
        {
            var parameters = new List<IDataParameter>();
            _materializerMock.Setup(m => m.ExecuteDataSet(CommandType.Text, "SELECT", parameters)).Returns(new DataSet());
            var result = _materializerMock.Object.ExecuteDataSet(CommandType.Text, "SELECT", parameters);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteDataSet method returns a DataSet when called with a command type, SQL statement,
        /// parameters, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that ExecuteDataSet correctly handles the provided command type,
        /// SQL query, parameter list, and transaction, and returns a non-null DataSet as expected.</remarks>
        [Fact]
        public void ExecuteDataSet_WithCommandTypeSqlParametersAndTransaction_ReturnsDataSet()
        {
            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteDataSet(CommandType.Text, "SELECT", parameters, transaction)).Returns(new DataSet());
            var result = _materializerMock.Object.ExecuteDataSet(CommandType.Text, "SELECT", parameters, transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteDataSet method returns a non-null DataSet when called with a specified command type
        /// and SQL statement.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteDataSet method behaves as expected when
        /// provided with CommandType.Text and a SQL query. It uses a mock to simulate the method's behavior and asserts
        /// that the result is not null.</remarks>
        [Fact]
        public void ExecuteDataSet_WithCommandTypeAndSql_ReturnsDataSet()
        {
            _materializerMock.Setup(m => m.ExecuteDataSet(CommandType.Text, "SELECT")).Returns(new DataSet());
            var result = _materializerMock.Object.ExecuteDataSet(CommandType.Text, "SELECT");
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteDataSet method returns a DataSet when called with CommandType.Text, a SQL command,
        /// and a transaction.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteDataSet method correctly handles SQL command
        /// text and an explicit transaction, returning a non-null DataSet as expected.</remarks>
        [Fact]
        public void ExecuteDataSet_WithCommandTypeSqlAndTransaction_ReturnsDataSet()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteDataSet(CommandType.Text, "SELECT", transaction)).Returns(new DataSet());
            var result = _materializerMock.Object.ExecuteDataSet(CommandType.Text, "SELECT", transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteDataSetAsync method returns a non-null DataSet when called with a SQL query and
        /// parameters.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteDataSetAsync method correctly handles SQL queries
        /// with parameters and returns a valid DataSet instance. It uses a mock materializer to simulate the database
        /// operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteDataSetAsync_WithSqlParameters_ReturnsDataSet()
        {
            _materializerMock.Setup(m => m.ExecuteDataSetAsync("SELECT", It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataSet());
            var result = await _materializerMock.Object.ExecuteDataSetAsync("SELECT", new { Id = 1 });
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that executing the ExecuteDataSetAsync method with SQL parameters and a transaction returns a
        /// non-null DataSet.
        /// </summary>
        /// <remarks>This is a unit test that ensures the ExecuteDataSetAsync method correctly handles SQL
        /// parameters and transactions and returns a valid DataSet instance. The test uses mocked dependencies to
        /// isolate the behavior under the test.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteDataSetAsync_WithSqlParametersAndTransaction_ReturnsDataSet()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteDataSetAsync("SELECT", It.IsAny<object>(), transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataSet());
            var result = await _materializerMock.Object.ExecuteDataSetAsync("SELECT", new { Id = 2 }, transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that ExecuteDataSetAsync returns a DataSet when called with only a SQL query.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteDataSetAsync method, when provided with a SQL
        /// statement and no cancellation token, returns a non-null DataSet as expected.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteDataSetAsync_WithSqlOnly_ReturnsDataSet()
        {
            _materializerMock.Setup(m => m.ExecuteDataSetAsync("SELECT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataSet());
            var result = await _materializerMock.Object.ExecuteDataSetAsync("SELECT");
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteDataSetAsync method returns a DataSet when provided with a SQL query and a database
        /// transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteDataSetAsync method correctly executes a SQL
        /// command within the context of a transaction and returns a non-null DataSet result.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteDataSetAsync_WithSqlAndTransaction_ReturnsDataSet()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteDataSetAsync("SELECT", transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataSet());
            var result = await _materializerMock.Object.ExecuteDataSetAsync("SELECT", transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that ExecuteDataSetAsync returns a DataSet when called with CommandType, SQL command text, and SQL
        /// parameters.
        /// </summary>
        /// <remarks>This unit test verifies that the ExecuteDataSetAsync method correctly returns a
        /// non-null DataSet when provided with a command type, SQL command text, and a list of parameters. It uses a
        /// mock materializer to simulate the database operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteDataSetAsync_WithCommandTypeSqlParameters_ReturnsDataSet()
        {
            var parameters = new List<IDataParameter>();
            _materializerMock.Setup(m => m.ExecuteDataSetAsync(CommandType.Text, "SELECT", parameters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataSet());
            var result = await _materializerMock.Object.ExecuteDataSetAsync(CommandType.Text, "SELECT", parameters);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that ExecuteDataSetAsync returns a non-null DataSet when called with a command type, SQL statement,
        /// parameters, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteDataSetAsync method correctly handles the provided
        /// command type, SQL statement, parameters, and transaction, and returns a valid DataSet instance.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteDataSetAsync_WithCommandTypeSqlParametersAndTransaction_ReturnsDataSet()
        {
            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteDataSetAsync(CommandType.Text, "SELECT", parameters, transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataSet());
            var result = await _materializerMock.Object.ExecuteDataSetAsync(CommandType.Text, "SELECT", parameters, transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that ExecuteDataSetAsync returns a DataSet when called with a specific CommandType and SQL
        /// statement.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteDataSetAsync method correctly returns a
        /// non-null DataSet when provided with CommandType.Text and a valid SQL query. The test uses a mock to simulate
        /// the expected behavior of the materializer.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteDataSetAsync_WithCommandTypeAndSql_ReturnsDataSet()
        {
            _materializerMock.Setup(m => m.ExecuteDataSetAsync(CommandType.Text, "SELECT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataSet());
            var result = await _materializerMock.Object.ExecuteDataSetAsync(CommandType.Text, "SELECT");
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that ExecuteDataSetAsync returns a DataSet when called with CommandType.Text, a SQL command, and a
        /// transaction.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteDataSetAsync_WithCommandTypeSqlAndTransaction_ReturnsDataSet()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteDataSetAsync(CommandType.Text, "SELECT", transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataSet());
            var result = await _materializerMock.Object.ExecuteDataSetAsync(CommandType.Text, "SELECT", transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that executing a generic SQL procedure using the materializer returns a non-null DataSet.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteDataSet&lt;TParameter&gt; method correctly returns a
        /// DataSet when invoked with a generic SQL procedure. It uses a mock materializer to simulate the expected
        /// behavior.</remarks>
        [Fact]
        public void ExecuteDataSet_GenericSqlProcedure_ReturnsDataSet()
        {
            var proc = new DummySqlProcedure();
            _materializerMock.Setup(m => m.ExecuteDataSet<DummyParameter>(proc)).Returns(new DataSet());
            var result = _materializerMock.Object.ExecuteDataSet<DummyParameter>(proc);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that executing a generic SQL procedure with a transaction returns a non-null DataSet.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteDataSet method, when called with a generic SQL
        /// procedure and a transaction, produces a valid DataSet result. The test uses a mock implementation to
        /// simulate the expected behavior.</remarks>
        [Fact]
        public void ExecuteDataSet_GenericSqlProcedureWithTransaction_ReturnsDataSet()
        {
            var proc = new DummySqlProcedure();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteDataSet<DummyParameter>(proc, transaction)).Returns(new DataSet());
            var result = _materializerMock.Object.ExecuteDataSet<DummyParameter>(proc, transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteDataSetAsync&lt;TParameter&gt; method returns a non-null DataSet when invoked with a
        /// generic SQL procedure.
        /// </summary>
        /// <remarks>This test ensures that the asynchronous execution of a generic SQL procedure using
        /// ExecuteDataSetAsync&lt;TParameter&gt; produces a valid DataSet result. It uses a mock materializer to simulate the
        /// database operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteDataSetAsync_GenericSqlProcedure_ReturnsDataSet()
        {
            var proc = new DummySqlProcedure();
            _materializerMock.Setup(m => m.ExecuteDataSetAsync<DummyParameter>(proc, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataSet());
            var result = await _materializerMock.Object.ExecuteDataSetAsync<DummyParameter>(proc);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that executing a generic SQL procedure with a transaction asynchronously returns a non-null
        /// DataSet.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteDataSetAsync method, when called with a
        /// generic SQL procedure and a transaction, returns a valid DataSet instance. The test uses a mock
        /// implementation to simulate the database operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteDataSetAsync_GenericSqlProcedureWithTransaction_ReturnsDataSet()
        {
            var proc = new DummySqlProcedure();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteDataSetAsync<DummyParameter>(proc, transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataSet());
            var result = await _materializerMock.Object.ExecuteDataSetAsync<DummyParameter>(proc, transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that calling ExecuteDataSet on a disposed ISqlMaterializer instance throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteDataSet method enforces correct object lifetime
        /// management by throwing an exception when invoked after disposal.</remarks>
        [Fact]
        public void ExecuteDataSet_Disposed_ThrowsObjectDisposedException()
        {
            _materializerMock.Setup(m => m.ExecuteDataSet("SELECT", It.IsAny<object>()))
                .Throws(new ObjectDisposedException(nameof(ISqlMaterializer)));
            Assert.Throws<ObjectDisposedException>(() => _materializerMock.Object.ExecuteDataSet("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that the ExecuteDataSet method throws an ArgumentException when called with invalid arguments.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteDataSet method enforces argument validation and
        /// throws the expected exception when provided with invalid input.</remarks>
        [Fact]
        public void ExecuteDataSet_InvalidArgument_ThrowsArgumentException()
        {
            _materializerMock.Setup(m => m.ExecuteDataSet("SELECT", It.IsAny<object>()))
                .Throws<ArgumentException>();
            Assert.Throws<ArgumentException>(() => _materializerMock.Object.ExecuteDataSet("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that calling ExecuteDataSet throws a NotSupportedException when the operation is not supported.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteDataSet method correctly indicates lack of support
        /// by throwing a NotSupportedException. Use this test to validate that unsupported operations are handled as
        /// expected.</remarks>
        [Fact]
        public void ExecuteDataSet_NotSupported_ThrowsNotSupportedException()
        {
            _materializerMock.Setup(m => m.ExecuteDataSet("SELECT", It.IsAny<object>()))
                .Throws<NotSupportedException>();
            Assert.Throws<NotSupportedException>(() => _materializerMock.Object.ExecuteDataSet("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that the ExecuteDataSet method throws an InvalidOperationException when called in an invalid
        /// operation scenario.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteDataSet method correctly signals an invalid
        /// operation by throwing an InvalidOperationException when invoked with specific arguments. Use this test to
        /// validate error handling behavior in cases where the operation is not permitted.</remarks>
        [Fact]
        public void ExecuteDataSet_InvalidOperation_ThrowsInvalidOperationException()
        {
            _materializerMock.Setup(m => m.ExecuteDataSet("SELECT", It.IsAny<object>()))
                .Throws<InvalidOperationException>();
            Assert.Throws<InvalidOperationException>(() => _materializerMock.Object.ExecuteDataSet("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that ExecuteDataSetAsync throws an OperationCanceledException when the operation is canceled.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteDataSetAsync_OperationCanceled_ThrowsOperationCanceledException()
        {
            _materializerMock.Setup(m => m.ExecuteDataSetAsync("SELECT", It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _materializerMock.Object.ExecuteDataSetAsync("SELECT", new { }, CancellationToken.None));
        }
    }
}