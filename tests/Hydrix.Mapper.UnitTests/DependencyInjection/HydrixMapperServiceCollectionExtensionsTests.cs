using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.DependencyInjection;
using Hydrix.Mapper.Primitives;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Hydrix.Mapper.UnitTests.DependencyInjection
{
    /// <summary>
    /// Validates the dependency-injection helpers that register Hydrix mapper services and synchronize global defaults.
    /// </summary>
    /// <remarks>
    /// These tests run in the <see cref="GlobalStateTestCollection"/> because the service-registration extension also updates
    /// the process-wide mapper configuration singleton.
    /// </remarks>
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
        /// Verifies that the configuration callback updates the globally exposed Hydrix mapper options.
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

            Assert.Equal(
                StringTransforms.Uppercase,
                HydrixMapperConfiguration.Options.String.Transform);
        }

        /// <summary>
        /// Verifies that later registrations overwrite the previously published global configuration.
        /// </summary>
        [Fact]
        public void AddHydrixMapper_CalledMultipleTimes_LastRegistrationWins()
        {
            var services = new ServiceCollection();
            services.AddHydrixMapper(
                options => options.String.Transform = StringTransforms.Lowercase);
            services.AddHydrixMapper(
                options => options.String.Transform = StringTransforms.Uppercase);

            Assert.Equal(
                StringTransforms.Uppercase,
                HydrixMapperConfiguration.Options.String.Transform);
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
