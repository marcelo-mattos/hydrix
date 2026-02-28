using Hydrix.Orchestrator.Metadata.Materializers;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Metadata.Materializers
{
    /// <summary>
    /// Unit tests for the <see cref="ColumnMaterializeMetadata"/> class.
    /// </summary>
    public class ColumnMaterializeMetadataTests
    {
        /// <summary>
        /// Simple test entity for property reflection.
        /// </summary>
        private class TestEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            public int Id { get; set; }
        }

        /// <summary>
        /// Verifies that the constructor correctly assigns all properties.
        /// </summary>
        [Fact]
        public void Constructor_Assigns_All_Properties()
        {
            // Arrange
            var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Id));
            var setter = new Action<object, object>((obj, value) => property.SetValue(obj, value));
            var targetType = typeof(int);
            var attribute = new ColumnAttribute("Id");

            // Act
            var metadata = new ColumnMaterializeMetadata(property, setter, targetType, attribute);

            // Assert
            Assert.Equal(property, metadata.Property);
            Assert.Equal(setter, metadata.Setter);
            Assert.Equal(targetType, metadata.TargetType);
            Assert.Equal(attribute, metadata.Attribute);
        }

        /// <summary>
        /// Verifies that the setter delegate sets the property value as expected.
        /// </summary>
        [Fact]
        public void Setter_Delegate_Sets_Property_Value()
        {
            // Arrange
            var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Id));
            var setter = new Action<object, object>((obj, value) => property.SetValue(obj, value));
            var targetType = typeof(int);
            var attribute = new ColumnAttribute("Id");
            var metadata = new ColumnMaterializeMetadata(property, setter, targetType, attribute);
            var entity = new TestEntity();

            // Act
            metadata.Setter(entity, 123);

            // Assert
            Assert.Equal(123, entity.Id);
        }

        /// <summary>
        /// Verifies that the constructor allows null arguments and assigns them.
        /// </summary>
        [Fact]
        public void Constructor_Allows_Null_Arguments()
        {
            // Act
            var metadata = new ColumnMaterializeMetadata(null, null, null, null);

            // Assert
            Assert.Null(metadata.Property);
            Assert.Null(metadata.Setter);
            Assert.Null(metadata.TargetType);
            Assert.Null(metadata.Attribute);
        }
    }
}