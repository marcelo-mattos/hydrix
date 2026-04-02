using Hydrix.Configuration;
using Hydrix.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Hydrix.UnitTests.DependencyInjection
{
    /// <summary>
    /// Contains unit tests for the Hydrix service collection extension methods.
    /// </summary>
    /// <remarks>These tests verify that the Hydrix extension methods for IServiceCollection correctly add
    /// Hydrix services and handle configuration options as expected. The tests ensure that the extension methods return
    /// the original service collection, apply configuration delegates, and support null delegates without
    /// errors.</remarks>
    public class HydrixServiceCollectionExtensionsTests
    {
        /// <summary>
        /// Provides synchronized access to tests that mutate global <see cref="HydrixConfiguration"/> state.
        /// </summary>
        private static readonly object ConfigurationSyncRoot = new object();

        /// <summary>
        /// Verifies that the AddHydrix extension method returns the same IServiceCollection instance it was called on.
        /// </summary>
        /// <remarks>This test ensures that the AddHydrix method supports method chaining by returning the
        /// original service collection, which is a common pattern for configuring dependency injection in .NET
        /// applications.</remarks>
        [Fact]
        public void AddHydrix_ReturnsSameServiceCollection()
        {
            ExecuteWithIsolatedConfiguration(() =>
            {
                var services = new ServiceCollection();
                var result = services.AddHydrix();

                Assert.Same(services, result);
            });
        }

        /// <summary>
        /// Verifies that the AddHydrix method correctly configures Hydrix options using a delegate.
        /// </summary>
        /// <remarks>This test ensures that when a delegate is provided to configure Hydrix options, the
        /// options are set as expected and the configured instance is used by HydrixConfiguration. It validates that
        /// the delegate is invoked and the specified option values are applied.</remarks>
        [Fact]
        public void AddHydrix_ConfiguresOptionsWithDelegate()
        {
            ExecuteWithIsolatedConfiguration(() =>
            {
                var services = new ServiceCollection();
                HydrixOptions captured = null;

                services.AddHydrix(options =>
                {
                    options.CommandTimeout = 99;
                    captured = options;
                });

                Assert.NotNull(captured);
                Assert.Equal(99, captured.CommandTimeout);
                Assert.Same(captured, HydrixConfiguration.Options);

                using var serviceProvider = services.BuildServiceProvider();
                var resolved = serviceProvider.GetService<HydrixOptions>();

                Assert.Same(captured, resolved);
            });
        }

        /// <summary>
        /// Verifies that AddHydrix registers <see cref="HydrixOptions"/> in the dependency injection container.
        /// </summary>
        [Fact]
        public void AddHydrix_RegistersHydrixOptionsInContainer()
        {
            ExecuteWithIsolatedConfiguration(() =>
            {
                var services = new ServiceCollection();

                services.AddHydrix();

                using var serviceProvider = services.BuildServiceProvider();
                var resolved = serviceProvider.GetService<HydrixOptions>();

                Assert.NotNull(resolved);
                Assert.Same(HydrixConfiguration.Options, resolved);
            });
        }

        /// <summary>
        /// Verifies that the AddHydrix extension method supports a null delegate and correctly initializes Hydrix
        /// configuration options.
        /// </summary>
        /// <remarks>This test ensures that passing a null delegate to AddHydrix does not alter the
        /// service collection and that HydrixConfiguration.Options is properly initialized. It validates the method's
        /// ability to handle optional configuration delegates without error.</remarks>
        [Fact]
        public void AddHydrix_AllowsNullDelegate()
        {
            ExecuteWithIsolatedConfiguration(() =>
            {
                var services = new ServiceCollection();
                var result = services.AddHydrix(null);

                Assert.Same(services, result);
                Assert.NotNull(HydrixConfiguration.Options);
            });
        }

        /// <summary>
        /// Executes a test action while isolating and restoring global Hydrix configuration state.
        /// </summary>
        /// <param name="action">The test action that may mutate <see cref="HydrixConfiguration.Options"/>.</param>
        private static void ExecuteWithIsolatedConfiguration(
            System.Action action)
        {
            lock (ConfigurationSyncRoot)
            {
                var originalOptions = HydrixConfiguration.Options;

                try
                {
                    action();
                }
                finally
                {
                    HydrixConfiguration.Configure(originalOptions);
                }
            }
        }
    }
}
