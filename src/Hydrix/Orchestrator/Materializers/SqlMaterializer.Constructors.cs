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
        /// Constructor.
        /// </summary>
        /// <param name="connection">Sets the database connection.</param>
        /// <param name="timeout">
        /// The wait time (in seconds) before terminating the attempt to execute a command and
        /// generating an error.
        /// </param>
        /// <param name="parameterPrefix">
        /// The prefix to use for parameter names.
        /// </param>
        public SqlMaterializer(
            IDbConnection connection,
            int timeout = DefaultTimeout,
            string parameterPrefix = DefaultParameterPrefix)
        {
            lock (this._lockConnection)
                this.DbConnection = connection;

            this.Timeout = timeout;
            this._parameterPrefix = parameterPrefix;
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~SqlMaterializer()
        {
            this.Dispose(false);
        }
    }
}