using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Mapping
{
    /// <summary>
    /// Contains unit tests for the SqlFieldMap class, verifying correct property mapping and value assignment for SQL
    /// field attributes.
    /// </summary>
    /// <remarks>These tests ensure that SqlFieldMap correctly associates property metadata with SQL field
    /// attributes and that its setter delegates assign values as expected. The tests use a sample TestEntity class to
    /// validate mapping behavior.</remarks>
    public class SqlFieldMapTests
    {
        /// <summary>
        /// Represents a test entity mapped to a database table for use with SQL-based data access.
        /// </summary>
        /// <remarks>This class is typically used in scenarios where entities are mapped to database
        /// tables using the SqlEntity attribute. It is intended for internal use within data access layers or testing
        /// contexts.</remarks>
        [SqlEntity]
        private class TestEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [SqlField]
            public int Id { get; set; }
        }

        /// <summary>
        /// Verifies that the SqlFieldMap constructor correctly initializes the Property, Attribute, TargetType, and
        /// Setter properties.
        /// </summary>
        /// <remarks>This test ensures that the SqlFieldMap instance reflects the values provided to its
        /// constructor and that the Setter property is properly initialized.</remarks>
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Id));
            var attribute = new SqlFieldAttribute("id");
            var targetType = typeof(int);

            // Act
            var map = new SqlFieldMap(property, attribute, targetType);

            // Assert
            Assert.Equal(property, map.Property);
            Assert.Equal(attribute, map.Attribute);
            Assert.Equal(targetType, map.TargetType);
            Assert.NotNull(map.Setter);
        }

        /// <summary>
        /// Verifies that the setter delegate of the SqlFieldMap correctly assigns the specified value to the target
        /// property of the entity.
        /// </summary>
        /// <remarks>This test ensures that invoking the Setter with a given value updates the
        /// corresponding property on the provided entity instance as expected.</remarks>
        [Fact]
        public void Setter_SetsPropertyValue()
        {
            // Arrange
            var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Id));
            var attribute = new SqlFieldAttribute("id");
            var targetType = typeof(int);
            var map = new SqlFieldMap(property, attribute, targetType);
            var entity = new TestEntity();

            // Act
            map.Setter(entity, 42);

            // Assert
            Assert.Equal(42, entity.Id);
        }
    }
}