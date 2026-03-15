using System;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for the Materializer.Dispose logic, verifying correct resource cleanup and state management.
    /// </summary>
    public partial class MaterializerTests
    {
        /// <summary>
        /// Verifies that calling Dispose sets the IsDisposed and IsDisposing properties to true.
        /// </summary>
        /// <remarks>This unit test ensures that the TestMaterializer correctly updates its disposal
        /// state when Dispose is invoked. It is intended to validate the object's resource management
        /// behavior.</remarks>
        [Fact]
        public void Dispose_SetsIsDisposedAndIsDisposing()
        {
            var mat = new TestMaterializerDispose();
            mat.Dispose();
            Assert.True(mat.IsDisposed);
            Assert.True(mat.IsDisposing);
        }

        /// <summary>
        /// Verifies that disposing the TestMaterializer instance calls both Rollback and Close operations.
        /// </summary>
        /// <remarks>This test ensures that the Dispose method triggers the expected cleanup actions by
        /// asserting that both RollbackCalled and CloseCalled are set to true after disposal.</remarks>
        [Fact]
        public void Dispose_CallsRollbackAndClose()
        {
            var mat = new TestMaterializerDispose();
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
            var mat = new TestMaterializerDispose();
            var conn = (TestDbConnection)mat.DbConnection;
            mat.Dispose();
            Assert.True(conn.Disposed);
            Assert.Null(mat.DbConnection);
        }

        /// <summary>
        /// Verifies that calling Dispose multiple times on a TestMaterializer instance does not perform disposal
        /// actions more than once.
        /// </summary>
        /// <remarks>This test ensures that repeated calls to Dispose do not result in multiple rollbacks
        /// or closures, and that the disposed state remains consistent after the first call.</remarks>
        [Fact]
        public void Dispose_DoesNotDisposeTwice()
        {
            var mat = new TestMaterializerDispose();
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
            var matEx = new ExceptionThrowingMaterializer();
            matEx.SetDisposed(false);

            Exception ex = Record.Exception(() => matEx.Dispose());
            Assert.True(matEx.IsDisposed);
            Assert.True(matEx.IsDisposing);
        }

        /// <summary>
        /// Verifies that invoking the protected dispose routine with <c>disposing=false</c> still marks the
        /// materializer as disposing/disposed and releases the connection.
        /// </summary>
        [Fact]
        public void DisposeCore_False_SetsStateAndDisposesConnection()
        {
            var mat = new TestMaterializerDispose();
            var conn = (TestDbConnection)mat.DbConnection;

            mat.CallDisposeCore(false);

            Assert.True(mat.IsDisposing);
            Assert.True(mat.IsDisposed);
            Assert.True(conn.Disposed);
            Assert.Null(mat.DbConnection);
        }

        /// <summary>
        /// Verifies that disposing with a null connection does not throw and marks the instance as disposed.
        /// </summary>
        [Fact]
        public void Dispose_WithNullConnection_DoesNotThrow()
        {
            var mat = new TestMaterializerDispose();
            mat.SetDbConnectionNull();

            var exception = Record.Exception(() => mat.Dispose());

            Assert.Null(exception);
            Assert.True(mat.IsDisposing);
            Assert.True(mat.IsDisposed);
            Assert.Null(mat.DbConnection);
        }
    }
}