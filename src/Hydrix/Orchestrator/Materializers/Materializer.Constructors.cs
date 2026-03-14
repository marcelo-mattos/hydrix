using Hydrix.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// Provides functionality to materialize data from a database using a specified connection and configuration
    /// options.
    /// </summary>
    /// <remarks>This class allows for customization of the database connection, command execution timeout,
    /// and parameter naming conventions. It is designed to facilitate data retrieval and manipulation in a structured
    /// manner.</remarks>
    public partial class Materializer :
        Contract.IMaterializer
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
        public Materializer(
            IDbConnection connection,
            int timeout = HydrixOptions.DefaultTimeout,
            string parameterPrefix = HydrixOptions.DefaultParameterPrefix) : this(
                connection,
                null,
                timeout,
                parameterPrefix)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="connection">Sets the database connection.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="timeout">
        /// The wait time (in seconds) before terminating the attempt to execute a command and
        /// generating an error.
        /// </param>
        /// <param name="parameterPrefix">
        /// The prefix to use for parameter names.
        /// </param>
        public Materializer(
            IDbConnection connection,
            ILogger logger,
            int timeout = HydrixOptions.DefaultTimeout,
            string parameterPrefix = HydrixOptions.DefaultParameterPrefix)
        {
            this._logger = logger;

            lock (this._lockConnection)
                this.DbConnection = connection;

            this.Timeout = timeout;
            this._parameterPrefix = parameterPrefix;
        }
    }
}