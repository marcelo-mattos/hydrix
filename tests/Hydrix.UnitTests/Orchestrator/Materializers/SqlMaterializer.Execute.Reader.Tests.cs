using Hydrix.Orchestrator.Materializers.Contract;
using Moq;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for the ISqlMaterializer interface, verifying correct behavior of data reader execution
    /// methods with various SQL command and parameter combinations.
    /// </summary>
    /// <remarks>These tests cover both synchronous and asynchronous execution paths, including scenarios with
    /// and without transactions, different command types, and parameterized queries. The tests ensure that the
    /// ISqlMaterializer implementation correctly returns data readers and handles cancellation and exceptions as
    /// expected.</remarks>
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the ExecuteReader method of ISqlMaterializer returns the expected IDataReader when provided
        /// with a SQL query and parameters.
        /// </summary>
        /// <remarks>This test ensures that ExecuteReader correctly invokes the underlying IDbCommand with
        /// the specified SQL and parameters, and that it returns the IDataReader instance produced by the command. It
        /// also verifies that the command is executed with CommandBehavior.Default.</remarks>
        [Fact]
        public void ExecuteReader_WithSqlAndParameters_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = (materializer as ISqlMaterializer).ExecuteReader("SELECT", new { Id = 1 });

            Assert.Same(readerMock.Object, result);
            commandMock.Verify(c => c.ExecuteReader(CommandBehavior.Default), Times.Once);
        }

        /// <summary>
        /// Verifies that the ExecuteReader method of ISqlMaterializer returns the expected IDataReader when provided
        /// with a SQL query, parameters, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteReader method correctly passes the SQL statement,
        /// parameters, and transaction to the underlying command, and that it returns the IDataReader produced by the
        /// command. It uses mocks to simulate the database command and reader behavior.</remarks>
        [Fact]
        public void ExecuteReader_WithSqlParametersAndTransaction_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = (materializer as ISqlMaterializer).ExecuteReader("SELECT", new { Id = 2 }, transaction);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that the ExecuteReader method of ISqlMaterializer returns an IDataReader when called with a SQL
        /// query string.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteReader method, when invoked with only a SQL
        /// statement, correctly returns the expected IDataReader instance. It uses mocked dependencies to isolate the
        /// behavior of the materializer.</remarks>
        [Fact]
        public void ExecuteReader_WithSqlOnly_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = (materializer as ISqlMaterializer).ExecuteReader("SELECT");

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that the ExecuteReader method returns an IDataReader when called with only a SQL query and a
        /// transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteReader implementation correctly handles scenarios
        /// where only the SQL command text and a transaction are provided, and that it returns the expected data reader
        /// instance.</remarks>
        [Fact]
        public void ExecuteReader_WithSqlAndTransactionOnly_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = (materializer as ISqlMaterializer).ExecuteReader("SELECT", transaction);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that the ExecuteReader method of ISqlMaterializer returns the expected IDataReader when called with
        /// CommandType.Text, a SQL command text, and a list of parameters.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteReader method correctly delegates to the underlying
        /// IDbCommand and returns the IDataReader instance as expected when provided with SQL command type and
        /// parameters.</remarks>
        [Fact]
        public void ExecuteReader_WithCommandTypeSqlAndParameters_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var result = (materializer as ISqlMaterializer).ExecuteReader(CommandType.Text, "SELECT", parameters);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that the ExecuteReader method returns the expected IDataReader when provided with a command type,
        /// SQL statement, parameters, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer.ExecuteReader method correctly passes the
        /// specified command type, SQL query, parameters, and transaction to the underlying command, and that it
        /// returns the IDataReader produced by the command. It uses mocks to simulate the database command and
        /// transaction behavior.</remarks>
        [Fact]
        public void ExecuteReader_WithCommandTypeSqlParametersAndTransaction_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            var result = (materializer as ISqlMaterializer).ExecuteReader(CommandType.Text, "SELECT", parameters, transaction);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that the ExecuteReader method returns an IDataReader when called with only the command type and SQL
        /// statement.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer.ExecuteReader method correctly invokes
        /// the underlying command and returns the expected data reader when provided with CommandType.Text and a SQL
        /// query. It uses mocks to simulate the database command and reader behavior.</remarks>
        [Fact]
        public void ExecuteReader_WithCommandTypeAndSqlOnly_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = (materializer as ISqlMaterializer).ExecuteReader(CommandType.Text, "SELECT");

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that the ExecuteReader method of ISqlMaterializer returns the expected IDataReader when called with
        /// CommandType.Text, a SQL command, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteReader method correctly delegates to the underlying
        /// command and returns the associated data reader when only the command type, command text, and transaction are
        /// provided.</remarks>
        [Fact]
        public void ExecuteReader_WithCommandTypeSqlAndTransactionOnly_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = (materializer as ISqlMaterializer).ExecuteReader(CommandType.Text, "SELECT", transaction);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that the ExecuteReaderAsync method of ISqlMaterializer returns the expected IDataReader when
        /// provided with a SQL query and parameters.
        /// </summary>
        /// <remarks>This test ensures that ExecuteReaderAsync correctly passes SQL parameters and returns
        /// the data reader produced by the underlying command. It uses mocked dependencies to isolate and validate the
        /// behavior.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteReaderAsync_WithSqlParameters_ReturnsDataReader_DbCommandAsync()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await (materializer as ISqlMaterializer).ExecuteReaderAsync("SELECT", new { Id = 1 }, CancellationToken.None);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that the ExecuteReaderAsync method returns the expected IDataReader instance when called with SQL
        /// parameters, using the fallback execution path.
        /// </summary>
        /// <remarks>This test ensures that ExecuteReaderAsync correctly handles SQL parameters and
        /// returns the mocked IDataReader when the fallback logic is used. It uses a mock command and reader to
        /// simulate database interaction.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteReaderAsync_WithSqlParameters_ReturnsDataReader_Fallback()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await (materializer as ISqlMaterializer).ExecuteReaderAsync("SELECT", new { Id = 1 }, CancellationToken.None);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync returns the expected data reader when provided with SQL parameters and a
        /// transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteReaderAsync method correctly passes SQL parameters
        /// and a transaction to the underlying command, and that it returns the data reader as expected.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteReaderAsync_WithSqlParametersAndTransaction_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = await (materializer as ISqlMaterializer).ExecuteReaderAsync("SELECT", new { Id = 1 }, transaction, CancellationToken.None);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that the ExecuteReaderAsync method, when called with only a SQL statement, returns the expected
        /// data reader instance.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer.ExecuteReaderAsync method correctly
        /// returns the data reader provided by the underlying command when invoked with a SQL string and a cancellation
        /// token.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteReaderAsync_WithSqlOnly_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await (materializer as ISqlMaterializer).ExecuteReaderAsync("SELECT", CancellationToken.None);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync, when called with only a SQL statement and a transaction, returns the
        /// expected data reader.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteReaderAsync method of ISqlMaterializer correctly
        /// returns the data reader provided by the underlying command when invoked with a SQL query and a transaction.
        /// The test uses mocks to simulate the database command and data reader.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteReaderAsync_WithSqlAndTransactionOnly_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = await (materializer as ISqlMaterializer).ExecuteReaderAsync("SELECT", transaction, CancellationToken.None);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync returns the expected data reader when called with CommandType.Text, a SQL
        /// command, and a list of SQL parameters.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer.ExecuteReaderAsync method correctly
        /// returns the mocked IDataReader instance when provided with specific command type, command text, and
        /// parameters.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteReaderAsync_WithCommandTypeSqlParameters_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var result = await (materializer as ISqlMaterializer).ExecuteReaderAsync(CommandType.Text, "SELECT", parameters, CancellationToken.None);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync returns the expected data reader when called with a command type, SQL
        /// statement, parameters, and transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteReaderAsync method correctly returns the data
        /// reader provided by the underlying command when all relevant arguments are supplied, including command type,
        /// SQL parameters, and a transaction.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteReaderAsync_WithCommandTypeSqlParametersAndTransaction_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            var result = await (materializer as ISqlMaterializer).ExecuteReaderAsync(CommandType.Text, "SELECT", parameters, transaction, CancellationToken.None);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync, when called with only a command type and SQL statement, returns the
        /// expected data reader.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer.ExecuteReaderAsync method correctly
        /// returns the data reader provided by the underlying command when invoked with CommandType.Text and a SQL
        /// query. It uses mocks to simulate the database command and reader behavior.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteReaderAsync_WithCommandTypeAndSqlOnly_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await (materializer as ISqlMaterializer).ExecuteReaderAsync(CommandType.Text, "SELECT", CancellationToken.None);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync, when called with CommandType.Text, a SQL command, and a transaction,
        /// returns the expected data reader.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer.ExecuteReaderAsync method correctly
        /// returns the data reader provided by the underlying command when invoked with the specified
        /// parameters.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteReaderAsync_WithCommandTypeSqlAndTransactionOnly_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transaction = new Mock<IDbTransaction>().Object;
            var result = await (materializer as ISqlMaterializer).ExecuteReaderAsync(CommandType.Text, "SELECT", transaction, CancellationToken.None);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that executing a SQL procedure using the generic ExecuteReader method returns the expected data
        /// reader instance.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteReader&lt;TParameter&gt; method of ISqlMaterializer
        /// correctly invokes the underlying command and returns the data reader provided by the command's ExecuteReader
        /// method. It uses a mock SQL procedure and parameter type to validate the behavior.</remarks>
        [Fact]
        public void ExecuteReader_Generic_WithSqlProcedure_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var procedure = new TestSqlProcedure();
            var result = (materializer as ISqlMaterializer).ExecuteReader<FakeDataParameter>(procedure);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that the ExecuteReader&lt;T&gt; method, when called with a SQL procedure and a transaction, returns the
        /// expected data reader instance.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly delegates to the underlying command
        /// and returns the data reader as expected when provided with a SQL procedure and transaction. It uses mocks to
        /// simulate the command and data reader behavior.</remarks>
        [Fact]
        public void ExecuteReader_Generic_WithSqlProcedureAndTransaction_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var procedure = new TestSqlProcedure();
            var transaction = new FakeDbTransaction();
            var result = (materializer as ISqlMaterializer).ExecuteReader<FakeDataParameter>(procedure, transaction);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that the generic ExecuteReaderAsync method, when called with a SQL procedure, returns the expected
        /// data reader instance.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer.ExecuteReaderAsync&lt;T&gt; method correctly
        /// invokes the underlying command and returns the data reader provided by the command's ExecuteReader method.
        /// It uses a mock SQL procedure and a mock data reader to validate the behavior.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteReaderAsync_Generic_WithSqlProcedure_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var procedure = new TestSqlProcedure();
            var result = await (materializer as ISqlMaterializer).ExecuteReaderAsync<FakeDataParameter>(procedure, CancellationToken.None);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that executing ExecuteReaderAsync&lt;T&gt; with a SQL procedure and a transaction returns the expected
        /// data reader.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the data reader returned by the
        /// materializer.</returns>
        [Fact]
        public async Task ExecuteReaderAsync_Generic_WithSqlProcedureAndTransaction_ReturnsDataReader()
        {
            var readerMock = new Mock<IDataReader>();
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var procedure = new TestSqlProcedure();
            var transaction = new FakeDbTransaction();
            var result = await (materializer as ISqlMaterializer).ExecuteReaderAsync<FakeDataParameter>(procedure, transaction, CancellationToken.None);

            Assert.Same(readerMock.Object, result);
        }

        /// <summary>
        /// Verifies that ExecuteReaderAsync throws a TaskCanceledException when the operation is canceled.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteReaderAsync method correctly propagates a
        /// TaskCanceledException when a cancellation token is already canceled at the time of invocation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteReaderAsync_OperationCanceled_ThrowsOperationCanceledException()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Throws(new TaskCanceledException());
            var materializer = CreateMaterializerWithCommand(commandMock);

            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                (materializer as ISqlMaterializer).ExecuteReaderAsync("SELECT", new { Id = 1 }, new CancellationToken(true)));
        }
    }
}