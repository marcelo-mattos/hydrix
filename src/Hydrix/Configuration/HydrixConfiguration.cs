namespace Hydrix.Configuration
{
    /// <summary>
    /// Provides static methods and properties for configuring and accessing Hydrix framework options.
    /// </summary>
    /// <remarks>Call the Configure method before accessing the Options property to ensure that the desired
    /// settings are applied. This class is intended for internal use within the Hydrix framework and is not intended to
    /// be accessed directly by application code.</remarks>
    internal static class HydrixConfiguration
    {
        /// <summary>
        /// Holds the default configuration options for the Hydrix framework.
        /// </summary>
        /// <remarks>Modifying this static field will affect all components that use the default Hydrix
        /// options. Changes to this instance are global and persist for the lifetime of the application.</remarks>
        private static HydrixOptions _options =
            new HydrixOptions();

        /// <summary>
        /// Gets the current configuration options for the Hydrix application.
        /// </summary>
        /// <remarks>This property provides access to the settings that control the behavior of the Hydrix
        /// application. Modifications to the options may affect the application's performance and
        /// functionality.</remarks>
        public static HydrixOptions Options =>
            _options;

        /// <summary>
        /// Configures the application with the specified options.
        /// </summary>
        /// <param name="options">The options to configure the application. This parameter cannot be null.</param>
        public static void Configure(
            HydrixOptions options)
            => _options = options;
    }
}
