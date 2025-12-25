using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Hydrix.Orchestrator.Metadata
{
    /// <summary>
    /// Provides factory methods for creating compiled metadata
    /// components used during entity materialization.
    /// </summary>
    internal static class SqlMetadataFactory
    {
        /// <summary>
        /// Creates metadata for a scalar SQL field.
        /// </summary>
        public static SqlFieldMetadata CreateField(
            PropertyInfo property,
            SqlFieldAttribute attribute)
            => new SqlFieldMetadata(
                    property,
                    CreateSetter(property),
                    property.PropertyType,
                    attribute);

        /// <summary>
        /// Creates metadata for a SQL entity.
        /// </summary>
        public static SqlEntityMetadata CreateEntity(
            IReadOnlyList<SqlFieldMap> fields,
            IReadOnlyList<SqlEntityMap> entities)
            => new SqlEntityMetadata(
                    fields,
                    entities);

        /// <summary>
        /// Creates metadata for a nested SQL entity relationship.
        /// </summary>
        public static SqlNestedEntityMetadata CreateNestedEntity(
            PropertyInfo property,
            SqlEntityAttribute attribute)
            => new SqlNestedEntityMetadata(
                    property,
                    attribute,
                    CreateFactory(property.PropertyType),
                    CreateSetter(property));

        /// <summary>
        /// Creates a compiled property setter delegate.
        /// </summary>
        internal static Action<object, object> CreateSetter(PropertyInfo property)
        {
            var instance = Expression.Parameter(
                typeof(object),
                "instance");

            var value = Expression.Parameter(
                typeof(object),
                "value");

            var castInstance = Expression.Convert(
                instance,
                property.DeclaringType);

            var castValue = Expression.Convert(
                value,
                property.PropertyType);

            var assign = Expression.Assign(
                Expression.Property(
                    castInstance,
                    property),
                castValue
            );

            return Expression
                .Lambda<Action<object, object>>(
                    assign,
                    instance,
                    value)
                .Compile();
        }

        /// <summary>
        /// Creates a compiled factory delegate for the specified type.
        /// </summary>
        internal static Func<object> CreateFactory(Type type)
            => Expression
                .Lambda<Func<object>>(
                    Expression.New(
                        type))
                .Compile();
    }
}
