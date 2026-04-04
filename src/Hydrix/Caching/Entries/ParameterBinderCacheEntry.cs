using Hydrix.Binders.Parameter;
using System;

namespace Hydrix.Caching.Entries
{
    /// <summary>
    /// Represents an immutable hot-cache entry for a parameter binder.
    /// </summary>
    /// <remarks>This type stores the parameter CLR type together with the resolved binder so the
    /// process-wide hot cache can publish a single immutable reference for concurrent callers.</remarks>
    internal sealed class ParameterBinderCacheEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterBinderCacheEntry"/> class.
        /// </summary>
        /// <param name="parameterType">The CLR type associated with the cached binder.</param>
        /// <param name="binder">The cached binder for the parameter type.</param>
        public ParameterBinderCacheEntry(
            Type parameterType,
            ParameterObjectBinder binder)
        {
            ParameterType = parameterType;
            Binder = binder;
        }

        /// <summary>
        /// Gets the CLR type associated with the cached binder.
        /// </summary>
        public Type ParameterType { get; }

        /// <summary>
        /// Gets the cached parameter binder.
        /// </summary>
        public ParameterObjectBinder Binder { get; }
    }
}
