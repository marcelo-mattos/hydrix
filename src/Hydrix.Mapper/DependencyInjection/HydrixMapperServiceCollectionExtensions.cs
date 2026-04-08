using Hydrix.Mapper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Hydrix.Mapper.DependencyInjection
{
    /// <summary>
    /// Provides dependency-injection registration helpers for <c>Hydrix.Mapper</c>.
    /// </summary>
    public static class HydrixMapperServiceCollectionExtensions
    {
        /// <summary>
        /// Registers an isolated default mapper implementation for the target service collection.
        /// </summary>
        /// <param name="services">
        /// The service collection that should receive the <see cref="IHydrixMapper"/> singleton registration.
        /// </param>
        /// <param name="configure">
        /// An optional callback used to customize the <see cref="HydrixMapperOptions"/> snapshot before the mapper is
        /// registered.
        /// </param>
        /// <returns>
        /// The same <see cref="IServiceCollection"/> instance so additional registrations can be chained fluently.
        /// </returns>
        /// <remarks>
        /// This method does not mutate the process-wide mapper used by the convenience extension methods. Use
        /// <see cref="HydrixMapperGlobalConfiguration"/> explicitly when the global extension-method mapper should be
        /// reconfigured.
        /// </remarks>
        public static IServiceCollection AddHydrixMapper(
            this IServiceCollection services,
            Action<HydrixMapperOptions> configure = null)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(
                services);
#else
            if (services == null)
                throw new ArgumentNullException(
                    nameof(services));
#endif
            var options = new HydrixMapperOptions();
            configure?.Invoke(
                options);

            services.Replace(
                ServiceDescriptor.Singleton<IHydrixMapper>(
                    _ => new HydrixMapper(
                        options)));

            return services;
        }
    }
}
