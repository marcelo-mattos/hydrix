using Hydrix.Configuration;
using Microsoft.Extensions.Logging;
using System;
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
#pragma warning disable S1133
    [Obsolete("This class is deprecated. Please use the extension methods in HydrixDataCore class.")]
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
            int? timeout = null,
            string parameterPrefix = null) : this(
                connection,
                null,
                timeout ?? HydrixConfiguration.Options.CommandTimeout,
                parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix)
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
            int? timeout = null,
            string parameterPrefix = null)
        {
            _logger = logger;

            lock (_lockConnection)
                DbConnection = connection;

            Timeout = timeout ?? HydrixConfiguration.Options.CommandTimeout;
            _parameterPrefix = parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix;
        }
    }
#pragma warning restore S1133
}
