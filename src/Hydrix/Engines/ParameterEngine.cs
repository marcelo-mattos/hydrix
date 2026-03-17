using Hydrix.Orchestrator.Caching;
using System;
using System.Collections.Generic;
using System.Data;

namespace Hydrix.Engines
{
    internal static class ParameterEngine
    {
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

            var binder = ParameterBinderCache.GetOrAdd(
                parameters.GetType());

            binder.Bind(
                command,
                parameters,
                parameterPrefix,
                AddParameter);
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