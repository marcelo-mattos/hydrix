using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Metadata;
using System;
using System.Reflection;

namespace Hydrix.Orchestrator.Mapping
{
    /// <summary>
    /// Represents the cached metadata for a scalar SQL-mapped field.
    /// </summary>
    /// <remarks>
    /// This class encapsulates all reflection-derived information required to map
    /// a single database column to an entity property during materialization.
    ///
    /// The metadata is built once per entity type and reused across multiple
    /// mapping operations, eliminating repeated reflection and improving
    /// performance when processing large result sets.
    ///
    /// It contains:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// The reflected <see cref="PropertyInfo"/> associated with the entity property,
    /// used only during metadata construction.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The <see cref="SqlFieldAttribute"/> that defines the column mapping metadata.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The normalized target CLR type used for safe value conversion, with nullable
    /// types already unwrapped when applicable.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// A compiled setter delegate that assigns values to the entity property
    /// without using reflection during runtime execution.
    /// </description>
    /// </item>
    /// </list>
    /// This design ensures predictable behavior, thread safety, and high-performance
    /// entity materialization by moving all reflection costs to the metadata
    /// initialization phase.
    /// </remarks>
    internal sealed class SqlFieldMap
    {
        /// <summary>
        /// Gets the reflected property associated with this field.
        /// Used only during metadata construction.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets the SQL field mapping attribute.
        /// </summary>
        public SqlFieldAttribute Attribute { get; }

        /// <summary>
        /// Gets the normalized target CLR type for value conversion.
        /// </summary>
        public Type TargetType { get; }

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
        /// <param name="targetType">The target type that the property value will be converted to when mapping to or from the SQL field. Cannot
        /// be null.</param>
        public SqlFieldMap(
            PropertyInfo property,
            SqlFieldAttribute attribute,
            Type targetType)
        {
            Property = property;
            Attribute = attribute;
            TargetType = targetType;
            Setter = SqlMetadataFactory.CreateSetter(property);
        }
    }
}