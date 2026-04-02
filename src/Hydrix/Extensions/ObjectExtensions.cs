using System;
using System.Collections.Concurrent;
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
        /// Holds the target type of the most recently used converter in the process-wide hot cache.
        /// </summary>
        private static Type _lastConverterTargetType;

        /// <summary>
        /// Holds the converter delegate of the most recently used converter in the process-wide hot cache.
        /// </summary>
        private static Func<object, object> _lastConverter;

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
        /// <remarks>This method uses Convert.ChangeType to perform the conversion. An exception is thrown
        /// if the conversion is not supported for the specified type.</remarks>
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
            var cachedTargetType = Volatile.Read(ref _lastConverterTargetType);
            var cachedConverter = Volatile.Read(ref _lastConverter);

            if (ReferenceEquals(
                    cachedTargetType,
                    targetType) &&
                cachedConverter != null)
            {
                return cachedConverter;
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
                ref _lastConverter,
                converter);

            Volatile.Write(
                ref _lastConverterTargetType,
                targetType);

            return converter;
        }

        /// <summary>
        /// Adds the converter to cache when possible; otherwise, reuses the cached converter and rolls back the
        /// reserved cache slot.
        /// </summary>
        /// <param name="targetType">The conversion target type used as cache key.</param>
        /// <param name="currentConverter">The current converter instance materialized for the target type.</param>
        /// <returns>The cached converter when cache insertion loses a race; otherwise, <paramref name="currentConverter"/>.</returns>
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
        /// <returns><see langword="true"/> when a slot is reserved; otherwise, <see langword="false"/>.</returns>
        private static bool TryReserveConverterCacheSlot()
            => TryReserveConverterCacheSlotCore(
                () => Volatile.Read(ref _converterCacheSize),
                TryUpdateConverterCacheSize);

        /// <summary>
        /// Attempts to reserve a cache slot using pluggable read and update delegates.
        /// </summary>
        /// <param name="readCacheSize">Delegate used to read the current cache size.</param>
        /// <param name="tryUpdate">Delegate used to atomically attempt the cache size update.</param>
        /// <returns><see langword="true"/> when a slot is reserved; otherwise, <see langword="false"/>.</returns>
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
        /// Attempts to update converter cache size atomically for a single reservation attempt.
        /// </summary>
        /// <param name="currentSize">The expected current cache size value.</param>
        /// <param name="updatedSize">The cache size value to set when the expected value matches.</param>
        /// <returns><see langword="true"/> when the update succeeds; otherwise, <see langword="false"/>.</returns>
        private static bool TryUpdateConverterCacheSize(
            int currentSize,
            int updatedSize)
            => Interlocked.CompareExchange(
                ref _converterCacheSize,
                updatedSize,
                currentSize) == currentSize;

        /// <summary>
        /// Returns the cached converter for the specified target type when present; otherwise, returns the current
        /// converter instance.
        /// </summary>
        /// <param name="targetType">The type whose converter is being resolved from cache.</param>
        /// <param name="currentConverter">The converter to use when no cached converter exists.</param>
        /// <returns>The cached converter when found; otherwise, <paramref name="currentConverter"/>.</returns>
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
        /// conversions such as enums, GUIDs, strings, booleans, numbers, and date/time values.
        /// </summary>
        /// <remarks>The returned converter handles several common .NET types explicitly. If the target
        /// type is not one of the supported types, the converter uses standard type conversion with invariant culture.
        /// This method is intended for internal use when dynamic type conversion is required.</remarks>
        /// <param name="targetType">The type to which the input object should be converted. May be a nullable type.</param>
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

            return data =>
                conversionType.IsInstanceOfType(data)
                    ? data
                    : Convert.ChangeType(
                        data,
                        conversionType,
                        CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Attempts to create a delegate that converts values to the specified enumeration type.
        /// </summary>
        /// <remarks>If the specified type is an enumeration, the returned delegate can convert from a
        /// string (case-insensitive) or from a value of the underlying type. If the type is not an enumeration, flow
        /// control is indicated as true and no delegate is provided.</remarks>
        /// <param name="conversionType">The target type to convert to. Must be an enumeration type to enable conversion.</param>
        /// <returns>A tuple containing a Boolean indicating whether flow control should continue, and a delegate that converts
        /// an object to the specified enumeration type if applicable; otherwise, null.</returns>
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
        /// Converts the specified value to an enumeration object of the given type, supporting both string and numeric
        /// representations.
        /// </summary>
        /// <remarks>If the value is a string, the comparison is case-insensitive. If the value is not a
        /// string, it is treated as a numeric value and converted to the corresponding enum value.</remarks>
        /// <param name="value">The value to convert. Can be a string representing the name of an enum member or a numeric value
        /// corresponding to an enum value.</param>
        /// <param name="conversionType">The type of enumeration to convert the value to. Must be a valid enum type.</param>
        /// <returns>An object representing the enumeration value of the specified type that corresponds to the provided value.</returns>
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
        /// Attempts to create a delegate that converts an object to a Guid if the specified type is Guid.
        /// </summary>
        /// <remarks>The returned delegate handles conversion from Guid, string, and byte array
        /// representations to Guid. For other types, it uses standard type conversion.</remarks>
        /// <param name="conversionType">The target type to check for Guid conversion. If this is typeof(Guid), a conversion delegate is returned.</param>
        /// <returns>A tuple containing a Boolean indicating whether flow control should continue, and a delegate that converts
        /// an object to a Guid if applicable. If the type is not Guid, the delegate is null and flow control is set to
        /// true.</returns>
        private static (bool flowControl, Func<object, object> value) TryConvertGuid(
            Type conversionType)
        {
            if (conversionType == typeof(Guid))
            {
                return (
                    flowControl: false,
                    value: value =>
                    {
                        return value switch
                        {
                            Guid guid => guid,
                            string text => Guid.Parse(text),
                            byte[] bytes => new Guid(bytes),
                            _ => Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture)
                        };
                    }
                );
            }

            return (
                flowControl: true,
                value: null);
        }

        /// <summary>
        /// Attempts to create a string conversion delegate for the specified type.
        /// </summary>
        /// <remarks>If the specified type is string, the returned delegate converts an object to its
        /// string representation using ToString. For other types, the delegate is null and flow control is indicated as
        /// required.</remarks>
        /// <param name="conversionType">The target type to check for string conversion support. Typically, this is the type to which a value may be
        /// converted.</param>
        /// <returns>A tuple containing a Boolean value that indicates whether flow control is required, and a delegate that
        /// performs the conversion if supported; otherwise, null.</returns>
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
        /// <remarks>The returned delegate can convert values of several numeric types to boolean,
        /// interpreting nonzero values as <see langword="true"/>. For unsupported types, the method indicates that flow
        /// control should be used instead.</remarks>
        /// <param name="conversionType">The target type to check for boolean conversion support. Typically, this is the type to which a value should
        /// be converted.</param>
        /// <returns>A tuple containing a flow control flag and a delegate for converting values to boolean. If the conversion is
        /// supported, the delegate is non-null and the flow control flag is false; otherwise, the delegate is null and
        /// the flow control flag is true.</returns>
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

                        return value switch
                        {
                            int i => i != 0,
                            short s => s != 0,
                            long l => l != 0,
                            byte bt => bt != 0,
                            sbyte sb => sb != 0,
                            ushort us => us != 0,
                            uint ui => ui != 0,
                            ulong ul => ul != 0,
                            _ => Convert.ChangeType(
                                value,
                                typeof(bool),
                                CultureInfo.InvariantCulture)
                        };
                    }
                );
            }

            return (
                flowControl: true,
                value: null);
        }

        /// <summary>
        /// Attempts to create a conversion delegate for the specified numeric type.
        /// </summary>
        /// <remarks>If the specified type is not a supported numeric type, the returned delegate will be
        /// null and the flow control flag will be set to true. The conversion uses CultureInfo.InvariantCulture to
        /// ensure consistent parsing of numeric values.</remarks>
        /// <param name="conversionType">The target numeric type to which values should be converted. Supported types include int, long, short, byte,
        /// decimal, double, and float.</param>
        /// <returns>A tuple where the first element indicates whether the conversion type is unsupported, and the second element
        /// is a delegate that converts an object to the specified numeric type using invariant culture, or null if the
        /// type is not supported.</returns>
        private static (bool flowControl, Func<object, object> value) TryConvertNumber(
            Type conversionType)
        {
            return conversionType switch
            {
                var t when t == typeof(int) => (
                    false,
                    value => Convert.ToInt32(
                        value,
                        CultureInfo.InvariantCulture)),

                var t when t == typeof(long) => (
                    false,
                    value => Convert.ToInt64(
                        value,
                        CultureInfo.InvariantCulture)),

                var t when t == typeof(short) => (
                    false,
                    value => Convert.ToInt16(
                        value,
                        CultureInfo.InvariantCulture)),

                var t when t == typeof(byte) => (
                    false,
                    value => Convert.ToByte(
                        value,
                        CultureInfo.InvariantCulture)),

                var t when t == typeof(decimal) => (
                    false,
                    value => Convert.ToDecimal(
                        value,
                        CultureInfo.InvariantCulture)),

                var t when t == typeof(double) => (
                    false,
                    value => Convert.ToDouble(
                        value,
                        CultureInfo.InvariantCulture)),

                var t when t == typeof(float) => (
                    false,
                    value => Convert.ToSingle(
                        value,
                        CultureInfo.InvariantCulture)),

                _ => (true, null)
            };
        }

        /// <summary>
        /// Attempts to create a conversion delegate for values to the specified type if it is a DateTime.
        /// </summary>
        /// <remarks>If the specified type is DateTime, the returned delegate converts the input value to
        /// a DateTime using invariant culture. For other types, no conversion delegate is provided.</remarks>
        /// <param name="conversionType">The target type to convert values to. Must be a valid type, typically DateTime.</param>
        /// <returns>A tuple containing a Boolean indicating whether flow control should continue, and a delegate that converts
        /// an object to a DateTime if applicable. If the type is not DateTime, the delegate is null and flow control is
        /// set to true.</returns>
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
    }
}
