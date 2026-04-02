using Hydrix.Attributes.Parameters;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Attributes.Parameters
{
    /// <summary>
    /// Contains unit tests for the ParameterAttribute class, verifying its constructors and property assignments.
    /// </summary>
    /// <remarks>These tests ensure that ParameterAttribute correctly sets its properties for various
    /// constructor inputs, including edge cases such as empty parameter names and undefined enum values. The tests are
    /// intended to validate the public API contract of ParameterAttribute.</remarks>
    public class ParameterAttributeTests
    {
        /// <summary>
        /// Verifies that the ParameterAttribute constructor correctly sets the ParameterName, DbType, and Direction
        /// properties.
        /// </summary>
        [Fact]
        public void Constructor_SetsProperties_Correctly()
        {
            var attr = new ParameterAttribute("param1", DbType.Int32);

            Assert.Equal("param1", attr.Name);
            Assert.Equal(DbType.Int32, attr.DbType);
            Assert.Equal(ParameterDirection.Input, attr.Direction);
        }

        /// <summary>
        /// Verifies that the ParameterAttribute constructor correctly sets the ParameterName, DbType, and Direction
        /// properties when a direction is specified.
        /// </summary>
        [Fact]
        public void Constructor_WithDirection_SetsProperties_Correctly()
        {
            var attr = new ParameterAttribute("param2", DbType.DateTime)
            {
                Direction = ParameterDirection.Output
            };

            Assert.Equal("param2", attr.Name);
            Assert.Equal(DbType.DateTime, attr.DbType);
            Assert.Equal(ParameterDirection.Output, attr.Direction);
        }

        /// <summary>
        /// Verifies that the ParameterAttribute constructor allows an empty parameter name without throwing an
        /// exception.
        /// </summary>
        /// <remarks>This test ensures that an empty string can be used as the parameter name when
        /// creating a ParameterAttribute, and that the resulting attribute correctly reflects the provided
        /// values.</remarks>
        [Fact]
        public void Constructor_Allows_EmptyParameterName()
        {
            var attr = new ParameterAttribute(string.Empty, DbType.String)
            {
                Direction = ParameterDirection.Input
            };

            Assert.Equal(string.Empty, attr.Name);
            Assert.Equal(DbType.String, attr.DbType);
            Assert.Equal(ParameterDirection.Input, attr.Direction);
        }

        /// <summary>
        /// Verifies that the ParameterAttribute constructor accepts any value of the DbType enumeration, including
        /// values outside the defined range.
        /// </summary>
        /// <remarks>This test ensures that the constructor does not restrict DbType to only known or
        /// standard enumeration values, allowing for custom or future extensions.</remarks>
        [Fact]
        public void Constructor_Allows_AnyDbType_EnumValue()
        {
            var attr = new ParameterAttribute("param", (DbType)999)
            {
                Direction = ParameterDirection.Input
            };

            Assert.Equal((DbType)999, attr.DbType);
        }

        /// <summary>
        /// Verifies that the constructor of ParameterAttribute accepts any value of the ParameterDirection
        /// enumeration, including values outside the defined range.
        /// </summary>
        /// <remarks>This test ensures that the Direction property can be set to undefined or custom
        /// ParameterDirection values without throwing an exception. This is important for scenarios where non-standard
        /// or future enum values may be used.</remarks>
        [Fact]
        public void Constructor_Allows_AnyParameterDirection_EnumValue()
        {
            var attr = new ParameterAttribute("param", DbType.String)
            {
                Direction = (ParameterDirection)999
            };

            Assert.Equal((ParameterDirection)999, attr.Direction);
        }
    }
}
