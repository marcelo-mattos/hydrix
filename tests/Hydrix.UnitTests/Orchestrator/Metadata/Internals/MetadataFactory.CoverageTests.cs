using Hydrix.Orchestrator.Metadata.Internals;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Metadata.Internals
{
    public class MetadataFactoryCoverageTests
    {
        private enum CoverageEnum
        {
            Zero = 0,
            One = 1
        }

        private sealed class CoverageEntity
        {
            public int Int32Value { get; set; }
            public int? NullableInt32Value { get; set; }
            public Guid GuidValue { get; set; }
            public CoverageEnum EnumValue { get; set; }
            public CustomReference CustomValue { get; set; }
        }

        private sealed class CustomReference
        {
            public string Value { get; set; }
        }

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

        private static object InvokePrivate(string methodName, params object[] args)
            => typeof(MetadataFactory)
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, args);
    }
}
