using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Metadata;
using Hydrix.Schemas.Contract;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Metadata
{
    /// <summary>
    /// Unit tests for <see cref="TableMetadata"/>.
    /// </summary>
    public class TableMetadataTests
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
        /// Verifies that scalar fields and nested entities are mapped correctly.
        /// </summary>
        [Fact]
        public void BuildEntityMetadata_MapsScalarFieldsAndEntities()
        {
            var metadata = TableMetadata.BuildEntityMetadata(typeof(TestEntity));

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
            var metadata = TableMetadata.BuildEntityMetadata(typeof(TestEntity));
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
            var metadata = TableMetadata.BuildEntityMetadata(typeof(TestEntity));
            Assert.DoesNotContain(metadata.Fields, f => f.Property.Name == "NotMapped");
        }

        /// <summary>
        /// Verifies that the metadata collections are read-only after construction.
        /// </summary>
        [Fact]
        public void SqlEntityMetadata_IsImmutableAfterConstruction()
        {
            var metadata = TableMetadata.BuildEntityMetadata(typeof(TestEntity));
            Assert.IsAssignableFrom<IReadOnlyList<ColumnMap>>(metadata.Fields);
            Assert.IsAssignableFrom<IReadOnlyList<TableMap>>(metadata.Entities);
        }

        /// <summary>
        /// Verifies that no mappings are created for types without mapping attributes.
        /// </summary>
        [Fact]
        public void BuildEntityMetadata_EmptyForNoAttributes()
        {
            var metadata = TableMetadata.BuildEntityMetadata(typeof(NoAttributesEntity));
            Assert.Empty(metadata.Fields);
            Assert.Empty(metadata.Entities);
        }

        /// <summary>
        /// Verifies that TestChildEntity fields are mapped correctly and no nested entities exist.
        /// </summary>
        [Fact]
        public void BuildEntityMetadata_MapsTestChildEntityFields()
        {
            var metadata = TableMetadata.BuildEntityMetadata(typeof(TestChildEntity));

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
            var metadata = TableMetadata.BuildEntityMetadata(typeof(TestEntity));

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
            var metadata = TableMetadata.BuildEntityMetadata(typeof(TestEntity));

            Assert.DoesNotContain(metadata.Fields, f => f.Property.Name == "NotMapped");
            Assert.DoesNotContain(metadata.Entities, e => e.Property.Name == "NotMapped");
        }
    }
}