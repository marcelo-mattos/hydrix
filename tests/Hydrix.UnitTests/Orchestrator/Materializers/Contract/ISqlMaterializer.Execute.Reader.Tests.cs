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
    /// asynchronous execution of SQL queries and procedures, parameter handling, transaction support, and exception
    /// scenarios for ExecuteReader methods.
    /// </summary>
    /// <remarks>
    /// These tests use mock implementations to simulate database operations and validate that the
    /// ISqlMaterializer interface methods return the expected results or throw appropriate exceptions under various
    /// conditions. The tests cover a range of scenarios, including command execution with different parameter and
    /// transaction combinations, as well as error and cancellation handling for both synchronous and asynchronous
    /// methods.
    /// </remarks>
    public partial class ISqlMaterializerTests
    {
        /// <summary>
        /// Verifies that ExecuteReader returns a data reader when called with SQL and parameters.
        /// </summary>
        [Fact]
        public void ExecuteReader_WithSqlAndParameters_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            _materializerMock.Setup(m => m.ExecuteReader("SELECT", It.IsAny<object>())).Returns(reader);
            var result = _materializerMock.Object.ExecuteReader("SELECT", new { Id = 1 });
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReader returns a data reader when called with SQL, parameters, and a transaction.
        /// </summary>
        [Fact]
        public void ExecuteReader_WithSqlParametersAndTransaction_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteReader("SELECT", It.IsAny<object>(), transaction)).Returns(reader);
            var result = _materializerMock.Object.ExecuteReader("SELECT", new { Id = 2 }, transaction);
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReader returns a data reader when called with only SQL.
        /// </summary>
        [Fact]
        public void ExecuteReader_WithSqlOnly_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            _materializerMock.Setup(m => m.ExecuteReader("SELECT")).Returns(reader);
            var result = _materializerMock.Object.ExecuteReader("SELECT");
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReader returns a data reader when called with SQL and a transaction.
        /// </summary>
        [Fact]
        public void ExecuteReader_WithSqlAndTransaction_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteReader("SELECT", transaction)).Returns(reader);
            var result = _materializerMock.Object.ExecuteReader("SELECT", transaction);
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReader returns a data reader when called with CommandType, SQL, and parameters.
        /// </summary>
        [Fact]
        public void ExecuteReader_WithCommandTypeSqlAndParameters_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            var parameters = new List<IDataParameter>();
            _materializerMock.Setup(m => m.ExecuteReader(CommandType.Text, "SELECT", parameters)).Returns(reader);
            var result = _materializerMock.Object.ExecuteReader(CommandType.Text, "SELECT", parameters);
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReader returns a data reader when called with CommandType, SQL, parameters, and transaction.
        /// </summary>
        [Fact]
        public void ExecuteReader_WithCommandTypeSqlParametersAndTransaction_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteReader(CommandType.Text, "SELECT", parameters, transaction)).Returns(reader);
            var result = _materializerMock.Object.ExecuteReader(CommandType.Text, "SELECT", parameters, transaction);
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReader returns a data reader when called with CommandType and SQL.
        /// </summary>
        [Fact]
        public void ExecuteReader_WithCommandTypeAndSql_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            _materializerMock.Setup(m => m.ExecuteReader(CommandType.Text, "SELECT")).Returns(reader);
            var result = _materializerMock.Object.ExecuteReader(CommandType.Text, "SELECT");
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReader returns a data reader when called with CommandType, SQL, and transaction.
        /// </summary>
        [Fact]
        public void ExecuteReader_WithCommandTypeSqlAndTransaction_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteReader(CommandType.Text, "SELECT", transaction)).Returns(reader);
            var result = _materializerMock.Object.ExecuteReader(CommandType.Text, "SELECT", transaction);
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync returns a data reader when called with SQL, parameters, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteReaderAsync_WithSqlParameters_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            _materializerMock.Setup(m => m.ExecuteReaderAsync("SELECT", It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(reader);
            var result = await _materializerMock.Object.ExecuteReaderAsync("SELECT", new { Id = 1 });
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync returns a data reader when called with SQL, parameters, transaction, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteReaderAsync_WithSqlParametersAndTransaction_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteReaderAsync("SELECT", It.IsAny<object>(), transaction, It.IsAny<CancellationToken>())).ReturnsAsync(reader);
            var result = await _materializerMock.Object.ExecuteReaderAsync("SELECT", new { Id = 2 }, transaction);
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync returns a data reader when called with only SQL and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteReaderAsync_WithSqlOnly_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            _materializerMock.Setup(m => m.ExecuteReaderAsync("SELECT", It.IsAny<CancellationToken>())).ReturnsAsync(reader);
            var result = await _materializerMock.Object.ExecuteReaderAsync("SELECT");
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync returns a data reader when called with SQL, transaction, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteReaderAsync_WithSqlAndTransaction_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteReaderAsync("SELECT", transaction, It.IsAny<CancellationToken>())).ReturnsAsync(reader);
            var result = await _materializerMock.Object.ExecuteReaderAsync("SELECT", transaction);
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync returns a data reader when called with CommandType, SQL, parameters, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteReaderAsync_WithCommandTypeSqlParameters_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            var parameters = new List<IDataParameter>();
            _materializerMock.Setup(m => m.ExecuteReaderAsync(CommandType.Text, "SELECT", parameters, It.IsAny<CancellationToken>())).ReturnsAsync(reader);
            var result = await _materializerMock.Object.ExecuteReaderAsync(CommandType.Text, "SELECT", parameters);
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync returns a data reader when called with CommandType, SQL, parameters, transaction, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteReaderAsync_WithCommandTypeSqlParametersAndTransaction_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteReaderAsync(CommandType.Text, "SELECT", parameters, transaction, It.IsAny<CancellationToken>())).ReturnsAsync(reader);
            var result = await _materializerMock.Object.ExecuteReaderAsync(CommandType.Text, "SELECT", parameters, transaction);
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync returns a data reader when called with CommandType, SQL, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteReaderAsync_WithCommandTypeAndSql_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            _materializerMock.Setup(m => m.ExecuteReaderAsync(CommandType.Text, "SELECT", It.IsAny<CancellationToken>())).ReturnsAsync(reader);
            var result = await _materializerMock.Object.ExecuteReaderAsync(CommandType.Text, "SELECT");
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync returns a data reader when called with CommandType, SQL, transaction, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteReaderAsync_WithCommandTypeSqlAndTransaction_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteReaderAsync(CommandType.Text, "SELECT", transaction, It.IsAny<CancellationToken>())).ReturnsAsync(reader);
            var result = await _materializerMock.Object.ExecuteReaderAsync(CommandType.Text, "SELECT", transaction);
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReader returns a data reader when called with a generic SQL procedure.
        /// </summary>
        [Fact]
        public void ExecuteReader_GenericSqlProcedure_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            var proc = new DummySqlProcedure();
            _materializerMock.Setup(m => m.ExecuteReader(proc)).Returns(reader);
            var result = _materializerMock.Object.ExecuteReader(proc);
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReader returns a data reader when called with a generic SQL procedure and transaction.
        /// </summary>
        [Fact]
        public void ExecuteReader_GenericSqlProcedureWithTransaction_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            var proc = new DummySqlProcedure();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteReader(proc, transaction)).Returns(reader);
            var result = _materializerMock.Object.ExecuteReader(proc, transaction);
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync returns a data reader when called with a generic SQL procedure.
        /// </summary>
        [Fact]
        public async Task ExecuteReaderAsync_GenericSqlProcedure_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            var proc = new DummySqlProcedure();
            _materializerMock.Setup(m => m.ExecuteReaderAsync(proc, It.IsAny<CancellationToken>())).ReturnsAsync(reader);
            var result = await _materializerMock.Object.ExecuteReaderAsync(proc);
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync returns a data reader when called with a generic SQL procedure and transaction.
        /// </summary>
        [Fact]
        public async Task ExecuteReaderAsync_GenericSqlProcedureWithTransaction_ReturnsDataReader()
        {
            var reader = new Mock<IDataReader>().Object;
            var proc = new DummySqlProcedure();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteReaderAsync(proc, transaction, It.IsAny<CancellationToken>())).ReturnsAsync(reader);
            var result = await _materializerMock.Object.ExecuteReaderAsync(proc, transaction);
            Assert.Same(reader, result);
        }

        /// <summary>
        /// Verifies that calling ExecuteReader on a disposed materializer throws an ObjectDisposedException.
        /// </summary>
        [Fact]
        public void ExecuteReader_Disposed_ThrowsObjectDisposedException()
        {
            _materializerMock.Setup(m => m.ExecuteReader("SELECT", It.IsAny<object>())).Throws(new ObjectDisposedException(nameof(ISqlMaterializer)));
            Assert.Throws<ObjectDisposedException>(() => _materializerMock.Object.ExecuteReader("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that calling ExecuteReader with an invalid argument throws an ArgumentException.
        /// </summary>
        [Fact]
        public void ExecuteReader_InvalidArgument_ThrowsArgumentException()
        {
            _materializerMock.Setup(m => m.ExecuteReader("SELECT", It.IsAny<object>())).Throws<ArgumentException>();
            Assert.Throws<ArgumentException>(() => _materializerMock.Object.ExecuteReader("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that calling ExecuteReader with an unsupported operation throws a NotSupportedException.
        /// </summary>
        [Fact]
        public void ExecuteReader_NotSupported_ThrowsNotSupportedException()
        {
            _materializerMock.Setup(m => m.ExecuteReader("SELECT", It.IsAny<object>())).Throws<NotSupportedException>();
            Assert.Throws<NotSupportedException>(() => _materializerMock.Object.ExecuteReader("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that calling ExecuteReader with an invalid operation throws an InvalidOperationException.
        /// </summary>
        [Fact]
        public void ExecuteReader_InvalidOperation_ThrowsInvalidOperationException()
        {
            _materializerMock.Setup(m => m.ExecuteReader("SELECT", It.IsAny<object>())).Throws<InvalidOperationException>();
            Assert.Throws<InvalidOperationException>(() => _materializerMock.Object.ExecuteReader("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync throws an OperationCanceledException when the operation is canceled.
        /// </summary>
        [Fact]
        public async Task ExecuteReaderAsync_OperationCanceled_ThrowsOperationCanceledException()
        {
            _materializerMock.Setup(m => m.ExecuteReaderAsync("SELECT", It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _materializerMock.Object.ExecuteReaderAsync("SELECT", new { }, CancellationToken.None));
        }
    }
}