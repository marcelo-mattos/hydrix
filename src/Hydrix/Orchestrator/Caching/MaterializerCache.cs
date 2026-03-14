using Hydrix.Configuration;
using Hydrix.Orchestrator.Materializers;
using System.Data;
using System.Runtime.CompilerServices;

namespace Hydrix.Orchestrator.Caching
{
    /// <summary>
    /// Provides a thread-safe cache for associating Materializer instances with database connections, enabling
    /// efficient reuse of materializers within the application.
    /// </summary>
    /// <remarks>MaterializerCache uses a conditional weak table to ensure that Materializer instances are
    /// retained only as long as their associated IDbConnection objects are alive. This design helps optimize resource
    /// usage and performance by reusing materializers when possible, while allowing for automatic cleanup when
    /// connections are disposed. This class is intended for internal use and is not thread-safe for external
    /// modification.</remarks>
    internal static class MaterializerCache
    {
        /// <summary>
        /// Provides a thread-safe cache that associates each IDbConnection instance with its corresponding Materializer
        /// instance, enabling efficient retrieval and management of Materializers for database connections.
        /// </summary>
        /// <remarks>This cache uses a ConditionalWeakTable to ensure that Materializer instances are
        /// automatically eligible for garbage collection when their associated IDbConnection objects are no longer
        /// referenced. This design helps prevent memory leaks by not prolonging the lifetime of connections or
        /// materializers beyond their intended use.</remarks>
        private static readonly ConditionalWeakTable<IDbConnection, Materializer> _cache =
            new ConditionalWeakTable<IDbConnection, Materializer>();

        /// <summary>
        /// Gets an existing Materializer instance associated with the specified database connection, or creates a new
        /// one if none exists.
        /// </summary>
        /// <remarks>This method uses a cache to ensure that only one Materializer instance is associated
        /// with each database connection. Reusing Materializer instances can improve performance and resource
        /// management.</remarks>
        /// <param name="connection">The open and valid database connection to associate with the Materializer. Cannot be null.</param>
        /// <param name="options">The options that configure the Materializer's behavior, including logging and command timeout settings.
        /// Cannot be null.</param>
        /// <returns>A Materializer instance configured with the provided connection and options. If a Materializer is already
        /// cached for the connection, the cached instance is returned.</returns>
        public static Materializer GetOrCreate(
            IDbConnection connection,
            HydrixOptions options)
            => _cache.GetValue(connection, conn =>
                new Materializer(
                    conn,
                    options.Logger,
                    options.CommandTimeout,
                    options.ParameterPrefix));
    }
}