using System.Collections.Generic;

namespace Hydrix.Mapper
{
    /// <summary>
    /// Defines the contract for object-to-object mapping based on cached, compiled mapping plans.
    /// </summary>
    public interface IHydrixMapper
    {
        /// <summary>
        /// Maps a single source object to a newly created destination instance.
        /// </summary>
        /// <typeparam name="TTarget">
        /// The destination type that should be instantiated and populated.
        /// </typeparam>
        /// <param name="source">
        /// The source object that provides the values for the destination instance.
        /// </param>
        /// <returns>
        /// A populated <typeparamref name="TTarget"/> instance created according to the cached mapping plan for the
        /// source and destination type pair.
        /// </returns>
        TTarget Map<TTarget>(
            object source);

        /// <summary>
        /// Maps a single source object whose type is known at compile time to a newly created destination instance.
        /// </summary>
        /// <typeparam name="TSource">
        /// The compile-time source type used to compile and resolve the cached mapping plan.
        /// </typeparam>
        /// <typeparam name="TTarget">
        /// The destination type that should be instantiated and populated.
        /// </typeparam>
        /// <param name="source">
        /// The source object that provides the values for the destination instance.
        /// </param>
        /// <returns>
        /// A populated <typeparamref name="TTarget"/> instance created according to the cached mapping plan for the
        /// compile-time <typeparamref name="TSource"/> and <typeparamref name="TTarget"/> pair.
        /// </returns>
        TTarget Map<TSource, TTarget>(
            TSource source);

        /// <summary>
        /// Maps every element of the supplied source collection to a newly created destination instance.
        /// </summary>
        /// <typeparam name="TTarget">
        /// The destination type instantiated for each non-null source element.
        /// </typeparam>
        /// <param name="sources">
        /// The source collection to map. A <see langword="null"/> value produces an empty result.
        /// </param>
        /// <returns>
        /// A read-only list containing the mapped destination objects in the same iteration order as the input sequence,
        /// excluding source elements that are <see langword="null"/>.
        /// </returns>
        IReadOnlyList<TTarget> MapList<TTarget>(
            IEnumerable<object> sources);

        /// <summary>
        /// Maps every element of the supplied homogeneous source collection to a newly created destination instance by
        /// using a single cached plan for the compile-time source type.
        /// </summary>
        /// <typeparam name="TSource">
        /// The compile-time source type used to compile and resolve the cached mapping plan.
        /// </typeparam>
        /// <typeparam name="TTarget">
        /// The destination type instantiated for each non-null source element.
        /// </typeparam>
        /// <param name="sources">
        /// The source collection to map. A <see langword="null"/> value produces an empty result.
        /// </param>
        /// <returns>
        /// A read-only list containing the mapped destination objects in the same iteration order as the input sequence,
        /// excluding source elements that are <see langword="null"/>.
        /// </returns>
        IReadOnlyList<TTarget> MapList<TSource, TTarget>(
            IEnumerable<TSource> sources);
    }
}
