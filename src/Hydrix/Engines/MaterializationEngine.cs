using Hydrix.Configuration;
using Hydrix.Orchestrator.Caching;
using Hydrix.Orchestrator.Materializers;
using Hydrix.Schemas.Contract;
using System.Collections.Generic;
using System.Data;
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
        /// <param name="connection">The database connection to use for executing the query. Must be open and valid.</param>
        /// <param name="sql">The SQL query to execute against the database.</param>
        /// <param name="parameters">An object containing the parameters to be passed to the SQL query, or null if no parameters are required.</param>
        /// <param name="transaction">The transaction context in which to execute the query, or null to execute outside of a transaction.</param>
        /// <param name="commandType">Specifies how the command string is interpreted. The default is CommandType.Text.</param>
        /// <param name="limit">The maximum number of entities to return. Specify 0 to return all results.</param>
        /// <param name="commandTimeout">The command timeout, in seconds. If null, the default timeout for the connection is used.</param>
        /// <param name="parameterPrefix">The prefix to use for parameter names in the SQL query. If null, the default prefix from configuration is
        /// used.</param>
        /// <returns>A list of entities of type TEntity mapped from the result set. The list will be empty if no rows are
        /// returned.</returns>
        public static IList<TEntity> Query<TEntity>(
            IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int limit = 0,
            int? commandTimeout = null,
            string parameterPrefix = null)
            where TEntity : ITable, new()
        {
            EnsureValidEntityRequest<TEntity>();

            using var dataReader = ExecutionEngine.ExecuteReader(
                connection,
                sql,
                parameters,
                transaction,
                commandType,
                commandTimeout,
                CommandBehavior.Default,
                parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix);

            return Materializer.ConvertDataReaderToEntities<TEntity>(
                dataReader,
                limit);
        }

        /// <summary>
        /// Asynchronously executes a SQL query and maps the result set to a list of entities of type TEntity.
        /// </summary>
        /// <remarks>The method automatically maps each row in the result set to an instance of TEntity
        /// using the configured materializer. The caller is responsible for managing the connection's lifetime and
        /// ensuring it is open before calling this method.</remarks>
        /// <typeparam name="TEntity">The type of entity to map the query results to. Must implement ITable and have a parameterless constructor.</typeparam>
        /// <param name="connection">The database connection to use for executing the query. Must be open and valid.</param>
        /// <param name="sql">The SQL query to execute against the database.</param>
        /// <param name="parameters">An object containing the parameters to be passed to the SQL query, or null if no parameters are required.</param>
        /// <param name="transaction">The transaction context in which to execute the query, or null to execute outside of a transaction.</param>
        /// <param name="commandType">Specifies how the command string is interpreted. The default is CommandType.Text.</param>
        /// <param name="limit">The maximum number of entities to return. Specify 0 to return all results.</param>
        /// <param name="commandTimeout">The command timeout, in seconds. If null, the default timeout for the connection is used.</param>
        /// <param name="parameterPrefix">The prefix to use for parameter names in the SQL query. If null, the default prefix from configuration is
        /// used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a list of entities of type TEntity
        /// mapped from the query results. The list is empty if no rows are returned.</returns>
        public static async Task<IList<TEntity>> QueryAsync<TEntity>(
            IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int limit = 0,
            int? commandTimeout = null,
            string parameterPrefix = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
        {
            EnsureValidEntityRequest<TEntity>();

            using var dataReader = await ExecutionEngine
                .ExecuteReaderAsync(
                    connection,
                    sql,
                    parameters,
                    transaction,
                    commandType,
                    commandTimeout,
                    CommandBehavior.Default,
                    parameterPrefix ?? HydrixConfiguration.Options.ParameterPrefix,
                    cancellationToken)
                .ConfigureAwait(false);

            return Materializer.ConvertDataReaderToEntities<TEntity>(
                dataReader,
                limit);
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
    }
}
