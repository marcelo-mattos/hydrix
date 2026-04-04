using System;

namespace Hydrix.Caching.Entries
{
    /// <summary>
    /// Represents an immutable hot-cache entry for a converter delegate.
    /// </summary>
    /// <remarks>This type stores the target CLR type together with the resolved converter delegate so the
    /// process-wide hot cache can expose both values through a single immutable object.</remarks>
    internal sealed class ConverterCacheEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConverterCacheEntry"/> class.
        /// </summary>
        /// <param name="targetType">The target CLR type associated with the cached converter.</param>
        /// <param name="converter">The cached converter delegate.</param>
        public ConverterCacheEntry(
            Type targetType,
            Func<object, object> converter)
        {
            TargetType = targetType;
            Converter = converter;
        }

        /// <summary>
        /// Gets the target CLR type associated with the cached converter.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Gets the cached converter delegate.
        /// </summary>
        public Func<object, object> Converter { get; }
    }
}