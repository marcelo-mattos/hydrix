using System.Collections;
using System.Collections.Generic;

namespace Hydrix.Mapper.UnitTests.Internals
{
    /// <summary>
    /// Exposes a string sequence as <see cref="IReadOnlyCollection{T}"/> without implementing
    /// <see cref="ICollection{T}"/>.
    /// </summary>
    public sealed class StringReadOnlySequence : IReadOnlyCollection<string>
    {
        private readonly string[] _items;

        /// <summary>
        /// Initializes the sequence with the supplied items.
        /// </summary>
        /// <param name="items">The items exposed by the sequence.</param>
        public StringReadOnlySequence(
            params string[] items)
        {
            _items = items;
        }

        /// <summary>
        /// Gets the number of items in the sequence.
        /// </summary>
        public int Count => _items.Length;

        /// <inheritdoc />
        public IEnumerator<string> GetEnumerator()
        {
            for (var index = 0; index < _items.Length; index++)
                yield return _items[index];
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
