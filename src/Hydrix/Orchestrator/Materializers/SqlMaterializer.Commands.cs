using Hydrix.Attributes.Parameters;
using Hydrix.Attributes.Schemas;
using Hydrix.Schemas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// SQL Materializer Class
    /// </summary>
    public partial class SqlMaterializer :
        Contract.ISqlMaterializer
    {
        /// <summary>
        /// Writes a human-readable representation of the SQL command being executed to the console,
        /// including its text and bound parameters.
        ///
        /// This method is designed exclusively for **diagnostic and debugging** purposes, providing
        /// visibility into the final SQL command and its parameter values exactly as they will be
        /// sent to the database provider.
        ///
        /// The output includes:
        /// <list type="bullet">
        /// <item>
        /// <description>The raw SQL command text ( <see cref="IDbCommand.CommandText"/>).</description>
        /// </item>
        /// <item>
        /// <description>
        /// A list of parameters with their names, formatted values and corresponding <see cref="System.Data.DbType"/>.
        /// </description>
        /// </item>
        /// </list>
        /// Parameter values are formatted using <see cref="FormatParameterValue(object)"/> to
        /// improve readability and consistency with common SQL literal conventions.
        /// <para>
        /// <b>Important:</b> This logging mechanism should never be used to build executable SQL
        /// statements and must not be enabled in production environments that handle sensitive data
        /// unless properly controlled.
        /// </para>
        /// </summary>
        /// <param name="command">
        /// The <see cref="IDbCommand"/> instance representing the SQL operation about to be executed.
        /// </param>
        private static void LogCommand(IDbCommand command)
        {
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("Executing DbCommand");
            Console.WriteLine();
            Console.WriteLine(command.CommandText);
            Console.WriteLine();

            if (command.Parameters.Count > 0)
            {
                Console.WriteLine("Parameters:");

                foreach (IDataParameter parameter in command.Parameters)
                {
                    Console.WriteLine(
                        $"  {parameter.ParameterName} = {FormatParameterValue(parameter.Value)} ({parameter.DbType})"
                    );
                }
            }

            Console.WriteLine("--------------------------------------------------");
        }

        /// <summary>
        /// Creates and configures a fully initialized <see cref="IDbCommand"/> instance associated
        /// with the current database connection, applying all common command settings and optional
        /// parameter binding logic.
        ///
        /// This method acts as the core command factory used by all public <c>CreateCommand</c>
        /// overloads, centralizing shared responsibilities such as:
        /// <list type="bullet">
        /// <item>
        /// <description>Validation of the handler lifecycle and connection state.</description>
        /// </item>
        /// <item>
        /// <description>Creation of the underlying provider-specific command instance.</description>
        /// </item>
        /// <item>
        /// <description>
        /// Configuration of command metadata, including command type, command text, timeout and
        /// active transaction association.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Delegation of parameter materialization through the supplied <paramref
        /// name="parameterBinder"/> callback.
        /// </description>
        /// </item>
        /// </list>
        /// The optional <paramref name="parameterBinder"/> allows callers to inject custom
        /// parameter binding strategies (e.g. object-based, attribute-based or provider-specific
        /// parameters) without duplicating command creation logic.
        ///
        /// This design promotes code reuse, consistency and extensibility, making it easy to
        /// introduce cross-cutting concerns such as logging, tracing or metrics in a single,
        /// centralized location.
        /// </summary>
        /// <param name="commandType">
        /// Indicates how the <see cref="IDbCommand.CommandText"/> property should be interpreted
        /// (e.g. <see cref="CommandType.Text"/>, <see cref="CommandType.StoredProcedure"/>).
        /// </param>
        /// <param name="sql">
        /// The SQL statement or stored procedure name to be executed by the command.
        /// </param>
        /// <param name="transaction">
        /// Represents the transaction to be used for the command.
        /// </param>
        /// <param name="parameterBinder">
        /// An optional delegate responsible for binding parameters to the command. When
        /// <c>null</c>, the command is created without parameters.
        /// </param>
        /// <returns>A fully configured <see cref="IDbCommand"/> instance ready for execution.</returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the data handler has already been disposed.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the database connection is not open.
        /// </exception>
        private IDbCommand CreateCommandCore(
            CommandType commandType,
            string sql,
            Action<IDbCommand> parameterBinder,
            IDbTransaction transaction)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(
                IsDisposed,
                nameof(Contract.ISqlMaterializer));
#else
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(Contract.ISqlMaterializer));
#endif

            if (DbConnection.State != ConnectionState.Open)
                throw new InvalidOperationException("Database connection is not open.");

            IDbCommand command;

            lock (_lockConnection)
                command = DbConnection.CreateCommand();

            command.CommandType = commandType;
            command.CommandText = sql;
            command.CommandTimeout = Timeout;

            if (transaction == null && IsTransactionActive)
                transaction = DbTransaction;

            if (transaction != null)
                command.Transaction = transaction;

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
        IDbCommand Contract.ISqlMaterializer.CreateCommand(
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
        IDbCommand Contract.ISqlMaterializer.CreateCommand(
            string sql,
            object parameters)
        {
            IDbTransaction transaction = null;

            if (this.IsTransactionActive)
                transaction = this.DbTransaction;

            return (this as Contract.ISqlMaterializer).CreateCommand(
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
        IDbCommand Contract.ISqlMaterializer.CreateCommand(
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
        IDbCommand Contract.ISqlMaterializer.CreateCommand(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters)
        {
            IDbTransaction transaction = null;

            if (this.IsTransactionActive)
                transaction = this.DbTransaction;

            return (this as Contract.ISqlMaterializer).CreateCommand(
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
        IDbCommand Contract.ISqlMaterializer.CreateCommand<TDataParameterDriver>(
            ISqlProcedure<TDataParameterDriver> sqlProcedure)
        {
            IDbTransaction transaction = null;

            if (this.IsTransactionActive)
                transaction = this.DbTransaction;

            return (this as Contract.ISqlMaterializer).CreateCommand(
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
        IDbCommand Contract.ISqlMaterializer.CreateCommand<TDataParameterDriver>(
            ISqlProcedure<TDataParameterDriver> sqlProcedure,
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
                .GetCustomAttributes(typeof(SqlProcedureAttribute), false)
                .Cast<SqlProcedureAttribute>()
                .FirstOrDefault() ?? throw new MissingMemberException(
                    "The SqlProcedure does not have a SqlProcedureAttribute decorating itself.");

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
                    .GetCustomAttributes(typeof(SqlParameterAttribute), false)
                    .Cast<SqlParameterAttribute>();

                foreach (var parameterAttribute in parameterAttributes)
                {
                    var dataParameter = new TDataParameterDriver
                    {
                        ParameterName = parameterAttribute.ParameterName,
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