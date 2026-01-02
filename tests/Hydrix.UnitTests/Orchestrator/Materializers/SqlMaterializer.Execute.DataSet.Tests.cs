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
    /// Contains unit tests for verifying the behavior of SQL materializer methods that execute DataSet database commands
    /// synchronously and asynchronously.
    /// </summary>
    /// <remarks>
    /// These tests cover various overloads of the ExecuteDataSet and ExecuteDataSetAsync methods,
    /// including scenarios with different command types, SQL statements, parameters, transactions, and procedures.
    /// The tests ensure that the materializer returns the expected DataSet and correctly handles cancellation and exceptions.
    /// </remarks>
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Verifies that ExecuteDataSet returns a DataSet with expected table for SQL and parameters.
        /// </summary>
        [Fact]
        public void ExecuteDataSet_WithSqlAndParameters_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.ExecuteDataSet("SELECT * FROM Test", new { });

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSet returns a DataSet for SQL, parameters, and transaction.
        /// </summary>
        [Fact]
        public void ExecuteDataSet_WithSqlParametersAndTransaction_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = materializer.ExecuteDataSet("SELECT * FROM Test", new { }, transaction);

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSet returns a DataSet for SQL only.
        /// </summary>
        [Fact]
        public void ExecuteDataSet_WithSqlOnly_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.ExecuteDataSet("SELECT * FROM Test");

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSet returns a DataSet for SQL and transaction.
        /// </summary>
        [Fact]
        public void ExecuteDataSet_WithSqlAndTransactionOnly_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = materializer.ExecuteDataSet("SELECT * FROM Test", transaction);

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSet returns a DataSet for CommandType, SQL, and parameters.
        /// </summary>
        [Fact]
        public void ExecuteDataSet_WithCommandTypeSqlAndParameters_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var result = materializer.ExecuteDataSet(CommandType.Text, "SELECT * FROM Test", parameters);

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSet returns a DataSet for CommandType, SQL, parameters, and transaction.
        /// </summary>
        [Fact]
        public void ExecuteDataSet_WithCommandTypeSqlParametersAndTransaction_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            var result = materializer.ExecuteDataSet(CommandType.Text, "SELECT * FROM Test", parameters, transaction);

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSet returns a DataSet for CommandType and SQL only.
        /// </summary>
        [Fact]
        public void ExecuteDataSet_WithCommandTypeAndSqlOnly_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.ExecuteDataSet(CommandType.Text, "SELECT * FROM Test");

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSet returns a DataSet for CommandType, SQL, and transaction.
        /// </summary>
        [Fact]
        public void ExecuteDataSet_WithCommandTypeSqlAndTransactionOnly_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = materializer.ExecuteDataSet(CommandType.Text, "SELECT * FROM Test", transaction);

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSet returns a DataSet for a SQL procedure.
        /// </summary>
        [Fact]
        public void ExecuteDataSet_WithGenericProcedure_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.ExecuteDataSet<FakeDataParameter>(new TestSqlProcedure());

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSet returns a DataSet for a SQL procedure and transaction.
        /// </summary>
        [Fact]
        public void ExecuteDataSet_WithGenericProcedureAndTransaction_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.ExecuteDataSet<FakeDataParameter>(new TestSqlProcedure(), new FakeDbTransaction());

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSetAsync returns a DataSet for SQL, parameters, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteDataSetAsync_WithSqlParameters_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.ExecuteDataSetAsync("SELECT * FROM Test", new { }, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSetAsync returns a DataSet for SQL, parameters, transaction, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteDataSetAsync_WithSqlParametersAndTransaction_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = await materializer.ExecuteDataSetAsync("SELECT * FROM Test", new { }, transaction, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSetAsync returns a DataSet for SQL and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteDataSetAsync_WithSqlOnly_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.ExecuteDataSetAsync("SELECT * FROM Test", CancellationToken.None);

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSetAsync returns a DataSet for SQL, transaction, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteDataSetAsync_WithSqlAndTransactionOnly_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = await materializer.ExecuteDataSetAsync("SELECT * FROM Test", transaction, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSetAsync returns a DataSet for CommandType, SQL, parameters, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteDataSetAsync_WithCommandTypeSqlParameters_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var result = await materializer.ExecuteDataSetAsync(CommandType.Text, "SELECT * FROM Test", parameters, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSetAsync returns a DataSet for CommandType, SQL, parameters, transaction, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteDataSetAsync_WithCommandTypeSqlParametersAndTransaction_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            var result = await materializer.ExecuteDataSetAsync(CommandType.Text, "SELECT * FROM Test", parameters, transaction, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSetAsync returns a DataSet for CommandType and SQL only.
        /// </summary>
        [Fact]
        public async Task ExecuteDataSetAsync_WithCommandTypeAndSqlOnly_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.ExecuteDataSetAsync(CommandType.Text, "SELECT * FROM Test", CancellationToken.None);

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSetAsync returns a DataSet for CommandType, SQL, transaction, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteDataSetAsync_WithCommandTypeSqlAndTransactionOnly_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = await materializer.ExecuteDataSetAsync(CommandType.Text, "SELECT * FROM Test", transaction, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSetAsync returns a DataSet for a SQL procedure and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteDataSetAsync_WithGenericProcedure_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.ExecuteDataSetAsync<FakeDataParameter>(new TestSqlProcedure(), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that ExecuteDataSetAsync returns a DataSet for a SQL procedure, transaction, and cancellation token.
        /// </summary>
        [Fact]
        public async Task ExecuteDataSetAsync_WithGenericProcedureAndTransaction_ReturnsDataSet()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.ExecuteDataSetAsync<FakeDataParameter>(new TestSqlProcedure(), new FakeDbTransaction(), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Single(result.Tables);
            Assert.Equal(2, result.Tables[0].Rows.Count);
            Assert.Equal("Alice", result.Tables[0].Rows[0]["Name"]);
        }
    }
}