using Hydrix.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

            Assert.True((bool)result);
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

            var cacheField = objectExtensionsType.GetField("ConverterCache", flags);
            var cacheSizeField = objectExtensionsType.GetField("_converterCacheSize", flags);
            var lastConverterTargetTypeField = objectExtensionsType.GetField("_lastConverterTargetType", flags);
            var lastConverterField = objectExtensionsType.GetField("_lastConverter", flags);
            var buildConverterMethod = objectExtensionsType.GetMethod("BuildConverter", flags);

            var cache = (ConcurrentDictionary<Type, Func<object, object>>)cacheField.GetValue(null);
            var previousEntries = cache.ToArray();
            var previousCacheSize = (int)cacheSizeField.GetValue(null);
            var previousLastType = lastConverterTargetTypeField.GetValue(null);
            var previousLastConverter = lastConverterField.GetValue(null);

            try
            {
                cache.Clear();
                cacheSizeField.SetValue(null, 0);
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

                cacheSizeField.SetValue(null, maxConverterCacheSize);

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

                cacheSizeField.SetValue(null, previousCacheSize);
                lastConverterTargetTypeField.SetValue(null, previousLastType);
                lastConverterField.SetValue(null, previousLastConverter);
            }
        }

        /// <summary>
        /// Verifies that GetConverter returns the newly built converter when cache insertion is skipped and no
        /// converter exists in the dictionary for the target type.
        /// </summary>
        [Fact]
        public void GetConverter_ReturnsCurrentConverter_WhenCacheLimitReachedAndTypeIsNotCached()
        {
            var objectExtensionsType = typeof(ObjectExtensions);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;

            var cacheField = objectExtensionsType.GetField("ConverterCache", flags);
            var cacheSizeField = objectExtensionsType.GetField("_converterCacheSize", flags);
            var maxCacheSizeField = objectExtensionsType.GetField("MaxConverterCacheSize", flags);
            var lastConverterTargetTypeField = objectExtensionsType.GetField("_lastConverterTargetType", flags);
            var lastConverterField = objectExtensionsType.GetField("_lastConverter", flags);

            var cache = (ConcurrentDictionary<Type, Func<object, object>>)cacheField.GetValue(null);
            var previousEntries = cache.ToArray();
            var previousCacheSize = (int)cacheSizeField.GetValue(null);
            var previousLastType = lastConverterTargetTypeField.GetValue(null);
            var previousLastConverter = lastConverterField.GetValue(null);

            try
            {
                cache.Clear();
                lastConverterTargetTypeField.SetValue(null, null);
                lastConverterField.SetValue(null, null);

                var maxCacheSize = (int)maxCacheSizeField.GetValue(null);
                cacheSizeField.SetValue(null, maxCacheSize);

                var targetType = typeof(DateTimeOffset);
                var converter = ObjectExtensions.GetConverter(targetType);

                Assert.NotNull(converter);
                Assert.False(cache.ContainsKey(targetType));
                Assert.Equal(targetType, lastConverterTargetTypeField.GetValue(null));
                Assert.Same(converter, lastConverterField.GetValue(null));
            }
            finally
            {
                cache.Clear();
                foreach (var entry in previousEntries)
                    cache.TryAdd(entry.Key, entry.Value);

                cacheSizeField.SetValue(null, previousCacheSize);
                lastConverterTargetTypeField.SetValue(null, previousLastType);
                lastConverterField.SetValue(null, previousLastConverter);
            }
        }

        /// <summary>
        /// Verifies that cache fallback returns the existing cached converter when one is available for the target
        /// type.
        /// </summary>
        [Fact]
        public void GetCachedConverterOrCurrent_ReturnsCachedConverter_WhenExists()
        {
            var objectExtensionsType = typeof(ObjectExtensions);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;

            var cacheField = objectExtensionsType.GetField("ConverterCache", flags);
            var method = objectExtensionsType.GetMethod("GetCachedConverterOrCurrent", flags);

            var cache = (ConcurrentDictionary<Type, Func<object, object>>)cacheField.GetValue(null);
            var previousEntries = cache.ToArray();

            try
            {
                cache.Clear();

                Func<object, object> cached = _ => "cached";
                Func<object, object> current = _ => "current";
                cache.TryAdd(typeof(char), cached);

                var result = (Func<object, object>)method.Invoke(
                    null,
                    new object[] { typeof(char), current });

                Assert.Same(cached, result);
            }
            finally
            {
                cache.Clear();
                foreach (var entry in previousEntries)
                    cache.TryAdd(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Verifies that cache fallback returns the current converter when no cached converter exists for the target
        /// type.
        /// </summary>
        [Fact]
        public void GetCachedConverterOrCurrent_ReturnsCurrentConverter_WhenCacheMisses()
        {
            var objectExtensionsType = typeof(ObjectExtensions);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;

            var cacheField = objectExtensionsType.GetField("ConverterCache", flags);
            var method = objectExtensionsType.GetMethod("GetCachedConverterOrCurrent", flags);

            var cache = (ConcurrentDictionary<Type, Func<object, object>>)cacheField.GetValue(null);
            var previousEntries = cache.ToArray();

            try
            {
                cache.Clear();

                Func<object, object> current = _ => "current";

                var result = (Func<object, object>)method.Invoke(
                    null,
                    new object[] { typeof(char), current });

                Assert.Same(current, result);
            }
            finally
            {
                cache.Clear();
                foreach (var entry in previousEntries)
                    cache.TryAdd(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Verifies that cache insertion helper returns the current converter when cache insertion succeeds.
        /// </summary>
        [Fact]
        public void AddConverterOrReuseCached_ReturnsCurrentConverter_WhenTryAddSucceeds()
        {
            var objectExtensionsType = typeof(ObjectExtensions);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;

            var cacheField = objectExtensionsType.GetField("ConverterCache", flags);
            var cacheSizeField = objectExtensionsType.GetField("_converterCacheSize", flags);
            var method = objectExtensionsType.GetMethod("AddConverterOrReuseCached", flags);

            var cache = (ConcurrentDictionary<Type, Func<object, object>>)cacheField.GetValue(null);
            var previousEntries = cache.ToArray();
            var previousCacheSize = (int)cacheSizeField.GetValue(null);

            try
            {
                cache.Clear();
                cacheSizeField.SetValue(null, 1);

                Func<object, object> current = _ => "current";

                var result = (Func<object, object>)method.Invoke(
                    null,
                    new object[] { typeof(char), current });

                Assert.Same(current, result);
                Assert.True(cache.ContainsKey(typeof(char)));
                Assert.Equal(1, (int)cacheSizeField.GetValue(null));
            }
            finally
            {
                cache.Clear();
                foreach (var entry in previousEntries)
                    cache.TryAdd(entry.Key, entry.Value);

                cacheSizeField.SetValue(null, previousCacheSize);
            }
        }

        /// <summary>
        /// Verifies that cache insertion helper rolls back reserved slot and reuses cached converter when insertion
        /// loses a race.
        /// </summary>
        [Fact]
        public void AddConverterOrReuseCached_DecrementsCacheSizeAndReturnsCachedConverter_WhenTryAddFails()
        {
            var objectExtensionsType = typeof(ObjectExtensions);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;

            var cacheField = objectExtensionsType.GetField("ConverterCache", flags);
            var cacheSizeField = objectExtensionsType.GetField("_converterCacheSize", flags);
            var method = objectExtensionsType.GetMethod("AddConverterOrReuseCached", flags);

            var cache = (ConcurrentDictionary<Type, Func<object, object>>)cacheField.GetValue(null);
            var previousEntries = cache.ToArray();
            var previousCacheSize = (int)cacheSizeField.GetValue(null);

            try
            {
                cache.Clear();

                Func<object, object> cached = _ => "cached";
                Func<object, object> current = _ => "current";

                cache.TryAdd(typeof(char), cached);
                cacheSizeField.SetValue(null, 2);

                var result = (Func<object, object>)method.Invoke(
                    null,
                    new object[] { typeof(char), current });

                Assert.Same(cached, result);
                Assert.Equal(1, (int)cacheSizeField.GetValue(null));
            }
            finally
            {
                cache.Clear();
                foreach (var entry in previousEntries)
                    cache.TryAdd(entry.Key, entry.Value);

                cacheSizeField.SetValue(null, previousCacheSize);
            }
        }

        /// <summary>
        /// Verifies that atomic cache-size update succeeds when expected current size matches the actual value.
        /// </summary>
        [Fact]
        public void TryUpdateConverterCacheSize_ReturnsTrue_WhenExpectedSizeMatches()
        {
            var objectExtensionsType = typeof(ObjectExtensions);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;

            var cacheSizeField = objectExtensionsType.GetField("_converterCacheSize", flags);
            var method = objectExtensionsType.GetMethod("TryUpdateConverterCacheSize", flags);

            var previousCacheSize = (int)cacheSizeField.GetValue(null);

            try
            {
                cacheSizeField.SetValue(null, 10);

                var result = (bool)method.Invoke(
                    null,
                    new object[] { 10, 11 });

                Assert.True(result);
                Assert.Equal(11, (int)cacheSizeField.GetValue(null));
            }
            finally
            {
                cacheSizeField.SetValue(null, previousCacheSize);
            }
        }

        /// <summary>
        /// Verifies that atomic cache-size update fails when expected current size does not match the actual value.
        /// </summary>
        [Fact]
        public void TryUpdateConverterCacheSize_ReturnsFalse_WhenExpectedSizeDiffers()
        {
            var objectExtensionsType = typeof(ObjectExtensions);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;

            var cacheSizeField = objectExtensionsType.GetField("_converterCacheSize", flags);
            var method = objectExtensionsType.GetMethod("TryUpdateConverterCacheSize", flags);

            var previousCacheSize = (int)cacheSizeField.GetValue(null);

            try
            {
                cacheSizeField.SetValue(null, 10);

                var result = (bool)method.Invoke(
                    null,
                    new object[] { 9, 10 });

                Assert.False(result);
                Assert.Equal(10, (int)cacheSizeField.GetValue(null));
            }
            finally
            {
                cacheSizeField.SetValue(null, previousCacheSize);
            }
        }

        /// <summary>
        /// Verifies that cache slot reservation core retries when atomic update fails and succeeds on a later attempt.
        /// </summary>
        [Fact]
        public void TryReserveConverterCacheSlotCore_RetriesAfterFailedUpdate_AndThenSucceeds()
        {
            var objectExtensionsType = typeof(ObjectExtensions);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;

            var method = objectExtensionsType.GetMethod("TryReserveConverterCacheSlotCore", flags);

            Assert.NotNull(method);

            var reads = new Queue<int>(new[] { 0, 0 });
            Func<int> readCacheSize = () => reads.Dequeue();

            var updateCalls = 0;
            Func<int, int, bool> tryUpdate = (current, updated) =>
            {
                updateCalls++;
                return updateCalls == 2;
            };

            var result = (bool)method.Invoke(
                null,
                new object[] { readCacheSize, tryUpdate });

            Assert.True(result);
            Assert.Equal(2, updateCalls);
        }

        /// <summary>
        /// Verifies that cache slot reservation core stops when cache size has reached the configured maximum.
        /// </summary>
        [Fact]
        public void TryReserveConverterCacheSlotCore_ReturnsFalse_WhenCacheIsFull()
        {
            var objectExtensionsType = typeof(ObjectExtensions);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;

            var method = objectExtensionsType.GetMethod("TryReserveConverterCacheSlotCore", flags);
            var maxCacheSizeField = objectExtensionsType.GetField("MaxConverterCacheSize", flags);

            Assert.NotNull(method);
            Assert.NotNull(maxCacheSizeField);

            var maxCacheSize = (int)maxCacheSizeField.GetRawConstantValue();

            Func<int> readCacheSize = () => maxCacheSize;

            var updateCalls = 0;
            Func<int, int, bool> tryUpdate = (current, updated) =>
            {
                updateCalls++;
                return true;
            };

            var result = (bool)method.Invoke(
                null,
                new object[] { readCacheSize, tryUpdate });

            Assert.False(result);
            Assert.Equal(0, updateCalls);
        }

        /// <summary>
        /// Verifies that GetConverter returns an already cached converter when cache insertion is skipped and the
        /// target type is already present in the dictionary.
        /// </summary>
        [Fact]
        public void GetConverter_UsesExistingCachedConverter_WhenCacheLimitReachedAndTypeAlreadyCached()
        {
            var objectExtensionsType = typeof(ObjectExtensions);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;

            var cacheField = objectExtensionsType.GetField("ConverterCache", flags);
            var cacheSizeField = objectExtensionsType.GetField("_converterCacheSize", flags);
            var maxCacheSizeField = objectExtensionsType.GetField("MaxConverterCacheSize", flags);
            var lastConverterTargetTypeField = objectExtensionsType.GetField("_lastConverterTargetType", flags);
            var lastConverterField = objectExtensionsType.GetField("_lastConverter", flags);

            var cache = (ConcurrentDictionary<Type, Func<object, object>>)cacheField.GetValue(null);
            var previousEntries = cache.ToArray();
            var previousCacheSize = (int)cacheSizeField.GetValue(null);
            var previousLastType = lastConverterTargetTypeField.GetValue(null);
            var previousLastConverter = lastConverterField.GetValue(null);

            try
            {
                cache.Clear();
                lastConverterTargetTypeField.SetValue(null, null);
                lastConverterField.SetValue(null, null);

                var maxCacheSize = (int)maxCacheSizeField.GetValue(null);
                cacheSizeField.SetValue(null, maxCacheSize);

                var existingConverter = new Func<object, object>(_ => "from-cache");
                cache.TryAdd(typeof(char), existingConverter);

                var converter = ObjectExtensions.GetConverter(typeof(char));

                Assert.Same(existingConverter, converter);
                Assert.Equal("from-cache", converter("ignored"));
            }
            finally
            {
                cache.Clear();
                foreach (var entry in previousEntries)
                    cache.TryAdd(entry.Key, entry.Value);

                cacheSizeField.SetValue(null, previousCacheSize);
                lastConverterTargetTypeField.SetValue(null, previousLastType);
                lastConverterField.SetValue(null, previousLastConverter);
            }
        }

        /// <summary>
        /// Verifies that the BuildConverter method returns the same enum instance when the input value already matches
        /// the target enum type.
        /// </summary>
        /// <remarks>This test ensures that the converter delegate produced by BuildConverter does not
        /// create a new enum instance if the input is already of the correct enum type. This behavior is important for
        /// preserving reference equality and avoiding unnecessary allocations when converting enum values.</remarks>
        [Fact]
        public void BuildConverter_EnumDelegate_ReturnsSameEnumInstance_WhenValueAlreadyMatchesTargetEnumType()
        {
            var method = typeof(ObjectExtensions).GetMethod(
                "BuildConverter",
                BindingFlags.NonPublic | BindingFlags.Static);

            var converter = (Func<object, object>)method.Invoke(null, new object[] { typeof(TestStatus) });
            var result = converter(TestStatus.Active);

            Assert.Equal(TestStatus.Active, result);
        }

        /// <summary>
        /// Verifies that the GetConverter method throws an ArgumentNullException when the target type parameter is
        /// null.
        /// </summary>
        /// <remarks>This test ensures that ObjectExtensions.GetConverter enforces its contract by
        /// validating input parameters and throwing the appropriate exception when a null target type is
        /// provided.</remarks>
        [Fact]
        public void GetConverter_ThrowsArgumentNullException_WhenTargetTypeIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectExtensions.GetConverter(null));
        }
    }
}
