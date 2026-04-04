using Hydrix.Caching.Entries;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Hydrix.Extensions
{
    /// <summary>
    /// Provides extension methods for performing flexible type conversions on objects.
    /// </summary>
    /// <remarks>The methods in this class enable fluent and readable type conversion operations on objects,
    /// allowing for more concise and expressive code when working with dynamic or loosely-typed data.</remarks>
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Holds the most recently used converter cache entry in the process-wide hot cache.
        /// </summary>
        /// <remarks>This field stores the target-type/converter pair as a single immutable object so volatile reads
        /// and writes remain atomically consistent under concurrent access.</remarks>
        private static ConverterCacheEntry _lastConverterCache;


        /// <summary>
        /// Tracks the number of cached converters without calling ConcurrentDictionary.Count on the hot path.
        /// </summary>
        private static int _converterCacheSize;

        /// <summary>
        /// Caches conversion delegates by target type to reduce repeated branch evaluation in hot paths.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Func<object, object>> ConverterCache =
            new ConcurrentDictionary<Type, Func<object, object>>();

        /// <summary>
        /// Caches typed conversion delegates by (sourceType, targetType) pair.
        /// </summary>
        /// <remarks>Eliminates <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> overhead on the
        /// fallback path after the first encounter of each pair. Populated lazily; bounded only by the number of
        /// distinct type pairs observed at runtime, which is typically very small in practice.</remarks>
        private static readonly ConcurrentDictionary<(Type Source, Type Target), Func<object, object>> PairConverterCache =
            new ConcurrentDictionary<(Type, Type), Func<object, object>>();

        /// <summary>
        /// Provides a mapping of numeric types to functions that convert an object to the corresponding numeric type
        /// using invariant culture settings.
        /// </summary>
        /// <remarks>This dictionary enables efficient conversion of objects to various numeric types by
        /// associating each supported type with a conversion function. The conversions use CultureInfo.InvariantCulture
        /// to ensure consistent parsing of numeric values regardless of the current culture.</remarks>
        private static readonly Dictionary<Type, Func<object, object>> NumberConverters =
            new Dictionary<Type, Func<object, object>>()
            {
                [typeof(int)] = value => Convert.ToInt32(value, CultureInfo.InvariantCulture),
                [typeof(long)] = value => Convert.ToInt64(value, CultureInfo.InvariantCulture),
                [typeof(short)] = value => Convert.ToInt16(value, CultureInfo.InvariantCulture),
                [typeof(byte)] = value => Convert.ToByte(value, CultureInfo.InvariantCulture),
                [typeof(decimal)] = value => Convert.ToDecimal(value, CultureInfo.InvariantCulture),
                [typeof(double)] = value => Convert.ToDouble(value, CultureInfo.InvariantCulture),
                [typeof(float)] = value => Convert.ToSingle(value, CultureInfo.InvariantCulture),
                [typeof(sbyte)] = value => Convert.ToSByte(value, CultureInfo.InvariantCulture),
                [typeof(ushort)] = value => Convert.ToUInt16(value, CultureInfo.InvariantCulture),
                [typeof(uint)] = value => Convert.ToUInt32(value, CultureInfo.InvariantCulture),
                [typeof(ulong)] = value => Convert.ToUInt64(value, CultureInfo.InvariantCulture),
                [typeof(char)] = value => Convert.ToChar(value, CultureInfo.InvariantCulture)
            };

        /// <summary>
        /// Provides a mapping from a source type to a function that builds a converter for that type.
        /// </summary>
        /// <remarks>Each entry associates a supported primitive or common type with a factory method that
        /// creates a converter function. This enables dynamic construction of conversion delegates for supported types
        /// at runtime.</remarks>
        private static readonly Dictionary<Type, Func<Type, Func<object, object>>> PairBuilders =
            new Dictionary<Type, Func<Type, Func<object, object>>>()
            {
                [typeof(int)] = BuildPairConverterFromInt,
                [typeof(long)] = BuildPairConverterFromLong,
                [typeof(decimal)] = BuildPairConverterFromDecimal,
                [typeof(double)] = BuildPairConverterFromDouble,
                [typeof(float)] = BuildPairConverterFromFloat,
                [typeof(short)] = BuildPairConverterFromShort,
                [typeof(byte)] = BuildPairConverterFromByte,
                [typeof(char)] = BuildPairConverterFromChar,
                [typeof(string)] = BuildPairConverterFromString
            };

        /// <summary>
        /// Provides a mapping of target types to conversion functions that convert an object containing a 32-bit
        /// integer value to the specified type.
        /// </summary>
        /// <remarks>Each entry in the dictionary associates a numeric or character type with a function
        /// that performs a cast from an integer to that type. This is typically used to facilitate type conversions
        /// when handling values that originate as 32-bit integers but need to be represented as other types. The
        /// dictionary is intended for internal use to streamline type conversion logic.</remarks>
        private static readonly Dictionary<Type, Func<object, object>> IntPairConverters =
            new Dictionary<Type, Func<object, object>>()
            {
                [typeof(long)] = value => (long)(int)value,
                [typeof(short)] = value => (short)(int)value,
                [typeof(byte)] = value => (byte)(int)value,
                [typeof(sbyte)] = value => (sbyte)(int)value,
                [typeof(ushort)] = value => (ushort)(int)value,
                [typeof(uint)] = value => (uint)(int)value,
                [typeof(ulong)] = value => (ulong)(long)(int)value,
                [typeof(decimal)] = value => (decimal)(int)value,
                [typeof(double)] = value => (double)(int)value,
                [typeof(float)] = value => (float)(int)value,
                [typeof(char)] = value => (char)(int)value
            };

        /// <summary>
        /// Provides a mapping of target types to conversion functions that convert a boxed 64-bit integer value to the
        /// specified type.
        /// </summary>
        /// <remarks>Each entry in the dictionary associates a numeric type with a function that casts a
        /// boxed long value to that type. This is typically used to facilitate type-safe conversions from long values
        /// to other primitive numeric types. The dictionary supports common integral and floating-point types, as well
        /// as decimal.</remarks>
        private static readonly Dictionary<Type, Func<object, object>> LongPairConverters =
            new Dictionary<Type, Func<object, object>>()
            {
                [typeof(int)] = value => (int)(long)value,
                [typeof(short)] = value => (short)(long)value,
                [typeof(byte)] = value => (byte)(long)value,
                [typeof(sbyte)] = value => (sbyte)(long)value,
                [typeof(ushort)] = value => (ushort)(long)value,
                [typeof(uint)] = value => (uint)(long)value,
                [typeof(ulong)] = value => (ulong)(long)value,
                [typeof(decimal)] = value => (decimal)(long)value,
                [typeof(double)] = value => (double)(long)value,
                [typeof(float)] = value => (float)(long)value
            };

        /// <summary>
        /// Provides a mapping from numeric types to functions that convert a boxed decimal value to the specified type.
        /// </summary>
        /// <remarks>This dictionary is used to efficiently convert a value of type decimal, boxed as an
        /// object, to various numeric types such as int, long, short, byte, double, and float. The conversion functions
        /// assume that the input object is a decimal and will throw an exception if the cast is invalid.</remarks>
        private static readonly Dictionary<Type, Func<object, object>> DecimalPairConverters =
            new Dictionary<Type, Func<object, object>>()
            {
                [typeof(int)] = value => (int)(decimal)value,
                [typeof(long)] = value => (long)(decimal)value,
                [typeof(short)] = value => (short)(decimal)value,
                [typeof(byte)] = value => (byte)(decimal)value,
                [typeof(double)] = value => (double)(decimal)value,
                [typeof(float)] = value => (float)(decimal)value
            };

        /// <summary>
        /// Provides a mapping from numeric types to functions that convert a boxed double value to the specified type.
        /// </summary>
        /// <remarks>This dictionary is used to perform type-safe conversions from double to various
        /// numeric types, such as decimal, float, int, long, short, and byte. Each entry contains a delegate that casts
        /// the input object, expected to be a double, to the target type. This is useful when dynamically converting
        /// values retrieved as doubles to other numeric types.</remarks>
        private static readonly Dictionary<Type, Func<object, object>> DoublePairConverters =
            new Dictionary<Type, Func<object, object>>()
            {
                [typeof(decimal)] = value => (decimal)(double)value,
                [typeof(float)] = value => (float)(double)value,
                [typeof(int)] = value => (int)(double)value,
                [typeof(long)] = value => (long)(double)value,
                [typeof(short)] = value => (short)(double)value,
                [typeof(byte)] = value => (byte)(double)value
            };

        /// <summary>
        /// Provides a mapping from numeric types to functions that convert an object containing a single-precision
        /// floating-point value to the specified numeric type.
        /// </summary>
        /// <remarks>Each entry in the dictionary associates a target numeric type with a delegate that
        /// performs the conversion from a boxed float value to the corresponding type. This is useful for scenarios
        /// where dynamic type conversion from float to other numeric types is required.</remarks>
        private static readonly Dictionary<Type, Func<object, object>> FloatPairConverters =
            new Dictionary<Type, Func<object, object>>()
            {
                [typeof(double)] = value => (double)(float)value,
                [typeof(decimal)] = value => (decimal)(double)(float)value,
                [typeof(int)] = value => (int)(float)value,
                [typeof(long)] = value => (long)(float)value,
                [typeof(short)] = value => (short)(float)value,
                [typeof(byte)] = value => (byte)(float)value
            };

        /// <summary>
        /// Provides a mapping from target types to functions that convert a boxed 16-bit integer value to the specified
        /// type.
        /// </summary>
        /// <remarks>Each function in the dictionary takes an object representing a 16-bit integer and
        /// returns the value converted to the corresponding type. This dictionary supports conversions to common
        /// numeric types such as int, long, byte, sbyte, decimal, double, and float.</remarks>
        private static readonly Dictionary<Type, Func<object, object>> ShortPairConverters =
            new Dictionary<Type, Func<object, object>>()
            {
                [typeof(int)] = value => (int)(short)value,
                [typeof(long)] = value => (long)(short)value,
                [typeof(byte)] = value => (byte)(short)value,
                [typeof(sbyte)] = value => (sbyte)(short)value,
                [typeof(decimal)] = value => (decimal)(short)value,
                [typeof(double)] = value => (double)(short)value,
                [typeof(float)] = value => (float)(short)value
            };

        /// <summary>
        /// Provides a mapping of target types to functions that convert a boxed byte value to the specified type.
        /// </summary>
        /// <remarks>Each entry in the dictionary associates a supported target type with a delegate that
        /// performs the conversion from a byte to that type. The functions expect the input object to be a boxed byte
        /// value. Attempting to use an unsupported type or passing a value that is not a byte may result in an
        /// exception.</remarks>
        private static readonly Dictionary<Type, Func<object, object>> BytePairConverters =
            new Dictionary<Type, Func<object, object>>()
            {
                [typeof(int)] = value => (int)(byte)value,
                [typeof(long)] = value => (long)(byte)value,
                [typeof(short)] = value => (short)(byte)value,
                [typeof(decimal)] = value => (decimal)(byte)value,
                [typeof(double)] = value => (double)(byte)value,
                [typeof(float)] = value => (float)(byte)value
            };

        /// <summary>
        /// Provides a mapping from target types to functions that convert a character value to the specified type.
        /// </summary>
        /// <remarks>This dictionary is used to convert a character to supported types, such as int or
        /// string, by applying the corresponding conversion function. The key is the target type, and the value is a
        /// delegate that performs the conversion from an object representing a character to the target type.</remarks>
        private static readonly Dictionary<Type, Func<object, object>> CharPairConverters =
            new Dictionary<Type, Func<object, object>>()
            {
                [typeof(int)] = value => (int)(char)value,
                [typeof(string)] = value => ((char)value).ToString()
            };

        /// <summary>
        /// Provides a mapping of target types to functions that convert an object, typically a string, to the specified
        /// type for use in string pair conversions.
        /// </summary>
        /// <remarks>This dictionary is used to facilitate type conversions from string representations to
        /// specific types, such as converting a string to a char. Each entry associates a target type with a conversion
        /// function that performs the necessary transformation. If a conversion cannot be performed, the function may
        /// throw an exception to indicate the failure.</remarks>
        private static readonly Dictionary<Type, Func<object, object>> StringPairConverters =
            new Dictionary<Type, Func<object, object>>()
            {
                [typeof(char)] = value =>
                {
                    var text = (string)value;

                    if (text.Length > 0)
                        return (object)text[0];

                    throw new InvalidCastException("Cannot convert empty string to char.");
                }
            };

        /// <summary>
        /// Defines the maximum number of converter delegates stored in the shared cache.
        /// </summary>
        /// <remarks>When the limit is reached, new converters are created on demand without being added to the
        /// dictionary, avoiding unbounded memory growth while keeping hot-path performance through the volatile cache.
        /// The limit is enforced under concurrency using atomic slot reservation.</remarks>
        private const int MaxConverterCacheSize = 256;

        /// <summary>
        /// Converts the specified object to the specified type, returning the default value if the object is null or
        /// represents a database null (DBNull).
        /// </summary>
        /// <typeparam name="T">The type to which to convert the object.</typeparam>
        /// <param name="value">The object to convert. If this parameter is null or represents a database null (DBNull), the method returns
        /// the default value for type T.</param>
        /// <returns>The converted value of type T, or the default value of T if the input is null or represents a database null
        /// (DBNull).</returns>
        public static T As<T>(
            this object value)
        {
            if (value == null || value is DBNull)
                return default(T);

            var targetType = typeof(T);
            if (value is T typedValue)
                return typedValue;

            return (T)GetConverter(targetType)(value);
        }

        /// <summary>
        /// Retrieves or creates a cached converter delegate for the specified target type.
        /// </summary>
        /// <remarks>Uses a volatile single-entry hot cache for the most recently requested target type before
        /// falling back to the shared <see cref="ConverterCache"/>. Both the hot cache and dictionary are updated on
        /// each successful resolution to keep the hot path fast for repeated conversions to the same target
        /// type.</remarks>
        /// <param name="targetType">The type that conversion results should match.</param>
        /// <returns>A cached converter delegate for the requested target type.</returns>
        internal static Func<object, object> GetConverter(
            Type targetType)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(targetType);
#else
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));
#endif
            var cachedEntry = Volatile.Read(ref _lastConverterCache);
            if (cachedEntry != null &&
                ReferenceEquals(
                    cachedEntry.TargetType,
                    targetType))
            {
                return cachedEntry.Converter;
            }

            if (!ConverterCache.TryGetValue(
                targetType,
                out var converter))
            {
                converter = BuildConverter(targetType);

                if (TryReserveConverterCacheSlot())
                {
                    converter = AddConverterOrReuseCached(
                        targetType,
                        converter);
                }
                else
                {
                    converter = GetCachedConverterOrCurrent(
                        targetType,
                        converter);
                }
            }

            Volatile.Write(
                ref _lastConverterCache,
                new ConverterCacheEntry(
                    targetType,
                    converter));

            return converter;
        }

        /// <summary>
        /// Attempts to add the converter to the shared cache; if a race is lost, rolls back the reserved slot and
        /// returns the already-cached converter.
        /// </summary>
        /// <param name="targetType">The conversion target type used as the cache key.</param>
        /// <param name="currentConverter">The converter instance built for the target type prior to cache insertion.</param>
        /// <returns>
        /// <paramref name="currentConverter"/> when the insertion wins; the pre-existing cached converter when
        /// another thread inserted first and the reservation slot is rolled back.
        /// </returns>
        private static Func<object, object> AddConverterOrReuseCached(
            Type targetType,
            Func<object, object> currentConverter)
        {
            if (ConverterCache.TryAdd(
                targetType,
                currentConverter))
            {
                return currentConverter;
            }

            Interlocked.Decrement(ref _converterCacheSize);

            return GetCachedConverterOrCurrent(
                targetType,
                currentConverter);
        }

        /// <summary>
        /// Attempts to reserve a cache slot while enforcing the maximum converter cache size under concurrency.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> when a slot is reserved and the caller may proceed with cache insertion;
        /// <see langword="false"/> when the cache is at capacity and insertion should be skipped.
        /// </returns>
        private static bool TryReserveConverterCacheSlot()
            => TryReserveConverterCacheSlotCore(
                () => Volatile.Read(ref _converterCacheSize),
                TryUpdateConverterCacheSize);

        /// <summary>
        /// Attempts to reserve a cache slot using pluggable read and update delegates.
        /// </summary>
        /// <remarks>Retries in a spin loop until either the update succeeds or the cache is found to be at
        /// capacity, ensuring that a slot is never over-counted under concurrent access.</remarks>
        /// <param name="readCacheSize">Delegate used to read the current cache size on each iteration.</param>
        /// <param name="tryUpdate">Delegate used to atomically attempt the cache size increment.</param>
        /// <returns>
        /// <see langword="true"/> when a slot is successfully reserved; <see langword="false"/> when the cache
        /// is at or beyond <see cref="MaxConverterCacheSize"/>.
        /// </returns>
        private static bool TryReserveConverterCacheSlotCore(
            Func<int> readCacheSize,
            Func<int, int, bool> tryUpdate)
        {
            while (true)
            {
                var currentSize = readCacheSize();
                if (currentSize >= MaxConverterCacheSize)
                    return false;

                var updatedSize = currentSize + 1;
                if (tryUpdate(
                    currentSize,
                    updatedSize))
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Attempts to update the converter cache size field atomically from
        /// <paramref name="currentSize"/> to <paramref name="updatedSize"/>.
        /// </summary>
        /// <param name="currentSize">The expected current value of <see cref="_converterCacheSize"/>.</param>
        /// <param name="updatedSize">The value to store when the expected value is confirmed.</param>
        /// <returns>
        /// <see langword="true"/> when the compare-exchange succeeds; <see langword="false"/> when another thread
        /// modified the value concurrently and the caller must retry.
        /// </returns>
        private static bool TryUpdateConverterCacheSize(
            int currentSize,
            int updatedSize)
            => Interlocked.CompareExchange(
                ref _converterCacheSize,
                updatedSize,
                currentSize) == currentSize;

        /// <summary>
        /// Returns the cached converter for the specified target type when one exists; otherwise returns
        /// <paramref name="currentConverter"/>.
        /// </summary>
        /// <param name="targetType">The type whose converter is being resolved from the shared cache.</param>
        /// <param name="currentConverter">The converter to use as fallback when no cached entry is found.</param>
        /// <returns>
        /// The cached converter when found in <see cref="ConverterCache"/>; otherwise
        /// <paramref name="currentConverter"/>.
        /// </returns>
        private static Func<object, object> GetCachedConverterOrCurrent(
            Type targetType,
            Func<object, object> currentConverter)
            => ConverterCache.TryGetValue(
                targetType,
                out var existingConverter)
                ? existingConverter
                : currentConverter;

        /// <summary>
        /// Creates a delegate that converts an input object to the specified target type, handling common type
        /// conversions such as enums, GUIDs, strings, booleans, numbers, date/time values, and temporal spans.
        /// </summary>
        /// <remarks>The returned converter handles several common .NET types explicitly via dedicated fast paths.
        /// For source/target combinations not covered by a dedicated path, the converter resolves a typed pair
        /// delegate on first encounter and caches it in <see cref="PairConverterCache"/>, avoiding repeated
        /// <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> overhead on the hot path after
        /// warm-up.</remarks>
        /// <param name="targetType">The type to which the input object should be converted. May be a nullable type, in which case
        /// the underlying non-nullable type drives converter selection.</param>
        /// <returns>A function that takes an object and returns its value converted to the specified target type.</returns>
        private static Func<object, object> BuildConverter(
            Type targetType)
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            var conversionType = underlyingType ?? targetType;

            var (flowControl, value) = TryConvertEnum(conversionType);
            if (!flowControl)
                return value;

            (flowControl, value) = TryConvertGuid(conversionType);
            if (!flowControl)
                return value;

            (flowControl, value) = TryConvertString(conversionType);
            if (!flowControl)
                return value;

            (flowControl, value) = TryConvertBoolean(conversionType);
            if (!flowControl)
                return value;

            (flowControl, value) = TryConvertNumber(conversionType);
            if (!flowControl)
                return value;

            (flowControl, value) = TryConvertDateTime(conversionType);
            if (!flowControl)
                return value;

            (flowControl, value) = TryConvertDateTimeOffset(conversionType);
            if (!flowControl)
                return value;

            (flowControl, value) = TryConvertTimeSpan(conversionType);
            if (!flowControl)
                return value;

            // Final fallback: dispatch by (sourceType, targetType) pair.
            // Builds and caches a typed delegate on first encounter to avoid Convert.ChangeType
            // on heterogeneous-type hot paths after warm-up.
            return data =>
            {
                if (conversionType.IsInstanceOfType(
                    data))
                    return data;

                var sourceType = data.GetType();
                if (!PairConverterCache.TryGetValue(
                    (sourceType, conversionType),
                    out var pairConverter))
                {
                    pairConverter = BuildPairConverter(
                        sourceType,
                        conversionType);

                    PairConverterCache.TryAdd(
                        (sourceType, conversionType),
                        pairConverter);
                }

                return pairConverter(data);
            };
        }

        /// <summary>
        /// Attempts to create a delegate that converts values to the specified enumeration type.
        /// </summary>
        /// <remarks>If the type is an enumeration, the returned delegate handles already-typed values via an
        /// identity path and converts other values through <see cref="ParseEnumValue"/>. If the type is not an
        /// enumeration, the flow control flag signals the caller to proceed to the next converter
        /// attempt.</remarks>
        /// <param name="conversionType">The candidate target type. A converter is produced only when this is an enum type.</param>
        /// <returns>
        /// A tuple where <c>flowControl</c> is <see langword="false"/> and <c>value</c> is the converter delegate
        /// when <paramref name="conversionType"/> is an enum; otherwise <c>flowControl</c> is
        /// <see langword="true"/> and <c>value</c> is <see langword="null"/>.
        /// </returns>
        private static (bool flowControl, Func<object, object> value) TryConvertEnum(
            Type conversionType)
        {
            if (conversionType.IsEnum)
            {
                return (
                    flowControl: false,
                    value: value =>
                        conversionType.IsInstanceOfType(value)
                            ? value
                            : value.ParseEnumValue(conversionType));
            }

            return (
                flowControl: true,
                value: null);
        }

        /// <summary>
        /// Converts the specified value to an enumeration object of the given type, supporting both string and
        /// numeric representations.
        /// </summary>
        /// <remarks>String values are parsed case-insensitively via <see cref="Enum.Parse(Type, string, bool)"/>.
        /// Non-string values are treated as the underlying integral type and converted via
        /// <see cref="Enum.ToObject(Type, object)"/>.</remarks>
        /// <param name="value">The value to convert. May be a string containing the enum member name or a numeric value
        /// corresponding to an enum member.</param>
        /// <param name="conversionType">The enum type to convert <paramref name="value"/> to. Must be a valid enum type.</param>
        /// <returns>An object representing the enum value of <paramref name="conversionType"/> that corresponds to
        /// <paramref name="value"/>.</returns>
        private static object ParseEnumValue(
            this object value,
            Type conversionType)
            => value is string text
                ? Enum.Parse(
                    conversionType,
                    text,
                    ignoreCase: true)
                : Enum.ToObject(
                    conversionType,
                    value);

        /// <summary>
        /// Attempts to create a delegate that converts an object to <see cref="Guid"/> when the specified type is
        /// <see cref="Guid"/>.
        /// </summary>
        /// <remarks>Handles <see cref="Guid"/>, <see cref="string"/>, and <c>byte[]</c> representations with
        /// direct, allocation-free paths. For any other source type the delegate falls back to
        /// <c>Guid.Parse(value.ToString())</c>, which may throw <see cref="FormatException"/> for values that do
        /// not represent a valid GUID string.</remarks>
        /// <param name="conversionType">The candidate target type. A converter is produced only when this is <see cref="Guid"/>.</param>
        /// <returns>
        /// A tuple where <c>flowControl</c> is <see langword="false"/> and <c>value</c> is the converter delegate
        /// when <paramref name="conversionType"/> is <see cref="Guid"/>; otherwise <c>flowControl</c> is
        /// <see langword="true"/> and <c>value</c> is <see langword="null"/>.
        /// </returns>
        private static (bool flowControl, Func<object, object> value) TryConvertGuid(
            Type conversionType)
        {
            if (conversionType == typeof(Guid))
            {
                return (
                    flowControl: false,
                    value: value => value switch
                    {
                        Guid guid => guid,
                        string text => Guid.Parse(text),
                        byte[] bytes => new Guid(bytes),
                        _ => Guid.Parse(value.ToString())
                    });
            }

            return (
                flowControl: true,
                value: null);
        }

        /// <summary>
        /// Attempts to create a delegate that converts an object to <see cref="string"/> when the specified type
        /// is <see cref="string"/>.
        /// </summary>
        /// <param name="conversionType">The candidate target type. A converter is produced only when this is <see cref="string"/>.</param>
        /// <returns>
        /// A tuple where <c>flowControl</c> is <see langword="false"/> and <c>value</c> is a delegate that calls
        /// <see cref="object.ToString"/> when <paramref name="conversionType"/> is <see cref="string"/>;
        /// otherwise <c>flowControl</c> is <see langword="true"/> and <c>value</c> is <see langword="null"/>.
        /// </returns>
        private static (bool flowControl, Func<object, object> value) TryConvertString(
            Type conversionType)
        {
            if (conversionType == typeof(string))
                return (
                    flowControl: false,
                    value: value => value.ToString());

            return (
                flowControl: true,
                value: null);
        }

        /// <summary>
        /// Attempts to create a boolean conversion delegate for the specified type.
        /// </summary>
        /// <remarks>Covers all signed and unsigned integer types, floating-point types, decimal, and
        /// string without routing through
        /// <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/>. Numeric types use direct
        /// inequality-with-zero comparisons. Strings are parsed via <see cref="ParseBooleanFromString"/>, which
        /// accepts <c>true/false</c>, <c>1/0</c>, <c>yes/no</c>, and <c>on/off</c> in addition to the standard
        /// <see cref="bool.TryParse(string, out bool)"/> values.</remarks>
        /// <param name="conversionType">The candidate target type. A converter is produced only when this is <see cref="bool"/>.</param>
        /// <returns>
        /// A tuple where <c>flowControl</c> is <see langword="false"/> and <c>value</c> is the converter delegate
        /// when <paramref name="conversionType"/> is <see cref="bool"/>; otherwise <c>flowControl</c> is
        /// <see langword="true"/> and <c>value</c> is <see langword="null"/>.
        /// </returns>
        private static (bool flowControl, Func<object, object> value) TryConvertBoolean(
            Type conversionType)
        {
            if (conversionType == typeof(bool))
            {
                return (
                    flowControl: false,
                    value: value =>
                    {
                        if (value is bool b)
                            return b;

                        switch (value)
                        {
                            case int i: return i != 0;
                            case short s: return s != 0;
                            case long l: return l != 0;
                            case byte bt: return bt != 0;
                            case sbyte sb: return sb != 0;
                            case ushort us: return us != 0;
                            case uint ui: return ui != 0;
                            case ulong ul: return ul != 0;
                            case float f: return Math.Abs(f) > 1e-6f;
                            case double d: return Math.Abs(d) > 1e-12;
                            case decimal m: return m != 0m;
                            case string str: return ParseBooleanFromString(str);
                            default:
                                return Convert.ToBoolean(
                                    value,
                                    CultureInfo.InvariantCulture);
                        }
                    }
                );
            }

            return (
                flowControl: true,
                value: null);
        }

        /// <summary>
        /// Parses a boolean value from a string, recognising <c>true/false</c>, <c>1/0</c>,
        /// <c>yes/no</c>, and <c>on/off</c> without routing through
        /// <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/>.
        /// </summary>
        /// <remarks>Comparisons for <c>yes/no</c> and <c>on/off</c> are case-insensitive. For values that
        /// match none of the recognised patterns, <see cref="Convert.ToBoolean(object, IFormatProvider)"/> is used
        /// as a final fallback.</remarks>
        /// <param name="value">The string to parse.</param>
        /// <returns>
        /// <see langword="true"/> for <c>true</c>, <c>1</c>, <c>yes</c>, or <c>on</c>;
        /// <see langword="false"/> for <c>false</c>, <c>0</c>, <c>no</c>, or <c>off</c>;
        /// or the result of <see cref="Convert.ToBoolean(object, IFormatProvider)"/> for other values.
        /// </returns>
        private static bool ParseBooleanFromString(
            string value)
        {
            if (bool.TryParse(value, out var result))
                return result;

            if (value == "1" ||
                value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("on", StringComparison.OrdinalIgnoreCase))
                return true;

            if (value == "0" ||
                value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("off", StringComparison.OrdinalIgnoreCase))
                return false;

            return Convert.ToBoolean(
                value,
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Attempts to create a conversion delegate for the specified numeric type.
        /// </summary>
        /// <remarks>Covers all standard signed and unsigned integer types, floating-point types, decimal, and
        /// char using the type-specific <c>Convert.ToXxx</c> methods, which are significantly faster than the
        /// generic <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> path because they bypass
        /// runtime type dispatch and call the underlying <see cref="IConvertible"/> implementation
        /// directly.</remarks>
        /// <param name="conversionType">The candidate target numeric type. Supported types are <see cref="int"/>, <see cref="long"/>,
        /// <see cref="short"/>, <see cref="byte"/>, <see cref="decimal"/>, <see cref="double"/>,
        /// <see cref="float"/>, <see cref="sbyte"/>, <see cref="ushort"/>, <see cref="uint"/>,
        /// <see cref="ulong"/>, and <see cref="char"/>.</param>
        /// <returns>
        /// A tuple where <c>flowControl</c> is <see langword="false"/> and <c>value</c> is the typed converter
        /// delegate when <paramref name="conversionType"/> matches a supported numeric type; otherwise
        /// <c>flowControl</c> is <see langword="true"/> and <c>value</c> is <see langword="null"/>.
        /// </returns>
        private static (bool flowControl, Func<object, object> value) TryConvertNumber(
            Type conversionType)
        {
            if (NumberConverters.TryGetValue(
                conversionType,
                out var converter))
                return (false, converter);

            return (true, null);
        }

        /// <summary>
        /// Attempts to create a conversion delegate for values to <see cref="DateTime"/>.
        /// </summary>
        /// <remarks>When the source is already a <see cref="DateTime"/> the value is returned via an identity
        /// path. For other source types, <see cref="Convert.ToDateTime(object, IFormatProvider)"/> is used with
        /// <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        /// <param name="conversionType">The candidate target type. A converter is produced only when this is <see cref="DateTime"/>.</param>
        /// <returns>
        /// A tuple where <c>flowControl</c> is <see langword="false"/> and <c>value</c> is the converter delegate
        /// when <paramref name="conversionType"/> is <see cref="DateTime"/>; otherwise <c>flowControl</c> is
        /// <see langword="true"/> and <c>value</c> is <see langword="null"/>.
        /// </returns>
        private static (bool flowControl, Func<object, object> value) TryConvertDateTime(
            Type conversionType)
        {
            if (conversionType == typeof(DateTime))
            {
                return (
                    flowControl: false,
                    value: value => value is DateTime dateTime
                        ? (object)dateTime
                        : Convert.ToDateTime(
                            value,
                            CultureInfo.InvariantCulture));
            }

            return (
                flowControl: true,
                value: null);
        }

        /// <summary>
        /// Attempts to create a conversion delegate for values to <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <remarks>Handles <see cref="DateTimeOffset"/>, <see cref="DateTime"/>, <see cref="string"/>, and
        /// <see cref="long"/> (interpreted as UTC ticks) representations with explicit paths that avoid
        /// <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/>. Any other source type falls back to
        /// <see cref="DateTimeOffset.Parse(string, IFormatProvider)"/> on the string representation of the
        /// value.</remarks>
        /// <param name="conversionType">The candidate target type. A converter is produced only when this is <see cref="DateTimeOffset"/>.</param>
        /// <returns>
        /// A tuple where <c>flowControl</c> is <see langword="false"/> and <c>value</c> is the converter delegate
        /// when <paramref name="conversionType"/> is <see cref="DateTimeOffset"/>; otherwise <c>flowControl</c>
        /// is <see langword="true"/> and <c>value</c> is <see langword="null"/>.
        /// </returns>
        private static (bool flowControl, Func<object, object> value) TryConvertDateTimeOffset(
            Type conversionType)
        {
            if (conversionType == typeof(DateTimeOffset))
            {
                return (
                    flowControl: false,
                    value: value => value switch
                    {
                        DateTimeOffset dto => dto,
                        DateTime dt => new DateTimeOffset(dt),
                        string str => DateTimeOffset.Parse(str, CultureInfo.InvariantCulture),
                        long ticks => new DateTimeOffset(new DateTime(ticks, DateTimeKind.Utc)),
                        _ => DateTimeOffset.Parse(value.ToString(), CultureInfo.InvariantCulture)
                    });
            }

            return (
                flowControl: true,
                value: null);
        }

        /// <summary>
        /// Attempts to create a conversion delegate for values to <see cref="TimeSpan"/>.
        /// </summary>
        /// <remarks>Handles <see cref="TimeSpan"/>, <see cref="long"/> (ticks), <see cref="int"/>
        /// (milliseconds), and <see cref="string"/> representations with explicit paths that avoid
        /// <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/>. Any other source type falls back to
        /// <see cref="TimeSpan.Parse(string, IFormatProvider)"/> on the string representation of the
        /// value.</remarks>
        /// <param name="conversionType">The candidate target type. A converter is produced only when this is <see cref="TimeSpan"/>.</param>
        /// <returns>
        /// A tuple where <c>flowControl</c> is <see langword="false"/> and <c>value</c> is the converter delegate
        /// when <paramref name="conversionType"/> is <see cref="TimeSpan"/>; otherwise <c>flowControl</c> is
        /// <see langword="true"/> and <c>value</c> is <see langword="null"/>.
        /// </returns>
        private static (bool flowControl, Func<object, object> value) TryConvertTimeSpan(
            Type conversionType)
        {
            if (conversionType == typeof(TimeSpan))
            {
                return (
                    flowControl: false,
                    value: value => value switch
                    {
                        TimeSpan ts => ts,
                        long ticks => new TimeSpan(ticks),
                        int ms => TimeSpan.FromMilliseconds(ms),
                        string str => TimeSpan.Parse(str, CultureInfo.InvariantCulture),
                        _ => TimeSpan.Parse(value.ToString(), CultureInfo.InvariantCulture)
                    });
            }

            return (
                flowControl: true,
                value: null);
        }

        /// <summary>
        /// Builds a typed conversion delegate for a specific (<paramref name="sourceType"/>,
        /// <paramref name="targetType"/>) pair by dispatching to a per-source-type helper, avoiding
        /// <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> for all common numeric cross-type
        /// conversions.
        /// </summary>
        /// <remarks>Delegates to a dedicated helper for each recognised source type. For source/target
        /// combinations not covered by any helper, a <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/>
        /// delegate is returned; this fallback is stored in <see cref="PairConverterCache"/> so it is only
        /// resolved once per unique pair.</remarks>
        /// <param name="sourceType">The runtime type of the input value.</param>
        /// <param name="targetType">The desired output type.</param>
        /// <returns>A delegate that converts an object of <paramref name="sourceType"/> to
        /// <paramref name="targetType"/>.</returns>
        private static Func<object, object> BuildPairConverter(
            Type sourceType,
            Type targetType)
        {
            if (PairBuilders.TryGetValue(
                sourceType,
                out var builder))
                return builder(targetType);

            return value => Convert.ChangeType(
                value,
                targetType,
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a delegate that converts an <see cref="int"/> value to <paramref name="targetType"/> using a
        /// direct cast, or a <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> fallback for
        /// unrecognised targets.
        /// </summary>
        /// <param name="targetType">The desired output type.</param>
        /// <returns>A delegate that unboxes an <see cref="int"/> and casts it to <paramref name="targetType"/>,
        /// or falls back to <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> for exotic
        /// targets.</returns>
        private static Func<object, object> BuildPairConverterFromInt(
            Type targetType)
        {
            if (IntPairConverters.TryGetValue(
                targetType,
                out var converter))
                return converter;

            return value => Convert.ChangeType(
                value,
                targetType,
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a delegate that converts a <see cref="long"/> value to <paramref name="targetType"/> using a
        /// direct cast, or a <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> fallback for
        /// unrecognised targets.
        /// </summary>
        /// <param name="targetType">The desired output type.</param>
        /// <returns>A delegate that unboxes a <see cref="long"/> and casts it to <paramref name="targetType"/>,
        /// or falls back to <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> for exotic
        /// targets.</returns>
        private static Func<object, object> BuildPairConverterFromLong(
            Type targetType)
        {
            if (LongPairConverters.TryGetValue(
                targetType,
                out var converter))
                return converter;

            return value => Convert.ChangeType(
                value,
                targetType,
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a delegate that converts a <see cref="decimal"/> value to <paramref name="targetType"/> using a
        /// direct cast, or a <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> fallback for
        /// unrecognised targets.
        /// </summary>
        /// <param name="targetType">The desired output type.</param>
        /// <returns>A delegate that unboxes a <see cref="decimal"/> and casts it to <paramref name="targetType"/>,
        /// or falls back to <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> for exotic
        /// targets.</returns>
        private static Func<object, object> BuildPairConverterFromDecimal(
            Type targetType)
        {
            if (DecimalPairConverters.TryGetValue(
                targetType,
                out var converter))
                return converter;

            return value => Convert.ChangeType(
                value,
                targetType,
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a delegate that converts a <see cref="double"/> value to <paramref name="targetType"/> using a
        /// direct cast, or a <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> fallback for
        /// unrecognised targets.
        /// </summary>
        /// <param name="targetType">The desired output type.</param>
        /// <returns>A delegate that unboxes a <see cref="double"/> and casts it to <paramref name="targetType"/>,
        /// or falls back to <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> for exotic
        /// targets.</returns>
        private static Func<object, object> BuildPairConverterFromDouble(
            Type targetType)
        {
            if (DoublePairConverters.TryGetValue(
                targetType,
                out var converter))
                return converter;

            return value => Convert.ChangeType(
                value,
                targetType,
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a delegate that converts a <see cref="float"/> value to <paramref name="targetType"/> using a
        /// direct cast, or a <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> fallback for
        /// unrecognised targets.
        /// </summary>
        /// <param name="targetType">The desired output type.</param>
        /// <returns>A delegate that unboxes a <see cref="float"/> and casts it to <paramref name="targetType"/>,
        /// or falls back to <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> for exotic
        /// targets.</returns>
        private static Func<object, object> BuildPairConverterFromFloat(
            Type targetType)
        {
            if (FloatPairConverters.TryGetValue(
                targetType,
                out var converter))
                return converter;

            return value => Convert.ChangeType(
                value,
                targetType,
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a delegate that converts a <see cref="short"/> value to <paramref name="targetType"/> using a
        /// direct cast, or a <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> fallback for
        /// unrecognised targets.
        /// </summary>
        /// <param name="targetType">The desired output type.</param>
        /// <returns>A delegate that unboxes a <see cref="short"/> and casts it to <paramref name="targetType"/>,
        /// or falls back to <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> for exotic
        /// targets.</returns>
        private static Func<object, object> BuildPairConverterFromShort(
            Type targetType)
        {
            if (ShortPairConverters.TryGetValue(
                targetType,
                out var converter))
                return converter;

            return value => Convert.ChangeType(
                value,
                targetType,
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a delegate that converts a <see cref="byte"/> value to <paramref name="targetType"/> using a
        /// direct cast, or a <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> fallback for
        /// unrecognised targets.
        /// </summary>
        /// <param name="targetType">The desired output type.</param>
        /// <returns>A delegate that unboxes a <see cref="byte"/> and casts it to <paramref name="targetType"/>,
        /// or falls back to <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> for exotic
        /// targets.</returns>
        private static Func<object, object> BuildPairConverterFromByte(
            Type targetType)
        {
            if (BytePairConverters.TryGetValue(
                targetType,
                out var converter))
                return converter;

            return value => Convert.ChangeType(
                value,
                targetType,
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a delegate that converts a <see cref="char"/> value to <paramref name="targetType"/> using a
        /// direct cast, or a <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> fallback for
        /// unrecognised targets.
        /// </summary>
        /// <param name="targetType">The desired output type.</param>
        /// <returns>A delegate that unboxes a <see cref="char"/> and casts it to <paramref name="targetType"/>,
        /// or falls back to <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> for exotic
        /// targets.</returns>
        private static Func<object, object> BuildPairConverterFromChar(
            Type targetType)
        {
            if (CharPairConverters.TryGetValue(
                targetType,
                out var converter))
                return converter;

            return value => Convert.ChangeType(
                value,
                targetType,
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a delegate that converts a <see cref="string"/> value to <paramref name="targetType"/> using a
        /// direct cast, or a <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> fallback for
        /// unrecognised targets.
        /// </summary>
        /// <param name="targetType">The desired output type.</param>
        /// <returns>A delegate that casts a <see cref="string"/> to <paramref name="targetType"/>,
        /// or falls back to <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> for exotic
        /// targets.</returns>
        private static Func<object, object> BuildPairConverterFromString(
            Type targetType)
        {
            if (StringPairConverters.TryGetValue(
                targetType,
                out var converter))
                return converter;

            return value => Convert.ChangeType(
                value,
                targetType,
                CultureInfo.InvariantCulture);
        }
    }
}
