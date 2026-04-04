using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hydrix.DependencyInjection
{
    /// <summary>
    /// Provides startup extension methods for executing deferred Hydrix Entity Framework registrations.
    /// </summary>
    public static class HydrixServiceProviderExtensions
    {
        /// <summary>
        /// Resolves all queued Entity Framework model registrations from the dependency injection container and applies them to Hydrix.
        /// </summary>
        /// <remarks>Call this method once during application startup after the root <see cref="IServiceProvider"/>
        /// is built. Hydrix creates a scope, resolves every DbContext type registered through
        /// <see cref="HydrixServiceCollectionExtensions.AddHydrixEntityFrameworkModel{TDbContext}(IServiceCollection)"/>,
        /// translates each compatible Entity Framework model, and stores the result in the existing Hydrix caches.</remarks>
        /// <param name="serviceProvider">The root service provider that should resolve the queued registrations.</param>
        /// <returns>The same instance of <see cref="IServiceProvider"/> that was provided, to support fluent startup code.</returns>
        public static IServiceProvider UseHydrixEntityFrameworkModels(
            this IServiceProvider serviceProvider)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(serviceProvider);
#else
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));
#endif

            using var scope = serviceProvider.CreateScope();
            var scopedProvider = scope.ServiceProvider;

            foreach (var registration in scopedProvider.GetServices<IHydrixEntityFrameworkModelRegistration>())
            {
                registration.Register(scopedProvider);
            }

            return serviceProvider;
        }
    }
}
