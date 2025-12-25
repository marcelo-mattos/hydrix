using Hydrix.Attributes.Schemas;
using System;
using System.Reflection;

namespace Hydrix.Orchestrator.Metadata
{
    /// <summary>
    /// Represents metadata for a nested SQL entity relationship.
    /// Provides factory and setter delegates for efficient
    /// instantiation and assignment without reflection.
    /// </summary>
    internal sealed class SqlNestedEntityMetadata
    {
        /// <summary>
        /// Gets the property representing the nested entity.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets the SQL entity mapping attribute.
        /// </summary>
        public SqlEntityAttribute Attribute { get; }

        /// <summary>
        /// Gets the compiled factory delegate used to create
        /// new nested entity instances.
        /// </summary>
        public Func<object> Factory { get; }

        /// <summary>
        /// Gets the compiled setter delegate used to assign
        /// the nested entity to the parent entity.
        /// </summary>
        public Action<object, object> Setter { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="SqlNestedEntityMetadata"/>.
        /// </summary>
        public SqlNestedEntityMetadata(
            PropertyInfo property,
            SqlEntityAttribute attribute,
            Func<object> factory,
            Action<object, object> setter)
        {
            Property = property;
            Attribute = attribute;
            Factory = factory;
            Setter = setter;
        }
    }
}
