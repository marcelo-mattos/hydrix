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
        private static readonly IReadOnlyDictionary<Type, Func<IDataRecord, int, object>> _baseReaders
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

            if (type.IsEnum)
            {
                var enumUnderlying = Enum.GetUnderlyingType(type);

                if (!_baseReaders.TryGetValue(enumUnderlying, out var enumReader))
                    enumReader = (record, ordinal) => record.GetValue(ordinal);

                return (record, ordinal) =>
                {
                    if (record.IsDBNull(ordinal))
                        return isNullable ? null : defaultValue;

                    var raw = enumReader(record, ordinal);
                    return Enum.ToObject(type, raw);
                };
            }

            if (_baseReaders.TryGetValue(type, out var reader))
            {
                return (record, ordinal) =>
                    record.IsDBNull(ordinal)
                        ? (isNullable ? null : defaultValue)
                        : reader(record, ordinal);
            }

            return (record, ordinal) =>
                record.IsDBNull(ordinal)
                    ? null
                    : record.GetValue(ordinal);
        }
    }
}