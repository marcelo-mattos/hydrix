using Hydrix.Mapper.Caching;
using System;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Caching
{
    /// <summary>
    /// Exercises the equality branches of <see cref="MapPlanKey"/> that are easiest to target directly.
    /// </summary>
    public class MapPlanKeyCoverageTests
    {
        /// <summary>
        /// Represents the primary source type used by the equality scenarios.
        /// </summary>
        private sealed class SourceA
        {
        }

        /// <summary>
        /// Represents an alternate source type used to force the source-type comparison to fail.
        /// </summary>
        private sealed class SourceB
        {
        }

        /// <summary>
        /// Represents the destination type shared by the equality scenarios.
        /// </summary>
        private sealed class DestinationA
        {
        }

        /// <summary>
        /// Verifies that strongly typed equality returns <see langword="false"/> when the source types differ.
        /// </summary>
        [Fact]
        public void Equals_ReturnsFalse_WhenSourceTypesDiffer()
        {
            var left = new MapPlanKey(
                typeof(SourceA),
                typeof(DestinationA));
            var right = new MapPlanKey(
                typeof(SourceB),
                typeof(DestinationA));

            Assert.False(
                left.Equals(
                    right));
        }

        /// <summary>
        /// Verifies that object-based equality returns <see langword="true"/> for an equivalent key instance.
        /// </summary>
        [Fact]
        public void EqualsObject_ReturnsTrue_WhenKeysMatch()
        {
            var left = new MapPlanKey(
                typeof(SourceA),
                typeof(DestinationA));
            object right = new MapPlanKey(
                typeof(SourceA),
                typeof(DestinationA));

            Assert.True(
                left.Equals(
                    right));
        }
    }
}