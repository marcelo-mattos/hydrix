using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Metadata.Materializers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace Hydrix.Orchestrator.Metadata.Internals
{
    /// <summary>
    /// Provides factory methods for creating compiled metadata
    /// components used during entity materialization.
    /// </summary>
    internal static class MetadataFactory
    {
        /// <summary>
        /// Creates metadata for a scalar xolumn.
        /// </summary>
        public static ColumnMaterializeMetadata CreateField(
            PropertyInfo property,
            ColumnAttribute attribute)
            => new ColumnMaterializeMetadata(
                    property,
                    CreateSetter(property),
                    property.PropertyType,
                    attribute);

        /// <summary>
        /// Creates metadata for a table.
        /// </summary>
        public static TableMaterializeMetadata CreateEntity(
            IReadOnlyList<ColumnMap> fields,
            IReadOnlyList<TableMap> entities)
            => new TableMaterializeMetadata(
                    fields,
                    entities);

        /// <summary>
        /// Creates metadata for a nested table relationship.
        /// </summary>
        public static ForeignTableMaterializeMetadata CreateNestedEntity(
            PropertyInfo property,
            ForeignTableAttribute attribute)
            => new ForeignTableMaterializeMetadata(
                    property,
                    attribute,
                    CreateFactory(property.PropertyType),
                    CreateSetter(property));

        /// <summary>
        /// Creates a delegate that retrieves the value of the specified property from a given object instance.
        /// </summary>
        /// <remarks>The returned delegate uses expression trees to provide efficient, strongly-typed
        /// access to the property value at runtime. The input object must be of the type that declares the property or
        /// a compatible derived type; otherwise, an exception may be thrown at invocation.</remarks>
        /// <param name="property">The property metadata for which to create a getter delegate. Must not be null and must represent a property
        /// of the declaring type.</param>
        /// <returns>A delegate that takes an object instance and returns the value of the specified property as an object.</returns>
        internal static Func<object, object> CreateGetter(
            PropertyInfo property)
        {
            var parameter = Expression.Parameter(
                typeof(object),
                "entity");

            var cast = Expression.Convert(
                parameter,
                property.DeclaringType);

            var propertyAccess = Expression.Property(
                cast,
                property);

            var convert = Expression.Convert(
                propertyAccess,
                typeof(object));

            return Expression
                .Lambda<Func<object, object>>(
                    convert,
                    parameter)
                .Compile();
        }

        /// <summary>
        /// Creates a delegate that sets the value of a specified property on a given object instance.
        /// </summary>
        /// <remarks>This method uses expression trees to generate a setter delegate, which can improve
        /// performance when setting property values dynamically. The returned delegate performs type conversions as
        /// needed based on the property's declaring type and property type.</remarks>
        /// <param name="property">The <see cref="PropertyInfo"/> representing the property to set. Must not be null and must refer to a
        /// writable property.</param>
        /// <returns>An <see cref="Action{T1, T2}"/> delegate that takes an object instance and a value, and sets the
        /// specified property on the instance to the provided value.</returns>
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

        /// <summary>
        /// Creates a factory function that returns the default value for the specified type.
        /// </summary>
        /// <remarks>This method uses expression trees to compile a lambda function that produces the
        /// default value. The returned factory can be used to obtain a new instance of the default value each time it
        /// is invoked.</remarks>
        /// <param name="type">The type for which to generate a default value factory. If the type is a reference type, the factory will
        /// return null; if it is a value type, the factory will return the default value for that type.</param>
        /// <returns>A function that returns the default value of the specified type, or null if the type is a reference type.</returns>
        public static Func<object> CreateDefaultValueFactory(Type type)
        {
            if (!type.IsValueType)
                return () => null;

            var body = Expression.Convert(Expression.Default(type), typeof(object));
            return Expression.Lambda<Func<object>>(body).Compile();
        }
    }
}