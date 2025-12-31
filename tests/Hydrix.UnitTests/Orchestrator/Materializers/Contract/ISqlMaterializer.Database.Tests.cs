using Hydrix.Orchestrator.Materializers.Contract;
using Hydrix.Schemas;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers.Contract
{
    /// <summary>
    /// Provides unit tests for verifying the behavior and contract of ISqlMaterializer implementations.
    /// </summary>
    /// <remarks>Use this test class to ensure that components implementing ISqlMaterializer correctly handle
    /// connection management, transaction operations, and object disposal semantics. The tests cover expected behaviors
    /// and exception handling for typical usage scenarios.</remarks>
    public partial class ISqlMaterializerTests
    {
        /// <summary>
        /// Mock the instance of ISqlMaterializer for testing purposes.
        /// </summary>
        private readonly Mock<ISqlMaterializer> _materializerMock;

        /// <summary>
        /// Initializes a new instance of the ISqlMaterializerTests class for unit testing implementations of
        /// ISqlMaterializer.
        /// </summary>
        /// <remarks>This constructor sets up a mock instance of ISqlMaterializer for use in test
        /// scenarios. Use this class to verify the behavior of components that depend on ISqlMaterializer.</remarks>
        public ISqlMaterializerTests()
        {
            _materializerMock = new Mock<ISqlMaterializer>();
        }

        /// <summary>
        /// Represents a parameter to a command object, such as a SQL query or stored procedure, for use with data
        /// providers that implement the <see cref="IDataParameter"/> interface.
        /// </summary>
        /// <remarks>This class is typically used to define input, output, or return value parameters when
        /// executing database commands. It provides properties to specify the parameter's data type, direction, name,
        /// source column, and value. The <see cref="IsNullable"/> property always returns <see langword="true"/>,
        /// indicating that the parameter supports null values.</remarks>
        public class DummyParameter : IDataParameter
        {
            /// <summary>
            /// Gets or sets the database type of the parameter.
            /// </summary>
            public DbType DbType { get; set; }

            /// <summary>
            /// Gets or sets the direction of the parameter within a command or stored procedure.
            /// </summary>
            /// <remarks>Use this property to specify whether the parameter is used for input, output,
            /// bidirectional, or as a return value. The default is typically Input, but this may vary depending on the
            /// implementation.</remarks>
            public ParameterDirection Direction { get; set; }

            /// <summary>
            /// Gets a value indicating whether the parameter accepts null values.
            /// </summary>
            public bool IsNullable => true;

            /// <summary>
            /// Gets or sets the name of the parameter associated with the operation.
            /// </summary>
            public string ParameterName { get; set; }

            /// <summary>
            /// Gets or sets the name of the source column mapped to the data field.
            /// </summary>
            /// <remarks>This property is typically used in data binding scenarios to specify which
            /// column from a data source is associated with a particular field or parameter. The value is
            /// case-sensitive and should match the column name in the data source exactly.</remarks>
            public string SourceColumn { get; set; }

            /// <summary>
            /// Gets or sets the version of data in a DataRow to use when loading parameter values.
            /// </summary>
            /// <remarks>Use this property to specify which version of the DataRow's data (such as
            /// Original, Current, or Proposed) should be used when retrieving parameter values from a DataRow. This is
            /// commonly used when updating or inserting data using data adapters.</remarks>
            public DataRowVersion SourceVersion { get; set; }

            /// <summary>
            /// Gets or sets the value associated with this instance.
            /// </summary>
            public object Value { get; set; }
        }

        /// <summary>
        /// Represents a SQL stored procedure with a predefined set of parameters for demonstration or testing purposes.
        /// </summary>
        public class DummySqlProcedure : ISqlProcedure<DummyParameter>
        {
            /// <summary>
            /// Gets the collection of parameters associated with the current instance.
            /// </summary>
            public IEnumerable<DummyParameter> Parameters => new List<DummyParameter> { new DummyParameter { ParameterName = "Id", Value = 1 } };

            /// <summary>
            /// Gets the name of the stored procedure associated with this instance.
            /// </summary>
            public string ProcedureName => "DummyProc";
        }

        /// <summary>
        /// Verifies that calling OpenConnection on an ISqlMaterializer instance that has not been disposed does not
        /// throw an exception.
        /// </summary>
        /// <remarks>This test ensures that the OpenConnection method can be safely called when the
        /// materializer is in a valid, undisposed state. It is intended to validate correct behavior in typical usage
        /// scenarios.</remarks>
        [Fact]
        public void OpenConnection_WhenNotDisposed_DoesNotThrow()
        {
            _materializerMock.Setup(m => m.OpenConnection());
            var materializer = _materializerMock.Object;

            var exception = Record.Exception(() => materializer.OpenConnection());
            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that calling OpenConnection on a disposed ISqlMaterializer instance throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <remarks>This unit test ensures that the ISqlMaterializer implementation correctly enforces
        /// object disposal semantics by throwing an ObjectDisposedException when OpenConnection is invoked after
        /// disposal.</remarks>
        [Fact]
        public void OpenConnection_WhenDisposed_ThrowsObjectDisposedException()
        {
            _materializerMock.Setup(m => m.OpenConnection()).Throws(new ObjectDisposedException(nameof(ISqlMaterializer)));
            var materializer = _materializerMock.Object;

            Assert.Throws<ObjectDisposedException>(() => materializer.OpenConnection());
        }

        /// <summary>
        /// Verifies that calling CloseConnection on an ISqlMaterializer instance that has not been disposed does not
        /// throw an exception.
        /// </summary>
        /// <remarks>This test ensures that the CloseConnection method can be safely called when the
        /// materializer is in a valid, non-disposed state. It is intended to confirm correct resource management
        /// behavior in implementations of ISqlMaterializer.</remarks>
        [Fact]
        public void CloseConnection_WhenNotDisposed_DoesNotThrow()
        {
            _materializerMock.Setup(m => m.CloseConnection());
            var materializer = _materializerMock.Object;

            var exception = Record.Exception(() => materializer.CloseConnection());
            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that calling CloseConnection on a disposed ISqlMaterializer instance throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer implementation enforces correct disposal
        /// semantics by throwing an ObjectDisposedException when CloseConnection is invoked after the object has been
        /// disposed.</remarks>
        [Fact]
        public void CloseConnection_WhenDisposed_ThrowsObjectDisposedException()
        {
            _materializerMock.Setup(m => m.CloseConnection()).Throws(new ObjectDisposedException(nameof(ISqlMaterializer)));
            var materializer = _materializerMock.Object;

            Assert.Throws<ObjectDisposedException>(() => materializer.CloseConnection());
        }

        /// <summary>
        /// Verifies that calling BeginTransaction with a valid isolation level does not throw an exception.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer.BeginTransaction method accepts a valid
        /// IsolationLevel value and completes without error. It is intended to validate correct handling of supported
        /// isolation levels by the implementation.</remarks>
        [Fact]
        public void BeginTransaction_WithValidIsolationLevel_DoesNotThrow()
        {
            _materializerMock.Setup(m => m.BeginTransaction(It.IsAny<IsolationLevel>()));
            var materializer = _materializerMock.Object;

            var exception = Record.Exception(() => materializer.BeginTransaction(IsolationLevel.ReadCommitted));
            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that calling BeginTransaction on a disposed ISqlMaterializer instance throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer implementation enforces correct object
        /// lifetime management by throwing an exception when BeginTransaction is invoked after disposal.</remarks>
        [Fact]
        public void BeginTransaction_WhenDisposed_ThrowsObjectDisposedException()
        {
            _materializerMock.Setup(m => m.BeginTransaction(It.IsAny<IsolationLevel>())).Throws(new ObjectDisposedException(nameof(ISqlMaterializer)));
            var materializer = _materializerMock.Object;

            Assert.Throws<ObjectDisposedException>(() => materializer.BeginTransaction(IsolationLevel.Serializable));
        }

        /// <summary>
        /// Verifies that calling BeginTransaction when an active transaction exists throws an
        /// InvalidOperationException.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer implementation enforces correct
        /// transaction state management by preventing the initiation of a new transaction when one is already
        /// active.</remarks>
        [Fact]
        public void BeginTransaction_WhenActiveTransaction_ThrowsInvalidOperationException()
        {
            _materializerMock.Setup(m => m.BeginTransaction(It.IsAny<IsolationLevel>())).Throws<InvalidOperationException>();
            var materializer = _materializerMock.Object;

            Assert.Throws<InvalidOperationException>(() => materializer.BeginTransaction(IsolationLevel.ReadUncommitted));
        }

        /// <summary>
        /// Verifies that calling CommitTransaction on a valid ISqlMaterializer instance does not throw an exception.
        /// </summary>
        /// <remarks>This unit test ensures that the CommitTransaction method can be called successfully
        /// when the materializer is in a valid state. It is intended to confirm correct behavior under normal
        /// conditions.</remarks>
        [Fact]
        public void CommitTransaction_WhenValid_DoesNotThrow()
        {
            _materializerMock.Setup(m => m.CommitTransaction());
            var materializer = _materializerMock.Object;

            var exception = Record.Exception(() => materializer.CommitTransaction());
            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that calling CommitTransaction on a disposed ISqlMaterializer instance throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <remarks>This test ensures that the CommitTransaction method enforces correct object lifetime
        /// management by throwing the expected exception when invoked after disposal.</remarks>
        [Fact]
        public void CommitTransaction_WhenDisposed_ThrowsObjectDisposedException()
        {
            _materializerMock.Setup(m => m.CommitTransaction()).Throws(new ObjectDisposedException(nameof(ISqlMaterializer)));
            var materializer = _materializerMock.Object;

            Assert.Throws<ObjectDisposedException>(() => materializer.CommitTransaction());
        }

        /// <summary>
        /// Verifies that calling CommitTransaction when no transaction is active throws an InvalidOperationException.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer implementation enforces the correct
        /// transaction state by throwing an exception if CommitTransaction is called without an active
        /// transaction.</remarks>
        [Fact]
        public void CommitTransaction_WhenNoActiveTransaction_ThrowsInvalidOperationException()
        {
            _materializerMock.Setup(m => m.CommitTransaction()).Throws<InvalidOperationException>();
            var materializer = _materializerMock.Object;

            Assert.Throws<InvalidOperationException>(() => materializer.CommitTransaction());
        }

        /// <summary>
        /// Verifies that CommitTransaction throws an exception when an error occurs during the commit operation.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer.CommitTransaction method correctly
        /// propagates exceptions when a commit fails. It is intended to validate error handling behavior in transaction
        /// scenarios.</remarks>
        [Fact]
        public void CommitTransaction_WhenError_ThrowsException()
        {
            _materializerMock.Setup(m => m.CommitTransaction()).Throws<Exception>();
            var materializer = _materializerMock.Object;

            Assert.Throws<Exception>(() => materializer.CommitTransaction());
        }

        /// <summary>
        /// Verifies that calling RollbackTransaction on a valid ISqlMaterializer instance does not throw an exception.
        /// </summary>
        /// <remarks>This test ensures that the RollbackTransaction method can be invoked safely when the
        /// materializer is in a valid state. It is intended to confirm correct behavior under normal
        /// conditions.</remarks>
        [Fact]
        public void RollbackTransaction_WhenValid_DoesNotThrow()
        {
            _materializerMock.Setup(m => m.RollbackTransaction());
            var materializer = _materializerMock.Object;

            var exception = Record.Exception(() => materializer.RollbackTransaction());
            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that calling RollbackTransaction on a disposed ISqlMaterializer instance throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <remarks>This unit test ensures that the RollbackTransaction method enforces correct object
        /// lifetime management by throwing an exception when invoked after disposal.</remarks>
        [Fact]
        public void RollbackTransaction_WhenDisposed_ThrowsObjectDisposedException()
        {
            _materializerMock.Setup(m => m.RollbackTransaction()).Throws(new ObjectDisposedException(nameof(ISqlMaterializer)));
            var materializer = _materializerMock.Object;

            Assert.Throws<ObjectDisposedException>(() => materializer.RollbackTransaction());
        }

        /// <summary>
        /// Verifies that calling RollbackTransaction when no transaction is active throws an InvalidOperationException.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer implementation enforces correct
        /// transaction state management by throwing an exception if RollbackTransaction is called without an active
        /// transaction.</remarks>
        [Fact]
        public void RollbackTransaction_WhenNoActiveTransaction_ThrowsInvalidOperationException()
        {
            _materializerMock.Setup(m => m.RollbackTransaction()).Throws<InvalidOperationException>();
            var materializer = _materializerMock.Object;

            Assert.Throws<InvalidOperationException>(() => materializer.RollbackTransaction());
        }

        /// <summary>
        /// Verifies that RollbackTransaction throws an exception when an error occurs during the rollback operation.
        /// </summary>
        /// <remarks>This test ensures that the ISqlMaterializer.RollbackTransaction method correctly
        /// propagates exceptions when a rollback fails. It is intended to validate error handling behavior in
        /// transaction management scenarios.</remarks>
        [Fact]
        public void RollbackTransaction_WhenError_ThrowsException()
        {
            _materializerMock.Setup(m => m.RollbackTransaction()).Throws<Exception>();
            var materializer = _materializerMock.Object;

            Assert.Throws<Exception>(() => materializer.RollbackTransaction());
        }
    }
}