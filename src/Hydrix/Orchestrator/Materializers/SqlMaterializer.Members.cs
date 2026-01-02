using Hydrix.Orchestrator.Metadata;
using System;
using System.Collections.Concurrent;
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
        /// The wait time (in seconds) before terminating the attempt to execute a command and generating an error.
        /// </summary>
        private int _timeout = DefaultTimeout;

        /// <summary>
        /// The database connection.
        /// </summary>
        private IDbConnection _dbConnection = default;

        /// <summary>
        /// The database transaction.
        /// </summary>
        private IDbTransaction _dbTransaction = default;

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
        private readonly string _parameterPrefix = DefaultParameterPrefix;

        /// <summary>
        /// Caches SQL entity metadata by CLR type to avoid repeated reflection analysis.
        /// </summary>
        /// <remarks>
        /// This cache stores <see cref="SqlEntityMetadata"/> instances indexed by the
        /// corresponding entity <see cref="Type"/>.
        ///
        /// Each metadata entry is built once via the metadata builder and reused across
        /// all subsequent SQL-to-entity mapping operations, dramatically reducing
        /// reflection overhead and improving performance when processing large result sets.
        ///
        /// The cache is implemented using <see cref="ConcurrentDictionary{TKey, TValue}"/>
        /// to ensure thread-safe access and lazy initialization in multi-threaded
        /// execution environments.
        /// </remarks>
        private static readonly ConcurrentDictionary<Type, SqlEntityMetadata> _entityMetadataCache
            = new ConcurrentDictionary<Type, SqlEntityMetadata>();
    }
}