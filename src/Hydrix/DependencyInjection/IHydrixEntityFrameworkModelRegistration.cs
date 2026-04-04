using System;

namespace Hydrix.DependencyInjection
{
    /// <summary>
    /// Defines the contract for deferred Entity Framework model registrations configured through dependency injection.
    /// </summary>
    /// <remarks>Implementations resolve the configured Entity Framework source from the application service
    /// provider and register the translated metadata with Hydrix during startup.</remarks>
    internal interface IHydrixEntityFrameworkModelRegistration
    {
        /// <summary>
        /// Resolves the configured Entity Framework model source from the supplied service provider and registers it with Hydrix.
        /// </summary>
        /// <param name="serviceProvider">The scoped service provider used to resolve the configured DbContext instance.</param>
        void Register(
            IServiceProvider serviceProvider);
    }
}
