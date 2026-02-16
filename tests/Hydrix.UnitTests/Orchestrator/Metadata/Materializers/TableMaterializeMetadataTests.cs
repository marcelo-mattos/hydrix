using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Metadata.Materializers;
using Hydrix.Schemas.Contract;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Metadata.Materializers
{
    /// <summary>
    /// Unit tests for <see cref="TableMaterializeMetadata"/>.
    /// </summary>
    public class TableMaterializeMetadataTests
    {
        /// <summary>
        /// Test entity with no mapping attributes.
        /// </summary>
        private class NoAttributesEntity : ITable
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with the object.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Test child entity for nested mapping.
        /// </summary>
        [Table("Child", Schema = "tests")]
        private class TestChildEntity : ITable
        {
            /// <summary>
            /// Gets or sets the identifier of the child entity associated with this record.
            /// </summary>
            [Column("ChildId")]
            public int ChildId { get; set; }

            /// <summary>
            /// Gets or sets the name of the child associated with this entity.
            /// </summary>
            [Column("ChildName")]
            public string ChildName { get; set; }
        }

        /// <summary>
        /// Test entity with scalar and nested mappings.
        /// </summary>
        [Table("Test", Schema = "tests")]
        private class TestEntity : ITable
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [Column("Id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with the entity.
            /// </summary>
            [Column("Name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the nullable integer value associated with this instance.
            /// </summary>
            [Column("NullableValue")]
            public int? NullableValue { get; set; }

            /// <summary>
            /// Gets or sets the child entity associated with this instance.
            /// </summary>
            [ForeignTable("Child", Schema = "tests", PrimaryKeys = new[] { "ChildId" })]
            public TestChildEntity Child { get; set; }

            /// <summary>
            /// Gets or sets the value that is not mapped to any database column.
            /// </summary>
            public string NotMapped { get; set; }
        }

        /// <summary>
        /// Represents a product in the inventory system, including its identification, name, price, and associated
        /// category.
        /// </summary>
        /// <remarks>The product's price is nullable, indicating that it may not be set. Each product is
        /// linked to a category, which provides context for its classification.</remarks>
        [Table("products")]
        private class Product
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [Column("id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with the entity.
            /// </summary>
            [Column("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the price of the item. This property may be null if the price is not specified.
            /// </summary>
            /// <remarks>The price is represented as a nullable decimal value, allowing for scenarios
            /// where the price may be unknown or not applicable.</remarks>
            [Column("price")]
            public decimal? Price { get; set; }

            /// <summary>
            /// Gets or sets the category associated with the current entity.
            /// </summary>
            /// <remarks>The category provides a way to classify the entity, allowing for better
            /// organization and retrieval of related items.</remarks>
            [ForeignTable("categories", Alias = "c", Schema = "dbo", PrimaryKeys = new[] { "Id" }, ForeignKeys = new[] { "CategoryId" })]
            public Category Category { get; set; }
        }

        /// <summary>
        /// Represents a category entity with an identifier and a name.
        /// </summary>
        /// <remarks>This class is typically used to organize or classify items within the application.
        /// Each instance corresponds to a record in the 'categories' database table.</remarks>
        [Table("categories")]
        private class Category
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [Column("id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with the entity.
            /// </summary>
            [Column("name")]
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents an entity with an immutable identifier and a reference to a related child entity.
        /// </summary>
        /// <remarks>All properties except 'NotMapped' are read-only and can only be set during object
        /// construction. The 'Id' property is mapped to a database column, while the 'Child' property represents a
        /// foreign relationship to another entity. The 'NotMapped' property is not persisted in the database.</remarks>
        private class NoSetterEntity
        {
            /// <summary>
            /// Gets the unique identifier for the entity.
            /// </summary>
            [Column("id")]
            public int Id { get; }

            /// <summary>
            /// Gets the child object associated with this instance.
            /// </summary>
            /// <remarks>The child object is retrieved from the foreign table 'child'. This property
            /// is read-only and cannot be set directly.</remarks>
            [ForeignTable("child")]
            public object Child { get; }

            /// <summary>
            /// Gets or sets the value indicating that this property is not mapped to a database column.
            /// </summary>
            /// <remarks>Use this property to mark data members that should be excluded from database
            /// persistence. This is useful for properties that are used only within the application and do not require
            /// storage in the database schema.</remarks>
            public string NotMapped { get; set; }
        }

        /// <summary>
        /// Represents a placeholder class with no fields or entities defined.
        /// </summary>
        private class NoFieldsOrEntities
        { }

        /// <summary>
        /// Verifies that scalar fields and nested entities are mapped correctly.
        /// </summary>
        [Fact]
        public void BuildEntityMetadata_MapsScalarFieldsAndEntities()
        {
            var metadata = TableMaterializeMetadata.BuildEntityMetadata(typeof(TestEntity));

            Assert.NotNull(metadata);
            Assert.Equal(3, metadata.Fields.Count);
            Assert.Single(metadata.Entities);

            var fieldNames = new HashSet<string>();
            foreach (var field in metadata.Fields)
                fieldNames.Add(field.Property.Name);

            Assert.Contains("Id", fieldNames);
            Assert.Contains("Name", fieldNames);
            Assert.Contains("NullableValue", fieldNames);

            var entity = metadata.Entities[0];
            Assert.Equal("Child", entity.Property.Name);
            Assert.IsType<ForeignTableAttribute>(entity.Attribute);
        }

        /// <summary>
        /// Verifies that nullable types are unwrapped to their underlying type.
        /// </summary>
        [Fact]
        public void BuildEntityMetadata_UnwrapsNullableTypes()
        {
            var metadata = TableMaterializeMetadata.BuildEntityMetadata(typeof(TestEntity));
            var nullableField = metadata.Fields.FirstOrDefault(f => f.Property.Name == "NullableValue");
            Assert.NotNull(nullableField);
            Assert.Equal(typeof(int), nullableField.TargetType);
        }

        /// <summary>
        /// Verifies that properties without mapping attributes are ignored.
        /// </summary>
        [Fact]
        public void BuildEntityMetadata_IgnoresNonDecoratedProperties()
        {
            var metadata = TableMaterializeMetadata.BuildEntityMetadata(typeof(TestEntity));
            Assert.DoesNotContain(metadata.Fields, f => f.Property.Name == "NotMapped");
        }

        /// <summary>
        /// Verifies that the metadata collections are read-only after construction.
        /// </summary>
        [Fact]
        public void SqlEntityMetadata_IsImmutableAfterConstruction()
        {
            var metadata = TableMaterializeMetadata.BuildEntityMetadata(typeof(TestEntity));
            Assert.IsAssignableFrom<IReadOnlyList<ColumnMap>>(metadata.Fields);
            Assert.IsAssignableFrom<IReadOnlyList<TableMap>>(metadata.Entities);
        }

        /// <summary>
        /// Verifies that no mappings are created for types without mapping attributes.
        /// </summary>
        [Fact]
        public void BuildEntityMetadata_EmptyForNoAttributes()
        {
            var metadata = TableMaterializeMetadata.BuildEntityMetadata(typeof(NoAttributesEntity));
            Assert.Empty(metadata.Fields);
            Assert.Empty(metadata.Entities);
        }

        /// <summary>
        /// Verifies that TestChildEntity fields are mapped correctly and no nested entities exist.
        /// </summary>
        [Fact]
        public void BuildEntityMetadata_MapsTestChildEntityFields()
        {
            var metadata = TableMaterializeMetadata.BuildEntityMetadata(typeof(TestChildEntity));

            Assert.NotNull(metadata);
            Assert.Equal(2, metadata.Fields.Count);
            Assert.Empty(metadata.Entities);

            var fieldNames = metadata.Fields.Select(f => f.Property.Name).ToList();
            Assert.Contains("ChildId", fieldNames);
            Assert.Contains("ChildName", fieldNames);

            var childIdField = metadata.Fields.First(f => f.Property.Name == "ChildId");
            Assert.Equal(typeof(int), childIdField.TargetType);
            Assert.IsType<ColumnAttribute>(childIdField.Attribute);
            Assert.Equal("ChildId", ((ColumnAttribute)childIdField.Attribute).Name);

            var childNameField = metadata.Fields.First(f => f.Property.Name == "ChildName");
            Assert.Equal(typeof(string), childNameField.TargetType);
            Assert.IsType<ColumnAttribute>(childNameField.Attribute);
            Assert.Equal("ChildName", ((ColumnAttribute)childNameField.Attribute).Name);
        }

        /// <summary>
        /// Verifies that TestEntity's Child property is mapped as a nested entity with correct attribute.
        /// </summary>
        [Fact]
        public void BuildEntityMetadata_MapsTestEntityChildEntity()
        {
            var metadata = TableMaterializeMetadata.BuildEntityMetadata(typeof(TestEntity));

            Assert.NotNull(metadata);
            Assert.Single(metadata.Entities);

            var childEntity = metadata.Entities[0];
            Assert.Equal("Child", childEntity.Property.Name);
            Assert.IsType<ForeignTableAttribute>(childEntity.Attribute);

            var attr = (ForeignTableAttribute)childEntity.Attribute;
            Assert.Equal("tests", attr.Schema);
            Assert.Equal("Child", attr.Name);
            Assert.Equal(new[] { "ChildId" }, attr.PrimaryKeys);
        }

        /// <summary>
        /// Verifies that TestEntity's NotMapped property is not included in fields or entities.
        /// </summary>
        [Fact]
        public void BuildEntityMetadata_DoesNotMapNotMappedProperty()
        {
            var metadata = TableMaterializeMetadata.BuildEntityMetadata(typeof(TestEntity));

            Assert.DoesNotContain(metadata.Fields, f => f.Property.Name == "NotMapped");
            Assert.DoesNotContain(metadata.Entities, e => e.Property.Name == "NotMapped");
        }

        /// <summary>
        /// Verifies that the TableMaterializeMetadata constructor correctly assigns the provided fields and entities to
        /// the corresponding properties.
        /// </summary>
        /// <remarks>This test ensures that the constructor does not create copies of the input lists, but
        /// instead assigns the references directly. This behavior is important for scenarios where reference equality
        /// is required or when the caller expects changes to the original lists to be reflected in the metadata
        /// instance.</remarks>
        [Fact]
        public void Constructor_SetsFieldsAndEntities()
        {
            // Arrange
            var fields = new List<ColumnMap>();
            var entities = new List<TableMap>();

            // Act
            var metadata = new TableMaterializeMetadata(fields, entities);

            // Assert
            Assert.Same(fields, metadata.Fields);
            Assert.Same(entities, metadata.Entities);
        }

        /// <summary>
        /// Verifies that the entity metadata builder correctly maps fields and related entities for the specified type.
        /// </summary>
        /// <remarks>This test ensures that the metadata produced for the Product type contains the
        /// expected fields and entity relationships, including correct attribute mapping and handling of nullable
        /// types. It also validates that foreign table relationships are properly identified and mapped.</remarks>
        [Fact]
        public void BuildEntityMetadata_MapsFieldsAndEntitiesCorrectly()
        {
            // Act
            var metadata = TableMaterializeMetadata.BuildEntityMetadata(typeof(Product));

            // Assert
            Assert.NotNull(metadata);
            Assert.NotNull(metadata.Fields);
            Assert.NotNull(metadata.Entities);

            // Fields
            Assert.Equal(3, metadata.Fields.Count);
            Assert.Contains(metadata.Fields, f => f.Property.Name == "Id" && f.Attribute.Name == "id");
            Assert.Contains(metadata.Fields, f => f.Property.Name == "Name" && f.Attribute.Name == "name");
            Assert.Contains(metadata.Fields, f => f.Property.Name == "Price" && f.Attribute.Name == "price");

            // Nullable type is unwrapped
            var priceField = metadata.Fields.First(f => f.Property.Name == "Price");
            Assert.Equal(typeof(decimal), priceField.TargetType);

            // Entities
            Assert.Single(metadata.Entities);
            var entity = metadata.Entities[0];
            Assert.Equal("Category", entity.Property.Name);
            Assert.IsType<ForeignTableAttribute>(entity.Attribute);
            Assert.Equal("categories", ((ForeignTableAttribute)entity.Attribute).Name);
        }

        /// <summary>
        /// Verifies that building entity metadata for a type with no fields or entities produces non-null metadata with
        /// empty collections.
        /// </summary>
        /// <remarks>This test ensures that the metadata builder correctly handles types that do not
        /// define any fields or entities, resulting in empty metadata collections without errors.</remarks>
        [Fact]
        public void BuildEntityMetadata_HandlesNoFieldsOrEntities()
        {
            // Arrange
            var type = typeof(NoFieldsOrEntities);

            // Act
            var metadata = TableMaterializeMetadata.BuildEntityMetadata(type);

            // Assert
            Assert.NotNull(metadata);
            Assert.Empty(metadata.Fields);
            Assert.Empty(metadata.Entities);
        }

        /// <summary>
        /// Verifies that the entity metadata builder excludes properties that lack a setter or a required attribute.
        /// </summary>
        /// <remarks>This test ensures that when building metadata for an entity type, only properties
        /// with setters or the appropriate attribute are included. Properties without setters or the necessary
        /// attribute should not appear in the resulting metadata.</remarks>
        [Fact]
        public void BuildEntityMetadata_IgnoresPropertiesWithoutSetterOrAttribute()
        {
            var metadata = TableMaterializeMetadata.BuildEntityMetadata(typeof(NoSetterEntity));

            Assert.Empty(metadata.Fields);
            Assert.Empty(metadata.Entities);
        }
    }
}