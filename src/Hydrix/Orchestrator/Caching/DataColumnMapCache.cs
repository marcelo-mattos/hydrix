using Hydrix.Orchestrator.Binders.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hydrix.Orchestrator.Caching
{
    /// <summary>
    /// Provides a thread-safe cache for mapping entity properties to data columns, enabling efficient data binding for
    /// the specified entity type.
    /// </summary>
    /// <remarks>This static class ensures that a DataColumnBinder instance is lazily initialized and reused
    /// for each entity type, optimizing performance and resource usage in data binding scenarios. The cache inspects
    /// entity properties and respects Column attributes to determine column names, facilitating flexible and consistent
    /// mapping.</remarks>
    /// <typeparam name="TEntity">The type of the entity whose properties are mapped to data columns.</typeparam>
    internal static class DataColumnMapCache<TEntity>
    {
        /// <summary>
        /// Provides a thread-safe, lazily initialized instance of the DataColumnBinder for the specified entity type.
        /// </summary>
        /// <remarks>The DataColumnBinder instance is created only when it is first accessed, ensuring
        /// efficient resource usage. This approach is particularly useful in scenarios where the binder may not be
        /// needed immediately, thus avoiding unnecessary initialization overhead.</remarks>
        private static readonly Lazy<DataColumnBinder<TEntity>> Map =
            new Lazy<DataColumnBinder<TEntity>>(Build, isThreadSafe: true);

        /// <summary>
        /// Gets the existing instance of the DataColumnBinder for the specified entity type, or creates a new one if it
        /// does not exist.
        /// </summary>
        /// <remarks>This method ensures that there is always a valid DataColumnBinder available for the
        /// specified entity type, facilitating data binding operations.</remarks>
        /// <returns>A DataColumnBinder&lt;TEntity&gt; instance that is either retrieved from the map or newly created.</returns>
        public static DataColumnBinder<TEntity> GetOrCreate() =>
            Map.Value;

        /// <summary>
        /// Creates a new instance of the DataColumnBinder for the specified entity type, binding its readable
        /// properties to data columns.
        /// </summary>
        /// <remarks>This method inspects the properties of the entity type, filtering for readable
        /// properties without index parameters. It also respects any Column attributes to determine the column names
        /// for binding. Properties without a Column attribute use their property name as the column name.</remarks>
        /// <returns>A DataColumnBinder&lt;TEntity&gt; that contains the bindings for the readable, non-indexed properties of the
        /// specified entity type.</returns>
        private static DataColumnBinder<TEntity> Build()
        {
            var type = typeof(TEntity);

            var properties = type
                .GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public)
                .Where(property =>
                    property.CanRead &&
                    property.GetIndexParameters().Length == 0 &&
                    !property.IsDefined(typeof(NotMappedAttribute), inherit: true))
                .ToArray();

            var columns = new List<DataColumnBinding<TEntity>>(properties.Length);

            foreach (var property in properties)
            {
                var columnAttribute = property.GetCustomAttribute<ColumnAttribute>(inherit: true);

                var name = columnAttribute != null && !string.IsNullOrWhiteSpace(columnAttribute.Name)
                    ? columnAttribute.Name
                    : property.Name;

                var getter = CreateGetter(property);
                var dataType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                columns.Add(new DataColumnBinding<TEntity>(
                    columnName: name,
                    dataType: dataType,
                    getter: getter));
            }

            return new DataColumnBinder<TEntity>(columns.ToArray());
        }

        /// <summary>
        /// Creates a delegate that retrieves the value of a specified property from an instance of the entity type.
        /// </summary>
        /// <remarks>This method uses expression trees to create a strongly-typed getter for the specified
        /// property, allowing for efficient retrieval of property values. It handles both value types and reference
        /// types appropriately.</remarks>
        /// <param name="property">The property information for which the getter delegate is to be created. This must not be null and should
        /// represent a valid property of the entity type.</param>
        /// <returns>A delegate that takes an instance of the entity type and returns the value of the specified property as an
        /// object.</returns>
        private static Func<TEntity, object> CreateGetter(
            PropertyInfo property)
        {
            var entityParam = Expression.Parameter(typeof(TEntity), "e");
            var prop = Expression.Property(entityParam, property);

            Expression body = prop.Type.IsValueType
                ? Expression.Convert(prop, typeof(object))
                : Expression.TypeAs(prop, typeof(object));

            return Expression
                .Lambda<Func<TEntity, object>>(
                    body,
                    entityParam)
                .Compile();
        }
    }
}