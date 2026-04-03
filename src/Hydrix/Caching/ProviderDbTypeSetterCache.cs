using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hydrix.Caching
{
    /// <summary>
    /// Provides a thread-safe cache of provider-specific DbType setters
    /// (e.g. SqlDbType, NpgsqlDbType, MySqlDbType).
    ///
    /// This avoids reflection in the hot path when setting provider-specific
    /// parameter types.
    /// </summary>
    internal static class ProviderDbTypeSetterCache
    {
        /// <summary>
        /// Represents a no-operation action that accepts an IDataParameter and an integer as parameters and performs no
        /// work.
        /// </summary>
        /// <remarks>This delegate can be used as a placeholder where an Action&lt;IDataParameter, int&gt; is
        /// required but no operation is needed. It is useful for avoiding null checks or conditional logic when an
        /// action is optional.</remarks>
        private static readonly Action<IDataParameter, int> Noop = (_, __) => { };

        /// <summary>
        /// Provides a thread-safe cache that stores actions for setting parameter values of a specific type in a data
        /// command.
        /// </summary>
        /// <remarks>This dictionary enables efficient reuse of delegate actions for parameter assignment,
        /// reducing overhead when processing multiple parameters of the same type. It is safe for concurrent access by
        /// multiple threads.</remarks>
        private static readonly ConcurrentDictionary<Type, Action<IDataParameter, int>> Cache =
            new ConcurrentDictionary<Type, Action<IDataParameter, int>>();

        /// <summary>
        /// Gets or builds a compiled setter delegate for a provider-specific DbType property.
        /// </summary>
        /// <param name="type">Concrete type of IDataParameter.</param>
        /// <returns>
        /// A delegate that sets the provider-specific enum value,
        /// or null if the provider does not expose such property.
        /// </returns>
        public static Action<IDataParameter, int> GetOrAdd(
            Type type)
            => Cache.GetOrAdd(
                type,
                BuildSetter);

        /// <summary>
        /// Creates a delegate that sets a database type property on a parameter object of the specified type.
        /// </summary>
        /// <remarks>This method searches for a public writable property on the specified type that is an
        /// enum and whose name ends with 'DbType', such as 'SqlDbType' or 'NpgsqlDbType'. If such a property exists,
        /// the returned delegate can be used to set its value on instances of the parameter type. This is useful for
        /// dynamically assigning provider-specific database types to parameter objects in data access
        /// scenarios.</remarks>
        /// <param name="parameterType">The type of the parameter object. This type must have a public writable property of an enum type whose name
        /// ends with 'DbType'.</param>
        /// <returns>An action that assigns an integer value to the database type property of the parameter object. Returns null
        /// if no suitable property is found.</returns>
        private static Action<IDataParameter, int> BuildSetter(
            Type parameterType)
        {
            var property = parameterType
                .GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public)
                .FirstOrDefault(p =>
                    p.CanWrite &&
                    p.PropertyType.IsEnum &&
                    p.Name.EndsWith(
                        "DbType",
                        StringComparison.Ordinal) &&
                    !string.Equals(
                        p.Name,
                        nameof(IDataParameter.DbType),
                        StringComparison.Ordinal));

            if (property == null)
                return Noop;

            var paramExp = Expression.Parameter(typeof(IDataParameter), "param");
            var valueExp = Expression.Parameter(typeof(int), "value");

            var castParam = Expression.Convert(paramExp, parameterType);
            var castValue = Expression.Convert(valueExp, property.PropertyType);

            var propertyAccess = Expression.Property(castParam, property);
            var assign = Expression.Assign(propertyAccess, castValue);

            var lambda = Expression.Lambda<Action<IDataParameter, int>>(
                assign,
                paramExp,
                valueExp);

            return lambda.Compile();
        }
    }
}
