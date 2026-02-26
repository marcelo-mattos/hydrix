using Hydrix.Orchestrator.Caching;
using Hydrix.Orchestrator.Metadata.Internals;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Hydrix.Orchestrator.Mapping
{
    /// <summary>
    /// Represents the mapping between a CLR property and a SQL field, encapsulating metadata and value conversion
    /// information for use during entity metadata construction.
    /// </summary>
    /// <remarks>ColumnMap is used internally to facilitate the association of entity properties with their
    /// corresponding SQL fields. It provides access to mapping attributes, type normalization for value conversion,
    /// default value handling, and a compiled setter for efficient property assignment. This class is intended for
    /// internal metadata infrastructure and is not designed for direct use in application code.</remarks>
    internal sealed class ColumnMap
    {
        /// <summary>
        /// Gets the reflected property associated with this field.
        /// Used only during metadata construction.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets the SQL field mapping attribute.
        /// </summary>
        public ColumnAttribute Attribute { get; }

        /// <summary>
        /// Gets the normalized target CLR type for value conversion.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Gets the default value associated with this member.
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        /// Gets the compiled setter delegate used to assign values
        /// to the entity property without reflection.
        /// </summary>
        public Action<object, object> Setter { get; }

        /// <summary>
        /// Initializes a new instance of the SqlFieldMap class using the specified property, SQL field attribute, and
        /// target type.
        /// </summary>
        /// <param name="property">The property to be mapped to a SQL field. Cannot be null.</param>
        /// <param name="attribute">The SQL field attribute that provides mapping metadata for the property. Cannot be null.</param>
        public ColumnMap(
            PropertyInfo property,
            ColumnAttribute attribute)
        {
            Property = property;
            Attribute = attribute;

            var underlying = Nullable.GetUnderlyingType(property.PropertyType);

            var isNullable =
                underlying != null ||
                !property.PropertyType.IsValueType;

            TargetType = underlying ?? property.PropertyType;
            Setter = MetadataFactory.CreateSetter(property);

            DefaultValue = isNullable
                ? null
                : DefaultValueFactoryCache.Get(TargetType)();
        }
    }
}