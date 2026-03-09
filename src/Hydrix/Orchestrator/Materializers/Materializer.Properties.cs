using System;
using System.Data;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// Provides functionality for managing database connections and transactions, including connection state,
    /// transaction control, command timeout, and SQL logging options.
    /// </summary>
    /// <remarks>The Materializer class implements the IMaterializer interface and is responsible for handling
    /// database operations such as opening and closing connections, managing transactions, and configuring command
    /// execution settings. It also offers options for logging SQL statements, which can assist with debugging and
    /// auditing. Thread safety is ensured for connection and transaction access. Dispose-related properties indicate
    /// the resource management state of the instance.</remarks>
    public partial class Materializer :
        Contract.IMaterializer
    {
        /// <summary>
        /// Gets the database connection.
        /// </summary>
        public IDbConnection DbConnection
        {
            get
            {
                lock (this._lockConnection)
                    return this._dbConnection;
            }
            private set
            {
                lock (this._lockConnection)
                    this._dbConnection = value;
            }
        }

        /// <summary>
        /// Get the database transaction.
        /// </summary>
        internal IDbTransaction DbTransaction
        {
            get
            {
                lock (this._lockTransaction)
                    return this._dbTransaction;
            }
            private set
            {
                lock (this._lockTransaction)
                    this._dbTransaction = value;
            }
        }

        /// <summary>
        /// Gets a value that determines if a transaction exists and it is active on database.
        /// </summary>
        public Boolean IsTransactionActive
        {
            get
            {
                lock (this._lockTransaction)
                    return this.DbTransaction != null;
            }
        }

        /// <summary>
        /// Gets the database connection string.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        public string ConnectionString
        {
            get
            {
                if (this.IsDisposed)
                    throw new ObjectDisposedException("The connection has been disposed.");

                lock (this._lockConnection)
                    return this.DbConnection.ConnectionString;
            }
        }

        /// <summary>
        /// Gets the database connection state.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        public ConnectionState State
        {
            get
            {
                if (this.IsDisposed)
                    throw new ObjectDisposedException("The connection has been disposed.");

                lock (this._lockConnection)
                    return this.DbConnection.State;
            }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and
        /// generating an error.
        /// </summary>
        /// <returns>
        /// The time (in seconds) to wait for the command to execute. The default value is 30 seconds.
        /// </returns>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        public int Timeout
        {
            get => this._timeout;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("The property value assigned is less than 0.");

                this._timeout = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether SQL statements are logged during database operations.
        /// </summary>
        /// <remarks>
        /// When enabled, all executed SQL queries are written to the application's log output. This
        /// can be useful for debugging or auditing purposes, but may expose sensitive information
        /// and impact performance in production environments.
        /// </remarks>
        public bool EnableSqlLogging { get; set; } = true;

        /// <summary>
        /// Gets a value that determines freeing, releasing, or resetting condition of unmanaged resources.
        /// </summary>
        public Boolean IsDisposed { get; private set; } = default;

        /// <summary>
        /// Gets a value that determines freeing, releasing, or resetting condition of unmanaged resources.
        /// </summary>
        public Boolean IsDisposing { get; private set; } = default;
    }
}