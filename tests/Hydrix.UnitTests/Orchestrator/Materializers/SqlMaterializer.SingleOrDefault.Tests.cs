using Moq;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for verifying the behavior of the SingleOrDefault and SingleOrDefaultAsync methods of the
    /// SQL materializer, ensuring correct entity mapping and handling of various query scenarios.
    /// </summary>
    /// <remarks>The tests in this class cover both synchronous and asynchronous execution paths, including
    /// cases with SQL queries, parameters, transactions, command types, and stored procedures. They validate that the
    /// materializer returns the expected entity or null when appropriate, and that exceptions are properly propagated
    /// in cancellation scenarios. These tests use mocked database commands to simulate different database responses and
    /// ensure reliable, isolated testing of the materializer's logic.</remarks>
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when executed with a SQL query and
        /// parameters.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly maps the result of the SQL query to
        /// a TestEntity instance when provided with specific parameters. It checks that the returned entity is not null
        /// and that its properties match the expected values.</remarks>
        [Fact]
        public void SingleOrDefault_WithSqlAndParameters_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.SingleOrDefault<TestEntity>("SELECT 1", new { Id = 1 });

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns null when executing a SQL query with parameters and no
        /// matching records are found.
        /// </summary>
        /// <remarks>This test ensures that SingleOrDefault&lt;T&gt; correctly returns null when the result set
        /// is empty, validating expected behavior for queries that yield no results.</remarks>
        [Fact]
        public void SingleOrDefault_WithSqlAndParameters_ReturnsNull()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(true).Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.SingleOrDefault<TestEntity>("SELECT 1", new { Id = 1 });

            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when executed with SQL parameters and a
        /// database transaction.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly maps the result of a parameterized
        /// SQL query within a transaction to a TestEntity instance. It validates that the returned entity has the
        /// expected property values.</remarks>
        [Fact]
        public void SingleOrDefault_WithSqlParametersAndTransaction_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.SingleOrDefault<TestEntity>("SELECT 1", new { Id = 1 }, Mock.Of<IDbTransaction>());

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when executed with a raw SQL query.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly maps the result of the provided SQL
        /// query to a TestEntity instance. It uses a mocked database command to simulate the query execution.</remarks>
        [Fact]
        public void SingleOrDefault_WithSqlOnly_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.SingleOrDefault<TestEntity>("SELECT 1");

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when provided with a SQL query and a
        /// transaction, without additional parameters.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly executes the SQL command within the
        /// specified transaction context and returns a single entity when one is present. It checks that the returned
        /// entity has the expected property values.</remarks>
        [Fact]
        public void SingleOrDefault_WithSqlAndTransactionOnly_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.SingleOrDefault<TestEntity>("SELECT 1", Mock.Of<IDbTransaction>());

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when executed with a specified command
        /// type, SQL query, and parameters.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly maps the result of a SQL command to
        /// a TestEntity instance when provided with command type, SQL text, and a list of parameters. It checks that
        /// the returned entity is not null and that its properties match the expected values.</remarks>
        [Fact]
        public void SingleOrDefault_WithCommandTypeSqlAndParameters_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.SingleOrDefault<TestEntity>(CommandType.Text, "SELECT 1", new List<IDataParameter>());

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when called with a command type, SQL
        /// statement, SQL parameters, and a database transaction.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly executes a query using the
        /// specified command type, SQL parameters, and transaction, and that it returns the appropriate entity instance
        /// when a matching record is found.</remarks>
        [Fact]
        public void SingleOrDefault_WithCommandTypeSqlParametersAndTransaction_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.SingleOrDefault<TestEntity>(CommandType.Text, "SELECT 1", new List<IDataParameter>(), Mock.Of<IDbTransaction>());

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns a mapped entity when executed with only a command type and
        /// SQL statement.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly retrieves and maps a single entity
        /// from the database when provided with minimal command information. It checks that the returned entity has the
        /// expected property values.</remarks>
        [Fact]
        public void SingleOrDefault_WithCommandTypeAndSqlOnly_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.SingleOrDefault<TestEntity>(CommandType.Text, "SELECT 1");

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when called with a command type, SQL
        /// statement, and transaction, but without additional parameters or command setup.
        /// </summary>
        /// <remarks>This test ensures that SingleOrDefault can successfully materialize an entity from a
        /// SQL query when only the command type, SQL, and transaction are provided. It checks that the returned entity
        /// has the correct property values, confirming correct mapping and execution in this usage scenario.</remarks>
        [Fact]
        public void SingleOrDefault_WithCommandTypeSqlAndTransactionOnly_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.SingleOrDefault<TestEntity>(CommandType.Text, "SELECT 1", Mock.Of<IDbTransaction>());

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when executed with a SQL procedure.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly maps the result of a SQL procedure
        /// to a TestEntity instance. It checks that the returned entity is not null and that its properties match the
        /// expected values.</remarks>
        [Fact]
        public void SingleOrDefault_WithSqlProcedure_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.SingleOrDefault<TestEntity, FakeDataParameter>(new TestSqlProcedure());

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when executed with a SQL procedure and
        /// a database transaction.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly retrieves a single entity from the
        /// database when provided with a SQL procedure and an active transaction. It validates that the returned entity
        /// has the expected property values.</remarks>
        [Fact]
        public void SingleOrDefault_WithSqlProcedureAndTransaction_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.SingleOrDefault<TestEntity, FakeDataParameter>(new TestSqlProcedure(), new FakeDbTransaction());

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when executed with a SQL query and
        /// parameters.
        /// </summary>
        /// <remarks>This test ensures that SingleOrDefaultAsync correctly materializes a TestEntity from
        /// the result of the provided SQL query and parameters. It checks that the returned entity is not null and that
        /// its properties match the expected values.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithSqlAndParameters_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.SingleOrDefaultAsync<TestEntity>("SELECT 1", new { Id = 1 });

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that SingleOrDefaultAsync returns null when the specified SQL query and parameters do not match any
        /// records.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly returns null when no results are
        /// found for the given SQL and parameters. It is useful for validating the behavior of data access methods when
        /// queries yield no results.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithSqlAndParameters_ReturnsNull()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(true).Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.SingleOrDefaultAsync<TestEntity>("SELECT 1", new { Id = 1 });

            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when executed with SQL parameters
        /// and a database transaction.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly maps the result of the SQL query to
        /// a TestEntity object when provided with specific parameters and a transaction context.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithSqlParametersAndTransaction_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.SingleOrDefaultAsync<TestEntity>("SELECT 1", new { Id = 1 }, Mock.Of<IDbTransaction>());

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when executed with a raw SQL
        /// query.
        /// </summary>
        /// <remarks>This test ensures that executing SingleOrDefaultAsync with a SQL-only input retrieves
        /// a single entity matching the query, or null if no entity is found. It uses a mocked database command to
        /// simulate the query execution.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithSqlOnly_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.SingleOrDefaultAsync<TestEntity>("SELECT 1");

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when provided with only a SQL
        /// query and a transaction.
        /// </summary>
        /// <remarks>This test ensures that SingleOrDefaultAsync can successfully retrieve a single entity
        /// when called with a SQL statement and a transaction, without additional parameters. The test asserts that the
        /// returned entity is not null and that its properties match the expected values.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithSqlAndTransactionOnly_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.SingleOrDefaultAsync<TestEntity>("SELECT 1", Mock.Of<IDbTransaction>());

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that SingleOrDefaultAsync returns the expected entity when executed with a SQL command type, a SQL
        /// query, and a collection of parameters.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly retrieves a single entity or null
        /// when provided with a specific command type, SQL statement, and parameters. It asserts that the returned
        /// entity matches the expected values.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithCommandTypeSqlAndParameters_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.SingleOrDefaultAsync<TestEntity>(CommandType.Text, "SELECT 1", new List<IDataParameter>());

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when executed with a specified
        /// command type, SQL parameters, and transaction.
        /// </summary>
        /// <remarks>This test ensures that SingleOrDefaultAsync correctly materializes a TestEntity when
        /// provided with CommandType.Text, a SQL query, a list of IDataParameter objects, and an IDbTransaction. The
        /// test asserts that the returned entity is not null and that its properties match the expected
        /// values.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithCommandTypeSqlParametersAndTransaction_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.SingleOrDefaultAsync<TestEntity>(CommandType.Text, "SELECT 1", new List<IDataParameter>(), Mock.Of<IDbTransaction>());

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when called with only the command
        /// type and SQL statement.
        /// </summary>
        /// <remarks>This test ensures that SingleOrDefaultAsync correctly materializes a single entity
        /// from the result set when provided with a command type and SQL query, without additional parameters or
        /// configuration.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithCommandTypeAndSqlOnly_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.SingleOrDefaultAsync<TestEntity>(CommandType.Text, "SELECT 1");

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when called with a SQL command
        /// type, a SQL query, and a transaction, without additional parameters.
        /// </summary>
        /// <remarks>This test ensures that SingleOrDefaultAsync correctly materializes a single entity
        /// from the result set when only the command type, SQL statement, and transaction are provided. It asserts that
        /// the returned entity is not null and that its properties match the expected values.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithCommandTypeSqlAndTransactionOnly_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.SingleOrDefaultAsync<TestEntity>(CommandType.Text, "SELECT 1", Mock.Of<IDbTransaction>());

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when executed with a SQL
        /// procedure.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithSqlProcedure_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.SingleOrDefaultAsync<TestEntity, FakeDataParameter>(new TestSqlProcedure());

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when executed with a SQL procedure
        /// and a database transaction.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly retrieves a single entity from the
        /// database when provided with a SQL procedure and an active transaction. The test asserts that the returned
        /// entity is not null and that its properties match the expected values.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithSqlProcedureAndTransaction_ReturnsEntity()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.SingleOrDefaultAsync<TestEntity, FakeDataParameter>(new TestSqlProcedure(), new FakeDbTransaction());

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.Name);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method throws a TaskCanceledException when the operation is canceled.
        /// </summary>
        /// <remarks>This test ensures that SingleOrDefaultAsync correctly propagates a
        /// TaskCanceledException when the underlying command is canceled. It uses a mocked IDbCommand to simulate the
        /// cancellation scenario.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_OperationCanceled_ThrowsOperationCanceledException()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Throws(new TaskCanceledException());
            var materializer = CreateMaterializerWithCommand(commandMock);

            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                materializer.SingleOrDefaultAsync<TestEntity>("SELECT 1", new { }, new CancellationToken(true)));
        }
    }
}