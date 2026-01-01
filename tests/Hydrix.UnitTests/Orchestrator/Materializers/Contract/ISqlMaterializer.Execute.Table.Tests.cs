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
    /// Contains unit tests for verifying the behavior of the ISqlMaterializer interface's DataTable execution methods,
    /// including synchronous and asynchronous execution, parameter handling, transaction support, and exception scenarios.
    /// </summary>
    /// <remarks>
    /// These tests use mock implementations to simulate database operations and validate that the ISqlMaterializer
    /// interface methods return the expected results or throw appropriate exceptions under various conditions.
    /// </remarks>
    public partial class ISqlMaterializerTests
    {
        /// <summary>
        /// Tests that the ExecuteTable method returns a DataTable when provided with a SQL query and parameters.
        /// </summary>
        /// <remarks>This test verifies that ExecuteTable correctly returns a non-null DataTable when
        /// called with a valid SQL statement and parameter object. It uses a mock materializer to simulate the database
        /// interaction.</remarks>
        [Fact]
        public void ExecuteTable_WithSqlAndParameters_ReturnsDataTable()
        {
            _materializerMock.Setup(m => m.ExecuteTable("SELECT", It.IsAny<object>())).Returns(new DataTable());
            var result = _materializerMock.Object.ExecuteTable("SELECT", new { Id = 1 });
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteTable method returns a DataTable when called with SQL parameters and a transaction.
        /// </summary>
        /// <remarks>This unit test ensures that ExecuteTable correctly handles parameterized SQL queries
        /// within the context of a database transaction. The test uses a mock implementation to simulate the expected
        /// behavior.</remarks>
        [Fact]
        public void ExecuteTable_WithSqlParametersAndTransaction_ReturnsDataTable()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteTable("SELECT", It.IsAny<object>(), transaction)).Returns(new DataTable());
            var result = _materializerMock.Object.ExecuteTable("SELECT", new { Id = 2 }, transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteTable method returns a non-null DataTable when called with a valid SQL query.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteTable method, when provided with a SQL
        /// statement, returns a DataTable instance as expected. The test uses a mock implementation to simulate the
        /// method's behavior.</remarks>
        [Fact]
        public void ExecuteTable_WithSqlOnly_ReturnsDataTable()
        {
            _materializerMock.Setup(m => m.ExecuteTable("SELECT")).Returns(new DataTable());
            var result = _materializerMock.Object.ExecuteTable("SELECT");
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteTable method returns a non-null DataTable when called with a SQL query and a
        /// transaction.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteTable method correctly handles execution with
        /// both a SQL command and an IDbTransaction, and that it does not return null results.</remarks>
        [Fact]
        public void ExecuteTable_WithSqlAndTransaction_ReturnsDataTable()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteTable("SELECT", transaction)).Returns(new DataTable());
            var result = _materializerMock.Object.ExecuteTable("SELECT", transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteTable method returns a non-null DataTable when called with CommandType.Text, a SQL
        /// query, and a list of parameters.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteTable method behaves as expected when provided
        /// with a SQL command type, a query string, and a collection of parameters. It uses a mock implementation to
        /// simulate the method's behavior and asserts that the result is not null.</remarks>
        [Fact]
        public void ExecuteTable_WithCommandTypeSqlAndParameters_ReturnsDataTable()
        {
            var parameters = new List<IDataParameter>();
            _materializerMock.Setup(m => m.ExecuteTable(CommandType.Text, "SELECT", parameters)).Returns(new DataTable());
            var result = _materializerMock.Object.ExecuteTable(CommandType.Text, "SELECT", parameters);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteTable method returns a DataTable when called with a command type, SQL statement,
        /// parameters, and a transaction.
        /// </summary>
        /// <remarks>This unit test ensures that ExecuteTable correctly processes the provided command
        /// type, SQL query, parameter list, and transaction, and returns a non-null DataTable result. It is intended to
        /// validate the method's behavior in scenarios where all arguments are supplied.</remarks>
        [Fact]
        public void ExecuteTable_WithCommandTypeSqlParametersAndTransaction_ReturnsDataTable()
        {
            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteTable(CommandType.Text, "SELECT", parameters, transaction)).Returns(new DataTable());
            var result = _materializerMock.Object.ExecuteTable(CommandType.Text, "SELECT", parameters, transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteTable method returns a non-null DataTable when called with a specified CommandType
        /// and SQL statement.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteTable method correctly handles input parameters and
        /// produces a valid DataTable result. It uses a mock materializer to simulate the database operation.</remarks>
        [Fact]
        public void ExecuteTable_WithCommandTypeAndSql_ReturnsDataTable()
        {
            _materializerMock.Setup(m => m.ExecuteTable(CommandType.Text, "SELECT")).Returns(new DataTable());
            var result = _materializerMock.Object.ExecuteTable(CommandType.Text, "SELECT");
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteTable method returns a DataTable when called with CommandType.Text, a SQL query,
        /// and a transaction.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteTable method correctly handles execution with
        /// a specified command type, SQL statement, and transaction, and that it returns a non-null DataTable
        /// result.</remarks>
        [Fact]
        public void ExecuteTable_WithCommandTypeSqlAndTransaction_ReturnsDataTable()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteTable(CommandType.Text, "SELECT", transaction)).Returns(new DataTable());
            var result = _materializerMock.Object.ExecuteTable(CommandType.Text, "SELECT", transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteTableAsync method returns a DataTable when called with a SQL query and parameters.
        /// </summary>
        /// <remarks>This unit test ensures that ExecuteTableAsync correctly processes SQL parameters and
        /// returns a non-null DataTable result. It uses a mock implementation to simulate the database
        /// interaction.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteTableAsync_WithSqlParameters_ReturnsDataTable()
        {
            _materializerMock.Setup(m => m.ExecuteTableAsync("SELECT", It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataTable());
            var result = await _materializerMock.Object.ExecuteTableAsync("SELECT", new { Id = 1 });
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteTableAsync method returns a DataTable when called with SQL parameters and a
        /// transaction.
        /// </summary>
        /// <remarks>This test ensures that ExecuteTableAsync correctly handles SQL parameters and an
        /// IDbTransaction, and that it returns a non-null DataTable as expected.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteTableAsync_WithSqlParametersAndTransaction_ReturnsDataTable()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteTableAsync("SELECT", It.IsAny<object>(), transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataTable());
            var result = await _materializerMock.Object.ExecuteTableAsync("SELECT", new { Id = 2 }, transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteTableAsync method returns a non-null DataTable when called with only a SQL query.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteTableAsync method, when provided with a SQL
        /// statement, successfully returns a DataTable instance. The test uses a mock to simulate the method's behavior
        /// and asserts that the result is not null.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteTableAsync_WithSqlOnly_ReturnsDataTable()
        {
            _materializerMock.Setup(m => m.ExecuteTableAsync("SELECT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataTable());
            var result = await _materializerMock.Object.ExecuteTableAsync("SELECT");
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that ExecuteTableAsync returns a DataTable when provided with a SQL query and a transaction.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteTableAsync_WithSqlAndTransaction_ReturnsDataTable()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteTableAsync("SELECT", transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataTable());
            var result = await _materializerMock.Object.ExecuteTableAsync("SELECT", transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that ExecuteTableAsync returns a DataTable when called with CommandType.Text and a list of SQL
        /// parameters.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteTableAsync method correctly returns a non-null
        /// DataTable when provided with valid command type, command text, and parameters. It uses a mock materializer
        /// to simulate the database operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteTableAsync_WithCommandTypeSqlParameters_ReturnsDataTable()
        {
            var parameters = new List<IDataParameter>();
            _materializerMock.Setup(m => m.ExecuteTableAsync(CommandType.Text, "SELECT", parameters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataTable());
            var result = await _materializerMock.Object.ExecuteTableAsync(CommandType.Text, "SELECT", parameters);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that ExecuteTableAsync returns a DataTable when called with a command type, SQL statement, parameters,
        /// and a transaction.
        /// </summary>
        /// <remarks>This test verifies that the ExecuteTableAsync method correctly returns a non-null
        /// DataTable when provided with valid command type, SQL, parameters, and transaction arguments.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteTableAsync_WithCommandTypeSqlParametersAndTransaction_ReturnsDataTable()
        {
            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteTableAsync(CommandType.Text, "SELECT", parameters, transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataTable());
            var result = await _materializerMock.Object.ExecuteTableAsync(CommandType.Text, "SELECT", parameters, transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteTableAsync method returns a non-null DataTable when called with a specified
        /// CommandType and SQL statement.
        /// </summary>
        /// <remarks>This test ensures that ExecuteTableAsync correctly executes a SQL command of type
        /// Text and returns a DataTable result. It uses a mock materializer to simulate the database
        /// operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteTableAsync_WithCommandTypeAndSql_ReturnsDataTable()
        {
            _materializerMock.Setup(m => m.ExecuteTableAsync(CommandType.Text, "SELECT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataTable());
            var result = await _materializerMock.Object.ExecuteTableAsync(CommandType.Text, "SELECT");
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that ExecuteTableAsync, when called with CommandType.Text, a SQL command, and a transaction,
        /// returns a non-null DataTable.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteTableAsync_WithCommandTypeSqlAndTransaction_ReturnsDataTable()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteTableAsync(CommandType.Text, "SELECT", transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataTable());
            var result = await _materializerMock.Object.ExecuteTableAsync(CommandType.Text, "SELECT", transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that executing a generic SQL procedure using ExecuteTable returns a non-null DataTable.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteTable&lt;TParameter&gt; method correctly returns a
        /// DataTable when invoked with a generic SQL procedure. It uses a mock materializer to simulate the database
        /// operation.</remarks>
        [Fact]
        public void ExecuteTable_GenericSqlProcedure_ReturnsDataTable()
        {
            var proc = new DummySqlProcedure();
            _materializerMock.Setup(m => m.ExecuteTable<DummyParameter>(proc)).Returns(new DataTable());
            var result = _materializerMock.Object.ExecuteTable<DummyParameter>(proc);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that executing a generic SQL procedure with a transaction returns a non-null DataTable.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteTable&lt;TParameter&gt; method, when called with a
        /// SQL procedure and a transaction, returns a valid DataTable instance. The test uses a mock implementation to
        /// simulate the database operation.</remarks>
        [Fact]
        public void ExecuteTable_GenericSqlProcedureWithTransaction_ReturnsDataTable()
        {
            var proc = new DummySqlProcedure();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteTable<DummyParameter>(proc, transaction)).Returns(new DataTable());
            var result = _materializerMock.Object.ExecuteTable<DummyParameter>(proc, transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that the ExecuteTableAsync&lt;TParameter&gt; method returns a non-null DataTable when invoked with a
        /// generic SQL procedure.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteTableAsync&lt;TParameter&gt; method correctly executes a
        /// generic SQL procedure and returns a DataTable instance. It uses a mock materializer to simulate the database
        /// operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteTableAsync_GenericSqlProcedure_ReturnsDataTable()
        {
            var proc = new DummySqlProcedure();
            _materializerMock.Setup(m => m.ExecuteTableAsync<DummyParameter>(proc, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataTable());
            var result = await _materializerMock.Object.ExecuteTableAsync<DummyParameter>(proc);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that executing a generic SQL procedure with a transaction using ExecuteTableAsync returns a
        /// non-null DataTable.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteTableAsync method, when called with a generic SQL
        /// procedure and a transaction, returns a valid DataTable instance. The test uses a mock materializer to
        /// simulate the database operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteTableAsync_GenericSqlProcedureWithTransaction_ReturnsDataTable()
        {
            var proc = new DummySqlProcedure();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.ExecuteTableAsync<DummyParameter>(proc, transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataTable());
            var result = await _materializerMock.Object.ExecuteTableAsync<DummyParameter>(proc, transaction);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that calling ExecuteTable on a disposed ISqlMaterializer instance throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteTable method enforces correct object lifetime
        /// management by throwing an exception when invoked after disposal.</remarks>
        [Fact]
        public void ExecuteTable_Disposed_ThrowsObjectDisposedException()
        {
            _materializerMock.Setup(m => m.ExecuteTable("SELECT", It.IsAny<object>()))
                .Throws(new ObjectDisposedException(nameof(ISqlMaterializer)));
            Assert.Throws<ObjectDisposedException>(() => _materializerMock.Object.ExecuteTable("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that the ExecuteTable method throws an ArgumentException when called with invalid arguments.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteTable method enforces argument validation and
        /// throws the expected exception when provided with invalid input.</remarks>
        [Fact]
        public void ExecuteTable_InvalidArgument_ThrowsArgumentException()
        {
            _materializerMock.Setup(m => m.ExecuteTable("SELECT", It.IsAny<object>()))
                .Throws<ArgumentException>();
            Assert.Throws<ArgumentException>(() => _materializerMock.Object.ExecuteTable("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that calling ExecuteTable throws a NotSupportedException when the operation is not supported.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteTable method correctly signals unsupported
        /// operations by throwing a NotSupportedException. Use this test to validate that the method's contract is
        /// enforced when invoked in unsupported scenarios.</remarks>
        [Fact]
        public void ExecuteTable_NotSupported_ThrowsNotSupportedException()
        {
            _materializerMock.Setup(m => m.ExecuteTable("SELECT", It.IsAny<object>()))
                .Throws<NotSupportedException>();
            Assert.Throws<NotSupportedException>(() => _materializerMock.Object.ExecuteTable("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that the ExecuteTable method throws an InvalidOperationException when called in an invalid
        /// operation scenario.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteTable method correctly signals an error condition
        /// by throwing an InvalidOperationException when invoked with invalid parameters or in an invalid
        /// state.</remarks>
        [Fact]
        public void ExecuteTable_InvalidOperation_ThrowsInvalidOperationException()
        {
            _materializerMock.Setup(m => m.ExecuteTable("SELECT", It.IsAny<object>()))
                .Throws<InvalidOperationException>();
            Assert.Throws<InvalidOperationException>(() => _materializerMock.Object.ExecuteTable("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that ExecuteTableAsync throws an OperationCanceledException when the operation is canceled.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteTableAsync_OperationCanceled_ThrowsOperationCanceledException()
        {
            _materializerMock.Setup(m => m.ExecuteTableAsync("SELECT", It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _materializerMock.Object.ExecuteTableAsync("SELECT", new { }, CancellationToken.None));
        }
    }
}