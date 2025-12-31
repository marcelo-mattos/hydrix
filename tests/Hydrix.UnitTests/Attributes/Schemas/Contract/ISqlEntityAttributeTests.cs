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
    /// ISqlEntityAttribute interface, and that ISqlEntityAttribute inherits from ISqlSchemaAttribute. These tests help
    /// maintain the integrity of the type hierarchy for SQL-related attributes.</remarks>
    public class ISqlEntityAttributeTestsImpl
    {
        /// <summary>
        /// Verifies that the SqlEntityAttribute type implements the ISqlEntityAttribute interface.
        /// </summary>
        [Fact]
        public void SqlEntityAttribute_Implements_ISqlEntityAttribute()
        {
            // Arrange
            var type = typeof(SqlEntityAttribute);

            // Act
            var interfaces = type.GetInterfaces();

            // Assert
            Assert.Contains(typeof(ISqlEntityAttribute), interfaces);
        }

        /// <summary>
        /// Verifies that the ISqlEntityAttribute interface inherits from ISqlSchemaAttribute.
        /// </summary>
        /// <remarks>This test ensures that ISqlEntityAttribute implements ISqlSchemaAttribute, which may
        /// be required for correct schema-related behavior in consuming code.</remarks>
        [Fact]
        public void ISqlEntityAttribute_Inherits_ISqlSchemaAttribute()
        {
            // Arrange
            var type = typeof(ISqlEntityAttribute);

            // Act
            var interfaces = type.GetInterfaces();

            // Assert
            Assert.Contains(typeof(ISqlSchemaAttribute), interfaces);
        }
    }
}