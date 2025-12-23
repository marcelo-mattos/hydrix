using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Adapters;
using Hydrix.Schemas;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Hydrix.Orchestrator.Mappers
{
    /// <summary>
    /// Represents the mapping definition of a nested SQL entity within a parent entity.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the metadata required to map a composed or nested <see
    /// cref="ISqlEntity"/> from a flattened SQL result set.
    ///
    /// It is used to describe relationships where an entity contains another entity as a property,
    /// typically populated from JOINed tables using aliased column names (e.g. <c>Parent.Child.Property</c>).
    ///
    /// The mapping behavior is driven by the presence of the <see cref="SqlEntityAttribute"/>,
    /// which defines how and when the nested entity should be instantiated during data materialization.
    /// </remarks>
    internal sealed class SqlEntityMap
    {
        /// <summary>
        /// Gets the reflected property that represents the nested entity.
        /// </summary>
        /// <remarks>
        /// This property represents a writable CLR property whose type implements <see
        /// cref="ISqlEntity"/> and is decorated with <see cref="SqlEntityAttribute"/>.
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
        /// The <see cref="SqlEntityAttribute"/> provides metadata used to control:
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
        public SqlEntityAttribute Attribute { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlEntityMap"/> class.
        /// </summary>
        /// <param name="property">
        /// The reflected property that represents the nested entity on the parent type.
        /// </param>
        /// <param name="attribute">
        /// The <see cref="SqlEntityAttribute"/> instance associated with the property, defining how
        /// the nested entity should be resolved and instantiated.
        /// </param>
        /// <remarks>
        /// This constructor is intended to be invoked during metadata discovery and cached for reuse.
        ///
        /// Once created, the mapping definition should be treated as immutable and safely reused
        /// across concurrent mapping operations.
        /// </remarks>
        public SqlEntityMap(
            PropertyInfo property,
            SqlEntityAttribute attribute)
        {
            this.Property = property;
            this.Attribute = attribute;
        }

        /// <summary>
        /// Recursively populates an <see cref="ISqlEntity"/> instance from a <see cref="DataRow"/>
        /// using precomputed SQL entity metadata.
        /// </summary>
        /// <param name="entity">
        /// The target entity instance to be populated with values from the current <see cref="DataRow"/>.
        /// </param>
        /// <param name="row">The data row containing the flattened SQL result set values.</param>
        /// <param name="metadata">
        /// The metadata definition associated with the entity type, describing all scalar fields
        /// and nested entities participating in the mapping process.
        /// </param>
        /// <param name="path">
        /// The hierarchical path representing the current entity nesting level, used to resolve
        /// prefixed column names in flattened SQL projections (e.g. <c>Parent.Child.Property</c>).
        /// </param>
        /// <param name="entityMetadataCache">
        /// A shared cache of <see cref="SqlEntityMetadata"/> instances indexed by entity <see
        /// cref="Type"/>, used to avoid repeated reflection when processing nested entities.
        /// </param>
        /// <remarks>
        /// This method is the core recursive engine responsible for materializing hierarchical
        /// object graphs from flat SQL result sets.
        ///
        /// The mapping process follows these rules:
        /// <list type="number">
        /// <item>
        /// <description>
        /// Scalar fields decorated with <see cref="SqlFieldAttribute"/> are populated by resolving
        /// their corresponding column names using the current path prefix.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Nested entities decorated with <see cref="SqlEntityAttribute"/> are conditionally
        /// instantiated based on the presence and value of their configured primary key column,
        /// preventing the creation of empty objects when LEFT JOINs return null values.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// For each instantiated nested entity, the method recursively applies the same mapping
        /// logic using updated path information.
        /// </description>
        /// </item>
        /// </list>
        /// All metadata used during the mapping process is retrieved from a shared, thread-safe
        /// cache, ensuring optimal performance even when processing large result sets or deeply
        /// nested entity graphs.
        ///
        /// This method intentionally avoids any SQL or provider-specific logic, operating purely on
        /// ADO.NET primitives and reflection-derived metadata.
        /// </remarks>
        internal static void SetEntity(
            ISqlEntity entity,
            DataRow row,
            SqlEntityMetadata metadata,
            IReadOnlyList<string> path,
            ConcurrentDictionary<Type, SqlEntityMetadata> entityMetadataCache)
            => SetEntity(
                entity,
                new DataRowDataRecordAdapter(row),
                metadata,
                path,
                entityMetadataCache);

        internal static void SetEntity(
            ISqlEntity entity,
            IDataRecord record,
            SqlEntityMetadata metadata,
            IReadOnlyList<string> path,
            ConcurrentDictionary<Type, SqlEntityMetadata> entityMetadataCache)
        {
            string prefix = path.Count > 0
                ? $"{string.Join(".", path)}."
                : string.Empty;

            foreach (var field in metadata.Fields)
            {
                string columnName = prefix +
                    (string.IsNullOrWhiteSpace(field.Attribute.FieldName)
                        ? field.Property.Name
                        : field.Attribute.FieldName);

                int ordinal;
                try
                {
                    ordinal = record.GetOrdinal(columnName);
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }

                if (record.IsDBNull(ordinal))
                {
                    field.Property.SetValue(entity, null);
                    continue;
                }

                var value = record.GetValue(ordinal);
                field.Property.SetValue(entity, ConvertValue(value, field.TargetType));
            }

            foreach (var nested in metadata.Entities)
            {
                string primaryKeyColumn = string.IsNullOrWhiteSpace(nested.Attribute.PrimaryKey)
                    ? null
                    : $"{prefix}{nested.Property.Name}.{nested.Attribute.PrimaryKey}";

                if (primaryKeyColumn != null)
                {
                    int pkOrdinal;
                    try
                    {
                        pkOrdinal = record.GetOrdinal(primaryKeyColumn);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        continue;
                    }

                    if (record.IsDBNull(pkOrdinal))
                        continue;
                }

                var nestedEntity = (ISqlEntity)Activator.CreateInstance(nested.Property.PropertyType);
                nested.Property.SetValue(entity, nestedEntity);

                var nestedMetadata = entityMetadataCache.GetOrAdd(
                    nested.Property.PropertyType,
                    SqlEntityMetadata.BuildEntityMetadata
                );

                SetEntity(
                    nestedEntity,
                    record,
                    nestedMetadata,
                    new List<string>(path) { nested.Property.Name },
                    entityMetadataCache
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
                return Enum.ToObject(targetType, value);

            if (targetType == typeof(Guid))
                return value is Guid g ? g : Guid.Parse(value.ToString());

            return Convert.ChangeType(value, targetType);
        }
    }
}