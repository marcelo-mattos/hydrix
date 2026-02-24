using System;
using System.Collections.Generic;
using System.Data;

namespace Hydrix.Orchestrator.Binders.Parameter
{
    /// <summary>
    /// Binds parameters from a specified object to a database command using defined bindings.
    /// </summary>
    /// <remarks>This class is designed to facilitate the binding of parameters to an IDbCommand by iterating
    /// over an array of ParameterObjectBinding instances. Each binding defines how to extract a value from the
    /// parameters object. The method allows for customization of parameter names through a prefix and supports various
    /// rules for handling null values and collections.</remarks>
    internal sealed class ParameterObjectBinder
    {
        /// <summary>
        /// Gets the array of parameter object bindings used for processing input parameters.
        /// </summary>
        /// <remarks>This field is initialized with the parameter bindings necessary for the operation of
        /// the associated method or class. It is intended for internal use and should not be modified
        /// directly.</remarks>
        private readonly ParameterObjectBinding[] _bindings;

        /// <summary>
        /// Initializes a new instance of the ParameterObjectBinder class using the specified parameter bindings.
        /// </summary>
        /// <param name="bindings">An array of ParameterObjectBinding instances that define the parameter bindings to be used by the binder.
        /// Cannot be null.</param>
        public ParameterObjectBinder(
            ParameterObjectBinding[] bindings)
            => _bindings = bindings;

        /// <summary>
        /// Binds the specified parameters to the provided database command using the given prefix and parameter
        /// addition logic.
        /// </summary>
        /// <remarks>This method iterates over the available parameter bindings and applies the provided
        /// logic for adding each parameter to the command. It is the caller's responsibility to ensure that the
        /// parameters object contains the expected properties and that the addParameter action correctly handles null
        /// values and collections as needed.</remarks>
        /// <param name="command">The database command to which the parameters will be bound. Must not be null.</param>
        /// <param name="parameters">An object containing the parameter values to bind. The properties of this object are used to retrieve
        /// parameter values.</param>
        /// <param name="prefix">A string prefix to prepend to each parameter name when binding to the command. Can be empty if no prefix is
        /// required.</param>
        /// <param name="addParameter">An action that defines how to add each parameter to the command. Receives the command, the full parameter
        /// name (including prefix), and the parameter value.</param>
        public void Bind(
            IDbCommand command,
            object parameters,
            string prefix,
            Action<IDbCommand, string, object> addParameter)
        {
            for (int index = 0; index < _bindings.Length; index++)
            {
                var binder = _bindings[index];
                var value = binder.Getter(parameters);

                if (IsEnumerableParameter(value))
                {
                    ExpandEnumerableParameter(
                        command,
                        binder.Name,
                        prefix,
                        (System.Collections.IEnumerable)value);

                    continue;
                }

                addParameter(command, $"{prefix}{binder.Name}", value);
            }
        }

        /// <summary>
        /// Determines whether the specified object is an enumerable collection, excluding strings and byte arrays.
        /// </summary>
        /// <remarks>This method returns false for strings and byte arrays, as they are not considered
        /// enumerable collections.</remarks>
        /// <param name="value">The object to evaluate for enumerable status. Must not be null.</param>
        /// <returns>true if the object is an enumerable collection other than a string or byte array; otherwise, false.</returns>
        private static bool IsEnumerableParameter(object value)
        {
            if (value == null)
                return false;

            if (value is string || value is byte[])
                return false;

            return value is System.Collections.IEnumerable;
        }

        /// <summary>
        /// Expands an enumerable parameter into individual parameters for a database command, replacing the placeholder
        /// in the command text with the generated parameter names.
        /// </summary>
        /// <remarks>This method generates unique parameter names by combining the specified prefix and
        /// base name with an index. Null values in the collection are assigned as DBNull.Value. The method updates the
        /// command text by replacing the original parameter placeholder with a comma-separated list of the generated
        /// parameter names.</remarks>
        /// <param name="command">The database command to which the parameters will be added.</param>
        /// <param name="name">The base name for the parameters to be generated and inserted into the command text.</param>
        /// <param name="prefix">A prefix to prepend to each generated parameter name, allowing for grouping or disambiguation.</param>
        /// <param name="values">An enumerable collection of values to be added as individual parameters. Each value is assigned to a
        /// separate parameter.</param>
        private static void ExpandEnumerableParameter(
            IDbCommand command,
            string name,
            string prefix,
            System.Collections.IEnumerable values)
        {
            var parameterNames = new List<string>();
            int index = 0;

            foreach (var item in values)
            {
                string parameterName = $"{prefix}{name}_{index++}";
                parameterNames.Add(parameterName);

                var parameter = command.CreateParameter();
                parameter.ParameterName = parameterName;
                parameter.Value = item ?? DBNull.Value;

                command.Parameters.Add(parameter);
            }

            command.CommandText = command.CommandText.Replace(
                $"{prefix}{name}",
                string.Join(", ", parameterNames)
            );
        }
    }
}
