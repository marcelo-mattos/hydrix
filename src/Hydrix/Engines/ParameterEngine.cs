using Hydrix.Orchestrator.Binders.Parameter;
using Hydrix.Orchestrator.Caching;
using System;
using System.Collections.Generic;
using System.Data;

namespace Hydrix.Engines
{
    /// <summary>
    /// Provides methods for binding object properties as database command parameters and formatting parameter values
    /// for SQL queries. This class is intended for internal use within the parameter binding infrastructure.
    /// </summary>
    /// <remarks>The class includes thread-specific caching of parameter binders to optimize performance for
    /// repeated accesses. It supports mapping public properties of objects to command parameters and formatting values
    /// for safe inclusion in SQL statements. All methods and fields are intended for internal use and are not
    /// thread-safe beyond the thread-specific caching mechanism.</remarks>
    internal static class ParameterEngine
    {
        /// <summary>
        /// Holds the last parameter type used in the current thread context.
        /// </summary>
        /// <remarks>This field is marked with the [ThreadStatic] attribute, meaning its value is unique
        /// to each thread. It is intended for internal tracking of parameter types within thread-specific
        /// operations.</remarks>
        [ThreadStatic]
        private static Type _lastParameterType;

        /// <summary>
        /// Holds the last instance of the parameter object binder used in the current thread.
        /// </summary>
        /// <remarks>This field is marked with the [ThreadStatic] attribute, ensuring that each thread has
        /// its own independent value. It is intended for internal tracking of binder state within thread-specific
        /// operations.</remarks>
        [ThreadStatic]
        private static ParameterObjectBinder _lastBinder;

        /// <summary>
        /// Binds the properties of the specified parameters object as parameters to the given database command.
        /// </summary>
        /// <remarks>This method uses a parameter binder appropriate for the type of the provided
        /// parameters object to map its properties to command parameters. Only public properties of the parameters
        /// object are considered.</remarks>
        /// <param name="command">The database command to which parameters will be added. Must not be null.</param>
        /// <param name="parameters">An object whose public properties represent the parameters to bind to the command. If null, no parameters
        /// are bound.</param>
        /// <param name="parameterPrefix">The prefix to use for parameter names (e.g., "@").
        /// This is used to ensure that parameter names are correctly formatted for the target database.</param>
        public static void BindParametersFromObject(
            IDbCommand command,
            object parameters,
            string parameterPrefix)
        {
            if (parameters == null)
                return;

            if (parameters is IDataParameter dbParam)
            {
                command.Parameters.Add(dbParam);
                return;
            }

            if (parameters is IEnumerable<IDataParameter> dbParams)
            {
                foreach (var parameter in dbParams)
                    command.Parameters.Add(parameter);
                return;
            }

            var binder = GetOrAddBinder(
                parameters.GetType());

            binder.Bind(
                command,
                parameters,
                parameterPrefix,
                AddParameter);
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
            if (ReferenceEquals(
                    _lastParameterType,
                    parameterType) &&
                _lastBinder != null)
            {
                return _lastBinder;
            }

            var binder = ParameterBinderCache.GetOrAdd(
                parameterType);

            _lastParameterType = parameterType;
            _lastBinder = binder;

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
                string s => $"'{s}'",
                DateTime d => $"'{d:yyyy-MM-dd HH:mm:ss.fff}'",
                Guid g => $"'{g}'",
                bool b => b ? "1" : "0",
                _ => value.ToString()
            };
        }
    }
}