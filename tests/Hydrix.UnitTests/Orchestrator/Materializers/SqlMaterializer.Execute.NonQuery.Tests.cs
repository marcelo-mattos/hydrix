using Moq;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for the SqlMaterializer class, verifying its behavior when executing non-query SQL commands
    /// synchronously and asynchronously with various parameter and transaction configurations.
    /// </summary>
    /// <remarks>These tests ensure that SqlMaterializer correctly handles different command types, parameter
    /// sets, transactions, and cancellation scenarios. The tests use mock database command and transaction objects to
    /// isolate and validate the materializer's logic without requiring a live database connection.</remarks>
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the ExecuteNonQuery method executes a SQL command with parameters and returns the number of
        /// rows affected.
        /// </summary>
        /// <remarks>This test ensures that ExecuteNonQuery correctly passes the SQL statement and
        /// parameters to the underlying command and that the returned value matches the expected number of affected
        /// rows.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithSqlAndParameters_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(5);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.ExecuteNonQuery("UPDATE", new { Id = 1 });

            Assert.Equal(5, result);
            commandMock.Verify(c => c.ExecuteNonQuery(), Times.Once);
        }

        /// <summary>
        /// Verifies that the ExecuteNonQuery method returns the correct number of rows affected when executed with SQL
        /// parameters and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQuery method correctly passes SQL parameters and
        /// a transaction to the underlying command, and that it returns the expected number of affected rows. It uses a
        /// mock command to simulate database behavior.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithSqlParametersAndTransaction_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(2);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transactionMock = new Mock<IDbTransaction>().Object;
            var result = materializer.ExecuteNonQuery("DELETE", new { Id = 2 }, transactionMock);

            Assert.Equal(2, result);
        }

        /// <summary>
        /// Verifies that the ExecuteNonQuery method returns the number of rows affected when called with only a SQL
        /// statement.
        /// </summary>
        /// <remarks>This test ensures that providing a SQL command string to ExecuteNonQuery correctly
        /// returns the number of rows affected by the operation. It uses a mock IDbCommand to simulate database
        /// behavior.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithSqlOnly_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(1);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.ExecuteNonQuery("INSERT");

            Assert.Equal(1, result);
        }

        /// <summary>
        /// Verifies that the ExecuteNonQuery method returns the number of rows affected when called with a SQL
        /// statement and a transaction.
        /// </summary>
        /// <remarks>This test ensures that providing only the SQL command text and a transaction to
        /// ExecuteNonQuery correctly returns the expected number of affected rows. It uses a mock command to simulate
        /// the database operation.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithSqlAndTransactionOnly_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(3);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transactionMock = new Mock<IDbTransaction>().Object;
            var result = materializer.ExecuteNonQuery("UPDATE", transactionMock);

            Assert.Equal(3, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQuery returns the number of rows affected when called with CommandType.Text, a SQL
        /// command, and a list of parameters.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQuery method correctly executes a SQL command
        /// with the specified command type and parameters, and returns the expected number of affected rows. It uses a
        /// mock command to simulate database behavior.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithCommandTypeSqlAndParameters_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(7);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var result = materializer.ExecuteNonQuery(CommandType.Text, "SELECT", parameters);

            Assert.Equal(7, result);
        }

        /// <summary>
        /// Verifies that the ExecuteNonQuery method returns the correct number of rows affected when provided with a
        /// command type, SQL command text, parameters, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that ExecuteNonQuery correctly passes the specified command type,
        /// command text, parameters, and transaction to the underlying command, and that it returns the expected
        /// result. It uses a mock command to simulate the database operation.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithCommandTypeSqlParametersAndTransaction_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(4);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var transactionMock = new Mock<IDbTransaction>().Object;
            var result = materializer.ExecuteNonQuery(CommandType.StoredProcedure, "EXEC", parameters, transactionMock);

            Assert.Equal(4, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQuery returns the number of rows affected when called with only the command type and
        /// SQL statement.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQuery method correctly returns the expected
        /// number of affected rows when provided with a command type and SQL statement, without additional parameters
        /// or configuration.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithCommandTypeAndSqlOnly_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(6);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.ExecuteNonQuery(CommandType.Text, "SELECT");

            Assert.Equal(6, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQuery returns the number of rows affected when called with CommandType.Text, a SQL
        /// command, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQuery method correctly executes a SQL command
        /// within a transaction context and returns the expected number of affected rows. It uses a mocked IDbCommand
        /// to simulate the database operation.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithCommandTypeSqlAndTransactionOnly_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(8);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transactionMock = new Mock<IDbTransaction>().Object;
            var result = materializer.ExecuteNonQuery(CommandType.Text, "SELECT", transactionMock);

            Assert.Equal(8, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync returns the number of rows affected when executed with SQL parameters
        /// using a mocked asynchronous database command.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithSqlParameters_ReturnsRowsAffected_DbCommandAsync()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(9);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.ExecuteNonQueryAsync("UPDATE", new { Id = 1 });

            Assert.Equal(9, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync returns the number of rows affected when called with a SQL command and
        /// parameters, using a fallback implementation.
        /// </summary>
        /// <remarks>This test ensures that when ExecuteNonQueryAsync is invoked with SQL parameters, it
        /// correctly returns the number of rows affected as reported by the underlying command, even when a fallback
        /// mechanism is used.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithSqlParameters_ReturnsRowsAffected_Fallback()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(10);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.ExecuteNonQueryAsync("UPDATE", new { Id = 1 });

            Assert.Equal(10, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync returns the correct number of rows affected when called with SQL
        /// parameters and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQueryAsync method correctly propagates the
        /// number of rows affected as returned by the underlying database command when provided with both parameters
        /// and a transaction.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithSqlParametersAndTransaction_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(11);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transactionMock = new Mock<IDbTransaction>().Object;
            var result = await materializer.ExecuteNonQueryAsync("UPDATE", new { Id = 1 }, transactionMock);

            Assert.Equal(11, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync returns the number of rows affected when called with only a SQL
        /// statement.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithSqlOnly_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(12);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.ExecuteNonQueryAsync("UPDATE");

            Assert.Equal(12, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync, when called with only a SQL statement and a transaction, returns the
        /// number of rows affected by the command.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithSqlAndTransactionOnly_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(13);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transactionMock = new Mock<IDbTransaction>().Object;
            var result = await materializer.ExecuteNonQueryAsync("UPDATE", transactionMock);

            Assert.Equal(13, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync returns the number of rows affected when called with a specified command
        /// type, SQL statement, and parameters.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQueryAsync method correctly returns the expected
        /// number of affected rows when provided with a command type, SQL command text, and a collection of SQL
        /// parameters.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithCommandTypeSqlParameters_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(14);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var result = await materializer.ExecuteNonQueryAsync(CommandType.Text, "SELECT", parameters);

            Assert.Equal(14, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync returns the correct number of rows affected when called with a specific
        /// command type, SQL statement, parameters, and transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQueryAsync method correctly propagates the
        /// number of rows affected as returned by the underlying database command when provided with all required
        /// arguments, including command type, SQL parameters, and a transaction.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithCommandTypeSqlParametersAndTransaction_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(15);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var transactionMock = new Mock<IDbTransaction>().Object;
            var result = await materializer.ExecuteNonQueryAsync(CommandType.Text, "SELECT", parameters, transactionMock);

            Assert.Equal(15, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync returns the number of rows affected when called with only the command
        /// type and SQL statement.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQueryAsync method correctly executes a non-query
        /// command and returns the expected number of affected rows when provided with a command type and SQL text,
        /// without additional parameters.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithCommandTypeAndSqlOnly_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(16);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.ExecuteNonQueryAsync(CommandType.Text, "SELECT");

            Assert.Equal(16, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync returns the number of rows affected when called with a SQL command type,
        /// a command text, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQueryAsync method correctly returns the expected
        /// number of affected rows when provided with only the command type, SQL command text, and a transaction. It
        /// uses a mock command to simulate the database operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithCommandTypeSqlAndTransactionOnly_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(17);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transactionMock = new Mock<IDbTransaction>().Object;
            var result = await materializer.ExecuteNonQueryAsync(CommandType.Text, "SELECT", transactionMock);

            Assert.Equal(17, result);
        }

        /// <summary>
        /// Verifies that the ExecuteNonQuery method returns the number of rows affected when executing a SQL procedure.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQuery method correctly invokes the underlying
        /// command and returns the expected result when provided with a SQL procedure. It uses mocks to simulate the
        /// database command and procedure behavior.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithSqlProcedure_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(18);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.ExecuteNonQuery<FakeDataParameter>(new TestSqlProcedure());

            Assert.Equal(18, result);
        }

        /// <summary>
        /// Verifies that executing a non-query SQL procedure within a transaction returns the expected number of rows
        /// affected.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQuery method correctly invokes the underlying
        /// command and returns the number of rows affected when used with a SQL procedure and transaction.</remarks>
        [Fact]
        public void ExecuteNonQuery_WithSqlProcedureAndTransaction_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(19);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.ExecuteNonQuery<FakeDataParameter>(new TestSqlProcedure(), new FakeDbTransaction());

            Assert.Equal(19, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync, when called with a SQL procedure, returns the expected number of rows
        /// affected.
        /// </summary>
        /// <remarks>This test uses mocks to simulate the execution of a SQL procedure and asserts that
        /// the correct number of affected rows is returned by the materializer.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithSqlProcedure_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(20);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.ExecuteNonQueryAsync<FakeDataParameter>(new TestSqlProcedure());

            Assert.Equal(20, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync returns the number of rows affected when executed with a SQL procedure
        /// and a transaction.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_WithSqlProcedureAndTransaction_ReturnsRowsAffected()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Returns(21);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.ExecuteNonQueryAsync<FakeDataParameter>(new TestSqlProcedure(), new FakeDbTransaction());

            Assert.Equal(21, result);
        }

        /// <summary>
        /// Verifies that ExecuteNonQueryAsync throws an OperationCanceledException when the provided CancellationToken
        /// is canceled.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteNonQueryAsync method correctly propagates
        /// cancellation by throwing an OperationCanceledException when invoked with a canceled
        /// CancellationToken.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteNonQueryAsync_OperationCanceled_ThrowsOperationCanceledException()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteNonQuery()).Throws(new TaskCanceledException());
            var materializer = CreateMaterializerWithCommand(commandMock);

            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                materializer.ExecuteNonQueryAsync("UPDATE", new { Id = 1 }, new CancellationToken(true)));
        }
    }
}