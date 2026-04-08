using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Internals
{
    /// <summary>
    /// Covers branch behavior in <c>NestedCollectionHelper.MapList</c> through reflection-based invocation.
    /// </summary>
    public class NestedCollectionHelperTests
    {
        /// <summary>
        /// Stores the runtime type for the internal nested collection helper.
        /// </summary>
        private static readonly Type NestedCollectionHelperType = typeof(HydrixMapper).Assembly.GetType(
            "Hydrix.Mapper.Internals.NestedCollectionHelper",
            throwOnError: true);

        /// <summary>
        /// Stores the open generic method info for <c>MapList</c>.
        /// </summary>
        private static readonly MethodInfo MapListMethod = NestedCollectionHelperType.GetMethod(
            "MapList",
            BindingFlags.Static | BindingFlags.Public) ?? throw new InvalidOperationException(
            "NestedCollectionHelper.MapList method was not found.");

        /// <summary>
        /// Verifies that the helper returns <see langword="null"/> when the source sequence is <see langword="null"/>.
        /// </summary>
        [Fact]
        public void MapList_ReturnsNull_WhenSourceIsNull()
        {
            var result = InvokeMapList<string, string>(
                null,
                value => value);

            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that the helper maps an <see cref="ICollection{T}"/> source and preserves the expected element count.
        /// </summary>
        [Fact]
        public void MapList_MapsCollectionSource()
        {
            var source = new List<string>
            {
                "A",
                "B",
            };

            var result = InvokeMapList<string, string>(
                source,
                value => value + "1");

            Assert.Equal(
                2,
                result.Count);
            Assert.Equal(
                "A1",
                result[0]);
            Assert.Equal(
                "B1",
                result[1]);
            Assert.Equal(
                2,
                result.Capacity);
        }

        /// <summary>
        /// Verifies that the helper pre-sizes from an <see cref="IReadOnlyCollection{T}"/> source that does not also
        /// implement <see cref="ICollection{T}"/>.
        /// </summary>
        [Fact]
        public void MapList_PreSizesFromReadOnlyCollectionSource()
        {
            var source = new StringReadOnlySequence(
                "A",
                "B",
                "C");

            var result = InvokeMapList<string, string>(
                source,
                value => value + "R");

            Assert.Equal(
                3,
                result.Count);
            Assert.Equal(
                3,
                result.Capacity);
            Assert.Equal(
                "AR",
                result[0]);
            Assert.Equal(
                "BR",
                result[1]);
            Assert.Equal(
                "CR",
                result[2]);
        }

        /// <summary>
        /// Verifies that the helper pre-sizes from a non-generic <see cref="System.Collections.ICollection"/> source
        /// when no generic collection interface is available.
        /// </summary>
        [Fact]
        public void MapList_PreSizesFromNonGenericCollectionSource()
        {
            var source = new StringNonGenericCollection(
                "A",
                "B",
                "C");

            var result = InvokeMapList<string, string>(
                source,
                value => value + "N");

            Assert.Equal(
                3,
                result.Count);
            Assert.Equal(
                3,
                result.Capacity);
            Assert.Equal(
                "AN",
                result[0]);
            Assert.Equal(
                "BN",
                result[1]);
            Assert.Equal(
                "CN",
                result[2]);
        }

        /// <summary>
        /// Verifies that the helper maps a non-collection enumerable source, exercising the fallback list-allocation branch.
        /// </summary>
        [Fact]
        public void MapList_MapsNonCollectionEnumerableSource()
        {
            var result = InvokeMapList<string, string>(
                Yield(
                    "A",
                    null,
                    "B"),
                value => value + "2");

            Assert.Equal(
                2,
                result.Count);
            Assert.Equal(
                "A2",
                result[0]);
            Assert.Equal(
                "B2",
                result[1]);
        }

        /// <summary>
        /// Produces a deferred sequence that does not implement <see cref="ICollection{T}"/>.
        /// </summary>
        /// <param name="items">The sequence items to yield.</param>
        /// <returns>A deferred enumerable with the supplied items.</returns>
        private static IEnumerable<string> Yield(
            params string[] items)
        {
            foreach (var item in items)
                yield return item;
        }

        /// <summary>
        /// Invokes <c>NestedCollectionHelper.MapList</c> for the supplied generic types.
        /// </summary>
        /// <typeparam name="TSrc">The source element type.</typeparam>
        /// <typeparam name="TDest">The destination element type.</typeparam>
        /// <param name="source">The source enumerable passed to the helper.</param>
        /// <param name="map">The mapping delegate passed to the helper.</param>
        /// <returns>The mapped destination list returned by the helper.</returns>
        private static List<TDest> InvokeMapList<TSrc, TDest>(
            IEnumerable<TSrc> source,
            Func<TSrc, TDest> map)
        {
            var closedMethod = MapListMethod.MakeGenericMethod(
                typeof(TSrc),
                typeof(TDest));

            return (List<TDest>)closedMethod.Invoke(
                null,
                new object[]
                {
                    source,
                    map,
                });
        }
    }
}
