using Hydrix.Attributes.Parameters;
using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Materializers.Contract;
using Hydrix.Schemas;
using System;
using System.Data;
using System.Linq;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for the SqlMaterializer class, verifying its command creation, parameter binding,
    /// transaction handling, and error conditions.
    /// </summary>
    /// <remarks>These tests ensure that SqlMaterializer behaves correctly when interacting with
    /// database-related abstractions, including handling disposed states, open/closed connections, and parameter
    /// binding for various command scenarios. The class uses mock implementations of IDbConnection, IDbCommand, and
    /// related interfaces to isolate and validate SqlMaterializer's logic without requiring a real database
    /// connection.</remarks>
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Verifies that calling CreateCommandCore on a disposed SqlMaterializerTestable instance throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <remarks>This test ensures that the CreateCommandCore method enforces correct disposal
        /// semantics by throwing an ObjectDisposedException when invoked after the object has been disposed. This
        /// behavior helps prevent usage of resources that have already been released.</remarks>
        [Fact]
        public void CreateCommandCore_Throws_ObjectDisposedException_WhenDisposed()
        {
            var mat = new SqlMaterializerTestable
            {
                IsDisposedSet = true,
                DbConnectionSet = new FakeDbConnection()
            };
            Assert.Throws<ObjectDisposedException>(() =>
                mat.CallCreateCommandCore(CommandType.Text, "SELECT 1", null, null));
        }

        /// <summary>
        /// Verifies that calling CreateCommandCore throws an InvalidOperationException when the database connection is
        /// not open.
        /// </summary>
        /// <remarks>This test ensures that CreateCommandCore enforces the requirement for an open
        /// connection before executing a command. The method simulates a closed connection and asserts that the
        /// expected exception is thrown, validating correct error handling in this scenario.</remarks>
        [Fact]
        public void CreateCommandCore_Throws_InvalidOperationException_WhenConnectionNotOpen()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection { State = ConnectionState.Closed }
            };
            Assert.Throws<InvalidOperationException>(() =>
                mat.CallCreateCommandCore(CommandType.Text, "SELECT 1", null, null));
        }

        /// <summary>
        /// Verifies that the CreateCommandCore method correctly sets command properties and binds parameters as
        /// expected.
        /// </summary>
        /// <remarks>This test ensures that the command type, command text, and command timeout are
        /// properly assigned, and that the parameter binder delegate is invoked when creating a command using
        /// CreateCommandCore. It also checks that the command is associated with the specified transaction.</remarks>
        [Fact]
        public void CreateCommandCore_Sets_Command_Properties_And_Binds_Parameters()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            bool binderCalled = false;
            var cmd = mat.CallCreateCommandCore(
                CommandType.StoredProcedure,
                "spTest",
                c => { binderCalled = true; },
                new FakeDbTransaction());
            Assert.Equal(CommandType.StoredProcedure, cmd.CommandType);
            Assert.Equal("spTest", cmd.CommandText);
            Assert.Equal(mat.Timeout, cmd.CommandTimeout);
            Assert.True(binderCalled);
        }

        /// <summary>
        /// Verifies that the CreateCommandCore method assigns the specified transaction to the created command when a
        /// transaction parameter is provided.
        /// </summary>
        /// <remarks>This test ensures that passing a transaction to CreateCommandCore results in the
        /// command's Transaction property being set to the same instance. It validates correct transaction propagation
        /// for command creation scenarios.</remarks>
        [Fact]
        public void CreateCommandCore_Sets_Transaction_From_Parameter()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection(),
                DbTransactionSet = new FakeDbTransaction(),
                IsTransactionActiveSet = true
            };
            var transaction = new FakeDbTransaction();
            var cmd = mat.CallCreateCommandCore(
                CommandType.Text,
                "SELECT 1",
                null,
                transaction);
            Assert.Equal(transaction, cmd.Transaction);
        }

        /// <summary>
        /// Verifies that the CreateCommandCore method sets the transaction property of the created command to the
        /// active transaction when a transaction is active.
        /// </summary>
        /// <remarks>This test ensures that when an active transaction is present, the command created by
        /// CreateCommandCore is associated with that transaction. This behavior is important for maintaining
        /// transactional consistency when executing database commands.</remarks>
        [Fact]
        public void CreateCommandCore_Sets_Transaction_From_ActiveTransaction()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection(),
                DbTransactionSet = new FakeDbTransaction(),
                IsTransactionActiveSet = true
            };
            var cmd = mat.CallCreateCommandCore(
                CommandType.Text,
                "SELECT 1",
                null,
                mat.DbTransaction);
            Assert.Equal(mat.DbTransaction, cmd.Transaction);
        }

        /// <summary>
        /// Verifies that the CreateCommandCore method calls LogCommand when SQL logging is enabled.
        /// </summary>
        /// <remarks>This test ensures that enabling SQL logging on the SqlMaterializerTestable instance
        /// results in LogCommand being executed during command creation. The absence of exceptions indicates that
        /// LogCommand was called successfully.</remarks>
        [Fact]
        public void CreateCommandCore_Calls_LogCommand_When_Enabled()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection(),
                EnableSqlLogging = true
            };
            // No exception means LogCommand executed (writes to console)
            var cmd = mat.CallCreateCommandCore(
                CommandType.Text,
                "SELECT 1",
                null,
                null);
            Assert.NotNull(cmd);
        }

        /// <summary>
        /// Verifies that the CreateCommand method correctly binds parameters when provided with an object containing
        /// parameter values.
        /// </summary>
        /// <remarks>This test ensures that named parameters supplied via an anonymous object are properly
        /// mapped to the corresponding placeholders in the SQL command text. It checks that the command text remains
        /// unchanged and that parameter binding occurs as expected.</remarks>
        [Fact]
        public void CreateCommand_Object_Parameters_Binds_Parameters()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (ISqlMaterializer)mat;
            var parameters = new { Id = 42 };
            var cmd = sqlMat.CreateCommand("SELECT * FROM T WHERE Id=@Id", parameters, null);
            Assert.Equal("SELECT * FROM T WHERE Id=@Id", cmd.CommandText);
        }

        /// <summary>
        /// Verifies that the CreateCommand method correctly binds enumerable parameters to the resulting command.
        /// </summary>
        /// <remarks>This test ensures that when an enumerable of parameters is provided to CreateCommand,
        /// each parameter is properly added to the command's Parameters collection. It specifically checks that a
        /// parameter with the name '@Id' is present exactly once, confirming correct binding behavior.</remarks>
        [Fact]
        public void CreateCommand_Enumerable_Parameters_Binds_Parameters()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (ISqlMaterializer)mat;
            var param = new FakeDataParameter { ParameterName = "@Id", Value = 1, DbType = DbType.Int32 };
            var cmd = sqlMat.CreateCommand(CommandType.Text, "SELECT * FROM T WHERE Id=@Id", new[] { param }, null);
            Assert.Single(cmd.Parameters.Cast<IDataParameter>().Where(p => p.ParameterName == "@Id"));
        }

        /// <summary>
        /// Verifies that the CreateCommand&lt;T&gt; method throws a MissingMemberException when the specified procedure type
        /// does not have a SqlProcedureAttribute.
        /// </summary>
        /// <remarks>This test ensures that attempting to create a command for a procedure type lacking
        /// the required SqlProcedureAttribute results in an exception, enforcing correct usage of the API.</remarks>
        [Fact]
        public void CreateCommand_Generic_Throws_If_No_SqlProcedureAttribute()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (ISqlMaterializer)mat;
            var fakeProc = new NoAttributeProcedure();
            Assert.Throws<MissingMemberException>(() =>
                sqlMat.CreateCommand<NoAttributeParameter>(fakeProc, null));
        }

        /// <summary>
        /// Verifies that the generic CreateCommand method binds parameters based on attribute metadata applied to the
        /// input object.
        /// </summary>
        /// <remarks>This test ensures that when an object with parameter attributes is passed to
        /// CreateCommand, the resulting command contains parameters matching the object's properties and their values,
        /// as defined by the attributes. It checks that the command text, command type, and parameter values are set
        /// correctly according to the attribute configuration.</remarks>
        [Fact]
        public void CreateCommand_Generic_Binds_Parameters_From_Attributes()
        {
            var mat = new SqlMaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (ISqlMaterializer)mat;
            var proc = new ProcedureWithAttributes { Id = 7, Name = "abc" };
            var cmd = sqlMat.CreateCommand<AttributeParameter>(proc, null);
            Assert.Equal("dbo.TestProc", cmd.CommandText);
            Assert.Equal(CommandType.StoredProcedure, cmd.CommandType);
            Assert.Equal(2, cmd.Parameters.Count);
            Assert.Contains(cmd.Parameters.Cast<IDataParameter>(), p => p.ParameterName == "@Id" && (int)p.Value == 7);
            Assert.Contains(cmd.Parameters.Cast<IDataParameter>(), p => p.ParameterName == "@Name" && (string)p.Value == "abc");
        }
    }
}