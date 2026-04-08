using System;
using System.Runtime.CompilerServices;

namespace Hydrix.Mapper.Caching
{
    /// <summary>
    /// Caches strongly typed execute delegates per mapper instance for the generic typed API.
    /// </summary>
    /// <typeparam name="TSource">The compile-time source type associated with the cached delegate.</typeparam>
    /// <typeparam name="TTarget">The compile-time destination type associated with the cached delegate.</typeparam>
    internal static class TypedDelegateCache<TSource, TTarget>
    {
        /// <summary>
        /// Stores the typed execute delegate for each mapper instance that has resolved this generic type pair.
        /// </summary>
        private static readonly ConditionalWeakTable<HydrixMapper, Func<TSource, TTarget>> Cache =
            new ConditionalWeakTable<HydrixMapper, Func<TSource, TTarget>>();

        /// <summary>
        /// Returns the cached typed execute delegate for the supplied mapper instance, creating it on first use.
        /// </summary>
        /// <param name="mapper">The mapper whose per-instance typed delegate should be returned.</param>
        /// <returns>The cached typed execute delegate for this mapper instance and type pair.</returns>
        internal static Func<TSource, TTarget> GetOrAdd(
            HydrixMapper mapper)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(
                mapper);

            return Cache.GetValue(
                mapper,
                static current => current.CreateTypedExecute<TSource, TTarget>());
#else
            if (mapper == null)
                throw new ArgumentNullException(
                    nameof(mapper));

            return Cache.GetValue(
                mapper,
                current => current.CreateTypedExecute<TSource, TTarget>());
#endif
        }
    }
}
