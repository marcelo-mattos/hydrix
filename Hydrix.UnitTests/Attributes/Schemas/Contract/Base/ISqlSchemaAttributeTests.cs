using Hydrix.Attributes.Schemas.Contract.Base;
using Xunit;

namespace Hydrix.UnitTests.Attributes.Schemas.Contract.Base
{
    /// <summary>
    /// Contains unit tests for verifying the behavior of types that implement the ISqlSchemaAttribute interface.
    /// </summary>
    /// <remarks>These tests ensure that the Schema and Name properties of ISqlSchemaAttribute implementations
    /// return the expected values. The class is intended for use with automated test frameworks such as
    /// xUnit.</remarks>
    public class SqlSchemaAttributeTests
    {
        /// <summary>
        /// Represents a SQL schema attribute with a specified schema and object name.
        /// </summary>
        private class TestSqlSchemaAttribute : ISqlSchemaAttribute
        {
            /// <summary>
            /// Initializes a new instance of the TestSqlSchemaAttribute class with the specified schema and name.
            /// </summary>
            /// <param name="schema">The database schema to associate with the attribute. Cannot be null or empty.</param>
            /// <param name="name">The name of the database object to associate with the attribute. Cannot be null or empty.</param>
            public TestSqlSchemaAttribute(string schema, string name)
            {
                Schema = schema;
                Name = name;
            }

            /// <summary>
            /// Gets the database schema associated with the current object.
            /// </summary>
            public string Schema { get; }

            /// <summary>
            /// Gets the name associated with the current instance.
            /// </summary>
            public string Name { get; }
        }

        /// <summary>
        /// Verifies that the Schema property of the TestSqlSchemaAttribute returns the expected value.
        /// </summary>
        [Fact]
        public void SchemaProperty_ReturnsExpectedValue()
        {
            var expectedSchema = "dbo";
            var attribute = new TestSqlSchemaAttribute(expectedSchema, "TestProc");

            Assert.Equal(expectedSchema, attribute.Schema);
        }

        /// <summary>
        /// Verifies that the Name property of the TestSqlSchemaAttribute returns the expected value.
        /// </summary>
        /// <remarks>This test ensures that the Name property is correctly set by the constructor and
        /// returns the value provided during initialization.</remarks>
        [Fact]
        public void NameProperty_ReturnsExpectedValue()
        {
            var expectedName = "TestProc";
            var attribute = new TestSqlSchemaAttribute("dbo", expectedName);

            Assert.Equal(expectedName, attribute.Name);
        }
    }
}