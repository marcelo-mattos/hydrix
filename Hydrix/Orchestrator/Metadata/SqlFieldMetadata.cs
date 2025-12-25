using Hydrix.Attributes.Schemas;
using System;
using System.Reflection;

namespace Hydrix.Orchestrator.Metadata
{
    /// <summary>
    /// Represents metadata for a scalar SQL-mapped field.
    /// Holds all information required to efficiently assign
    /// a value from a data record to an entity property.
    /// </summary>
    internal sealed class SqlFieldMetadata
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
        public SqlFieldAttribute Attribute { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="SqlFieldMetadata"/>.
        /// </summary>
        public SqlFieldMetadata(
            PropertyInfo property,
            Action<object, object> setter,
            Type targetType,
            SqlFieldAttribute attribute)
        {
            Property = property;
            Setter = setter;
            TargetType = targetType;
            Attribute = attribute;
        }
    }
}
