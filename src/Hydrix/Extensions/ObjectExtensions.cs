using System;

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
                return default;

            var targetType = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            var conversionType = underlyingType ?? targetType;

            if (conversionType == typeof(Guid))
            {
                if (value is Guid guid)
                    return (T)(object)guid;

                if (value is string guidText)
                    return (T)(object)Guid.Parse(guidText);
            }

            if (value is T typedValue)
                return typedValue;

            if (conversionType.IsEnum)
            {
                if (value is string enumName)
                    return (T)Enum.Parse(conversionType, enumName, ignoreCase: true);

                var enumUnderlyingType = Enum.GetUnderlyingType(conversionType);
                var numericValue = Convert.ChangeType(value, enumUnderlyingType);
                return (T)Enum.ToObject(conversionType, numericValue);
            }

            var convertedValue = Convert.ChangeType(value, conversionType);
            return (T)convertedValue;
        }
    }
}