using Hydrix.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        /// dependency injection container. If called multiple times, the Hydrix options registration is replaced and
        /// the latest configuration wins.</remarks>
        /// <param name="services">The service collection to which Hydrix services will be added. Cannot be null.</param>
        /// <param name="configure">An optional delegate to configure Hydrix options. If null, default options are used.</param>
        /// <returns>The same instance of <see cref="IServiceCollection"/> that was provided, to support method chaining.</returns>
        public static IServiceCollection AddHydrix(
            this IServiceCollection services,
            Action<HydrixOptions> configure = null)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(services);
#else
            if (services == null)
                throw new ArgumentNullException(nameof(services));
#endif

            var options = new HydrixOptions();

            configure?.Invoke(options);

            services.Replace(ServiceDescriptor.Singleton(provider =>
            {
                options.Logger ??= provider
                    .GetService<ILoggerFactory>()?
                    .CreateLogger("Hydrix");
                return options;
            }));

            HydrixConfiguration.Configure(options);

            return services;
        }

        /// <summary>
        /// Adds an Entity Framework model registration descriptor that can be executed during application startup.
        /// </summary>
        /// <remarks>Use this method together with <see cref="HydrixServiceProviderExtensions.UseHydrixEntityFrameworkModels(IServiceProvider)"/>
        /// so Hydrix can resolve the configured <typeparamref name="TDbContext"/> from the dependency injection
        /// container, translate its <c>OnModelCreating</c> metadata, and cache the compatible mappings without changing
        /// the existing attribute-based pipeline.</remarks>
        /// <typeparam name="TDbContext">The DbContext type whose Entity Framework model should be registered with Hydrix.</typeparam>
        /// <param name="services">The service collection that should receive the Entity Framework registration descriptor.</param>
        /// <returns>The same instance of <see cref="IServiceCollection"/> that was provided, to support method chaining.</returns>
        public static IServiceCollection AddHydrixEntityFrameworkModel<TDbContext>(
            this IServiceCollection services)
            where TDbContext : class
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(services);
#else
            if (services == null)
                throw new ArgumentNullException(nameof(services));
#endif

            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IHydrixEntityFrameworkModelRegistration, HydrixEntityFrameworkModelRegistration<TDbContext>>());

            return services;
        }
    }
}
