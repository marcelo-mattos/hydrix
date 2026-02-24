using Hydrix.Attributes.Parameters;
using Hydrix.Attributes.Schemas;
using Hydrix.Schemas.Contract;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// Provides a centralized facility for creating and configuring database command objects, supporting parameter
    /// binding, transaction management, and extensibility for various command execution scenarios.
    /// </summary>
    /// <remarks>The Materializer class implements the Contract.IMaterializer interface and serves as the
    /// primary entry point for generating IDbCommand instances tailored to specific SQL operations. It abstracts common
    /// command creation logic, including validation of connection state, transaction association, and parameter
    /// materialization, to promote consistency and code reuse. The class supports multiple overloads for command
    /// creation, enabling flexible parameter binding strategies such as object-based, attribute-driven, or
    /// provider-specific approaches. Diagnostic features, such as SQL command logging, are integrated to aid debugging
    /// and traceability. This design facilitates the introduction of cross-cutting concerns (e.g., logging, metrics) in
    /// a single, maintainable location. Thread safety is ensured for connection and transaction operations. Users
    /// should avoid enabling SQL logging in production environments that handle sensitive data unless appropriate
    /// controls are in place.</remarks>
    public partial class Materializer :
        Contract.IMaterializer
    {
        /// <summary>
        /// Logs the details of the specified database command, including its SQL statement and parameters, for
        /// diagnostic purposes.
        /// </summary>
        /// <remarks>Logging occurs only if a logger is initialized. The logged information includes the
        /// command text and all associated parameters, which can assist in debugging and monitoring database
        /// operations.</remarks>
        /// <param name="command">The database command whose execution details are to be logged. Must not be null.</param>
        private void LogCommand(IDbCommand command)
        {
            if (_logger is null)
                return;

            var logMessage = new System.Text.StringBuilder();
            logMessage.AppendLine("Executing DbCommand");
            logMessage.AppendLine(command.CommandText);

            if (command.Parameters.Count > 0)
            {
                logMessage.AppendLine("Parameters:");
                foreach (IDataParameter parameter in command.Parameters)
                {
                    logMessage.AppendLine(
                        $"  {parameter.ParameterName} = {FormatParameterValue(parameter.Value)} ({parameter.DbType})"
                    );
                }
            }

            _logger.LogInformation(logMessage.ToString());
        }

        /// <summary>
        /// Creates and configures a database command with the specified command type, SQL statement, parameter binding
        /// action, and transaction context.
        /// </summary>
        /// <param name="commandType">The type of command to execute, such as text, stored procedure, or table direct.</param>
        /// <param name="sql">The SQL statement or stored procedure name to execute against the database.</param>
        /// <param name="parameterBinder">An action that binds parameters to the command. This allows for dynamic parameterization of the command
        /// before execution. May be null if no parameters are required.</param>
        /// <param name="transaction">An optional database transaction within which the command will be executed. If null, the current active
        /// transaction is used if available.</param>
        /// <returns>An IDbCommand instance configured with the specified command type, SQL, parameters, and transaction context.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the database connection is not open when attempting to create the command.</exception>
        protected IDbCommand CreateCommandCore(
            CommandType commandType,
            string sql,
            Action<IDbCommand> parameterBinder,
            IDbTransaction transaction)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(
                IsDisposed,
                nameof(Contract.IMaterializer));
#else
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(Contract.IMaterializer));
#endif

            if (DbConnection.State != ConnectionState.Open)
                throw new InvalidOperationException("Database connection is not open.");

            IDbCommand command;

            lock (_lockConnection)
                command = DbConnection.CreateCommand();

            command.CommandType = commandType;
            command.CommandText = sql;
            command.CommandTimeout = Timeout;

            lock (_lockTransaction)
                command.Transaction = transaction ?? (IsTransactionActive ? DbTransaction : null);

            parameterBinder?.Invoke(command);

            if (EnableSqlLogging)
                LogCommand(command);

            return command;
        }

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="transaction">
        /// Represents the transaction to be used for the command.
        /// </param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        IDbCommand Contract.IMaterializer.CreateCommand(
            string sql,
            object parameters,
            IDbTransaction transaction)
            => CreateCommandCore(
                CommandType.Text,
                sql,
                command => BindParametersFromObject(command, parameters),
                transaction
            );

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        IDbCommand Contract.IMaterializer.CreateCommand(
            string sql,
            object parameters)
        {
            IDbTransaction transaction = null;

            if (this.IsTransactionActive)
                transaction = this.DbTransaction;

            return (this as Contract.IMaterializer).CreateCommand(
                sql,
                parameters,
                transaction);
        }

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">
        /// Represents the transaction to be used for the command.
        /// </param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        IDbCommand Contract.IMaterializer.CreateCommand(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction)
            => CreateCommandCore(
                commandType,
                sql,
                command =>
                {
                    if (parameters == null)
                        return;

                    foreach (var parameter in parameters)
                        command.Parameters.Add(parameter);
                },
                transaction);

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        IDbCommand Contract.IMaterializer.CreateCommand(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters)
        {
            IDbTransaction transaction = null;

            if (this.IsTransactionActive)
                transaction = this.DbTransaction;

            return (this as Contract.IMaterializer).CreateCommand(
                commandType,
                sql,
                parameters,
                transaction);
        }

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to
        /// System.Data.DataSet columns; and is implemented by .NET Framework data providers that
        /// access data sources.
        /// </typeparam>
        /// <param name="sqlProcedure">
        /// Represents a Sql Entity that holds the data parameters to be executed by the connection command.
        /// </param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The SqlProcedure does not have a SqlProcedureAttibute decorating itself.
        /// </exception>
        IDbCommand Contract.IMaterializer.CreateCommand<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> sqlProcedure)
        {
            IDbTransaction transaction = null;

            if (this.IsTransactionActive)
                transaction = this.DbTransaction;

            return (this as Contract.IMaterializer).CreateCommand(
                sqlProcedure,
                transaction);
        }

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to
        /// System.Data.DataSet columns; and is implemented by .NET Framework data providers that
        /// access data sources.
        /// </typeparam>
        /// <param name="sqlProcedure">
        /// Represents a Sql Entity that holds the data parameters to be executed by the connection command.
        /// </param>
        /// <param name="transaction">
        /// Represents the transaction to be used for the command.
        /// </param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The SqlProcedure does not have a SqlProcedureAttibute decorating itself.
        /// </exception>
        IDbCommand Contract.IMaterializer.CreateCommand<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> sqlProcedure,
            IDbTransaction transaction)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(
                IsDisposed,
                "The connection has been disposed.");

            ArgumentNullException.ThrowIfNull(
                sqlProcedure);
#else
            if (this.IsDisposed)
                throw new ObjectDisposedException("The connection has been disposed.");

            if (sqlProcedure == null)
                throw new ArgumentNullException(nameof(sqlProcedure));
#endif

            if (DbConnection.State != ConnectionState.Open)
                throw new InvalidOperationException("Database connection is not open.");

            var procedureType = sqlProcedure.GetType();

            var sqlProcedureAttribute = procedureType
                .GetCustomAttributes(typeof(ProcedureAttribute), false)
                .Cast<ProcedureAttribute>()
                .FirstOrDefault() ?? throw new MissingMemberException(
                    "The SqlProcedure does not have a ProcedureAttribute decorating itself.");

            IDbCommand command;

            lock (this._lockConnection)
                command = this.DbConnection.CreateCommand();

            command.CommandType = sqlProcedureAttribute.CommandType;
            command.CommandText = sqlProcedureAttribute.CommandText;
            command.CommandTimeout = this.Timeout;

            if (transaction != null)
                command.Transaction = transaction;

            var properties = procedureType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead);

            foreach (var property in properties)
            {
                var parameterAttributes = property
                    .GetCustomAttributes(typeof(ParameterAttribute), false)
                    .Cast<ParameterAttribute>();

                foreach (var parameterAttribute in parameterAttributes)
                {
                    var dataParameter = new TDataParameterDriver
                    {
                        ParameterName = parameterAttribute.Name,
                        Direction = parameterAttribute.Direction,
                        Value = property.GetValue(sqlProcedure) ?? DBNull.Value
                    };

                    if (Enum.IsDefined(typeof(DbType), (int)parameterAttribute.DbType))
                    {
                        dataParameter.DbType = parameterAttribute.DbType;
                    }
                    else
                    {
                        var sqlDbTypeProperty = dataParameter
                            .GetType()
                            .GetProperty(nameof(SqlDbType), BindingFlags.Instance | BindingFlags.Public);

                        if (sqlDbTypeProperty != null &&
                            Enum.IsDefined(typeof(SqlDbType), (int)parameterAttribute.DbType))
                        {
                            sqlDbTypeProperty.SetValue(
                                dataParameter,
                                parameterAttribute.DbType);
                        }
                    }

                    command.Parameters.Add(dataParameter);
                }
            }

            if (EnableSqlLogging)
                LogCommand(command);

            return command;
        }
    }
}