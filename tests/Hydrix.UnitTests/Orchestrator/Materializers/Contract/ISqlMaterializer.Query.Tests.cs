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
    /// Contains unit tests for verifying the behavior of SQL materializer query methods, including synchronous and
    /// asynchronous execution, parameter handling, transaction support, and exception scenarios.
    /// </summary>
    /// <remarks>These tests ensure that the ISqlMaterializer interface and its implementations correctly
    /// handle various query overloads, support for command types, SQL procedures, and expected exceptions. The tests
    /// use mocks to simulate database interactions and validate that the materializer returns the expected entities or
    /// throws appropriate exceptions under different conditions.</remarks>
    public partial class ISqlMaterializerTests
    {
        /// <summary>
        /// Verifies that querying with a SQL statement and parameters returns the expected entities.
        /// </summary>
        /// <remarks>This unit test ensures that the Query method correctly processes a SQL query with
        /// parameters and returns the appropriate entity collection. It uses a mock materializer to simulate the data
        /// retrieval.</remarks>
        [Fact]
        public void Query_WithSqlAndParameters_ReturnsEntities()
        {
            _materializerMock.Setup(m => m.Query<DummyEntity>("SELECT", It.IsAny<object>()))
                .Returns(_entityList);
            var result = _materializerMock.Object.Query<DummyEntity>("SELECT", new { Id = 1 });
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that querying with SQL parameters and a transaction returns the expected entities.
        /// </summary>
        /// <remarks>This test ensures that the Query method correctly handles SQL parameters and an
        /// explicit transaction, returning the appropriate entity results.</remarks>
        [Fact]
        public void Query_WithSqlParametersAndTransaction_ReturnsEntities()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.Query<DummyEntity>("SELECT", It.IsAny<object>(), transaction))
                .Returns(_entityList);
            var result = _materializerMock.Object.Query<DummyEntity>("SELECT", new { Id = 2 }, transaction);
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that querying with only a SQL statement returns the expected entities.
        /// </summary>
        /// <remarks>This test ensures that the Query method returns the correct set of entities when
        /// provided with a SQL query string, without additional parameters or options.</remarks>
        [Fact]
        public void Query_WithSqlOnly_ReturnsEntities()
        {
            _materializerMock.Setup(m => m.Query<DummyEntity>("SELECT"))
                .Returns(_entityList);
            var result = _materializerMock.Object.Query<DummyEntity>("SELECT");
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that querying with a SQL statement and a transaction returns the expected entities.
        /// </summary>
        /// <remarks>This test ensures that the Query method correctly retrieves entities when provided
        /// with both a SQL query and a transaction object. It validates that the result contains the expected number of
        /// entities.</remarks>
        [Fact]
        public void Query_WithSqlAndTransaction_ReturnsEntities()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.Query<DummyEntity>("SELECT", transaction))
                .Returns(_entityList);
            var result = _materializerMock.Object.Query<DummyEntity>("SELECT", transaction);
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that querying with a SQL command type, a SQL statement, and a collection of parameters returns the
        /// expected entities.
        /// </summary>
        /// <remarks>This test ensures that the Query method correctly processes SQL text commands with
        /// parameters and returns the appropriate entity collection. It uses a mock materializer to simulate database
        /// interaction.</remarks>
        [Fact]
        public void Query_WithCommandTypeSqlAndParameters_ReturnsEntities()
        {
            var parameters = new List<IDataParameter>();
            _materializerMock.Setup(m => m.Query<DummyEntity>(CommandType.Text, "SELECT", parameters))
                .Returns(_entityList);
            var result = _materializerMock.Object.Query<DummyEntity>(CommandType.Text, "SELECT", parameters);
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that querying with a specified command type, SQL parameters, and transaction returns the expected
        /// entities.
        /// </summary>
        /// <remarks>This test ensures that the Query method correctly handles command type, SQL
        /// parameters, and transaction inputs, and returns the appropriate result set. It is intended to validate the
        /// integration of these parameters in the data access layer.</remarks>
        [Fact]
        public void Query_WithCommandTypeSqlParametersAndTransaction_ReturnsEntities()
        {
            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.Query<DummyEntity>(CommandType.Text, "SELECT", parameters, transaction))
                .Returns(_entityList);
            var result = _materializerMock.Object.Query<DummyEntity>(CommandType.Text, "SELECT", parameters, transaction);
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that the Query method returns the expected entities when called with a specific command type and
        /// SQL statement.
        /// </summary>
        /// <remarks>This test ensures that the Query method correctly materializes entities from the
        /// provided SQL command when using CommandType.Text. It validates that the returned collection contains the
        /// expected number of entities.</remarks>
        [Fact]
        public void Query_WithCommandTypeAndSql_ReturnsEntities()
        {
            _materializerMock.Setup(m => m.Query<DummyEntity>(CommandType.Text, "SELECT"))
                .Returns(_entityList);
            var result = _materializerMock.Object.Query<DummyEntity>(CommandType.Text, "SELECT");
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that querying with a SQL command type and an explicit transaction returns the expected entities.
        /// </summary>
        /// <remarks>This test ensures that the Query method correctly handles CommandType.Text and an
        /// IDbTransaction parameter, returning the appropriate entity collection.</remarks>
        [Fact]
        public void Query_WithCommandTypeSqlAndTransaction_ReturnsEntities()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.Query<DummyEntity>(CommandType.Text, "SELECT", transaction))
                .Returns(_entityList);
            var result = _materializerMock.Object.Query<DummyEntity>(CommandType.Text, "SELECT", transaction);
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that querying a generic SQL procedure returns the expected entities.
        /// </summary>
        /// <remarks>This test ensures that the Query method, when invoked with a generic SQL procedure
        /// and parameter types, returns the correct set of entities. It uses a mock materializer to simulate the
        /// database interaction.</remarks>
        [Fact]
        public void Query_GenericSqlProcedure_ReturnsEntities()
        {
            var proc = new DummySqlProcedure();
            _materializerMock.Setup(m => m.Query<DummyEntity, DummyParameter>(proc))
                .Returns(_entityList);
            var result = _materializerMock.Object.Query<DummyEntity, DummyParameter>(proc);
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that querying a generic SQL procedure with a transaction returns the expected entities.
        /// </summary>
        /// <remarks>This unit test ensures that the Query method, when called with a specific SQL
        /// procedure and transaction, returns the correct set of entities. The test uses mocks to simulate the database
        /// interaction and asserts that the result contains the expected number of entities.</remarks>
        [Fact]
        public void Query_GenericSqlProcedureWithTransaction_ReturnsEntities()
        {
            var proc = new DummySqlProcedure();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.Query<DummyEntity, DummyParameter>(proc, transaction))
                .Returns(_entityList);
            var result = _materializerMock.Object.Query<DummyEntity, DummyParameter>(proc, transaction);
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns the expected entities when called with SQL parameters.
        /// </summary>
        /// <remarks>This test ensures that providing SQL parameters to the QueryAsync method results in
        /// the correct entities being returned. It uses a mock materializer to simulate database interaction and
        /// asserts that the result contains the expected number of entities.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithSqlParameters_ReturnsEntities()
        {
            _materializerMock.Setup(m => m.QueryAsync<DummyEntity>("SELECT", It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entityList);
            var result = await _materializerMock.Object.QueryAsync<DummyEntity>("SELECT", new { Id = 1 });
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns the expected entities when called with SQL parameters and a
        /// database transaction.
        /// </summary>
        /// <remarks>This test ensures that the QueryAsync method correctly processes SQL parameters and
        /// an explicit transaction, returning the appropriate set of entities. It uses a mock materializer to simulate
        /// database interaction.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithSqlParametersAndTransaction_ReturnsEntities()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.QueryAsync<DummyEntity>("SELECT", It.IsAny<object>(), transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entityList);
            var result = await _materializerMock.Object.QueryAsync<DummyEntity>("SELECT", new { Id = 2 }, transaction);
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns the expected entities when called with only a SQL query.
        /// </summary>
        /// <remarks>This test ensures that the QueryAsync method, when provided with a SQL statement and
        /// no additional parameters, retrieves the correct set of entities from the data source.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithSqlOnly_ReturnsEntities()
        {
            _materializerMock.Setup(m => m.QueryAsync<DummyEntity>("SELECT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entityList);
            var result = await _materializerMock.Object.QueryAsync<DummyEntity>("SELECT");
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns the expected entities when called with a SQL query and a
        /// database transaction.
        /// </summary>
        /// <remarks>This test ensures that the QueryAsync method correctly retrieves entities when
        /// provided with both a SQL statement and an IDbTransaction. It uses a mock materializer to simulate the
        /// database operation and asserts that the result contains the expected number of entities.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithSqlAndTransaction_ReturnsEntities()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.QueryAsync<DummyEntity>("SELECT", transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entityList);
            var result = await _materializerMock.Object.QueryAsync<DummyEntity>("SELECT", transaction);
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns the expected entities when called with CommandType.Text and a
        /// list of SQL parameters.
        /// </summary>
        /// <remarks>This unit test ensures that the QueryAsync method correctly retrieves entities when
        /// provided with a specific command type and parameter list. It uses a mock materializer to simulate database
        /// interaction and asserts that the result contains the expected number of entities.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithCommandTypeSqlParameters_ReturnsEntities()
        {
            var parameters = new List<IDataParameter>();
            _materializerMock.Setup(m => m.QueryAsync<DummyEntity>(CommandType.Text, "SELECT", parameters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entityList);
            var result = await _materializerMock.Object.QueryAsync<DummyEntity>(CommandType.Text, "SELECT", parameters);
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns the expected entities when called with CommandType.Text, a SQL
        /// command, a list of SQL parameters, and a database transaction.
        /// </summary>
        /// <remarks>This test ensures that the QueryAsync method correctly processes the provided command
        /// type, SQL statement, parameters, and transaction, and returns the expected result set. It uses a mock
        /// materializer to simulate database interaction.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithCommandTypeSqlParametersAndTransaction_ReturnsEntities()
        {
            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.QueryAsync<DummyEntity>(CommandType.Text, "SELECT", parameters, transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entityList);
            var result = await _materializerMock.Object.QueryAsync<DummyEntity>(CommandType.Text, "SELECT", parameters, transaction);
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns the expected entities when called with a specific command type
        /// and SQL statement.
        /// </summary>
        /// <remarks>This test ensures that the QueryAsync method correctly retrieves entities when
        /// provided with CommandType.Text and a SQL query. It uses a mock materializer to simulate the data retrieval
        /// and asserts that the result contains the expected number of entities.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithCommandTypeAndSql_ReturnsEntities()
        {
            _materializerMock.Setup(m => m.QueryAsync<DummyEntity>(CommandType.Text, "SELECT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entityList);
            var result = await _materializerMock.Object.QueryAsync<DummyEntity>(CommandType.Text, "SELECT");
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that the QueryAsync method returns the expected entities when called with CommandType.Text, a SQL
        /// command, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the QueryAsync method correctly retrieves entities when
        /// provided with a SQL command type and an explicit database transaction. It validates that the result contains
        /// the expected number of entities.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_WithCommandTypeSqlAndTransaction_ReturnsEntities()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.QueryAsync<DummyEntity>(CommandType.Text, "SELECT", transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entityList);
            var result = await _materializerMock.Object.QueryAsync<DummyEntity>(CommandType.Text, "SELECT", transaction);
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that the generic QueryAsync method for a SQL procedure returns the expected entities.
        /// </summary>
        /// <remarks>This test ensures that when QueryAsync is called with a specific SQL procedure and
        /// entity type, it returns the correct collection of entities as expected. The test uses a mock materializer to
        /// simulate the database interaction.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_GenericSqlProcedure_ReturnsEntities()
        {
            var proc = new DummySqlProcedure();
            _materializerMock.Setup(m => m.QueryAsync<DummyEntity, DummyParameter>(proc, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entityList);
            var result = await _materializerMock.Object.QueryAsync<DummyEntity, DummyParameter>(proc);
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that querying a generic SQL procedure with a transaction asynchronously returns the expected
        /// entities.
        /// </summary>
        /// <remarks>This test ensures that the QueryAsync method, when called with a SQL procedure and a
        /// transaction, returns the correct set of entities. It uses a mock materializer to simulate the database
        /// operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_GenericSqlProcedureWithTransaction_ReturnsEntities()
        {
            var proc = new DummySqlProcedure();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.QueryAsync<DummyEntity, DummyParameter>(proc, transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entityList);
            var result = await _materializerMock.Object.QueryAsync<DummyEntity, DummyParameter>(proc, transaction);
            Assert.Single(result);
        }

        /// <summary>
        /// Verifies that calling the Query method on a disposed ISqlMaterializer instance throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <remarks>This test ensures that the Query method enforces correct object lifetime management
        /// by throwing an exception when invoked after disposal. Proper exception handling in this scenario helps
        /// prevent undefined behavior when using disposed resources.</remarks>
        [Fact]
        public void Query_ObjectDisposed_ThrowsObjectDisposedException()
        {
            _materializerMock.Setup(m => m.Query<DummyEntity>("SELECT", It.IsAny<object>()))
                .Throws(new ObjectDisposedException(nameof(ISqlMaterializer)));
            Assert.Throws<ObjectDisposedException>(() => _materializerMock.Object.Query<DummyEntity>("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that the Query method throws an ArgumentException when called with invalid arguments.
        /// </summary>
        /// <remarks>This test ensures that the Query method correctly enforces its argument validation by
        /// throwing an ArgumentException when provided with improper input. It is intended to validate error handling
        /// behavior in the data materializer implementation.</remarks>
        [Fact]
        public void Query_ArgumentException_ThrowsArgumentException()
        {
            _materializerMock.Setup(m => m.Query<DummyEntity>("SELECT", It.IsAny<object>()))
                .Throws<ArgumentException>();
            Assert.Throws<ArgumentException>(() => _materializerMock.Object.Query<DummyEntity>("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that the Query method throws a NotSupportedException when invoked under unsupported conditions.
        /// </summary>
        /// <remarks>This test ensures that the Query method correctly signals unsupported operations by
        /// throwing a NotSupportedException. Use this test to validate exception handling behavior for unsupported
        /// queries.</remarks>
        [Fact]
        public void Query_NotSupportedException_ThrowsNotSupportedException()
        {
            _materializerMock.Setup(m => m.Query<DummyEntity>("SELECT", It.IsAny<object>()))
                .Throws<NotSupportedException>();
            Assert.Throws<NotSupportedException>(() => _materializerMock.Object.Query<DummyEntity>("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that the Query method throws a MissingMemberException when a required member is missing.
        /// </summary>
        /// <remarks>This test ensures that the materializer's Query method correctly propagates a
        /// MissingMemberException when it encounters a missing member during query execution. Use this test to validate
        /// exception handling behavior for missing members in the data mapping process.</remarks>
        [Fact]
        public void Query_MissingMemberException_ThrowsMissingMemberException()
        {
            _materializerMock.Setup(m => m.Query<DummyEntity>("SELECT", It.IsAny<object>()))
                .Throws<MissingMemberException>();
            Assert.Throws<MissingMemberException>(() => _materializerMock.Object.Query<DummyEntity>("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that the Query method throws an InvalidOperationException when an invalid operation occurs during
        /// query execution.
        /// </summary>
        /// <remarks>This test ensures that the Query method correctly propagates
        /// InvalidOperationException when the underlying materializer encounters an invalid operation. Use this test to
        /// validate exception handling behavior in scenarios where the query cannot be processed as expected.</remarks>
        [Fact]
        public void Query_InvalidOperationException_ThrowsInvalidOperationException()
        {
            _materializerMock.Setup(m => m.Query<DummyEntity>("SELECT", It.IsAny<object>()))
                .Throws<InvalidOperationException>();
            Assert.Throws<InvalidOperationException>(() => _materializerMock.Object.Query<DummyEntity>("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that the QueryAsync method throws an OperationCanceledException when the operation is canceled.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task QueryAsync_OperationCanceled_ThrowsOperationCanceledException()
        {
            _materializerMock.Setup(m => m.QueryAsync<DummyEntity>("SELECT", It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _materializerMock.Object.QueryAsync<DummyEntity>("SELECT", new { }, CancellationToken.None));
        }
    }
}