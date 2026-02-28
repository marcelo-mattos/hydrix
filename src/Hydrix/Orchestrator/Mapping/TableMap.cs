using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Caching;
using Hydrix.Orchestrator.Metadata.Internals;
using Hydrix.Orchestrator.Metadata.Materializers;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Hydrix.Orchestrator.Mapping
{
    /// <summary>
    /// Provides mapping metadata and utilities for associating nested SQL entities with their corresponding properties
    /// and attributes in a parent entity type.
    /// </summary>
    /// <remarks>The TableMap class is used internally to facilitate the mapping of nested entities when
    /// materializing object graphs from SQL data records. It encapsulates the property reflection, attribute metadata,
    /// and compiled delegates required to instantiate and assign nested entities efficiently. Instances of this class
    /// are typically created during metadata discovery and cached for reuse, ensuring thread safety and performance
    /// during concurrent mapping operations.</remarks>
    internal sealed class TableMap
    {
        /// <summary>
        /// Gets the metadata for the property represented by this instance.
        /// </summary>
        /// <remarks>This property provides access to reflection information about the associated
        /// property, such as its name, type, and custom attributes. The value is initialized during construction and is
        /// read-only.</remarks>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// Gets the metadata that describes the characteristics of the associated foreign table.
        /// </summary>
        /// <remarks>This property provides access to the <see cref="ForeignTableAttribute"/> instance
        /// that contains information about the foreign table mapping. The value is set internally and cannot be
        /// modified directly.</remarks>
        public ForeignTableAttribute Attribute { get; private set; }

        /// <summary>
        /// Compiled factory delegate for nested entity instantiation.
        /// </summary>
        public Func<object> Factory { get; }

        /// <summary>
        /// Compiled setter delegate for assigning the nested entity.
        /// </summary>
        public Action<object, object> Setter { get; }

        /// <summary>
        /// Gets the prefix and suffix used for formatting string values.
        /// </summary>
        /// <remarks>Use this property to retrieve the formatting characters that are prepended and
        /// appended to string representations. This ensures consistent formatting across different contexts where
        /// string values are displayed or processed.</remarks>
        public string PrefixSuffix { get; }

        /// <summary>
        /// Gets the primary key value associated with the entity.
        /// </summary>
        /// <remarks>The primary key is used to uniquely identify the entity within the database. It is
        /// essential for operations such as retrieval and updates.</remarks>
        public string PrimaryKey { get; }

        /// <summary>
        /// Gets the suffix that is appended to key column names.
        /// </summary>
        /// <remarks>This property is useful for identifying key columns in a database schema,
        /// particularly when dealing with multiple tables that may have similar column names.</remarks>
        public string KeyColumnSuffix { get; }

        /// <summary>
        /// Provides lazy initialization for the metadata required to materialize table data.
        /// </summary>
        /// <remarks>The metadata is created only when it is first accessed, which can improve performance
        /// by avoiding unnecessary computation if the metadata is not needed.</remarks>
        private readonly Lazy<TableMaterializeMetadata> _nestedMetadata;

        /// <summary>
        /// Gets or sets the array of candidate ordinals used for processing.
        /// </summary>
        /// <remarks>This field holds the ordinals that represent the positions of candidates in a
        /// specific context. It is important to ensure that the array is initialized before use to avoid null reference
        /// exceptions.</remarks>
        private int[] _candidateOrdinals;

        /// <summary>
        /// Stores the hash value that represents the schema of candidate ordinals.
        /// </summary>
        private int _candidateOrdinalsSchemaHash;

        /// <summary>
        /// Gets the lock object used for synchronizing access to the candidate.
        /// </summary>
        private readonly object _candidateLock = new object();

        /// <summary>
        /// Indicates whether the candidate ordinals have been initialized.
        /// </summary>
        /// <remarks>This field is used internally to track the initialization state of candidate
        /// ordinals. Its value may affect subsequent operations that depend on the availability of candidate
        /// ordinals.</remarks>
        private bool _candidateOrdinalsInitialized;

        /// <summary>
        /// Initializes a new instance of the TableMap class, mapping the specified property to a foreign table using
        /// the provided attribute.
        /// </summary>
        /// <remarks>This constructor sets up dynamic mapping by creating a factory for the property's
        /// type and a setter for the property, enabling runtime assignment and retrieval of mapped values.</remarks>
        /// <param name="property">The property to be mapped to the foreign table. Cannot be null.</param>
        /// <param name="attribute">The attribute that provides metadata for the foreign table mapping. Cannot be null.</param>
        public TableMap(
            PropertyInfo property,
            ForeignTableAttribute attribute)
        {
            Property = property;
            Attribute = attribute;
            Factory = MetadataFactory.CreateFactory(property.PropertyType);
            Setter = MetadataFactory.CreateSetter(property);
            PrefixSuffix = string.Concat(property.Name, ".");
            PrimaryKey = attribute.PrimaryKeys?.FirstOrDefault();
            KeyColumnSuffix = !string.IsNullOrWhiteSpace(PrimaryKey)
                ? string.Concat(PrefixSuffix, PrimaryKey)
                : null;

            _nestedMetadata = new Lazy<TableMaterializeMetadata>(
                () => EntityMetadataCache.GetOrAdd(Property.PropertyType),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Populates the specified entity with values from the provided data record, including both direct fields and
        /// nested entities.
        /// </summary>
        /// <remarks>This method is intended for internal use when materializing entities from data
        /// records, such as during database mapping operations. It sets both the direct fields and any nested entities
        /// defined in the metadata.</remarks>
        /// <param name="entity">The entity instance to be populated with data from the record. Must not be null.</param>
        /// <param name="record">The data record containing the values to assign to the entity's fields and nested entities. Must not be
        /// null.</param>
        /// <param name="metadata">Metadata describing the structure and mapping of the entity's fields and nested entities. Must not be null.</param>
        /// <param name="prefix">A prefix to apply to field names when mapping values from the record to the entity. May be empty or null if
        /// no prefix is required.</param>
        /// <param name="ordinals">A read-only dictionary mapping field names to their corresponding ordinal positions in the data record. Must
        /// not be null.</param>
        /// <param name="schemaHash">A hash value representing the schema of the data record. This is used to detect changes in the schema
        /// and optimize mapping operations.</param>
        internal static void SetEntity(
            ITable entity,
            IDataRecord record,
            TableMaterializeMetadata metadata,
            string prefix,
            IReadOnlyDictionary<string, int> ordinals,
            int schemaHash)
        {
            SetEntityFields(
                entity,
                record,
                metadata,
                prefix,
                ordinals);

            SetEntityNestedEntities(
                entity,
                record,
                metadata,
                prefix,
                ordinals,
                schemaHash);
        }

        /// <summary>
        /// Retrieves the metadata for the nested entity type associated with the current property.
        /// </summary>
        /// <remarks>The metadata is initialized and cached on the first invocation to improve efficiency.
        /// This method ensures that repeated access to the nested entity metadata does not incur additional
        /// overhead.</remarks>
        /// <returns>The metadata for the nested entity type. The same instance is returned on subsequent calls for performance
        /// optimization.</returns>
        private TableMaterializeMetadata GetNestedMetadata()
            => _nestedMetadata.Value;

        /// <summary>
        /// Retrieves an array of ordinal values whose keys in the specified dictionary begin with the given prefix.
        /// </summary>
        /// <remarks>The result is cached after the first invocation. Ensure that the ordinals dictionary
        /// is fully populated before calling this method, as subsequent calls will return the cached result.</remarks>
        /// <param name="ordinals">A read-only dictionary that maps string keys to integer ordinal values to be filtered.</param>
        /// <param name="fullPrefix">The prefix used to filter dictionary keys. Only keys that start with this value are considered.</param>
        /// <param name="schemaHash">A hash value representing the schema of the data record. This is used to detect changes in the schema
        /// and optimize mapping operations.</param>
        /// <returns>An array of integers containing the ordinals that match the specified prefix. Returns an empty array if no
        /// matches are found.</returns>
        private int[] GetCandidateOrdinals(
            IReadOnlyDictionary<string, int> ordinals,
            string fullPrefix,
            int schemaHash)
        {
            if (_candidateOrdinalsInitialized && _candidateOrdinalsSchemaHash == schemaHash)
                return _candidateOrdinals;

            lock (_candidateLock)
            {
                if (_candidateOrdinalsInitialized && _candidateOrdinalsSchemaHash == schemaHash)
                    return _candidateOrdinals;

                var list = new List<int>(8);
                foreach (var ordinal in ordinals)
                {
                    if (ordinal.Key.StartsWith(fullPrefix, StringComparison.OrdinalIgnoreCase))
                        list.Add(ordinal.Value);
                }

                _candidateOrdinals = list.Count == 0
                    ? Array.Empty<int>()
                    : list.ToArray();
                _candidateOrdinalsSchemaHash = schemaHash;
                _candidateOrdinalsInitialized = true;

                return _candidateOrdinals;
            }
        }

        /// <summary>
        /// Populates the properties of the specified entity with values from the given data record, using the provided
        /// metadata and optional column name prefix.
        /// </summary>
        /// <remarks>Only fields defined in the metadata and present in the data record are set. Fields
        /// corresponding to missing columns or database null values are skipped or set to null, respectively.</remarks>
        /// <param name="entity">The entity instance whose properties are to be set with values from the data record.</param>
        /// <param name="record">The data record containing the values to assign to the entity's properties.</param>
        /// <param name="metadata">The metadata describing the mapping between the entity's properties and the data record columns.</param>
        /// <param name="prefix">An optional prefix to prepend to column names when retrieving values from the data record. Can be an empty
        /// string if no prefix is required.</param>
        /// <param name="ordinals">A dictionary containing the column ordinals for efficient lookup.</param>
        private static void SetEntityFields(
            ITable entity,
            IDataRecord record,
            TableMaterializeMetadata metadata,
            String prefix,
            IReadOnlyDictionary<string, int> ordinals)
        {
            foreach (var field in metadata.Fields)
            {
                var columnName = prefix.Length == 0
                    ? field.Name
                    : string.Concat(prefix, field.Name);

                if (!ordinals.TryGetValue(columnName, out var ordinal))
                    continue;

                var value = field.Reader(record, ordinal);
                field.Setter(entity, value);
            }
        }

        /// <summary>
        /// Populates the nested entity properties of the specified entity by extracting values from the provided data
        /// record, using the given metadata and property path.
        /// </summary>
        /// <remarks>This method is intended for internal use when materializing entities from a data
        /// record, and recursively sets all nested entities defined in the metadata. If a nested entity's primary key
        /// column is missing or null in the data record, that nested entity is not set.</remarks>
        /// <param name="entity">The entity whose nested entity properties are to be set. Must not be null.</param>
        /// <param name="record">The data record containing the values to populate the nested entities. Must not be null.</param>
        /// <param name="metadata">The metadata describing the structure and nested entities of the entity. Must not be null.</param>
        /// <param name="prefix">The prefix to apply to column names when accessing nested entity values in the data record. Can be an empty
        /// string.</param>
        /// <param name="ordinals">A dictionary containing the column ordinals for efficient lookup.</param>
        /// <param name="schemaHash">A hash value representing the schema of the data record. This is used to detect changes in the schema
        /// and optimize mapping operations.</param>
        private static void SetEntityNestedEntities(
            ITable entity,
            IDataRecord record,
            TableMaterializeMetadata metadata,
            string prefix,
            IReadOnlyDictionary<string, int> ordinals,
            int schemaHash)
        {
            foreach (var nested in metadata.Entities)
            {
                var nestedPrefix = prefix.Length == 0
                    ? nested.PrefixSuffix
                    : string.Concat(prefix, nested.PrefixSuffix);

                var shouldInstantiate = false;

                if (!string.IsNullOrWhiteSpace(nested.PrimaryKey))
                {
                    var keyColumn = prefix.Length == 0
                        ? nested.KeyColumnSuffix
                        : string.Concat(prefix, nested.KeyColumnSuffix);

                    shouldInstantiate =
                        ordinals.TryGetValue(keyColumn, out var pkOrdinal) &&
                        !record.IsDBNull(pkOrdinal);
                }
                else
                {
                    var candidates = nested.GetCandidateOrdinals(
                        ordinals,
                        nestedPrefix,
                        schemaHash);

                    if (candidates.Length != 0)
                    {
                        for (var index = 0; index < candidates.Length; index++)
                        {
                            if (!record.IsDBNull(candidates[index]))
                            {
                                shouldInstantiate = true;
                                break;
                            }
                        }
                    }
                }

                if (!shouldInstantiate)
                    continue;

                var nestedEntity = (ITable)nested.Factory();
                nested.Setter(entity, nestedEntity);

                var nestedMetadata = nested.GetNestedMetadata();

                SetEntity(
                    nestedEntity,
                    record,
                    nestedMetadata,
                    nestedPrefix,
                    ordinals,
                    schemaHash
                );
            }
        }
    }
}