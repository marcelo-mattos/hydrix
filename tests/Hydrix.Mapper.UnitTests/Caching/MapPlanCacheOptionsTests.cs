using Hydrix.Mapper.Caching;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Primitives;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Caching
{
    /// <summary>
    /// Validates that the plan cache segments entries by the effective mapper configuration rather than only by the type
    /// pair.
    /// </summary>
    [Collection(CacheStateTestCollection.Name)]
    public class MapPlanCacheOptionsTests
    {
        /// <summary>
        /// Represents the source type used by the cache option-segmentation scenarios.
        /// </summary>
        private sealed class SourceModel
        {
            /// <summary>
            /// Gets or sets the source text copied into the destination model.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents the destination type used by the cache option-segmentation scenarios.
        /// </summary>
        private sealed class DestinationModel
        {
            /// <summary>
            /// Gets or sets the mapped text.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Creates a mapper-option snapshot with the supplied string transform.
        /// </summary>
        /// <param name="transform">
        /// The string transform to apply to compatible string mappings.
        /// </param>
        /// <returns>A configured option snapshot.</returns>
        private static HydrixMapperOptions CreateOptions(
            StringTransforms transform)
        {
            var options = new HydrixMapperOptions();
            options.String.Transform = transform;
            return options;
        }

        /// <summary>
        /// Verifies that the cache stores separate plan instances when the mapper options differ for the same type pair.
        /// </summary>
        [Fact]
        public void GetOrAdd_ReturnsDifferentPlans_WhenOptionsDiffer()
        {
            MapPlanCache.Clear();
            var uppercase = CreateOptions(
                StringTransforms.Uppercase);
            var lowercase = CreateOptions(
                StringTransforms.Lowercase);

            var uppercasePlan = MapPlanCache.GetOrAdd(
                typeof(SourceModel),
                typeof(DestinationModel),
                uppercase);
            var lowercasePlan = MapPlanCache.GetOrAdd(
                typeof(SourceModel),
                typeof(DestinationModel),
                lowercase);

            Assert.NotSame(
                uppercasePlan,
                lowercasePlan);
            Assert.True(
                MapPlanCache.IsCached(
                    typeof(SourceModel),
                    typeof(DestinationModel),
                    uppercase));
            Assert.True(
                MapPlanCache.IsCached(
                    typeof(SourceModel),
                    typeof(DestinationModel),
                    lowercase));
        }

        /// <summary>
        /// Verifies that equivalent option values reuse the same cached plan even when they come from distinct option
        /// instances.
        /// </summary>
        [Fact]
        public void GetOrAdd_ReturnsSamePlan_WhenOptionsAreEquivalentByValue()
        {
            MapPlanCache.Clear();
            var firstOptions = CreateOptions(
                StringTransforms.Trim | StringTransforms.Uppercase);
            var secondOptions = CreateOptions(
                StringTransforms.Trim | StringTransforms.Uppercase);

            var firstPlan = MapPlanCache.GetOrAdd(
                typeof(SourceModel),
                typeof(DestinationModel),
                firstOptions);
            var secondPlan = MapPlanCache.GetOrAdd(
                typeof(SourceModel),
                typeof(DestinationModel),
                secondOptions);

            Assert.Same(
                firstPlan,
                secondPlan);
        }
    }
}
