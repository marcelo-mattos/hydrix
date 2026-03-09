using Hydrix.Configuration;
using Hydrix.Orchestrator.Materializers;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for the constructors of the Materializer class.
    /// </summary>
    /// <remarks>These tests verify that the Materializer constructors correctly assign connection and
    /// property values, apply default values when appropriate, and handle null connections as expected.</remarks>
    public partial class MaterializerTests
    {
        /// <summary>
        /// Verifies that the Materializer constructor correctly assigns the provided connection, logger, timeout, and
        /// parameter prefix to their respective properties.
        /// </summary>
        [Fact]
        public void Constructor_Assigns_Logger_And_Properties()
        {
            var connection = new DummyDbConnection();
            var logger = new Mock<ILogger>().Object;
            int timeout = 77;
            string prefix = "@@";
            var materializer = new Materializer(connection, logger, timeout, prefix);

            // DbConnection is public or internal property
            var dbConnProp = typeof(Materializer).GetProperty("DbConnection", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var dbConn = dbConnProp.GetValue(materializer);
            Assert.Same(connection, dbConn);

            var loggerField = typeof(Materializer).GetField("_logger", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.Same(logger, loggerField.GetValue(materializer));

            var timeoutProp = typeof(Materializer).GetProperty("Timeout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.Equal(timeout, (int)timeoutProp.GetValue(materializer));

            Assert.Equal(prefix, GetPrivateParameterPrefix(materializer));
        }

        /// <summary>
        /// Verifies that the Materializer constructor correctly assigns the provided connection, timeout, and
        /// parameter prefix to their respective properties.
        /// </summary>
        /// <remarks>This test ensures that the constructor initializes all relevant properties with the
        /// values supplied during instantiation. It checks both public and non-public members as needed to confirm
        /// correct assignment.</remarks>
        [Fact]
        public void Constructor_Assigns_Connection_And_Properties()
        {
            var connection = new DummyDbConnection();
            int timeout = 77;
            string prefix = "@@";
            var materializer = new Materializer(connection, timeout, prefix);

            // DbConnection is public or internal property
            var dbConnProp = typeof(Materializer).GetProperty("DbConnection", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var dbConn = dbConnProp.GetValue(materializer);
            Assert.Same(connection, dbConn);

            var timeoutProp = typeof(Materializer).GetProperty("Timeout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.Equal(timeout, (int)timeoutProp.GetValue(materializer));

            Assert.Equal(prefix, GetPrivateParameterPrefix(materializer));
        }

        /// <summary>
        /// Verifies that the Materializer constructor initializes the Timeout and parameter prefix properties to
        /// their default values.
        /// </summary>
        /// <remarks>This test ensures that when a new instance of Materializer is created without
        /// explicitly specifying a timeout or parameter prefix, the instance uses the class's predefined default values
        /// for these settings.</remarks>
        [Fact]
        public void Constructor_Uses_Default_Timeout_And_Prefix()
        {
            var connection = new DummyDbConnection();
            var defaultTimeoutField = typeof(HydrixOptions).GetField("DefaultTimeout", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var defaultPrefixField = typeof(HydrixOptions).GetField("DefaultParameterPrefix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            int defaultTimeout = (int)defaultTimeoutField.GetValue(null);
            string defaultPrefix = (string)defaultPrefixField.GetValue(null);

            var materializer = new Materializer(connection);

            var timeoutProp = typeof(Materializer).GetProperty("Timeout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.Equal(defaultTimeout, (int)timeoutProp.GetValue(materializer));
            Assert.Equal(defaultPrefix, GetPrivateParameterPrefix(materializer));
        }

        /// <summary>
        /// Verifies that the Materializer constructor allows a null DbConnection parameter without throwing an
        /// exception.
        /// </summary>
        /// <remarks>This test ensures that passing null to the Materializer constructor results in a
        /// materializer instance with a null DbConnection property.</remarks>
        [Fact]
        public void Constructor_Allows_Null_Connection()
        {
            var materializer = new Materializer(null);

            var dbConnProp = typeof(Materializer).GetProperty("DbConnection", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.Null(dbConnProp.GetValue(materializer));
        }
    }
}