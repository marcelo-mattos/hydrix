using Hydrix.Orchestrator.Materializers;
using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for the SqlMaterializer.Dispose logic, verifying correct resource cleanup and state management.
    /// </summary>
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Verifies that calling Dispose sets the IsDisposed and IsDisposing properties to true.
        /// </summary>
        /// <remarks>This unit test ensures that the TestSqlMaterializer correctly updates its disposal
        /// state when Dispose is invoked. It is intended to validate the object's resource management
        /// behavior.</remarks>
        [Fact]
        public void Dispose_SetsIsDisposedAndIsDisposing()
        {
            var mat = new TestSqlMaterializerDispose();
            mat.Dispose();
            Assert.True(mat.IsDisposed);
            Assert.True(mat.IsDisposing);
        }

        /// <summary>
        /// Verifies that disposing the TestSqlMaterializer instance calls both Rollback and Close operations.
        /// </summary>
        /// <remarks>This test ensures that the Dispose method triggers the expected cleanup actions by
        /// asserting that both RollbackCalled and CloseCalled are set to true after disposal.</remarks>
        [Fact]
        public void Dispose_CallsRollbackAndClose()
        {
            var mat = new TestSqlMaterializerDispose();
            mat.Dispose();
            Assert.True(mat.RollbackCalled);
            Assert.True(mat.CloseCalled);
        }

        /// <summary>
        /// Verifies that disposing the materializer disposes the underlying database connection and sets the
        /// DbConnection property to null.
        /// </summary>
        /// <remarks>This test ensures that resource cleanup is performed correctly when the materializer
        /// is disposed. It checks that the associated database connection is properly disposed and that the reference
        /// to the connection is cleared.</remarks>
        [Fact]
        public void Dispose_DisposesDbConnectionAndSetsNull()
        {
            var mat = new TestSqlMaterializerDispose();
            var conn = (TestDbConnection)mat.DbConnection;
            mat.Dispose();
            Assert.True(conn.Disposed);
            Assert.Null(mat.DbConnection);
        }

        /// <summary>
        /// Verifies that calling Dispose multiple times on a TestSqlMaterializer instance does not perform disposal
        /// actions more than once.
        /// </summary>
        /// <remarks>This test ensures that repeated calls to Dispose do not result in multiple rollbacks
        /// or closures, and that the disposed state remains consistent after the first call.</remarks>
        [Fact]
        public void Dispose_DoesNotDisposeTwice()
        {
            var mat = new TestSqlMaterializerDispose();
            mat.Dispose();
            var disposedState = mat.IsDisposed;
            mat.RollbackCalled = false;
            mat.CloseCalled = false;
            mat.Dispose();
            Assert.True(mat.IsDisposed);
            Assert.True(disposedState);
            Assert.False(mat.RollbackCalled);
            Assert.False(mat.CloseCalled);
        }

        /// <summary>
        /// Verifies that the Dispose method correctly handles exceptions thrown during transaction rollback and
        /// connection close operations.
        /// </summary>
        /// <remarks>This test ensures that Dispose sets the object's disposal state even if exceptions
        /// occur in RollbackTransaction or CloseConnection. It validates that IsDisposed and IsDisposing are set to
        /// true after disposal, regardless of exceptions.</remarks>
        [Fact]
        public void Dispose_HandlesExceptionsInRollbackAndClose()
        {
            var matEx = new ExceptionThrowingSqlMaterializer();
            matEx.SetDisposed(false);

            Exception ex = Record.Exception(() => matEx.Dispose());
            Assert.True(matEx.IsDisposed);
            Assert.True(matEx.IsDisposing);
        }
    }
}