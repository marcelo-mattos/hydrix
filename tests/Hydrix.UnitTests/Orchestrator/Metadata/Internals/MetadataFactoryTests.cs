using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Metadata.Internals;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Metadata.Internals
{
    /// <summary>
    /// Contains unit tests for the MetadataFactory class to verify the correct creation of property accessors,
    /// factories, and metadata objects.
    /// </summary>
    /// <remarks>These tests cover various scenarios, including handling types with and without default
    /// constructors, and ensure that MetadataFactory methods behave as expected when generating metadata for properties
    /// and entities. The tests help maintain the reliability and correctness of the MetadataFactory API.</remarks>
    public class MetadataFactoryTests
    {
        /// <summary>
        /// Represents a test class containing properties for integer and string values.
        /// </summary>
        private class TestClass
        {
            /// <summary>
            /// Gets or sets the integer value associated with this property.
            /// </summary>
            public int IntProp { get; set; }

            /// <summary>
            /// Gets or sets the string value associated with this property.
            /// </summary>
            public string StringProp { get; set; }

            /// <summary>
            /// Gets or sets the instance of the NoDefaultCtor class, which requires specific parameters for
            /// instantiation.
            /// </summary>
            /// <remarks>This property is intended for use in scenarios where a default constructor is
            /// not available, ensuring that the necessary parameters are provided during object creation.</remarks>
            public NoDefaultCtor NoDefaultCtor { get; set; }
        }

        /// <summary>
        /// Represents a class that cannot be instantiated without providing specific parameters to its constructor.
        /// </summary>
        /// <remarks>This class does not expose a public default constructor, enforcing the use of a
        /// designated constructor for instantiation. This design ensures that all required initialization parameters
        /// are supplied when creating an instance.</remarks>
        private class NoDefaultCtor
        {
            /// <summary>
            /// Initializes a new instance of the NoDefaultCtor class. This constructor is private, preventing
            /// instantiation from outside the class.
            /// </summary>
            /// <remarks>This design pattern is typically used to enforce the use of factory methods
            /// or to prevent the creation of instances without specific parameters.</remarks>
            private NoDefaultCtor()
            { }
        }

        /// <summary>
        /// Verifies that the CreateGetter method returns a delegate which retrieves the correct property value from an
        /// instance of the target class.
        /// </summary>
        /// <remarks>This test ensures that the getter delegate produced by MetadataFactory.CreateGetter
        /// accesses the specified property and returns its value as expected.</remarks>
        [Fact]
        public void CreateGetter_ReturnsCorrectValue()
        {
            var prop = typeof(TestClass).GetProperty(nameof(TestClass.IntProp));
            var getter = MetadataFactory.CreateGetter(prop);
            var obj = new TestClass { IntProp = 42 };
            Assert.Equal(42, getter(obj));
        }

        /// <summary>
        /// Verifies that the setter created by the MetadataFactory correctly assigns the specified value to the
        /// property of a TestClass instance.
        /// </summary>
        /// <remarks>This test ensures that the setter function produced for a property using reflection
        /// sets the property's value as expected. It demonstrates usage with the 'StringProp' property of TestClass and
        /// validates that the value is updated accordingly.</remarks>
        [Fact]
        public void CreateSetter_SetsValueCorrectly()
        {
            var prop = typeof(TestClass).GetProperty(nameof(TestClass.StringProp));
            var setter = MetadataFactory.CreateSetter(prop);
            var obj = new TestClass();
            setter(obj, "abc");
            Assert.Equal("abc", obj.StringProp);
        }

        /// <summary>
        /// Verifies that the factory method created by MetadataFactory successfully instantiates an object of the
        /// specified type.
        /// </summary>
        /// <remarks>This test ensures that the CreateFactory method returns a delegate capable of
        /// constructing an instance of the provided type, given that the type has a parameterless
        /// constructor.</remarks>
        [Fact]
        public void CreateFactory_CreatesInstance()
        {
            var factory = MetadataFactory.CreateFactory(typeof(TestClass));
            var obj = factory();
            Assert.IsType<TestClass>(obj);
        }

        /// <summary>
        /// Verifies that the CreateDefaultValueFactory method returns a factory function that produces the default
        /// value for a specified value type.
        /// </summary>
        /// <remarks>This test ensures that when CreateDefaultValueFactory is called with a value type,
        /// such as int, the resulting factory function returns the correct default value for that type. This is
        /// important for scenarios where default initialization of value types is required.</remarks>
        [Fact]
        public void CreateDefaultValueFactory_ValueType()
        {
            var factory = MetadataFactory.CreateDefaultValueFactory(typeof(int));
            Assert.Equal(0, factory());
        }

        /// <summary>
        /// Verifies that the default value factory created for a reference type returns null, as expected by .NET
        /// conventions.
        /// </summary>
        /// <remarks>This test ensures that when a default value factory is generated for a reference
        /// type, such as string, the resulting factory function produces a null value. This behavior aligns with the
        /// standard .NET behavior for default values of reference types.</remarks>
        [Fact]
        public void CreateDefaultValueFactory_ReferenceType()
        {
            var factory = MetadataFactory.CreateDefaultValueFactory(typeof(string));
            Assert.Null(factory());
        }

        /// <summary>
        /// Verifies that the CreateField method correctly creates a column metadata object from the specified property
        /// and column attribute.
        /// </summary>
        /// <remarks>This test ensures that the resulting metadata object accurately associates the
        /// provided property with its corresponding column attribute, which is essential for correct property-to-column
        /// mapping in ORM scenarios.</remarks>
        [Fact]
        public void CreateField_CreatesColumnMetadata()
        {
            var prop = typeof(TestClass).GetProperty(nameof(TestClass.IntProp));
            var attr = new ColumnAttribute("int_prop");
            var meta = MetadataFactory.CreateField(prop, attr);
            Assert.Equal(prop, meta.Property);
            Assert.Equal(attr, meta.Attribute);
        }

        /// <summary>
        /// Verifies that the CreateEntity method correctly creates table metadata with the specified fields and
        /// entities.
        /// </summary>
        /// <remarks>This test ensures that the metadata returned by CreateEntity contains the expected
        /// field and entity mappings, validating the integrity of the mapping process.</remarks>
        [Fact]
        public void CreateEntity_CreatesTableMetadata()
        {
            var columnAttr = new ColumnAttribute("int_prop");
            var foreignAttr = new ForeignTableAttribute("int_prop");

            var fields = new[] { new ColumnMap(typeof(TestClass).GetProperty(nameof(TestClass.IntProp)), columnAttr.Name) };
            var entities = new[] { new TableMap(typeof(TestClass).GetProperty(nameof(TestClass.NoDefaultCtor)), foreignAttr) };
            var meta = MetadataFactory.CreateEntity(fields, entities);
            Assert.Equal(fields, meta.Fields);
            Assert.Equal(entities, meta.Entities);
        }

        /// <summary>
        /// Verifies that the CreateNestedEntity method generates metadata associating a property with a specified
        /// ForeignTableAttribute.
        /// </summary>
        /// <remarks>This test ensures that the metadata returned by CreateNestedEntity correctly
        /// references both the provided property and the foreign table attribute, validating the method's behavior for
        /// managing nested entity relationships.</remarks>
        [Fact]
        public void CreateNestedEntity_CreatesForeignTableMetadata()
        {
            var prop = typeof(TestClass).GetProperty(nameof(TestClass.NoDefaultCtor));
            var attr = new ForeignTableAttribute("tbl");
            var meta = MetadataFactory.CreateNestedEntity(prop, attr);
            Assert.Equal(prop, meta.Property);
            Assert.Equal(attr, meta.Attribute);
        }
    }
}