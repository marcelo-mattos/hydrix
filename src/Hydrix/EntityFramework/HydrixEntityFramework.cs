using Hydrix.Caching;
using Hydrix.Schemas.Contract;
using System;

namespace Hydrix.EntityFramework
{
    /// <summary>
    /// Provides the entry point for registering Hydrix metadata translated from an Entity Framework model.
    /// </summary>
    /// <remarks>This API is intentionally additive. It reads metadata produced by Entity Framework,
    /// translates it to the existing Hydrix metadata structures, and stores the result in a dedicated cache so the
    /// current query-building, validation, and materialization pipelines can keep operating without structural
    /// changes.</remarks>
    public static class HydrixEntityFramework
    {
        /// <summary>
        /// Reads the supplied Entity Framework model and registers the compatible mappings for Hydrix.
        /// </summary>
        /// <remarks>The supplied value can be either a <c>DbContext</c> instance or the corresponding
        /// <c>DbContext.Model</c> object. Only entities that implement <see cref="ITable"/> are translated and
        /// registered.</remarks>
        /// <param name="dbContextOrModel">An Entity Framework <c>DbContext</c> instance or its model object.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContextOrModel"/> is null.</exception>
        public static void RegisterModel(
            object dbContextOrModel)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(dbContextOrModel);
#else
            if (dbContextOrModel == null)
                throw new ArgumentNullException(nameof(dbContextOrModel));
#endif

            EntityFrameworkMetadataCache.Register(
                EntityFrameworkModelTranslator.Translate(dbContextOrModel));
        }
    }
}
