using Hydrix.Caching.Entries;
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
    /// Defines a non-parallelized test collection for tests that mutate the shared ObjectExtensions converter caches.
    /// </summary>
    [CollectionDefinition("ObjectExtensionsSequential", DisableParallelization = true)]
    public class ObjectExtensionsSequentialCollection
    { }

    /// <summary>
    /// Contains unit tests for the ObjectExtensions class, verifying the behavior of the As&lt;T&gt;() extension method under
    /// various input scenarios.
    /// </summary>
    /// <remarks>These tests ensure that the As&lt;T&gt;() method correctly handles null values, DBNull, convertible
    /// types, and non-convertible types, returning default values or throwing exceptions as appropriate. The tests help
    /// validate the robustness and correctness of type conversion logic implemented in ObjectExtensions.</remarks>
    [Collection("ObjectExtensionsSequential")]
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
        public void As_ThrowsFormatException_WhenGuidTargetAndSourceIsUnsupported()
        {
            object value = 10;

            Assert.Throws<FormatException>(() => value.As<Guid>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; throws when target is nullable Guid and source is neither Guid nor string.
        /// </summary>
        [Fact]
        public void As_ThrowsFormatException_WhenNullableGuidTargetAndSourceIsUnsupported()
        {
            object value = 10;

            Assert.Throws<FormatException>(() => value.As<Guid?>());
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
        /// Verifies that the DateTimeOffset converter delegate executes the direct DateTimeOffset switch arm.
        /// </summary>
        [Fact]
        public void BuildConverter_DateTimeOffsetDelegate_ReturnsDirectDateTimeOffset_WhenValueIsDateTimeOffset()
        {
            var method = typeof(ObjectExtensions).GetMethod(
                "BuildConverter",
                BindingFlags.NonPublic | BindingFlags.Static);

            var converter = (Func<object, object>)method.Invoke(null, new object[] { typeof(DateTimeOffset) });
            var value = new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.Zero);

            var result = converter(value);

            Assert.Equal(value, result);
        }

        /// <summary>
        /// Verifies that the TimeSpan converter delegate executes the direct TimeSpan switch arm.
        /// </summary>
        [Fact]
        public void BuildConverter_TimeSpanDelegate_ReturnsDirectTimeSpan_WhenValueIsTimeSpan()
        {
            var method = typeof(ObjectExtensions).GetMethod(
                "BuildConverter",
                BindingFlags.NonPublic | BindingFlags.Static);

            var converter = (Func<object, object>)method.Invoke(null, new object[] { typeof(TimeSpan) });
            var value = TimeSpan.FromMinutes(2);

            var result = converter(value);

            Assert.Equal(value, result);
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
            var lastConverterCacheField = objectExtensionsType.GetField("_lastConverterCache", flags);
            var buildConverterMethod = objectExtensionsType.GetMethod("BuildConverter", flags);

            var cache = (ConcurrentDictionary<Type, Func<object, object>>)cacheField.GetValue(null);
            var previousEntries = cache.ToArray();
            var previousCacheSize = (int)cacheSizeField.GetValue(null);
            var previousLastCache = lastConverterCacheField.GetValue(null);

            try
            {
                cache.Clear();
                cacheSizeField.SetValue(null, 0);
                lastConverterCacheField.SetValue(null, null);

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
                lastConverterCacheField.SetValue(null, previousLastCache);
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
            var lastConverterCacheField = objectExtensionsType.GetField("_lastConverterCache", flags);

            var cache = (ConcurrentDictionary<Type, Func<object, object>>)cacheField.GetValue(null);
            var previousEntries = cache.ToArray();
            var previousCacheSize = (int)cacheSizeField.GetValue(null);
            var previousLastCache = lastConverterCacheField.GetValue(null);

            try
            {
                cache.Clear();
                lastConverterCacheField.SetValue(null, null);

                var maxCacheSize = (int)maxCacheSizeField.GetValue(null);
                cacheSizeField.SetValue(null, maxCacheSize);

                var targetType = typeof(DateTimeOffset);
                var converter = ObjectExtensions.GetConverter(targetType);

                Assert.NotNull(converter);
                Assert.False(cache.ContainsKey(targetType));
                Assert.Equal(targetType, ReadConverterHotCacheTargetType(lastConverterCacheField.GetValue(null)));
                Assert.Same(converter, ReadConverterHotCacheConverter(lastConverterCacheField.GetValue(null)));
            }
            finally
            {
                cache.Clear();
                foreach (var entry in previousEntries)
                    cache.TryAdd(entry.Key, entry.Value);

                cacheSizeField.SetValue(null, previousCacheSize);
                lastConverterCacheField.SetValue(null, previousLastCache);
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
            var lastConverterCacheField = objectExtensionsType.GetField("_lastConverterCache", flags);

            var cache = (ConcurrentDictionary<Type, Func<object, object>>)cacheField.GetValue(null);
            var previousEntries = cache.ToArray();
            var previousCacheSize = (int)cacheSizeField.GetValue(null);
            var previousLastCache = lastConverterCacheField.GetValue(null);

            try
            {
                cache.Clear();
                lastConverterCacheField.SetValue(null, null);

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
                lastConverterCacheField.SetValue(null, previousLastCache);
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
        /// Verifies that As&lt;T&gt; converts floating-point and decimal values to boolean using numeric zero checks.
        /// </summary>
        [Fact]
        public void As_ConvertsFloatingPointAndDecimalValues_ToBoolean()
        {
            object floatNonZero = 0.1f;
            object floatZero = 0f;
            object doubleNonZero = 0.1d;
            object doubleZero = 0d;
            object decimalNonZero = 1m;
            object decimalZero = 0m;

            Assert.True(floatNonZero.As<bool>());
            Assert.False(floatZero.As<bool>());
            Assert.True(doubleNonZero.As<bool>());
            Assert.False(doubleZero.As<bool>());
            Assert.True(decimalNonZero.As<bool>());
            Assert.False(decimalZero.As<bool>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts string aliases to boolean values through ParseBooleanFromString.
        /// </summary>
        [Fact]
        public void As_ConvertsStringAliases_ToBoolean()
        {
            Assert.True("1".As<bool>());
            Assert.False("0".As<bool>());
            Assert.True("yes".As<bool>());
            Assert.False("NO".As<bool>());
            Assert.True("On".As<bool>());
            Assert.False("off".As<bool>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; throws for unsupported boolean string values.
        /// </summary>
        [Fact]
        public void As_ThrowsFormatException_WhenBooleanStringIsInvalid()
        {
            Assert.Throws<FormatException>(() => "not-bool".As<bool>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts all supported DateTimeOffset source variants.
        /// </summary>
        [Fact]
        public void As_ConvertsDateTimeOffset_FromAllSupportedSources()
        {
            var dto = new DateTimeOffset(2025, 2, 3, 4, 5, 6, TimeSpan.Zero);
            var dt = dto.UtcDateTime;
            object fromDto = dto;
            object fromDateTime = dt;
            object fromString = dto.ToString("o");
            object fromTicks = dt.Ticks;
            object fromFallbackToString = new ToStringWrapper(dto.ToString("o"));

            Assert.Equal(dto, fromDto.As<DateTimeOffset>());
            Assert.Equal(new DateTimeOffset(dt), fromDateTime.As<DateTimeOffset>());
            Assert.Equal(dto, fromString.As<DateTimeOffset>());
            Assert.Equal(new DateTimeOffset(new DateTime(dt.Ticks, DateTimeKind.Utc)), fromTicks.As<DateTimeOffset>());
            Assert.Equal(dto, fromFallbackToString.As<DateTimeOffset>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts all supported TimeSpan source variants.
        /// </summary>
        [Fact]
        public void As_ConvertsTimeSpan_FromAllSupportedSources()
        {
            var ts = TimeSpan.FromSeconds(90);
            object fromTimeSpan = ts;
            object fromTicks = ts.Ticks;
            object fromMilliseconds = 1500;
            object fromString = "00:00:02";
            object fromFallbackToString = new ToStringWrapper("00:00:03");

            Assert.Equal(ts, fromTimeSpan.As<TimeSpan>());
            Assert.Equal(new TimeSpan(ts.Ticks), fromTicks.As<TimeSpan>());
            Assert.Equal(TimeSpan.FromMilliseconds(1500), fromMilliseconds.As<TimeSpan>());
            Assert.Equal(TimeSpan.Parse("00:00:02"), fromString.As<TimeSpan>());
            Assert.Equal(TimeSpan.Parse("00:00:03"), fromFallbackToString.As<TimeSpan>());
        }

        /// <summary>
        /// Verifies that As&lt;T&gt; converts string values to remaining supported numeric target types.
        /// </summary>
        [Fact]
        public void As_ConvertsString_ToRemainingSupportedNumericTypes()
        {
            Assert.Equal((sbyte)-8, "-8".As<sbyte>());
            Assert.Equal((ushort)12, "12".As<ushort>());
            Assert.Equal((uint)13, "13".As<uint>());
            Assert.Equal((ulong)14, "14".As<ulong>());
            Assert.Equal('A', "A".As<char>());
        }

        /// <summary>
        /// Verifies that BuildConverter fallback returns the same object when source already matches target type.
        /// </summary>
        [Fact]
        public void BuildConverter_FallbackPath_ReturnsSameObject_WhenSourceAlreadyMatchesTargetType()
        {
            var method = typeof(ObjectExtensions).GetMethod(
                "BuildConverter",
                BindingFlags.NonPublic | BindingFlags.Static);

            var converter = (Func<object, object>)method.Invoke(null, new object[] { typeof(Version) });
            var value = new Version(1, 2, 3, 4);

            var result = converter(value);

            Assert.Same(value, result);
        }

        /// <summary>
        /// Verifies that BuildConverter fallback populates PairConverterCache and reuses cached pair converters.
        /// </summary>
        [Fact]
        public void BuildConverter_FallbackPath_UsesPairConverterCache_OnSubsequentCalls()
        {
            var objectExtensionsType = typeof(ObjectExtensions);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            var pairCacheField = objectExtensionsType.GetField("PairConverterCache", flags);
            var buildConverterMethod = objectExtensionsType.GetMethod("BuildConverter", flags);

            var pairCache = (ConcurrentDictionary<(Type Source, Type Target), Func<object, object>>)pairCacheField.GetValue(null);
            var previousEntries = pairCache.ToArray();

            try
            {
                pairCache.Clear();

                var converter = (Func<object, object>)buildConverterMethod.Invoke(null, new object[] { typeof(Version) });

                Assert.Throws<InvalidCastException>(() => converter("1.2.3.4"));
                Assert.True(pairCache.ContainsKey((typeof(string), typeof(Version))));

                Assert.Throws<InvalidCastException>(() => converter("5.6.7.8"));
                Assert.True(pairCache.ContainsKey((typeof(string), typeof(Version))));
            }
            finally
            {
                pairCache.Clear();
                foreach (var entry in previousEntries)
                    pairCache.TryAdd(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Verifies that BuildConverter fallback does not cache pair converters for runtime source types outside the known fast-path set.
        /// </summary>
        [Fact]
        public void BuildConverter_FallbackPath_DoesNotCacheUnknownRuntimeSourceType()
        {
            var objectExtensionsType = typeof(ObjectExtensions);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            var pairCacheField = objectExtensionsType.GetField("PairConverterCache", flags);
            var buildConverterMethod = objectExtensionsType.GetMethod("BuildConverter", flags);

            var pairCache = (ConcurrentDictionary<(Type Source, Type Target), Func<object, object>>)pairCacheField.GetValue(null);
            var previousEntries = pairCache.ToArray();

            try
            {
                pairCache.Clear();

                var converter = (Func<object, object>)buildConverterMethod.Invoke(null, new object[] { typeof(Version) });

                Assert.Throws<InvalidCastException>(() => converter(new ToStringWrapper("1.2.3.4")));
                Assert.False(pairCache.ContainsKey((typeof(ToStringWrapper), typeof(Version))));
            }
            finally
            {
                pairCache.Clear();
                foreach (var entry in previousEntries)
                    pairCache.TryAdd(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Verifies that TryGetKnownPairConverter resolves converters for every runtime source type supported by the fast-path cache.
        /// </summary>
        [Fact]
        public void TryGetKnownPairConverter_ReturnsConverters_ForAllSupportedRuntimeTypes()
        {
            var method = typeof(ObjectExtensions).GetMethod(
                "TryGetKnownPairConverter",
                BindingFlags.NonPublic | BindingFlags.Static);

            var cases = new[]
            {
                new { Value = (object)1, TargetType = typeof(long), Expected = (object)1L },
                new { Value = (object)2L, TargetType = typeof(decimal), Expected = (object)2m },
                new { Value = (object)3m, TargetType = typeof(double), Expected = (object)3d },
                new { Value = (object)4d, TargetType = typeof(float), Expected = (object)4f },
                new { Value = (object)5f, TargetType = typeof(decimal), Expected = (object)5m },
                new { Value = (object)(short)6, TargetType = typeof(long), Expected = (object)6L },
                new { Value = (object)(byte)7, TargetType = typeof(double), Expected = (object)7d },
                new { Value = (object)'H', TargetType = typeof(string), Expected = (object)"H" },
                new { Value = (object)"I", TargetType = typeof(char), Expected = (object)'I' }
            };

            foreach (var testCase in cases)
            {
                var arguments = new object[] { testCase.Value, testCase.TargetType, null };
                var resolved = (bool)method.Invoke(
                    null,
                    arguments);

                Assert.True(resolved);

                var converter = Assert.IsType<Func<object, object>>(arguments[2]);
                Assert.Equal(
                    testCase.Expected,
                    converter(testCase.Value));
            }
        }

        /// <summary>
        /// Verifies that BuildPairConverter uses the generic ChangeType fallback when source type has no registered builder.
        /// </summary>
        [Fact]
        public void BuildPairConverter_UsesChangeTypeFallback_WhenSourceTypeHasNoRegisteredBuilder()
        {
            var method = typeof(ObjectExtensions).GetMethod(
                "BuildPairConverter",
                BindingFlags.NonPublic | BindingFlags.Static);

            var converter = (Func<object, object>)method.Invoke(null, new object[] { typeof(bool), typeof(int) });
            var result = converter(true);

            Assert.Equal(1, result);
        }

        /// <summary>
        /// Verifies that pair-converter builders execute both dictionary hit and fallback branches.
        /// </summary>
        [Fact]
        public void BuildPairConverterBuilders_ExecuteHitAndFallbackBranches()
        {
            AssertPairBuilderHitAndFallback("BuildPairConverterFromInt", 7, typeof(long), 7L, typeof(bool), true);
            AssertPairBuilderHitAndFallback("BuildPairConverterFromLong", 7L, typeof(int), 7, typeof(bool), true);
            AssertPairBuilderHitAndFallback("BuildPairConverterFromDecimal", 7m, typeof(int), 7, typeof(bool), true);
            AssertPairBuilderHitAndFallback("BuildPairConverterFromDouble", 7d, typeof(int), 7, typeof(bool), true);
            AssertPairBuilderHitAndFallback("BuildPairConverterFromFloat", 7f, typeof(int), 7, typeof(bool), true);
            AssertPairBuilderHitAndFallback("BuildPairConverterFromShort", (short)7, typeof(int), 7, typeof(bool), true);
            AssertPairBuilderHitAndFallback("BuildPairConverterFromByte", (byte)7, typeof(int), 7, typeof(bool), true);
            AssertPairBuilderHitAndFallback("BuildPairConverterFromString", "Z", typeof(char), 'Z', typeof(int), 12, fallbackInput: "12");

            var charBuilder = typeof(ObjectExtensions).GetMethod(
                "BuildPairConverterFromChar",
                BindingFlags.NonPublic | BindingFlags.Static);
            var charHit = (Func<object, object>)charBuilder.Invoke(null, new object[] { typeof(string) });
            Assert.Equal("A", charHit('A'));

            var charFallback = (Func<object, object>)charBuilder.Invoke(null, new object[] { typeof(double) });
            Assert.Throws<InvalidCastException>(() => charFallback('A'));
        }

        /// <summary>
        /// Verifies that <c>BuildPairConverterFromInt</c> covers every dictionary-mapped cast branch.
        /// </summary>
        [Fact]
        public void BuildPairConverterFromInt_CoversAllMappedTargets()
        {
            const int value = 65;

            Assert.Equal((short)65, InvokePairBuilder("BuildPairConverterFromInt", typeof(short), value));
            Assert.Equal((byte)65, InvokePairBuilder("BuildPairConverterFromInt", typeof(byte), value));
            Assert.Equal((sbyte)65, InvokePairBuilder("BuildPairConverterFromInt", typeof(sbyte), value));
            Assert.Equal((ushort)65, InvokePairBuilder("BuildPairConverterFromInt", typeof(ushort), value));
            Assert.Equal((uint)65, InvokePairBuilder("BuildPairConverterFromInt", typeof(uint), value));
            Assert.Equal((ulong)65, InvokePairBuilder("BuildPairConverterFromInt", typeof(ulong), value));
            Assert.Equal(65m, InvokePairBuilder("BuildPairConverterFromInt", typeof(decimal), value));
            Assert.Equal(65d, InvokePairBuilder("BuildPairConverterFromInt", typeof(double), value));
            Assert.Equal(65f, InvokePairBuilder("BuildPairConverterFromInt", typeof(float), value));
            Assert.Equal('A', InvokePairBuilder("BuildPairConverterFromInt", typeof(char), value));
        }

        /// <summary>
        /// Verifies that <c>BuildPairConverterFromLong</c> covers every dictionary-mapped cast branch.
        /// </summary>
        [Fact]
        public void BuildPairConverterFromLong_CoversAllMappedTargets()
        {
            const long value = 66L;

            Assert.Equal((short)66, InvokePairBuilder("BuildPairConverterFromLong", typeof(short), value));
            Assert.Equal((byte)66, InvokePairBuilder("BuildPairConverterFromLong", typeof(byte), value));
            Assert.Equal((sbyte)66, InvokePairBuilder("BuildPairConverterFromLong", typeof(sbyte), value));
            Assert.Equal((ushort)66, InvokePairBuilder("BuildPairConverterFromLong", typeof(ushort), value));
            Assert.Equal((uint)66, InvokePairBuilder("BuildPairConverterFromLong", typeof(uint), value));
            Assert.Equal((ulong)66, InvokePairBuilder("BuildPairConverterFromLong", typeof(ulong), value));
            Assert.Equal(66m, InvokePairBuilder("BuildPairConverterFromLong", typeof(decimal), value));
            Assert.Equal(66d, InvokePairBuilder("BuildPairConverterFromLong", typeof(double), value));
            Assert.Equal(66f, InvokePairBuilder("BuildPairConverterFromLong", typeof(float), value));
        }

        /// <summary>
        /// Verifies that decimal, double, float, short, and byte pair builders cover every dictionary-mapped cast branch.
        /// </summary>
        [Fact]
        public void BuildPairConverters_NumericBuilders_CoverAllMappedTargets()
        {
            Assert.Equal(67L, InvokePairBuilder("BuildPairConverterFromDecimal", typeof(long), 67m));
            Assert.Equal((short)67, InvokePairBuilder("BuildPairConverterFromDecimal", typeof(short), 67m));
            Assert.Equal((byte)67, InvokePairBuilder("BuildPairConverterFromDecimal", typeof(byte), 67m));
            Assert.Equal(67d, InvokePairBuilder("BuildPairConverterFromDecimal", typeof(double), 67m));
            Assert.Equal(67f, InvokePairBuilder("BuildPairConverterFromDecimal", typeof(float), 67m));

            Assert.Equal(68m, InvokePairBuilder("BuildPairConverterFromDouble", typeof(decimal), 68d));
            Assert.Equal(68f, InvokePairBuilder("BuildPairConverterFromDouble", typeof(float), 68d));
            Assert.Equal(68L, InvokePairBuilder("BuildPairConverterFromDouble", typeof(long), 68d));
            Assert.Equal((short)68, InvokePairBuilder("BuildPairConverterFromDouble", typeof(short), 68d));
            Assert.Equal((byte)68, InvokePairBuilder("BuildPairConverterFromDouble", typeof(byte), 68d));

            Assert.Equal(69d, InvokePairBuilder("BuildPairConverterFromFloat", typeof(double), 69f));
            Assert.Equal(69m, InvokePairBuilder("BuildPairConverterFromFloat", typeof(decimal), 69f));
            Assert.Equal(69L, InvokePairBuilder("BuildPairConverterFromFloat", typeof(long), 69f));
            Assert.Equal((short)69, InvokePairBuilder("BuildPairConverterFromFloat", typeof(short), 69f));
            Assert.Equal((byte)69, InvokePairBuilder("BuildPairConverterFromFloat", typeof(byte), 69f));

            Assert.Equal(70L, InvokePairBuilder("BuildPairConverterFromShort", typeof(long), (short)70));
            Assert.Equal((byte)70, InvokePairBuilder("BuildPairConverterFromShort", typeof(byte), (short)70));
            Assert.Equal((sbyte)70, InvokePairBuilder("BuildPairConverterFromShort", typeof(sbyte), (short)70));
            Assert.Equal(70m, InvokePairBuilder("BuildPairConverterFromShort", typeof(decimal), (short)70));
            Assert.Equal(70d, InvokePairBuilder("BuildPairConverterFromShort", typeof(double), (short)70));
            Assert.Equal(70f, InvokePairBuilder("BuildPairConverterFromShort", typeof(float), (short)70));

            Assert.Equal(71L, InvokePairBuilder("BuildPairConverterFromByte", typeof(long), (byte)71));
            Assert.Equal((short)71, InvokePairBuilder("BuildPairConverterFromByte", typeof(short), (byte)71));
            Assert.Equal(71m, InvokePairBuilder("BuildPairConverterFromByte", typeof(decimal), (byte)71));
            Assert.Equal(71d, InvokePairBuilder("BuildPairConverterFromByte", typeof(double), (byte)71));
            Assert.Equal(71f, InvokePairBuilder("BuildPairConverterFromByte", typeof(float), (byte)71));
        }

        /// <summary>
        /// Verifies that char and string pair builders execute all mapped branches, including empty-string failure.
        /// </summary>
        [Fact]
        public void BuildPairConverters_CharAndStringBuilders_CoverAllMappedTargetsAndThrowOnEmptyString()
        {
            Assert.Equal(65, InvokePairBuilder("BuildPairConverterFromChar", typeof(int), 'A'));
            Assert.Equal("A", InvokePairBuilder("BuildPairConverterFromChar", typeof(string), 'A'));

            var toChar = GetPairBuilderConverter("BuildPairConverterFromString", typeof(char));
            Assert.Equal('Z', toChar("Z"));
            Assert.Throws<InvalidCastException>(() => toChar(string.Empty));
        }

        /// <summary>
        /// Verifies that converting an empty string to char throws through the string pair-converter path.
        /// </summary>
        [Fact]
        public void As_ThrowsFormatException_WhenConvertingEmptyStringToChar()
        {
            Assert.Throws<FormatException>(() => "".As<char>());
        }

        /// <summary>
        /// Verifies that GetConverter uses and returns the process-wide hot cache converter when type matches.
        /// </summary>
        [Fact]
        public void GetConverter_ReturnsHotCachedConverter_WhenLastTypeMatches()
        {
            var objectExtensionsType = typeof(ObjectExtensions);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            var lastConverterCacheField = objectExtensionsType.GetField("_lastConverterCache", flags);

            var previousLastCache = lastConverterCacheField.GetValue(null);

            try
            {
                Func<object, object> hotConverter = _ => "hot";
                lastConverterCacheField.SetValue(null, CreateConverterCacheEntry(typeof(Guid), hotConverter));

                var converter = ObjectExtensions.GetConverter(typeof(Guid));

                Assert.Same(hotConverter, converter);
                Assert.Equal("hot", converter(Guid.NewGuid()));
            }
            finally
            {
                lastConverterCacheField.SetValue(null, previousLastCache);
            }
        }

        /// <summary>
        /// Verifies that GetConverter stores converters in cache when below the configured cache size limit.
        /// </summary>
        [Fact]
        public void GetConverter_AddsConverterToCache_WhenCacheHasCapacity()
        {
            var objectExtensionsType = typeof(ObjectExtensions);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            var cacheField = objectExtensionsType.GetField("ConverterCache", flags);
            var cacheSizeField = objectExtensionsType.GetField("_converterCacheSize", flags);
            var lastConverterCacheField = objectExtensionsType.GetField("_lastConverterCache", flags);

            var cache = (ConcurrentDictionary<Type, Func<object, object>>)cacheField.GetValue(null);
            var previousEntries = cache.ToArray();
            var previousCacheSize = (int)cacheSizeField.GetValue(null);
            var previousLastCache = lastConverterCacheField.GetValue(null);

            try
            {
                cache.Clear();
                cacheSizeField.SetValue(null, 0);
                lastConverterCacheField.SetValue(null, null);

                var converter = ObjectExtensions.GetConverter(typeof(DateTimeOffset));

                Assert.NotNull(converter);
                Assert.True(cache.ContainsKey(typeof(DateTimeOffset)));
                Assert.Equal(1, (int)cacheSizeField.GetValue(null));
            }
            finally
            {
                cache.Clear();
                foreach (var entry in previousEntries)
                    cache.TryAdd(entry.Key, entry.Value);

                cacheSizeField.SetValue(null, previousCacheSize);
                lastConverterCacheField.SetValue(null, previousLastCache);
            }
        }

        /// <summary>
        /// Verifies that nullable DateTimeOffset conversion executes the direct DateTimeOffset switch arm.
        /// </summary>
        [Fact]
        public void As_ConvertsDirectDateTimeOffset_ToNullableDateTimeOffset_ThroughConverterSwitchArm()
        {
            var value = new DateTimeOffset(2025, 2, 3, 4, 5, 6, TimeSpan.Zero);
            object source = value;

            var converted = source.As<DateTimeOffset?>();

            Assert.True(converted.HasValue);
            Assert.Equal(value, converted.Value);
        }

        /// <summary>
        /// Verifies that nullable TimeSpan conversion executes the direct TimeSpan switch arm.
        /// </summary>
        [Fact]
        public void As_ConvertsDirectTimeSpan_ToNullableTimeSpan_ThroughConverterSwitchArm()
        {
            var value = TimeSpan.FromSeconds(42);
            object source = value;

            var converted = source.As<TimeSpan?>();

            Assert.True(converted.HasValue);
            Assert.Equal(value, converted.Value);
        }

        /// <summary>
        /// Verifies that boolean conversion reaches the default switch branch for unsupported runtime types.
        /// </summary>
        [Fact]
        public void As_BooleanConversion_UsesDefaultSwitchBranch_ForUnsupportedRuntimeType()
        {
            object source = DateTime.UtcNow;

            Assert.Throws<InvalidCastException>(() => source.As<bool>());
        }

        /// <summary>
        /// Represents a simple wrapper whose string representation can be controlled in tests.
        /// </summary>
        private sealed class ToStringWrapper
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ToStringWrapper"/> class.
            /// </summary>
            /// <param name="text">The string value returned by <see cref="ToString"/>.</param>
            public ToStringWrapper(string text)
            {
                Text = text;
            }

            /// <summary>
            /// Gets the text returned by <see cref="ToString"/>.
            /// </summary>
            public string Text { get; }

            /// <summary>
            /// Returns the configured text value.
            /// </summary>
            /// <returns>The configured text.</returns>
            public override string ToString()
                => Text;
        }

        /// <summary>
        /// Invokes a pair-converter builder method and validates both dictionary hit and fallback behavior.
        /// </summary>
        /// <param name="methodName">The private static builder method name.</param>
        /// <param name="hitInput">Input value used for the dictionary-hit branch.</param>
        /// <param name="hitTargetType">Target type that exists in the builder dictionary.</param>
        /// <param name="expectedHit">Expected conversion result for the dictionary-hit branch.</param>
        /// <param name="fallbackTargetType">Target type that forces Convert.ChangeType fallback branch.</param>
        /// <param name="expectedFallback">Expected conversion result for the fallback branch.</param>
        /// <param name="fallbackInput">Optional fallback input; when null, <paramref name="hitInput"/> is used.</param>
        private static void AssertPairBuilderHitAndFallback(
            string methodName,
            object hitInput,
            Type hitTargetType,
            object expectedHit,
            Type fallbackTargetType,
            object expectedFallback,
            object fallbackInput = null)
        {
            var method = typeof(ObjectExtensions).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Static);

            var hitConverter = (Func<object, object>)method.Invoke(null, new object[] { hitTargetType });
            var hitResult = hitConverter(hitInput);

            Assert.Equal(expectedHit, hitResult);

            var fallbackConverter = (Func<object, object>)method.Invoke(null, new object[] { fallbackTargetType });
            var fallbackResult = fallbackConverter(fallbackInput ?? hitInput);

            Assert.Equal(expectedFallback, fallbackResult);
        }

        /// <summary>
        /// Resolves and executes a private pair-builder converter method for a given target type and input value.
        /// </summary>
        /// <param name="methodName">Name of the private static pair-builder method.</param>
        /// <param name="targetType">Target type requested from the pair-builder.</param>
        /// <param name="input">Input value passed to the resulting converter delegate.</param>
        /// <returns>The converted value returned by the resolved converter delegate.</returns>
        private static object InvokePairBuilder(
            string methodName,
            Type targetType,
            object input)
            => GetPairBuilderConverter(
                methodName,
                targetType)(input);

        /// <summary>
        /// Resolves a private pair-builder method and returns the converter delegate for the provided target type.
        /// </summary>
        /// <param name="methodName">Name of the private static pair-builder method.</param>
        /// <param name="targetType">Target type passed to the pair-builder.</param>
        /// <returns>A converter delegate produced by the specified pair-builder method.</returns>
        private static Func<object, object> GetPairBuilderConverter(
            string methodName,
            Type targetType)
        {
            var method = typeof(ObjectExtensions).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Static);

            return (Func<object, object>)method.Invoke(
                null,
                new object[] { targetType });
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

        /// <summary>
        /// Creates an instance of the converter hot-cache entry used by <see cref="ObjectExtensions"/>.
        /// </summary>
        /// <param name="targetType">The target type associated with the cached converter.</param>
        /// <param name="converter">The converter delegate to store in the hot-cache entry.</param>
        /// <returns>An object instance representing the converter hot-cache entry.</returns>
        private static object CreateConverterCacheEntry(
            Type targetType,
            Func<object, object> converter)
            => new ConverterCacheEntry(
                targetType,
                converter);

        /// <summary>
        /// Reads the cached target type from a converter hot-cache entry.
        /// </summary>
        /// <param name="cacheEntry">The hot-cache entry instance to inspect. May be null.</param>
        /// <returns>The cached target type, or null when <paramref name="cacheEntry"/> is null.</returns>
        private static Type ReadConverterHotCacheTargetType(
            object cacheEntry)
            => cacheEntry == null
                ? null
                : ((ConverterCacheEntry)cacheEntry).TargetType;

        /// <summary>
        /// Reads the cached converter delegate from a converter hot-cache entry.
        /// </summary>
        /// <param name="cacheEntry">The hot-cache entry instance to inspect. May be null.</param>
        /// <returns>The cached converter delegate, or null when <paramref name="cacheEntry"/> is null.</returns>
        private static Func<object, object> ReadConverterHotCacheConverter(
            object cacheEntry)
            => cacheEntry == null
                ? null
                : ((ConverterCacheEntry)cacheEntry).Converter;
    }
}
