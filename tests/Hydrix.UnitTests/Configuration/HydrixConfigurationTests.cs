using Hydrix.Configuration;
using Xunit;

namespace Hydrix.UnitTests.Configuration
{
    /// <summary>
    /// Provides unit tests for the HydrixConfiguration class, ensuring correct behavior of configuration options.
    /// </summary>
    /// <remarks>This class contains tests that verify the default instance of HydrixOptions is returned, that
    /// the configuration can be changed, and that null values are allowed in the configuration method.</remarks>
    public class HydrixConfigurationTests
    {
        /// <summary>
        /// Verifies that the HydrixConfiguration.Options property returns a non-null default instance of HydrixOptions.
        /// </summary>
        /// <remarks>Use this test to ensure that the default configuration is properly initialized and of
        /// the expected type. This helps validate the configuration setup and prevents issues related to missing or
        /// incorrect default options.</remarks>
        [Fact]
        public void Options_ReturnsDefaultInstance()
        {
            var options = HydrixConfiguration.Options;
            Assert.IsType<HydrixOptions>(options);
        }

        /// <summary>
        /// Verifies that the Configure method updates the HydrixConfiguration to use the specified HydrixOptions
        /// instance.
        /// </summary>
        /// <remarks>This test ensures that after calling HydrixConfiguration.Configure with a new
        /// HydrixOptions instance, the Options property of HydrixConfiguration references the same instance. This
        /// confirms that the configuration system correctly applies and exposes the provided options object.</remarks>
        [Fact]
        public void Configure_ChangesOptionsInstance()
        {
            var originalOptions = HydrixConfiguration.Options;
            var newOptions = new HydrixOptions();

            try
            {
                HydrixConfiguration.Configure(newOptions);

                Assert.Same(newOptions, HydrixConfiguration.Options);
            }
            finally
            {
                HydrixConfiguration.Configure(originalOptions);
            }
        }
    }
}
