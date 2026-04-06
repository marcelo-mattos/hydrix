using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Mapping;
using System;
using System.Collections.Generic;

namespace Hydrix.Mapper.Extensions
{
    /// <summary>
    /// Exposes convenience extension methods that route mapping calls through the shared default mapper instance.
    /// </summary>
    public static class MapperExtensions
    {
        /// <summary>
        /// Maps the supplied source object to a new destination instance by using the process-wide default mapper.
        /// </summary>
        /// <typeparam name="TTarget">
        /// The destination type that should be created and populated.
        /// </typeparam>
        /// <param name="source">
        /// The source object that provides the values to be copied and converted.
        /// </param>
        /// <returns>
        /// A newly created <typeparamref name="TTarget"/> instance populated according to the compiled mapping plan.
        /// </returns>
        public static TTarget ToDto<TTarget>(
            this object source)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(
                source);
#else
            if (source == null)
                throw new ArgumentNullException(
                    nameof(source));
#endif
            return HydrixMapperConfiguration.GetOrCreateDefaultMapper().Map<TTarget>(
                source);
        }

        /// <summary>
        /// Maps the supplied source object whose type is known at compile time to a new destination instance by using the
        /// process-wide default mapper.
        /// </summary>
        /// <typeparam name="TSource">
        /// The compile-time source type used to compile and resolve the cached mapping plan.
        /// </typeparam>
        /// <typeparam name="TTarget">
        /// The destination type that should be created and populated.
        /// </typeparam>
        /// <param name="source">
        /// The source object that provides the values to be copied and converted.
        /// </param>
        /// <returns>
        /// A newly created <typeparamref name="TTarget"/> instance populated according to the compiled mapping plan.
        /// </returns>
        public static TTarget ToDto<TSource, TTarget>(
            this TSource source)
        {
            if (source is null)
                throw new ArgumentNullException(
                    nameof(source));

            return HydrixMapperConfiguration.GetOrCreateDefaultMapper().Map<TSource, TTarget>(
                source);
        }

        /// <summary>
        /// Maps every element in the supplied source sequence to a new destination instance by using the process-wide
        /// default mapper.
        /// </summary>
        /// <typeparam name="TTarget">
        /// The destination type created for each non-null source element.
        /// </typeparam>
        /// <param name="sources">
        /// The source sequence to map. A <see langword="null"/> value produces an empty result.
        /// </param>
        /// <returns>
        /// A read-only list containing one mapped destination instance for each non-null source element.
        /// </returns>
        public static IReadOnlyList<TTarget> ToDtoList<TTarget>(
            this IEnumerable<object> sources) =>
            HydrixMapperConfiguration.GetOrCreateDefaultMapper().MapList<TTarget>(
                sources);

        /// <summary>
        /// Maps every element in the supplied homogeneous source sequence to a new destination instance by using the
        /// process-wide default mapper and a single cached plan for the compile-time source type.
        /// </summary>
        /// <typeparam name="TSource">
        /// The compile-time source type used to compile and resolve the cached mapping plan.
        /// </typeparam>
        /// <typeparam name="TTarget">
        /// The destination type created for each non-null source element.
        /// </typeparam>
        /// <param name="sources">
        /// The source sequence to map. A <see langword="null"/> value produces an empty result.
        /// </param>
        /// <returns>
        /// A read-only list containing one mapped destination instance for each non-null source element.
        /// </returns>
        public static IReadOnlyList<TTarget> ToDtoList<TSource, TTarget>(
            this IEnumerable<TSource> sources) =>
            HydrixMapperConfiguration.GetOrCreateDefaultMapper().MapList<TSource, TTarget>(
                sources);
    }
}
