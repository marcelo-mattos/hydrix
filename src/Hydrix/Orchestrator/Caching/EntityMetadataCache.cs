using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Metadata.Internals;
using Hydrix.Orchestrator.Metadata.Materializers;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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
        private static readonly ConcurrentDictionary<Type, TableMaterializeMetadata> _cache
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
            => _cache.GetOrAdd(
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
            var fields = type
                .GetProperties()
                .Where(property =>
                    property.CanWrite &&
                    property.GetIndexParameters().Length == 0 &&
                    !property.IsDefined(typeof(NotMappedAttribute), inherit: true))
                .Select(property =>
                {
                    var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                    var columnName = columnAttribute?.Name ?? property.Name;

                    var setter = MetadataFactory.CreateSetter(property);
                    var reader = FieldReaderFactory.Create(property.PropertyType);

                    return new ColumnMap(
                        columnName,
                        setter,
                        reader);
                })
                .ToList();

            var entities = type
                .GetProperties()
                .Where(property =>
                    property.CanWrite &&
                    Attribute.IsDefined(property, typeof(ForeignTableAttribute)))
                .Select(property => new TableMap(
                    property,
                    (ForeignTableAttribute)property
                        .GetCustomAttributes(typeof(ForeignTableAttribute), false)
                        .First()))
                .ToList();

            return MetadataFactory.CreateEntity(
                fields,
                entities);
        }
    }
}