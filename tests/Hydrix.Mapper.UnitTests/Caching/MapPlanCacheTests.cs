using Hydrix.Mapper.Caching;
using Hydrix.Mapper.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Caching
{
    /// <summary>
    /// Validates cache reuse, equality behavior, and concurrent access patterns for <see cref="MapPlanCache"/>.
    /// </summary>
    [Collection(CacheStateTestCollection.Name)]
    public class MapPlanCacheTests
    {
        /// <summary>
        /// Represents a source object with two mappable members.
        /// </summary>
        private sealed class Source
        {
            /// <summary>
            /// Gets or sets the numeric identifier copied into destination objects.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the textual value copied into destination objects.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents an alternate source type used to force the cache lookup loop through a non-matching entry.
        /// </summary>
        private sealed class Source2
        {
            /// <summary>
            /// Gets or sets the numeric identifier copied into destination objects.
            /// </summary>
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents the primary destination type used to validate cache reuse and mapper output.
        /// </summary>
        private sealed class Dest
        {
            /// <summary>
            /// Gets or sets the mapped identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the mapped text.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents an alternate destination type used to confirm cache separation by destination type.
        /// </summary>
        private sealed class Dest2
        {
            /// <summary>
            /// Gets or sets the mapped identifier.
            /// </summary>
            public int Id { get; set; }
        }

        /// <summary>
        /// Verifies that the cache returns the exact same compiled plan instance for the same source and destination pair.
        /// </summary>
        [Fact]
        public void GetOrAdd_ReturnsSamePlanInstance_ForSameTypePair()
        {
            MapPlanCache.Clear();
            var options = new HydrixMapperOptions();

            var firstPlan = MapPlanCache.GetOrAdd(
                typeof(Source),
                typeof(Dest),
                options);
            var secondPlan = MapPlanCache.GetOrAdd(
                typeof(Source),
                typeof(Dest),
                options);

            Assert.Same(
                firstPlan,
                secondPlan);
        }

        /// <summary>
        /// Verifies that the cache stores separate plans when the destination type changes.
        /// </summary>
        [Fact]
        public void GetOrAdd_ReturnsDifferentPlan_ForDifferentDestination()
        {
            MapPlanCache.Clear();
            var options = new HydrixMapperOptions();

            var firstPlan = MapPlanCache.GetOrAdd(
                typeof(Source),
                typeof(Dest),
                options);
            var secondPlan = MapPlanCache.GetOrAdd(
                typeof(Source),
                typeof(Dest2),
                options);

            Assert.NotSame(
                firstPlan,
                secondPlan);
        }

        /// <summary>
        /// Verifies that a type pair is reported as uncached before any plan compilation occurs.
        /// </summary>
        [Fact]
        public void IsCached_ReturnsFalse_BeforeFirstAccess()
        {
            MapPlanCache.Clear();

            Assert.False(
                MapPlanCache.IsCached(
                    typeof(Source),
                    typeof(Dest)));
        }

        /// <summary>
        /// Verifies that a type pair is reported as cached after the first plan has been built.
        /// </summary>
        [Fact]
        public void IsCached_ReturnsTrue_AfterFirstAccess()
        {
            MapPlanCache.Clear();
            var options = new HydrixMapperOptions();

            MapPlanCache.GetOrAdd(
                typeof(Source),
                typeof(Dest),
                options);

            Assert.True(
                MapPlanCache.IsCached(
                    typeof(Source),
                    typeof(Dest)));
        }

        /// <summary>
        /// Verifies that the type-pair lookup returns <see langword="false"/> when the cache contains only non-matching
        /// entries.
        /// </summary>
        [Fact]
        public void IsCached_ReturnsFalse_WhenOnlyDifferentPairsAreCached()
        {
            MapPlanCache.Clear();
            var options = new HydrixMapperOptions();

            MapPlanCache.GetOrAdd(
                typeof(Source),
                typeof(Dest2),
                options);
            MapPlanCache.GetOrAdd(
                typeof(Source2),
                typeof(Dest),
                options);

            Assert.False(
                MapPlanCache.IsCached(
                    typeof(Source),
                    typeof(Dest)));
        }

        /// <summary>
        /// Verifies that <see cref="MapPlanKey"/> compares equal only when both the source and destination types match.
        /// </summary>
        [Fact]
        public void MapPlanKey_Equality_WorksCorrectly()
        {
            var firstKey = new MapPlanKey(
                typeof(Source),
                typeof(Dest));
            var secondKey = new MapPlanKey(
                typeof(Source),
                typeof(Dest));
            var thirdKey = new MapPlanKey(
                typeof(Source),
                typeof(Dest2));

            Assert.Equal(
                firstKey,
                secondKey);
            Assert.NotEqual(
                firstKey,
                thirdKey);
            Assert.Equal(
                firstKey.GetHashCode(),
                secondKey.GetHashCode());
        }

        /// <summary>
        /// Verifies that the mapper reuses the previously compiled cache entry on a second mapping call.
        /// </summary>
        [Fact]
        public void Map_ReusesCachedPlan_OnSubsequentCalls()
        {
            MapPlanCache.Clear();
            var mapper = new HydrixMapper(
                new HydrixMapperOptions());
            var source = new Source
            {
                Id = 1,
                Name = "X",
            };

            mapper.Map<Dest>(
                source);

            Assert.True(
                MapPlanCache.IsCached(
                    typeof(Source),
                    typeof(Dest)));

            var destination = mapper.Map<Dest>(
                source);

            Assert.Equal(
                1,
                destination.Id);
            Assert.Equal(
                "X",
                destination.Name);
        }

        /// <summary>
        /// Verifies that multiple concurrent cache requests observe a valid plan without corrupting the cache state.
        /// </summary>
        [Fact]
        public void MapPlanCache_IsThreadSafe_UnderConcurrentAccess()
        {
            MapPlanCache.Clear();
            var options = new HydrixMapperOptions();
            var tasks = new List<Task<Plans.MapPlan>>(32);

            for (var index = 0; index < 32; index++)
            {
                tasks.Add(
                    Task.Run(
                        () => MapPlanCache.GetOrAdd(
                            typeof(Source),
                            typeof(Dest),
                            options)));
            }

            Task.WaitAll(
                tasks.ToArray());

            var firstPlan = tasks[0].Result;
            foreach (var task in tasks)
            {
                Assert.NotNull(
                    task.Result);
                Assert.Same(
                    firstPlan,
                    task.Result);
            }

            Assert.True(
                MapPlanCache.IsCached(
                    typeof(Source),
                    typeof(Dest)));
        }

        /// <summary>
        /// Verifies that concurrent mapper executions produce correct destination values while sharing the cache safely.
        /// </summary>
        [Fact]
        public void Mapper_ProducesCorrectResult_UnderConcurrentAccess()
        {
            MapPlanCache.Clear();
            var mapper = new HydrixMapper(
                new HydrixMapperOptions());
            var results = new Dest[64];

            Parallel.For(
                0,
                64,
                index =>
                {
                    results[index] = mapper.Map<Dest>(
                        new Source
                        {
                            Id = index,
                            Name = index.ToString(),
                        });
                });

            for (var index = 0; index < 64; index++)
            {
                Assert.Equal(
                    index,
                    results[index].Id);
                Assert.Equal(
                    index.ToString(),
                    results[index].Name);
            }
        }

        /// <summary>
        /// Verifies that the object-based equality overload rejects values that are not cache keys.
        /// </summary>
        [Fact]
        public void MapPlanKey_EqualsObject_ReturnsFalse_ForDifferentObjectType()
        {
            var key = new MapPlanKey(
                typeof(Source),
                typeof(Dest));

            Assert.False(
                key.Equals(
                    "not-a-key"));
        }
    }
}
