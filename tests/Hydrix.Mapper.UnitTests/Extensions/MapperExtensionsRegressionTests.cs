using Hydrix.Mapper.Caching;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Extensions;
using Hydrix.Mapper.Primitives;
using System;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Extensions
{
    /// <summary>
    /// Covers regression scenarios for the process-wide mapper configuration used by the extension methods.
    /// </summary>
    [Collection(GlobalStateTestCollection.Name)]
    public class MapperExtensionsRegressionTests : IDisposable
    {
        /// <summary>
        /// Represents the source type shared by the global reconfiguration scenario.
        /// </summary>
        private sealed class EntityModel
        {
            /// <summary>
            /// Gets or sets the source text copied into the destination model.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents the destination type shared by the global reconfiguration scenario.
        /// </summary>
        private sealed class DestinationModel
        {
            /// <summary>
            /// Gets or sets the mapped text.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Restores the process-wide mapper configuration to the default state before each test instance performs
        /// assertions.
        /// </summary>
        public MapperExtensionsRegressionTests()
        {
            MapPlanCache.Clear();

            HydrixMapperConfiguration.Configure(
                new HydrixMapperOptions());
        }

        /// <summary>
        /// Restores the process-wide mapper configuration after each test completes.
        /// </summary>
        public void Dispose()
        {
            HydrixMapperConfiguration.Configure(
                new HydrixMapperOptions());

            MapPlanCache.Clear();
        }

        /// <summary>
        /// Verifies that reconfiguring the global mapper updates subsequent default mappings for the same type pair.
        /// </summary>
        [Fact]
        public void ToDto_UsesUpdatedGlobalConfiguration_AfterReconfigure()
        {
            var uppercase = new HydrixMapperOptions();
            uppercase.String.Transform = StringTransforms.Uppercase;
            HydrixMapperConfiguration.Configure(
                uppercase);

            var first = new EntityModel
            {
                Name = "Hello",
            }.ToDto<DestinationModel>();

            var lowercase = new HydrixMapperOptions();
            lowercase.String.Transform = StringTransforms.Lowercase;
            HydrixMapperConfiguration.Configure(
                lowercase);

            var second = new EntityModel
            {
                Name = "Hello",
            }.ToDto<DestinationModel>();

            Assert.Equal(
                "HELLO",
                first.Name);
            Assert.Equal(
                "hello",
                second.Name);
        }

        /// <summary>
        /// Verifies that replacing the process-wide default mapper does not clear compiled plans that belong to other
        /// mapper instances with different option snapshots.
        /// </summary>
        [Fact]
        public void Configure_DoesNotClearGlobalPlanCache_ForNonDefaultMapperPlans()
        {
            var uppercase = new HydrixMapperOptions();
            uppercase.String.Transform = StringTransforms.Uppercase;

            var mapper = new HydrixMapper(
                uppercase);

            var first = mapper.Map<EntityModel, DestinationModel>(
                new EntityModel
                {
                    Name = "Hello",
                });

            Assert.Equal(
                "HELLO",
                first.Name);
            Assert.True(
                MapPlanCache.IsCached(
                    typeof(EntityModel),
                    typeof(DestinationModel),
                    uppercase));
            Assert.Equal(
                1,
                MapPlanCache.PlanCompilationCount);

            var lowercase = new HydrixMapperOptions();
            lowercase.String.Transform = StringTransforms.Lowercase;
            HydrixMapperConfiguration.Configure(
                lowercase);

            Assert.True(
                MapPlanCache.IsCached(
                    typeof(EntityModel),
                    typeof(DestinationModel),
                    uppercase));
            Assert.Equal(
                1,
                MapPlanCache.PlanCompilationCount);

            var second = new EntityModel
            {
                Name = "Hello",
            }.ToDto<DestinationModel>();

            Assert.Equal(
                "hello",
                second.Name);
            Assert.True(
                MapPlanCache.IsCached(
                    typeof(EntityModel),
                    typeof(DestinationModel),
                    lowercase));
            Assert.Equal(
                2,
                MapPlanCache.PlanCompilationCount);
        }

        /// <summary>
        /// Verifies that replacing the global mapper configuration rejects a <see langword="null"/> options instance.
        /// </summary>
        [Fact]
        public void Configure_ThrowsArgumentNullException_WhenOptionsIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => HydrixMapperConfiguration.Configure(
                    null));

            Assert.Equal(
                "options",
                exception.ParamName);
        }
    }
}
