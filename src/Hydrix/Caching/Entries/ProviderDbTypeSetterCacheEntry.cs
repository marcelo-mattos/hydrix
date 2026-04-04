using System;
using System.Data;

namespace Hydrix.Caching.Entries
{
    /// <summary>
    /// Represents an immutable hot-cache entry for a provider-specific database-type setter.
    /// </summary>
    /// <remarks>This type stores the parameter CLR type together with the compiled provider-specific
    /// setter so the process-wide hot cache can publish and consume both values atomically.</remarks>
    internal sealed class ProviderDbTypeSetterCacheEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderDbTypeSetterCacheEntry"/> class.
        /// </summary>
        /// <param name="parameterType">The parameter CLR type associated with the cached setter.</param>
        /// <param name="setter">The cached provider-specific setter.</param>
        public ProviderDbTypeSetterCacheEntry(
            Type parameterType,
            Action<IDataParameter, int> setter)
        {
            ParameterType = parameterType;
            Setter = setter;
        }

        /// <summary>
        /// Gets the parameter CLR type associated with the cached setter.
        /// </summary>
        public Type ParameterType { get; }

        /// <summary>
        /// Gets the cached provider-specific setter.
        /// </summary>
        public Action<IDataParameter, int> Setter { get; }
    }
}