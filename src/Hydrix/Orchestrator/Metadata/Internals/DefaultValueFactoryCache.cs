using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Hydrix.Orchestrator.Metadata.Internals
{
    /// <summary>
    /// Provides a thread-safe cache for factory functions that generate default values for specified types.
    /// </summary>
    /// <remarks>This class is intended for internal use and optimizes the retrieval of default values by
    /// caching factory functions. It supports both value types and reference types, returning null for reference types
    /// and the default value for value types. The cache improves performance when default values are needed repeatedly
    /// for various types.</remarks>
    internal static class DefaultValueFactoryCache
    {
        /// <summary>
        /// Provides a thread-safe cache that stores factory functions for creating instances of specified types.
        /// </summary>
        /// <remarks>This dictionary enables efficient retrieval and reuse of object creation delegates.
        /// It is safe for concurrent access by multiple threads, preventing data corruption when adding or retrieving
        /// factory functions.</remarks>
        private static readonly ConcurrentDictionary<Type, Func<object>> _cache =
            new ConcurrentDictionary<Type, Func<object>>();

        /// <summary>
        /// Retrieves a factory function that creates instances of the specified type.
        /// </summary>
        /// <remarks>If the factory function for the specified type is not already cached, it will be
        /// created and added to the cache. This method is thread-safe and can be called concurrently from multiple
        /// threads.</remarks>
        /// <param name="type">The type for which to retrieve the factory function. This parameter must be a valid, non-null <see
        /// cref="Type"/> object.</param>
        /// <returns>A function that, when invoked, creates an instance of the specified type.</returns>
        public static Func<object> Get(Type type)
            => _cache.GetOrAdd(type, CreateFactory);

        /// <summary>
        /// Creates a factory function that returns the default value for the specified type.
        /// </summary>
        /// <remarks>This method is particularly useful for generating factory functions in scenarios
        /// where default values are needed for various types, especially in generic programming.</remarks>
        /// <param name="type">The type for which the factory function is created. If the type is a value type, the factory will return the
        /// default value for that type; otherwise, it returns null.</param>
        /// <returns>A function that returns the default value of the specified type, or null if the type is a reference type.</returns>
        private static Func<object> CreateFactory(Type type)
        {
            if (!type.IsValueType)
                return () => null;

            var body = Expression.Convert(Expression.Default(type), typeof(object));
            return Expression.Lambda<Func<object>>(body).Compile();
        }
    }
}