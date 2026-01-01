using Hydrix.Orchestrator.Materializers;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for the constructors of the SqlMaterializer class.
    /// </summary>
    /// <remarks>These tests verify that the SqlMaterializer constructors correctly assign connection and
    /// property values, apply default values when appropriate, and handle null connections as expected.</remarks>
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the SqlMaterializer constructor correctly assigns the provided connection, timeout, and
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
            var materializer = new SqlMaterializer(connection, timeout, prefix);

            // DbConnection is public or internal property
            var dbConnProp = typeof(SqlMaterializer).GetProperty("DbConnection", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var dbConn = dbConnProp.GetValue(materializer);
            Assert.Same(connection, dbConn);

            var timeoutProp = typeof(SqlMaterializer).GetProperty("Timeout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.Equal(timeout, (int)timeoutProp.GetValue(materializer));

            Assert.Equal(prefix, GetPrivateParameterPrefix(materializer));
        }

        /// <summary>
        /// Verifies that the SqlMaterializer constructor initializes the Timeout and parameter prefix properties to
        /// their default values.
        /// </summary>
        /// <remarks>This test ensures that when a new instance of SqlMaterializer is created without
        /// explicitly specifying a timeout or parameter prefix, the instance uses the class's predefined default values
        /// for these settings.</remarks>
        [Fact]
        public void Constructor_Uses_Default_Timeout_And_Prefix()
        {
            var connection = new DummyDbConnection();
            var defaultTimeoutField = typeof(SqlMaterializer).GetField("DefaultTimeout", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var defaultPrefixField = typeof(SqlMaterializer).GetField("DefaultParameterPrefix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            int defaultTimeout = (int)defaultTimeoutField.GetValue(null);
            string defaultPrefix = (string)defaultPrefixField.GetValue(null);

            var materializer = new SqlMaterializer(connection);

            var timeoutProp = typeof(SqlMaterializer).GetProperty("Timeout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.Equal(defaultTimeout, (int)timeoutProp.GetValue(materializer));
            Assert.Equal(defaultPrefix, GetPrivateParameterPrefix(materializer));
        }

        /// <summary>
        /// Verifies that the SqlMaterializer constructor allows a null DbConnection parameter without throwing an
        /// exception.
        /// </summary>
        /// <remarks>This test ensures that passing null to the SqlMaterializer constructor results in a
        /// materializer instance with a null DbConnection property.</remarks>
        [Fact]
        public void Constructor_Allows_Null_Connection()
        {
            var materializer = new SqlMaterializer(null);

            var dbConnProp = typeof(SqlMaterializer).GetProperty("DbConnection", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.Null(dbConnProp.GetValue(materializer));
        }
    }
}