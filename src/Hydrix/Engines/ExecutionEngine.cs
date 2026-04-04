using Hydrix.Configuration;
using Hydrix.Engines.Options;
using Hydrix.Schemas.Contract;
using Hydrix.Wrappers;
using System;
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
        /// <param name="sql">The SQL command text or stored procedure name.</param>
        /// <param name="parameters">The optional parameters object or parameter collection.</param>
        /// <param name="options">The command execution options.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(
            string sql,
            object parameters = null,
            ExecutionCommandOptions options = null)
        {
            options = ResolveCommandOptions(options);
            EnsureConnectionConfigured(options);

            using var command = CommandEngine.CreateCommand(
                options.Connection,
                options.Transaction,
                options.CommandType,
                sql,
                parameters,
                ResolveParameterPrefix(options),
                options.CommandTimeout);

            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a stored procedure command and returns the number of affected rows.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="procedure">The procedure descriptor and parameters.</param>
        /// <param name="options">The command execution options.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            ExecutionOptions options = null)
            where TDataParameterDriver : IDataParameter, new()
        {
            options = ResolveOptions(options);
            EnsureConnectionConfigured(options);

            using var command = CommandEngine.CreateCommand(
                options.Connection,
                options.Transaction,
                procedure,
                ResolveParameterPrefix(options),
                options.CommandTimeout);

            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a command asynchronously and returns the number of affected rows.
        /// </summary>
        /// <remarks>When the underlying connection provides a <see cref="System.Data.Common.DbCommand"/>, true
        /// asynchronous I/O is used and <paramref name="cancellationToken"/> can cancel the in-flight I/O.
        /// Otherwise the execution falls back to <see cref="System.Threading.Tasks.Task.Run(System.Action)"/>,
        /// which offloads the synchronous call to a thread-pool thread. In that fallback mode the operation
        /// occupies a thread and the cancellation token cannot interrupt the provider I/O. For true async
        /// support ensure the connection provider exposes <see cref="System.Data.Common.DbCommand"/>.</remarks>
        /// <param name="sql">The SQL command text or stored procedure name.</param>
        /// <param name="parameters">The optional parameters object or parameter collection.</param>
        /// <param name="options">The command execution options.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <returns>A task containing the number of rows affected.</returns>
        public static async Task<int> ExecuteNonQueryAsync(
            string sql,
            object parameters = null,
            ExecutionCommandOptions options = null,
            CancellationToken cancellationToken = default)
        {
            options = ResolveCommandOptions(options);
            EnsureConnectionConfigured(options);

            using var command = CommandEngine.CreateCommand(
                options.Connection,
                options.Transaction,
                options.CommandType,
                sql,
                parameters,
                ResolveParameterPrefix(options),
                options.CommandTimeout);

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
        /// <remarks>When the underlying connection provides a <see cref="System.Data.Common.DbCommand"/>, true
        /// asynchronous I/O is used and <paramref name="cancellationToken"/> can cancel the in-flight I/O.
        /// Otherwise the execution falls back to <see cref="System.Threading.Tasks.Task.Run(System.Action)"/>,
        /// which offloads the synchronous call to a thread-pool thread. In that fallback mode the operation
        /// occupies a thread and the cancellation token cannot interrupt the provider I/O. For true async
        /// support ensure the connection provider exposes <see cref="System.Data.Common.DbCommand"/>.</remarks>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="procedure">The procedure descriptor and parameters.</param>
        /// <param name="options">The command execution options.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <returns>A task containing the number of rows affected.</returns>
        public static async Task<int> ExecuteNonQueryAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            ExecutionOptions options = null,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new()
        {
            options = ResolveOptions(options);
            EnsureConnectionConfigured(options);

            using var command = CommandEngine.CreateCommand(
                options.Connection,
                options.Transaction,
                procedure,
                ResolveParameterPrefix(options),
                options.CommandTimeout);

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
        /// <param name="sql">The SQL command text or stored procedure name.</param>
        /// <param name="parameters">The optional parameters object or parameter collection.</param>
        /// <param name="options">The command execution options.</param>
        /// <returns>The scalar result.</returns>
        public static object ExecuteScalar(
            string sql,
            object parameters = null,
            ExecutionCommandOptions options = null)
        {
            options = ResolveCommandOptions(options);
            EnsureConnectionConfigured(options);

            using var command = CommandEngine.CreateCommand(
                options.Connection,
                options.Transaction,
                options.CommandType,
                sql,
                parameters,
                ResolveParameterPrefix(options),
                options.CommandTimeout);

            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes a stored procedure command and returns the first column of the first row.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="procedure">The procedure descriptor and parameters.</param>
        /// <param name="options">The command execution options.</param>
        /// <returns>The scalar result.</returns>
        public static object ExecuteScalar<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            ExecutionOptions options = null)
            where TDataParameterDriver : IDataParameter, new()
        {
            options = ResolveOptions(options);
            EnsureConnectionConfigured(options);

            using var command = CommandEngine.CreateCommand(
                options.Connection,
                options.Transaction,
                procedure,
                ResolveParameterPrefix(options),
                options.CommandTimeout);

            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes a command asynchronously and returns the first column of the first row.
        /// </summary>
        /// <remarks>When the underlying connection provides a <see cref="System.Data.Common.DbCommand"/>, true
        /// asynchronous I/O is used and <paramref name="cancellationToken"/> can cancel the in-flight I/O.
        /// Otherwise the execution falls back to <see cref="System.Threading.Tasks.Task.Run{TResult}(System.Func{TResult})"/>,
        /// which offloads the synchronous call to a thread-pool thread. In that fallback mode the operation
        /// occupies a thread and the cancellation token cannot interrupt the provider I/O. For true async
        /// support ensure the connection provider exposes <see cref="System.Data.Common.DbCommand"/>.</remarks>
        /// <param name="sql">The SQL command text or stored procedure name.</param>
        /// <param name="parameters">The optional parameters object or parameter collection.</param>
        /// <param name="options">The command execution options.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <returns>A task containing the scalar result.</returns>
        public static async Task<object> ExecuteScalarAsync(
            string sql,
            object parameters = null,
            ExecutionCommandOptions options = null,
            CancellationToken cancellationToken = default)
        {
            options = ResolveCommandOptions(options);
            EnsureConnectionConfigured(options);

            using var command = CommandEngine.CreateCommand(
                options.Connection,
                options.Transaction,
                options.CommandType,
                sql,
                parameters,
                ResolveParameterPrefix(options),
                options.CommandTimeout);

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
        /// <remarks>When the underlying connection provides a <see cref="System.Data.Common.DbCommand"/>, true
        /// asynchronous I/O is used and <paramref name="cancellationToken"/> can cancel the in-flight I/O.
        /// Otherwise the execution falls back to <see cref="System.Threading.Tasks.Task.Run{TResult}(System.Func{TResult})"/>,
        /// which offloads the synchronous call to a thread-pool thread. In that fallback mode the operation
        /// occupies a thread and the cancellation token cannot interrupt the provider I/O. For true async
        /// support ensure the connection provider exposes <see cref="System.Data.Common.DbCommand"/>.</remarks>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="procedure">The procedure descriptor and parameters.</param>
        /// <param name="options">The command execution options.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <returns>A task containing the scalar result.</returns>
        public static async Task<object> ExecuteScalarAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            ExecutionOptions options = null,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new()
        {
            options = ResolveOptions(options);
            EnsureConnectionConfigured(options);

            using var command = CommandEngine.CreateCommand(
                options.Connection,
                options.Transaction,
                procedure,
                ResolveParameterPrefix(options),
                options.CommandTimeout);

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
        /// <param name="sql">The SQL command text or stored procedure name.</param>
        /// <param name="parameters">The optional parameters object or parameter collection.</param>
        /// <param name="behavior">The command behavior flags.</param>
        /// <param name="options">The command execution options.</param>
        /// <returns>The data reader.</returns>
        public static IDataReader ExecuteReader(
            string sql,
            object parameters = null,
            CommandBehavior behavior = CommandBehavior.Default,
            ExecutionCommandOptions options = null)
        {
            options = ResolveCommandOptions(options);
            EnsureConnectionConfigured(options);

            var command = CommandEngine.CreateCommand(
                options.Connection,
                options.Transaction,
                options.CommandType,
                sql,
                parameters,
                ResolveParameterPrefix(options),
                options.CommandTimeout);

            return ExecuteReaderAndOwnCommand(
                command,
                behavior);
        }

        /// <summary>
        /// Executes a stored procedure command and returns a data reader.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="procedure">The procedure descriptor and parameters.</param>
        /// <param name="behavior">The command behavior flags.</param>
        /// <param name="options">The command execution options.</param>
        /// <returns>The data reader.</returns>
        public static IDataReader ExecuteReader<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            CommandBehavior behavior = CommandBehavior.Default,
            ExecutionOptions options = null)
            where TDataParameterDriver : IDataParameter, new()
        {
            options = ResolveOptions(options);
            EnsureConnectionConfigured(options);

            var command = CommandEngine.CreateCommand(
                options.Connection,
                options.Transaction,
                procedure,
                ResolveParameterPrefix(options),
                options.CommandTimeout);

            return ExecuteReaderAndOwnCommand(
                command,
                behavior);
        }

        /// <summary>
        /// Executes a command asynchronously and returns a data reader.
        /// </summary>
        /// <remarks>When the underlying connection provides a <see cref="System.Data.Common.DbCommand"/>, true
        /// asynchronous I/O is used and <paramref name="cancellationToken"/> can cancel the in-flight I/O.
        /// Otherwise the execution falls back to <see cref="System.Threading.Tasks.Task.Run{TResult}(System.Func{TResult})"/>,
        /// which offloads the synchronous call to a thread-pool thread. In that fallback mode the operation
        /// occupies a thread and the cancellation token cannot interrupt the provider I/O. For true async
        /// support ensure the connection provider exposes <see cref="System.Data.Common.DbCommand"/>.</remarks>
        /// <param name="sql">The SQL command text or stored procedure name.</param>
        /// <param name="parameters">The optional parameters object or parameter collection.</param>
        /// <param name="behavior">The command behavior flags.</param>
        /// <param name="options">The command execution options.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <returns>The data reader.</returns>
        public static async Task<IDataReader> ExecuteReaderAsync(
            string sql,
            object parameters = null,
            CommandBehavior behavior = CommandBehavior.Default,
            ExecutionCommandOptions options = null,
            CancellationToken cancellationToken = default)
        {
            options = ResolveCommandOptions(options);
            EnsureConnectionConfigured(options);

            var command = CommandEngine.CreateCommand(
                options.Connection,
                options.Transaction,
                options.CommandType,
                sql,
                parameters,
                ResolveParameterPrefix(options),
                options.CommandTimeout);

            return await ExecuteReaderAsyncAndOwnCommand(
                    command,
                    behavior,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a stored procedure command asynchronously and returns a data reader.
        /// </summary>
        /// <remarks>When the underlying connection provides a <see cref="System.Data.Common.DbCommand"/>, true
        /// asynchronous I/O is used and <paramref name="cancellationToken"/> can cancel the in-flight I/O.
        /// Otherwise the execution falls back to <see cref="System.Threading.Tasks.Task.Run{TResult}(System.Func{TResult})"/>,
        /// which offloads the synchronous call to a thread-pool thread. In that fallback mode the operation
        /// occupies a thread and the cancellation token cannot interrupt the provider I/O. For true async
        /// support ensure the connection provider exposes <see cref="System.Data.Common.DbCommand"/>.</remarks>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="procedure">The procedure descriptor and parameters.</param>
        /// <param name="behavior">The command behavior flags.</param>
        /// <param name="options">The command execution options.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <returns>The data reader.</returns>
        public static async Task<IDataReader> ExecuteReaderAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            CommandBehavior behavior = CommandBehavior.Default,
            ExecutionOptions options = null,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new()
        {
            options = ResolveOptions(options);
            EnsureConnectionConfigured(options);

            var command = CommandEngine.CreateCommand(
                options.Connection,
                options.Transaction,
                procedure,
                ResolveParameterPrefix(options),
                options.CommandTimeout);

            return await ExecuteReaderAsyncAndOwnCommand(
                    command,
                    behavior,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a reader-producing command and transfers command disposal to the returned reader.
        /// </summary>
        /// <param name="command">The configured command to execute. Ownership is transferred to the returned reader on success,
        /// or disposed immediately if execution throws.</param>
        /// <param name="behavior">The command behavior flags to pass to <see cref="IDbCommand.ExecuteReader(CommandBehavior)"/>.</param>
        /// <returns>An <see cref="IDataReader"/> that owns the underlying command and disposes it when closed.</returns>
        private static IDataReader ExecuteReaderAndOwnCommand(
            IDbCommand command,
            CommandBehavior behavior)
        {
            try
            {
                return CommandOwningReader.Wrap(
                    command,
                    command.ExecuteReader(behavior));
            }
            catch
            {
                command.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Executes a reader-producing command asynchronously and transfers command disposal to the returned reader.
        /// </summary>
        /// <param name="command">The configured command to execute. Ownership is transferred to the returned reader on success,
        /// or disposed immediately if execution throws.</param>
        /// <param name="behavior">The command behavior flags to pass to the underlying execute call.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. Effective only when the command
        /// is a <see cref="DbCommand"/>; ignored in the <see cref="Task.Run{TResult}(System.Func{TResult})"/> fallback path.</param>
        /// <returns>A task containing an <see cref="IDataReader"/> that owns the underlying command and disposes it when closed.</returns>
        private static async Task<IDataReader> ExecuteReaderAsyncAndOwnCommand(
            IDbCommand command,
            CommandBehavior behavior,
            CancellationToken cancellationToken)
        {
            try
            {
                if (command is DbCommand dbCommand)
                {
                    var dbDataReader = await dbCommand
                        .ExecuteReaderAsync(behavior, cancellationToken)
                        .ConfigureAwait(false);

                    return CommandOwningReader.Wrap(
                        command,
                        dbDataReader);
                }

                var reader = await Task.Run(
                        () => command.ExecuteReader(behavior),
                        cancellationToken)
                    .ConfigureAwait(false);

                return CommandOwningReader.Wrap(
                    command,
                    reader);
            }
            catch
            {
                command.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Resolves command execution options, returning a default instance when none is provided.
        /// </summary>
        /// <param name="options">The command execution options.</param>
        /// <returns>A non-null command execution options instance.</returns>
        private static ExecutionCommandOptions ResolveCommandOptions(
            ExecutionCommandOptions options)
            => options ?? new ExecutionCommandOptions();

        /// <summary>
        /// Resolves execution options, returning a default instance when none is provided.
        /// </summary>
        /// <param name="options">The execution options.</param>
        /// <returns>A non-null execution options instance.</returns>
        private static ExecutionOptions ResolveOptions(
            ExecutionOptions options)
            => options ?? new ExecutionOptions();

        /// <summary>
        /// Validates that execution options include a database connection.
        /// </summary>
        /// <param name="options">The execution options to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <see cref="ExecutionOptions.Connection"/> is null.</exception>
        private static void EnsureConnectionConfigured(
            ExecutionOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(
                    nameof(options),
                    "Execution options are required. Provide a non-null options instance with Connection configured.");

            if (options.Connection == null)
                throw new ArgumentException(
                    "A non-null Connection is required. Provide options.Connection or use an API overload that accepts an explicit connection.",
                    nameof(options));
        }

        /// <summary>
        /// Resolves the effective parameter prefix for command execution.
        /// </summary>
        /// <param name="options">The execution options instance.</param>
        /// <returns>The provided parameter prefix or the configured default prefix when none is specified.</returns>
        private static string ResolveParameterPrefix(
            ExecutionOptions options)
            => options.ParameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix;
    }
}
