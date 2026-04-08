using Hydrix.Mapper.Caching;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Extensions;
using Hydrix.Mapper.Primitives;
using System;
using System.Reflection;
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
        /// Verifies that configuring the process-wide mapper captures an immutable options snapshot for future default
        /// mapper creation.
        /// </summary>
        [Fact]
        public void Configure_CapturesOptionsSnapshot_ForFutureDefaultMapperCreation()
        {
            var uppercase = new HydrixMapperOptions();
            uppercase.String.Transform = StringTransforms.Uppercase;
            HydrixMapperConfiguration.Configure(
                uppercase);

            uppercase.String.Transform = StringTransforms.Lowercase;

            var result = new EntityModel
            {
                Name = "Hello",
            }.ToDto<DestinationModel>();

            Assert.Equal(
                StringTransforms.Uppercase,
                HydrixMapperConfiguration.Options.String.Transform);
            Assert.Equal(
                "HELLO",
                result.Name);
        }

        /// <summary>
        /// Verifies that a mapper created from a previous configuration generation cannot replace the current default
        /// mapper after the global configuration is updated.
        /// </summary>
        [Fact]
        public void Configure_DoesNotAllowOldGenerationMapperToReplaceCurrentDefaultMapper()
        {
            var uppercase = new HydrixMapperOptions();
            uppercase.String.Transform = StringTransforms.Uppercase;
            HydrixMapperConfiguration.Configure(
                uppercase);

            var staleState = GetCurrentConfigurationState();

            var lowercase = new HydrixMapperOptions();
            lowercase.String.Transform = StringTransforms.Lowercase;
            HydrixMapperConfiguration.Configure(
                lowercase);

            var staleMapper = staleState.GetOrCreateDefaultMapper();
            var currentMapper = HydrixMapperConfiguration.GetOrCreateDefaultMapper();
            var source = new EntityModel
            {
                Name = "Hello",
            };

            var staleResult = staleMapper.Map<EntityModel, DestinationModel>(
                source);
            var currentResult = currentMapper.Map<EntityModel, DestinationModel>(
                source);
            var defaultResult = source.ToDto<DestinationModel>();

            Assert.NotSame(
                staleMapper,
                currentMapper);
            Assert.Equal(
                "HELLO",
                staleResult.Name);
            Assert.Equal(
                "hello",
                currentResult.Name);
            Assert.Equal(
                "hello",
                defaultResult.Name);
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

        /// <summary>
        /// Verifies that a configuration generation rejects a <see langword="null"/> options instance.
        /// </summary>
        [Fact]
        public void HydrixMapperConfigurationState_ThrowsArgumentNullException_WhenOptionsIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new HydrixMapperConfigurationState(
                    null));

            Assert.Equal(
                "options",
                exception.ParamName);
        }

        /// <summary>
        /// Verifies that the public global configuration wrapper rejects a <see langword="null"/> options instance.
        /// </summary>
        [Fact]
        public void HydrixMapperGlobalConfiguration_ThrowsArgumentNullException_WhenOptionsIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => HydrixMapperGlobalConfiguration.Configure(
                    (HydrixMapperOptions)null));

            Assert.Equal(
                "options",
                exception.ParamName);
        }

        /// <summary>
        /// Verifies that the callback-based global configuration wrapper also supports a <see langword="null"/> callback,
        /// publishing a default snapshot without throwing.
        /// </summary>
        [Fact]
        public void HydrixMapperGlobalConfiguration_AllowsNullCallback_AndPublishesDefaultSnapshot()
        {
            HydrixMapperGlobalConfiguration.Configure(
                (Action<HydrixMapperOptions>)null);

            var result = new EntityModel
            {
                Name = "Hello",
            }.ToDto<DestinationModel>();

            Assert.Equal(
                StringTransforms.None,
                HydrixMapperConfiguration.Options.String.Transform);
            Assert.Equal(
                "Hello",
                result.Name);
        }

        /// <summary>
        /// Returns the current private configuration generation stored by the global mapper configuration.
        /// </summary>
        /// <returns>The active configuration generation.</returns>
        private static HydrixMapperConfigurationState GetCurrentConfigurationState()
        {
            var field = typeof(HydrixMapperConfiguration).GetField(
                "_state",
                BindingFlags.Static | BindingFlags.NonPublic);

            return (HydrixMapperConfigurationState)field.GetValue(
                null);
        }
    }
}
