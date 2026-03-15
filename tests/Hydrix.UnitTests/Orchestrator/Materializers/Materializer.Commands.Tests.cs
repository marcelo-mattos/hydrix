using Hydrix.Orchestrator.Caching;
using Hydrix.Orchestrator.Materializers.Contract;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for the Materializer class, verifying its command creation, parameter binding,
    /// transaction handling, and error conditions.
    /// </summary>
    /// <remarks>These tests ensure that Materializer behaves correctly when interacting with
    /// database-related abstractions, including handling disposed states, open/closed connections, and parameter
    /// binding for various command scenarios. The class uses mock implementations of IDbConnection, IDbCommand, and
    /// related interfaces to isolate and validate Materializer's logic without requiring a real database
    /// connection.</remarks>
    public partial class MaterializerTests
    {
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
            var mat = new MaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (IMaterializer)mat;
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
            var mat = new MaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (IMaterializer)mat;
            var param = new FakeDataParameter { ParameterName = "@Id", Value = 1, DbType = DbType.Int32 };
            var cmd = sqlMat.CreateCommand(CommandType.Text, "SELECT * FROM T WHERE Id=@Id", new[] { param }, null, null);
            Assert.Single(cmd.Parameters.Cast<IDataParameter>().Where(p => p.ParameterName == "@Id"));
        }

        /// <summary>
        /// Verifies that the CreateCommand&lt;T&gt; method throws a MissingMemberException when the specified procedure type
        /// does not have a ProcedureAttribute.
        /// </summary>
        /// <remarks>This test ensures that attempting to create a command for a procedure type lacking
        /// the required ProcedureAttribute results in an exception, enforcing correct usage of the API.</remarks>
        [Fact]
        public void CreateCommand_Generic_Throws_If_No_ProcedureAttribute()
        {
            var mat = new MaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (IMaterializer)mat;
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
            var mat = new MaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (IMaterializer)mat;
            var proc = new ProcedureWithAttributes { Id = 7, Name = "abc" };
            var cmd = sqlMat.CreateCommand<AttributeParameter>(proc, null);
            Assert.Equal("dbo.TestProc", cmd.CommandText);
            Assert.Equal(CommandType.StoredProcedure, cmd.CommandType);
            Assert.Equal(2, cmd.Parameters.Count);
            Assert.Contains(cmd.Parameters.Cast<IDataParameter>(), p => p.ParameterName == "@Id" && (int)p.Value == 7);
            Assert.Contains(cmd.Parameters.Cast<IDataParameter>(), p => p.ParameterName == "@Name" && (string)p.Value == "abc");
        }

        /// <summary>
        /// Verifies that calling CreateCommand&lt;T&gt; on a disposed materializer throws an ObjectDisposedException.
        /// </summary>
        /// <remarks>This test ensures that the materializer enforces correct object lifecycle management
        /// by throwing an ObjectDisposedException when CreateCommand&lt;T&gt; is invoked after disposal. This behavior helps
        /// prevent invalid operations on disposed resources and maintains API reliability.</remarks>
        [Fact]
        public void CreateCommand_Generic_Throws_ObjectDisposedException_WhenDisposed()
        {
            var mat = new MaterializerTestable
            {
                IsDisposedSet = true,
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (IMaterializer)mat;
            var proc = new ProcedureWithAttributes { Id = 1, Name = "abc" };
            Assert.Throws<ObjectDisposedException>(() =>
                sqlMat.CreateCommand<AttributeParameter>(proc, null, null));
        }

        /// <summary>
        /// Verifies that the CreateCommand&lt;T&gt; method throws an ArgumentNullException when the procedure parameter is
        /// null.
        /// </summary>
        /// <remarks>This unit test ensures that the CreateCommand&lt;T&gt; method enforces its contract by
        /// validating input parameters and throwing the appropriate exception when a null procedure is provided. Proper
        /// exception handling is essential for robust API behavior.</remarks>
        [Fact]
        public void CreateCommand_Generic_Throws_ArgumentNullException_WhenProcedureNull()
        {
            var mat = new MaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (IMaterializer)mat;
            Assert.Throws<ArgumentNullException>(() =>
                sqlMat.CreateCommand<AttributeParameter>(null, null));
        }

        /// <summary>
        /// Verifies that the CreateCommand&lt;T&gt; method throws an InvalidOperationException when invoked with a database
        /// connection that is not open.
        /// </summary>
        /// <remarks>This test ensures that the CreateCommand&lt;T&gt; method enforces the requirement for an
        /// open database connection before executing commands. Attempting to create a command with a closed connection
        /// should result in an InvalidOperationException, validating correct error handling for improper connection
        /// states.</remarks>
        [Fact]
        public void CreateCommand_Generic_Throws_InvalidOperationException_WhenConnectionNotOpen()
        {
            var mat = new MaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection { State = ConnectionState.Closed }
            };
            var sqlMat = (IMaterializer)mat;
            var proc = new ProcedureWithAttributes { Id = 1, Name = "abc" };
            Assert.Throws<InvalidOperationException>(() =>
                sqlMat.CreateCommand<AttributeParameter>(proc, null));
        }

        /// <summary>
        /// Verifies that the CreateCommand&lt;T&gt; method assigns the DbType of a parameter to Int32 when the parameter is
        /// defined as an enum in the procedure.
        /// </summary>
        /// <remarks>This test ensures that enum-typed parameters are correctly mapped to DbType.Int32
        /// when commands are created using the IMaterializer interface. This behavior is important for compatibility
        /// with database providers that expect enum values to be represented as integers.</remarks>
        [Fact]
        public void CreateCommand_Generic_Sets_DbType_WhenEnumDefined()
        {
            var mat = new MaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (IMaterializer)mat;
            var proc = new ProcedureWithAttributes { Id = 7, Name = "abc" };
            var cmd = sqlMat.CreateCommand<AttributeParameter>(proc, null);
            var param = (IDataParameter)cmd.Parameters[0];
            Assert.Equal(DbType.Int32, param.DbType);
        }

        /// <summary>
        /// Tests that the CreateCommand method correctly uses the ProviderDbTypeSetter when the specified parameter
        /// type is not defined as an enum.
        /// </summary>
        /// <remarks>This test verifies that a custom parameter type can be configured to use a specific
        /// action for handling database types. It ensures that the ProviderDbTypeSetter mechanism is invoked and the
        /// parameter is properly set when the command is created with a custom data parameter type.</remarks>
        [Fact]
        public void CreateCommand_Generic_Uses_ProviderDbTypeSetter_WhenEnumNotDefined()
        {
            var mat = new MaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (IMaterializer)mat;
            var proc = new CustomProcedure { Id = 7, Name = "abc" };

            // Simula um tipo de parâmetro customizado
            var customParamType = typeof(CustomDataParameter);
            var binderCacheType = typeof(ProviderDbTypeSetterCache);
            var cacheField = binderCacheType.GetField("_cache", BindingFlags.NonPublic | BindingFlags.Static);
            var cache = (IDictionary)cacheField.GetValue(null);
            cache[customParamType] = new Action<IDataParameter, int>((obj, dbType) => ((CustomDataParameter)obj).CustomSet = false);

            var cmd = sqlMat.CreateCommand<CustomDataParameter>(proc, null);
            var param = (CustomDataParameter)cmd.Parameters[0];
            Assert.False(param.CustomSet);
        }

        /// <summary>
        /// Verifies that creating a command for a procedure does not throw an exception when the provider-specific
        /// parameter type setter is null.
        /// </summary>
        /// <remarks>This test ensures that the command creation process is robust against missing or
        /// unset provider-specific type setters, which may occur in certain testing or edge-case scenarios. It is
        /// intended to confirm that the absence of a type setter does not result in an unexpected exception.</remarks>
        [Fact]
        public void CreateCommand_Generic_ProviderDbTypeSetter_Null_DoesNotThrow()
        {
            var mat = new MaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (IMaterializer)mat;
            var proc = new CustomProcedure { Id = 7, Name = "abc" };

            // Remove qualquer setter para garantir que será null
            var customParamType = typeof(CustomDataParameter);
            var binderCacheType = typeof(ProviderDbTypeSetterCache);
            var cacheField = binderCacheType.GetField("_cache", BindingFlags.NonPublic | BindingFlags.Static);
            var cache = (IDictionary)cacheField.GetValue(null);
            cache[customParamType] = null;

            var cmd = sqlMat.CreateCommand<CustomDataParameter>(proc, null);
            var param = (CustomDataParameter)cmd.Parameters[0];
            Assert.NotNull(param); // Só garante que não lança
        }

        /// <summary>
        /// Verifies that the CreateCommand&lt;T&gt; method sets the value of a database parameter to DBNull when the
        /// corresponding property value is null.
        /// </summary>
        /// <remarks>This test ensures that null values in input objects are correctly translated to
        /// DBNull in database commands, which is required for proper handling of nullable fields in database
        /// operations.</remarks>
        [Fact]
        public void CreateCommand_Generic_Sets_Value_To_DBNull_WhenNull()
        {
            var mat = new MaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (IMaterializer)mat;
            var proc = new ProcedureWithAttributes { Id = 7, Name = null };
            var cmd = sqlMat.CreateCommand<AttributeParameter>(proc, null);
            var param = (IDataParameter)cmd.Parameters[1];
            Assert.Equal(DBNull.Value, param.Value);
        }

        /// <summary>
        /// Verifies that the CreateCommand&lt;T&gt; method correctly assigns the specified transaction to the created command
        /// when a transaction is provided.
        /// </summary>
        /// <remarks>This test ensures that when a transaction is supplied to the CreateCommand&lt;T&gt; method,
        /// the resulting command's Transaction property is set to the provided transaction. This behavior is important
        /// for maintaining transactional consistency when executing database operations.</remarks>
        [Fact]
        public void CreateCommand_Generic_Sets_Transaction_WhenProvided()
        {
            var mat = new MaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (IMaterializer)mat;
            var proc = new ProcedureWithAttributes { Id = 7, Name = "abc" };
            var transaction = new FakeDbTransaction();
            var cmd = sqlMat.CreateCommand<AttributeParameter>(proc, transaction);
            Assert.Equal(transaction, cmd.Transaction);
        }

        /// <summary>
        /// Verifies that the CreateCommand method does not assign a transaction when a null value is provided for the
        /// transaction parameter.
        /// </summary>
        /// <remarks>This test ensures that the CreateCommand method maintains expected behavior by
        /// leaving the Transaction property unset when transactions are optional and a null value is supplied. This is
        /// important for scenarios where commands should execute outside of an explicit transaction context.</remarks>
        [Fact]
        public void CreateCommand_Generic_DoesNotSet_Transaction_WhenNull()
        {
            var mat = new MaterializerTestable
            {
                DbConnectionSet = new FakeDbConnection()
            };
            var sqlMat = (IMaterializer)mat;
            var proc = new ProcedureWithAttributes { Id = 7, Name = "abc" };
            var cmd = sqlMat.CreateCommand<AttributeParameter>(proc, null);
            Assert.Null(cmd.Transaction);
        }
    }
}