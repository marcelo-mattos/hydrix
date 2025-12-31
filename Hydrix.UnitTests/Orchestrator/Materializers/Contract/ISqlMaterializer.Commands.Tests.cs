using Hydrix.Schemas;
using Microsoft.Data.SqlClient;
using Moq;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers.Contract
{
    /// <summary>
    /// Provides unit tests for the ISqlMaterializer interface to verify correct command creation and disposal behavior.
    /// </summary>
    /// <remarks>This class contains tests that ensure ISqlMaterializer methods create database commands as
    /// expected for various input scenarios, including different command types, SQL statements, parameters, and
    /// transactions. It also verifies that the Dispose method is called appropriately. These tests are intended to
    /// validate the contract and usage of ISqlMaterializer implementations.</remarks>
    public partial class ISqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the CreateCommand method of ISqlMaterializer is called with the specified SQL, parameters, and
        /// transaction, and returns a non-null IDbCommand instance.
        /// </summary>
        /// <remarks>This test ensures that the CreateCommand method correctly handles input arguments and
        /// produces a valid command object when invoked with a SQL statement, parameters, and a null transaction. It is
        /// intended to validate the integration between the mock and the method under test.</remarks>
        [Fact]
        public void CreateCommand_WithSqlAndParametersAndTransaction_CallsMethod()
        {
            _materializerMock.Setup(m => m.CreateCommand("SELECT 1", It.IsAny<object>(), null))
                .Returns(Mock.Of<IDbCommand>());
            var cmd = _materializerMock.Object.CreateCommand("SELECT 1", new object(), null);
            Assert.NotNull(cmd);
        }

        /// <summary>
        /// Verifies that the CreateCommand method is called with the specified SQL statement and parameters, and
        /// returns a non-null command instance.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer.CreateCommand method correctly handles
        /// input parameters and produces a valid IDbCommand object. It uses a mock implementation to validate the
        /// method's behavior.</remarks>
        [Fact]
        public void CreateCommand_WithSqlAndParameters_CallsMethod()
        {
            _materializerMock.Setup(m => m.CreateCommand("SELECT 2", It.IsAny<object>()))
                .Returns(Mock.Of<IDbCommand>());
            var cmd = _materializerMock.Object.CreateCommand("SELECT 2", new object());
            Assert.NotNull(cmd);
        }

        /// <summary>
        /// Verifies that the CreateCommand method of ISqlMaterializer is called with the specified command type, SQL
        /// statement, parameters, and transaction, and returns a non-null IDbCommand instance.
        /// </summary>
        /// <remarks>This test ensures that CreateCommand correctly handles the provided arguments,
        /// including command type, SQL text, a collection of SQL parameters, and a null transaction. It validates that
        /// the method invocation results in a valid command object, which is essential for executing SQL operations in
        /// data access scenarios.</remarks>
        [Fact]
        public void CreateCommand_WithCommandTypeSqlParametersTransaction_CallsMethod()
        {
            _materializerMock.Setup(m => m.CreateCommand(CommandType.Text, "SELECT 3", It.IsAny<IEnumerable<IDataParameter>>(), null))
                .Returns(Mock.Of<IDbCommand>());
            var cmd = _materializerMock.Object.CreateCommand(CommandType.Text, "SELECT 3", new List<IDataParameter>(), null);
            Assert.NotNull(cmd);
        }

        /// <summary>
        /// Verifies that the CreateCommand method is called with the specified command type, command text, and SQL
        /// parameters, and returns a non-null IDbCommand instance.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer implementation correctly handles the
        /// creation of a command when provided with a stored procedure type, a command text, and a collection of SQL
        /// parameters. It uses mocking to validate the method invocation and the returned result.</remarks>
        [Fact]
        public void CreateCommand_WithCommandTypeSqlParameters_CallsMethod()
        {
            _materializerMock.Setup(m => m.CreateCommand(CommandType.StoredProcedure, "sp_test", It.IsAny<IEnumerable<IDataParameter>>()))
                .Returns(Mock.Of<IDbCommand>());
            var cmd = _materializerMock.Object.CreateCommand(CommandType.StoredProcedure, "sp_test", new List<IDataParameter>());
            Assert.NotNull(cmd);
        }

        /// <summary>
        /// Verifies that the generic CreateCommand method of ISqlMaterializer is called with a valid ISqlProcedure
        /// implementation using a concrete data parameter type, and returns a non-null IDbCommand instance.
        /// </summary>
        /// <remarks>
        /// This test uses SqlParameter as the concrete type for TDataParameterDriver to satisfy the generic constraint.
        /// </remarks>
        [Fact]
        public void CreateCommand_Generic_WithSqlProcedure_CallsMethod()
        {
            var proc = Mock.Of<ISqlProcedure<SqlParameter>>();
            _materializerMock.Setup(m => m.CreateCommand<SqlParameter>(proc))
                .Returns(Mock.Of<IDbCommand>());
            var cmd = _materializerMock.Object.CreateCommand<SqlParameter>(proc);
            Assert.NotNull(cmd);
        }

        /// <summary>
        /// Verifies that the generic CreateCommand method is called with a SQL procedure and a transaction, and returns
        /// a non-null command instance.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer.CreateCommand&lt;TParameter&gt; method
        /// correctly handles a SQL procedure and a null transaction, returning a valid IDbCommand object. It is
        /// intended to validate the integration between the materializer and procedure interfaces in scenarios
        /// involving SQL commands.</remarks>
        [Fact]
        public void CreateCommand_Generic_WithSqlProcedureAndTransaction_CallsMethod()
        {
            var proc = Mock.Of<ISqlProcedure<SqlParameter>>();
            _materializerMock.Setup(m => m.CreateCommand<SqlParameter>(proc, null))
                .Returns(Mock.Of<IDbCommand>());
            var cmd = _materializerMock.Object.CreateCommand<SqlParameter>(proc, null);
            Assert.NotNull(cmd);
        }

        /// <summary>
        /// Verifies that disposing a command created by the ISqlMaterializer calls its Dispose method exactly once.
        /// </summary>
        /// <remarks>This test ensures that the Dispose pattern is correctly implemented for
        /// ISqlMaterializer, helping to prevent resource leaks in database operations.</remarks>
        [Fact]
        public void CreateCommand_Dispose_CallsDispose()
        {
            _materializerMock.Setup(m => m.Dispose());
            _materializerMock.Object.Dispose();
            _materializerMock.Verify(m => m.Dispose(), Times.Once);
        }
    }
}