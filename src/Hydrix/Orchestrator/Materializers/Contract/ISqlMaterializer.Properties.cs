using System;
using System.Data;

namespace Hydrix.Orchestrator.Materializers.Contract
{
    /// <summary>
    /// SQL Data Handler Interface
    /// </summary>
    public partial interface ISqlMaterializer :
        IDisposable
    {
        /// <summary>
        /// Gets a value that determines if a transaction exists and it is active on database.
        /// </summary>
        Boolean IsTransactionActive { get; }

        /// <summary>
        /// Gets the database connection string.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        string ConnectionString { get; }

        /// <summary>
        /// Gets the database connection state.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        ConnectionState State { get; }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and
        /// generating an error.
        /// </summary>
        /// <returns>
        /// The time (in seconds) to wait for the command to execute. The default value is 30 seconds.
        /// </returns>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        int Timeout { get; set; }

        /// <summary>
        /// Gets a value that determines freeing, releasing, or resetting condition of unmanaged resources.
        /// </summary>
        Boolean IsDisposed { get; }

        /// <summary>
        /// Gets a value that determines freeing, releasing, or resetting condition of unmanaged resources.
        /// </summary>
        Boolean IsDisposing { get; }
    }
}