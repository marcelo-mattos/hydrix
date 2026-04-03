using Hydrix.Attributes.Parameters;
using Hydrix.Attributes.Schemas;
using Hydrix.Binders.Procedure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hydrix.Caching
{
    /// <summary>
    /// Provides a thread-safe cache for storing and retrieving procedure binders associated with specific types,
    /// enabling efficient binding of procedure metadata and parameters.
    /// </summary>
    /// <remarks>This static class ensures that each type is processed only once by caching the corresponding
    /// procedure binder. It uses reflection to extract procedure and parameter metadata from attributes applied to the
    /// type and its properties. Attempting to retrieve a binder for a type that lacks the required attributes will
    /// result in an exception. This class is intended for internal use to optimize repeated procedure binding
    /// operations in data access scenarios.</remarks>
    internal static class ProcedureBinderCache
    {
        /// <summary>
        /// Provides a thread-safe cache of procedure binders, indexed by their associated types.
        /// </summary>
        /// <remarks>This field is implemented as a concurrent dictionary to ensure safe access and
        /// modification in multi-threaded scenarios. It enables efficient retrieval and storage of procedure binders
        /// for different types.</remarks>
        private static readonly ConcurrentDictionary<Type, ProcedureBinder> Cache =
            new ConcurrentDictionary<Type, ProcedureBinder>();

        /// <summary>
        /// Gets a ProcedureBinder instance associated with the specified type, creating and caching it if it does not
        /// already exist.
        /// </summary>
        /// <remarks>If a ProcedureBinder for the specified type already exists in the cache, it is
        /// returned; otherwise, a new instance is created and added to the cache. This method is thread-safe.</remarks>
        /// <param name="type">The type for which to retrieve or create a ProcedureBinder. This parameter must not be null.</param>
        /// <returns>A ProcedureBinder instance associated with the specified type.</returns>
        public static ProcedureBinder GetOrAdd(
            Type type)
            => Cache.GetOrAdd(
                type,
                BuildBinder);

        /// <summary>
        /// Builds a ProcedureBinder instance for the specified type, which must be annotated with the
        /// ProcedureAttribute.
        /// </summary>
        /// <remarks>This method inspects the public instance properties of the provided type to create
        /// parameter bindings based on the ParameterAttribute. Ensure that the type is properly annotated with the
        /// required attributes to enable correct binding.</remarks>
        /// <param name="procedureType">The type that defines the procedure metadata and parameters. This type must have the [Procedure] attribute
        /// applied.</param>
        /// <returns>A ProcedureBinder that encapsulates the command type, command text, and parameter bindings derived from the
        /// specified type.</returns>
        /// <exception cref="MissingMemberException">Thrown if the specified type does not have the [Procedure] attribute or if the procedure name is null or
        /// empty.</exception>
        private static ProcedureBinder BuildBinder(
            Type procedureType)
        {
            var procedureAttribute = procedureType.GetCustomAttribute<ProcedureAttribute>(inherit: true) ??
                throw new MissingMemberException($"The procedure does not have a ProcedureAttribute decorating itself");

            if (string.IsNullOrWhiteSpace(procedureAttribute.Name))
                throw new MissingMemberException($"The procedure does not have a valid name in its ProcedureAttribute");

            var commandType = procedureAttribute.CommandType;
            var commandText = procedureAttribute.CommandText;

            var properties = procedureType
                .GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public)
                .Where(property =>
                    property.CanRead &&
                    property.GetIndexParameters().Length == 0)
                .ToArray();

            var bindings = new List<ProcedureParameterBinding>(properties.Length);

            foreach (var property in properties)
            {
                var parameterAttribute = property.GetCustomAttribute<ParameterAttribute>(inherit: true);
                if (parameterAttribute == null)
                    continue;

                bindings.Add(new ProcedureParameterBinding(
                    name: parameterAttribute.Name,
                    direction: parameterAttribute.Direction,
                    dbType: (int)parameterAttribute.DbType,
                    getter: CreateGetter(procedureType, property)));
            }

            return new ProcedureBinder(
                commandType,
                commandText,
                bindings.ToArray());
        }

        /// <summary>
        /// Creates a delegate that retrieves the value of a specified property from an instance of a given type.
        /// </summary>
        /// <remarks>This method is useful for dynamically accessing property values at runtime,
        /// particularly in scenarios involving reflection or expression trees.</remarks>
        /// <param name="declaringType">The type that declares the property to be accessed.</param>
        /// <param name="property">The property metadata that identifies the property whose value will be retrieved.</param>
        /// <returns>A delegate that takes an object instance and returns the value of the specified property from that instance.</returns>
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

            return Expression
                .Lambda<Func<object, object>>(
                    body,
                    objParam)
                .Compile();
        }
    }
}
