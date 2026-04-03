using Hydrix.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Hydrix.UnitTests.Configuration
{
    /// <summary>
    /// Provides unit tests for the HydrixOptions class, verifying its default values, property setters and getters, and
    /// constant values.
    /// </summary>
    /// <remarks>These tests ensure that the HydrixOptions class behaves as expected, including correct
    /// initialization of default values and the ability to set and retrieve property values. The tests also validate
    /// that the defined constants remain consistent.</remarks>
    public class HydrixOptionsTests
    {
        /// <summary>
        /// Verifies that the default property values of the HydrixOptions class are set as expected upon
        /// initialization.
        /// </summary>
        /// <remarks>This test ensures that the CommandTimeout and ParameterPrefix properties are
        /// initialized to their documented default values, and that the Logger property is null by default. Use this
        /// test to confirm that changes to HydrixOptions do not inadvertently alter its default
        /// configuration.</remarks>
        [Fact]
        public void Defaults_AreCorrect()
        {
            var options = new HydrixOptions();
            Assert.Equal(HydrixOptions.DefaultTimeout, options.CommandTimeout);
            Assert.Equal(HydrixOptions.DefaultParameterPrefix, options.ParameterPrefix);
            Assert.Null(options.Logger);
        }

        /// <summary>
        /// Verifies that the properties of the HydrixOptions class can be set and retrieved as expected.
        /// </summary>
        /// <remarks>This test ensures that the CommandTimeout, ParameterPrefix, and Logger properties of
        /// HydrixOptions retain their assigned values. It confirms correct property behavior for typical usage
        /// scenarios.</remarks>
        [Fact]
        public void Can_Set_And_Get_Properties()
        {
            var options = new HydrixOptions
            {
                CommandTimeout = 99,
                ParameterPrefix = "#",
                Logger = new Mock<ILogger>().Object
            };

            Assert.Equal(99, options.CommandTimeout);
            Assert.Equal("#", options.ParameterPrefix);
            Assert.NotNull(options.Logger);
        }

        /// <summary>
        /// Verifies that the default values of the HydrixOptions class constants are as expected.
        /// </summary>
        /// <remarks>This test ensures that the DefaultTimeout and DefaultParameterPrefix constants in
        /// HydrixOptions remain unchanged, which is critical for consistent configuration behavior across the
        /// application.</remarks>
        [Fact]
        public void Constants_AreExpected()
        {
            Assert.Equal(30, HydrixOptions.DefaultTimeout);
            Assert.Equal("@", HydrixOptions.DefaultParameterPrefix);
        }
    }
}
