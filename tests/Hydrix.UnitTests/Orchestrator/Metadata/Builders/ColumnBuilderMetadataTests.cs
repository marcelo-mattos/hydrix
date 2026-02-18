using Hydrix.Orchestrator.Metadata.Builders;
using Hydrix.Orchestrator.Metadata.Internals;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Metadata.Builders
{
    /// <summary>
    /// Provides unit tests for the ColumnBuilderMetadata class to verify that its properties are correctly initialized
    /// by the constructor.
    /// </summary>
    /// <remarks>These tests ensure that the ColumnBuilderMetadata constructor assigns all provided values to
    /// the corresponding properties as expected, covering different combinations of key and required flags.</remarks>
    public class ColumnBuilderMetadataTests
    {
        /// <summary>
        /// Represents a simple entity with an identifier and a name.
        /// </summary>
        private class TestEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        /// <summary>
        /// Verifies that the ColumnBuilderMetadata constructor correctly initializes all property values with the
        /// provided arguments.
        /// </summary>
        /// <remarks>This test ensures that each property of the ColumnBuilderMetadata instance reflects
        /// the values passed to the constructor, including property name, column name, key status, required status, and
        /// property information. Accurate initialization is essential for the correct functioning of metadata-dependent
        /// features.</remarks>
        [Fact]
        public void Constructor_SetsAllPropertiesCorrectly()
        {
            // Arrange
            var getter = MetadataFactory.CreateGetter(typeof(TestEntity).GetProperty(nameof(TestEntity.Id)));
            string propertyName = "Id";
            string columnName = "id";
            bool isKey = true;
            bool isRequired = false;

            // Act
            var metadata = new ColumnBuilderMetadata(
                propertyName,
                columnName,
                isKey,
                isRequired,
                getter);

            // Assert
            Assert.Equal(propertyName, metadata.PropertyName);
            Assert.Equal(columnName, metadata.ColumnName);
            Assert.Equal(isKey, metadata.IsKey);
            Assert.Equal(isRequired, metadata.IsRequired);
            Assert.Equal(getter, metadata.Getter);
        }

        /// <summary>
        /// Verifies that the ColumnBuilderMetadata constructor correctly initializes all properties when the isRequired
        /// parameter is set to true.
        /// </summary>
        /// <remarks>This test ensures that the ColumnBuilderMetadata instance is created with the
        /// expected property values, specifically validating that the isRequired flag is handled as intended when set
        /// to true.</remarks>
        [Fact]
        public void Constructor_AllowsIsRequiredTrue()
        {
            // Arrange
            var getter = MetadataFactory.CreateGetter(typeof(TestEntity).GetProperty(nameof(TestEntity.Name)));
            string propertyName = "Name";
            string columnName = "name";
            bool isKey = false;
            bool isRequired = true;

            // Act
            var metadata = new ColumnBuilderMetadata(
                propertyName,
                columnName,
                isKey,
                isRequired,
                getter);

            // Assert
            Assert.Equal(propertyName, metadata.PropertyName);
            Assert.Equal(columnName, metadata.ColumnName);
            Assert.Equal(isKey, metadata.IsKey);
            Assert.Equal(isRequired, metadata.IsRequired);
            Assert.Equal(getter, metadata.Getter);
        }
    }
}
