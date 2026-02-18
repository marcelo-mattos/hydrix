using Hydrix.Orchestrator.Mapping;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Mapping
{
    /// <summary>
    /// Contains unit tests for the ColumnMap class, verifying correct property mapping and value assignment for SQL
    /// field attributes.
    /// </summary>
    /// <remarks>These tests ensure that ColumnMap correctly associates property metadata with SQL field
    /// attributes and that its setter delegates assign values as expected. The tests use a sample TestEntity class to
    /// validate mapping behavior.</remarks>
    public class ColumnMapTests
    {
        /// <summary>
        /// Represents a test entity mapped to a database table for use with SQL-based data access.
        /// </summary>
        /// <remarks>This class is typically used in scenarios where entities are mapped to database
        /// tables using the SqlEntity attribute. It is intended for internal use within data access layers or testing
        /// contexts.</remarks>
        [Table("test")]
        private class TestEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [Column]
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents an entity that is identified by a name.
        /// </summary>
        private class RefEntity
        {
            /// <summary>
            /// Gets or sets the name of the entity.
            /// </summary>
            [Column]
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents an entity that encapsulates a Boolean flag value.
        /// </summary>
        private class BoolEntity
        {
            /// <summary>
            /// Gets or sets a value indicating whether the flag is active.
            /// </summary>
            /// <remarks>This property can be used to control conditional logic in the application
            /// based on its value.</remarks>
            [Column]
            public bool Flag { get; set; }
        }

        /// <summary>
        /// Represents an entity that contains date-related information.
        /// </summary>
        /// <remarks>This class is used to encapsulate date properties for various entities in the
        /// application.</remarks>
        private class DateEntity
        {
            /// <summary>
            /// Gets or sets the date and time when the entity was created.
            /// </summary>
            /// <remarks>This property is typically set automatically when the entity is created and
            /// should not be modified afterward.</remarks>
            [Column]
            public DateTime Created { get; set; }
        }

        /// <summary>
        /// Represents the status of an entity, indicating whether it is active.
        /// </summary>
        /// <remarks>Use this enumeration to specify or check whether an entity is currently active. The
        /// value 'None' indicates that the entity is not active, while 'Active' indicates that it is active.</remarks>
        private enum Status
        {
            /// <summary>
            /// Represents the absence of a value or a default state.
            /// </summary>
            None,

            /// <summary>
            /// Gets or sets a value indicating whether the object is currently active.
            /// </summary>
            /// <remarks>This property is typically used to determine the state of the object in
            /// relation to its operational context. An active state may imply that the object is ready to perform its
            /// intended functions.</remarks>
            Active
        }

        /// <summary>
        /// Represents an entity that maintains a state using a predefined set of status values.
        /// </summary>
        /// <remarks>Use this class to encapsulate and manage the current status of an entity within the
        /// application. The State property provides access to the entity's status, which may influence its behavior or
        /// processing logic.</remarks>
        private class EnumEntity
        {
            /// <summary>
            /// Gets or sets the current status of the entity.
            /// </summary>
            /// <remarks>The State property reflects the operational status of the entity, which can
            /// influence its behavior in the application. Ensure to validate the status before performing operations
            /// that depend on it.</remarks>
            [Column]
            public Status State { get; set; }
        }

        /// <summary>
        /// Verifies that the ColumnMap constructor correctly initializes the Property, Attribute, TargetType, and
        /// Setter properties.
        /// </summary>
        /// <remarks>This test ensures that the ColumnMap instance reflects the values provided to its
        /// constructor and that the Setter property is properly initialized.</remarks>
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Id));
            var attribute = new ColumnAttribute("id");
            var defaultValue = 0;

            // Act
            var map = new ColumnMap(property, attribute);

            // Assert
            Assert.Equal(property, map.Property);
            Assert.Equal(attribute, map.Attribute);
            Assert.Equal(defaultValue, map.DefaultValue);
            Assert.NotNull(map.Setter);
        }

        /// <summary>
        /// Verifies that the setter delegate of the ColumnMap correctly assigns the specified value to the target
        /// property of the entity.
        /// </summary>
        /// <remarks>This test ensures that invoking the Setter with a given value updates the
        /// corresponding property on the provided entity instance as expected.</remarks>
        [Fact]
        public void Setter_SetsPropertyValue()
        {
            // Arrange
            var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Id));
            var attribute = new ColumnAttribute("id");
            var map = new ColumnMap(property, attribute);
            var entity = new TestEntity();

            // Act
            map.Setter(entity, 42);

            // Assert
            Assert.Equal(42, entity.Id);
        }

        /// <summary>
        /// Verifies that the ColumnMap constructor sets the DefaultValue property to null when mapping a reference type
        /// property.
        /// </summary>
        /// <remarks>This test ensures that, for reference type properties, the ColumnMap does not assign
        /// a default value unless explicitly specified. This behavior is important to prevent unintended default
        /// assignments in entity mapping scenarios.</remarks>
        [Fact]
        public void Constructor_DefaultValue_IsNull_ForReferenceType()
        {
            // Arrange
            var property = typeof(RefEntity).GetProperty(nameof(RefEntity.Name));
            var attribute = new ColumnAttribute("name");

            // Act
            var map = new ColumnMap(property, attribute);
            // Assert
            Assert.Null(map.DefaultValue);
        }

        /// <summary>
        /// Verifies that the ColumnMap constructor sets the default value of a value type property to its type's
        /// default value.
        /// </summary>
        /// <remarks>This test ensures that when a ColumnMap is created for a boolean property, the
        /// DefaultValue property is initialized to false, which is the default for the Boolean type in .NET.</remarks>
        [Fact]
        public void Constructor_DefaultValue_IsDefault_ForValueType()
        {
            // Arrange
            var property = typeof(BoolEntity).GetProperty(nameof(BoolEntity.Flag));
            var attribute = new ColumnAttribute("flag");

            // Act
            var map = new ColumnMap(property, attribute);

            // Assert
            Assert.Equal(false, map.DefaultValue); // default(bool) == false
        }

        /// <summary>
        /// Verifies that the ColumnMap constructor sets the default value to the default DateTime value when mapping a
        /// DateTime property.
        /// </summary>
        /// <remarks>This test ensures that when a ColumnMap is created for a property of type DateTime,
        /// the DefaultValue property is initialized to DateTime.MinValue (01/01/0001 00:00:00). This behavior is
        /// important for scenarios where the absence of a value should be represented by the default
        /// DateTime.</remarks>
        [Fact]
        public void Constructor_DefaultValue_IsDefault_ForDateTimeValueType()
        {
            // Arrange
            var property = typeof(DateEntity).GetProperty(nameof(DateEntity.Created));
            var attribute = new ColumnAttribute("created");

            // Act
            var map = new ColumnMap(property, attribute);

            // Assert
            Assert.Equal(default(DateTime), map.DefaultValue); // default(DateTime) == 01/01/0001 00:00:00
        }

        /// <summary>
        /// Verifies that the ColumnMap constructor sets the default value of an enum property to the default value of
        /// the enum type.
        /// </summary>
        /// <remarks>This test ensures that when a ColumnMap is created for an enum property, the
        /// DefaultValue property is initialized to the default value of the enum, which is typically the first defined
        /// value (e.g., Status.None).</remarks>
        [Fact]
        public void Constructor_DefaultValue_IsDefault_ForEnumValueType()
        {
            var property = typeof(EnumEntity).GetProperty(nameof(EnumEntity.State));
            var attribute = new ColumnAttribute("state");

            var map = new ColumnMap(property, attribute);

            Assert.Equal(Status.None, map.DefaultValue);
        }
    }
}