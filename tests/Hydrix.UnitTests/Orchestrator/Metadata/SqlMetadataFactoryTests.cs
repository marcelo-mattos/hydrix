using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Metadata;
using Hydrix.Schemas;
using System.Collections.Generic;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Metadata
{
    /// <summary>
    /// Unit tests for <see cref="SqlMetadataFactory"/>.
    /// </summary>
    public class SqlMetadataFactoryTests
    {
        /// <summary>
        /// Represents a simple SQL entity with a string property for demonstration or placeholder purposes.
        /// </summary>
        private class DummyEntity : ISqlEntity
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
        /// Tests that <see cref="SqlMetadataFactory.CreateField"/> creates correct metadata and the setter works.
        /// </summary>
        [Fact]
        public void CreateField_CreatesCorrectMetadata_AndSetterWorks()
        {
            var prop = typeof(Dummy).GetProperty(nameof(Dummy.IntProp));
            var attr = new SqlFieldAttribute("int_prop");
            var metadata = SqlMetadataFactory.CreateField(prop, attr);

            Assert.Equal(prop, metadata.Property);
            Assert.Equal(typeof(int), metadata.TargetType);
            Assert.Equal(attr, metadata.Attribute);

            var dummy = new Dummy();
            metadata.Setter(dummy, 42);
            Assert.Equal(42, dummy.IntProp);
        }

        /// <summary>
        /// Tests that <see cref="SqlMetadataFactory.CreateEntity"/> creates correct metadata.
        /// </summary>
        [Fact]
        public void CreateEntity_CreatesCorrectMetadata()
        {
            var prop = typeof(Dummy).GetProperty(nameof(Dummy.IntProp));
            var attr = new SqlFieldAttribute("int_prop");
            var fieldMap = new SqlFieldMap(prop, attr, typeof(int));

            var entityProp = typeof(Dummy).GetProperty(nameof(Dummy.EntityProp));
            var entityAttr = new SqlEntityAttribute("dummy_schema", "dummy_entity", "dummy_key");
            var entityMap = new SqlEntityMap(entityProp, entityAttr);

            var fields = new List<SqlFieldMap> { fieldMap };
            var entities = new List<SqlEntityMap> { entityMap };

            var metadata = SqlMetadataFactory.CreateEntity(fields, entities);

            Assert.Equal(fields, metadata.Fields);
            Assert.Equal(entities, metadata.Entities);
        }

        /// <summary>
        /// Tests that <see cref="SqlMetadataFactory.CreateNestedEntity"/> creates correct metadata and delegates work.
        /// </summary>
        [Fact]
        public void CreateNestedEntity_CreatesCorrectMetadata_AndDelegatesWork()
        {
            var prop = typeof(Dummy).GetProperty(nameof(Dummy.EntityProp));
            var attr = new SqlEntityAttribute("dummy_schema", "dummy_entity", "dummy_key");
            var metadata = SqlMetadataFactory.CreateNestedEntity(prop, attr);

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
        /// Tests that <see cref="SqlMetadataFactory.CreateSetter"/> sets property value.
        /// </summary>
        [Fact]
        public void CreateSetter_SetsPropertyValue()
        {
            var prop = typeof(Dummy).GetProperty(nameof(Dummy.IntProp));
            var setter = SqlMetadataFactory.CreateSetter(prop);

            var dummy = new Dummy();
            setter(dummy, 99);
            Assert.Equal(99, dummy.IntProp);
        }

        /// <summary>
        /// Tests that <see cref="SqlMetadataFactory.CreateFactory"/> creates a new instance.
        /// </summary>
        [Fact]
        public void CreateFactory_CreatesNewInstance()
        {
            var factory = SqlMetadataFactory.CreateFactory(typeof(Dummy));
            var instance = factory();
            Assert.IsType<Dummy>(instance);
        }
    }
}