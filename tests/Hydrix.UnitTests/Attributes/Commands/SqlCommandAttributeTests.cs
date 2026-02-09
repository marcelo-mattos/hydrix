using Hydrix.Attributes.Commands;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Attributes.Commands
{
    /// <summary>
    /// Specifies a SQL command to be used for testing purposes in data-driven tests.
    /// </summary>
    /// <remarks>Use this attribute to associate a specific SQL command with a test method or class. This is
    /// typically used in scenarios where tests require execution of predefined SQL statements, such as integration
    /// tests against a database. Inherits behavior from CommandAttribute.</remarks>
    internal sealed class TestCommandAttribute :
        CommandAttribute
    {
        /// <summary>
        /// Initializes a new instance of the TestCommandAttribute class with the specified command type and command
        /// text.
        /// </summary>
        /// <param name="commandType">The type of SQL command to execute, such as Text, StoredProcedure, or TableDirect.</param>
        /// <param name="commandText">The text of the SQL command to execute. Cannot be null.</param>
        public TestCommandAttribute(
            CommandType commandType,
            string commandText) :
            base(
                commandType,
                commandText)
        { }
    }

    /// <summary>
    /// Contains unit tests for the CommandAttribute class, verifying its construction and property behavior.
    /// </summary>
    /// <remarks>These tests ensure that the CommandAttribute correctly sets its CommandType and
    /// CommandText properties when instantiated, including scenarios with empty command text.</remarks>
    public class CommandAttributeTests
    {
        /// <summary>
        /// Verifies that the constructor of the TestCommandAttribute class correctly sets the CommandType and
        /// CommandText properties.
        /// </summary>
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var expectedType = CommandType.Text;
            var expectedText = "SELECT * FROM Users";

            // Act
            var attr = new TestCommandAttribute(expectedType, expectedText);

            // Assert
            Assert.Equal(expectedType, attr.CommandType);
            Assert.Equal(expectedText, attr.CommandText);
        }

        /// <summary>
        /// Verifies that the constructor of TestCommandAttribute allows an empty command text without throwing an
        /// exception.
        /// </summary>
        /// <remarks>This test ensures that providing an empty string for the command text parameter is a
        /// valid scenario and that the CommandText property is set accordingly.</remarks>
        [Fact]
        public void Constructor_AllowsEmptyCommandText()
        {
            // Arrange
            var expectedType = CommandType.StoredProcedure;
            var expectedText = string.Empty;

            // Act
            var attr = new TestCommandAttribute(expectedType, expectedText);

            // Assert
            Assert.Equal(expectedType, attr.CommandType);
            Assert.Equal(expectedText, attr.CommandText);
        }
    }
}