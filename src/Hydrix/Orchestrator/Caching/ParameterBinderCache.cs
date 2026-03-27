using Hydrix.Orchestrator.Binders.Parameter;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hydrix.Orchestrator.Caching
{
    /// <summary>
    /// Provides a thread-safe cache for retrieving or creating parameter object binders based on a specified type.
    /// </summary>
    /// <remarks>This class is intended for internal use and enables efficient reuse of parameter binders by
    /// storing them in a concurrent dictionary. It is safe for use in multi-threaded scenarios.</remarks>
    internal static class ParameterBinderCache
    {
        /// <summary>
        /// Gets the cache of parameter object binders, indexed by their associated types.
        /// </summary>
        /// <remarks>This static field is used to store instances of ParameterObjectBinder for efficient
        /// retrieval based on type. It ensures thread-safe access to the cache, allowing multiple threads to read and
        /// write without causing data corruption.</remarks>
        private static readonly ConcurrentDictionary<Type, ParameterObjectBinder> Cache
            = new ConcurrentDictionary<Type, ParameterObjectBinder>();

        /// <summary>
        /// Retrieves the existing parameter object binder for the specified type, or adds a new binder if one does not
        /// already exist.
        /// </summary>
        /// <remarks>This method uses a cache to store binders for types, which improves performance by
        /// avoiding repeated binder creation for the same type.</remarks>
        /// <param name="type">The type for which to retrieve or add a parameter object binder. This must be a valid <see cref="Type"/>
        /// instance.</param>
        /// <returns>A <see cref="ParameterObjectBinder"/> instance associated with the specified type.</returns>
        public static ParameterObjectBinder GetOrAdd(
            Type type)
            => Cache.GetOrAdd(
                type,
                BuildBinder);

        /// <summary>
        /// Creates a binder that maps the public instance properties of the specified type to their corresponding value
        /// getters.
        /// </summary>
        /// <remarks>Only properties that are readable and are not indexers are included in the binder.
        /// This enables dynamic access to property values for the given type.</remarks>
        /// <param name="type">The type for which to create a binder. This type must have public instance properties that can be read.</param>
        /// <returns>A ParameterObjectBinder that encapsulates bindings for the readable public instance properties of the
        /// specified type.</returns>
        private static ParameterObjectBinder BuildBinder(
            Type type)
        {
            var properties = type
                .GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public)
                .Where(property =>
                    property.CanRead &&
                    property.GetIndexParameters().Length == 0 &&
                    !property.IsDefined(typeof(NotMappedAttribute), inherit: true))
                .ToArray();

            var bindings = new ParameterObjectBinding[properties.Length];

            for (int index = 0; index < properties.Length; index++)
            {
                var property = properties[index];
                bindings[index] = new ParameterObjectBinding(
                    name: property.Name,
                    getter: CreateGetter(type, property));
            }

            return new ParameterObjectBinder(bindings);
        }

        /// <summary>
        /// Creates a delegate that retrieves the value of a specified property from an instance of a given type.
        /// </summary>
        /// <remarks>This method is useful for dynamically accessing property values at runtime. It
        /// handles both reference and value types appropriately, converting value types to objects as needed.</remarks>
        /// <param name="declaringType">The type of the object from which the property value will be retrieved.</param>
        /// <param name="property">The property information representing the property whose value will be accessed.</param>
        /// <returns>A delegate that takes an object as input and returns the value of the specified property as an object.</returns>
        private static Func<object, object> CreateGetter(
            Type declaringType,
            PropertyInfo property)
        {
            var objParam = Expression.Parameter(typeof(object), "obj");
            var casted = Expression.Convert(objParam, declaringType);
            var prop = Expression.Property(casted, property);

            Expression body = prop.Type.IsValueType
                ? Expression.Convert(prop, typeof(object))
                : Expression.TypeAs(prop, typeof(object));

            var lambda = Expression.Lambda<Func<object, object>>(body, objParam);
            return lambda.Compile();
        }
    }
}