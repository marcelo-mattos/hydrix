using Hydrix.Extensions;
using System;
using System.Data;

namespace Hydrix.Binders.Procedure
{
    /// <summary>
    /// Encapsulates the binding of a stored procedure's command type, command text, and parameter definitions for
    /// execution against a database command.
    /// </summary>
    /// <remarks>Use this class to configure an IDbCommand for executing a stored procedure, including setting
    /// the command type, command text, and binding all required parameters. The ProcedureBinder is typically
    /// constructed with the metadata for a specific stored procedure and can then be used to apply these settings to
    /// any compatible IDbCommand instance. This class is intended for internal use within data access layers that
    /// automate stored procedure invocation.</remarks>
    internal sealed class ProcedureBinder
    {
        /// <summary>
        /// Represents the command type associated with the database operation.
        /// </summary>
        private readonly CommandType _commandType;

        /// <summary>
        /// Gets the command text used for executing database operations.
        /// </summary>
        private readonly string _commandText;

        /// <summary>
        /// Gets the collection of parameters used for binding in the procedure.
        /// </summary>
        /// <remarks>This field is read-only and initialized during the construction of the binding. It
        /// contains the parameters that define the behavior of the procedure being executed.</remarks>
        private readonly ProcedureParameterBinding[] _parameters;

        /// <summary>
        /// Initializes a new instance of the ProcedureBinder class with the specified command type, command text, and
        /// parameter bindings.
        /// </summary>
        /// <remarks>Use this constructor to create a ProcedureBinder that encapsulates a database command
        /// and its associated parameters for execution. Ensure that the parameters array matches the expected
        /// parameters for the specified command.</remarks>
        /// <param name="commandType">The type of database command to execute. This value determines whether the command is a text command, a
        /// stored procedure, or another supported command type.</param>
        /// <param name="commandText">The SQL statement or stored procedure name to be executed against the database.</param>
        /// <param name="parameters">An array of ProcedureParameterBinding objects that define the parameters to be supplied with the command.
        /// May be empty if the command does not require parameters.</param>
        public ProcedureBinder(
            CommandType commandType,
            string commandText,
            ProcedureParameterBinding[] parameters)
        {
            _commandType = commandType;
            _commandText = commandText;
            _parameters = parameters;
        }

        /// <summary>
        /// Applies the configured command type and command text to the specified database command.
        /// </summary>
        /// <param name="command">The database command to which the command type and command text will be applied. Cannot be null.</param>
        public void ApplyCommand(
            IDbCommand command)
        {
            command.CommandType = _commandType;
            command.CommandText = _commandText;
        }

        /// <summary>
        /// Binds the parameters of a stored procedure to the specified command object using a caller-provided callback.
        /// </summary>
        /// <remarks>This overload preserves the delegate-based binding path for callers that need to inspect
        /// or customize parameter creation. The callback receives the command, parameter name, value, direction, and
        /// database type for each parameter.</remarks>
        /// <param name="command">The command object to which the parameters will be added. Must not be null.</param>
        /// <param name="procedureInstance">An object instance containing the values to assign to each parameter. Typically, this is an instance of the
        /// procedure's parameter class.</param>
        /// <param name="prefix">A string to prepend to each parameter name when adding it to the command. Can be used to match database
        /// parameter naming conventions.</param>
        /// <param name="addParameter">An action delegate that adds a parameter to the command. Receives the command, parameter name, value,
        /// direction, and database type as arguments.</param>
        public void BindParameters(
            IDbCommand command,
            object procedureInstance,
            string prefix,
            Action<IDbCommand, string, object, ParameterDirection, int> addParameter)
        {
            foreach (var parameter in _parameters)
            {
                addParameter(
                    command,
                    $"{prefix}{parameter.Name}",
                    parameter.Getter(procedureInstance) ?? DBNull.Value,
                    parameter.Direction,
                    parameter.DbType);
            }
        }

        /// <summary>
        /// Binds the parameters of a stored procedure to the specified command object using values from the provided
        /// procedure instance.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">The concrete parameter type created for each stored procedure argument.</typeparam>
        /// <remarks>This method iterates through the cached parameter metadata, creates a parameter instance
        /// for each definition, and appends it directly to the command without allocating a per-call closure. The
        /// prefix is applied to each parameter name, and the parameter's direction and type are set according to the
        /// parameter definition. Provider-specific type handling is delegated to
        /// <paramref name="providerDbTypeSetter"/> when the declared type falls outside the standard
        /// <see cref="DbType"/> range.</remarks>
        /// <param name="command">The command object to which the parameters will be added. Must not be null.</param>
        /// <param name="procedureInstance">An object instance containing the values to assign to each parameter. Typically, this is an instance of the
        /// procedure's parameter class.</param>
        /// <param name="prefix">A string to prepend to each parameter name when adding it to the command. Can be used to match database
        /// parameter naming conventions.</param>
        /// <param name="providerDbTypeSetter">The provider-specific setter used when a parameter maps to a database type outside the standard
        /// <see cref="DbType"/> enumeration range.</param>
        public void BindParameters<TDataParameterDriver>(
            IDbCommand command,
            object procedureInstance,
            string prefix,
            Action<IDataParameter, int> providerDbTypeSetter)
            where TDataParameterDriver : IDataParameter, new()
        {
            foreach (var parameter in _parameters)
            {
                AddParameter<TDataParameterDriver>(
                    command,
                    prefix,
                    procedureInstance,
                    parameter,
                    providerDbTypeSetter);
            }
        }

        /// <summary>
        /// Creates and appends a parameter for a single stored procedure argument.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">The concrete parameter type created for the current argument.</typeparam>
        /// <param name="command">The command that receives the created parameter.</param>
        /// <param name="prefix">The prefix applied to the parameter name.</param>
        /// <param name="procedureInstance">The stored procedure instance that supplies the runtime value.</param>
        /// <param name="parameter">The cached parameter metadata describing the current argument.</param>
        /// <param name="providerDbTypeSetter">The provider-specific type setter used for non-standard database types.</param>
        private static void AddParameter<TDataParameterDriver>(
            IDbCommand command,
            string prefix,
            object procedureInstance,
            ProcedureParameterBinding parameter,
            Action<IDataParameter, int> providerDbTypeSetter)
            where TDataParameterDriver : IDataParameter, new()
        {
            var dataParameter = new TDataParameterDriver
            {
                ParameterName = $"{prefix}{parameter.Name}",
                Direction = parameter.Direction,
                Value = parameter.Getter(procedureInstance) ?? DBNull.Value
            };

            if (parameter.DbType.IsStandardDbType())
            {
                dataParameter.DbType = (DbType)parameter.DbType;
            }
            else
            {
                providerDbTypeSetter(
                    dataParameter,
                    parameter.DbType);
            }

            command.Parameters.Add(
                dataParameter);
        }
    }
}
