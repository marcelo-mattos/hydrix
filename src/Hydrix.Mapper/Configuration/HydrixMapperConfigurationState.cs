using System;
using System.Threading;

namespace Hydrix.Mapper.Configuration
{
    /// <summary>
    /// Represents one immutable configuration generation for the process-wide default mapper.
    /// </summary>
    internal sealed class HydrixMapperConfigurationState
    {
        /// <summary>
        /// Stores the option snapshot captured when this configuration generation was created.
        /// </summary>
        private readonly HydrixMapperOptions _options;

        /// <summary>
        /// Stores the lazily created default mapper bound to <see cref="_options"/>.
        /// </summary>
        private HydrixMapper _defaultMapper;

        /// <summary>
        /// Initializes a new configuration generation from the supplied options.
        /// </summary>
        /// <param name="options">The options that should be captured for this generation.</param>
        public HydrixMapperConfigurationState(
            HydrixMapperOptions options)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(
                options);
#else
            if (options == null)
                throw new ArgumentNullException(
                    nameof(options));
#endif
            _options = options.Clone();
        }

        /// <summary>
        /// Gets the options snapshot captured for this generation.
        /// </summary>
        public HydrixMapperOptions Options => _options;

        /// <summary>
        /// Gets the default mapper associated with this generation, creating it on first use.
        /// </summary>
        /// <returns>The cached mapper for this configuration generation.</returns>
        public HydrixMapper GetOrCreateDefaultMapper()
        {
            var current = Volatile.Read(
                ref _defaultMapper);

            if (current != null)
                return current;

            var created = new HydrixMapper(
                _options);

            Interlocked.CompareExchange(
                ref _defaultMapper,
                created,
                null);

            return Volatile.Read(
                ref _defaultMapper);
        }
    }
}
