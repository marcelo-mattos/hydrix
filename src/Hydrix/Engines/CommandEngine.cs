using Hydrix.Configuration;
using Hydrix.Orchestrator.Binders.Procedure;
using Hydrix.Orchestrator.Caching;
using Hydrix.Schemas.Contract;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Text;

namespace Hydrix.Engines
{
    /// <summary>
    /// Provides static methods for creating and configuring database command objects with parameter binding,
    /// transaction context, and optional logging support.
    /// </summary>
    /// <remarks>The CommandEngine class is intended for internal use to facilitate the construction of
    /// IDbCommand instances in a consistent and extensible manner. It supports various command types, parameterization
    /// strategies, and integrates with logging for diagnostic purposes. All methods require an open database connection
    /// and may throw exceptions if preconditions are not met.</remarks>
    internal static class CommandEngine
    {
        /// <summary>
        /// Holds the last procedure type used in the current thread context.
        /// </summary>
        /// <remarks>This field is marked with the [ThreadStatic] attribute, meaning its value is unique
        /// to each thread. It is intended for internal tracking of procedure types and should not be accessed directly
        /// by consumers.</remarks>
        [ThreadStatic]
        private static Type _lastProcedureType;

        /// <summary>
        /// Holds the last used procedure binder for the current thread.
        /// </summary>
        /// <remarks>This field is marked with the ThreadStatic attribute, meaning its value is unique per
        /// thread. It is intended for internal tracking of procedure binding operations within a thread
        /// context.</remarks>
        [ThreadStatic]
        private static ProcedureBinder _lastProcedureBinder;

        /// <summary>
        /// Holds the type of the last parameter used to set a provider, specific to the current thread context.
        /// </summary>
        /// <remarks>This field is marked with the [ThreadStatic] attribute, meaning its value is unique
        /// per thread. It is intended for internal tracking of provider setter parameter types and should not be
        /// accessed directly by consumers.</remarks>
        [ThreadStatic]
        private static Type _lastProviderSetterParameterType;

        /// <summary>
        /// Stores the last provider-specific parameter setter action used in the current thread context.
        /// </summary>
        /// <remarks>This field is marked with the [ThreadStatic] attribute, ensuring that each thread has
        /// its own instance. It is intended for internal caching of parameter setter delegates to optimize repeated
        /// parameter assignments in data operations.</remarks>
        [ThreadStatic]
        private static Action<IDataParameter, int> _lastProviderSetter;

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <param name="connection">The database connection to use for creating the command. Must be an open connection.</param>
        /// <param name="transaction">An optional database transaction within which the command will be executed. If null, the current active
        /// transaction is used if available.</param>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="parameterPrefix">The prefix to use for parameter names (e.g., "@").
        /// This is used to ensure that parameter names are correctly formatted for the target database.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute
        /// a command and generating an error.</param>
        /// <param name="logger">An optional logger instance to use for logging command execution details. If null, logging is skipped.</param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        public static IDbCommand CreateCommand(
            in IDbConnection connection,
            IDbTransaction transaction,
            CommandType commandType,
            string sql,
            object parameters,
            string parameterPrefix = null,
            int? timeout = null,
            ILogger logger = null)
            => CreateCommandCore(
                connection,
                transaction,
                commandType,
                sql,
                command => ParameterEngine.BindParametersFromObject(
                    command,
                    parameters,
                    parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix),
                timeout,
                logger);

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to
        /// System.Data.DataSet columns; and is implemented by .NET Framework data providers that
        /// access data sources.
        /// </typeparam>
        /// <param name="connection">The database connection to use for creating the command. Must be an open connection.</param>
        /// <param name="transaction">An optional database transaction within which the command will be executed. If null, the current active
        /// transaction is used if available.</param>
        /// <param name="procedure">
        /// Represents a Sql Entity that holds the data parameters to be executed by the connection command.
        /// </param>
        /// <param name="parameterPrefix">The prefix to use for parameter names (e.g., "@").
        /// This is used to ensure that parameter names are correctly formatted for the target database.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute
        /// a command and generating an error.</param>
        /// <param name="logger">An optional logger instance to use for logging command execution details. If null, logging is skipped.</param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        public static IDbCommand CreateCommand<TDataParameterDriver>(
            in IDbConnection connection,
            IDbTransaction transaction,
            IProcedure<TDataParameterDriver> procedure,
            string parameterPrefix = null,
            int? timeout = null,
            ILogger logger = null)
            where TDataParameterDriver : IDataParameter, new()
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(procedure);
#else
            if (procedure == null)
                throw new ArgumentNullException(nameof(procedure));
#endif
            if (connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Database connection is not open.");

            var procedureType = procedure.GetType();
            var binder = GetOrAddProcedureBinder(procedureType);

            var command = connection.CreateCommand();
            binder.ApplyCommand(command);
            command.CommandTimeout = timeout ?? HydrixConfiguration.Options.CommandTimeout;
            command.Transaction = transaction;
            var providerDbTypeSetter = GetOrAddProviderDbTypeSetter(typeof(TDataParameterDriver));

            binder.BindParameters(
                command,
                procedure,
                parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix,
                (cmd, name, value, direction, dbType) =>
                {
                    var dataParameter = new TDataParameterDriver
                    {
                        ParameterName = name,
                        Direction = direction,
                        Value = value
                    };

                    if (Enum.IsDefined(typeof(DbType), dbType))
                    {
                        dataParameter.DbType = (DbType)dbType;
                    }
                    else
                    {
                        providerDbTypeSetter?.Invoke(
                            dataParameter,
                            dbType);
                    }

                    cmd.Parameters.Add(dataParameter);
                });

            LogCommand(logger, command);

            return command;
        }

        /// <summary>
        /// Retrieves a cached procedure binder for the specified procedure type, or creates and caches a new binder if
        /// one does not already exist.
        /// </summary>
        /// <remarks>This method optimizes repeated access by caching the most recently used procedure
        /// binder. If the binder for the given type is already cached, it is returned directly; otherwise, a new binder
        /// is created and stored for future use.</remarks>
        /// <param name="procedureType">The type of the procedure for which to obtain a binder. Cannot be null.</param>
        /// <returns>A procedure binder instance associated with the specified procedure type.</returns>
        private static ProcedureBinder GetOrAddProcedureBinder(
            Type procedureType)
        {
            if (ReferenceEquals(
                    _lastProcedureType,
                    procedureType) &&
                _lastProcedureBinder != null)
            {
                return _lastProcedureBinder;
            }

            var binder = ProcedureBinderCache.GetOrAdd(
                procedureType);

            _lastProcedureType = procedureType;
            _lastProcedureBinder = binder;

            return binder;
        }

        /// <summary>
        /// Retrieves a cached delegate that sets the provider-specific database type for a parameter, or adds a new
        /// setter to the cache if one does not exist.
        /// </summary>
        /// <remarks>This method uses an internal cache to optimize retrieval of provider-specific
        /// database type setters. Repeated calls with the same parameter type are efficient due to caching.</remarks>
        /// <param name="parameterType">The type of the parameter for which to obtain or add a provider-specific database type setter. Cannot be
        /// null.</param>
        /// <returns>An action delegate that sets the provider-specific database type for the specified parameter type.</returns>
        private static Action<IDataParameter, int> GetOrAddProviderDbTypeSetter(
            Type parameterType)
        {
            if (ReferenceEquals(
                    _lastProviderSetterParameterType,
                    parameterType) &&
                _lastProviderSetter != null)
            {
                return _lastProviderSetter;
            }

            var setter = ProviderDbTypeSetterCache.GetOrAdd(parameterType);

            _lastProviderSetterParameterType = parameterType;
            _lastProviderSetter = setter;

            return setter;
        }

        /// <summary>
        /// Creates and configures a database command with the specified command type, SQL statement, parameter binding
        /// action, and transaction context.
        /// </summary>
        /// <param name="connection">The database connection to use for creating the command. Must be an open connection.</param>
        /// <param name="transaction">An optional database transaction within which the command will be executed. If null, the current active
        /// transaction is used if available.</param>
        /// <param name="commandType">The type of command to execute, such as text, stored procedure, or table direct.</param>
        /// <param name="sql">The SQL statement or stored procedure name to execute against the database.</param>
        /// <param name="parameterBinder">An action that binds parameters to the command. This allows for dynamic parameterization of the command
        /// before execution. May be null if no parameters are required.</param>
        /// <param name="timeout">An optional command timeout, in seconds, to use for this command.
        /// If null, the default timeout configured for the Materializer is used.</param>
        /// <param name="logger">An optional logger instance to use for logging command execution details. If null, logging is skipped.</param>
        /// <returns>An IDbCommand instance configured with the specified command type, SQL, parameters, and transaction context.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the database connection is not open when attempting to create the command.</exception>
        internal static IDbCommand CreateCommandCore(
            in IDbConnection connection,
            IDbTransaction transaction,
            CommandType commandType,
            string sql,
            Action<IDbCommand> parameterBinder,
            int? timeout = null,
            ILogger logger = null)
        {
            if (connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Database connection is not open.");

            var command = connection.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = sql;
            command.CommandTimeout = timeout ?? HydrixConfiguration.Options.CommandTimeout;
            command.Transaction = transaction;

            parameterBinder?.Invoke(command);
            LogCommand(logger, command);

            return command;
        }

        /// <summary>
        /// Logs the details of the specified database command, including its SQL statement and parameters, for
        /// diagnostic purposes.
        /// </summary>
        /// <remarks>Logging occurs only if a logger is initialized. The logged information includes the
        /// command text and all associated parameters, which can assist in debugging and monitoring database
        /// operations.</remarks>
        /// <param name="logger">The logger instance to use for logging the command details. If null, logging is skipped.</param>
        /// <param name="command">The database command whose execution details are to be logged. Must not be null.</param>
        private static void LogCommand(
            ILogger logger,
            IDbCommand command)
        {
            if (logger is null)
                return;

            if (!logger.IsEnabled(LogLevel.Information))
                return;

            var logMessage = new StringBuilder();
            logMessage.AppendLine("Executing DbCommand");
            logMessage.AppendLine(command.CommandText);

            if (command.Parameters.Count > 0)
            {
                logMessage.AppendLine("Parameters:");
                foreach (IDataParameter parameter in command.Parameters)
                {
                    logMessage.AppendLine(
                        $"  {parameter.ParameterName} = {ParameterEngine.FormatParameterValue(parameter.Value)} ({parameter.DbType})"
                    );
                }
            }

            logger.LogInformation("{LogMessage}", logMessage.ToString());
        }
    }
}