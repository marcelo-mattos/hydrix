using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Hydrix.Orchestrator.Caching
{
    /// <summary>
    /// Provides a thread-safe cache for validating whether an entity type is decorated with a TableAttribute and
    /// contains at least one property with a ColumnAttribute.
    /// </summary>
    /// <remarks>This class is intended for internal use to optimize repeated validation checks on entity
    /// types. It ensures that only types meeting the required attribute criteria are considered valid for further
    /// processing. If a type does not have a TableAttribute, an exception is thrown during validation.</remarks>
    internal static class EntityRequestValidationCache
    {
        /// <summary>
        /// Stores cached boolean values associated with each type to improve lookup performance.
        /// </summary>
        /// <remarks>This cache is thread-safe and can be accessed concurrently by multiple threads. It is
        /// used to avoid repeated evaluations for the same type, which can enhance performance in multi-threaded
        /// scenarios.</remarks>
        private static readonly ConcurrentDictionary<Type, bool> Cache
            = new ConcurrentDictionary<Type, bool>();

        /// <summary>
        /// Determines whether the specified type is valid according to the entity request validation rules.
        /// </summary>
        /// <remarks>This method uses a cache to store validation results, which improves performance when
        /// validating the same type multiple times.</remarks>
        /// <param name="type">The type to validate. This parameter must not be null.</param>
        /// <returns>true if the specified type is valid; otherwise, false.</returns>
        public static bool Validate(
            Type type)
            => Cache.GetOrAdd(
                type,
                BuildMetadata);

        /// <summary>
        /// Determines whether the specified type has any properties decorated with the ColumnAttribute.
        /// </summary>
        /// <remarks>This method checks for the presence of a TableAttribute on the type and verifies if
        /// any of its properties are marked with a ColumnAttribute, which is essential for entity mapping in data
        /// contexts.</remarks>
        /// <param name="type">The type to inspect for TableAttribute and ColumnAttribute decorations.</param>
        /// <returns>true if the type has at least one property with a ColumnAttribute; otherwise, false.</returns>
        /// <exception cref="MissingMemberException">Thrown if the specified type does not have a TableAttribute decorating itself.</exception>
        internal static bool BuildMetadata(
            Type type)
        {
            var tableAttribute = type
                .GetCustomAttributes(typeof(TableAttribute), false)
                .Cast<TableAttribute>()
                .FirstOrDefault();

            if (tableAttribute == null)
                throw new MissingMemberException("The entity does not have a TableAttribute decorating itself.");

            return true;
        }
    }
}