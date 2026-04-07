using Hydrix.Mapper.Caching;
using System;
using System.Threading;

namespace Hydrix.Mapper.Configuration
{
    /// <summary>
    /// Stores the active mapper options for the current process and lazily creates the default mapper instance used by
    /// the object extension methods.
    /// </summary>
    /// <remarks>
    /// This type centralizes the mutable global state required by the convenience API. It is intentionally internal so
    /// external callers configure it through dependency injection or the public extension surface instead of mutating
    /// the process-wide state directly.
    /// </remarks>
    internal static class HydrixMapperConfiguration
    {
        /// <summary>
        /// Stores the currently active mapper options snapshot used to create new default mapper instances.
        /// </summary>
        private static HydrixMapperOptions _options = new HydrixMapperOptions();

        /// <summary>
        /// Stores the lazily created default mapper instance shared by the extension methods.
        /// </summary>
        private static HydrixMapper _defaultMapper;

        /// <summary>
        /// Gets the currently active mapper options snapshot.
        /// </summary>
        /// <returns>
        /// The process-wide <see cref="HydrixMapperOptions"/> instance that will be used for future default mapper
        /// creation.
        /// </returns>
        public static HydrixMapperOptions Options =>
            Volatile.Read(
                ref _options);

        /// <summary>
        /// Replaces the active mapper options, clears the compiled-plan cache, and clears the cached default mapper so a
        /// new instance is created on the next request.
        /// </summary>
        /// <param name="options">
        /// The new options snapshot that should become active for all future default mapper resolutions.
        /// </param>
        public static void Configure(
            HydrixMapperOptions options)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(
                options);
#else
            if (options == null)
                throw new ArgumentNullException(
                    nameof(options));
#endif
            Volatile.Write(
                ref _options,
                options);

            MapPlanCache.Clear();

            Volatile.Write(
                ref _defaultMapper,
                null);
        }

        /// <summary>
        /// Returns the shared default mapper instance, creating it lazily when accessed for the first time.
        /// </summary>
        /// <returns>
        /// A process-wide <see cref="Mapper.HydrixMapper"/> configured with the current <see cref="Options"/>.
        /// </returns>
        internal static HydrixMapper GetOrCreateDefaultMapper()
        {
            var current = Volatile.Read(
                ref _defaultMapper);

            if (current != null)
                return current;

            var created = new HydrixMapper(
                Options);

            Interlocked.CompareExchange(
                ref _defaultMapper,
                created,
                null);

            return Volatile.Read(
                ref _defaultMapper);
        }
    }
}
