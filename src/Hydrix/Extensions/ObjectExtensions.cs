using System;
using System.Globalization;

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
        public static T As<T>(this object value)
        {
            if (value == null || value is DBNull)
                return default(T);

            var targetType = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            var conversionType = underlyingType ?? targetType;

            var (flowControl, converted) = TryConvertEnum<T>(value, conversionType);
            if (!flowControl)
                return converted;

            (flowControl, converted) = TryConvertGuid<T>(value, conversionType);
            if (!flowControl)
                return converted;

            (flowControl, converted) = TryConvertString<T>(value, conversionType);
            if (!flowControl)
                return converted;

            (flowControl, converted) = TryConvertBoolean<T>(value, conversionType);
            if (!flowControl)
                return converted;

            (flowControl, converted) = TryConvertNumber<T>(value, conversionType);
            if (!flowControl)
                return converted;

            (flowControl, converted) = TryConvertDateTime<T>(value, conversionType);
            if (!flowControl)
                return converted;

            (flowControl, converted) = TryConvertType<T>(value);
            if (!flowControl)
                return converted;

            return (T)Convert.ChangeType(
                value,
                conversionType,
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Attempts to convert the specified object to the given type and indicates whether flow control should be
        /// applied based on the conversion result.
        /// </summary>
        /// <typeparam name="T">The target type to which the object is to be converted.</typeparam>
        /// <param name="value">The object to convert to the specified type. Can be null.</param>
        /// <returns>A tuple containing a Boolean value indicating whether flow control should be applied and the converted value
        /// of type T. If the conversion fails, the value is the default for type T.</returns>
        private static (bool flowControl, T value) TryConvertType<T>(
            object value)
        {
            if (value is T t)
                return (
                    flowControl: false,
                    value: t);

            return (
                flowControl: true,
                value: default);
        }

        /// <summary>
        /// Attempts to convert the specified value to a DateTime type and returns the result along with a flow control
        /// flag.
        /// </summary>
        /// <remarks>If the conversion type is not DateTime, the method returns the default value of T and
        /// sets the flow control flag to true. The conversion uses invariant culture settings.</remarks>
        /// <typeparam name="T">The type to which the value is cast after conversion. Typically used to match the expected return type.</typeparam>
        /// <param name="value">The object to convert to a DateTime. Can be a DateTime instance or a value convertible to DateTime.</param>
        /// <param name="conversionType">The target type for conversion. If set to typeof(DateTime), the method attempts the conversion; otherwise,
        /// no conversion is performed.</param>
        /// <returns>A tuple containing a flow control flag and the converted value. The flow control flag is false if conversion
        /// was attempted; otherwise, true. The value is the converted DateTime cast to type T, or the default value of
        /// T if conversion was not performed.</returns>
        private static (bool flowControl, T value) TryConvertDateTime<T>(
            object value,
            Type conversionType)
        {
            if (conversionType == typeof(DateTime))
            {
                if (value is DateTime)
                    return (
                        flowControl: false,
                        value: (T)value);

                return (
                    flowControl: false,
                    value: (T)(object)Convert.ToDateTime(
                        value,
                        CultureInfo.InvariantCulture));
            }

            return (
                flowControl: true,
                value: default);
        }

        /// <summary>
        /// Attempts to convert the specified value to a numeric type using the provided conversion type.
        /// </summary>
        /// <remarks>Conversion is performed using invariant culture. If the conversion type is not
        /// supported, the method returns flowControl as true and value as the default for type T.</remarks>
        /// <typeparam name="T">The target numeric type to which the value will be converted.</typeparam>
        /// <param name="value">The value to convert. Must be compatible with the specified conversion type.</param>
        /// <param name="conversionType">The type representing the numeric type to convert to. Supported types include int, long, short, byte,
        /// decimal, double, and float.</param>
        /// <returns>A tuple containing a flow control flag and the converted value. If conversion is successful, flowControl is
        /// false and value contains the converted result; otherwise, flowControl is true and value is the default for
        /// type T.</returns>
        private static (bool flowControl, T value) TryConvertNumber<T>(
            object value,
            Type conversionType)
        {
            if (conversionType == typeof(int))
                return (
                    flowControl: false,
                    value: (T)(object)Convert.ToInt32(
                        value,
                        CultureInfo.InvariantCulture));

            if (conversionType == typeof(long))
                return (
                    flowControl: false,
                    value: (T)(object)Convert.ToInt64(
                        value,
                        CultureInfo.InvariantCulture));

            if (conversionType == typeof(short))
                return (
                    flowControl: false,
                    value: (T)(object)Convert.ToInt16(
                        value,
                        CultureInfo.InvariantCulture));

            if (conversionType == typeof(byte))
                return (
                    flowControl: false,
                    value: (T)(object)Convert.ToByte(
                        value,
                        CultureInfo.InvariantCulture));

            if (conversionType == typeof(decimal))
                return (
                    flowControl: false,
                    value: (T)(object)Convert.ToDecimal(
                        value,
                        CultureInfo.InvariantCulture));

            if (conversionType == typeof(double))
                return (
                    flowControl: false,
                    value: (T)(object)Convert.ToDouble(
                        value,
                        CultureInfo.InvariantCulture));

            if (conversionType == typeof(float))
                return (
                    flowControl: false,
                    value: (T)(object)Convert.ToSingle(
                        value,
                        CultureInfo.InvariantCulture));

            return (
                flowControl: true,
                value: default);
        }

        /// <summary>
        /// Attempts to convert the specified value to a Boolean type or its equivalent, returning a tuple indicating
        /// whether the conversion was successful and the converted value.
        /// </summary>
        /// <remarks>Supported conversions include Boolean, integer types (int, short, long), where
        /// nonzero values are treated as <see langword="true"/>. If the conversion type is not Boolean or the value is
        /// not convertible, the method returns the default value and sets the flow control flag to <see
        /// langword="true"/>.</remarks>
        /// <typeparam name="T">The type to which the Boolean value is converted. Must be compatible with Boolean or its equivalent
        /// representation.</typeparam>
        /// <param name="value">The value to convert. Can be a Boolean, integer, or other supported type.</param>
        /// <param name="conversionType">The target type for conversion. Typically Boolean or a type representing Boolean values.</param>
        /// <returns>A tuple containing a flow control flag and the converted value. The flow control flag is <see
        /// langword="false"/> if conversion was successful; otherwise, <see langword="true"/>. The value is the
        /// converted result or the default value of <typeparamref name="T"/> if conversion fails.</returns>
        private static (bool flowControl, T value) TryConvertBoolean<T>(
            object value,
            Type conversionType)
        {
            if (conversionType == typeof(bool))
            {
                if (value is bool)
                    return (
                        flowControl: false,
                        value: (T)value);

                if (value is int i)
                    return (
                        flowControl: false,
                        value: (T)(object)(i != 0));

                if (value is short s)
                    return (
                        flowControl: false,
                        value: (T)(object)(s != 0));

                if (value is long l)
                    return (
                        flowControl: false,
                        value: (T)(object)(l != 0));
            }

            return (
                flowControl: true,
                value: default);
        }

        /// <summary>
        /// Attempts to convert the specified value to a string if the target conversion type is string.
        /// </summary>
        /// <typeparam name="T">The type to which the value is to be converted.</typeparam>
        /// <param name="value">The value to be converted.</param>
        /// <param name="conversionType">The target type for conversion. If this is typeof(string), the value is converted to a string.</param>
        /// <returns>A tuple containing a flow control flag and the converted value. If conversionType is string, flowControl is
        /// false and value is the string representation; otherwise, flowControl is true and value is the default value
        /// of type T.</returns>
        private static (bool flowControl, T value) TryConvertString<T>(
            object value,
            Type conversionType)
        {
            if (conversionType == typeof(string))
                return (
                    flowControl: false,
                    value: (T)(object)value.ToString());

            return (
                flowControl: true,
                value: default);
        }

        /// <summary>
        /// Attempts to convert the specified value to a Guid using the provided conversion type.
        /// </summary>
        /// <remarks>If the conversion type is not Guid or the value cannot be converted, the method
        /// returns flowControl as true and value as default. Supported input types are Guid, string, and byte
        /// array.</remarks>
        /// <typeparam name="T">The type to which the value is converted. Must be compatible with Guid.</typeparam>
        /// <param name="value">The value to convert. Can be a Guid, a string representation of a Guid, or a byte array containing Guid
        /// data.</param>
        /// <param name="conversionType">The target type for conversion. Must be typeof(Guid) to perform the conversion.</param>
        /// <returns>A tuple containing a flow control flag and the converted value. If conversion is successful, flowControl is
        /// false and value contains the converted Guid; otherwise, flowControl is true and value is the default value
        /// of T.</returns>
        private static (bool flowControl, T value) TryConvertGuid<T>(
            object value,
            Type conversionType)
        {
            if (conversionType == typeof(Guid))
            {
                if (value is Guid)
                    return (
                        flowControl: false,
                        value: (T)value);

                if (value is string s)
                    return (
                        flowControl: false,
                        value: (T)(object)Guid.Parse(s));

                if (value is byte[] b)
                    return (
                        flowControl: false,
                        value: (T)(object)new Guid(b));
            }

            return (
                flowControl: true,
                value: default);
        }

        /// <summary>
        /// Attempts to convert the specified value to the given enum type, returning a tuple indicating whether flow
        /// control should continue and the converted value.
        /// </summary>
        /// <remarks>If the conversion type is not an enum, the method returns the default value for the
        /// type and signals that flow control should continue.</remarks>
        /// <typeparam name="T">The enum type to which the value is to be converted.</typeparam>
        /// <param name="value">The value to convert. Can be a string or a value compatible with the target enum type.</param>
        /// <param name="conversionType">The target enum type for conversion. Must be a valid enum type.</param>
        /// <returns>A tuple containing a boolean indicating whether flow control should continue and the converted enum value.
        /// If conversion is not possible, flow control is set to true and the value is the default for the type.</returns>
        private static (bool flowControl, T value) TryConvertEnum<T>(
            object value,
            Type conversionType)
        {
            if (conversionType.IsEnum)
            {
                if (value is string s)
                    return (
                        flowControl: false,
                        value: (T)Enum.Parse(
                            conversionType,
                            s,
                            true));

                return (
                    flowControl: false,
                    value: (T)Enum.ToObject(
                        conversionType,
                        value));
            }

            return (
                flowControl: true,
                value: default);
        }
    }
}