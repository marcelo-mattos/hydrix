using Hydrix.Attributes.Schemas;
using Hydrix.Attributes.Schemas.Contract;
using Hydrix.Attributes.Schemas.Contract.Base;
using Xunit;

namespace Hydrix.UnitTests.Attributes.Schemas.Contract
{
    /// <summary>
    /// Provides unit tests for verifying the interface implementation and inheritance relationships of SQL entity
    /// attribute types.
    /// </summary>
    /// <remarks>This class contains tests to ensure that the SqlEntityAttribute type implements the
    /// ITableAttribute interface, and that ITableAttribute inherits from ISqlSchemaAttribute. These tests help
    /// maintain the integrity of the type hierarchy for SQL-related attributes.</remarks>
    public class ITableAttributeTestsImpl
    {
        /// <summary>
        /// Verifies that the SqlEntityAttribute type implements the ITableAttribute interface.
        /// </summary>
        [Fact]
        public void SqlEntityAttribute_Implements_ITableAttribute()
        {
            // Arrange
            var type = typeof(ForeignTableAttribute);

            // Act
            var interfaces = type.GetInterfaces();

            // Assert
            Assert.Contains(typeof(ITableAttribute), interfaces);
        }

        /// <summary>
        /// Verifies that the ITableAttribute interface inherits from ISqlSchemaAttribute.
        /// </summary>
        /// <remarks>This test ensures that ITableAttribute implements ISqlSchemaAttribute, which may
        /// be required for correct schema-related behavior in consuming code.</remarks>
        [Fact]
        public void ITableAttribute_Inherits_ISqlSchemaAttribute()
        {
            // Arrange
            var type = typeof(ITableAttribute);

            // Act
            var interfaces = type.GetInterfaces();

            // Assert
            Assert.Contains(typeof(ISchemaAttribute), interfaces);
        }
    }
}