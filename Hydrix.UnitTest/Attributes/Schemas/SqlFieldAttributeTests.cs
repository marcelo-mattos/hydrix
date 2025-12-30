using Hydrix.Attributes.Schemas;
using System.Linq;
using Xunit;

namespace Hydrix.UnitTest.Attributes.Schemas
{
    /// <summary>
    /// Contains unit tests for the SqlFieldAttribute class, verifying its constructors and attribute usage.
    /// </summary>
    /// <remarks>These tests ensure that SqlFieldAttribute correctly sets the FieldName property when
    /// constructed directly or applied as an attribute, and that it can be retrieved via reflection. The tests are
    /// intended to validate expected behavior for consumers of the SqlFieldAttribute API.</remarks>
    public class SqlFieldAttributeTests
    {
        /// <summary>
        /// Verifies that the default constructor of the SqlFieldAttribute class initializes the FieldName property to
        /// an empty string.
        /// </summary>
        [Fact]
        public void DefaultConstructor_SetsFieldNameToEmpty()
        {
            var attr = new SqlFieldAttribute();
            Assert.Equal(string.Empty, attr.FieldName);
        }

        /// <summary>
        /// Verifies that the parameterized constructor of the SqlFieldAttribute class correctly sets the FieldName
        /// property.
        /// </summary>
        [Fact]
        public void ParameterizedConstructor_SetsFieldName()
        {
            var attr = new SqlFieldAttribute("MyField");
            Assert.Equal("MyField", attr.FieldName);
        }

        /// <summary>
        /// Represents a dummy type used for testing or placeholder purposes.
        /// </summary>
        private class Dummy
        {
            [SqlField("TestField")]
            public string Field { get; set; } = string.Empty;
        }

        /// <summary>
        /// Verifies that the FieldName property of the SqlFieldAttribute applied to the Dummy.Field property is set
        /// correctly via reflection.
        /// </summary>
        /// <remarks>This test ensures that the SqlFieldAttribute is properly assigned and that its
        /// FieldName value matches the expected value when accessed through reflection.</remarks>
        [Fact]
        public void AttributeUsage_FieldNameIsSetViaReflection()
        {
            var prop = typeof(Dummy).GetProperty(nameof(Dummy.Field));
            var attr = prop?.GetCustomAttributes(typeof(SqlFieldAttribute), false)
                            .Cast<SqlFieldAttribute>()
                            .FirstOrDefault();

            Assert.NotNull(attr);
            Assert.Equal("TestField", attr.FieldName);
        }
    }
}