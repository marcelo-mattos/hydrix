using Hydrix.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hydrix.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for registering and configuring the Hydrix service within an application's dependency
    /// injection container.
    /// </summary>
    /// <remarks>This class contains extension methods for IServiceCollection that enable the addition and
    /// customization of Hydrix services during application startup. Use these methods to configure Hydrix options and
    /// ensure the service is properly registered for dependency injection.</remarks>
    public static class HydrixServiceCollectionExtensions
    {
        /// <summary>
        /// Registers Hydrix services in the specified dependency injection container, optionally configuring service
        /// behavior using provided options.
        /// </summary>
        /// <remarks>This method allows customization of Hydrix service registration by supplying
        /// configuration options. Callers can use the configure parameter to adjust settings prior to service
        /// registration.</remarks>
        /// <param name="services">The service collection to which Hydrix services will be added. Must not be null.</param>
        /// <param name="configure">An optional delegate to configure Hydrix options before registration. If specified, it is invoked with a new
        /// instance of HydrixOptions.</param>
        /// <returns>The IServiceCollection instance with Hydrix services registered, enabling method chaining.</returns>
        public static IServiceCollection AddHydrix(
            this IServiceCollection services,
            Action<HydrixOptions> configure = null)
        {
            var options = new HydrixOptions();

            configure?.Invoke(options);

            HydrixConfiguration.Configure(options);

            return services;
        }
    }
}