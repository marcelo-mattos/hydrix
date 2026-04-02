using Hydrix.Orchestrator.Binders.Parameter;
using Hydrix.Orchestrator.Caching;
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
        /// Holds the last parameter type used in the process-wide hot cache.
        /// </summary>
        /// <remarks>This field is used with volatile reads/writes as a lock-free hot cache key for binder reuse.</remarks>
        private static Type _lastParameterType;

        /// <summary>
        /// Holds the last instance of the parameter object binder used in the process-wide hot cache.
        /// </summary>
        /// <remarks>This field is used with volatile reads/writes as a lock-free hot cache value for binder reuse.</remarks>
        private static ParameterObjectBinder _lastBinder;

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
            var cachedType = Volatile.Read(ref _lastParameterType);
            var cachedBinder = Volatile.Read(ref _lastBinder);

            if (ReferenceEquals(
                    cachedType,
                    parameterType) &&
                cachedBinder != null)
            {
                return cachedBinder;
            }

            var binder = ParameterBinderCache.GetOrAdd(
                parameterType);

            Volatile.Write(
                ref _lastBinder,
                binder);

            Volatile.Write(
                ref _lastParameterType,
                parameterType);

            return binder;
        }

        /// <summary>
        /// Creates and adds a scalar SQL parameter to the specified command.
        ///
        /// The parameter name is automatically prefixed with '@' and the provided value is assigned
        /// directly, falling back to <see cref="DBNull.Value"/> when the value is <c>null</c>.
        ///
        /// This method is intended for simple (non-collection) values and is used internally by the
        /// parameter binding infrastructure, including dynamic object-based parameter mapping and
        /// SQL <c>IN</c> expansion support.
        /// </summary>
        /// <param name="command">
        /// The <see cref="IDbCommand"/> instance to which the parameter will be added.
        /// </param>
        /// <param name="name">The logical parameter name without the '@' prefix.</param>
        /// <param name="value">
        /// The value to assign to the parameter, or <c>null</c> to represent a database NULL.
        /// </param>
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
