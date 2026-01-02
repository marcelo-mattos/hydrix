using Moq;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for verifying the behavior of the SQL materializer's query and query-async methods under
    /// various input and execution scenarios.
    /// </summary>
    /// <remarks>The tests in this class ensure that the SQL materializer correctly executes parameterized SQL
    /// queries, handles transactions, processes SQL procedures, and returns the expected collections of entity objects.
    /// Both synchronous and asynchronous query methods are covered, including validation of empty results and exception
    /// propagation for canceled operations. These tests help ensure the reliability and correctness of the
    /// materializer's data mapping and error handling behaviors.</remarks>
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the Query method returns the expected entities when provided with a SQL statement and
        /// parameters.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly executes a parameterized SQL query
        /// and materializes the results into entity objects. It checks that the returned collection contains the
        /// expected number of entities with the correct property values.</remarks>
        [Fact]
        public void Query_WithSqlAndParameters_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.Query<TestEntity>("SELECT 1", new { Id = 1 });

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that the Query method returns the expected entities when provided with SQL parameters and a
        /// database transaction.
        /// </summary>
        /// <remarks>This test ensures that the Query method correctly handles parameterized SQL queries
        /// and executes within the context of a supplied transaction. It validates that the returned entity collection
        /// contains the expected data based on the input parameters.</remarks>
        [Fact]
        public void Query_WithSqlParametersAndTransaction_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.Query<TestEntity>("SELECT 1", new { Id = 1 }, Mock.Of<IDbTransaction>());

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that querying with only a SQL statement returns the expected entities.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly maps the results of a SQL query to
        /// entity objects when no additional parameters or options are provided.</remarks>
        [Fact]
        public void Query_WithSqlOnly_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.Query<TestEntity>("SELECT 1");

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that querying with only a SQL statement and a transaction returns the expected entities.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly retrieves entities when provided
        /// with a SQL query and an active transaction, without additional parameters or command
        /// configuration.</remarks>
        [Fact]
        public void Query_WithSqlAndTransactionOnly_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.Query<TestEntity>("SELECT 1", Mock.Of<IDbTransaction>());

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that the Query method returns a list of entities when executed with a SQL command type, a SQL query
        /// string, and a collection of parameters.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly maps the results of a SQL query to
        /// a list of TestEntity objects when provided with the appropriate command type and parameters.</remarks>
        [Fact]
        public void Query_WithCommandTypeSqlAndParameters_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.Query<TestEntity>(CommandType.Text, "SELECT 1", new List<IDataParameter>());

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that the Query method returns the expected entities when provided with a command type, SQL
        /// statement, SQL parameters, and a database transaction.
        /// </summary>
        /// <remarks>This test ensures that the Query method correctly executes a SQL command with the
        /// specified parameters and transaction, and that it materializes the results into the expected entity
        /// objects.</remarks>
        [Fact]
        public void Query_WithCommandTypeSqlParametersAndTransaction_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.Query<TestEntity>(CommandType.Text, "SELECT 1", new List<IDataParameter>(), Mock.Of<IDbTransaction>());

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that querying with only a command type and SQL statement returns the expected entities.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly executes a query when provided with
        /// a command type and SQL string, and that it materializes the resulting data into entity objects as
        /// expected.</remarks>
        [Fact]
        public void Query_WithCommandTypeAndSqlOnly_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.Query<TestEntity>(CommandType.Text, "SELECT 1");

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that the Query method returns the expected entities when called with CommandType.Text, a SQL query,
        /// and a transaction, but without additional parameters or command setup.
        /// </summary>
        /// <remarks>This test ensures that the Query method correctly materializes entities from the
        /// database when only the command type, SQL statement, and transaction are provided. It checks that the
        /// returned collection contains the expected number of entities with the correct property values.</remarks>
        [Fact]
        public void Query_WithCommandTypeSqlAndTransactionOnly_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.Query<TestEntity>(CommandType.Text, "SELECT 1", Mock.Of<IDbTransaction>());

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that querying with a SQL procedure returns the expected collection of entities.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly executes a SQL procedure and
        /// materializes the resulting data into entity objects. It checks that the returned collection contains the
        /// expected number of entities with the correct property values.</remarks>
        [Fact]
        public void Query_WithSqlProcedure_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.Query<TestEntity, FakeDataParameter>(new TestSqlProcedure());

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that querying with a SQL procedure and an explicit transaction returns the expected entities.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly executes a SQL procedure within the
        /// context of a provided database transaction and returns the expected list of entities. It validates both the
        /// count and the content of the returned entities.</remarks>
        [Fact]
        public void Query_WithSqlProcedureAndTransaction_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.Query<TestEntity, FakeDataParameter>(new TestSqlProcedure(), new FakeDbTransaction());

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns a collection of entities when provided with a SQL query and
        /// parameters.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly maps the results of a parameterized
        /// SQL query to entity objects. It checks that the returned collection contains the expected number of entities
        /// with the correct property values.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithSqlAndParameters_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.QueryAsync<TestEntity>("SELECT 1", new { Id = 1 });

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns the expected entities when provided with SQL parameters and a
        /// transaction.
        /// </summary>
        /// <remarks>This test ensures that the QueryAsync method correctly materializes entities from the
        /// database when both SQL parameters and a transaction are supplied. It checks that the returned collection
        /// contains the expected number of entities with the correct property values.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithSqlParametersAndTransaction_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.QueryAsync<TestEntity>("SELECT 1", new { Id = 1 }, Mock.Of<IDbTransaction>());

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns the expected entities when executed with only a SQL query
        /// string.
        /// </summary>
        /// <remarks>This test ensures that the QueryAsync method correctly materializes entities from the
        /// results of a SQL query, without requiring additional parameters or configuration.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithSqlOnly_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.QueryAsync<TestEntity>("SELECT 1");

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns the expected entities when called with only a SQL query and a
        /// transaction.
        /// </summary>
        /// <remarks>This test ensures that QueryAsync correctly materializes entities from the database
        /// when provided with a SQL statement and an IDbTransaction, without additional parameters or
        /// options.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithSqlAndTransactionOnly_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.QueryAsync<TestEntity>("SELECT 1", Mock.Of<IDbTransaction>());

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns a list of entities when executed with a SQL command type, a SQL
        /// query string, and a collection of parameters.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly maps the results of a SQL query to
        /// a list of TestEntity objects when provided with the appropriate command type and parameters.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithCommandTypeSqlAndParameters_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.QueryAsync<TestEntity>(CommandType.Text, "SELECT 1", new List<IDataParameter>());

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns the expected list of entities when provided with a command type,
        /// SQL statement, SQL parameters, and a database transaction.
        /// </summary>
        /// <remarks>This test ensures that QueryAsync correctly materializes entities from the database
        /// when all relevant command options are specified, including command type, SQL parameters, and transaction
        /// context.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithCommandTypeSqlParametersAndTransaction_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.QueryAsync<TestEntity>(CommandType.Text, "SELECT 1", new List<IDataParameter>(), Mock.Of<IDbTransaction>());

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns a collection of entities when called with a command type and SQL
        /// statement only.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly executes a query using the
        /// specified command type and SQL, and that the resulting collection contains the expected entities. It
        /// validates both the count and the property values of the returned entities.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithCommandTypeAndSqlOnly_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.QueryAsync<TestEntity>(CommandType.Text, "SELECT 1");

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns a collection of entities when executed with a SQL command type,
        /// a SQL statement, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the QueryAsync method correctly materializes entities from the
        /// result set when provided with only the command type, SQL statement, and transaction. It validates that the
        /// returned collection contains the expected entities with correct property values.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithCommandTypeSqlAndTransactionOnly_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.QueryAsync<TestEntity>(CommandType.Text, "SELECT 1", Mock.Of<IDbTransaction>());

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns the expected entities when executed with a SQL procedure.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly maps the results of a SQL procedure
        /// to entity objects. It validates both the count and the property values of the returned entities.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithSqlProcedure_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.QueryAsync<TestEntity, FakeDataParameter>(new TestSqlProcedure());

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns the expected list of entities when executed with a SQL procedure
        /// and a database transaction.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly executes a SQL procedure within a
        /// transaction context and materializes the resulting data into entity objects. It validates that the returned
        /// collection contains the expected number of entities with the correct property values.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithSqlProcedureAndTransaction_ReturnsEntities()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader().Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.QueryAsync<TestEntity, FakeDataParameter>(new TestSqlProcedure(), new FakeDbTransaction());

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Alice", result[0].Name);
        }

        /// <summary>
        /// Verifies that the Query method returns an empty result when input validation fails.
        /// </summary>
        /// <remarks>This test ensures that when the Query method is called with parameters that do not
        /// pass validation, it returns an empty collection rather than throwing an exception or returning
        /// null.</remarks>
        [Fact]
        public void Query_ValidationFails_ReturnsEmpty()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(true).Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = materializer.Query<TestEntity>("SELECT 1", new { Id = 1 });

            Assert.NotNull(result);
            Assert.Equal(0, result.Count);
            Assert.Empty(result);
        }

        /// <summary>
        /// Verifies that QueryAsync returns an empty collection when input validation fails.
        /// </summary>
        /// <remarks>This test ensures that when QueryAsync is called with parameters that do not pass
        /// validation, the result is an empty collection rather than null or an exception.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_ValidationFails_ReturnsEmpty()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(CreateMockReader(true).Object);
            var materializer = CreateMaterializerWithCommand(commandMock);

            var result = await materializer.QueryAsync<TestEntity>("SELECT 1", new { Id = 1 });

            Assert.NotNull(result);
            Assert.Equal(0, result.Count);
            Assert.Empty(result);
        }

        /// <summary>
        /// Verifies that the QueryAsync method throws a TaskCanceledException when the operation is canceled.
        /// </summary>
        /// <remarks>This test ensures that QueryAsync correctly propagates a TaskCanceledException when
        /// the provided CancellationToken is already canceled at the time of invocation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_OperationCanceled_ThrowsOperationCanceledException()
        {
            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(c => c.ExecuteScalar()).Throws(new TaskCanceledException());
            var materializer = CreateMaterializerWithCommand(commandMock);

            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                materializer.QueryAsync<TestEntity>("SELECT 1", new { }, new CancellationToken(true)));
        }
    }
}