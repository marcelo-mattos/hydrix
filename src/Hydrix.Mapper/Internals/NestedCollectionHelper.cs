using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Hydrix.Mapper.Internals
{
    /// <summary>
    /// Provides the static collection mapping helper invoked from compiled expression trees during nested collection
    /// property mapping.
    /// </summary>
    internal static class NestedCollectionHelper
    {
        /// <summary>
        /// Maps each non-null element of the source sequence to a destination instance using the supplied delegate.
        /// Returns <see langword="null"/> when the source sequence is <see langword="null"/>.
        /// </summary>
        /// <typeparam name="TSrc">The source element type.</typeparam>
        /// <typeparam name="TDest">The destination element type.</typeparam>
        /// <param name="source">The source sequence to iterate. A null value yields a null result.</param>
        /// <param name="map">The compiled delegate used to convert each non-null source element.</param>
        /// <returns>
        /// A <see cref="List{TDest}"/> containing the mapped destination elements, or <see langword="null"/> when
        /// <paramref name="source"/> is <see langword="null"/>.
        /// </returns>
        [SuppressMessage(
            "Major Code Smell",
            "S3267:Loops should be simplified with \"LINQ\" expressions",
            Justification = "Explicit loop used to avoid LINQ overhead (allocations, delegates, and enumerator costs) in performance-critical path.")]
        public static List<TDest> MapList<TSrc, TDest>(
            IEnumerable<TSrc> source,
            Func<TSrc, TDest> map)
        {
            if (source == null)
                return null;

            var result = source is ICollection<TSrc> collection
                ? new List<TDest>(collection.Count)
                : new List<TDest>();

            foreach (var item in source)
            {
                if (item != null)
                    result.Add(map(item));
            }

            return result;
        }
    }
}
