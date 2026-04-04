using Hydrix.Binders.Parameter;
using Hydrix.Caching;
using Hydrix.Caching.Entries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;

namespace Hydrix.Engines
{
    /// <summary>
    /// Provides methods for binding object properties as database command parameters and formatting parameter values
    /// for SQL queries. This class is intended for internal use within the parameter binding infrastructure.
    /// </summary>
    /// <remarks>The class includes a lock-free process-wide hot cache of the most recently used binder to optimize
    /// repeated accesses in high-throughput and async scenarios. It supports mapping public properties of objects to
    /// command parameters and formatting values for safe inclusion in SQL statements.</remarks>
    internal static class ParameterEngine
    {
        /// <summary>
        /// Provides a culture-invariant format provider for parameter value rendering.
        /// </summary>
        private static readonly IFormatProvider InvariantCulture = CultureInfo.InvariantCulture;

        /// <summary>
        /// Holds the most recently used parameter-binder cache entry in the process-wide hot cache.
        /// </summary>
        /// <remarks>This field stores the parameter-type/binder pair as a single immutable object so volatile reads
        /// and writes remain atomically consistent under concurrent access.</remarks>
        private static ParameterBinderCacheEntry _lastBinderCache;

        /// <summary>
        /// Binds parameters to the specified database command by extracting values from the provided object or
        /// collection.
        /// </summary>
        /// <remarks>If the parameters argument is an object, its public properties are mapped to command
        /// parameters using the specified prefix. If it is an IDataParameter or a collection of IDataParameter, those
        /// are added directly to the command. This method does not clear existing parameters from the
        /// command.</remarks>
        /// <param name="command">The database command to which parameters will be added. Must not be null.</param>
        /// <param name="parameters">An object containing parameter values, a single IDataParameter, or an enumerable collection of
        /// IDataParameter instances. If null, no parameters are added.</param>
        /// <param name="parameterPrefix">The prefix to use for parameter names when binding properties from the object.</param>
        public static void BindParametersFromObject(
            IDbCommand command,
            object parameters,
            string parameterPrefix)
        {
            switch (parameters)
            {
                case null:
                    return;

                case IDataParameter dbParam:
                    command.Parameters.Add(dbParam);
                    return;

                case IEnumerable<IDataParameter> dbParams:
                    {
                        foreach (var parameter in dbParams)
                            command.Parameters.Add(parameter);
                        return;
                    }

                default:
                    {
                        var binder = GetOrAddBinder(
                        parameters.GetType());

                        binder.Bind(
                            command,
                            parameters,
                            parameterPrefix,
                            AddParameter);
                        break;
                    }
            }
        }

        /// <summary>
        /// Retrieves a cached parameter object binder for the specified parameter type, or creates and caches a new
        /// binder if one does not already exist.
        /// </summary>
        /// <remarks>This method optimizes binder retrieval by caching the most recently used binder.
        /// Subsequent calls with the same parameter type return the cached binder, improving performance for repeated
        /// accesses.</remarks>
        /// <param name="parameterType">The type of the parameter for which a binder is required. Cannot be null.</param>
        /// <returns>A parameter object binder associated with the specified parameter type.</returns>
        private static ParameterObjectBinder GetOrAddBinder(
            Type parameterType)
        {
            var cachedEntry = Volatile.Read(ref _lastBinderCache);
            if (cachedEntry != null &&
                ReferenceEquals(
                    cachedEntry.ParameterType,
                    parameterType))
            {
                return cachedEntry.Binder;
            }

            var binder = ParameterBinderCache.GetOrAdd(
                parameterType);

            Volatile.Write(
                ref _lastBinderCache,
                new ParameterBinderCacheEntry(
                    parameterType,
                    binder));

            return binder;
        }

        /// <summary>
        /// Adds a parameter with the specified name and value to the given database command.
        /// </summary>
        /// <param name="command">The database command to which the parameter will be added. Cannot be null.</param>
        /// <param name="name">The name of the parameter to add. Cannot be null or empty.</param>
        /// <param name="value">The value to assign to the parameter. If null, the parameter value is set to <see cref="DBNull.Value"/>.</param>
        private static void AddParameter(
            IDbCommand command,
            string name,
            object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;

            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Formats a parameter value for safe inclusion in a database query string.
        /// </summary>
        /// <remarks>String values are enclosed in single quotes. DateTime values are formatted as
        /// 'yyyy-MM-dd HH:mm:ss.fff' and enclosed in single quotes. Guid values are enclosed in single quotes. Boolean
        /// values are represented as '1' for <see langword="true"/> and '0' for <see langword="false"/>. All other
        /// types are converted to their string representation. This method does not perform SQL injection protection;
        /// ensure that values are properly sanitized before use in queries.</remarks>
        /// <param name="value">The value to format. Can be a string, DateTime, Guid, bool, or any other object. If the value is null or
        /// represents a database null, it is treated as a SQL NULL.</param>
        /// <returns>A string representation of the parameter value, formatted for use in a database query. Returns the string
        /// 'NULL' for null or database null values.</returns>
        internal static string FormatParameterValue(
            object value)
        {
            if (value == null || value == DBNull.Value)
                return "NULL";

            return value switch
            {
                string s => $"'{EscapeSqlLiteral(s)}'",
                DateTime d => $"'{d:yyyy-MM-dd HH:mm:ss.fff}'",
                DateTimeOffset dto => $"'{dto:yyyy-MM-dd HH:mm:ss.fff zzz}'",
                Guid g => $"'{g}'",
                bool b => b ? "1" : "0",
                IFormattable formattable => formattable.ToString(null, InvariantCulture),
                _ => value.ToString()
            };
        }

        /// <summary>
        /// Escapes SQL literal text by doubling single quotes.
        /// </summary>
        /// <param name="value">The text to escape.</param>
        /// <returns>The escaped text safe for SQL literal rendering.</returns>
        private static string EscapeSqlLiteral(
            string value)
            => string.IsNullOrEmpty(value)
                ? value
                : value.Replace("'", "''");
    }
}
