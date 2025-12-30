using Hydrix.Attributes.Commands;
using Hydrix.Attributes.Commands.Contract;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Attributes.Commands.Contract
{
    /// <summary>
    /// Specifies a SQL command and its type for use in test scenarios. Intended for use as an attribute to associate a
    /// SQL command with a test method or class.
    /// </summary>
    internal class TestSqlCommandAttribute : 
        SqlCommandAttribute
    {
        /// <summary>
        /// Initializes a new instance of the TestSqlCommandAttribute class with the specified command type and command
        /// text.
        /// </summary>
        /// <param name="commandType">The type of SQL command to execute, such as Text, StoredProcedure, or TableDirect.</param>
        /// <param name="commandText">The SQL command text or stored procedure name to be executed. Cannot be null.</param>
        public TestSqlCommandAttribute(
            CommandType commandType, 
            string commandText) :
            base(
                commandType, 
                commandText)
        { }
    }

    /// <summary>
    /// Contains unit tests for implementations of the ISqlCommandAttribute interface.
    /// </summary>
    /// <remarks>These tests verify that ISqlCommandAttribute implementations correctly expose their
    /// CommandType and CommandText properties, and that they implement the required interface. The tests are intended
    /// to ensure contract compliance for custom attribute types used to describe SQL commands in data access
    /// scenarios.</remarks>
    public class ISqlCommandAttributeTests
    {
        /// <summary>
        /// Verifies that the CommandType property returns the expected value when initialized with a specific command
        /// type.
        /// </summary>
        [Fact]
        public void CommandType_ReturnsExpectedValue()
        {
            var expected = CommandType.StoredProcedure;
            var attribute = new TestSqlCommandAttribute(expected, "sp_TestProc");
            Assert.Equal(expected, attribute.CommandType);
        }

        /// <summary>
        /// Verifies that the CommandText property of TestSqlCommandAttribute returns the expected SQL command text.
        /// </summary>
        /// <remarks>This test ensures that the CommandText property correctly reflects the value provided
        /// during initialization. It is intended to validate the behavior of the TestSqlCommandAttribute class when
        /// constructed with a specific command text.</remarks>
        [Fact]
        public void CommandText_ReturnsExpectedValue()
        {
            var expected = "SELECT * FROM Users";
            var attribute = new TestSqlCommandAttribute(CommandType.Text, expected);
            Assert.Equal(expected, attribute.CommandText);
        }
        
        /// <summary>
        /// Verifies that the TestSqlCommandAttribute class implements the ISqlCommandAttribute interface.
        /// </summary>
        /// <remarks>This test ensures that TestSqlCommandAttribute can be used wherever
        /// ISqlCommandAttribute is expected. Use this test to validate interface compliance after making changes to the
        /// attribute implementation.</remarks>
        [Fact]
        public void Implements_ISqlCommandAttribute_Interface()
        {
            var attribute = new TestSqlCommandAttribute(CommandType.Text, "SELECT 1");
            Assert.IsAssignableFrom<ISqlCommandAttribute>(attribute);
        }

        /// <summary>
        /// Verifies that the CommandType and CommandText properties of TestSqlCommandAttribute are set to the expected
        /// values after construction.
        /// </summary>
        /// <param name="commandType">The type of command to be tested. Specifies how the command string is interpreted (e.g., as text, stored
        /// procedure, or table direct).</param>
        /// <param name="commandText">The command text to be assigned. Represents the SQL statement, stored procedure name, or table name,
        /// depending on the command type.</param>
        [Theory]
        [InlineData(CommandType.Text, "SELECT * FROM Table")]
        [InlineData(CommandType.StoredProcedure, "sp_MyProc")]
        [InlineData(CommandType.TableDirect, "MyTable")]
        public void Properties_AreSetCorrectly(CommandType commandType, string commandText)
        {
            var attribute = new TestSqlCommandAttribute(commandType, commandText);
            Assert.Equal(commandType, attribute.CommandType);
            Assert.Equal(commandText, attribute.CommandText);
        }
    }
}