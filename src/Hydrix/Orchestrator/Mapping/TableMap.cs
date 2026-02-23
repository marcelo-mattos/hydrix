using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Metadata.Internals;
using Hydrix.Orchestrator.Metadata.Materializers;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Hydrix.Orchestrator.Mapping
{
    /// <summary>
    /// Represents the mapping definition of a nested SQL entity within a parent entity.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the metadata required to map a composed or nested <see
    /// cref="ITable"/> from a flattened SQL result set.
    ///
    /// It is used to describe relationships where an entity contains another entity as a property,
    /// typically populated from JOINed tables using aliased column names (e.g. <c>Parent.Child.Property</c>).
    ///
    /// The mapping behavior is driven by the presence of the <see cref="TableAttribute"/>,
    /// which defines how and when the nested entity should be instantiated during data materialization.
    /// </remarks>
    internal sealed class TableMap
    {
        /// <summary>
        /// Gets the reflected property that represents the nested entity.
        /// </summary>
        /// <remarks>
        /// This property represents a writable CLR property whose type implements <see
        /// cref="ITable"/> and is decorated with <see cref="TableAttribute"/>.
        ///
        /// The underlying <see cref="PropertyInfo"/> is used to:
        /// <list type="bullet">
        /// <item>
        /// <description>Instantiate the nested entity dynamically.</description>
        /// </item>
        /// <item>
        /// <description>Assign the created instance to the parent entity.</description>
        /// </item>
        /// </list>
        /// Only properties explicitly marked as SQL entities are included in this mapping, ensuring
        /// that object composition is intentional and explicit.
        /// </remarks>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// Gets the SQL entity attribute that defines the nested mapping behavior.
        /// </summary>
        /// <remarks>
        /// The <see cref="TableAttribute"/> provides metadata used to control:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// The primary key column used to determine whether the nested entity should be
        /// instantiated (particularly important for LEFT JOIN scenarios).
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// How column name prefixes are resolved when mapping flattened SQL projections into
        /// hierarchical object graphs.
        /// </description>
        /// </item>
        /// </list>
        /// This attribute allows the mapper to avoid creating empty or invalid nested entities when
        /// the corresponding SQL data is absent.
        /// </remarks>
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
        /// Initializes a new instance of the <see cref="TableMap"/> class.
        /// </summary>
        /// <param name="property">
        /// The reflected property that represents the nested entity on the parent type.
        /// </param>
        /// <param name="attribute">
        /// The <see cref="TableAttribute"/> instance associated with the property, defining how
        /// the nested entity should be resolved and instantiated.
        /// </param>
        /// <remarks>
        /// This constructor is intended to be invoked during metadata discovery and cached for reuse.
        ///
        /// Once created, the mapping definition should be treated as immutable and safely reused
        /// across concurrent mapping operations.
        /// </remarks>
        public TableMap(
            PropertyInfo property,
            ForeignTableAttribute attribute)
        {
            this.Property = property;
            this.Attribute = attribute;
            this.Factory = MetadataFactory.CreateFactory(property.PropertyType);
            this.Setter = MetadataFactory.CreateSetter(property);
        }

        /// <summary>
        /// Populates the specified SQL entity with data from the provided data record, including nested entities as
        /// defined by the metadata.
        /// </summary>
        /// <remarks>This method is intended for internal use when materializing entities from data
        /// records, such as during data access operations. It supports recursive population of nested entities based on
        /// the provided metadata and path.</remarks>
        /// <param name="entity">The SQL entity instance to populate with values from the data record. Cannot be null.</param>
        /// <param name="record">The data record containing the values to assign to the entity's fields and nested entities. Cannot be null.</param>
        /// <param name="metadata">The metadata describing the structure and mapping of the SQL entity. Cannot be null.</param>
        /// <param name="prefix">An optional prefix to prepend to column names when retrieving values from the data record, used for nested entities. 
        /// Can be an empty string if no prefix is required.</param>
        /// <param name="entityMetadataCache">A thread-safe cache of entity metadata, keyed by entity type, used to optimize metadata lookups for nested
        /// entities. Cannot be null.</param>
        /// <param name="ordinals">A mapping from field names to their corresponding ordinal positions in the data record. Cannot be null.</param>
        internal static void SetEntity(
            ITable entity,
            IDataRecord record,
            TableMaterializeMetadata metadata,
            string prefix,
            ConcurrentDictionary<Type, TableMaterializeMetadata> entityMetadataCache,
            IReadOnlyDictionary<string, int> ordinals)
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
                entityMetadataCache,
                ordinals);
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
                string columnName = string.IsNullOrWhiteSpace(field.Attribute.Name)
                        ? $"{prefix}{field.Property.Name}"
                        : $"{prefix}{field.Attribute.Name}";

                if (!ordinals.TryGetValue(columnName, out var ordinal))
                    continue;

                if (record.IsDBNull(ordinal))
                {
                    field.Setter(entity, field.DefaultValue);
                    continue;
                }

                var value = record.GetValue(ordinal);
                field.Setter(entity, ConvertValue(value, field.TargetType));
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
        /// <param name="entityMetadataCache">A cache of entity metadata, keyed by entity type, used to avoid redundant metadata construction. Must not be
        /// null.</param>
        /// <param name="prefix">The prefix to apply to column names when accessing nested entity values in the data record. Can be an empty
        /// string.</param>
        /// <param name="ordinals">A dictionary containing the column ordinals for efficient lookup.</param>
        private static void SetEntityNestedEntities(
            ITable entity,
            IDataRecord record,
            TableMaterializeMetadata metadata,
            string prefix,
            ConcurrentDictionary<Type, TableMaterializeMetadata> entityMetadataCache,
            IReadOnlyDictionary<string, int> ordinals)
        {
            foreach (var nested in metadata.Entities)
            {
                var nestedPrefix = string.Concat(prefix, nested.Property.Name, ".");
                var shouldInstantiate = false;
                var primaryKey = nested.Attribute.PrimaryKeys?.FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(primaryKey))
                {
                    var keyColumn = string.Concat(nestedPrefix, primaryKey);

                    shouldInstantiate = (ordinals.TryGetValue(keyColumn, out var ordinal) &&
                        !record.IsDBNull(ordinal));
                }

                if (!shouldInstantiate)
                    continue;

                var nestedEntity = (ITable)nested.Factory();
                nested.Setter(entity, nestedEntity);

                var nestedMetadata = entityMetadataCache.GetOrAdd(
                    nested.Property.PropertyType,
                    TableMaterializeMetadata.BuildEntityMetadata
                );

                SetEntity(
                    nestedEntity,
                    record,
                    nestedMetadata,
                    nestedPrefix,
                    entityMetadataCache,
                    ordinals
                );
            }
        }

        /// <summary>
        /// Converts a raw SQL value to the specified CLR target type in a safe and
        /// provider-agnostic manner.
        /// </summary>
        /// <param name="value">
        /// The value extracted from a <see cref="DataRow"/> column. This value is guaranteed to be
        /// non-null and not <see cref="DBNull"/> when passed to this method.
        /// </param>
        /// <param name="targetType">
        /// The resolved destination CLR type used for conversion, with nullable wrappers already
        /// removed when applicable.
        /// </param>
        /// <returns>The converted value compatible with the specified <paramref name="targetType"/>.</returns>
        /// <remarks>
        /// This method centralizes all value conversion logic required during SQL-to-entity
        /// materialization, providing explicit handling for common edge cases not reliably
        /// supported by <see cref="Convert.ChangeType(object, Type)"/>.
        ///
        /// The conversion rules are applied in the following order:
        /// <list type="number">
        /// <item>
        /// <description>
        /// Enumeration types are converted using <see cref="Enum.ToObject(Type, object)"/>,
        /// allowing both numeric and database-backed enum representations.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <see cref="Guid"/> values are handled explicitly to support providers that return GUIDs
        /// as either native types or string representations.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// All other types are converted using <see cref="Convert.ChangeType(object, Type)"/>.
        /// </description>
        /// </item>
        /// </list>
        /// Centralizing this logic ensures consistent behavior across the entire mapping pipeline
        /// and simplifies future extension to support additional CLR types or custom conversion rules.
        /// </remarks>
        private static object ConvertValue(object value, Type targetType)
        {
            if (targetType.IsEnum)
                return value is string s
                    ? Enum.Parse(targetType, s, ignoreCase: true)
                    : Enum.ToObject(targetType, value);

            if (targetType == typeof(Guid))
                return value is Guid g ? g : Guid.Parse(value.ToString());

            return Convert.ChangeType(value, targetType);
        }
    }
}