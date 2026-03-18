using System;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// Provides methods for managing the lifecycle of resources, including the disposal of unmanaged resources.
    /// </summary>
    /// <remarks>This class implements the IMaterializer interface and is responsible for ensuring that
    /// resources are properly released when no longer needed. It is important to call the Dispose method to free
    /// resources deterministically.</remarks>
    public partial class Materializer :
        Contract.IMaterializer
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">Flag to determine if the managed objects needs to be released.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!(this.IsDisposed))
            {
                this.IsDisposing = true;

                // Dispose managed state (managed objects).
                if (disposing)
                { }

                this.IsDisposed = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}