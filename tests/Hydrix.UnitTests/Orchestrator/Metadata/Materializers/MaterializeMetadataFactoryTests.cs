using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Metadata.Internals;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Metadata.Materializers
{
    /// <summary>
    /// Unit tests for <see cref="MetadataFactory"/>.
    /// </summary>
    public class MaterializeMetadataFactoryTests
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
        /// Tests that <see cref="MetadataFactory.CreateField"/> creates correct metadata and the setter works.
        /// </summary>
        [Fact]
        public void CreateField_CreatesCorrectMetadata_AndSetterWorks()
        {
            var prop = typeof(Dummy).GetProperty(nameof(Dummy.IntProp));
            var attr = new ColumnAttribute("int_prop");
            var metadata = MetadataFactory.CreateField(prop, attr);

            Assert.Equal(prop, metadata.Property);
            Assert.Equal(typeof(int), metadata.TargetType);
            Assert.Equal(attr, metadata.Attribute);

            var dummy = new Dummy();
            metadata.Setter(dummy, 42);
            Assert.Equal(42, dummy.IntProp);
        }

        /// <summary>
        /// Tests that <see cref="MetadataFactory.CreateEntity"/> creates correct metadata.
        /// </summary>
        [Fact]
        public void CreateEntity_CreatesCorrectMetadata()
        {
            var prop = typeof(Dummy).GetProperty(nameof(Dummy.IntProp));
            var fieldName = "int_prop";
            Action<object, object> setter = (obj, val) => prop.SetValue(obj, val);
            FieldReader reader = (record, ordinal) => 42; // Dummy reader
            var fieldMap = new ColumnMap(fieldName, setter, reader);

            var entityProp = typeof(Dummy).GetProperty(nameof(Dummy.EntityProp));
            var entityAttr = new ForeignTableAttribute("dummy_entity")
            {
                Schema = "dummy_schema",
                PrimaryKeys = new[] { "dummy_key" }
            };
            var entityMap = new TableMap(entityProp, entityAttr);

            var fields = new List<ColumnMap> { fieldMap };
            var entities = new List<TableMap> { entityMap };

            var metadata = MetadataFactory.CreateEntity(fields, entities);

            Assert.Equal(fields, metadata.Fields);
            Assert.Equal(entities, metadata.Entities);
        }

        /// <summary>
        /// Tests that <see cref="MetadataFactory.CreateNestedEntity"/> creates correct metadata and delegates work.
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
            var metadata = MetadataFactory.CreateNestedEntity(prop, attr);

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
        /// Tests that <see cref="MetadataFactory.CreateSetter"/> sets property value.
        /// </summary>
        [Fact]
        public void CreateSetter_SetsPropertyValue()
        {
            var prop = typeof(Dummy).GetProperty(nameof(Dummy.IntProp));
            var setter = MetadataFactory.CreateSetter(prop);

            var dummy = new Dummy();
            setter(dummy, 99);
            Assert.Equal(99, dummy.IntProp);
        }

        /// <summary>
        /// Tests that <see cref="MetadataFactory.CreateFactory"/> creates a new instance.
        /// </summary>
        [Fact]
        public void CreateFactory_CreatesNewInstance()
        {
            var factory = MetadataFactory.CreateFactory(typeof(Dummy));
            var instance = factory();
            Assert.IsType<Dummy>(instance);
        }
    }
}