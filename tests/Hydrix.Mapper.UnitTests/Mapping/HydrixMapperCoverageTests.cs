using Hydrix.Mapper.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Mapping
{
    /// <summary>
    /// Covers helper branches in list buffering and plan compilation that are easiest to reach through focused source and
    /// destination shapes.
    /// </summary>
    public class HydrixMapperCoverageTests
    {
        /// <summary>
        /// Represents the homogeneous source model shared by the list-buffering scenarios.
        /// </summary>
        private sealed class PersonEntity
        {
            /// <summary>
            /// Gets or sets the identifier copied into the destination model.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name copied into the destination model.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents the destination model shared by the list-buffering scenarios.
        /// </summary>
        private sealed class PersonDto
        {
            /// <summary>
            /// Gets or sets the mapped identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the mapped name.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents a source type whose property is write-only and therefore must be excluded from the source-property
        /// lookup.
        /// </summary>
        private sealed class SourceWithWriteOnlyHidden
        {
            private string _hidden;

            /// <summary>
            /// Sets the hidden value that should never be read by the mapper because the property has no getter.
            /// </summary>
            public string Hidden
            {
                set => _hidden = value;
            }
        }

        /// <summary>
        /// Represents a source type whose getter exists but is not public, forcing the source-property lookup to evaluate
        /// the accessor-visibility branch.
        /// </summary>
        public sealed class SourceWithExplicitPrivateGetter
        {
            private string _hidden;

            /// <summary>
            /// Gets or sets the hidden value used by the accessor-visibility scenario.
            /// </summary>
            public string Hidden
            {
                private get { return _hidden; }
                set { _hidden = value; }
            }
        }

        /// <summary>
        /// Represents a source type that exposes only an indexer, which must be ignored by the source-property lookup.
        /// </summary>
        private sealed class SourceWithIndexerOnly
        {
            private readonly string[] _values = new string[1];

            /// <summary>
            /// Gets or sets the indexed value exposed by the source type.
            /// </summary>
            /// <param name="index">The requested position.</param>
            /// <returns>The indexed value.</returns>
            public string this[int index]
            {
                get { return _values[index]; }
                set { _values[index] = value; }
            }
        }

        /// <summary>
        /// Represents a source type used to confirm that destination properties without setters are ignored.
        /// </summary>
        private sealed class SourceWithVisibleHidden
        {
            /// <summary>
            /// Gets or sets the mapped identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the hidden value that would be copied if the destination property accepted writes.
            /// </summary>
            public string Hidden { get; set; }
        }

        /// <summary>
        /// Represents a destination type that can receive the hidden value when a readable source property exists.
        /// </summary>
        private sealed class DestinationWithVisibleHidden
        {
            /// <summary>
            /// Gets or sets the mapped hidden value.
            /// </summary>
            public string Hidden { get; set; }
        }

        /// <summary>
        /// Represents an alternate source type used by private <c>TypePair</c> equality coverage.
        /// </summary>
        private sealed class AlternateSource
        {
        }

        /// <summary>
        /// Represents an alternate destination type used by private <c>TypePair</c> equality coverage.
        /// </summary>
        private sealed class AlternateDestination
        {
        }

        /// <summary>
        /// Represents a destination type whose setter exists but is not public, forcing the destination-visibility branch
        /// in the plan builder.
        /// </summary>
        public sealed class DestinationWithExplicitPrivateSetter
        {
            private string _hidden = "kept";

            /// <summary>
            /// Gets or sets the mapped identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets the hidden value that should keep its initializer because the setter is not public.
            /// </summary>
            public string Hidden
            {
                get { return _hidden; }
                private set { _hidden = value; }
            }
        }

        /// <summary>
        /// Represents a destination type whose hidden property is getter-only and must therefore remain unchanged.
        /// </summary>
        private sealed class DestinationWithReadOnlyHidden
        {
            /// <summary>
            /// Gets or sets the mapped identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets the hidden value that should keep its initializer because the property has no setter.
            /// </summary>
            public string Hidden { get; } = "kept";
        }

        /// <summary>
        /// Wraps a sequence as an object-only read-only collection so the mapper exercises the specialized buffer path for
        /// <see cref="IReadOnlyCollection{T}"/> without also matching <see cref="ICollection"/>.
        /// </summary>
        private sealed class ObjectReadOnlySequence : IReadOnlyCollection<object>
        {
            private readonly object[] _items;

            /// <summary>
            /// Initializes a new instance of the <see cref="ObjectReadOnlySequence"/> class.
            /// </summary>
            /// <param name="items">The wrapped source items.</param>
            public ObjectReadOnlySequence(
                params object[] items)
            {
                _items = items;
            }

            /// <summary>
            /// Gets the number of wrapped elements.
            /// </summary>
            public int Count => _items.Length;

            /// <summary>
            /// Returns the generic enumerator for the wrapped items.
            /// </summary>
            /// <returns>The generic enumerator.</returns>
            public IEnumerator<object> GetEnumerator() =>
                ((IEnumerable<object>)_items).GetEnumerator();

            /// <summary>
            /// Returns the non-generic enumerator for the wrapped items.
            /// </summary>
            /// <returns>The non-generic enumerator.</returns>
            IEnumerator IEnumerable.GetEnumerator() =>
                _items.GetEnumerator();
        }

        /// <summary>
        /// Wraps a typed sequence as a generic collection without implementing <see cref="IList{T}"/> so the mapper
        /// reaches the generic collection pre-sizing path in the fallback buffer factory.
        /// </summary>
        /// <typeparam name="T">The wrapped element type.</typeparam>
        private sealed class GenericCollectionOnlySequence<T> : ICollection<T>
        {
            /// <summary>
            /// Stores the wrapped items.
            /// </summary>
            private readonly List<T> _items;

            /// <summary>
            /// Initializes a new instance of the <see cref="GenericCollectionOnlySequence{T}"/> class.
            /// </summary>
            /// <param name="items">The wrapped source items.</param>
            public GenericCollectionOnlySequence(
                params T[] items)
            {
                _items = new List<T>(
                    items);
            }

            /// <summary>
            /// Gets the number of wrapped elements.
            /// </summary>
            public int Count => _items.Count;

            /// <summary>
            /// Gets a value indicating whether this collection is read-only.
            /// </summary>
            public bool IsReadOnly => false;

            /// <summary>
            /// Adds an item to the wrapped collection.
            /// </summary>
            /// <param name="item">The item to add.</param>
            public void Add(
                T item) =>
                _items.Add(
                    item);

            /// <summary>
            /// Removes all items from the wrapped collection.
            /// </summary>
            public void Clear() =>
                _items.Clear();

            /// <summary>
            /// Determines whether the wrapped collection contains the specified item.
            /// </summary>
            /// <param name="item">The item to locate.</param>
            /// <returns><see langword="true"/> when the item is present; otherwise, <see langword="false"/>.</returns>
            public bool Contains(
                T item) =>
                _items.Contains(
                    item);

            /// <summary>
            /// Copies wrapped items to the supplied array.
            /// </summary>
            /// <param name="array">The destination array.</param>
            /// <param name="arrayIndex">The start index in the destination array.</param>
            public void CopyTo(
                T[] array,
                int arrayIndex) =>
                _items.CopyTo(
                    array,
                    arrayIndex);

            /// <summary>
            /// Removes the first matching item from the wrapped collection.
            /// </summary>
            /// <param name="item">The item to remove.</param>
            /// <returns><see langword="true"/> when an item is removed; otherwise, <see langword="false"/>.</returns>
            public bool Remove(
                T item) =>
                _items.Remove(
                    item);

            /// <summary>
            /// Returns the generic enumerator for the wrapped items.
            /// </summary>
            /// <returns>The generic enumerator.</returns>
            public IEnumerator<T> GetEnumerator() =>
                _items.GetEnumerator();

            /// <summary>
            /// Returns the non-generic enumerator for the wrapped items.
            /// </summary>
            /// <returns>The non-generic enumerator.</returns>
            IEnumerator IEnumerable.GetEnumerator() =>
                _items.GetEnumerator();
        }

        /// <summary>
        /// Wraps a typed sequence as a read-only collection without implementing <see cref="ICollection{T}"/>.
        /// </summary>
        /// <typeparam name="T">The wrapped element type.</typeparam>
        private sealed class TypedReadOnlySequence<T> : IReadOnlyCollection<T>
        {
            private readonly T[] _items;

            /// <summary>
            /// Initializes a new instance of the <see cref="TypedReadOnlySequence{T}"/> class.
            /// </summary>
            /// <param name="items">The wrapped source items.</param>
            public TypedReadOnlySequence(
                params T[] items)
            {
                _items = items;
            }

            /// <summary>
            /// Gets the number of wrapped elements.
            /// </summary>
            public int Count => _items.Length;

            /// <summary>
            /// Returns the generic enumerator for the wrapped items.
            /// </summary>
            /// <returns>The generic enumerator.</returns>
            public IEnumerator<T> GetEnumerator() =>
                ((IEnumerable<T>)_items).GetEnumerator();

            /// <summary>
            /// Returns the non-generic enumerator for the wrapped items.
            /// </summary>
            /// <returns>The non-generic enumerator.</returns>
            IEnumerator IEnumerable.GetEnumerator() =>
                _items.GetEnumerator();
        }

        /// <summary>
        /// Wraps a typed sequence as a non-generic collection so the mapper exercises the fallback count path that only
        /// checks <see cref="ICollection"/>.
        /// </summary>
        /// <typeparam name="T">The wrapped element type.</typeparam>
        private sealed class NonGenericCollectionSequence<T> : IEnumerable<T>, ICollection
        {
            private readonly T[] _items;

            /// <summary>
            /// Initializes a new instance of the <see cref="NonGenericCollectionSequence{T}"/> class.
            /// </summary>
            /// <param name="items">The wrapped source items.</param>
            public NonGenericCollectionSequence(
                params T[] items)
            {
                _items = items;
            }

            /// <summary>
            /// Gets the number of wrapped elements.
            /// </summary>
            public int Count => _items.Length;

            /// <summary>
            /// Gets a value indicating whether access to the collection is synchronized.
            /// </summary>
            public bool IsSynchronized => false;

            /// <summary>
            /// Gets the synchronization root for the collection.
            /// </summary>
            public object SyncRoot => this;

            /// <summary>
            /// Copies the wrapped items to the supplied array.
            /// </summary>
            /// <param name="array">The target array.</param>
            /// <param name="index">The start index in the target array.</param>
            public void CopyTo(
                Array array,
                int index) =>
                _items.CopyTo(
                    array,
                    index);

            /// <summary>
            /// Returns the generic enumerator for the wrapped items.
            /// </summary>
            /// <returns>The generic enumerator.</returns>
            public IEnumerator<T> GetEnumerator() =>
                ((IEnumerable<T>)_items).GetEnumerator();

            /// <summary>
            /// Returns the non-generic enumerator for the wrapped items.
            /// </summary>
            /// <returns>The non-generic enumerator.</returns>
            IEnumerator IEnumerable.GetEnumerator() =>
                _items.GetEnumerator();
        }

        /// <summary>
        /// Creates a mapper configured with the default Hydrix options.
        /// </summary>
        /// <returns>A mapper instance ready for the coverage scenarios in this class.</returns>
        private static HydrixMapper CreateMapper() =>
            new HydrixMapper(
                new HydrixMapperOptions());

        /// <summary>
        /// Produces a pure object sequence that exposes no count-related collection interfaces.
        /// </summary>
        /// <param name="items">The items to enumerate.</param>
        /// <returns>A deferred object sequence.</returns>
        private static IEnumerable<object> YieldObjects(
            params object[] items)
        {
            foreach (var item in items)
                yield return item;
        }

        /// <summary>
        /// Produces a pure typed sequence that exposes no count-related collection interfaces.
        /// </summary>
        /// <param name="items">The items to enumerate.</param>
        /// <returns>A deferred typed sequence.</returns>
        private static IEnumerable<PersonEntity> YieldEntities(
            params PersonEntity[] items)
        {
            foreach (var item in items)
                yield return item;
        }

        /// <summary>
        /// Creates an instance of the private <c>HydrixMapper.TypePair</c> key for focused equality-branch coverage.
        /// </summary>
        /// <param name="sourceType">The source type captured by the key.</param>
        /// <param name="destinationType">The destination type captured by the key.</param>
        /// <returns>A boxed private key instance.</returns>
        private static object CreateTypePair(
            Type sourceType,
            Type destinationType)
        {
            var typePairType = typeof(HydrixMapper).GetNestedType(
                "TypePair",
                BindingFlags.NonPublic);

            Assert.NotNull(
                typePairType);

            var pair = Activator.CreateInstance(
                typePairType,
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                args: new object[]
                {
                    sourceType,
                    destinationType,
                },
                culture: null);

            Assert.NotNull(
                pair);

            return pair;
        }

        /// <summary>
        /// Verifies that the object-based list API maps sources exposed only as an object read-only collection.
        /// </summary>
        [Fact]
        public void MapList_ObjectOverload_MapsReadOnlyObjectCollection()
        {
            IEnumerable<object> sources = new ObjectReadOnlySequence(
                new PersonEntity
                {
                    Id = 1,
                    Name = "A",
                },
                new PersonEntity
                {
                    Id = 2,
                    Name = "B",
                });

            var result = CreateMapper().MapList<PersonDto>(
                sources);

            Assert.Equal(
                2,
                result.Count);
            Assert.Equal(
                "A",
                result[0].Name);
            Assert.Equal(
                "B",
                result[1].Name);
        }

        /// <summary>
        /// Verifies that the object-based list API also maps a deferred sequence with no count information.
        /// </summary>
        [Fact]
        public void MapList_ObjectOverload_MapsPureEnumerableSequence()
        {
            var result = CreateMapper().MapList<PersonDto>(
                YieldObjects(
                    new PersonEntity
                    {
                        Id = 1,
                        Name = "A",
                    },
                    new PersonEntity
                    {
                        Id = 2,
                        Name = "B",
                    }));

            Assert.Equal(
                2,
                result.Count);
            Assert.Equal(
                1,
                result[0].Id);
            Assert.Equal(
                2,
                result[1].Id);
        }

        /// <summary>
        /// Verifies that the strongly typed list API maps sources exposed only as a typed read-only collection.
        /// </summary>
        [Fact]
        public void MapList_TypedOverload_MapsReadOnlyTypedCollection()
        {
            IEnumerable<PersonEntity> sources = new TypedReadOnlySequence<PersonEntity>(
                new PersonEntity
                {
                    Id = 1,
                    Name = "A",
                },
                new PersonEntity
                {
                    Id = 2,
                    Name = "B",
                });

            var result = CreateMapper().MapList<PersonEntity, PersonDto>(
                sources);

            Assert.Equal(
                2,
                result.Count);
            Assert.Equal(
                "A",
                result[0].Name);
            Assert.Equal(
                "B",
                result[1].Name);
        }

        /// <summary>
        /// Verifies that the strongly typed list API maps sources exposed only through the non-generic collection count
        /// path.
        /// </summary>
        [Fact]
        public void MapList_TypedOverload_MapsNonGenericCollection()
        {
            IEnumerable<PersonEntity> sources = new NonGenericCollectionSequence<PersonEntity>(
                new PersonEntity
                {
                    Id = 3,
                    Name = "C",
                },
                new PersonEntity
                {
                    Id = 4,
                    Name = "D",
                });

            var result = CreateMapper().MapList<PersonEntity, PersonDto>(
                sources);

            Assert.Equal(
                2,
                result.Count);
            Assert.Equal(
                3,
                result[0].Id);
            Assert.Equal(
                4,
                result[1].Id);
        }

        /// <summary>
        /// Verifies that the strongly typed list API also maps a deferred sequence with no count information.
        /// </summary>
        [Fact]
        public void MapList_TypedOverload_MapsPureEnumerableSequence()
        {
            var result = CreateMapper().MapList<PersonEntity, PersonDto>(
                YieldEntities(
                    new PersonEntity
                    {
                        Id = 5,
                        Name = "E",
                    },
                    new PersonEntity
                    {
                        Id = 6,
                        Name = "F",
                    }));

            Assert.Equal(
                2,
                result.Count);
            Assert.Equal(
                "E",
                result[0].Name);
            Assert.Equal(
                "F",
                result[1].Name);
        }

        /// <summary>
        /// Verifies that the strongly typed list API returns the shared empty array when the source is an empty
        /// <see cref="List{T}"/>, covering the span fast-path empty branch.
        /// </summary>
        [Fact]
        public void MapList_TypedOverload_ReturnsArrayEmpty_WhenListSourceIsEmpty()
        {
            IReadOnlyList<PersonDto> result = CreateMapper().MapList<PersonEntity, PersonDto>(
                new List<PersonEntity>());

            Assert.Same(
                Array.Empty<PersonDto>(),
                result);
        }

        /// <summary>
        /// Verifies that the strongly typed list API returns the shared empty array when the source is an empty
        /// <see cref="IList{T}"/> implementation.
        /// </summary>
        [Fact]
        public void MapList_TypedOverload_ReturnsArrayEmpty_WhenIListSourceIsEmpty()
        {
            IReadOnlyList<PersonDto> result = CreateMapper().MapList<PersonEntity, PersonDto>(
                Array.Empty<PersonEntity>());

            Assert.Same(
                Array.Empty<PersonDto>(),
                result);
        }

        /// <summary>
        /// Verifies that the fallback typed-sequence path pre-sizes from <see cref="ICollection{T}"/> and skips null
        /// elements during enumeration.
        /// </summary>
        [Fact]
        public void MapList_TypedOverload_FallbackCollection_SkipsNullElements()
        {
            IEnumerable<PersonEntity> sources = new GenericCollectionOnlySequence<PersonEntity>(
                null,
                new PersonEntity
                {
                    Id = 7,
                    Name = "G",
                });

            var result = CreateMapper().MapList<PersonEntity, PersonDto>(
                sources);

            Assert.Single(
                result);
            Assert.Equal(
                7,
                result[0].Id);
            Assert.Equal(
                "G",
                result[0].Name);
        }

        /// <summary>
        /// Verifies that the strongly typed list API maps a non-empty array source, exercising the index-loop fast path
        /// on all target frameworks.
        /// </summary>
        [Fact]
        public void MapList_TypedOverload_MapsArraySource()
        {
            PersonEntity[] sources =
            {
                new PersonEntity
                {
                    Id = 10,
                    Name = "X",
                },
                new PersonEntity
                {
                    Id = 11,
                    Name = "Y",
                },
            };

            var result = CreateMapper().MapList<PersonEntity, PersonDto>(
                sources);

            Assert.Equal(
                2,
                result.Count);
            Assert.Equal(
                10,
                result[0].Id);
            Assert.Equal(
                "X",
                result[0].Name);
            Assert.Equal(
                11,
                result[1].Id);
            Assert.Equal(
                "Y",
                result[1].Name);
        }

        /// <summary>
        /// Verifies that the strongly typed list API skips null elements when the source is provided as a non-List
        /// <see cref="IList{T}"/>, covering the null-skip branch inside the index-loop fast path.
        /// </summary>
        [Fact]
        public void MapList_TypedOverload_ArraySource_SkipsNullElements()
        {
            IList<PersonEntity> sources = new PersonEntity[]
            {
                new PersonEntity
                {
                    Id = 1,
                    Name = "A",
                },
                null,
                new PersonEntity
                {
                    Id = 2,
                    Name = "B",
                },
            };

            var result = CreateMapper().MapList<PersonEntity, PersonDto>(
                sources);

            Assert.Equal(
                2,
                result.Count);
            Assert.Equal(
                1,
                result[0].Id);
            Assert.Equal(
                "A",
                result[0].Name);
            Assert.Equal(
                2,
                result[1].Id);
            Assert.Equal(
                "B",
                result[1].Name);
        }

        /// <summary>
        /// Verifies that destination properties without setters are ignored during plan compilation.
        /// </summary>
        [Fact]
        public void Map_IgnoresDestinationProperty_WithoutSetter()
        {
            var destination = CreateMapper().Map<DestinationWithReadOnlyHidden>(
                new SourceWithVisibleHidden
                {
                    Id = 10,
                    Hidden = "secret",
                });

            Assert.Equal(
                10,
                destination.Id);
            Assert.Equal(
                "kept",
                destination.Hidden);
        }

        /// <summary>
        /// Verifies that destination properties with non-public setters are also ignored during plan compilation.
        /// </summary>
        [Fact]
        public void Map_IgnoresDestinationProperty_WithExplicitNonPublicSetter()
        {
            var destination = CreateMapper().Map<DestinationWithExplicitPrivateSetter>(
                new SourceWithVisibleHidden
                {
                    Id = 11,
                    Hidden = "secret",
                });

            Assert.Equal(
                11,
                destination.Id);
            Assert.Equal(
                "kept",
                destination.Hidden);
        }

        /// <summary>
        /// Verifies that source properties with explicit non-public getters are ignored during plan compilation.
        /// </summary>
        [Fact]
        public void Map_IgnoresSourceProperty_WithExplicitNonPublicGetter()
        {
            var source = new SourceWithExplicitPrivateGetter();
            source.Hidden = "secret";

            var destination = CreateMapper().Map<DestinationWithVisibleHidden>(
                source);

            Assert.Null(
                destination.Hidden);
        }

        /// <summary>
        /// Verifies that source indexers are ignored during source-property discovery.
        /// </summary>
        [Fact]
        public void Map_IgnoresSourceIndexerProperty()
        {
            var source = new SourceWithIndexerOnly();
            source[0] = "secret";

            var destination = CreateMapper().Map<DestinationWithVisibleHidden>(
                source);

            Assert.Null(
                destination.Hidden);
        }

        /// <summary>
        /// Verifies that mapping still succeeds when every candidate source property is filtered out of the source-property
        /// lookup.
        /// </summary>
        [Fact]
        public void Map_ReturnsDefaultDestination_WhenNoReadableSourcePropertiesExist()
        {
            var source = new SourceWithWriteOnlyHidden();
            source.Hidden = "secret";

            var destination = CreateMapper().Map<DestinationWithVisibleHidden>(
                source);

            Assert.Null(
                destination.Hidden);
        }

        /// <summary>
        /// Verifies that private strongly typed key equality evaluates the destination comparison branch when source types
        /// match but destination types differ.
        /// </summary>
        [Fact]
        public void TypePairEquals_ReturnsFalse_WhenOnlyDestinationTypeDiffers()
        {
            var left = CreateTypePair(
                typeof(PersonEntity),
                typeof(PersonDto));
            var right = CreateTypePair(
                typeof(PersonEntity),
                typeof(AlternateDestination));

            var result = (bool)left.GetType().GetMethod(
                    "Equals",
                    new[]
                    {
                        left.GetType(),
                    })
                .Invoke(
                    left,
                    new[]
                    {
                        right,
                    });

            Assert.False(
                result);
        }

        /// <summary>
        /// Verifies that private strongly typed key equality returns <see langword="false"/> when source types differ.
        /// </summary>
        [Fact]
        public void TypePairEquals_ReturnsFalse_WhenSourceTypeDiffers()
        {
            var left = CreateTypePair(
                typeof(PersonEntity),
                typeof(PersonDto));
            var right = CreateTypePair(
                typeof(AlternateSource),
                typeof(PersonDto));

            var result = (bool)left.GetType().GetMethod(
                    "Equals",
                    new[]
                    {
                        left.GetType(),
                    })
                .Invoke(
                    left,
                    new[]
                    {
                        right,
                    });

            Assert.False(
                result);
        }

        /// <summary>
        /// Verifies that private object-based key equality returns <see langword="true"/> for equivalent key instances.
        /// </summary>
        [Fact]
        public void TypePairEqualsObject_ReturnsTrue_WhenKeysMatch()
        {
            var left = CreateTypePair(
                typeof(PersonEntity),
                typeof(PersonDto));
            object right = CreateTypePair(
                typeof(PersonEntity),
                typeof(PersonDto));

            var result = (bool)left.GetType().GetMethod(
                    "Equals",
                    new[]
                    {
                        typeof(object),
                    })
                .Invoke(
                    left,
                    new[]
                    {
                        right,
                    });

            Assert.True(
                result);
        }

        /// <summary>
        /// Verifies that private object-based key equality returns <see langword="false"/> when the compared object is
        /// not a key instance.
        /// </summary>
        [Fact]
        public void TypePairEqualsObject_ReturnsFalse_WhenComparedObjectIsDifferentType()
        {
            var left = CreateTypePair(
                typeof(PersonEntity),
                typeof(PersonDto));
            object other = new AlternateSource();

            var result = (bool)left.GetType().GetMethod(
                    "Equals",
                    new[]
                    {
                        typeof(object),
                    })
                .Invoke(
                    left,
                    new[]
                    {
                        other,
                    });

            Assert.False(
                result);
        }
    }
}
