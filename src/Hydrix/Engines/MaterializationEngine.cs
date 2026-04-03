using Hydrix.Caching;
using Hydrix.Configuration;
using Hydrix.Engines.Options;
using Hydrix.Extensions;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrix.Engines
{
    /// <summary>
    /// Provides static methods for executing SQL queries and mapping result sets to entity objects.
    /// </summary>
    /// <remarks>This class offers both synchronous and asynchronous methods for materializing database query
    /// results into strongly typed entity lists. It is intended for internal use and assumes that the caller manages
    /// the database connection's lifetime and state. All entity types used with this engine must implement the ITable
    /// interface and have a parameterless constructor.</remarks>
    internal static class MaterializationEngine
    {
        /// <summary>
        /// Executes the specified SQL query and maps the result set to a list of entities of type TEntity.
        /// </summary>
        /// <remarks>The method automatically maps each row in the result set to an instance of TEntity.
        /// If a limit is specified, only up to that number of entities will be returned. The caller is responsible for
        /// managing the connection's lifetime.</remarks>
        /// <typeparam name="TEntity">The type of entity to map the query results to. Must implement ITable and have a parameterless constructor.</typeparam>
        /// <param name="sql">The SQL query to execute against the database.</param>
        /// <param name="parameters">An object containing the parameters to be passed to the SQL query, or null if no parameters are required.</param>
        /// <param name="options">The command execution options.</param>
        /// <returns>A list of entities of type TEntity mapped from the result set. The list will be empty if no rows are
        /// returned.</returns>
        public static IList<TEntity> Query<TEntity>(
            string sql,
            object parameters = null,
            MaterializationCommandOptions options = null)
            where TEntity : ITable, new()
        {
            EnsureValidEntityRequest<TEntity>();
            options = ResolveCommandOptions(options);
            EnsureConnectionConfigured(options);

            using var dataReader = ExecutionEngine.ExecuteReader(
                sql,
                parameters,
                CommandBehavior.Default,
                options);

            return dataReader.MapTo<TEntity>(
                options.Limit);
        }

        /// <summary>
        /// Executes the specified stored procedure and maps the result set to a list of entities of type TEntity.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to map the query results to. Must implement ITable and have a parameterless constructor.</typeparam>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="procedure">The stored procedure descriptor and parameters.</param>
        /// <param name="options">The command execution options.</param>
        /// <returns>A list of entities of type TEntity mapped from the result set.</returns>
        public static IList<TEntity> Query<TEntity, TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            MaterializationOptions options = null)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            EnsureValidEntityRequest<TEntity>();
            options = ResolveOptions(options);
            EnsureConnectionConfigured(options);

            using var dataReader = ExecutionEngine.ExecuteReader(
                procedure,
                CommandBehavior.Default,
                options);

            return dataReader.MapTo<TEntity>(
                options.Limit);
        }

        /// <summary>
        /// Asynchronously executes a SQL query and maps the result set to a list of entities of type TEntity.
        /// </summary>
        /// <remarks>The method automatically maps each row in the result set to an instance of TEntity
        /// using the configured materializer. The caller is responsible for managing the connection's lifetime and
        /// ensuring it is open before calling this method.</remarks>
        /// <typeparam name="TEntity">The type of entity to map the query results to. Must implement ITable and have a parameterless constructor.</typeparam>
        /// <param name="sql">The SQL query to execute against the database.</param>
        /// <param name="parameters">An object containing the parameters to be passed to the SQL query, or null if no parameters are required.</param>
        /// <param name="options">The command execution options.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a list of entities of type TEntity
        /// mapped from the query results. The list is empty if no rows are returned.</returns>
        [SuppressMessage(
            "Major Code Smell",
            "S6966",
            Justification = "Synchronous execution is required in materialization pipeline for performance and allocation control")]
        public static async Task<IList<TEntity>> QueryAsync<TEntity>(
            string sql,
            object parameters = null,
            MaterializationCommandOptions options = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
        {
            EnsureValidEntityRequest<TEntity>();
            options = ResolveCommandOptions(options);
            EnsureConnectionConfigured(options);

            using var dataReader = await ExecutionEngine
                .ExecuteReaderAsync(
                    sql,
                    parameters,
                    CommandBehavior.Default,
                    options,
                    cancellationToken)
                .ConfigureAwait(false);

            if (dataReader is DbDataReader)
            {
                return await dataReader
                    .MapToAsync<TEntity>(
                        options.Limit,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            return dataReader.MapTo<TEntity>(
                options.Limit);
        }

        /// <summary>
        /// Asynchronously executes the specified stored procedure and maps the result set to a list of entities of type TEntity.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to map the query results to. Must implement ITable and have a parameterless constructor.</typeparam>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="procedure">The stored procedure descriptor and parameters.</param>
        /// <param name="options">The command execution options.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the mapped entity list.</returns>
        [SuppressMessage(
            "Major Code Smell",
            "S6966",
            Justification = "Synchronous execution is required in materialization pipeline for performance and allocation control")]
        public static async Task<IList<TEntity>> QueryAsync<TEntity, TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            MaterializationOptions options = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            EnsureValidEntityRequest<TEntity>();
            options = ResolveOptions(options);
            EnsureConnectionConfigured(options);

            using var dataReader = await ExecutionEngine
                .ExecuteReaderAsync(
                    procedure,
                    CommandBehavior.Default,
                    options,
                    cancellationToken)
                .ConfigureAwait(false);

            if (dataReader is DbDataReader)
            {
                return await dataReader
                    .MapToAsync<TEntity>(
                        options.Limit,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            return dataReader.MapTo<TEntity>(
                options.Limit);
        }

        /// <summary>
        /// Validates that the specified entity type meets the requirements for use in entity requests.
        /// </summary>
        /// <remarks>This method ensures that the entity type conforms to the expected structure for
        /// entity requests. If the type is invalid, an exception may be thrown by the underlying validation
        /// logic.</remarks>
        /// <typeparam name="TEntity">The type of entity to validate. Must implement the ITable interface and have a parameterless constructor.</typeparam>
        private static void EnsureValidEntityRequest<TEntity>()
            where TEntity : ITable, new()
            => EntityRequestValidationCache.Validate(typeof(TEntity));

        /// <summary>
        /// Validates that command execution options include a database connection.
        /// </summary>
        /// <param name="options">The execution options to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="ExecutionOptions.Connection"/> is null.</exception>
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
        /// Resolves command execution options, returning a default instance when none is provided.
        /// </summary>
        /// <param name="options">The command execution options.</param>
        /// <returns>A non-null command execution options instance.</returns>
        private static MaterializationCommandOptions ResolveCommandOptions(
            MaterializationCommandOptions options)
            => options ?? new MaterializationCommandOptions
            {
                ParameterPrefix = HydrixConfiguration.Options.ParameterPrefix
            };

        /// <summary>
        /// Resolves execution options, returning a default instance when none is provided.
        /// </summary>
        /// <param name="options">The execution options.</param>
        /// <returns>A non-null execution options instance.</returns>
        private static MaterializationOptions ResolveOptions(
            MaterializationOptions options)
            => options ?? new MaterializationOptions
            {
                ParameterPrefix = HydrixConfiguration.Options.ParameterPrefix
            };
    }
}
