using System;
using System.Data;

namespace Hydrix.Orchestrator.Binders.Procedure
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
        /// Binds the parameters of a stored procedure to the specified command object using values from the provided
        /// procedure instance.
        /// </summary>
        /// <remarks>This method iterates through the defined parameters and uses the provided delegate to
        /// add each one to the command. The prefix is applied to each parameter name, and the parameter's direction,
        /// type, and size are set according to the parameter definition. The caller is responsible for ensuring that
        /// the addParameter delegate correctly handles null values and type conversions as required by the database
        /// provider.</remarks>
        /// <param name="command">The command object to which the parameters will be added. Must not be null.</param>
        /// <param name="procedureInstance">An object instance containing the values to assign to each parameter. Typically, this is an instance of the
        /// procedure's parameter class.</param>
        /// <param name="prefix">A string to prepend to each parameter name when adding it to the command. Can be used to match database
        /// parameter naming conventions.</param>
        /// <param name="addParameter">An action delegate that adds a parameter to the command. Receives the command, parameter name, value,
        /// direction, type, and size as arguments.</param>
        public void BindParameters(
            IDbCommand command,
            object procedureInstance,
            string prefix,
            Action<IDbCommand, string, object, ParameterDirection, int> addParameter)
        {
            for (var index = 0; index < _parameters.Length; index++)
            {
                var parameter = _parameters[index];
                var value = parameter.Getter(procedureInstance) ?? DBNull.Value;

                addParameter(
                    command,
                    $"{prefix}{parameter.Name}",
                    value,
                    parameter.Direction,
                    parameter.DbType);
            }
        }
    }
}