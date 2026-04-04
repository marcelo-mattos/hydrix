using Hydrix.Attributes.Schemas;
using Hydrix.Caching;
using Hydrix.Metadata.Internals;
using Hydrix.Metadata.Materializers;
using Hydrix.Resolvers;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Hydrix.Mapping
{
    /// <summary>
    /// Provides mapping metadata and utilities for associating nested SQL entities with their corresponding properties
    /// and attributes in a parent entity type.
    /// </summary>
    internal sealed class TableMap
    {
        /// <summary>
        /// Gets the reflected navigation property represented by this mapping.
        /// </summary>
        /// <remarks>The property metadata is used to derive the nested-column prefix and to compile
        /// delegates responsible for creating and assigning nested entity instances during materialization.</remarks>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// Gets the foreign-table attribute associated with the navigation property when the mapping originates from
        /// Hydrix attributes.
        /// </summary>
        /// <remarks>This value is null when the mapping was translated from an Entity Framework model instead
        /// of the traditional attribute-based configuration.</remarks>
        public ForeignTableAttribute Attribute { get; private set; }

        /// <summary>
        /// Gets the compiled factory delegate used to instantiate the nested entity represented by this mapping.
        /// </summary>
        /// <remarks>The factory is created once and then reused for every materialization operation that needs
        /// to create a nested entity instance.</remarks>
        public Func<object> Factory { get; }

        /// <summary>
        /// Gets the compiled setter delegate used to assign the nested entity back to the parent entity.
        /// </summary>
        /// <remarks>The setter is resolved once from the reflected property and avoids repeated late-bound
        /// assignment during materialization.</remarks>
        public Action<object, object> Setter { get; }

        /// <summary>
        /// Gets the compiled activator delegate that creates and assigns the nested entity in a single operation.
        /// </summary>
        /// <remarks>This delegate is used by optimized nested materializers so instance creation and assignment
        /// can happen without additional reflection or manual coordination at runtime.</remarks>
        public Func<object, ITable> Activator { get; }

        /// <summary>
        /// Gets the alias prefix derived from the navigation property name.
        /// </summary>
        /// <remarks>The value includes the trailing dot so it can be concatenated directly with nested column
        /// names when Hydrix resolves aliases such as <c>Customer.Id</c>.</remarks>
        private string PrefixSuffix { get; }

        /// <summary>
        /// Gets the primary-key column name resolved for the nested entity represented by this mapping.
        /// </summary>
        /// <remarks>The value is used to decide whether a nested entity should be instantiated for the current
        /// data record and to infer the key suffix used by the optimized nested-materialization path.</remarks>
        public string PrimaryKey { get; }

        /// <summary>
        /// Gets the suffix appended to candidate key column names when checking whether the nested entity is present
        /// in the current record.
        /// </summary>
        /// <remarks>The suffix is precomputed once so the nested-materialization path can avoid rebuilding the
        /// key-column token every time a row is processed.</remarks>
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
            : this(
                property,
                attribute,
                attribute != null &&
                attribute.PrimaryKeys != null &&
                attribute.PrimaryKeys.Length > 0
                    ? attribute.PrimaryKeys[0]
                    : null)
        { }

        /// <summary>
        /// Initializes a new instance of the TableMap class using metadata translated from Entity Framework.
        /// </summary>
        /// <remarks>This overload preserves the existing Hydrix materialization structure while allowing the
        /// navigation metadata to come from an Entity Framework model instead of Hydrix attributes.</remarks>
        /// <param name="property">The navigation property represented by this instance. Cannot be null.</param>
        /// <param name="primaryKey">The mapped nested primary-key column name when available. Can be null when the
        /// target entity does not expose a resolvable primary key.</param>
        internal TableMap(
            PropertyInfo property,
            string primaryKey)
            : this(
                property,
                attribute: null,
                primaryKey: primaryKey)
        { }

        /// <summary>
        /// Initializes a new instance of the TableMap class using the supplied navigation metadata.
        /// </summary>
        /// <remarks>This constructor centralizes the common initialization path shared by attribute-based and
        /// Entity Framework-translated mappings.</remarks>
        /// <param name="property">The navigation property represented by this instance. Cannot be null.</param>
        /// <param name="attribute">The foreign-table attribute when the mapping comes from Hydrix attributes.
        /// Can be null when the mapping originates from Entity Framework metadata.</param>
        /// <param name="primaryKey">The mapped nested primary-key column name when available. Can be null when Hydrix
        /// cannot determine a nested primary key.</param>
        private TableMap(
            PropertyInfo property,
            ForeignTableAttribute attribute,
            string primaryKey)
        {
            Property = property ?? throw new ArgumentNullException(nameof(property));
            Attribute = attribute;
            Factory = MetadataFactory.CreateFactory(property.PropertyType);
            Setter = MetadataFactory.CreateSetter(property);
            Activator = MetadataFactory.CreateNestedEntityActivator(property);
            PrefixSuffix = string.Concat(property.Name, ".");
            PrimaryKey = string.IsNullOrWhiteSpace(primaryKey)
                ? null
                : primaryKey;
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
            var rowMaterializer = bindings.RowMaterializer;
            if (rowMaterializer != null)
            {
                rowMaterializer(entity, record);
                return;
            }

            if (bindings.Fields.Length != 0)
            {
                SetResolvedEntityFields(
                    entity,
                    record,
                    bindings.Fields);
            }

            if (bindings.Entities.Length != 0)
            {
                SetResolvedEntityNestedEntities(
                    entity,
                    record,
                    bindings.Entities);
            }
        }

        /// <summary>
        /// Retrieves the metadata for the nested entity type associated with the current property.
        /// </summary>
        private TableMaterializeMetadata GetNestedMetadata()
            => _nestedMetadata.Value;

        /// <summary>
        /// Creates a new instance of the ResolvedTableBindings class by resolving field and nested bindings for the
        /// specified data record and table metadata.
        /// </summary>
        /// <param name="record">The data record containing the values to bind to the table columns.</param>
        /// <param name="metadata">The metadata describing the table structure and materialization details.</param>
        /// <param name="prefix">A string prefix used to match column names in the data record.</param>
        /// <param name="ordinals">A read-only dictionary mapping column names to their ordinal positions in the data record.</param>
        /// <param name="schemaHash">An integer representing the hash of the schema, used to identify the current schema version.</param>
        /// <param name="columnNames">An optional array of column names corresponding to the ordinals, used for hot-path schema matching.
        /// Can be null if not provided.</param>
        /// <returns>A ResolvedTableBindings instance containing the resolved field and nested bindings for the specified record
        /// and metadata.</returns>
        internal static ResolvedTableBindings Bind(
            IDataRecord record,
            TableMaterializeMetadata metadata,
            string prefix,
            IReadOnlyDictionary<string, int> ordinals,
            int schemaHash,
            string[] columnNames = null)
        {
            var fields = ResolveFieldBindings(
                record,
                metadata,
                prefix,
                ordinals);
            var entities = ResolveNestedBindings(
                record,
                metadata,
                prefix,
                ordinals,
                schemaHash);

            return new ResolvedTableBindings(
                fields,
                entities,
                columnNames);
        }

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

                var list = ordinals
                    .Where(ordinal => ordinal.Key.StartsWith(fullPrefix, StringComparison.OrdinalIgnoreCase))
                    .Select(ordinal => ordinal.Value)
                    .ToList();

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

                var sourceType = GetFieldType(
                    record,
                    ordinal);

                var assigner = ResolveFieldAssignerWithSourceType(
                    field,
                    ordinal,
                    sourceType);

                if (assigner == null)
                    continue;

                fields.Add(new ResolvedFieldBinding(
                    assigner,
                    ordinal,
                    sourceType,
                    field.Property));
            }

            return fields.Count == 0
                ? Array.Empty<ResolvedFieldBinding>()
                : fields.ToArray();
        }

        /// <summary>
        /// Resolves a field assigner for the specified field using source type information from the record.
        /// </summary>
        /// <param name="record">The data record used to infer the source type at the provided ordinal.</param>
        /// <param name="field">The field mapping for which to resolve an assigner.</param>
        /// <param name="ordinal">The zero-based column ordinal in the data record.</param>
        /// <returns>An assigner action when resolution succeeds; otherwise, <see langword="null"/>.</returns>
        private static Action<object, IDataRecord> ResolveFieldAssigner(
            IDataRecord record,
            ColumnMap field,
            int ordinal)
            => ResolveFieldAssignerWithSourceType(
                field,
                ordinal,
                GetFieldType(
                    record,
                    ordinal));

        /// <summary>
        /// Resolves a field reader for the specified field using source type information from the record.
        /// </summary>
        /// <param name="record">The data record used to infer the source type at the provided ordinal.</param>
        /// <param name="field">The field mapping for which to resolve a reader.</param>
        /// <param name="ordinal">The zero-based column ordinal in the data record.</param>
        /// <returns>A resolved field reader delegate, or <see langword="null"/> when no reader is available.</returns>
        private static FieldReader ResolveFieldReader(
            IDataRecord record,
            ColumnMap field,
            int ordinal)
            => ResolveFieldReaderWithSourceType(
                field,
                GetFieldType(
                    record,
                    ordinal));

        /// <summary>
        /// Assigns scalar fields to the target entity using metadata and ordinal mappings.
        /// </summary>
        /// <param name="entity">The entity instance to populate.</param>
        /// <param name="record">The data record containing source values.</param>
        /// <param name="metadata">The table metadata containing scalar field mappings.</param>
        /// <param name="prefix">The field prefix used when resolving column names.</param>
        /// <param name="ordinals">A mapping between column names and ordinals.</param>
        private static void SetEntityFields(
            ITable entity,
            IDataRecord record,
            TableMaterializeMetadata metadata,
            string prefix,
            IReadOnlyDictionary<string, int> ordinals)
        {
            var fields = ResolveFieldBindings(
                record,
                metadata,
                prefix,
                ordinals);

            if (fields.Length == 0)
                return;

            SetResolvedEntityFields(
                entity,
                record,
                fields);
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
                var materializer = CreateNestedMaterializer(
                    nested,
                    usesPrimaryKey,
                    primaryKeyOrdinal,
                    candidateOrdinals,
                    nestedBindings);

                entities.Add(new ResolvedNestedBinding(
                    usesPrimaryKey,
                    primaryKeyOrdinal,
                    candidateOrdinals,
                    nested.Activator,
                    nestedBindings,
                    materializer,
                    nested.Property));
            }

            return entities.Count == 0
                ? Array.Empty<ResolvedNestedBinding>()
                : entities.ToArray();
        }

        /// <summary>
        /// Creates a materializer delegate for a nested entity, enabling assignment of data record fields to the nested
        /// entity's properties.
        /// </summary>
        /// <param name="nested">The mapping information for the nested table, including property and field metadata.</param>
        /// <param name="usesPrimaryKey">true if the materializer should use the primary key for entity identification; otherwise, false.</param>
        /// <param name="primaryKeyOrdinal">The ordinal position of the primary key field in the data record. Must be a non-negative integer if primary
        /// key usage is enabled.</param>
        /// <param name="candidateOrdinals">An array of ordinal positions for candidate fields in the data record used for materialization.</param>
        /// <param name="nestedBindings">The resolved bindings for the nested table, including field assigners and entity metadata. Cannot be null.</param>
        /// <returns>An Action delegate that assigns data record values to the nested entity's properties, or null if the nested
        /// bindings are not valid for materialization.</returns>
        private static Action<object, IDataRecord> CreateNestedMaterializer(
            TableMap nested,
            bool usesPrimaryKey,
            int primaryKeyOrdinal,
            int[] candidateOrdinals,
            ResolvedTableBindings nestedBindings)
        {
            if (nestedBindings == null ||
                nestedBindings.Entities.Length != 0 ||
                nested.Property == null)
            {
                return null;
            }

            var fieldAssigners = new Action<object, IDataRecord>[nestedBindings.Fields.Length];
            for (var index = 0; index < nestedBindings.Fields.Length; index++)
                fieldAssigners[index] = nestedBindings.Fields[index].Assigner;

            return MetadataFactory.CreateNestedEntityMaterializer(
                nested.Property,
                usesPrimaryKey,
                primaryKeyOrdinal,
                candidateOrdinals,
                fieldAssigners);
        }

        /// <summary>
        /// Assigns nested entities to the target entity using metadata and ordinal mappings.
        /// </summary>
        /// <param name="entity">The root entity instance to populate with nested entities.</param>
        /// <param name="record">The data record containing source values.</param>
        /// <param name="metadata">The table metadata containing nested entity mappings.</param>
        /// <param name="prefix">The field prefix used when resolving column names.</param>
        /// <param name="ordinals">A mapping between column names and ordinals.</param>
        /// <param name="schemaHash">The schema hash used for candidate-ordinal cache resolution.</param>
        private static void SetEntityNestedEntities(
            ITable entity,
            IDataRecord record,
            TableMaterializeMetadata metadata,
            string prefix,
            IReadOnlyDictionary<string, int> ordinals,
            int schemaHash)
        {
            var nestedEntities = ResolveNestedBindings(
                record,
                metadata,
                prefix,
                ordinals,
                schemaHash);

            if (nestedEntities.Length == 0)
                return;

            SetResolvedEntityNestedEntities(
                entity,
                record,
                nestedEntities);
        }

        /// <summary>
        /// Resolves an assigner action that sets a field value on an entity from an IDataRecord, using the specified
        /// source type for type conversion.
        /// </summary>
        /// <remarks>If the field mapping does not provide a property or a compatible setter, the method
        /// returns null. The returned action performs type conversion as needed based on the provided source
        /// type.</remarks>
        /// <param name="field">The column mapping information that describes the target property or field to assign.</param>
        /// <param name="ordinal">The zero-based column ordinal in the data record from which to read the value.</param>
        /// <param name="sourceType">The type of the source value in the data record, used to determine the appropriate conversion or assignment
        /// logic.</param>
        /// <returns>An action that assigns a value from the specified ordinal in the data record to the target entity's field or
        /// property, or null if assignment is not possible.</returns>
        private static Action<object, IDataRecord> ResolveFieldAssignerWithSourceType(
            ColumnMap field,
            int ordinal,
            Type sourceType)
        {
            if (field.Property != null)
            {
                return MetadataFactory.CreateRecordAssigner(
                    field.Property,
                    ordinal,
                    sourceType);
            }

            var reader = ResolveFieldReaderWithSourceType(
                field,
                sourceType);

            if (reader == null || field.Setter == null)
                return null;

            return (entity, dataRecord) => field.Setter(
                entity,
                reader(
                    dataRecord,
                    ordinal));
        }

        /// <summary>
        /// Resolves the appropriate field reader for a given column map and source type.
        /// </summary>
        /// <param name="field">The column map that defines the field and its associated target type and reader.</param>
        /// <param name="sourceType">The type of the source object from which the field value will be read.</param>
        /// <returns>A field reader suitable for reading the field from the specified source type. Returns the column map's
        /// existing reader if the target type is not specified.</returns>
        private static FieldReader ResolveFieldReaderWithSourceType(
            ColumnMap field,
            Type sourceType)
        {
            if (field.TargetType == null)
                return field.Reader;

            return FieldReaderFactory.Create(
                field.TargetType,
                sourceType);
        }

        /// <summary>
        /// Retrieves the data type of the specified field in the given data record.
        /// </summary>
        /// <remarks>If the underlying data record does not support retrieving the field type or is in an
        /// invalid state, this method returns null instead of throwing an exception.</remarks>
        /// <param name="record">The data record from which to obtain the field type. Cannot be null.</param>
        /// <param name="ordinal">The zero-based column ordinal indicating which field's type to retrieve. Must be within the valid range of
        /// field indices for the record.</param>
        /// <returns>A Type object representing the data type of the specified field, or null if the type cannot be determined.</returns>
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
                var materializer = nested.Materializer;

                if (materializer != null)
                {
                    materializer(entity, record);
                    continue;
                }

                var shouldInstantiate = nested.UsesPrimaryKey
                    ? nested.PrimaryKeyOrdinal >= 0 &&
                      !record.IsDBNull(nested.PrimaryKeyOrdinal)
                    : HasAnyNonDbNull(
                        record,
                        nested.CandidateOrdinals);

                if (!shouldInstantiate)
                    continue;

                var nestedBindings = nested.Bindings;
                var nestedEntity = nested.Activator(entity);

                if (nestedBindings.Fields.Length != 0)
                {
                    SetResolvedEntityFields(
                        nestedEntity,
                        record,
                        nestedBindings.Fields);
                }

                if (nestedBindings.Entities.Length != 0)
                {
                    SetResolvedEntityNestedEntities(
                        nestedEntity,
                        record,
                        nestedBindings.Entities);
                }
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
