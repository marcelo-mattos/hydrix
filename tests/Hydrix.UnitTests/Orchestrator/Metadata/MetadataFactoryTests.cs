using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Metadata.Materializers;
using Hydrix.Schemas.Contract;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Metadata
{
    /// <summary>
    /// Unit tests for <see cref="MaterializeMetadataFactory"/>.
    /// </summary>
    public class MetadataFactoryTests
    {
        /// <summary>
        /// Represents a simple SQL entity with a string property for demonstration or placeholder purposes.
        /// </summary>
        private class DummyEntity : ITable
        {
            /// <summary>
            /// Gets or sets the string value associated with this property.
            /// </summary>
            public string StringProp { get; set; }
        }

        /// <summary>
        /// Dummy class for property testing.
        /// </summary>
        private class Dummy
        {
            /// <summary>
            /// Gets or sets the integer value for this property.
            /// </summary>
            public int IntProp { get; set; }

            /// <summary>
            /// Gets or sets the associated dummy entity.
            /// </summary>
            public DummyEntity EntityProp { get; set; }
        }

        /// <summary>
        /// Tests that <see cref="MaterializeMetadataFactory.CreateField"/> creates correct metadata and the setter works.
        /// </summary>
        [Fact]
        public void CreateField_CreatesCorrectMetadata_AndSetterWorks()
        {
            var prop = typeof(Dummy).GetProperty(nameof(Dummy.IntProp));
            var attr = new ColumnAttribute("int_prop");
            var metadata = MaterializeMetadataFactory.CreateField(prop, attr);

            Assert.Equal(prop, metadata.Property);
            Assert.Equal(typeof(int), metadata.TargetType);
            Assert.Equal(attr, metadata.Attribute);

            var dummy = new Dummy();
            metadata.Setter(dummy, 42);
            Assert.Equal(42, dummy.IntProp);
        }

        /// <summary>
        /// Tests that <see cref="MaterializeMetadataFactory.CreateEntity"/> creates correct metadata.
        /// </summary>
        [Fact]
        public void CreateEntity_CreatesCorrectMetadata()
        {
            var prop = typeof(Dummy).GetProperty(nameof(Dummy.IntProp));
            var attr = new ColumnAttribute("int_prop");
            var fieldMap = new ColumnMap(prop, attr, typeof(int));

            var entityProp = typeof(Dummy).GetProperty(nameof(Dummy.EntityProp));
            var entityAttr = new ForeignTableAttribute("dummy_entity")
            {
                Schema = "dummy_schema",
                PrimaryKeys = new[] { "dummy_key" }
            };
            var entityMap = new TableMap(entityProp, entityAttr);

            var fields = new List<ColumnMap> { fieldMap };
            var entities = new List<TableMap> { entityMap };

            var metadata = MaterializeMetadataFactory.CreateEntity(fields, entities);

            Assert.Equal(fields, metadata.Fields);
            Assert.Equal(entities, metadata.Entities);
        }

        /// <summary>
        /// Tests that <see cref="MaterializeMetadataFactory.CreateNestedEntity"/> creates correct metadata and delegates work.
        /// </summary>
        [Fact]
        public void CreateNestedEntity_CreatesCorrectMetadata_AndDelegatesWork()
        {
            var prop = typeof(Dummy).GetProperty(nameof(Dummy.EntityProp));
            var attr = new ForeignTableAttribute("dummy_entity")
            {
                Schema = "dummy_schema",
                PrimaryKeys = new[] { "dummy_key" }
            };
            var metadata = MaterializeMetadataFactory.CreateNestedEntity(prop, attr);

            Assert.Equal(prop, metadata.Property);
            Assert.Equal(attr, metadata.Attribute);

            var factory = metadata.Factory;
            var instance = factory();
            Assert.IsType<DummyEntity>(instance);

            var dummy = new Dummy();
            var entity = new DummyEntity { StringProp = "abc" };
            metadata.Setter(dummy, entity);
            Assert.Equal("abc", dummy.EntityProp.StringProp);
        }

        /// <summary>
        /// Tests that <see cref="MaterializeMetadataFactory.CreateSetter"/> sets property value.
        /// </summary>
        [Fact]
        public void CreateSetter_SetsPropertyValue()
        {
            var prop = typeof(Dummy).GetProperty(nameof(Dummy.IntProp));
            var setter = MaterializeMetadataFactory.CreateSetter(prop);

            var dummy = new Dummy();
            setter(dummy, 99);
            Assert.Equal(99, dummy.IntProp);
        }

        /// <summary>
        /// Tests that <see cref="MaterializeMetadataFactory.CreateFactory"/> creates a new instance.
        /// </summary>
        [Fact]
        public void CreateFactory_CreatesNewInstance()
        {
            var factory = MaterializeMetadataFactory.CreateFactory(typeof(Dummy));
            var instance = factory();
            Assert.IsType<Dummy>(instance);
        }
    }
}