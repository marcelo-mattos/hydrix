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
    /// external callers configure it only through <see cref="HydrixMapperGlobalConfiguration"/> instead of mutating
    /// the process-wide state directly.
    /// </remarks>
    internal static class HydrixMapperConfiguration
    {
        /// <summary>
        /// Stores the currently active configuration generation used by the convenience API.
        /// </summary>
        private static HydrixMapperConfigurationState _state =
            new HydrixMapperConfigurationState(
                new HydrixMapperOptions());

        /// <summary>
        /// Gets the currently active mapper options snapshot.
        /// </summary>
        /// <returns>
        /// The process-wide <see cref="HydrixMapperOptions"/> instance that will be used for future default mapper
        /// creation.
        /// </returns>
        public static HydrixMapperOptions Options =>
            Volatile.Read(
                ref _state).Options;

        /// <summary>
        /// Replaces the active mapper options snapshot so the next default mapper resolution uses a fresh configuration
        /// generation.
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
                ref _state,
                new HydrixMapperConfigurationState(
                    options));
        }

        /// <summary>
        /// Returns the shared default mapper instance, creating it lazily when accessed for the first time.
        /// </summary>
        /// <returns>
        /// A process-wide <see cref="HydrixMapper"/> configured with the current <see cref="Options"/>.
        /// </returns>
        internal static HydrixMapper GetOrCreateDefaultMapper()
            => Volatile.Read(
                ref _state)
                .GetOrCreateDefaultMapper();
    }
}
