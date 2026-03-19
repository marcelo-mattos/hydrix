using Hydrix.Extensions;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using Xunit;

namespace Hydrix.UnitTests.Extensions
{
    /// <summary>
    /// Contains unit tests for the ObjectExtensions class, verifying the behavior of the As&lt;T&gt;() extension method under
    /// various input scenarios.
    /// </summary>
    /// <remarks>These tests ensure that the As&lt;T&gt;() method correctly handles null values, DBNull, convertible
    /// types, and non-convertible types, returning default values or throwing exceptions as appropriate. The tests help
    /// validate the robustness and correctness of type conversion logic implemented in ObjectExtensions.</remarks>
    public class ObjectExtensionsTests
    {
        /// <summary>
        /// Represents an enum used to validate enum conversions in As&lt;T&gt; tests.
        /// </summary>
        private enum TestStatus
        {
            Unknown = 0,
            Active = 1,
            Inactive = 2
        }

        /// <summary>
        /// Verifies that the As&lt;T&gt; extension method returns the default value of the specified type when the input
        /// object is null.
        /// </summary>
        /// <remarks>This test ensures that converting a null object using As&lt;T&gt; yields the default value
        /// for value types and null for reference types, confirming safe handling of null inputs.</remarks>
        [Fact]
        public void As_ReturnsDefault_WhenValueIsNull()
        {
            object value = null;
            Assert.Equal(0, value.As<int>());
            Assert.Null(value.As<string>());
        }

        /// <summary>
        /// Verifies that the As&lt;T&gt; extension method returns the default value for the target type when the input is
        /// DBNull.
        /// </summary>
        /// <remarks>This test ensures that As&lt;T&gt; safely handles database values that are DBNull by
        /// returning default(T) for value types and null for reference types, preventing exceptions during
        /// conversion.</remarks>
        [Fact]
        public void As_ReturnsDefault_WhenValueIsDBNull()
        {
            object value = DBNull.Value;
            Assert.Equal(0, value.As<int>());
            Assert.Null(value.As<string>());
        }

        /// <summary>
        /// Verifies that the As&lt;T&gt; extension method correctly converts an object to the specified target type when the
        /// conversion is valid.
        /// </summary>
        /// <remarks>This test ensures that convertible values are accurately cast to the desired type
        /// using the As&lt;T&gt; method. It covers scenarios where the input object can be converted to an integer or a
        /// double, validating the expected outcomes.</remarks>
        [Fact]
        public void As_ConvertsValue_WhenConvertible()
        {
            object value = "123";
            Assert.Equal(123, value.As<int>());

            object doubleValue = 42;
            Assert.Equal(42.0, doubleValue.As<double>());
        }

        /// <summary>
        /// Verifies that a FormatException is thrown when attempting to convert a non-convertible object to an integer
        /// using the As&lt;T&gt; extension method.
        /// </summary>
        /// <remarks>This test ensures that the As&lt;T&gt; method correctly raises a FormatException when the
        /// input value is a string that cannot be parsed as an integer. It validates the method's behavior for invalid
        /// conversions and helps ensure robust error handling.</remarks>
        [Fact]
        public void As_ThrowsException_WhenNotConvertible()
        {
            object value = "abc";
            Assert.Throws<FormatException>(() => value.As<int>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; correctly converts values to enum types from both string and numeric sources.
        /// </summary>
        [Fact]
        public void As_ConvertsEnum_FromStringAndNumeric()
        {
            object enumName = "active";
            object enumNumber = 2;

            Assert.Equal(TestStatus.Active, enumName.As<TestStatus>());
            Assert.Equal(TestStatus.Inactive, enumNumber.As<TestStatus>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; correctly converts Guid values from Guid and string inputs.
        /// </summary>
        [Fact]
        public void As_ConvertsGuid_FromGuidAndString()
        {
            var guid = Guid.NewGuid();
            object guidValue = guid;
            object guidText = guid.ToString();

            Assert.Equal(guid, guidValue.As<Guid>());
            Assert.Equal(guid, guidText.As<Guid>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts direct Guid values for both Guid and nullable Guid targets.
        /// </summary>
        [Fact]
        public void As_ConvertsDirectGuidValue_ToGuidAndNullableGuid()
        {
            var guid = Guid.NewGuid();
            object value = guid;

            var asGuid = value.As<Guid>();
            var asNullableGuid = value.As<Guid?>();

            Assert.Equal(guid, asGuid);
            Assert.True(asNullableGuid.HasValue);
            Assert.Equal(guid, asNullableGuid.Value);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts a valid Guid string to a nullable Guid target.
        /// </summary>
        [Fact]
        public void As_ConvertsGuidString_ToNullableGuid()
        {
            var guid = Guid.NewGuid();
            object guidText = guid.ToString();

            Guid? converted = guidText.As<Guid?>();

            Assert.True(converted.HasValue);
            Assert.Equal(guid, converted.Value);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts values to nullable target types.
        /// </summary>
        [Fact]
        public void As_ConvertsValue_ToNullableType()
        {
            object value = "123";
            int? converted = value.As<int?>();

            Assert.True(converted.HasValue);
            Assert.Equal(123, converted.Value);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; returns the original value when the input is already of the target type.
        /// </summary>
        [Fact]
        public void As_ReturnsSameValue_WhenAlreadyTargetType()
        {
            object value = 77;

            Assert.Equal(77, value.As<int>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; uses the same-type shortcut for targets without explicit conversion branches.
        /// </summary>
        [Fact]
        public void As_ReturnsSameValue_WhenAlreadyTargetType_ThroughSameTypeBranch()
        {
            object value = 'Z';

            var converted = value.As<char>();

            Assert.Equal('Z', converted);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; returns null for nullable value types when the input is null.
        /// </summary>
        [Fact]
        public void As_ReturnsNull_WhenNullableTargetAndValueIsNull()
        {
            object value = null;
            int? converted = value.As<int?>();

            Assert.Null(converted);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts Guid values to nullable Guid targets through the Guid specialized path.
        /// </summary>
        [Fact]
        public void As_ConvertsGuid_ToNullableGuid()
        {
            var guid = Guid.NewGuid();
            object value = guid;

            Guid? converted = value.As<Guid?>();

            Assert.True(converted.HasValue);
            Assert.Equal(guid, converted.Value);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; throws a format exception when converting an invalid Guid string.
        /// </summary>
        [Fact]
        public void As_ThrowsFormatException_WhenGuidStringIsInvalid()
        {
            object value = "invalid-guid";

            Assert.Throws<FormatException>(() => value.As<Guid>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; throws when target is Guid and source is neither Guid nor string.
        /// </summary>
        [Fact]
        public void As_ThrowsInvalidCastException_WhenGuidTargetAndSourceIsUnsupported()
        {
            object value = 10;

            Assert.Throws<InvalidCastException>(() => value.As<Guid>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; throws when target is nullable Guid and source is neither Guid nor string.
        /// </summary>
        [Fact]
        public void As_ThrowsInvalidCastException_WhenNullableGuidTargetAndSourceIsUnsupported()
        {
            object value = 10;

            Assert.Throws<InvalidCastException>(() => value.As<Guid?>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts numeric values to nullable enum targets.
        /// </summary>
        [Fact]
        public void As_ConvertsNumericValue_ToNullableEnum()
        {
            object value = 1;

            TestStatus? converted = value.As<TestStatus?>();

            Assert.True(converted.HasValue);
            Assert.Equal(TestStatus.Active, converted.Value);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts a Guid from a byte array payload.
        /// </summary>
        [Fact]
        public void As_ConvertsGuid_FromByteArray()
        {
            var guid = Guid.NewGuid();
            object bytes = guid.ToByteArray();

            var converted = bytes.As<Guid>();

            Assert.Equal(guid, converted);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts non-string values to string using ToString().
        /// </summary>
        [Fact]
        public void As_ConvertsNonStringValue_ToString()
        {
            object value = 123;

            var converted = value.As<string>();

            Assert.Equal("123", converted);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts integral values to boolean values.
        /// </summary>
        [Fact]
        public void As_ConvertsIntegralValues_ToBoolean()
        {
            object intValue = 1;
            object shortValue = (short)0;
            object longValue = 5L;

            Assert.True(intValue.As<bool>());
            Assert.False(shortValue.As<bool>());
            Assert.True(longValue.As<bool>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts additional integral numeric types to boolean values.
        /// </summary>
        [Fact]
        public void As_ConvertsAdditionalIntegralValues_ToBoolean()
        {
            object byteValue = (byte)1;
            object sbyteValue = (sbyte)0;
            object ushortValue = (ushort)2;
            object uintValue = (uint)0;
            object ulongValue = (ulong)3;

            Assert.True(byteValue.As<bool>());
            Assert.False(sbyteValue.As<bool>());
            Assert.True(ushortValue.As<bool>());
            Assert.False(uintValue.As<bool>());
            Assert.True(ulongValue.As<bool>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; returns direct boolean values through the specialized boolean branch.
        /// </summary>
        [Fact]
        public void As_ReturnsDirectBooleanValue_WhenSourceIsBoolean()
        {
            object value = true;

            var converted = value.As<bool>();

            Assert.True(converted);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; falls back to Convert.ChangeType for boolean conversion when source is not an integer type.
        /// </summary>
        [Fact]
        public void As_ConvertsString_ToBoolean_UsingFallback()
        {
            object value = "true";

            var converted = value.As<bool>();

            Assert.True(converted);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts string values to all supported numeric target types.
        /// </summary>
        [Fact]
        public void As_ConvertsString_ToSupportedNumericTypes()
        {
            object asInt = "10";
            object asLong = "20";
            object asShort = "30";
            object asByte = "40";
            object asDecimal = "50.5";
            object asDouble = "60.25";
            object asFloat = "70.75";

            Assert.Equal(10, asInt.As<int>());
            Assert.Equal(20L, asLong.As<long>());
            Assert.Equal((short)30, asShort.As<short>());
            Assert.Equal((byte)40, asByte.As<byte>());
            Assert.Equal(50.5m, asDecimal.As<decimal>());
            Assert.Equal(60.25d, asDouble.As<double>());
            Assert.Equal(70.75f, asFloat.As<float>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts a date string to DateTime.
        /// </summary>
        [Fact]
        public void As_ConvertsString_ToDateTime()
        {
            object value = "2024-01-31T10:20:30";

            var converted = value.As<DateTime>();

            Assert.Equal(2024, converted.Year);
            Assert.Equal(1, converted.Month);
            Assert.Equal(31, converted.Day);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; returns direct DateTime values through the specialized DateTime branch.
        /// </summary>
        [Fact]
        public void As_ReturnsDirectDateTimeValue_WhenSourceIsDateTime()
        {
            var now = DateTime.UtcNow;
            object value = now;

            var converted = value.As<DateTime>();

            Assert.Equal(now, converted);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts unsupported explicit target types through fallback conversion logic.
        /// </summary>
        [Fact]
        public void As_UsesFallback_ForTypesWithoutExplicitBranch()
        {
            object value = "A";

            var converted = value.As<char>();

            Assert.Equal('A', converted);
        }

        /// <summary>
        /// Verifies that the Guid converter delegate returns direct Guid values without additional parsing.
        /// </summary>
        [Fact]
        public void BuildConverter_GuidDelegate_ReturnsDirectGuid_WhenValueIsGuid()
        {
            var method = typeof(ObjectExtensions).GetMethod(
                "BuildConverter",
                BindingFlags.NonPublic | BindingFlags.Static);

            var converter = (Func<object, object>)method.Invoke(null, new object[] { typeof(Guid) });
            var guid = Guid.NewGuid();

            var result = converter(guid);

            Assert.Equal(guid, result);
        }

        /// <summary>
        /// Verifies that the boolean converter delegate returns direct bool values without fallback conversion.
        /// </summary>
        [Fact]
        public void BuildConverter_BooleanDelegate_ReturnsDirectBool_WhenValueIsBool()
        {
            var method = typeof(ObjectExtensions).GetMethod(
                "BuildConverter",
                BindingFlags.NonPublic | BindingFlags.Static);

            var converter = (Func<object, object>)method.Invoke(null, new object[] { typeof(bool) });

            var result = converter(true);

            Assert.Equal(true, result);
        }

        /// <summary>
        /// Verifies that the DateTime converter delegate returns direct DateTime values without fallback conversion.
        /// </summary>
        [Fact]
        public void BuildConverter_DateTimeDelegate_ReturnsDirectDateTime_WhenValueIsDateTime()
        {
            var method = typeof(ObjectExtensions).GetMethod(
                "BuildConverter",
                BindingFlags.NonPublic | BindingFlags.Static);

            var converter = (Func<object, object>)method.Invoke(null, new object[] { typeof(DateTime) });
            var now = DateTime.UtcNow;

            var result = converter(now);

            Assert.Equal(now, result);
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; builds converters on demand without growing the shared cache when the cache is at its
        /// configured maximum size.
        /// </summary>
        [Fact]
        public void As_DoesNotGrowCache_WhenConverterCacheHasReachedMaximumSize()
        {
            var objectExtensionsType = typeof(ObjectExtensions);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;

            var cacheField = objectExtensionsType.GetField("_converterCache", flags);
            var lastConverterTargetTypeField = objectExtensionsType.GetField("_lastConverterTargetType", flags);
            var lastConverterField = objectExtensionsType.GetField("_lastConverter", flags);
            var buildConverterMethod = objectExtensionsType.GetMethod("BuildConverter", flags);

            var cache = (ConcurrentDictionary<Type, Func<object, object>>)cacheField.GetValue(null);
            var previousEntries = cache.ToArray();
            var previousLastType = lastConverterTargetTypeField.GetValue(null);
            var previousLastConverter = lastConverterField.GetValue(null);

            try
            {
                cache.Clear();
                lastConverterTargetTypeField.SetValue(null, null);
                lastConverterField.SetValue(null, null);

                var assemblyName = new AssemblyName($"Hydrix.DynamicTypes.{Guid.NewGuid():N}");
                var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                var module = assembly.DefineDynamicModule("Main");

                const int maxConverterCacheSize = 256;
                for (int i = 0; i < maxConverterCacheSize; i++)
                {
                    var typeBuilder = module.DefineType(
                        $"Hydrix.DynamicType{i}",
                        TypeAttributes.Public | TypeAttributes.Class);

                    var dynamicType = typeBuilder.CreateTypeInfo().AsType();
                    var converter = (Func<object, object>)buildConverterMethod.Invoke(null, new object[] { dynamicType });
                    cache.TryAdd(dynamicType, converter);
                }

                var countBefore = cache.Count;
                var result = "A".As<char>();

                Assert.Equal('A', result);
                Assert.Equal(countBefore, cache.Count);
                Assert.False(cache.ContainsKey(typeof(char)));
            }
            finally
            {
                cache.Clear();
                foreach (var entry in previousEntries)
                    cache.TryAdd(entry.Key, entry.Value);

                lastConverterTargetTypeField.SetValue(null, previousLastType);
                lastConverterField.SetValue(null, previousLastConverter);
            }
        }
    }
}