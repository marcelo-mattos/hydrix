using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Metadata.Internals;
using Moq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
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
        /// Represents enum values used by coverage tests.
        /// </summary>
        private enum CoverageEnum
        {
            /// <summary>
            /// Represents the zero value.
            /// </summary>
            Zero = 0,

            /// <summary>
            /// Represents the one value.
            /// </summary>
            One = 1
        }

        /// <summary>
        /// Represents a test entity used to validate record assigner behavior.
        /// </summary>
        private sealed class CoverageEntity
        {
            /// <summary>
            /// Gets or sets an Int32 value.
            /// </summary>
            public int Int32Value { get; set; }

            /// <summary>
            /// Gets or sets a nullable Int32 value.
            /// </summary>
            public int? NullableInt32Value { get; set; }

            /// <summary>
            /// Gets or sets a Guid value.
            /// </summary>
            public Guid GuidValue { get; set; }

            /// <summary>
            /// Gets or sets an enum value.
            /// </summary>
            public CoverageEnum EnumValue { get; set; }

            /// <summary>
            /// Gets or sets a custom reference value.
            /// </summary>
            public CustomReference CustomValue { get; set; }
        }

        /// <summary>
        /// Represents a custom reference type used in fallback conversion tests.
        /// </summary>
        private sealed class CustomReference
        {
            /// <summary>
            /// Gets or sets a textual value.
            /// </summary>
            public string Value { get; set; }
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

            var prop = typeof(TestClass).GetProperty(nameof(TestClass.IntProp));
            Action<object, object> setter = (obj, val) => prop.SetValue(obj, val);
            FieldReader reader = (record, ordinal) => 123; // Dummy reader

            var fields = new[] { new ColumnMap(columnAttr.Name, setter, reader) };
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

        /// <summary>
        /// Verifies that CreateGetter throws when property metadata has no declaring type.
        /// </summary>
        [Fact]
        public void CreateGetter_Throws_WhenDeclaringTypeIsNull()
        {
            var property = new Mock<PropertyInfo>();
            property.SetupGet(p => p.Name).Returns("Orphan");
            property.SetupGet(p => p.DeclaringType).Returns((Type)null);

            Assert.Throws<InvalidOperationException>(() =>
                MetadataFactory.CreateGetter(property.Object));
        }

        /// <summary>
        /// Verifies that CreateSetter throws when property metadata has no declaring type.
        /// </summary>
        [Fact]
        public void CreateSetter_Throws_WhenDeclaringTypeIsNull()
        {
            var property = new Mock<PropertyInfo>();
            property.SetupGet(p => p.Name).Returns("Orphan");
            property.SetupGet(p => p.DeclaringType).Returns((Type)null);

            Assert.Throws<InvalidOperationException>(() =>
                MetadataFactory.CreateSetter(property.Object));
        }

        /// <summary>
        /// Verifies that <see cref="MetadataFactory.CreateRecordAssigner(PropertyInfo, int, Type)"/> throws when the
        /// property declaring type is null.
        /// </summary>
        [Fact]
        public void CreateRecordAssigner_Throws_WhenDeclaringTypeIsNull()
        {
            var property = new Mock<PropertyInfo>();
            property.SetupGet(p => p.Name).Returns("Detached");
            property.SetupGet(p => p.PropertyType).Returns(typeof(int));
            property.SetupGet(p => p.DeclaringType).Returns((Type)null);

            Assert.Throws<InvalidOperationException>(() =>
                MetadataFactory.CreateRecordAssigner(property.Object, 0, typeof(int)));
        }

        /// <summary>
        /// Verifies that enum assignment uses a typed getter when provider type maps to enum underlying type.
        /// </summary>
        [Fact]
        public void CreateRecordAssigner_UsesTypedGetter_ForEnumUnderlyingType()
        {
            var property = typeof(CoverageEntity).GetProperty(nameof(CoverageEntity.EnumValue));
            var assigner = MetadataFactory.CreateRecordAssigner(property, 0, typeof(int));
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(false);
            record.Setup(r => r.GetInt32(0)).Returns(1);
            var entity = new CoverageEntity();

            assigner(entity, record.Object);

            Assert.Equal(CoverageEnum.One, entity.EnumValue);
        }

        /// <summary>
        /// Verifies that enum assignment uses direct numeric conversion when provider type is numerically compatible.
        /// </summary>
        [Fact]
        public void CreateRecordAssigner_UsesDirectNumericConversion_ForEnumProviderType()
        {
            var property = typeof(CoverageEntity).GetProperty(nameof(CoverageEntity.EnumValue));
            var assigner = MetadataFactory.CreateRecordAssigner(property, 0, typeof(short));
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(false);
            record.Setup(r => r.GetInt16(0)).Returns((short)1);
            var entity = new CoverageEntity();

            assigner(entity, record.Object);

            Assert.Equal(CoverageEnum.One, entity.EnumValue);
        }

        /// <summary>
        /// Verifies that enum assignment uses fallback conversion when direct conversion is unavailable.
        /// </summary>
        [Fact]
        public void CreateRecordAssigner_UsesFallbackConverter_ForEnumWhenDirectConversionIsUnavailable()
        {
            var property = typeof(CoverageEntity).GetProperty(nameof(CoverageEntity.EnumValue));
            var assigner = MetadataFactory.CreateRecordAssigner(property, 0, typeof(string));
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(false);
            record.Setup(r => r.GetValue(0)).Returns("One");
            var entity = new CoverageEntity();

            assigner(entity, record.Object);

            Assert.Equal(CoverageEnum.One, entity.EnumValue);
        }

        /// <summary>
        /// Verifies that direct numeric conversion is used for compatible provider and target numeric types.
        /// </summary>
        [Fact]
        public void CreateRecordAssigner_UsesDirectNumericConversion_ForCompatibleProviderType()
        {
            var property = typeof(CoverageEntity).GetProperty(nameof(CoverageEntity.Int32Value));
            var assigner = MetadataFactory.CreateRecordAssigner(property, 0, typeof(long));
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(false);
            record.Setup(r => r.GetInt64(0)).Returns(42L);
            var entity = new CoverageEntity();

            assigner(entity, record.Object);

            Assert.Equal(42, entity.Int32Value);
        }

        /// <summary>
        /// Verifies that fallback conversion is used when provider type is unsupported for direct conversion.
        /// </summary>
        [Fact]
        public void CreateRecordAssigner_UsesFallbackConverter_ForUnsupportedProviderType()
        {
            var guid = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
            var property = typeof(CoverageEntity).GetProperty(nameof(CoverageEntity.GuidValue));
            var assigner = MetadataFactory.CreateRecordAssigner(property, 0, typeof(byte[]));
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(false);
            record.Setup(r => r.GetValue(0)).Returns(guid.ToByteArray());
            var entity = new CoverageEntity();

            assigner(entity, record.Object);

            Assert.Equal(guid, entity.GuidValue);
        }

        /// <summary>
        /// Verifies that fallback conversion is used when source provider type is unknown.
        /// </summary>
        [Fact]
        public void CreateRecordAssigner_UsesFallbackConverter_WhenSourceTypeIsUnknown()
        {
            var value = new CustomReference { Value = "custom" };
            var property = typeof(CoverageEntity).GetProperty(nameof(CoverageEntity.CustomValue));
            var assigner = MetadataFactory.CreateRecordAssigner(property, 0, null);
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(false);
            record.Setup(r => r.GetValue(0)).Returns(value);
            var entity = new CoverageEntity();

            assigner(entity, record.Object);

            Assert.Same(value, entity.CustomValue);
        }

        /// <summary>
        /// Verifies that assignment converts values to nullable target types.
        /// </summary>
        [Fact]
        public void CreateRecordAssigner_ConvertsToNullableTargetType()
        {
            var property = typeof(CoverageEntity).GetProperty(nameof(CoverageEntity.NullableInt32Value));
            var assigner = MetadataFactory.CreateRecordAssigner(property, 0, typeof(int));
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(false);
            record.Setup(r => r.GetInt32(0)).Returns(9);
            var entity = new CoverageEntity();

            assigner(entity, record.Object);

            Assert.Equal(9, entity.NullableInt32Value);
        }

        /// <summary>
        /// Verifies that assignment uses default target values when the record field is <see cref="DBNull"/>.
        /// </summary>
        [Fact]
        public void CreateRecordAssigner_AssignsDefaultValue_WhenRecordContainsDBNull()
        {
            var property = typeof(CoverageEntity).GetProperty(nameof(CoverageEntity.Int32Value));
            var assigner = MetadataFactory.CreateRecordAssigner(property, 0, typeof(int));
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(true);
            var entity = new CoverageEntity { Int32Value = 99 };

            assigner(entity, record.Object);

            Assert.Equal(0, entity.Int32Value);
        }

        /// <summary>
        /// Verifies that direct typed conversion permits only same-type pairs or numeric-compatible pairs.
        /// </summary>
        [Fact]
        public void CanUseDirectTypedConversion_AllowsOnlySameTypeOrNumericPairs()
        {
            Assert.True((bool)InvokePrivate(
                "CanUseDirectTypedConversion",
                typeof(int),
                typeof(int)));

            Assert.True((bool)InvokePrivate(
                "CanUseDirectTypedConversion",
                typeof(long),
                typeof(int)));

            Assert.False((bool)InvokePrivate(
                "CanUseDirectTypedConversion",
                typeof(bool),
                typeof(int)));
        }

        /// <summary>
        /// Verifies numeric type recognition for supported and unsupported types.
        /// </summary>
        /// <param name="type">The type under evaluation.</param>
        /// <param name="expected">The expected numeric recognition result.</param>
        [Theory]
        [InlineData(typeof(byte), true)]
        [InlineData(typeof(short), true)]
        [InlineData(typeof(int), true)]
        [InlineData(typeof(long), true)]
        [InlineData(typeof(float), true)]
        [InlineData(typeof(double), true)]
        [InlineData(typeof(decimal), true)]
        [InlineData(typeof(string), false)]
        public void IsNumericType_RecognizesSupportedTypes(Type type, bool expected)
        {
            var result = (bool)InvokePrivate("IsNumericType", type);

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Verifies that direct getter-call creation returns false when source type is null or unsupported.
        /// </summary>
        [Fact]
        public void TryCreateDirectConversionGetterCall_ReturnsFalse_WhenSourceTypeIsNullOrUnsupported()
        {
            var record = Expression.Parameter(typeof(IDataRecord), "record");
            var ordinal = Expression.Constant(0);
            var method = typeof(MetadataFactory).GetMethod(
                "TryCreateDirectConversionGetterCall",
                BindingFlags.NonPublic | BindingFlags.Static);

            var nullSourceArgs = new object[] { record, ordinal, null, typeof(int), null };
            var unsupportedSourceArgs = new object[] { record, ordinal, typeof(byte[]), typeof(int), null };
            var nonNumericEnumArgs = new object[] { record, ordinal, typeof(bool), typeof(CoverageEnum), null };
            var nonNumericTargetArgs = new object[] { record, ordinal, typeof(bool), typeof(Guid), null };

            Assert.False((bool)method.Invoke(null, nullSourceArgs));
            Assert.Null(nullSourceArgs[4]);
            Assert.False((bool)method.Invoke(null, unsupportedSourceArgs));
            Assert.Null(unsupportedSourceArgs[4]);
            Assert.False((bool)method.Invoke(null, nonNumericEnumArgs));
            Assert.Null(nonNumericEnumArgs[4]);
            Assert.False((bool)method.Invoke(null, nonNumericTargetArgs));
            Assert.Null(nonNumericTargetArgs[4]);
        }

        /// <summary>
        /// Invokes a non-public static method on <see cref="MetadataFactory"/> and returns its raw result.
        /// </summary>
        /// <param name="methodName">The name of the non-public static method to invoke.</param>
        /// <param name="args">The arguments passed to the invoked method.</param>
        /// <returns>The raw return value produced by the invoked method.</returns>
        private static object InvokePrivate(
            string methodName,
            params object[] args)
            => typeof(MetadataFactory)
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, args);
    }
}
