using Hydrix.Mapper.Builders;
using Hydrix.Mapper.Caching;
using Hydrix.Mapper.Configuration;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        /// Represents the nested collection element source used by enumerable-fallback concurrency coverage.
        /// </summary>
        private sealed class TagEntity
        {
            /// <summary>
            /// Gets or sets the tag value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the nested collection element destination used by enumerable-fallback concurrency coverage.
        /// </summary>
        private sealed class TagDto
        {
            /// <summary>
            /// Gets or sets the mapped tag value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the root source type whose collection property forces the enumerable fallback path.
        /// </summary>
        private sealed class EnumerableSource
        {
            /// <summary>
            /// Gets or sets the source identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the nested collection stored behind a non-indexed enumerable implementation.
            /// </summary>
            public HashSet<TagEntity> Tags { get; set; }
        }

        /// <summary>
        /// Represents the root destination type whose nested collection is mapped via a read-only abstraction.
        /// </summary>
        private sealed class EnumerableDestination
        {
            /// <summary>
            /// Gets or sets the mapped identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the mapped tags.
            /// </summary>
            public IReadOnlyList<TagDto> Tags { get; set; }
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
        public async Task MapPlanCache_IsThreadSafe_UnderConcurrentAccess()
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

            await Task.WhenAll(
                tasks.ToArray());

            var firstPlan = await tasks[0];
            foreach (var task in tasks)
            {
                Assert.NotNull(
                    await task);
                Assert.Same(
                    firstPlan,
                    await task);
            }

            Assert.True(
                MapPlanCache.IsCached(
                    typeof(Source),
                    typeof(Dest)));
        }

        /// <summary>
        /// Verifies that high-contention first access compiles the shared mapping plan exactly once.
        /// </summary>
        [Fact]
        public async Task MapPlanCache_CompilesPlanExactlyOnce_UnderHighContention()
        {
            MapPlanCache.Clear();
            var options = new HydrixMapperOptions();
            using var start = new ManualResetEventSlim(false);
            var tasks = new Task<Plans.MapPlan>[128];

            for (var index = 0; index < tasks.Length; index++)
            {
                tasks[index] = Task.Run(
                    () =>
                    {
                        start.Wait();
                        return MapPlanCache.GetOrAdd(
                            typeof(Source),
                            typeof(Dest),
                            options);
                    });
            }

            start.Set();
            await Task.WhenAll(
                tasks);

            var firstPlan = await tasks[0];
            foreach (var task in tasks)
            {
                Assert.Same(
                    firstPlan,
                    await task);
            }

            Assert.Equal(
                1,
                MapPlanCache.PlanCompilationCount);
        }

        /// <summary>
        /// Verifies that concurrent first-use mapping compiles both the outer plan and enumerable-fallback element
        /// delegate exactly once while preserving correct results.
        /// </summary>
        [Fact]
        public async Task Mapper_CompilesPlanAndElementDelegateExactlyOnce_UnderConcurrentFirstUse()
        {
            MapPlanBuilder.ClearCompilationCachesForTesting();
            MapPlanCache.Clear();

            var options = new HydrixMapperOptions();
            options.MapNested<TagEntity, TagDto>();

            var mapper = new HydrixMapper(options);
            var results = new ConcurrentBag<EnumerableDestination>();
            using var start = new ManualResetEventSlim(false);
            var tasks = new Task[128];

            for (var index = 0; index < tasks.Length; index++)
            {
                var capture = index;
                tasks[index] = Task.Run(
                    () =>
                    {
                        start.Wait();

                        results.Add(
                            mapper.Map<EnumerableDestination>(
                                new EnumerableSource
                                {
                                    Id = capture,
                                    Tags =
                                    new HashSet<TagEntity>
                                    {
                                        new TagEntity
                                        {
                                            Value = capture,
                                        },
                                        new TagEntity
                                        {
                                            Value = capture + 1,
                                        },
                                    },
                                }));
                    });
            }

            start.Set();
            await Task.WhenAll(
                tasks);

            Assert.Equal(
                128,
                results.Count);

            foreach (var result in results)
            {
                Assert.NotNull(
                    result.Tags);
                Assert.Equal(
                    2,
                    result.Tags.Count);

                var values = result.Tags
                    .Select(
                        tag => tag.Value)
                    .OrderBy(
                        value => value)
                    .ToArray();

                Assert.Equal(
                    result.Id,
                    values[0]);
                Assert.Equal(
                    result.Id + 1,
                    values[1]);
            }

            Assert.Equal(
                1,
                MapPlanCache.PlanCompilationCount);
            Assert.Equal(
                1,
                MapPlanBuilder.ElementDelegateCompilationCount);
        }

        /// <summary>
        /// Verifies that the mapper produces correct results when accessed concurrently from multiple threads.
        /// </summary>
        /// <remarks>This test ensures that the mapping operation is thread-safe and that each concurrent
        /// mapping produces the expected result. It is intended to detect issues related to shared state or race
        /// conditions in the mapping logic.</remarks>
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
        /// Verifies that the MapPlanKey.Equals(Object) method returns false when compared to an object of a different
        /// type.
        /// </summary>
        /// <remarks>This test ensures that the Equals method correctly handles comparisons with objects
        /// that are not of type MapPlanKey, returning false as expected.</remarks>
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

        /// <summary>
        /// Verifies that the three-parameter overload of the IsCached method returns true after a mapping plan has been
        /// compiled and added to the cache.
        /// </summary>
        /// <remarks>This test ensures that the cache correctly reflects the presence of a compiled
        /// mapping plan for the specified source type, destination type, and options. It first asserts that the cache
        /// does not contain the plan, adds the plan, and then asserts that the cache reports it as present.</remarks>
        [Fact]
        public void IsCached_ThreeParamOverload_ReturnsTrueAfterCompilation()
        {
            MapPlanCache.Clear();
            var options = new HydrixMapperOptions();

            Assert.False(
                MapPlanCache.IsCached(
                    typeof(Source),
                    typeof(Dest),
                    options));

            MapPlanCache.GetOrAdd(
                typeof(Source),
                typeof(Dest),
                options);

            Assert.True(
                MapPlanCache.IsCached(
                    typeof(Source),
                    typeof(Dest),
                    options));
        }

        /// <summary>
        /// Verifies that the three-parameter overload of the GetOrAdd method correctly populates nested mappings using
        /// the provided HydrixMapperOptions instance.
        /// </summary>
        /// <remarks>This test ensures that when MapNested is configured on the options, the mapping plan
        /// cache produces a non-null mapping plan for the specified source and destination types.</remarks>
        [Fact]
        public void GetOrAdd_ThreeParamOverload_PopulatesNestedMappingsViaToOptions()
        {
            MapPlanCache.Clear();

            var options = new HydrixMapperOptions();
            options.MapNested<Source, Dest>();

            var plan = MapPlanCache.GetOrAdd(
                typeof(Source),
                typeof(Dest),
                options);

            Assert.NotNull(
                plan);
        }
    }
}
