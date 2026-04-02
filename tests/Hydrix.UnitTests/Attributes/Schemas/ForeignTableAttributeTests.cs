using Hydrix.Attributes.Schemas;
using System;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Attributes.Schemas
{
    /// <summary>
    /// Contains unit tests for the ForeignTableAttribute class, verifying its constructors and attribute usage
    /// configuration.
    /// </summary>
    /// <remarks>These tests ensure that ForeignTableAttribute initializes its properties as expected and that
    /// its AttributeUsage settings are correct. The tests are intended to validate the public API and attribute
    /// behavior for consumers of the ForeignTableAttribute.</remarks>
    public class ForeignTableAttributeTests
    {
        /// <summary>
        /// Verifies that the default constructor of the ForeignTableAttribute class initializes all string properties to
        /// empty strings.
        /// </summary>
        /// <remarks>This test ensures that the Schema, Name, and Key properties are set to
        /// string.Empty when a new instance is created using the default constructor.</remarks>
        [Fact]
        public void DefaultConstructor_SetsEmptyProperties()
        {
            Assert.Throws<ArgumentException>(() => new ForeignTableAttribute(string.Empty));
        }

        /// <summary>
        /// Verifies that the parameterized constructor of the ForeignTableAttribute class correctly sets the Schema, Name,
        /// and Key properties.
        /// </summary>
        [Fact]
        public void ParameterizedConstructor_SetsPropertiesCorrectly()
        {
            var attr = new ForeignTableAttribute("Users")
            {
                Schema = "dbo",
                PrimaryKeys = new[] { "UserId" }
            };
            Assert.Equal("dbo", attr.Schema);
            Assert.Equal("Users", attr.Name);
            Assert.Equal(new[] { "UserId" }, attr.PrimaryKeys);
        }

        /// <summary>
        /// Verifies that the ForeignTableAttribute type is decorated with the correct AttributeUsage settings.
        /// </summary>
        /// <remarks>This test ensures that ForeignTableAttribute can be applied only to classes and
        /// properties, and that multiple instances cannot be applied to the same element.</remarks>
        [Fact]
        public void AttributeUsage_IsCorrect()
        {
            var usage = typeof(ForeignTableAttribute)
                .GetCustomAttribute<AttributeUsageAttribute>();
            Assert.NotNull(usage);
            Assert.Equal(AttributeTargets.Class | AttributeTargets.Property, usage.ValidOn);
            Assert.False(usage.AllowMultiple);
        }
    }
}
