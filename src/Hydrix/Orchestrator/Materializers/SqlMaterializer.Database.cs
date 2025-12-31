using System;
using System.Data;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// SQL Data Handler Class
    /// </summary>
    public partial class SqlMaterializer :
        Contract.ISqlMaterializer
    {
        /// <summary>
        /// Opens a database connection with the settings specified by the ConnectionString property
        /// of the provider-specific Connection object.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        public void OpenConnection()
        {
            if (this.IsDisposed)
                throw new ObjectDisposedException("The connection has been disposed.");

            lock (this._lockConnection)
            {
                if (this.DbConnection.State == ConnectionState.Closed)
                    this.DbConnection.Open();
            }
        }

        /// <summary>
        /// Closes the connection to the database.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        public void CloseConnection()
        {
            if (this.IsDisposed)
                throw new ObjectDisposedException("The connection has been disposed.");

            lock (this._lockConnection)
                this.DbConnection.Close();
        }

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <param name="isolationLevel">Specifies the transaction locking behavior for the connection.</param>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="InvalidOperationException">There is another active transaction.</exception>
        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            if (this.IsDisposed)
                throw new ObjectDisposedException("The connection has been disposed.");

            lock (this._lockTransaction)
            {
                if (null != this.DbTransaction)
                    throw new InvalidOperationException("There is another active transaction.");

                lock (this._lockConnection)
                    this.DbTransaction = this.DbConnection.BeginTransaction(isolationLevel);
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
        public void CommitTransaction()
        {
            if (this.IsDisposed)
                throw new ObjectDisposedException("The connection has been disposed.");

            lock (this._lockTransaction)
            {
                if (null == this.DbTransaction)
                    throw new InvalidOperationException("There is no active transaction.");

                this.DbTransaction.Commit();
                this.DbTransaction.Dispose();
                this.DbTransaction = null;
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
        public void RollbackTransaction()
        {
            if (this.IsDisposed)
                throw new ObjectDisposedException("The connection has been disposed.");

            lock (this._lockTransaction)
            {
                if (null == this.DbTransaction)
                {
                    if (!(this.IsDisposing))
                        throw new InvalidOperationException("There is no active transaction.");
                    return;
                }

                this.DbTransaction.Rollback();
                this.DbTransaction.Dispose();
                this.DbTransaction = null;
            }
        }
    }
}