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
    /// Contains unit tests for verifying the behavior of ISqlMaterializer implementations, including synchronous and
    /// asynchronous SingleOrDefault operations with various SQL command and parameter combinations.
    /// </summary>
    /// <remarks>These tests ensure that ISqlMaterializer correctly handles different input scenarios,
    /// expected return values, and exception cases. The class uses mocks to simulate materializer behavior and validate
    /// contract compliance, including proper exception throwing for invalid operations and cancellation
    /// scenarios.</remarks>
    public partial class ISqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when called with a SQL query and
        /// parameters.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly retrieves a single entity or null
        /// when executing a parameterized SQL query. It uses a mock to simulate the expected behavior of the data
        /// access layer.</remarks>
        [Fact]
        public void SingleOrDefault_WithSqlAndParameters_ReturnsEntity()
        {
            _materializerMock.Setup(m => m.SingleOrDefault<DummyEntity>("SELECT", It.IsAny<object>()))
                .Returns(_entity);
            var result = _materializerMock.Object.SingleOrDefault<DummyEntity>("SELECT", new { Id = 1 });
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when called with a SQL query,
        /// parameters, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the SingleOrDefault method correctly handles SQL parameters
        /// and an explicit transaction, returning the appropriate entity instance when found.</remarks>
        [Fact]
        public void SingleOrDefault_WithSqlParametersAndTransaction_ReturnsEntity()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.SingleOrDefault<DummyEntity>("SELECT", It.IsAny<object>(), transaction))
                .Returns(_entity);
            var result = _materializerMock.Object.SingleOrDefault<DummyEntity>("SELECT", new { Id = 2 }, transaction);
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when called with a SQL query string.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly returns the entity when executing a
        /// SQL-only query using SingleOrDefault. It uses a mock to simulate the expected behavior.</remarks>
        [Fact]
        public void SingleOrDefault_WithSqlOnly_ReturnsEntity()
        {
            _materializerMock.Setup(m => m.SingleOrDefault<DummyEntity>("SELECT"))
                .Returns(_entity);
            var result = _materializerMock.Object.SingleOrDefault<DummyEntity>("SELECT");
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when called with a SQL query and a
        /// transaction.
        /// </summary>
        /// <remarks>This unit test ensures that the materializer correctly retrieves a single entity or
        /// the default value when provided with a specific SQL statement and transaction context.</remarks>
        [Fact]
        public void SingleOrDefault_WithSqlAndTransaction_ReturnsEntity()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.SingleOrDefault<DummyEntity>("SELECT", transaction))
                .Returns(_entity);
            var result = _materializerMock.Object.SingleOrDefault<DummyEntity>("SELECT", transaction);
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when called with a command type of
        /// Text, a SQL query, and a list of parameters.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly retrieves a single entity or the
        /// default value when provided with specific command type, SQL, and parameters. It uses a mock to simulate the
        /// expected behavior of the materializer.</remarks>
        [Fact]
        public void SingleOrDefault_WithCommandTypeSqlAndParameters_ReturnsEntity()
        {
            var parameters = new List<IDataParameter>();
            _materializerMock.Setup(m => m.SingleOrDefault<DummyEntity>(CommandType.Text, "SELECT", parameters))
                .Returns(_entity);
            var result = _materializerMock.Object.SingleOrDefault<DummyEntity>(CommandType.Text, "SELECT", parameters);
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when called with a command type, SQL
        /// statement, parameters, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly retrieves a single entity or the
        /// default value when provided with specific command type, SQL, parameters, and transaction context.</remarks>
        [Fact]
        public void SingleOrDefault_WithCommandTypeSqlParametersAndTransaction_ReturnsEntity()
        {
            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.SingleOrDefault<DummyEntity>(CommandType.Text, "SELECT", parameters, transaction))
                .Returns(_entity);
            var result = _materializerMock.Object.SingleOrDefault<DummyEntity>(CommandType.Text, "SELECT", parameters, transaction);
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when called with a specific command
        /// type and SQL statement.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly retrieves a single entity or the
        /// default value when provided with a command type and SQL query. It uses a mock to simulate the expected
        /// behavior.</remarks>
        [Fact]
        public void SingleOrDefault_WithCommandTypeAndSql_ReturnsEntity()
        {
            _materializerMock.Setup(m => m.SingleOrDefault<DummyEntity>(CommandType.Text, "SELECT"))
                .Returns(_entity);
            var result = _materializerMock.Object.SingleOrDefault<DummyEntity>(CommandType.Text, "SELECT");
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method returns the expected entity when called with a SQL command type, a
        /// SQL statement, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the SingleOrDefault method correctly retrieves an entity when
        /// provided with specific command type, SQL, and transaction parameters. It uses a mock materializer to
        /// simulate the expected behavior.</remarks>
        [Fact]
        public void SingleOrDefault_WithCommandTypeSqlAndTransaction_ReturnsEntity()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.SingleOrDefault<DummyEntity>(CommandType.Text, "SELECT", transaction))
                .Returns(_entity);
            var result = _materializerMock.Object.SingleOrDefault<DummyEntity>(CommandType.Text, "SELECT", transaction);
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the generic SingleOrDefault method for SQL procedures returns the expected entity when invoked
        /// with a dummy procedure and parameters.
        /// </summary>
        /// <remarks>This test ensures that the mocked materializer returns the correct entity instance
        /// when SingleOrDefault is called with specific generic type arguments. It validates the behavior of the data
        /// access layer when executing a SQL procedure expected to return a single entity or null.</remarks>
        [Fact]
        public void SingleOrDefault_GenericSqlProcedure_ReturnsEntity()
        {
            var proc = new DummySqlProcedure();
            _materializerMock.Setup(m => m.SingleOrDefault<DummyEntity, DummyParameter>(proc))
                .Returns(_entity);
            var result = _materializerMock.Object.SingleOrDefault<DummyEntity, DummyParameter>(proc);
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the generic SQL procedure materializer returns the expected entity when executed within a
        /// transaction.
        /// </summary>
        /// <remarks>This test ensures that the SingleOrDefault method correctly retrieves an entity when
        /// provided with a SQL procedure and a transaction. It uses a mock materializer to simulate the expected
        /// behavior.</remarks>
        [Fact]
        public void SingleOrDefault_GenericSqlProcedureWithTransaction_ReturnsEntity()
        {
            var proc = new DummySqlProcedure();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.SingleOrDefault<DummyEntity, DummyParameter>(proc, transaction))
                .Returns(_entity);
            var result = _materializerMock.Object.SingleOrDefault<DummyEntity, DummyParameter>(proc, transaction);
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when called with a SQL query and
        /// parameters.
        /// </summary>
        /// <remarks>This test ensures that the SingleOrDefaultAsync method correctly materializes and
        /// returns an entity when provided with a specific SQL statement and parameter object. It uses a mock
        /// materializer to simulate the database operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithSqlAndParameters_ReturnsEntity()
        {
            _materializerMock.Setup(m => m.SingleOrDefaultAsync<DummyEntity>("SELECT", It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entity);
            var result = await _materializerMock.Object.SingleOrDefaultAsync<DummyEntity>("SELECT", new { Id = 1 });
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when called with SQL parameters
        /// and a database transaction.
        /// </summary>
        /// <remarks>This test ensures that the SingleOrDefaultAsync method correctly retrieves an entity
        /// when provided with a SQL query, parameter object, and transaction. It uses a mock materializer to simulate
        /// database behavior.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithSqlParametersAndTransaction_ReturnsEntity()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.SingleOrDefaultAsync<DummyEntity>("SELECT", It.IsAny<object>(), transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entity);
            var result = await _materializerMock.Object.SingleOrDefaultAsync<DummyEntity>("SELECT", new { Id = 2 }, transaction);
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when called with a SQL query only.
        /// </summary>
        /// <remarks>This test ensures that the SingleOrDefaultAsync method, when provided with a SQL
        /// query string, returns the correct entity instance as expected. It uses a mock materializer to simulate the
        /// database operation.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithSqlOnly_ReturnsEntity()
        {
            _materializerMock.Setup(m => m.SingleOrDefaultAsync<DummyEntity>("SELECT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entity);
            var result = await _materializerMock.Object.SingleOrDefaultAsync<DummyEntity>("SELECT");
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when called with a SQL query and a
        /// database transaction.
        /// </summary>
        /// <remarks>This test ensures that the SingleOrDefaultAsync method correctly retrieves a single
        /// entity or null when provided with a specific SQL statement and transaction context. It uses a mock
        /// materializer to simulate database behavior.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithSqlAndTransaction_ReturnsEntity()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.SingleOrDefaultAsync<DummyEntity>("SELECT", transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entity);
            var result = await _materializerMock.Object.SingleOrDefaultAsync<DummyEntity>("SELECT", transaction);
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Tests that the SingleOrDefaultAsync method returns the expected entity when called with CommandType.Text, a
        /// SQL query, and a list of parameters.
        /// </summary>
        /// <remarks>This test verifies that the SingleOrDefaultAsync method correctly retrieves a single
        /// entity or the default value when provided with a specific command type, SQL statement, and parameters. It
        /// uses a mock materializer to simulate the data retrieval.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithCommandTypeSqlAndParameters_ReturnsEntity()
        {
            var parameters = new List<IDataParameter>();
            _materializerMock.Setup(m => m.SingleOrDefaultAsync<DummyEntity>(CommandType.Text, "SELECT", parameters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entity);
            var result = await _materializerMock.Object.SingleOrDefaultAsync<DummyEntity>(CommandType.Text, "SELECT", parameters);
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when called with a command type,
        /// SQL parameters, and a transaction.
        /// </summary>
        /// <remarks>This test ensures that the SingleOrDefaultAsync method correctly retrieves an entity
        /// when provided with specific command type, SQL parameters, and a database transaction.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithCommandTypeSqlParametersAndTransaction_ReturnsEntity()
        {
            var parameters = new List<IDataParameter>();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.SingleOrDefaultAsync<DummyEntity>(CommandType.Text, "SELECT", parameters, transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entity);
            var result = await _materializerMock.Object.SingleOrDefaultAsync<DummyEntity>(CommandType.Text, "SELECT", parameters, transaction);
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when called with a specific
        /// command type and SQL statement.
        /// </summary>
        /// <remarks>This test ensures that the SingleOrDefaultAsync method correctly retrieves a single
        /// entity or the default value when provided with CommandType.Text and a SQL query. It uses a mocked
        /// materializer to simulate the data retrieval.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithCommandTypeAndSql_ReturnsEntity()
        {
            _materializerMock.Setup(m => m.SingleOrDefaultAsync<DummyEntity>(CommandType.Text, "SELECT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entity);
            var result = await _materializerMock.Object.SingleOrDefaultAsync<DummyEntity>(CommandType.Text, "SELECT");
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when called with a SQL command
        /// type and an explicit transaction.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly retrieves a single entity or the
        /// default value when executing a SQL command within a transaction context.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_WithCommandTypeSqlAndTransaction_ReturnsEntity()
        {
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.SingleOrDefaultAsync<DummyEntity>(CommandType.Text, "SELECT", transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entity);
            var result = await _materializerMock.Object.SingleOrDefaultAsync<DummyEntity>(CommandType.Text, "SELECT", transaction);
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when invoked with a generic SQL
        /// procedure.
        /// </summary>
        /// <remarks>This unit test uses a mock materializer to simulate the execution of a generic SQL
        /// procedure and asserts that the returned entity matches the expected result.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_GenericSqlProcedure_ReturnsEntity()
        {
            var proc = new DummySqlProcedure();
            _materializerMock.Setup(m => m.SingleOrDefaultAsync<DummyEntity, DummyParameter>(proc, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entity);
            var result = await _materializerMock.Object.SingleOrDefaultAsync<DummyEntity, DummyParameter>(proc);
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that the SingleOrDefaultAsync method returns the expected entity when called with a generic SQL
        /// procedure and a transaction.
        /// </summary>
        /// <remarks>This unit test ensures that the materializer correctly retrieves a single entity or
        /// null when executing a generic SQL procedure within a transaction context.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_GenericSqlProcedureWithTransaction_ReturnsEntity()
        {
            var proc = new DummySqlProcedure();
            var transaction = new Mock<IDbTransaction>().Object;
            _materializerMock.Setup(m => m.SingleOrDefaultAsync<DummyEntity, DummyParameter>(proc, transaction, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_entity);
            var result = await _materializerMock.Object.SingleOrDefaultAsync<DummyEntity, DummyParameter>(proc, transaction);
            Assert.Same(_entity, result);
        }

        /// <summary>
        /// Verifies that calling SingleOrDefault on a disposed ISqlMaterializer instance throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <remarks>This test ensures that the SingleOrDefault method enforces correct object lifetime
        /// management by throwing an ObjectDisposedException when invoked after the materializer has been
        /// disposed.</remarks>
        [Fact]
        public void SingleOrDefault_ObjectDisposed_ThrowsObjectDisposedException()
        {
            _materializerMock.Setup(m => m.SingleOrDefault<DummyEntity>("SELECT", It.IsAny<object>()))
                .Throws(new ObjectDisposedException(nameof(ISqlMaterializer)));
            Assert.Throws<ObjectDisposedException>(() => _materializerMock.Object.SingleOrDefault<DummyEntity>("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method throws an ArgumentException when invoked with invalid arguments.
        /// </summary>
        /// <remarks>This test ensures that the SingleOrDefault method correctly propagates an
        /// ArgumentException when the underlying materializer encounters invalid input. Use this test to validate
        /// proper exception handling in error scenarios.</remarks>
        [Fact]
        public void SingleOrDefault_ArgumentException_ThrowsArgumentException()
        {
            _materializerMock.Setup(m => m.SingleOrDefault<DummyEntity>("SELECT", It.IsAny<object>()))
                .Throws<ArgumentException>();
            Assert.Throws<ArgumentException>(() => _materializerMock.Object.SingleOrDefault<DummyEntity>("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method throws a NotSupportedException when invoked under unsupported
        /// conditions.
        /// </summary>
        /// <remarks>This test ensures that the materializer's SingleOrDefault method correctly throws a
        /// NotSupportedException when it is not supported, helping to validate error handling behavior.</remarks>
        [Fact]
        public void SingleOrDefault_NotSupportedException_ThrowsNotSupportedException()
        {
            _materializerMock.Setup(m => m.SingleOrDefault<DummyEntity>("SELECT", It.IsAny<object>()))
                .Throws<NotSupportedException>();
            Assert.Throws<NotSupportedException>(() => _materializerMock.Object.SingleOrDefault<DummyEntity>("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method throws a MissingMemberException when the required member is
        /// missing.
        /// </summary>
        /// <remarks>This test ensures that the materializer correctly propagates a MissingMemberException
        /// when attempting to retrieve a single entity and the expected member is not present. Use this test to
        /// validate exception handling behavior for missing members in the data mapping process.</remarks>
        [Fact]
        public void SingleOrDefault_MissingMemberException_ThrowsMissingMemberException()
        {
            _materializerMock.Setup(m => m.SingleOrDefault<DummyEntity>("SELECT", It.IsAny<object>()))
                .Throws<MissingMemberException>();
            Assert.Throws<MissingMemberException>(() => _materializerMock.Object.SingleOrDefault<DummyEntity>("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that the SingleOrDefault method throws an InvalidOperationException when an invalid operation
        /// occurs.
        /// </summary>
        /// <remarks>This test ensures that the SingleOrDefault method correctly propagates an
        /// InvalidOperationException when the underlying materializer encounters an invalid operation. Use this test to
        /// validate exception handling behavior in scenarios where the query cannot be completed as expected.</remarks>
        [Fact]
        public void SingleOrDefault_InvalidOperationException_ThrowsInvalidOperationException()
        {
            _materializerMock.Setup(m => m.SingleOrDefault<DummyEntity>("SELECT", It.IsAny<object>()))
                .Throws<InvalidOperationException>();
            Assert.Throws<InvalidOperationException>(() => _materializerMock.Object.SingleOrDefault<DummyEntity>("SELECT", new { }));
        }

        /// <summary>
        /// Verifies that SingleOrDefaultAsync throws an OperationCanceledException when the operation is canceled.
        /// </summary>
        /// <remarks>This test ensures that the SingleOrDefaultAsync method correctly propagates an
        /// OperationCanceledException when the underlying operation is canceled. It uses a mocked materializer to
        /// simulate the cancellation scenario.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task SingleOrDefaultAsync_OperationCanceled_ThrowsOperationCanceledException()
        {
            _materializerMock.Setup(m => m.SingleOrDefaultAsync<DummyEntity>("SELECT", It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _materializerMock.Object.SingleOrDefaultAsync<DummyEntity>("SELECT", new { }, CancellationToken.None));
        }
    }
}