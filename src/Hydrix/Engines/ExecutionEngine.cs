using Hydrix.Configuration;
using Hydrix.Schemas.Contract;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrix.Engines
{
    /// <summary>
    /// Provides centralized execution methods for non-query, scalar, and reader database operations.
    /// </summary>
    /// <remarks>This engine consolidates command execution paths to reduce overload duplication and keep
    /// execution logic decoupled from the materializer layer.</remarks>
    internal static class ExecutionEngine
    {
        /// <summary>
        /// Executes a command and returns the number of affected rows.
        /// </summary>
        /// <param name="connection">The database connection used to create and execute the command.</param>
        /// <param name="sql">The SQL command text or stored procedure name.</param>
        /// <param name="parameters">The optional parameters object or parameter collection.</param>
        /// <param name="transaction">The optional transaction associated with the command.</param>
        /// <param name="commandType">The command interpretation mode.</param>
        /// <param name="commandTimeout">The optional command timeout in seconds.</param>
        /// <param name="parameterPrefix">The parameter name prefix.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(
            IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null,
            string parameterPrefix = null)
        {
            using var command = CommandEngine.CreateCommand(
                connection,
                transaction,
                commandType,
                sql,
                parameters,
                parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix,
                commandTimeout,
                HydrixConfiguration.Options.Logger);

            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a stored procedure command and returns the number of affected rows.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="connection">The database connection used to create and execute the command.</param>
        /// <param name="procedure">The procedure descriptor and parameters.</param>
        /// <param name="transaction">The optional transaction associated with the command.</param>
        /// <param name="commandTimeout">The optional command timeout in seconds.</param>
        /// <param name="parameterPrefix">The parameter name prefix.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery<TDataParameterDriver>(
            IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            string parameterPrefix = null)
            where TDataParameterDriver : IDataParameter, new()
        {
            using var command = CommandEngine.CreateCommand(
                connection,
                transaction,
                procedure,
                parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix,
                commandTimeout,
                HydrixConfiguration.Options.Logger);

            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a command asynchronously and returns the number of affected rows.
        /// </summary>
        /// <param name="connection">The database connection used to create and execute the command.</param>
        /// <param name="sql">The SQL command text or stored procedure name.</param>
        /// <param name="parameters">The optional parameters object or parameter collection.</param>
        /// <param name="transaction">The optional transaction associated with the command.</param>
        /// <param name="commandType">The command interpretation mode.</param>
        /// <param name="commandTimeout">The optional command timeout in seconds.</param>
        /// <param name="parameterPrefix">The parameter name prefix.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <returns>A task containing the number of rows affected.</returns>
        public static async Task<int> ExecuteNonQueryAsync(
            IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null,
            string parameterPrefix = null,
            CancellationToken cancellationToken = default)
        {
            using var command = CommandEngine.CreateCommand(
                connection,
                transaction,
                commandType,
                sql,
                parameters,
                parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix,
                commandTimeout,
                HydrixConfiguration.Options.Logger);

            if (command is DbCommand dbCommand)
                return await dbCommand
                    .ExecuteNonQueryAsync(cancellationToken)
                    .ConfigureAwait(false);

            return await Task.Run(
                    command.ExecuteNonQuery,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a stored procedure command asynchronously and returns the number of affected rows.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="connection">The database connection used to create and execute the command.</param>
        /// <param name="procedure">The procedure descriptor and parameters.</param>
        /// <param name="transaction">The optional transaction associated with the command.</param>
        /// <param name="commandTimeout">The optional command timeout in seconds.</param>
        /// <param name="parameterPrefix">The parameter name prefix.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <returns>A task containing the number of rows affected.</returns>
        public static async Task<int> ExecuteNonQueryAsync<TDataParameterDriver>(
            IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            string parameterPrefix = null,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new()
        {
            using var command = CommandEngine.CreateCommand(
                connection,
                transaction,
                procedure,
                parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix,
                commandTimeout,
                HydrixConfiguration.Options.Logger);

            if (command is DbCommand dbCommand)
                return await dbCommand
                    .ExecuteNonQueryAsync(cancellationToken)
                    .ConfigureAwait(false);

            return await Task.Run(
                    command.ExecuteNonQuery,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a command and returns the first column of the first row.
        /// </summary>
        /// <param name="connection">The database connection used to create and execute the command.</param>
        /// <param name="sql">The SQL command text or stored procedure name.</param>
        /// <param name="parameters">The optional parameters object or parameter collection.</param>
        /// <param name="transaction">The optional transaction associated with the command.</param>
        /// <param name="commandType">The command interpretation mode.</param>
        /// <param name="commandTimeout">The optional command timeout in seconds.</param>
        /// <param name="parameterPrefix">The parameter name prefix.</param>
        /// <returns>The scalar result.</returns>
        public static object ExecuteScalar(
            IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null,
            string parameterPrefix = null)
        {
            using var command = CommandEngine.CreateCommand(
                connection,
                transaction,
                commandType,
                sql,
                parameters,
                parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix,
                commandTimeout,
                HydrixConfiguration.Options.Logger);

            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes a stored procedure command and returns the first column of the first row.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="connection">The database connection used to create and execute the command.</param>
        /// <param name="procedure">The procedure descriptor and parameters.</param>
        /// <param name="transaction">The optional transaction associated with the command.</param>
        /// <param name="commandTimeout">The optional command timeout in seconds.</param>
        /// <param name="parameterPrefix">The parameter name prefix.</param>
        /// <returns>The scalar result.</returns>
        public static object ExecuteScalar<TDataParameterDriver>(
            IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            string parameterPrefix = null)
            where TDataParameterDriver : IDataParameter, new()
        {
            using var command = CommandEngine.CreateCommand(
                connection,
                transaction,
                procedure,
                parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix,
                commandTimeout,
                HydrixConfiguration.Options.Logger);

            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes a command asynchronously and returns the first column of the first row.
        /// </summary>
        /// <param name="connection">The database connection used to create and execute the command.</param>
        /// <param name="sql">The SQL command text or stored procedure name.</param>
        /// <param name="parameters">The optional parameters object or parameter collection.</param>
        /// <param name="transaction">The optional transaction associated with the command.</param>
        /// <param name="commandType">The command interpretation mode.</param>
        /// <param name="commandTimeout">The optional command timeout in seconds.</param>
        /// <param name="parameterPrefix">The parameter name prefix.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <returns>A task containing the scalar result.</returns>
        public static async Task<object> ExecuteScalarAsync(
            IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null,
            string parameterPrefix = null,
            CancellationToken cancellationToken = default)
        {
            using var command = CommandEngine.CreateCommand(
                connection,
                transaction,
                commandType,
                sql,
                parameters,
                parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix,
                commandTimeout,
                HydrixConfiguration.Options.Logger);

            if (command is DbCommand dbCommand)
                return await dbCommand
                    .ExecuteScalarAsync(cancellationToken)
                    .ConfigureAwait(false);

            return await Task.Run(
                    command.ExecuteScalar,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a stored procedure command asynchronously and returns the first column of the first row.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="connection">The database connection used to create and execute the command.</param>
        /// <param name="procedure">The procedure descriptor and parameters.</param>
        /// <param name="transaction">The optional transaction associated with the command.</param>
        /// <param name="commandTimeout">The optional command timeout in seconds.</param>
        /// <param name="parameterPrefix">The parameter name prefix.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <returns>A task containing the scalar result.</returns>
        public static async Task<object> ExecuteScalarAsync<TDataParameterDriver>(
            IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            string parameterPrefix = null,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new()
        {
            using var command = CommandEngine.CreateCommand(
                connection,
                transaction,
                procedure,
                parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix,
                commandTimeout,
                HydrixConfiguration.Options.Logger);

            if (command is DbCommand dbCommand)
                return await dbCommand
                    .ExecuteScalarAsync(cancellationToken)
                    .ConfigureAwait(false);

            return await Task.Run(
                    command.ExecuteScalar,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a command and returns a data reader.
        /// </summary>
        /// <param name="connection">The database connection used to create and execute the command.</param>
        /// <param name="sql">The SQL command text or stored procedure name.</param>
        /// <param name="parameters">The optional parameters object or parameter collection.</param>
        /// <param name="transaction">The optional transaction associated with the command.</param>
        /// <param name="commandType">The command interpretation mode.</param>
        /// <param name="commandTimeout">The optional command timeout in seconds.</param>
        /// <param name="behavior">The command behavior flags.</param>
        /// <param name="parameterPrefix">The parameter name prefix.</param>
        /// <returns>The data reader.</returns>
        public static IDataReader ExecuteReader(
            IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null,
            CommandBehavior behavior = CommandBehavior.Default,
            string parameterPrefix = null)
        {
            using var command = CommandEngine.CreateCommand(
                connection,
                transaction,
                commandType,
                sql,
                parameters,
                parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix,
                commandTimeout,
                HydrixConfiguration.Options.Logger);

            return command.ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes a stored procedure command and returns a data reader.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="connection">The database connection used to create and execute the command.</param>
        /// <param name="procedure">The procedure descriptor and parameters.</param>
        /// <param name="transaction">The optional transaction associated with the command.</param>
        /// <param name="commandTimeout">The optional command timeout in seconds.</param>
        /// <param name="behavior">The command behavior flags.</param>
        /// <param name="parameterPrefix">The parameter name prefix.</param>
        /// <returns>The data reader.</returns>
        public static IDataReader ExecuteReader<TDataParameterDriver>(
            IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandBehavior behavior = CommandBehavior.Default,
            string parameterPrefix = null)
            where TDataParameterDriver : IDataParameter, new()
        {
            using var command = CommandEngine.CreateCommand(
                connection,
                transaction,
                procedure,
                parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix,
                commandTimeout,
                HydrixConfiguration.Options.Logger);

            return command.ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes a command asynchronously and returns a data reader.
        /// </summary>
        /// <param name="connection">The database connection used to create and execute the command.</param>
        /// <param name="sql">The SQL command text or stored procedure name.</param>
        /// <param name="parameters">The optional parameters object or parameter collection.</param>
        /// <param name="transaction">The optional transaction associated with the command.</param>
        /// <param name="commandType">The command interpretation mode.</param>
        /// <param name="commandTimeout">The optional command timeout in seconds.</param>
        /// <param name="behavior">The command behavior flags.</param>
        /// <param name="parameterPrefix">The parameter name prefix.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <returns>The data reader.</returns>
        public static async Task<IDataReader> ExecuteReaderAsync(
            IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null,
            CommandBehavior behavior = CommandBehavior.Default,
            string parameterPrefix = null,
            CancellationToken cancellationToken = default)
        {
            using var command = CommandEngine.CreateCommand(
                connection,
                transaction,
                commandType,
                sql,
                parameters,
                parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix,
                commandTimeout,
                HydrixConfiguration.Options.Logger);

            if (command is DbCommand dbCommand)
                return await dbCommand
                    .ExecuteReaderAsync(behavior, cancellationToken)
                    .ConfigureAwait(false);

            return await Task.Run(
                    () => command.ExecuteReader(behavior),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a stored procedure command asynchronously and returns a data reader.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="connection">The database connection used to create and execute the command.</param>
        /// <param name="procedure">The procedure descriptor and parameters.</param>
        /// <param name="transaction">The optional transaction associated with the command.</param>
        /// <param name="commandTimeout">The optional command timeout in seconds.</param>
        /// <param name="behavior">The command behavior flags.</param>
        /// <param name="parameterPrefix">The parameter name prefix.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <returns>The data reader.</returns>
        public static async Task<IDataReader> ExecuteReaderAsync<TDataParameterDriver>(
            IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandBehavior behavior = CommandBehavior.Default,
            string parameterPrefix = null,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new()
        {
            using var command = CommandEngine.CreateCommand(
                connection,
                transaction,
                procedure,
                parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix,
                commandTimeout,
                HydrixConfiguration.Options.Logger);

            if (command is DbCommand dbCommand)
                return await dbCommand
                    .ExecuteReaderAsync(behavior, cancellationToken)
                    .ConfigureAwait(false);

            return await Task.Run(
                    () => command.ExecuteReader(behavior),
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}