using Hydrix.Mapper;
using Hydrix.Mapper.Caching;
using Hydrix.Mapper.Configuration;
using System;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Caching
{
    /// <summary>
    /// Exercises the cache-key equality paths that depend on the captured mapper-option snapshot.
    /// </summary>
    public class MapPlanKeyOptionsTests
    {
        /// <summary>
        /// Represents the source type shared by the key-equality scenarios.
        /// </summary>
        private sealed class SourceModel
        {
        }

        /// <summary>
        /// Represents the destination type shared by the key-equality scenarios.
        /// </summary>
        private sealed class DestinationModel
        {
        }

        /// <summary>
        /// Creates a mapper-option snapshot with the supplied string transform.
        /// </summary>
        /// <param name="transform">
        /// The string transform to apply to compatible string mappings.
        /// </param>
        /// <returns>A configured option snapshot.</returns>
        private static HydrixMapperOptions CreateOptions(
            StringTransform transform)
        {
            var options = new HydrixMapperOptions();
            options.String.Transform = transform;
            return options;
        }

        /// <summary>
        /// Verifies that otherwise identical keys compare as different when their option snapshots differ.
        /// </summary>
        [Fact]
        public void Equals_ReturnsFalse_WhenOptionsDiffer()
        {
            var upperKey = new MapPlanKey(
                typeof(SourceModel),
                typeof(DestinationModel),
                MapPlanOptionsKey.Create(
                    CreateOptions(
                        StringTransform.Uppercase)));
            var lowerKey = new MapPlanKey(
                typeof(SourceModel),
                typeof(DestinationModel),
                MapPlanOptionsKey.Create(
                    CreateOptions(
                        StringTransform.Lowercase)));

            Assert.False(
                upperKey.Equals(
                    lowerKey));
            Assert.NotEqual(
                upperKey.GetHashCode(),
                lowerKey.GetHashCode());
        }

        /// <summary>
        /// Verifies that the option snapshot compares equal through the object-based overload when the captured values
        /// match.
        /// </summary>
        [Fact]
        public void OptionsEqualsObject_ReturnsTrue_WhenSnapshotsMatch()
        {
            var left = MapPlanOptionsKey.Create(
                CreateOptions(
                    StringTransform.Trim | StringTransform.Uppercase));
            object right = MapPlanOptionsKey.Create(
                CreateOptions(
                    StringTransform.Trim | StringTransform.Uppercase));

            Assert.True(
                left.Equals(
                    right));
        }

        /// <summary>
        /// Verifies that the option snapshot rejects object-based comparisons against unrelated values.
        /// </summary>
        [Fact]
        public void OptionsEqualsObject_ReturnsFalse_ForDifferentObjectType()
        {
            var key = MapPlanOptionsKey.Create(
                CreateOptions(
                    StringTransform.Trim));

            Assert.False(
                key.Equals(
                    new object()));
        }
    }
}

