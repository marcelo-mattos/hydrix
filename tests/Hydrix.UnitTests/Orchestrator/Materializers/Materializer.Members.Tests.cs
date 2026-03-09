using Hydrix.Configuration;
using Hydrix.Orchestrator.Materializers;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests that verify the initialization and internal state of the Materializer class.
    /// </summary>
    /// <remarks>These tests ensure that private fields and static members of Materializer are set to their
    /// expected default values upon instantiation or class load. The tests use reflection to access non-public members
    /// and validate their state, helping to maintain the integrity of the Materializer's internal implementation.
    /// This class is intended for use with a unit testing framework such as xUnit.</remarks>
    public partial class MaterializerTests
    {
        /// <summary>
        /// Verifies that the '_timeout' field of a new Materializer instance is initialized to the default timeout
        /// value.
        /// </summary>
        /// <remarks>This test ensures that the internal timeout configuration is set to its expected
        /// default when a Materializer is created. Changes to the default timeout value or field initialization
        /// logic may require updates to this test.</remarks>
        [Fact]
        public void TimeoutField_InitializedToDefault()
        {
            var materializer = CreateInstance(timeout: 30);
            var timeoutField = typeof(Materializer).GetField("_timeout", BindingFlags.NonPublic | BindingFlags.Instance);
            var defaultTimeoutField = typeof(HydrixOptions).GetField("DefaultTimeout", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(timeoutField);
            Assert.NotNull(defaultTimeoutField);
            var expected = (int)defaultTimeoutField.GetValue(null);
            var actual = (int)timeoutField.GetValue(materializer);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Verifies that the _dbConnection field of the Materializer instance is initialized to null.
        /// </summary>
        /// <remarks>This test ensures that a new instance of Materializer does not have an active
        /// database connection by default. This behavior is important to prevent unintended database operations before
        /// explicit initialization.</remarks>
        [Fact]
        public void DbConnectionField_InitializedToNull()
        {
            var materializer = CreateInstance();
            var field = typeof(Materializer).GetField("_dbConnection", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field);
            Assert.Null(field.GetValue(materializer));
        }

        /// <summary>
        /// Verifies that the _dbTransaction field of a new Materializer instance is initialized to null.
        /// </summary>
        /// <remarks>This test ensures that the internal _dbTransaction field is not set upon
        /// construction, which is important for correct transaction management behavior in Materializer.</remarks>
        [Fact]
        public void DbTransactionField_InitializedToNull()
        {
            var materializer = CreateInstance();
            var field = typeof(Materializer).GetField("_dbTransaction", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field);
            Assert.Null(field.GetValue(materializer));
        }

        /// <summary>
        /// Verifies that the '_lockConnection' field of the Materializer instance is initialized and not null.
        /// </summary>
        /// <remarks>This test ensures that the internal synchronization object used by Materializer
        /// for connection locking is properly created. This helps confirm thread safety mechanisms are in place within
        /// the class.</remarks>
        [Fact]
        public void LockConnectionField_IsObject()
        {
            var materializer = CreateInstance();
            var field = typeof(Materializer).GetField("_lockConnection", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field);
            Assert.NotNull(field.GetValue(materializer));
        }

        /// <summary>
        /// Verifies that the '_lockTransaction' field of the Materializer instance is an object and is not null.
        /// </summary>
        /// <remarks>This test ensures that the internal transaction lock field is properly initialized
        /// when a Materializer instance is created. It is intended to validate the object's construction logic and
        /// is not meant for direct use in application code.</remarks>
        [Fact]
        public void LockTransactionField_IsObject()
        {
            var materializer = CreateInstance();
            var field = typeof(Materializer).GetField("_lockTransaction", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field);
            Assert.NotNull(field.GetValue(materializer));
        }

        /// <summary>
        /// Verifies that the '_parameterPrefix' field of a new Materializer instance is initialized to the default
        /// parameter prefix value.
        /// </summary>
        /// <remarks>This test ensures that the internal parameter prefix used by Materializer matches
        /// the expected default value upon instantiation. This helps maintain consistency in parameter naming
        /// conventions for SQL operations.</remarks>
        [Fact]
        public void ParameterPrefixField_InitializedToDefault()
        {
            var materializer = CreateInstance(parameterPrefix: "@");
            var prefixField = typeof(Materializer).GetField("_parameterPrefix", BindingFlags.NonPublic | BindingFlags.Instance);
            var defaultPrefixField = typeof(HydrixOptions).GetField("DefaultParameterPrefix", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(prefixField);
            Assert.NotNull(defaultPrefixField);
            var expected = (string)defaultPrefixField.GetValue(null);
            var actual = (string)prefixField.GetValue(materializer);
            Assert.Equal(expected, actual);
        }
    }
}