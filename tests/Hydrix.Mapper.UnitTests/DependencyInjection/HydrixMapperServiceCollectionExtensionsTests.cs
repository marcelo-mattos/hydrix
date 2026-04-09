using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.DependencyInjection;
using Hydrix.Mapper.Extensions;
using Hydrix.Mapper.Primitives;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Hydrix.Mapper.UnitTests.DependencyInjection
{
    /// <summary>
    /// Validates the dependency-injection helpers that register isolated Hydrix mapper services.
    /// </summary>
    [Collection(GlobalStateTestCollection.Name)]
    public class HydrixMapperServiceCollectionExtensionsTests : IDisposable
    {
        /// <summary>
        /// Represents the source type used by the end-to-end registration test.
        /// </summary>
        private sealed class SourceModel
        {
            /// <summary>
            /// Gets or sets the numeric identifier copied into the destination object.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the text copied into the destination object.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents the destination type used by the end-to-end registration test.
        /// </summary>
        private sealed class DestinationModel
        {
            /// <summary>
            /// Gets or sets the mapped numeric identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the mapped text.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Restores the default global mapper configuration after each test case.
        /// </summary>
        public void Dispose()
        {
            HydrixMapperConfiguration.Configure(
                new HydrixMapperOptions());
        }

        /// <summary>
        /// Verifies that the DI extension registers <see cref="IHydrixMapper"/> as a singleton service.
        /// </summary>
        [Fact]
        public void AddHydrixMapper_RegistersIHydrixMapper_AsSingleton()
        {
            var services = new ServiceCollection();
            services.AddHydrixMapper();
            var provider = services.BuildServiceProvider();

            var firstMapper = provider.GetRequiredService<IHydrixMapper>();
            var secondMapper = provider.GetRequiredService<IHydrixMapper>();

            Assert.NotNull(
                firstMapper);
            Assert.Same(
                firstMapper,
                secondMapper);
        }

        /// <summary>
        /// Verifies that the configuration callback applies to both the DI mapper and the global extension-method
        /// mapper.
        /// </summary>
        [Fact]
        public void AddHydrixMapper_WithConfigure_AppliesOptions()
        {
            var services = new ServiceCollection();
            services.AddHydrixMapper(
                options =>
                {
                    options.String.Transform = StringTransforms.Uppercase;
                });
            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<IHydrixMapper>();
            var source = new SourceModel
            {
                Id = 1,
                Name = "hello",
            };
            var destination = mapper.Map<DestinationModel>(
                source);

            Assert.Equal(
                "HELLO",
                destination.Name);
            Assert.Equal(
                StringTransforms.Uppercase,
                HydrixMapperConfiguration.Options.String.Transform);
        }

        /// <summary>
        /// Verifies that later registrations overwrite the previously registered mapper inside the same service
        /// collection and publish the last configuration to the global extension-method mapper.
        /// </summary>
        [Fact]
        public void AddHydrixMapper_CalledMultipleTimes_LastRegistrationWins()
        {
            var services = new ServiceCollection();
            services.AddHydrixMapper(
                options => options.String.Transform = StringTransforms.Lowercase);
            services.AddHydrixMapper(
                options => options.String.Transform = StringTransforms.Uppercase);
            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<IHydrixMapper>();
            var destination = mapper.Map<DestinationModel>(
                new SourceModel
                {
                    Name = "Hello",
                });

            Assert.Equal(
                "HELLO",
                destination.Name);
            Assert.Equal(
                StringTransforms.Uppercase,
                HydrixMapperConfiguration.Options.String.Transform);
        }

        /// <summary>
        /// Verifies that two providers built from the same service collection receive isolated singleton mapper
        /// instances.
        /// </summary>
        [Fact]
        public void AddHydrixMapper_BuildingMultipleProviders_CreatesIsolatedSingletons()
        {
            var services = new ServiceCollection();
            services.AddHydrixMapper();

            using var firstProvider = services.BuildServiceProvider();
            using var secondProvider = services.BuildServiceProvider();

            var firstMapper = firstProvider.GetRequiredService<IHydrixMapper>();
            var secondMapper = secondProvider.GetRequiredService<IHydrixMapper>();

            Assert.NotSame(
                firstMapper,
                secondMapper);
        }

        /// <summary>
        /// Verifies that independent service providers configured differently do not interfere with each other.
        /// The global extension-method mapper reflects the most recent <c>AddHydrixMapper</c> call.
        /// </summary>
        [Fact]
        public void AddHydrixMapper_MultipleProvidersWithDifferentOptions_DoNotInterfere()
        {
            var uppercaseServices = new ServiceCollection();
            uppercaseServices.AddHydrixMapper(
                options => options.String.Transform = StringTransforms.Uppercase);

            var lowercaseServices = new ServiceCollection();
            lowercaseServices.AddHydrixMapper(
                options => options.String.Transform = StringTransforms.Lowercase);

            using var uppercaseProvider = uppercaseServices.BuildServiceProvider();
            using var lowercaseProvider = lowercaseServices.BuildServiceProvider();

            var uppercaseMapper = uppercaseProvider.GetRequiredService<IHydrixMapper>();
            var lowercaseMapper = lowercaseProvider.GetRequiredService<IHydrixMapper>();
            var source = new SourceModel
            {
                Name = "Hello",
            };

            var upperDestination = uppercaseMapper.Map<DestinationModel>(
                source);
            var lowerDestination = lowercaseMapper.Map<DestinationModel>(
                source);

            Assert.Equal(
                "HELLO",
                upperDestination.Name);
            Assert.Equal(
                "hello",
                lowerDestination.Name);
            Assert.Equal(
                StringTransforms.Lowercase,
                HydrixMapperConfiguration.Options.String.Transform);
        }

        /// <summary>
        /// Verifies that DI registration also publishes the configured options to the process-wide mapper used by the
        /// convenience extension methods.
        /// </summary>
        [Fact]
        public void AddHydrixMapper_AlsoConfiguresGlobalExtensionMapper()
        {
            var services = new ServiceCollection();
            services.AddHydrixMapper(
                options => options.String.Transform = StringTransforms.Uppercase);
            services.BuildServiceProvider();

            var destination = new SourceModel
            {
                Name = "Hello",
            }.ToDto<DestinationModel>();

            Assert.Equal(
                "HELLO",
                destination.Name);
        }

        /// <summary>
        /// Verifies that passing a <see langword="null"/> service collection produces the expected guard-clause exception.
        /// </summary>
        [Fact]
        public void AddHydrixMapper_ThrowsArgumentNullException_WhenServicesIsNull()
        {
            IServiceCollection services = null;

            Assert.Throws<ArgumentNullException>(
                () => services.AddHydrixMapper());
        }

        /// <summary>
        /// Verifies that the resolved mapper can immediately execute a simple object-to-object mapping.
        /// </summary>
        [Fact]
        public void AddHydrixMapper_ResolvedMapper_CanMap()
        {
            var services = new ServiceCollection();
            services.AddHydrixMapper();
            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<IHydrixMapper>();

            var source = new SourceModel
            {
                Id = 42,
                Name = "Test",
            };

            var destination = mapper.Map<DestinationModel>(
                source);

            Assert.Equal(
                42,
                destination.Id);
            Assert.Equal(
                "Test",
                destination.Name);
        }
    }
}
