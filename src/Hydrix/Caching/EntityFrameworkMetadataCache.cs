using Hydrix.Metadata.EntityFramework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Hydrix.Caching
{
    /// <summary>
    /// Provides a process-wide cache for metadata translated from an Entity Framework model.
    /// </summary>
    /// <remarks>This cache is additive and exists alongside the traditional attribute-based caches.
    /// Once metadata is registered for a CLR type, the existing Hydrix caches can reuse the translated metadata
    /// without changing the runtime structures they already consume.</remarks>
    internal static class EntityFrameworkMetadataCache
    {
        /// <summary>
        /// Stores translated metadata entries indexed by CLR type.
        /// </summary>
        /// <remarks>This dictionary is shared process-wide so registrations performed during application
        /// startup can be reused by the query-building, validation, and materialization pipelines.</remarks>
        private static readonly ConcurrentDictionary<Type, RegisteredEntityMetadata> Cache
            = new ConcurrentDictionary<Type, RegisteredEntityMetadata>();

        /// <summary>
        /// Stores the current cache version.
        /// </summary>
        /// <remarks>The version is incremented whenever the cache is mutated so external hot caches can
        /// detect that a refresh is required.</remarks>
        private static int _version;

        /// <summary>
        /// Gets the current cache version.
        /// </summary>
        /// <remarks>The value is read using <see cref="Volatile"/> so callers can safely compare it with
        /// their cached snapshots.</remarks>
        public static int Version
            => Volatile.Read(ref _version);

        /// <summary>
        /// Registers the supplied metadata entries in the process-wide cache.
        /// </summary>
        /// <remarks>Existing registrations for the same CLR type are overwritten, allowing the latest
        /// Entity Framework model snapshot to win. Once registration completes, the internal version is incremented so
        /// dependent caches can refresh themselves.</remarks>
        /// <param name="entries">The translated metadata entries to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entries"/> is null.</exception>
        public static void Register(
            IEnumerable<RegisteredEntityMetadata> entries)
        {
            if (entries == null)
                throw new ArgumentNullException(nameof(entries));

            foreach (var entry in entries)
            {
                if (entry == null)
                    continue;

                Cache[entry.Type] = entry;
            }

            Interlocked.Increment(ref _version);
        }

        /// <summary>
        /// Attempts to retrieve translated metadata for the specified CLR type.
        /// </summary>
        /// <param name="type">The CLR type whose metadata should be retrieved.</param>
        /// <param name="metadata">When this method returns, contains the registered metadata for the supplied type, if found.</param>
        /// <returns><see langword="true"/> when metadata is registered for <paramref name="type"/>; otherwise,
        /// <see langword="false"/>.</returns>
        public static bool TryGet(
            Type type,
            out RegisteredEntityMetadata metadata)
            => Cache.TryGetValue(type, out metadata);

        /// <summary>
        /// Clears the cache and bumps its version.
        /// </summary>
        /// <remarks>This method is intended for internal test isolation. Production code is expected to
        /// register metadata during startup and keep it for the process lifetime.</remarks>
        internal static void Clear()
        {
            Cache.Clear();
            Interlocked.Increment(ref _version);
        }
    }
}
