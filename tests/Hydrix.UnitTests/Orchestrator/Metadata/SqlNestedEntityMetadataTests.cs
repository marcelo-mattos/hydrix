using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Metadata;
using System;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Metadata
{
    /// <summary>
    /// Unit tests for the <see cref="SqlNestedEntityMetadata"/> class.
    /// </summary>
    public class SqlNestedEntityMetadataTests
    {
        /// <summary>
        /// Dummy parent class for testing nested entity property.
        /// </summary>
        private class Parent
        {
            /// <summary>
            /// Gets or sets the nested child entity associated with this instance.
            /// </summary>
            [SqlEntity]
            public Child Nested { get; set; }
        }

        /// <summary>
        /// Dummy child class for testing instantiation and assignment.
        /// </summary>
        private class Child
        { }

        /// <summary>
        /// Verifies that the constructor correctly assigns all properties.
        /// </summary>
        [Fact]
        public void Constructor_AssignsPropertiesCorrectly()
        {
            // Arrange
            var property = typeof(Parent).GetProperty(nameof(Parent.Nested));
            var attribute = (SqlEntityAttribute)Attribute.GetCustomAttribute(property, typeof(SqlEntityAttribute));
            Func<object> factory = () => new Child();
            Action<object, object> setter = (parent, value) => ((Parent)parent).Nested = (Child)value;

            // Act
            var metadata = new SqlNestedEntityMetadata(property, attribute, factory, setter);

            // Assert
            Assert.Equal(property, metadata.Property);
            Assert.Equal(attribute, metadata.Attribute);
            Assert.Equal(factory, metadata.Factory);
            Assert.Equal(setter, metadata.Setter);
        }

        /// <summary>
        /// Verifies that the factory delegate creates a new instance of the nested entity.
        /// </summary>
        [Fact]
        public void Factory_CreatesNewInstance()
        {
            // Arrange
            Func<object> factory = () => new Child();
            var metadata = new SqlNestedEntityMetadata(
                typeof(Parent).GetProperty(nameof(Parent.Nested)),
                new SqlEntityAttribute(),
                factory,
                (parent, value) => ((Parent)parent).Nested = (Child)value);

            // Act
            var instance = metadata.Factory();

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<Child>(instance);
        }

        /// <summary>
        /// Verifies that the setter delegate assigns the nested entity to the parent.
        /// </summary>
        [Fact]
        public void Setter_AssignsNestedEntity()
        {
            // Arrange
            var parent = new Parent();
            var child = new Child();
            Action<object, object> setter = (p, v) => ((Parent)p).Nested = (Child)v;
            var metadata = new SqlNestedEntityMetadata(
                typeof(Parent).GetProperty(nameof(Parent.Nested)),
                new SqlEntityAttribute(),
                () => new Child(),
                setter);

            // Act
            metadata.Setter(parent, child);

            // Assert
            Assert.Equal(child, parent.Nested);
        }
    }
}