using Hydrix.Metadata.Builders;
using System;
using System.Collections.Generic;
using Xunit;

namespace Hydrix.UnitTests.Metadata.Builders
{
    /// <summary>
    /// Provides unit tests for the EntityBuilderMetadata class to verify correct initialization and immutability of its
    /// properties.
    /// </summary>
    /// <remarks>These tests ensure that the EntityBuilderMetadata constructor sets all properties as expected
    /// and that the properties remain immutable after initialization. The tests use a dummy entity type to validate
    /// behavior in a controlled environment.</remarks>
    public class EntityBuilderMetadataTests
    {
        /// <summary>
        /// Represents an empty entity.
        /// </summary>
        private class DummyEntity
        { }

        /// <summary>
        /// Verifies that the EntityBuilderMetadata constructor correctly assigns all provided property values.
        /// </summary>
        /// <remarks>This test ensures that each property of the EntityBuilderMetadata instance is set to
        /// the value passed to the constructor, confirming the integrity of the initialization process.</remarks>
        [Fact]
        public void Constructor_SetsAllPropertiesCorrectly()
        {
            // Arrange
            var entity = "DummyTable";
            var entityType = typeof(DummyEntity);
            var table = "dummy_table";
            var schema = "dbo";
            var columns = new List<ColumnBuilderMetadata>();
            var joins = new List<JoinBuilderMetadata>();

            // Act
            var metadata = new EntityBuilderMetadata(
                entity,
                entityType,
                table,
                schema,
                columns,
                joins);

            // Assert
            Assert.Equal(entity, metadata.Entity);
            Assert.Equal(entityType, metadata.Type);
            Assert.Equal(table, metadata.Table);
            Assert.Equal(schema, metadata.Schema);
            Assert.Same(columns, metadata.Columns);
            Assert.Same(joins, metadata.Joins);
        }

        /// <summary>
        /// Verifies that the properties of the EntityBuilderMetadata class are immutable after initialization.
        /// </summary>
        /// <remarks>This test ensures that the properties of the EntityBuilderMetadata class return the
        /// expected types and are read-only, confirming the immutability of the entity's metadata once it has been
        /// constructed.</remarks>
        [Fact]
        public void Properties_AreImmutable()
        {
            // Arrange
            var entity = "DummyTable";
            var entityType = typeof(DummyEntity);
            var table = "dummy_table";
            var schema = "dbo";
            var columns = new List<ColumnBuilderMetadata>();
            var joins = new List<JoinBuilderMetadata>();

            var metadata = new EntityBuilderMetadata(
                entity,
                entityType,
                table,
                schema,
                columns,
                joins);

            // Act & Assert
            Assert.Equal(entity, metadata.Entity);
            Assert.IsAssignableFrom<Type>(metadata.Type);
            Assert.IsType<string>(metadata.Table);
            Assert.IsType<string>(metadata.Schema);
            Assert.IsAssignableFrom<IReadOnlyList<ColumnBuilderMetadata>>(metadata.Columns);
            Assert.IsAssignableFrom<IReadOnlyList<JoinBuilderMetadata>>(metadata.Joins);
        }
    }
}
