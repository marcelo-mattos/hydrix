using Hydrix.Mapping;
using Hydrix.Resolvers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Hydrix.Metadata.Materializers
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
        /// Each entry is wrapped in a Lazy to guarantee the factory executes exactly once per schema hash,
        /// even under concurrent access, preventing redundant delegate compilation during warm-up.
        /// </summary>
        private readonly ConcurrentDictionary<int, Lazy<ResolvedTableBindings>> _bindingsBySchemaHash =
            new ConcurrentDictionary<int, Lazy<ResolvedTableBindings>>();

        /// <summary>
        /// Stores the most recently reused binding plan for fast schema matches.
        /// </summary>
        private ResolvedTableBindings _hotBindings;

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
        public IReadOnlyList<ColumnMap> Fields { get; private set; }

        /// <summary>
        /// Gets the collection of table mappings that represent the entities managed by the context.
        /// </summary>
        public IReadOnlyList<TableMap> Entities { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TableMaterializeMetadata class using the specified fields and entities.
        /// </summary>
        /// <param name="fields">A read-only list of ColumnMap objects that defines the fields to be materialized.</param>
        /// <param name="entities">A read-only list of TableMap objects that specifies the entities associated with the materialization
        /// process.</param>
        public TableMaterializeMetadata(
            IReadOnlyList<ColumnMap> fields,
            IReadOnlyList<TableMap> entities)
        {
            Fields = fields;
            Entities = entities;
        }

        /// <summary>
        /// Retrieves a cached schema-bound binding plan or materializes it exactly once for future reuse.
        /// </summary>
        /// <remarks>Uses <see cref="Lazy{T}"/> with <see cref="LazyThreadSafetyMode.ExecutionAndPublication"/>
        /// to guarantee the factory is called at most once per schema hash, even under high concurrency.
        /// This prevents redundant expression-tree compilation during parallel warm-up.</remarks>
        /// <param name="schemaHash">The schema hash that uniquely identifies the column layout of the current data reader.</param>
        /// <param name="factory">A delegate invoked at most once to build the binding plan when no cached entry exists.</param>
        /// <returns>The cached or newly built <see cref="ResolvedTableBindings"/> for the given schema hash.</returns>
        public ResolvedTableBindings GetOrAddBindings(
            int schemaHash,
            Func<int, ResolvedTableBindings> factory)
        {
            if (_bindingsBySchemaHash.TryGetValue(
                schemaHash,
                out var existingLazy))
            {
                var cached = existingLazy.Value;
                RememberBindings(cached);
                return cached;
            }

            if (!TryReserveBindingsCacheSlot())
            {
                // Over capacity — build without caching, single execution guaranteed by caller context.
                var uncached = factory(schemaHash);
                RememberBindings(uncached);
                return uncached;
            }

            var newLazy = new Lazy<ResolvedTableBindings>(
                () => factory(schemaHash),
                LazyThreadSafetyMode.ExecutionAndPublication);

            var winningLazy = _bindingsBySchemaHash.GetOrAdd(schemaHash, newLazy);

            if (!ReferenceEquals(winningLazy, newLazy))
            {
                // Lost the GetOrAdd race — roll back the reserved slot.
                Interlocked.Decrement(ref _bindingsCacheSize);
            }

            var bindings = winningLazy.Value;
            RememberBindings(bindings);
            return bindings;
        }

        /// <summary>
        /// Attempts to retrieve a cached schema-bound binding plan.
        /// </summary>
        /// <param name="schemaHash">The schema hash that uniquely identifies the column layout to look up.</param>
        /// <param name="bindings">When this method returns, contains the cached binding plan if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a cached binding plan was found for the given schema hash; otherwise, <see langword="false"/>.</returns>
        public bool TryGetBindings(
            int schemaHash,
            out ResolvedTableBindings bindings)
        {
            if (_bindingsBySchemaHash.TryGetValue(
                schemaHash,
                out var lazy))
            {
                bindings = lazy.Value;
                return true;
            }

            bindings = null;
            return false;
        }

        /// <summary>
        /// Replaces the cached binding plan for a schema hash with a newly built one.
        /// Used when an existing cached binding fails schema validation (e.g. hash collision or schema drift),
        /// preventing repeated rebuilds on subsequent requests for the same schema.
        /// </summary>
        /// <param name="schemaHash">The schema hash key to replace.</param>
        /// <param name="bindings">The newly built binding plan to store.</param>
        public void ReplaceBindings(
            int schemaHash,
            ResolvedTableBindings bindings)
        {
            var replacement = new Lazy<ResolvedTableBindings>(
                () => bindings,
                LazyThreadSafetyMode.ExecutionAndPublication);

            _bindingsBySchemaHash[schemaHash] = replacement;
        }

        /// <summary>
        /// Attempts to reuse the most recently matched binding plan without recomputing schema hashes.
        /// </summary>
        /// <param name="reader">The data reader for which to match the hot bindings.</param>
        /// <param name="bindings">When this method returns, contains the hot bindings if a match is found; otherwise, null.</param>
        /// <returns><see langword="true"/> if the hot bindings match the provided data reader; otherwise, <see langword="false"/>.</returns>
        public bool TryGetHotBindings(
            IDataReader reader,
            out ResolvedTableBindings bindings)
        {
            var hotBindings = Volatile.Read(ref _hotBindings);
            if (hotBindings != null &&
                hotBindings.Matches(reader))
            {
                bindings = hotBindings;
                return true;
            }

            bindings = null;
            return false;
        }

        /// <summary>
        /// Records the most recently reused binding plan for later hot-path matches.
        /// </summary>
        /// <param name="bindings">The binding plan to remember as hot bindings.</param>
        public void RememberBindings(
            ResolvedTableBindings bindings)
        {
            if (bindings == null ||
                bindings.ColumnNames.Length == 0)
            {
                return;
            }

            Volatile.Write(ref _hotBindings, bindings);
        }

        /// <summary>
        /// Attempts to reserve a cache slot while enforcing the maximum bindings cache size under concurrency.
        /// </summary>
        /// <returns><see langword="true"/> if a cache slot was successfully reserved; <see langword="false"/> if the cache is already at capacity.</returns>
        private bool TryReserveBindingsCacheSlot()
            => TryReserveBindingsCacheSlotCore(
                () => Volatile.Read(ref _bindingsCacheSize),
                TryUpdateBindingsCacheSize);

        /// <summary>
        /// Attempts to reserve a cache slot using pluggable read and update delegates.
        /// </summary>
        /// <remarks>Spins until either the cache is found to be full or the atomic update succeeds,
        /// allowing the caller to inject test doubles for both operations.</remarks>
        /// <param name="readCacheSize">A delegate that returns the current observed cache size on each spin iteration.</param>
        /// <param name="tryUpdate">A delegate that attempts an atomic compare-and-swap from <c>current</c> to <c>updated</c>,
        /// returning <see langword="true"/> on success and <see langword="false"/> when a concurrent update was detected.</param>
        /// <returns><see langword="true"/> if a cache slot was successfully reserved; <see langword="false"/> if the cache is at capacity.</returns>
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
        /// Attempts to update the bindings cache size field atomically from
        /// <paramref name="currentSize"/> to <paramref name="updatedSize"/>.
        /// </summary>
        /// <param name="currentSize">The expected current value of <see cref="_bindingsCacheSize"/>.</param>
        /// <param name="updatedSize">The value to store when the expected value is confirmed.</param>
        /// <returns>
        /// <see langword="true"/> when the compare-exchange succeeds; <see langword="false"/> when another thread
        /// modified the value concurrently and the caller must retry.
        /// </returns>
        private bool TryUpdateBindingsCacheSize(
            int currentSize,
            int updatedSize)
            => Interlocked.CompareExchange(
                ref _bindingsCacheSize,
                updatedSize,
                currentSize) == currentSize;
    }
}
