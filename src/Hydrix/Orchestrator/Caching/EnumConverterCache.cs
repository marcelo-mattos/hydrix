using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Hydrix.Orchestrator.Caching
{
    /// <summary>
    /// Provides a thread-safe cache for enum value converters, enabling efficient retrieval and reuse of conversion
    /// delegates for specific enum types.
    /// </summary>
    /// <remarks>This class uses a concurrent dictionary to store compiled delegates that convert objects to
    /// enum values of a given type. Converters are created on demand and cached for subsequent use, which improves
    /// performance when converting values to enums repeatedly in multi-threaded scenarios.</remarks>
    internal static class EnumConverterCache
    {
        /// <summary>
        /// Stores delegates that convert objects of specific types to other objects, enabling efficient type-based
        /// conversions.
        /// </summary>
        /// <remarks>This dictionary is thread-safe and is intended for caching conversion functions to
        /// avoid recreating delegates for repeated type conversions. Access to this cache is safe for concurrent read
        /// and write operations from multiple threads.</remarks>
        private static readonly ConcurrentDictionary<Type, Func<object, object>> Cache
            = new ConcurrentDictionary<Type, Func<object, object>>();

        /// <summary>
        /// Retrieves a cached converter function for the specified type, or creates and caches a new one if none
        /// exists.
        /// </summary>
        /// <remarks>This method ensures that only one converter is created and cached per type, which can
        /// improve performance by avoiding redundant converter creation. The returned converter is thread-safe and can
        /// be reused across multiple calls.</remarks>
        /// <param name="type">The type for which to retrieve or add a converter. This must be a valid type that can be converted.</param>
        /// <returns>A function that converts an object of the specified type to another object. The returned function is cached
        /// for future use.</returns>
        public static Func<object, object> GetOrAdd(
            Type type)
            => Cache.GetOrAdd(
                type,
                CreateEnumConverter);

        /// <summary>
        /// Creates a function that converts an object to the specified enumeration type.
        /// </summary>
        /// <remarks>The returned converter uses expression trees for efficient conversion. The input
        /// object must be compatible with the underlying type of the specified enumeration; otherwise, an exception may
        /// be thrown at runtime.</remarks>
        /// <param name="enumType">The enumeration type to which the input object will be converted. Must be a valid enum type.</param>
        /// <returns>A delegate that takes an object and returns the corresponding value of the specified enumeration type, boxed
        /// as an object.</returns>
        private static Func<object, object> CreateEnumConverter(
            Type enumType)
        {
            var underlying = Enum.GetUnderlyingType(enumType);

            var param = Expression.Parameter(typeof(object), "value");

            var convertUnderlying = Expression.Convert(param, underlying);
            var convertEnum = Expression.Convert(convertUnderlying, enumType);
            var box = Expression.Convert(convertEnum, typeof(object));

            var lambda = Expression.Lambda<Func<object, object>>(box, param);

            return lambda.Compile();
        }
    }
}
