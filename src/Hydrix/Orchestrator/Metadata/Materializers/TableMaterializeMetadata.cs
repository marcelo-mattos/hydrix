using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Resolvers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
            => _bindingsBySchemaHash.GetOrAdd(
                schemaHash,
                factory);
    }
}