using System;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// SQL Data Handler Class
    /// </summary>
    public partial class SqlMaterializer :
        Contract.ISqlMaterializer
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

                // Dispose unmanaged state (unmanaged objects).
                try { this.RollbackTransaction(); } catch { }
                try { this.CloseConnection(); } catch { }

                lock (this._lockConnection)
                {
                    this.DbConnection?.Dispose();
                    this.DbConnection = null;
                }

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