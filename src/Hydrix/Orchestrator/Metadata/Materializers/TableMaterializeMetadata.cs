using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Resolvers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Hydrix.Orchestrator.Metadata.Materializers
{
    /// <summary>
    /// Provides metadata describing how to materialize an entity from a data source, including mappings for scalar
    /// fields and nested entities.
    /// </summary>
    /// <remarks>This class is used to facilitate the mapping of data from a data source to entity properties,
    /// supporting both simple scalar fields and complex nested entities. It enables efficient and accurate
    /// materialization of object graphs by defining how each property should be populated. Instances of this class are
    /// typically constructed by a metadata builder using reflection and are intended to be reused across multiple
    /// mapping operations for performance and consistency.</remarks>
    internal sealed class TableMaterializeMetadata
    {
        /// <summary>
        /// Stores resolved table bindings indexed by the schema hash value.
        /// </summary>
        /// <remarks>This dictionary enables efficient retrieval of table binding information based on a
        /// computed schema hash. It is thread-safe for concurrent read and write operations.</remarks>
        private readonly ConcurrentDictionary<int, ResolvedTableBindings> _bindingsBySchemaHash =
            new ConcurrentDictionary<int, ResolvedTableBindings>();

        /// <summary>
        /// Tracks the number of cached schema-bound binding plans without using dictionary count on hot paths.
        /// </summary>
        private int _bindingsCacheSize;

        /// <summary>
        /// Defines the maximum number of cached schema-bound binding plans.
        /// </summary>
        /// <remarks>When the cap is reached, new binding plans are materialized on demand without being added to the
        /// cache, preventing unbounded memory growth in long-running processes with high schema variation.</remarks>
        private const int MaxBindingsCacheSize = 256;

        /// <summary>
        /// Gets the collection of column mappings that define the structure of the data.
        /// </summary>
        /// <remarks>The returned collection is read-only and reflects the mapping between data fields and
        /// their corresponding database columns. The collection is initialized when the object is constructed and
        /// cannot be modified directly.</remarks>
        public IReadOnlyList<ColumnMap> Fields { get; private set; }

        /// <summary>
        /// Gets the collection of table mappings that represent the entities managed by the context.
        /// </summary>
        /// <remarks>Each table mapping defines how an entity is associated with a database table,
        /// including schema and relationship information. The collection is read-only and reflects the entities
        /// currently tracked by the context.</remarks>
        public IReadOnlyList<TableMap> Entities { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TableMaterializeMetadata class using the specified fields and entities.
        /// </summary>
        /// <remarks>Both fields and entities are required to accurately describe the structure and
        /// context for table materialization. Supplying null for either parameter will result in an error.</remarks>
        /// <param name="fields">A read-only list of ColumnMap objects that defines the fields to be materialized. This parameter cannot be
        /// null.</param>
        /// <param name="entities">A read-only list of TableMap objects that specifies the entities associated with the materialization
        /// process. This parameter cannot be null.</param>
        public TableMaterializeMetadata(
            IReadOnlyList<ColumnMap> fields,
            IReadOnlyList<TableMap> entities)
        {
            Fields = fields;
            Entities = entities;
        }

        /// <summary>
        /// Retrieves a cached schema-bound binding plan or materializes it once for future reuse.
        /// </summary>
        public ResolvedTableBindings GetOrAddBindings(
            int schemaHash,
            Func<int, ResolvedTableBindings> factory)
        {
            if (_bindingsBySchemaHash.TryGetValue(
                schemaHash,
                out var cachedBindings))
            {
                return cachedBindings;
            }

            var currentBindings = factory(schemaHash);

            if (TryReserveBindingsCacheSlot())
            {
                if (_bindingsBySchemaHash.TryAdd(
                    schemaHash,
                    currentBindings))
                {
                    return currentBindings;
                }

                Interlocked.Decrement(ref _bindingsCacheSize);
            }

            return _bindingsBySchemaHash.TryGetValue(
                schemaHash,
                out cachedBindings)
                ? cachedBindings
                : currentBindings;
        }

        /// <summary>
        /// Attempts to retrieve a cached schema-bound binding plan.
        /// </summary>
        public bool TryGetBindings(
            int schemaHash,
            out ResolvedTableBindings bindings)
            => _bindingsBySchemaHash.TryGetValue(
                schemaHash,
                out bindings);

        /// <summary>
        /// Attempts to reserve a cache slot while enforcing the maximum bindings cache size under concurrency.
        /// </summary>
        /// <returns><see langword="true"/> when a cache slot is reserved; otherwise, <see langword="false"/>.</returns>
        private bool TryReserveBindingsCacheSlot()
            => TryReserveBindingsCacheSlotCore(
                () => Volatile.Read(ref _bindingsCacheSize),
                TryUpdateBindingsCacheSize);

        /// <summary>
        /// Attempts to reserve a cache slot using pluggable read and update delegates.
        /// </summary>
        /// <param name="readCacheSize">Delegate used to read the current cache size.</param>
        /// <param name="tryUpdate">Delegate used to atomically attempt the cache size update.</param>
        /// <returns><see langword="true"/> when a cache slot is reserved; otherwise, <see langword="false"/>.</returns>
        private static bool TryReserveBindingsCacheSlotCore(
            Func<int> readCacheSize,
            Func<int, int, bool> tryUpdate)
        {
            while (true)
            {
                var currentSize = readCacheSize();
                if (currentSize >= MaxBindingsCacheSize)
                    return false;

                var updatedSize = currentSize + 1;
                if (tryUpdate(
                    currentSize,
                    updatedSize))
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Attempts to update bindings cache size atomically for a single reservation attempt.
        /// </summary>
        /// <param name="currentSize">The expected current cache size value.</param>
        /// <param name="updatedSize">The cache size value to set when the expected value matches.</param>
        /// <returns><see langword="true"/> when the update succeeds; otherwise, <see langword="false"/>.</returns>
        private bool TryUpdateBindingsCacheSize(
            int currentSize,
            int updatedSize)
            => Interlocked.CompareExchange(
                ref _bindingsCacheSize,
                updatedSize,
                currentSize) == currentSize;
    }
}
