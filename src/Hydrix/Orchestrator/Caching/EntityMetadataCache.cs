using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Metadata.Internals;
using Hydrix.Orchestrator.Metadata.Materializers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Hydrix.Orchestrator.Caching
{
    /// <summary>
    /// Provides a thread-safe cache for storing and retrieving metadata about entity types, enabling efficient access
    /// to mapping information required for materializing database records into objects.
    /// </summary>
    /// <remarks>This class is intended for internal use to optimize performance by avoiding repeated
    /// reflection and attribute inspection when mapping entities. The cache is keyed by the entity type and stores
    /// metadata describing how properties are mapped to database columns and related tables. Access to the cache is
    /// thread-safe, making it suitable for use in multi-threaded scenarios.</remarks>
    internal static class EntityMetadataCache
    {
        /// <summary>
        /// Stores a thread-safe cache of table materialization metadata, indexed by type.
        /// </summary>
        /// <remarks>This field enables efficient retrieval of metadata required for table materialization
        /// operations. Access to the cache is thread-safe, allowing concurrent read and write operations without
        /// additional synchronization.</remarks>
        private static readonly ConcurrentDictionary<Type, TableMaterializeMetadata> Cache
            = new ConcurrentDictionary<Type, TableMaterializeMetadata>();

        /// <summary>
        /// Retrieves the metadata associated with the specified type, adding it to the cache if it does not already
        /// exist.
        /// </summary>
        /// <remarks>This method uses a caching mechanism to improve performance by avoiding repeated
        /// metadata generation for the same type. The operation is thread-safe.</remarks>
        /// <param name="type">The type for which to retrieve or add metadata. This parameter must not be null.</param>
        /// <returns>The metadata associated with the specified type. If the metadata is not already cached, it is created and
        /// added to the cache.</returns>
        public static TableMaterializeMetadata GetOrAdd(
            Type type)
            => Cache.GetOrAdd(
                type,
                BuildMetadata);

        /// <summary>
        /// Builds metadata for the specified type by mapping its properties decorated with column and foreign table
        /// attributes.
        /// </summary>
        /// <remarks>Use this method to generate metadata required for materializing entities from data
        /// sources. Only properties that are writable and explicitly marked with ColumnAttribute or
        /// ForeignTableAttribute are included in the resulting metadata.</remarks>
        /// <param name="type">The type whose properties are inspected to generate metadata. The type must have writable properties marked
        /// with either the ColumnAttribute or ForeignTableAttribute.</param>
        /// <returns>A TableMaterializeMetadata instance containing mappings for the type's column and foreign table properties.</returns>
        internal static TableMaterializeMetadata BuildMetadata(
            Type type)
        {
            var properties = type
                .GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public);

            var fields = new List<ColumnMap>(properties.Length);
            var entities = new List<TableMap>();

            foreach (var property in properties)
            {
                var isForeign = property.IsDefined(typeof(ForeignTableAttribute), true);
                var isColumn  = property.IsDefined(typeof(ColumnAttribute), true);
                var isNotMapped = property.IsDefined(typeof(NotMappedAttribute), true);

                if (isForeign && isColumn)
                {
                    throw new InvalidOperationException(
                        $"Property '{property.Name}' in '{type.Name}' cannot have both [Column] and [ForeignTable].");
                }

                if (!property.CanWrite ||
                    property.GetIndexParameters().Length > 0 ||
                    isNotMapped)
                {
                    continue;
                }

                if (isForeign)
                {
                    var foreignAttribute = property
                        .GetCustomAttribute<ForeignTableAttribute>(true);

                    entities.Add(new TableMap(
                        property,
                        foreignAttribute));

                    continue;
                }

                var columnName = property
                    .GetCustomAttribute<ColumnAttribute>(true)?.Name
                    ?? property.Name;

                var setter = MetadataFactory.CreateSetter(property);
                var reader = FieldReaderFactory.Create(property.PropertyType);

                fields.Add(new ColumnMap(
                    columnName,
                    setter,
                    reader));
            }

            return MetadataFactory.CreateEntity(
                fields,
                entities);
        }
    }
}