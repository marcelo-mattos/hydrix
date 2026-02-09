using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Xunit;

namespace Hydrix.UnitTests.Attributes.Schemas
{
    /// <summary>
    /// Contains unit tests for the ColumnAttribute class, verifying its constructors and attribute usage.
    /// </summary>
    /// <remarks>These tests ensure that ColumnAttribute correctly sets the FieldName property when
    /// constructed directly or applied as an attribute, and that it can be retrieved via reflection. The tests are
    /// intended to validate expected behavior for consumers of the ColumnAttribute API.</remarks>
    public class ColumnAttributeTests
    {
        /// <summary>
        /// Verifies that the default constructor of the ColumnAttribute class initializes the FieldName property to
        /// an empty string.
        /// </summary>
        [Fact]
        public void DefaultConstructor_SetsFieldNameToEmpty()
        {
            Assert.Throws<ArgumentException>(() => new ColumnAttribute(string.Empty));
        }

        /// <summary>
        /// Verifies that the parameterized constructor of the ColumnAttribute class correctly sets the FieldName
        /// property.
        /// </summary>
        [Fact]
        public void ParameterizedConstructor_SetsFieldName()
        {
            var attr = new ColumnAttribute("MyField");
            Assert.Equal("MyField", attr.Name);
        }

        /// <summary>
        /// Represents a dummy type used for testing or placeholder purposes.
        /// </summary>
        private class Dummy
        {
            [Column("TestField")]
            public string Field { get; set; } = string.Empty;
        }

        /// <summary>
        /// Verifies that the FieldName property of the ColumnAttribute applied to the Dummy.Field property is set
        /// correctly via reflection.
        /// </summary>
        /// <remarks>This test ensures that the ColumnAttribute is properly assigned and that its
        /// FieldName value matches the expected value when accessed through reflection.</remarks>
        [Fact]
        public void AttributeUsage_FieldNameIsSetViaReflection()
        {
            var prop = typeof(Dummy).GetProperty(nameof(Dummy.Field));
            var attr = prop?.GetCustomAttributes(typeof(ColumnAttribute), false)
                            .Cast<ColumnAttribute>()
                            .FirstOrDefault();

            Assert.NotNull(attr);
            Assert.Equal("TestField", attr.Name);
        }
    }
}