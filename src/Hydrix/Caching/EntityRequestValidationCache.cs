using Hydrix.Attributes.Schemas;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Hydrix.Caching
{
    /// <summary>
    /// Provides a thread-safe cache for validating whether an entity type is eligible for Hydrix materialization.
    /// </summary>
    /// <remarks>This class is intended for internal use to optimize repeated validation checks on entity
    /// types. It supports both the traditional attribute-based validation path and metadata translated from Entity
    /// Framework. By caching validation results for each CLR type, the class avoids repeated reflection work and
    /// repeated cache lookups in multi-threaded scenarios.</remarks>
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
        /// <remarks>This method first checks whether Entity Framework metadata has already been registered
        /// for the supplied type. If so, the translated validation result is reused immediately. Otherwise, the method
        /// uses a cache to store validation results, which improves performance when validating the same type multiple
        /// times.</remarks>
        /// <param name="type">The type to validate. This parameter must not be null.</param>
        /// <returns><see langword="true"/> if the specified type is valid; otherwise, <see langword="false"/>.</returns>
        public static bool Validate(
            Type type)
        {
            if (EntityFrameworkMetadataCache.TryGet(
                type,
                out var registered))
            {
                return registered.IsValid;
            }

            return Cache.GetOrAdd(
                type,
                BuildMetadata);
        }

        /// <summary>
        /// Determines whether the specified type has at least one mappable property.
        /// </summary>
        /// <remarks>This method checks for the presence of a <see cref="TableAttribute"/> on the type and
        /// verifies whether any of its public instance properties participate in Hydrix mapping semantics. When Entity
        /// Framework metadata has been registered for the type, the translated validation result is reused instead of
        /// re-running attribute inspection.</remarks>
        /// <param name="type">The type to inspect for <see cref="TableAttribute"/> and mapped properties.</param>
        /// <returns><see langword="true"/> if the type has at least one property that is not marked with
        /// <see cref="NotMappedAttribute"/> and is eligible for mapping; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="MissingMemberException">Thrown if the specified type does not have a <see cref="TableAttribute"/> decorating itself when the
        /// attribute-based path is used.</exception>
        internal static bool BuildMetadata(
            Type type)
        {
            if (EntityFrameworkMetadataCache.TryGet(
                type,
                out var registered))
            {
                return registered.IsValid;
            }

            var tableAttribute = type
                .GetCustomAttributes(typeof(TableAttribute), false)
                .Cast<TableAttribute>()
                .FirstOrDefault();

            if (tableAttribute == null)
                throw new MissingMemberException("The entity does not have a TableAttribute decorating itself.");

            return type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Any(property =>
                    property.GetIndexParameters().Length == 0 &&
                    property.GetCustomAttributes(typeof(ForeignTableAttribute), false).Length == 0 &&
                    property.GetCustomAttributes(typeof(NotMappedAttribute), false).Length == 0);
        }
    }
}
