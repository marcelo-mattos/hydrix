using System;

namespace Hydrix.Mapper.Configuration
{
    /// <summary>
    /// Provides the explicit public entry point used to configure the process-wide default mapper consumed by the
    /// convenience extension methods.
    /// </summary>
    public static class HydrixMapperGlobalConfiguration
    {
        /// <summary>
        /// Replaces the process-wide default mapper options used by the convenience extension methods.
        /// </summary>
        /// <param name="options">The options snapshot that should become active for future default mapper resolutions.</param>
        public static void Configure(
            HydrixMapperOptions options) =>
            HydrixMapperConfiguration.Configure(
                options);

        /// <summary>
        /// Creates a new options instance, applies the supplied configuration callback, and publishes the resulting
        /// snapshot as the process-wide default mapper configuration.
        /// </summary>
        /// <param name="configure">The callback used to configure the new options instance.</param>
        public static void Configure(
            Action<HydrixMapperOptions> configure)
        {
            var options = new HydrixMapperOptions();
            configure?.Invoke(
                options);

            HydrixMapperConfiguration.Configure(
                options);
        }
    }
}
