using Hydrix.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Hydrix.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for registering Hydrix services with an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class HydrixServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures Hydrix services to the specified service collection.
        /// </summary>
        /// <remarks>Call this method during application startup to register Hydrix dependencies with the
        /// dependency injection container.</remarks>
        /// <param name="services">The service collection to which Hydrix services will be added. Cannot be null.</param>
        /// <param name="configure">An optional delegate to configure Hydrix options. If null, default options are used.</param>
        /// <returns>The same instance of <see cref="IServiceCollection"/> that was provided, to support method chaining.</returns>
        public static IServiceCollection AddHydrix(
            this IServiceCollection services,
            Action<HydrixOptions> configure = null)
        {
            var options = new HydrixOptions();

            configure?.Invoke(options);

            services.AddSingleton(provider =>
            {
                options.Logger ??= provider
                    .GetService<ILoggerFactory>()?
                    .CreateLogger("Hydrix");
                return options;
            });

            HydrixConfiguration.Configure(options);

            return services;
        }
    }
}
