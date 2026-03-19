using Hydrix.Orchestrator.Caching;
using Hydrix.Orchestrator.Mapping;
using System;
using System.Collections.Generic;
using System.Data;

namespace Hydrix.Orchestrator.Metadata.Internals
{
    /// <summary>
    /// Provides factory methods for creating field readers that extract values of various data types from an
    /// IDataRecord instance.
    /// </summary>
    /// <remarks>FieldReaderFactory supports both nullable and non-nullable types, including enums. It returns
    /// default values for non-nullable types when database fields contain DBNull, and handles type conversion for enum
    /// fields. This class is intended for internal use to facilitate efficient and type-safe data extraction from
    /// database records.</remarks>
    internal static class FieldReaderFactory
    {
        /// <summary>
        /// Provides a mapping of supported data types to functions that retrieve values from an IDataRecord at a
        /// specified ordinal position.
        /// </summary>
        /// <remarks>This dictionary enables dynamic extraction of values from a data record based on the
        /// type specified. Each entry associates a Type with a function that reads the corresponding value from the
        /// IDataRecord. The mapping covers common primitive and framework types such as int, long, short, byte, bool,
        /// decimal, double, float, Guid, DateTime, and string.</remarks>
        private static readonly Dictionary<Type, Func<IDataRecord, int, object>> _baseReaders
            = new Dictionary<Type, Func<IDataRecord, int, object>>()
            {
                [typeof(bool)] = (record, ordinal) => record.GetBoolean(ordinal),
                [typeof(byte)] = (record, ordinal) => record.GetByte(ordinal),
                [typeof(int)] = (record, ordinal) => record.GetInt32(ordinal),
                [typeof(long)] = (record, ordinal) => record.GetInt64(ordinal),
                [typeof(short)] = (record, ordinal) => record.GetInt16(ordinal),
                [typeof(float)] = (record, ordinal) => record.GetFloat(ordinal),
                [typeof(double)] = (record, ordinal) => record.GetDouble(ordinal),
                [typeof(decimal)] = (record, ordinal) => record.GetDecimal(ordinal),
                [typeof(Guid)] = (record, ordinal) => record.GetGuid(ordinal),
                [typeof(DateTime)] = (record, ordinal) => record.GetDateTime(ordinal),
                [typeof(string)] = (record, ordinal) => record.GetString(ordinal),
            };

        /// <summary>
        /// Provides a reusable fallback field reader that retrieves raw values by ordinal.
        /// </summary>
        private static readonly Func<IDataRecord, int, object> _valueReader =
            (record, ordinal) => record.GetValue(ordinal);

        /// <summary>
        /// Creates a FieldReader delegate for the specified target type, supporting nullable types and enums.
        /// </summary>
        /// <remarks>If the target type is an enum, the underlying type is used to read the value, and the
        /// result is converted back to the enum type. If the target type is nullable, the FieldReader will return null
        /// for database null values.</remarks>
        /// <param name="targetType">The type for which the FieldReader is created. This can be a nullable type or an enum type.</param>
        /// <returns>A FieldReader delegate that reads values from a data record based on the specified target type.</returns>
        public static FieldReader Create(
            Type targetType)
        {
            var underlying = Nullable.GetUnderlyingType(targetType);
            var isNullable = underlying != null || !targetType.IsValueType;
            var type = underlying ?? targetType;

            object defaultValue = null;

            if (!isNullable)
                defaultValue = DefaultValueFactoryCache.Get(type)();

            var nullValue = isNullable ? null : defaultValue;

            if (type.IsEnum)
            {
                var enumUnderlying = Enum.GetUnderlyingType(type);
                var converter = EnumConverterCache.GetOrAdd(type);

                if (!_baseReaders.TryGetValue(enumUnderlying, out var enumReader))
                    enumReader = _valueReader;

                return (record, ordinal) =>
                {
                    if (record.IsDBNull(ordinal))
                        return nullValue;

                    var raw = enumReader(record, ordinal);

                    return raw == null
                        ? nullValue
                        : converter(raw);
                };
            }

            return _baseReaders.TryGetValue(type, out var reader)
                ? CreateTypedReader(nullValue, reader)
                : CreateTypedReader(nullValue, _valueReader);
        }

        /// <summary>
        /// Creates a delegate that reads a typed value from an IDataRecord, returning a specified value when the
        /// database field is null.
        /// </summary>
        /// <param name="nullValue">The value to return when the database field is null. This value is used in place of DBNull.</param>
        /// <param name="valueReader">A function that reads and converts the value from the IDataRecord for a given ordinal. This function is
        /// called when the field is not null.</param>
        /// <returns>A FieldReader delegate that returns the specified null value if the field is null, or the result of the
        /// valueReader function otherwise.</returns>
        private static FieldReader CreateTypedReader(
            object nullValue,
            Func<IDataRecord, int, object> valueReader)
            => (record, ordinal) =>
                record.IsDBNull(ordinal)
                    ? nullValue
                    : valueReader(record, ordinal);
    }
}