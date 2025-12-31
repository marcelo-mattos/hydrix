using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Metadata;
using Hydrix.Schemas;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Metadata
{
    /// <summary>
    /// Unit tests for <see cref="SqlEntityMetadata"/>.
    /// </summary>
    public class SqlEntityMetadataTests
    {
        /// <summary>
        /// Test entity with no mapping attributes.
        /// </summary>
        private class NoAttributesEntity : ISqlEntity
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
        [SqlEntity("tests", "Child", "ChildId")]
        private class TestChildEntity : ISqlEntity
        {
            /// <summary>
            /// Gets or sets the identifier of the child entity associated with this record.
            /// </summary>
            [SqlField("ChildId")]
            public int ChildId { get; set; }

            /// <summary>
            /// Gets or sets the name of the child associated with this entity.
            /// </summary>
            [SqlField("ChildName")]
            public string ChildName { get; set; }
        }

        /// <summary>
        /// Test entity with scalar and nested mappings.
        /// </summary>
        [SqlEntity("tests", "Test", "Id")]
        private class TestEntity : ISqlEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [SqlField("Id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with the entity.
            /// </summary>
            [SqlField("Name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the nullable integer value associated with this instance.
            /// </summary>
            [SqlField("NullableValue")]
            public int? NullableValue { get; set; }

            /// <summary>
            /// Gets or sets the child entity associated with this instance.
            /// </summary>
            [SqlEntity("tests", "Child", "ChildId")]
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
            var metadata = SqlEntityMetadata.BuildEntityMetadata(typeof(TestEntity));

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
            Assert.IsType<SqlEntityAttribute>(entity.Attribute);
        }

        /// <summary>
        /// Verifies that nullable types are unwrapped to their underlying type.
        /// </summary>
        [Fact]
        public void BuildEntityMetadata_UnwrapsNullableTypes()
        {
            var metadata = SqlEntityMetadata.BuildEntityMetadata(typeof(TestEntity));
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
            var metadata = SqlEntityMetadata.BuildEntityMetadata(typeof(TestEntity));
            Assert.DoesNotContain(metadata.Fields, f => f.Property.Name == "NotMapped");
        }

        /// <summary>
        /// Verifies that the metadata collections are read-only after construction.
        /// </summary>
        [Fact]
        public void SqlEntityMetadata_IsImmutableAfterConstruction()
        {
            var metadata = SqlEntityMetadata.BuildEntityMetadata(typeof(TestEntity));
            Assert.IsAssignableFrom<IReadOnlyList<SqlFieldMap>>(metadata.Fields);
            Assert.IsAssignableFrom<IReadOnlyList<SqlEntityMap>>(metadata.Entities);
        }

        /// <summary>
        /// Verifies that no mappings are created for types without mapping attributes.
        /// </summary>
        [Fact]
        public void BuildEntityMetadata_EmptyForNoAttributes()
        {
            var metadata = SqlEntityMetadata.BuildEntityMetadata(typeof(NoAttributesEntity));
            Assert.Empty(metadata.Fields);
            Assert.Empty(metadata.Entities);
        }

        /// <summary>
        /// Verifies that TestChildEntity fields are mapped correctly and no nested entities exist.
        /// </summary>
        [Fact]
        public void BuildEntityMetadata_MapsTestChildEntityFields()
        {
            var metadata = SqlEntityMetadata.BuildEntityMetadata(typeof(TestChildEntity));

            Assert.NotNull(metadata);
            Assert.Equal(2, metadata.Fields.Count);
            Assert.Empty(metadata.Entities);

            var fieldNames = metadata.Fields.Select(f => f.Property.Name).ToList();
            Assert.Contains("ChildId", fieldNames);
            Assert.Contains("ChildName", fieldNames);

            var childIdField = metadata.Fields.First(f => f.Property.Name == "ChildId");
            Assert.Equal(typeof(int), childIdField.TargetType);
            Assert.IsType<SqlFieldAttribute>(childIdField.Attribute);
            Assert.Equal("ChildId", ((SqlFieldAttribute)childIdField.Attribute).FieldName);

            var childNameField = metadata.Fields.First(f => f.Property.Name == "ChildName");
            Assert.Equal(typeof(string), childNameField.TargetType);
            Assert.IsType<SqlFieldAttribute>(childNameField.Attribute);
            Assert.Equal("ChildName", ((SqlFieldAttribute)childNameField.Attribute).FieldName);
        }

        /// <summary>
        /// Verifies that TestEntity's Child property is mapped as a nested entity with correct attribute.
        /// </summary>
        [Fact]
        public void BuildEntityMetadata_MapsTestEntityChildEntity()
        {
            var metadata = SqlEntityMetadata.BuildEntityMetadata(typeof(TestEntity));

            Assert.NotNull(metadata);
            Assert.Single(metadata.Entities);

            var childEntity = metadata.Entities[0];
            Assert.Equal("Child", childEntity.Property.Name);
            Assert.IsType<SqlEntityAttribute>(childEntity.Attribute);

            var attr = (SqlEntityAttribute)childEntity.Attribute;
            Assert.Equal("tests", attr.Schema);
            Assert.Equal("Child", attr.Name);
            Assert.Equal("ChildId", attr.PrimaryKey);
        }

        /// <summary>
        /// Verifies that TestEntity's NotMapped property is not included in fields or entities.
        /// </summary>
        [Fact]
        public void BuildEntityMetadata_DoesNotMapNotMappedProperty()
        {
            var metadata = SqlEntityMetadata.BuildEntityMetadata(typeof(TestEntity));

            Assert.DoesNotContain(metadata.Fields, f => f.Property.Name == "NotMapped");
            Assert.DoesNotContain(metadata.Entities, e => e.Property.Name == "NotMapped");
        }
    }
}