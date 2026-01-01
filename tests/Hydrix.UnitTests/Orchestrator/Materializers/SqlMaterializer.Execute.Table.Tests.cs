using Hydrix.Orchestrator.Materializers;
using Hydrix.Orchestrator.Materializers.Contract;
using Hydrix.Schemas;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for verifying the behavior of SQL materializer methods that execute table database commands
    /// synchronously and asynchronously.
    /// </summary>
    /// <remarks>
    /// These tests cover various overloads of the ExecuteTable and ExecuteTableAsync methods,
    /// including scenarios with different command types, SQL statements, parameters, and transactions.
    /// The tests ensure that the materializer returns the expected DataTable and correctly handles cancellation and exceptions.
    /// </remarks>
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Verifies that ExecuteTable returns a DataTable with expected rows for SQL and parameters.
        /// </summary>
        [Fact]
        public void ExecuteTable_WithSqlAndParameters_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = (materializer as ISqlMaterializer).ExecuteTable("SELECT * FROM Test", new { });

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
            Assert.Equal("Alice", result.Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteTable returns a DataTable for SQL, parameters, and transaction.
        /// </summary>
        [Fact]
        public void ExecuteTable_WithSqlParametersAndTransaction_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = (materializer as ISqlMaterializer).ExecuteTable("SELECT * FROM Test", new { }, transaction);

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTable returns a DataTable for SQL only.
        /// </summary>
        [Fact]
        public void ExecuteTable_WithSqlOnly_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = (materializer as ISqlMaterializer).ExecuteTable("SELECT * FROM Test");

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTable returns a DataTable for SQL and transaction.
        /// </summary>
        [Fact]
        public void ExecuteTable_WithSqlAndTransactionOnly_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = (materializer as ISqlMaterializer).ExecuteTable("SELECT * FROM Test", transaction);

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTable returns a DataTable for CommandType, SQL, and parameters.
        /// </summary>
        [Fact]
        public void ExecuteTable_WithCommandTypeSqlAndParameters_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var result = (materializer as ISqlMaterializer).ExecuteTable(CommandType.Text, "SELECT * FROM Test", parameters);

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTable returns a DataTable for CommandType, SQL, parameters, and transaction.
        /// </summary>
        [Fact]
        public void ExecuteTable_WithCommandTypeSqlParametersAndTransaction_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            var result = (materializer as ISqlMaterializer).ExecuteTable(CommandType.Text, "SELECT * FROM Test", parameters, transaction);

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTable returns a DataTable for CommandType and SQL only.
        /// </summary>
        [Fact]
        public void ExecuteTable_WithCommandTypeAndSqlOnly_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = (materializer as ISqlMaterializer).ExecuteTable(CommandType.Text, "SELECT * FROM Test");

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTable returns a DataTable for CommandType, SQL, and transaction.
        /// </summary>
        [Fact]
        public void ExecuteTable_WithCommandTypeSqlAndTransactionOnly_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = (materializer as ISqlMaterializer).ExecuteTable(CommandType.Text, "SELECT * FROM Test", transaction);

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTableAsync returns a DataTable for SQL, parameters, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteTableAsync_WithSqlParameters_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await (materializer as ISqlMaterializer).ExecuteTableAsync("SELECT * FROM Test", new { }, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTableAsync returns a DataTable for SQL, parameters, transaction, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteTableAsync_WithSqlParametersAndTransaction_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = await (materializer as ISqlMaterializer).ExecuteTableAsync("SELECT * FROM Test", new { }, transaction, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTableAsync returns a DataTable for SQL and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteTableAsync_WithSqlOnly_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await (materializer as ISqlMaterializer).ExecuteTableAsync("SELECT * FROM Test", CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTableAsync returns a DataTable for SQL, transaction, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteTableAsync_WithSqlAndTransactionOnly_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = await (materializer as ISqlMaterializer).ExecuteTableAsync("SELECT * FROM Test", transaction, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTableAsync returns a DataTable for CommandType, SQL, parameters, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteTableAsync_WithCommandTypeSqlParameters_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var result = await (materializer as ISqlMaterializer).ExecuteTableAsync(CommandType.Text, "SELECT * FROM Test", parameters, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTableAsync returns a DataTable for CommandType, SQL, parameters, transaction, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteTableAsync_WithCommandTypeSqlParametersAndTransaction_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            var result = await (materializer as ISqlMaterializer).ExecuteTableAsync(CommandType.Text, "SELECT * FROM Test", parameters, transaction, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTableAsync returns a DataTable for CommandType, SQL, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteTableAsync_WithCommandTypeAndSqlOnly_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await (materializer as ISqlMaterializer).ExecuteTableAsync(CommandType.Text, "SELECT * FROM Test", CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTableAsync returns a DataTable for CommandType, SQL, transaction, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteTableAsync_WithCommandTypeSqlAndTransactionOnly_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = await (materializer as ISqlMaterializer).ExecuteTableAsync(CommandType.Text, "SELECT * FROM Test", transaction, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTable returns a DataTable for a SQL procedure.
        /// </summary>
        [Fact]
        public void ExecuteTable_WithSqlProcedure_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = (materializer as ISqlMaterializer).ExecuteTable<FakeDataParameter>(new TestSqlProcedure());

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTable returns a DataTable for a SQL procedure and transaction.
        /// </summary>
        [Fact]
        public void ExecuteTable_WithSqlProcedureAndTransaction_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = (materializer as ISqlMaterializer).ExecuteTable<FakeDataParameter>(new TestSqlProcedure(), new FakeDbTransaction());

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTableAsync returns a DataTable for a SQL procedure and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteTableAsync_WithSqlProcedure_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await (materializer as ISqlMaterializer).ExecuteTableAsync<FakeDataParameter>(new TestSqlProcedure(), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTableAsync returns a DataTable for a SQL procedure, transaction, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteTableAsync_WithSqlProcedureAndTransaction_ReturnsDataTable()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await (materializer as ISqlMaterializer).ExecuteTableAsync<FakeDataParameter>(new TestSqlProcedure(), new FakeDbTransaction(), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
        }

        /// <summary>
        /// Verifies that ExecuteTableAsync throws an OperationCanceledException when the operation is canceled.
        /// </summary>
        [Fact]
        public async Task ExecuteTableAsync_OperationCanceled_ThrowsOperationCanceledException()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Throws(new TaskCanceledException());
            var materializer = CreateMaterializerWithCommand(commandMock);

            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                (materializer as ISqlMaterializer).ExecuteTableAsync("SELECT * FROM Test", new { }, new CancellationToken(true)));
        }
    }
}