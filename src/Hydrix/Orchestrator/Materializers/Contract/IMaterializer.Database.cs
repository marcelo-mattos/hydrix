using System;
using System.Data;

namespace Hydrix.Orchestrator.Materializers.Contract
{
    /// <summary>
    /// Defines a contract for managing database connections and transactions, including opening and closing
    /// connections, and handling transactional operations such as begin, commit, and rollback.
    /// </summary>
    /// <remarks>Implementations of this interface are responsible for ensuring proper resource management and
    /// transactional integrity. Methods may throw exceptions if called in an invalid state, such as when the connection
    /// is disposed or when there is no active transaction. Users should ensure that connections are properly opened and
    /// closed, and that transactions are appropriately committed or rolled back to maintain data consistency.</remarks>
    public partial interface IMaterializer :
        IDisposable
    {
        /// <summary>
        /// Opens a database connection with the settings specified by the Connectionstring property
        /// of the provider-specific Connection object.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        void OpenConnection();

        /// <summary>
        /// Closes the connection to the database.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        void CloseConnection();

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <param name="isolationLevel">Specifies the transaction locking behavior for the connection.</param>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="InvalidOperationException">There is another active transaction.</exception>
        void BeginTransaction(IsolationLevel isolationLevel);

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        /// <exception cref="Exception">An error occurred while trying to commit the transaction.</exception>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="InvalidOperationException">
        /// The transaction has already been committed or rolled back. -or- The connection is broken.
        /// </exception>
        /// <exception cref="InvalidOperationException">There is no active transaction.</exception>
        void CommitTransaction();

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        /// <exception cref="Exception">An error occurred while trying to commit the transaction.</exception>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="InvalidOperationException">
        /// The transaction has already been committed or rolled back. -or- The connection is broken.
        /// </exception>
        /// <exception cref="InvalidOperationException">There is no active transaction.</exception>
        void RollbackTransaction();
    }
}
