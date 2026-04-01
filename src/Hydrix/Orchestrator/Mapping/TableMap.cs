using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Caching;
using Hydrix.Orchestrator.Metadata.Internals;
using Hydrix.Orchestrator.Metadata.Materializers;
using Hydrix.Orchestrator.Resolvers;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;

namespace Hydrix.Orchestrator.Mapping
{
    /// <summary>
    /// Provides mapping metadata and utilities for associating nested SQL entities with their corresponding properties
    /// and attributes in a parent entity type.
    /// </summary>
    internal sealed class TableMap
    {
        /// <summary>
        /// Gets the metadata for the property represented by this instance.
        /// </summary>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// Gets the metadata that describes the characteristics of the associated foreign table.
        /// </summary>
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
        private string PrefixSuffix { get; }

        /// <summary>
        /// Gets the primary key value associated with the entity.
        /// </summary>
        public string PrimaryKey { get; }

        /// <summary>
        /// Gets the suffix that is appended to key column names.
        /// </summary>
        public string KeyColumnSuffix { get; }

        /// <summary>
        /// Provides lazy initialization for the metadata required to materialize table data.
        /// </summary>
        private readonly Lazy<TableMaterializeMetadata> _nestedMetadata;

        /// <summary>
        /// Gets or sets the array of candidate ordinals used for processing.
        /// </summary>
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
        private bool _candidateOrdinalsInitialized;

        /// <summary>
        /// Initializes a new instance of the TableMap class using the specified property and foreign table attribute.
        /// </summary>
        /// <remarks>This constructor sets up the mapping between an entity property and a related table,
        /// including key and factory information required for materialization.</remarks>
        /// <param name="property">The property that represents the navigation or relationship to the foreign table. Cannot be null.</param>
        /// <param name="attribute">The attribute that defines metadata for the foreign table mapping. Cannot be null.</param>
        public TableMap(
            PropertyInfo property,
            ForeignTableAttribute attribute)
        {
            Property = property;
            Attribute = attribute;
            Factory = MetadataFactory.CreateFactory(property.PropertyType);
            Setter = MetadataFactory.CreateSetter(property);
            PrefixSuffix = string.Concat(property.Name, ".");
            PrimaryKey = attribute.PrimaryKeys != null && attribute.PrimaryKeys.Length > 0
                ? attribute.PrimaryKeys[0]
                : null;
            KeyColumnSuffix = !string.IsNullOrWhiteSpace(PrimaryKey)
                ? string.Concat(PrefixSuffix, PrimaryKey)
                : null;

            _nestedMetadata = new Lazy<TableMaterializeMetadata>(
                () => EntityMetadataCache.GetOrAdd(Property.PropertyType),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Populates the specified entity with values from the provided data record using the given metadata and
        /// mapping information.
        /// </summary>
        /// <param name="entity">The entity instance to populate with data from the record. Must implement the ITable interface.</param>
        /// <param name="record">The data record containing the values to assign to the entity's properties.</param>
        /// <param name="metadata">The metadata describing how columns in the data record map to the entity's properties.</param>
        /// <param name="prefix">A string prefix used to match column names in the data record to entity properties. Can be empty if no
        /// prefix is required.</param>
        /// <param name="ordinals">A read-only dictionary mapping column names to their ordinal positions in the data record.</param>
        /// <param name="schemaHash">An integer representing the hash of the schema, used to ensure the mapping is consistent with the expected
        /// structure.</param>
        internal static void SetEntity(
            ITable entity,
            IDataRecord record,
            TableMaterializeMetadata metadata,
            string prefix,
            IReadOnlyDictionary<string, int> ordinals,
            int schemaHash)
            => SetEntity(
                entity,
                record,
                Bind(
                    record,
                    metadata,
                    prefix,
                    ordinals,
                    schemaHash));

        /// <summary>
        /// Creates a new instance of the ResolvedTableBindings class by resolving field and nested bindings for the
        /// specified data record and table metadata.
        /// </summary>
        /// <param name="record">The data record containing the values to bind to the table columns.</param>
        /// <param name="metadata">The metadata describing the table structure and materialization details.</param>
        /// <param name="prefix">A string prefix used to match column names in the data record.</param>
        /// <param name="ordinals">A read-only dictionary mapping column names to their ordinal positions in the data record.</param>
        /// <param name="schemaHash">An integer representing the hash of the schema, used to identify the current schema version.</param>
        /// <returns>A ResolvedTableBindings instance containing the resolved field and nested bindings for the specified record
        /// and metadata.</returns>
        internal static ResolvedTableBindings Bind(
            IDataRecord record,
            TableMaterializeMetadata metadata,
            string prefix,
            IReadOnlyDictionary<string, int> ordinals,
            int schemaHash)
            => new ResolvedTableBindings(
                ResolveFieldBindings(
                    record,
                    metadata,
                    prefix,
                    ordinals),
                ResolveNestedBindings(
                    record,
                    metadata,
                    prefix,
                    ordinals,
                    schemaHash));

        /// <summary>
        /// Populates the specified entity with values from the provided data record using the given table bindings.
        /// </summary>
        /// <param name="entity">The entity to populate with data from the record. Must implement the ITable interface.</param>
        /// <param name="record">The data record containing the values to assign to the entity's fields and nested entities.</param>
        /// <param name="bindings">The resolved table bindings that define how fields and nested entities are mapped from the data record to
        /// the entity.</param>
        internal static void SetEntity(
            ITable entity,
            IDataRecord record,
            ResolvedTableBindings bindings)
        {
            SetResolvedEntityFields(
                entity,
                record,
                bindings.Fields);

            SetResolvedEntityNestedEntities(
                entity,
                record,
                bindings.Entities);
        }

        /// <summary>
        /// Retrieves the metadata for the nested entity type associated with the current property.
        /// </summary>
        private TableMaterializeMetadata GetNestedMetadata()
            => _nestedMetadata.Value;

        /// <summary>
        /// Retrieves the ordinals of all entries in the provided dictionary whose keys start with the specified prefix,
        /// using a schema hash to optimize repeated lookups.
        /// </summary>
        /// <remarks>This method caches results based on the schema hash to improve performance on
        /// repeated calls with the same schema. The comparison for the prefix is case-insensitive.</remarks>
        /// <param name="ordinals">A read-only dictionary mapping column or field names to their ordinal positions.</param>
        /// <param name="fullPrefix">The prefix to match against the dictionary keys. Only keys that start with this prefix are considered.</param>
        /// <param name="schemaHash">A hash value representing the current schema. Used to determine whether cached results can be reused.</param>
        /// <returns>An array of integers containing the ordinals of all dictionary entries whose keys start with the specified
        /// prefix. Returns an empty array if no matches are found.</returns>
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
        /// Resolves the field bindings for the specified data record and table metadata, using the provided column name
        /// prefix and ordinal mapping.
        /// </summary>
        /// <remarks>Only fields with matching column names in the ordinal mapping and a resolvable field
        /// assigner are included in the result. Fields without a corresponding column or assigner are
        /// skipped.</remarks>
        /// <param name="record">The data record containing the values to be bound to fields.</param>
        /// <param name="metadata">The metadata describing the fields to be materialized from the data record.</param>
        /// <param name="prefix">A string to prefix to each field name when matching column names in the ordinal mapping. Can be an empty
        /// string if no prefix is required.</param>
        /// <param name="ordinals">A read-only dictionary mapping column names to their corresponding ordinal positions in the data record.</param>
        /// <returns>An array of resolved field bindings that map fields from the metadata to columns in the data record. Returns
        /// an empty array if no fields are resolved.</returns>
        private static ResolvedFieldBinding[] ResolveFieldBindings(
            IDataRecord record,
            TableMaterializeMetadata metadata,
            string prefix,
            IReadOnlyDictionary<string, int> ordinals)
        {
            if (metadata.Fields.Count == 0)
                return Array.Empty<ResolvedFieldBinding>();

            var fields = new List<ResolvedFieldBinding>(metadata.Fields.Count);

            foreach (var field in metadata.Fields)
            {
                var columnName = prefix.Length == 0
                    ? field.Name
                    : string.Concat(prefix, field.Name);

                if (!ordinals.TryGetValue(columnName, out var ordinal))
                    continue;

                var assigner = ResolveFieldAssigner(
                    record,
                    field,
                    ordinal);

                if (assigner == null)
                    continue;

                fields.Add(new ResolvedFieldBinding(
                    assigner));
            }

            return fields.Count == 0
                ? Array.Empty<ResolvedFieldBinding>()
                : fields.ToArray();
        }

        /// <summary>
        /// Resolves and binds nested entity relationships for the specified data record using the provided
        /// materialization metadata and column ordinals.
        /// </summary>
        /// <remarks>This method is typically used during object materialization to recursively resolve
        /// and bind nested entities based on the provided metadata. It supports both primary key-based and candidate
        /// key-based binding strategies, depending on the entity configuration.</remarks>
        /// <param name="record">The data record containing the values to be materialized into nested entities.</param>
        /// <param name="metadata">The metadata describing the structure and relationships of the entities to be materialized.</param>
        /// <param name="prefix">The prefix to apply to column names when resolving nested entity bindings. May be an empty string if no
        /// prefix is required.</param>
        /// <param name="ordinals">A read-only dictionary mapping column names to their ordinal positions within the data record.</param>
        /// <param name="schemaHash">A hash value representing the schema of the data source, used to optimize binding resolution.</param>
        /// <returns>An array of resolved nested bindings representing the materialized nested entities. Returns an empty array
        /// if no nested entities are defined in the metadata.</returns>
        private static ResolvedNestedBinding[] ResolveNestedBindings(
            IDataRecord record,
            TableMaterializeMetadata metadata,
            string prefix,
            IReadOnlyDictionary<string, int> ordinals,
            int schemaHash)
        {
            if (metadata.Entities.Count == 0)
                return Array.Empty<ResolvedNestedBinding>();

            var entities = new List<ResolvedNestedBinding>(metadata.Entities.Count);

            foreach (var nested in metadata.Entities)
            {
                var nestedPrefix = prefix.Length == 0
                    ? nested.PrefixSuffix
                    : string.Concat(prefix, nested.PrefixSuffix);

                var usesPrimaryKey = !string.IsNullOrWhiteSpace(nested.PrimaryKey);
                var primaryKeyOrdinal = -1;
                var candidateOrdinals = Array.Empty<int>();

                if (usesPrimaryKey)
                {
                    var keyColumn = prefix.Length == 0
                        ? nested.KeyColumnSuffix
                        : string.Concat(prefix, nested.KeyColumnSuffix);

                    if (ordinals.TryGetValue(keyColumn, out var pkOrdinal))
                        primaryKeyOrdinal = pkOrdinal;
                }
                else
                {
                    candidateOrdinals = nested.GetCandidateOrdinals(
                        ordinals,
                        nestedPrefix,
                        schemaHash);
                }

                var nestedMetadata = nested.GetNestedMetadata();
                var nestedBindings = Bind(
                    record,
                    nestedMetadata,
                    nestedPrefix,
                    ordinals,
                    schemaHash);

                entities.Add(new ResolvedNestedBinding(
                    usesPrimaryKey,
                    primaryKeyOrdinal,
                    candidateOrdinals,
                    nested.Factory,
                    nested.Setter,
                    nestedBindings));
            }

            return entities.Count == 0
                ? Array.Empty<ResolvedNestedBinding>()
                : entities.ToArray();
        }

        /// <summary>
        /// Resolves an assigner action that sets a property value on an entity from a data record field at the
        /// specified ordinal position.
        /// </summary>
        /// <remarks>The returned action can be used to efficiently populate entity properties from data
        /// records during materialization. If the field mapping or setter is not available, the method returns
        /// null.</remarks>
        /// <param name="record">The data record providing access to the field values to be assigned.</param>
        /// <param name="field">The column mapping information describing the target property and how to assign its value.</param>
        /// <param name="ordinal">The zero-based ordinal position of the field within the data record.</param>
        /// <returns>An action that assigns the value from the specified field in the data record to the corresponding property
        /// on the entity, or null if assignment is not possible.</returns>
        private static Action<object, IDataRecord> ResolveFieldAssigner(
            IDataRecord record,
            ColumnMap field,
            int ordinal)
        {
            if (field.Property != null)
            {
                return MetadataFactory.CreateRecordAssigner(
                    field.Property,
                    ordinal,
                    GetFieldType(
                        record,
                        ordinal));
            }

            var reader = ResolveFieldReader(
                record,
                field,
                ordinal);

            if (reader == null || field.Setter == null)
                return null;

            return (entity, dataRecord) => field.Setter(
                entity,
                reader(
                    dataRecord,
                    ordinal));
        }

        /// <summary>
        /// Resolves the appropriate field reader for a given data record and column mapping at the specified ordinal
        /// position.
        /// </summary>
        /// <remarks>If the column mapping specifies a target type, a new field reader is created for that
        /// type; otherwise, the existing reader from the column mapping is returned.</remarks>
        /// <param name="record">The data record containing the field to be read.</param>
        /// <param name="field">The column mapping that describes the field and its target type.</param>
        /// <param name="ordinal">The zero-based column ordinal indicating the position of the field within the data record.</param>
        /// <returns>A field reader instance capable of reading the specified field from the data record.</returns>
        private static FieldReader ResolveFieldReader(
            IDataRecord record,
            ColumnMap field,
            int ordinal)
        {
            if (field.TargetType == null)
                return field.Reader;

            return FieldReaderFactory.Create(
                field.TargetType,
                GetFieldType(
                    record,
                    ordinal));
        }

        /// <summary>
        /// Retrieves the data type of the specified field in the given data record.
        /// </summary>
        /// <remarks>If the underlying data record does not support retrieving the field type or is in an
        /// invalid state, this method returns null instead of throwing an exception.</remarks>
        /// <param name="record">The data record from which to obtain the field type. Cannot be null.</param>
        /// <param name="ordinal">The zero-based column ordinal identifying the field whose type is to be retrieved.</param>
        /// <returns>A Type object representing the data type of the specified field; or null if the field type cannot be
        /// determined.</returns>
        private static Type GetFieldType(
            IDataRecord record,
            int ordinal)
        {
            try
            {
                return record.GetFieldType(ordinal);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        /// <summary>
        /// Populates the fields of the specified entity with values from the provided data record using the given
        /// metadata and field mapping information.
        /// </summary>
        /// <param name="entity">The entity instance whose fields are to be set with values from the data record.</param>
        /// <param name="record">The data record containing the values to assign to the entity's fields.</param>
        /// <param name="metadata">The metadata describing the mapping between the entity's fields and the data record columns.</param>
        /// <param name="prefix">The prefix to apply when matching field names to data record columns. Can be null or empty if no prefix is
        /// required.</param>
        /// <param name="ordinals">A read-only dictionary mapping column names to their ordinal positions in the data record.</param>
        private static void SetEntityFields(
            ITable entity,
            IDataRecord record,
            TableMaterializeMetadata metadata,
            string prefix,
            IReadOnlyDictionary<string, int> ordinals)
            => SetResolvedEntityFields(
                entity,
                record,
                ResolveFieldBindings(
                    record,
                    metadata,
                    prefix,
                    ordinals));

        /// <summary>
        /// Populates the nested entity properties of the specified table entity using data from the provided data
        /// record and metadata.
        /// </summary>
        /// <param name="entity">The table entity whose nested entities are to be set.</param>
        /// <param name="record">The data record containing the values to populate the nested entities.</param>
        /// <param name="metadata">The metadata describing the structure and bindings of the table entity.</param>
        /// <param name="prefix">The prefix used to identify nested entity fields within the data record.</param>
        /// <param name="ordinals">A read-only dictionary mapping field names to their ordinal positions in the data record.</param>
        /// <param name="schemaHash">A hash value representing the schema of the data record, used to ensure consistency when resolving bindings.</param>
        private static void SetEntityNestedEntities(
            ITable entity,
            IDataRecord record,
            TableMaterializeMetadata metadata,
            string prefix,
            IReadOnlyDictionary<string, int> ordinals,
            int schemaHash)
            => SetResolvedEntityNestedEntities(
                entity,
                record,
                ResolveNestedBindings(
                    record,
                    metadata,
                    prefix,
                    ordinals,
                    schemaHash));

        /// <summary>
        /// Assigns values from a data record to the specified entity using the provided field bindings.
        /// </summary>
        /// <param name="entity">The entity instance to which field values will be assigned.</param>
        /// <param name="record">The data record containing the values to assign to the entity fields.</param>
        /// <param name="fields">An array of field bindings that define how to assign values from the data record to the entity.</param>
        private static void SetResolvedEntityFields(
            ITable entity,
            IDataRecord record,
            ResolvedFieldBinding[] fields)
        {
            for (var index = 0; index < fields.Length; index++)
                fields[index].Assigner(
                    entity,
                    record);
        }

        /// <summary>
        /// Populates the specified entity with its nested entities by instantiating and setting each nested entity
        /// based on the provided data record and binding definitions.
        /// </summary>
        /// <remarks>Nested entities are only instantiated and set if the corresponding primary key or
        /// candidate fields in the data record are not null. This method is typically used during materialization of
        /// complex object graphs from data records.</remarks>
        /// <param name="entity">The parent entity to which nested entities will be assigned.</param>
        /// <param name="record">The data record containing the values used to instantiate and populate nested entities.</param>
        /// <param name="nestedEntities">An array of binding definitions that describe how to instantiate and assign nested entities to the parent
        /// entity.</param>
        private static void SetResolvedEntityNestedEntities(
            ITable entity,
            IDataRecord record,
            ResolvedNestedBinding[] nestedEntities)
        {
            for (var index = 0; index < nestedEntities.Length; index++)
            {
                var nested = nestedEntities[index];

                var shouldInstantiate = nested.UsesPrimaryKey
                    ? nested.PrimaryKeyOrdinal >= 0 &&
                      !record.IsDBNull(nested.PrimaryKeyOrdinal)
                    : HasAnyNonDbNull(
                        record,
                        nested.CandidateOrdinals);

                if (!shouldInstantiate)
                    continue;

                var nestedEntity = (ITable)nested.Factory();
                nested.Setter(
                    entity,
                    nestedEntity);

                SetEntity(
                    nestedEntity,
                    record,
                    nested.Bindings);
            }
        }

        /// <summary>
        /// Determines whether any of the specified fields in the data record contain a non-DBNull value.
        /// </summary>
        /// <param name="record">The data record to inspect for non-DBNull values. Cannot be null.</param>
        /// <param name="ordinals">An array of zero-based column ordinals to check within the data record. Cannot be null.</param>
        /// <returns>true if at least one of the specified fields does not contain DBNull; otherwise, false.</returns>
        private static bool HasAnyNonDbNull(
            IDataRecord record,
            int[] ordinals)
        {
            for (var index = 0; index < ordinals.Length; index++)
            {
                if (!record.IsDBNull(ordinals[index]))
                    return true;
            }

            return false;
        }
    }
}
