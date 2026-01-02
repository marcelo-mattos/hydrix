using Moq;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for verifying the behavior of SQL materializer methods that execute scalar database commands
    /// synchronously and asynchronously.
    /// </summary>
    /// <remarks>These tests cover various overloads of the ExecuteScalar and ExecuteScalarAsync methods,
    /// including scenarios with different command types, SQL statements, parameters, and transactions. The tests ensure
    /// that the materializer returns the expected values and correctly handles cancellation and exceptions.</remarks>
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected value when provided with a SQL query and
        /// parameters.
        /// </summary>
        /// <remarks>This test ensures that ExecuteScalar correctly executes the command and returns the
        /// result from the underlying data provider. It uses a mock command to simulate database interaction.</remarks>
        [Fact]
        public void ExecuteScalar_WithSqlAndParameters_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(42);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.ExecuteScalar("SELECT", new { Id = 1 });

            Assert.Equal(42, result);
            commandMock.Verify(c => c.ExecuteScalar(), Times.Once);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected value when provided with SQL parameters and a
        /// transaction.
        /// </summary>
        /// <remarks>This test ensures that ExecuteScalar correctly executes a scalar command using the
        /// specified SQL statement, parameters, and transaction, and that it returns the value produced by the
        /// underlying command.</remarks>
        [Fact]
        public void ExecuteScalar_WithSqlParametersAndTransaction_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns("abc");
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transactionMock = new Mock<IDbTransaction>().Object;
            var result = materializer.ExecuteScalar("SELECT", new { Id = 2 }, transactionMock);

            Assert.Equal("abc", result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected value when called with only a SQL statement.
        /// </summary>
        /// <remarks>This test ensures that providing a SQL query to ExecuteScalar results in the correct
        /// value being returned from the underlying command. It uses a mock command to simulate database
        /// interaction.</remarks>
        [Fact]
        public void ExecuteScalar_WithSqlOnly_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(1);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.ExecuteScalar("SELECT");

            Assert.Equal(1, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected value when called with a SQL statement and a
        /// transaction.
        /// </summary>
        /// <remarks>This test ensures that providing only the SQL command text and a transaction to
        /// ExecuteScalar produces the correct result. It uses a mocked command to simulate database
        /// interaction.</remarks>
        [Fact]
        public void ExecuteScalar_WithSqlAndTransactionOnly_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(3);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transactionMock = new Mock<IDbTransaction>().Object;
            var result = materializer.ExecuteScalar("SELECT", transactionMock);

            Assert.Equal(3, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected value when called with CommandType.Text, a SQL
        /// command, and a list of parameters.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly executes a scalar command and
        /// returns the result provided by the underlying command implementation. It uses a mock command to simulate the
        /// database interaction.</remarks>
        [Fact]
        public void ExecuteScalar_WithCommandTypeSqlAndParameters_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(7);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var result = materializer.ExecuteScalar(CommandType.Text, "SELECT", parameters);

            Assert.Equal(7, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected value when called with a specified command type,
        /// SQL statement, parameters, and transaction.
        /// </summary>
        /// <remarks>This test ensures that ExecuteScalar correctly executes a command using
        /// CommandType.StoredProcedure, passes the provided parameters and transaction, and returns the expected scalar
        /// result.</remarks>
        [Fact]
        public void ExecuteScalar_WithCommandTypeSqlParametersAndTransaction_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(4);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var transactionMock = new Mock<IDbTransaction>().Object;
            var result = materializer.ExecuteScalar(CommandType.StoredProcedure, "EXEC", parameters, transactionMock);

            Assert.Equal(4, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected value when called with only the command type and
        /// SQL statement.
        /// </summary>
        /// <remarks>This test ensures that ExecuteScalar correctly executes a scalar query using the
        /// specified command type and SQL, and that the returned value matches the expected result.</remarks>
        [Fact]
        public void ExecuteScalar_WithCommandTypeAndSqlOnly_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(6);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.ExecuteScalar(CommandType.Text, "SELECT");

            Assert.Equal(6, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected value when called with CommandType.Text, a SQL
        /// command, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that ExecuteScalar correctly executes a scalar SQL command within
        /// the context of a provided transaction and returns the expected result. It uses a mocked IDbCommand to
        /// simulate database interaction.</remarks>
        [Fact]
        public void ExecuteScalar_WithCommandTypeSqlAndTransactionOnly_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(8);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transactionMock = new Mock<IDbTransaction>().Object;
            var result = materializer.ExecuteScalar(CommandType.Text, "SELECT", transactionMock);

            Assert.Equal(8, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalarAsync method returns the expected value when provided with a SQL query and
        /// parameters.
        /// </summary>
        /// <remarks>This test ensures that ExecuteScalarAsync correctly executes a scalar SQL command
        /// with the specified parameters and returns the result as expected.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithSqlParameters_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(9);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.ExecuteScalarAsync("SELECT", new { Id = 1 });

            Assert.Equal(9, result);
        }

        /// <summary>
        /// Verifies that ExecuteScalarAsync returns the expected value when provided with SQL parameters and a
        /// transaction.
        /// </summary>
        /// <remarks>This unit test ensures that the ExecuteScalarAsync method correctly executes a scalar
        /// SQL command using the specified parameters and transaction, and that it returns the expected result. The
        /// test uses mocks to simulate database interactions.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithSqlParametersAndTransaction_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(11);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transactionMock = new Mock<IDbTransaction>().Object;
            var result = await materializer.ExecuteScalarAsync("SELECT", new { Id = 1 }, transactionMock);

            Assert.Equal(11, result);
        }

        /// <summary>
        /// Verifies that ExecuteScalarAsync returns the expected value when called with only a SQL statement.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalarAsync method correctly executes a scalar SQL
        /// query and returns the expected result when no additional parameters are provided.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithSqlOnly_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(12);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.ExecuteScalarAsync("SELECT");

            Assert.Equal(12, result);
        }

        /// <summary>
        /// Verifies that ExecuteScalarAsync returns the expected value when called with only a SQL statement and a
        /// transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalarAsync method correctly executes a scalar SQL
        /// command using the provided transaction and returns the expected result.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithSqlAndTransactionOnly_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(13);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transactionMock = new Mock<IDbTransaction>().Object;
            var result = await materializer.ExecuteScalarAsync("SELECT", transactionMock);

            Assert.Equal(13, result);
        }

        /// <summary>
        /// Verifies that ExecuteScalarAsync returns the expected value when provided with a command type, SQL
        /// statement, and parameters.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalarAsync method correctly executes a scalar
        /// command and returns the expected result when invoked with specific command type and parameters.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithCommandTypeSqlParameters_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(14);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var result = await materializer.ExecuteScalarAsync(CommandType.Text, "SELECT", parameters);

            Assert.Equal(14, result);
        }

        /// <summary>
        /// Verifies that ExecuteScalarAsync returns the expected value when called with a command type, SQL statement,
        /// parameters, and transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalarAsync method correctly executes a scalar
        /// command using the provided command type, SQL statement, parameters, and transaction, and returns the
        /// expected result.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithCommandTypeSqlParametersAndTransaction_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(15);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var parameters = new List<IDataParameter>();
            var transactionMock = new Mock<IDbTransaction>().Object;
            var result = await materializer.ExecuteScalarAsync(CommandType.Text, "SELECT", parameters, transactionMock);

            Assert.Equal(15, result);
        }

        /// <summary>
        /// Verifies that ExecuteScalarAsync returns the expected value when called with only the command type and SQL
        /// statement.
        /// </summary>
        /// <remarks>This test ensures that ExecuteScalarAsync correctly executes a scalar command using
        /// the specified command type and SQL, and returns the value produced by the underlying command.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithCommandTypeAndSqlOnly_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(16);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.ExecuteScalarAsync(CommandType.Text, "SELECT");

            Assert.Equal(16, result);
        }

        /// <summary>
        /// Verifies that ExecuteScalarAsync returns the expected value when called with CommandType.Text, a SQL
        /// command, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalarAsync method correctly executes a scalar SQL
        /// command within a transaction and returns the expected result.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithCommandTypeSqlAndTransactionOnly_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(17);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var transactionMock = new Mock<IDbTransaction>().Object;
            var result = await materializer.ExecuteScalarAsync(CommandType.Text, "SELECT", transactionMock);

            Assert.Equal(17, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected value when executed with a SQL procedure.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly invokes the ExecuteScalar method on
        /// the underlying command and returns the result as expected. It uses a mock command to simulate the database
        /// interaction.</remarks>
        [Fact]
        public void ExecuteScalar_WithSqlProcedure_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(18);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.ExecuteScalar<FakeDataParameter>(new TestSqlProcedure());

            Assert.Equal(18, result);
        }

        /// <summary>
        /// Verifies that the ExecuteScalar method returns the expected value when executed with a SQL procedure and a
        /// database transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalar method correctly executes a SQL procedure
        /// within the context of a transaction and returns the expected scalar result. It uses a mocked command to
        /// simulate the database operation.</remarks>
        [Fact]
        public void ExecuteScalar_WithSqlProcedureAndTransaction_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(19);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.ExecuteScalar<FakeDataParameter>(new TestSqlProcedure(), new FakeDbTransaction());

            Assert.Equal(19, result);
        }

        /// <summary>
        /// Verifies that ExecuteScalarAsync returns the expected value when executed with a SQL procedure.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalarAsync method correctly retrieves the scalar
        /// result from a SQL procedure using a mocked command. It validates that the returned value matches the
        /// expected result.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithSqlProcedure_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(20);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.ExecuteScalarAsync<FakeDataParameter>(new TestSqlProcedure());

            Assert.Equal(20, result);
        }

        /// <summary>
        /// Verifies that ExecuteScalarAsync returns the expected value when called with a SQL procedure and a database
        /// transaction.
        /// </summary>
        /// <remarks>This test ensures that the ExecuteScalarAsync method correctly executes a SQL
        /// procedure within the context of a transaction and returns the scalar result as expected.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_WithSqlProcedureAndTransaction_ReturnsExpectedValue()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Returns(21);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.ExecuteScalarAsync<FakeDataParameter>(new TestSqlProcedure(), new FakeDbTransaction());

            Assert.Equal(21, result);
        }

        /// <summary>
        /// Verifies that ExecuteScalarAsync throws a TaskCanceledException when the operation is canceled.
        /// </summary>
        /// <remarks>This test ensures that ExecuteScalarAsync correctly propagates a
        /// TaskCanceledException when the provided CancellationToken is already canceled. This behavior is important
        /// for consumers who rely on proper cancellation handling in asynchronous database operations.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ExecuteScalarAsync_OperationCanceled_ThrowsOperationCanceledException()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Throws(new TaskCanceledException());
            var materializer = CreateMaterializerWithCommand(commandMock);

            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                materializer.ExecuteScalarAsync("SELECT", new { Id = 1 }, new CancellationToken(true)));
        }
    }
}