using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Hydrix.Orchestrator.Metadata.Materializers
{
    /// <summary>
    /// Represents metadata for a scalar mapped column.
    /// Holds all information required to efficiently assign
    /// a value from a data record to an entity property.
    /// </summary>
    internal sealed class ColumnMaterializeMetadata
    {
        /// <summary>
        /// Gets the reflected property associated with this field.
        /// Used only during metadata construction.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets the compiled setter delegate used to assign
        /// values to the property without reflection.
        /// </summary>
        public Action<object, object> Setter { get; }

        /// <summary>
        /// Gets the target CLR type of the property.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Gets the SQL field mapping attribute applied to the property.
        /// </summary>
        public ColumnAttribute Attribute { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="ColumnMaterializeMetadata"/>.
        /// </summary>
        public ColumnMaterializeMetadata(
            PropertyInfo property,
            Action<object, object> setter,
            Type targetType,
            ColumnAttribute attribute)
        {
            Property = property;
            Setter = setter;
            TargetType = targetType;
            Attribute = attribute;
        }
    }
}