using System;
using System.Data;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// Provides methods for managing database connections and transactions, including opening, closing, and committing
    /// or rolling back transactions.
    /// </summary>
    /// <remarks>This class implements the IMaterializer interface and is designed to handle database
    /// operations safely, ensuring that connections and transactions are managed correctly. It is important to ensure
    /// that the connection is not disposed before performing any operations.</remarks>
    public partial class Materializer :
        Contract.IMaterializer
    {
        /// <summary>
        /// Opens a database connection with the settings specified by the ConnectionString property
        /// of the provider-specific Connection object.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        public virtual void OpenConnection()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("The connection has been disposed.");

            lock (_lockConnection)
            {
                if (DbConnection.State == ConnectionState.Closed)
                    DbConnection.Open();
            }
        }

        /// <summary>
        /// Closes the connection to the database.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        public virtual void CloseConnection()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("The connection has been disposed.");

            lock (_lockConnection)
            {
                if (DbConnection.State != ConnectionState.Closed)
                    DbConnection.Close();
            }
        }

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <param name="isolationLevel">Specifies the transaction locking behavior for the connection.</param>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="InvalidOperationException">There is another active transaction.</exception>
        public virtual void BeginTransaction(IsolationLevel isolationLevel)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("The connection has been disposed.");

            lock (_lockTransaction)
            {
                if (DbTransaction != null)
                    throw new InvalidOperationException("There is another active transaction.");

                lock (_lockConnection)
                    DbTransaction = DbConnection.BeginTransaction(isolationLevel);
            }
        }

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        /// <exception cref="Exception">An error occurred while trying to commit the transaction.</exception>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="InvalidOperationException">
        /// The transaction has already been committed or rolled back. -or- The connection is broken.
        /// </exception>
        /// <exception cref="InvalidOperationException">There is no active transaction.</exception>
        public virtual void CommitTransaction()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("The connection has been disposed.");

            lock (_lockTransaction)
            {
                if (null == DbTransaction)
                    throw new InvalidOperationException("There is no active transaction.");

                DbTransaction.Commit();
                DbTransaction.Dispose();
                DbTransaction = null;
            }
        }

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        /// <exception cref="Exception">An error occurred while trying to commit the transaction.</exception>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="InvalidOperationException">
        /// The transaction has already been committed or rolled back. -or- The connection is broken.
        /// </exception>
        /// <exception cref="InvalidOperationException">There is no active transaction.</exception>
        public virtual void RollbackTransaction()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("The connection has been disposed.");

            lock (_lockTransaction)
            {
                if (null == DbTransaction)
                {
                    if (!(IsDisposing))
                        throw new InvalidOperationException("There is no active transaction.");
                    return;
                }

                DbTransaction.Rollback();
                DbTransaction.Dispose();
                DbTransaction = null;
            }
        }
    }
}