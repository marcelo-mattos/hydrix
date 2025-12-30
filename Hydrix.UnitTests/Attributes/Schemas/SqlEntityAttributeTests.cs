using Hydrix.Attributes.Schemas;
using System;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Attributes.Schemas
{
    /// <summary>
    /// Contains unit tests for the SqlEntityAttribute class, verifying its constructors and attribute usage
    /// configuration.
    /// </summary>
    /// <remarks>These tests ensure that SqlEntityAttribute initializes its properties as expected and that
    /// its AttributeUsage settings are correct. The tests are intended to validate the public API and attribute
    /// behavior for consumers of the SqlEntityAttribute.</remarks>
    public class SqlEntityAttributeTests
    {
        /// <summary>
        /// Verifies that the default constructor of the SqlEntityAttribute class initializes all string properties to
        /// empty strings.
        /// </summary>
        /// <remarks>This test ensures that the Schema, Name, and PrimaryKey properties are set to
        /// string.Empty when a new instance is created using the default constructor.</remarks>
        [Fact]
        public void DefaultConstructor_SetsEmptyProperties()
        {
            var attr = new SqlEntityAttribute();
            Assert.Equal(string.Empty, attr.Schema);
            Assert.Equal(string.Empty, attr.Name);
            Assert.Equal(string.Empty, attr.PrimaryKey);
        }

        /// <summary>
        /// Verifies that the parameterized constructor of the SqlEntityAttribute class correctly sets the Schema, Name,
        /// and PrimaryKey properties.
        /// </summary>
        [Fact]
        public void ParameterizedConstructor_SetsPropertiesCorrectly()
        {
            var attr = new SqlEntityAttribute("dbo", "Users", "UserId");
            Assert.Equal("dbo", attr.Schema);
            Assert.Equal("Users", attr.Name);
            Assert.Equal("UserId", attr.PrimaryKey);
        }
        
        /// <summary>
        /// Verifies that the SqlEntityAttribute type is decorated with the correct AttributeUsage settings.
        /// </summary>
        /// <remarks>This test ensures that SqlEntityAttribute can be applied only to classes and
        /// properties, and that multiple instances cannot be applied to the same element.</remarks>
        [Fact]
        public void AttributeUsage_IsCorrect()
        {
            var usage = typeof(SqlEntityAttribute)
                .GetCustomAttribute<AttributeUsageAttribute>();
            Assert.NotNull(usage);
            Assert.Equal(AttributeTargets.Class | AttributeTargets.Property, usage.ValidOn);
            Assert.False(usage.AllowMultiple);
        }
    }
}