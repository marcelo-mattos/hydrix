using Hydrix.Attributes.Parameters;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Attributes.Parameters
{
    /// <summary>
    /// Contains unit tests for the SqlParameterAttribute class, verifying its constructors and property assignments.
    /// </summary>
    /// <remarks>These tests ensure that SqlParameterAttribute correctly sets its properties for various
    /// constructor inputs, including edge cases such as empty parameter names and undefined enum values. The tests are
    /// intended to validate the public API contract of SqlParameterAttribute.</remarks>
    public class SqlParameterAttributeTests
    {
        /// <summary>
        /// Verifies that the SqlParameterAttribute constructor correctly sets the ParameterName, DbType, and Direction
        /// properties.
        /// </summary>
        [Fact]
        public void Constructor_SetsProperties_Correctly()
        {
            var attr = new SqlParameterAttribute("param1", DbType.Int32);

            Assert.Equal("param1", attr.ParameterName);
            Assert.Equal(DbType.Int32, attr.DbType);
            Assert.Equal(ParameterDirection.Input, attr.Direction);
        }

        /// <summary>
        /// Verifies that the SqlParameterAttribute constructor correctly sets the ParameterName, DbType, and Direction
        /// properties when a direction is specified.
        /// </summary>
        [Fact]
        public void Constructor_WithDirection_SetsProperties_Correctly()
        {
            var attr = new SqlParameterAttribute("param2", DbType.DateTime, ParameterDirection.Output);

            Assert.Equal("param2", attr.ParameterName);
            Assert.Equal(DbType.DateTime, attr.DbType);
            Assert.Equal(ParameterDirection.Output, attr.Direction);
        }
        
        /// <summary>
        /// Verifies that the SqlParameterAttribute constructor allows an empty parameter name without throwing an
        /// exception.
        /// </summary>
        /// <remarks>This test ensures that an empty string can be used as the parameter name when
        /// creating a SqlParameterAttribute, and that the resulting attribute correctly reflects the provided
        /// values.</remarks>
        [Fact]
        public void Constructor_Allows_EmptyParameterName()
        {
            var attr = new SqlParameterAttribute(string.Empty, DbType.String, ParameterDirection.Input);

            Assert.Equal(string.Empty, attr.ParameterName);
            Assert.Equal(DbType.String, attr.DbType);
            Assert.Equal(ParameterDirection.Input, attr.Direction);
        }

        /// <summary>
        /// Verifies that the SqlParameterAttribute constructor accepts any value of the DbType enumeration, including
        /// values outside the defined range.
        /// </summary>
        /// <remarks>This test ensures that the constructor does not restrict DbType to only known or
        /// standard enumeration values, allowing for custom or future extensions.</remarks>
        [Fact]
        public void Constructor_Allows_AnyDbType_EnumValue()
        {
            var attr = new SqlParameterAttribute("param", (DbType)999, ParameterDirection.Input);

            Assert.Equal((DbType)999, attr.DbType);
        }

        /// <summary>
        /// Verifies that the constructor of SqlParameterAttribute accepts any value of the ParameterDirection
        /// enumeration, including values outside the defined range.
        /// </summary>
        /// <remarks>This test ensures that the Direction property can be set to undefined or custom
        /// ParameterDirection values without throwing an exception. This is important for scenarios where non-standard
        /// or future enum values may be used.</remarks>
        [Fact]
        public void Constructor_Allows_AnyParameterDirection_EnumValue()
        {
            var attr = new SqlParameterAttribute("param", DbType.String, (ParameterDirection)999);

            Assert.Equal((ParameterDirection)999, attr.Direction);
        }
    }
}