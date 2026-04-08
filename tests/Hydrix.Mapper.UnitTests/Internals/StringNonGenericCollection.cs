using System;
using System.Collections;
using System.Collections.Generic;

namespace Hydrix.Mapper.UnitTests.Internals
{
    /// <summary>
    /// Exposes a string sequence as <see cref="ICollection"/> without implementing any generic collection interface.
    /// </summary>
    public sealed class StringNonGenericCollection : IEnumerable<string>, ICollection
    {
        private readonly string[] _items;

        /// <summary>
        /// Initializes the collection with the supplied items.
        /// </summary>
        /// <param name="items">The items exposed by the collection.</param>
        public StringNonGenericCollection(
            params string[] items)
        {
            _items = items;
        }

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public int Count => _items.Length;

        /// <inheritdoc />
        public bool IsSynchronized => false;

        /// <inheritdoc />
        public object SyncRoot => this;

        /// <inheritdoc />
        public void CopyTo(
            Array array,
            int index)
        {
            _items.CopyTo(
                array,
                index);
        }

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
