using Hydrix.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hydrix.DependencyInjection
{
    /// <summary>
    /// Represents a deferred Entity Framework model registration configured for a specific DbContext type.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type that should be resolved from the dependency injection container.</typeparam>
    /// <remarks>The registration remains additive: the resolved model is translated into Hydrix's existing
    /// metadata caches without changing the current attribute-based pipeline.</remarks>
    internal sealed class HydrixEntityFrameworkModelRegistration<TDbContext> :
        IHydrixEntityFrameworkModelRegistration
        where TDbContext : class
    {
        /// <summary>
        /// Resolves the configured DbContext from the supplied service provider and registers its model with Hydrix.
        /// </summary>
        /// <param name="serviceProvider">The scoped service provider used to resolve <typeparamref name="TDbContext"/>.</param>
        public void Register(
            IServiceProvider serviceProvider)
            => HydrixEntityFramework.RegisterModel(
                serviceProvider.GetRequiredService<TDbContext>());
    }
}
