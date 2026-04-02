using Hydrix.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// Provides functionality for materializing data from a database into CLR objects, handling SQL command execution,
    /// and managing database connections and transactions.
    /// </summary>
    /// <remarks>This class implements the <see cref="Contract.IMaterializer"/> interface and is designed to
    /// optimize the performance of SQL-to-entity mapping operations by caching entity metadata and ensuring thread-safe
    /// access to database resources.</remarks>
    public partial class Materializer :
        Contract.IMaterializer
    {
        /// <summary>
        /// The wait time (in seconds) before terminating the attempt to execute a command and generating an error.
        /// </summary>
        private int _timeout = HydrixOptions.DefaultTimeout;

        /// <summary>
        /// The database connection.
        /// </summary>
        private IDbConnection _dbConnection;

        /// <summary>
        /// The database transaction.
        /// </summary>
        private IDbTransaction _dbTransaction;

        /// <summary>
        /// SQL connection critical section.
        /// </summary>
        private readonly object _lockConnection = new object();

        /// <summary>
        /// SQL transaction critical section.
        /// </summary>
        private readonly object _lockTransaction = new object();

        /// <summary>
        /// The prefix used for SQL parameters.
        /// </summary>
        private readonly string _parameterPrefix = HydrixOptions.DefaultParameterPrefix;

        /// <summary>
        /// The logger instance associated with this materializer.
        /// </summary>
        [SuppressMessage(
            "Major Code Smell",
            "S1144",
            Justification = "Used indirectly by tests through reflection.")]
        private readonly ILogger _logger;
    }
}
