using Hydrix.Mapper.Caching;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Primitives;
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
            StringTransforms transform)
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
                        StringTransforms.Uppercase)));
            var lowerKey = new MapPlanKey(
                typeof(SourceModel),
                typeof(DestinationModel),
                MapPlanOptionsKey.Create(
                    CreateOptions(
                        StringTransforms.Lowercase)));

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
                    StringTransforms.Trim | StringTransforms.Uppercase));
            object right = MapPlanOptionsKey.Create(
                CreateOptions(
                    StringTransforms.Trim | StringTransforms.Uppercase));

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
                    StringTransforms.Trim));

            Assert.False(
                key.Equals(
                    new object()));
        }

        /// <summary>
        /// Verifies that snapshots with identical string transforms and guid formats but different guid cases compare as
        /// not equal, exercising the <c>_guidCase</c> branch in the equality chain.
        /// </summary>
        [Fact]
        public void Equals_ReturnsFalse_WhenGuidCaseDiffers()
        {
            var upperOptions = new HydrixMapperOptions();
            upperOptions.Guid.Case = GuidCase.Upper;

            var lowerOptions = new HydrixMapperOptions();
            lowerOptions.Guid.Case = GuidCase.Lower;

            var upperKey = MapPlanOptionsKey.Create(upperOptions);
            var lowerKey = MapPlanOptionsKey.Create(lowerOptions);

            Assert.False(upperKey.Equals(lowerKey));
            Assert.NotEqual(upperKey.GetHashCode(), lowerKey.GetHashCode());
        }

        /// <summary>
        /// Verifies that snapshots with all scalar options identical but different nested-mapping registrations compare
        /// as not equal, exercising the nested-mappings reference branch in the equality chain.
        /// </summary>
        [Fact]
        public void Equals_ReturnsFalse_WhenNestedMappingsDiffer()
        {
            var options1 = new HydrixMapperOptions();
            options1.MapNested<ArgumentException, InvalidOperationException>();

            var options2 = new HydrixMapperOptions();
            options2.MapNested<ArgumentNullException, InvalidOperationException>();

            var key1 = MapPlanOptionsKey.Create(options1);
            var key2 = MapPlanOptionsKey.Create(options2);

            Assert.False(key1.Equals(key2));
        }
    }
}
