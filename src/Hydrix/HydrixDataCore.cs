using Hydrix.Configuration;
using Hydrix.Defaults;
using Hydrix.Engines;
using Hydrix.Engines.Options;
using Hydrix.Extensions;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrix
{
    /// <summary>
    /// Provides extension methods for executing SQL commands against a database connection.
    /// </summary>
    /// <remarks>These methods allow for synchronous and asynchronous execution of SQL commands with optional
    /// parameters, transaction support, and command type specification. The methods throw an ArgumentNullException if
    /// the connection is null.</remarks>
    public static class HydrixDataCore
    {
        /// <summary>
        /// Represents the error message used when an operation is performed on an empty sequence.
        /// </summary>
        /// <remarks>This constant is typically used in exception messages to indicate that a sequence
        /// does not contain any elements. It can be referenced when throwing exceptions such as
        /// InvalidOperationException in sequence-processing methods.</remarks>
        private const string SequenceContainsNoElementsMessage = "Sequence contains no elements.";

        /// <summary>
        /// Represents the error message used when a sequence contains more than one element in operations that expect a
        /// single result.
        /// </summary>
        /// <remarks>This constant is typically used in exception messages to indicate that an operation
        /// requiring a single element encountered multiple elements in the sequence.</remarks>
        private const string SequenceContainsMoreThanOneElementMessage = "Sequence contains more than one element.";

        /// <summary>
        /// Executes a SQL command against the specified database connection and returns the number of rows affected.
        /// </summary>
        /// <remarks>This method is an extension for IDbConnection that simplifies executing non-query SQL
        /// commands, such as INSERT, UPDATE, or DELETE statements, with optional parameters and transaction support.
        /// The method does not return any result sets. To execute queries that return results, use a different method
        /// designed for data retrieval.</remarks>
        /// <param name="connection">The database connection to use for executing the SQL command. This parameter cannot be null and must be open
        /// before calling this method.</param>
        /// <param name="sql">The SQL command text to execute against the database. This can be a query or a command such as an INSERT,
        /// UPDATE, or DELETE statement.</param>
        /// <param name="parameters">An optional object containing the parameters to be used in the SQL command. The object's properties should
        /// match the parameter names expected by the SQL statement. If null, the command is executed without
        /// parameters.</param>
        /// <param name="transaction">An optional database transaction within which to execute the command. If not provided, the command is
        /// executed outside of a transaction.</param>
        /// <param name="commandType">Specifies how the command string is interpreted. Use CommandType.Text for raw SQL queries or
        /// CommandType.StoredProcedure for stored procedures. The default is CommandType.Text.</param>
        /// <param name="commandTimeout">An optional timeout value, in seconds, for the command execution. If not specified, the default command
        /// timeout for the connection is used.</param>
        /// <returns>The number of rows affected by the command execution.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the connection parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the sql parameter is null, empty, or consists only of white-space characters.</exception>
        public static int Execute(
            this IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null)
        {
            ValidateCommand(connection, sql);
            return ExecutionEngine.ExecuteNonQuery(
                sql,
                parameters,
                new ExecutionCommandOptions
                {
                    Connection = connection,
                    Transaction = transaction,
                    CommandType = commandType,
                    CommandTimeout = commandTimeout
                });
        }

        /// <summary>
        /// Executes a stored procedure represented by the specified procedure object and returns the number of rows
        /// affected.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">The parameter driver type used by the procedure.</typeparam>
        /// <param name="connection">The database connection used to execute the procedure.</param>
        /// <param name="procedure">The stored procedure definition containing the command metadata and parameters.</param>
        /// <param name="transaction">An optional transaction in which to execute the procedure.</param>
        /// <param name="commandTimeout">An optional command timeout in seconds.</param>
        /// <returns>The number of rows affected by the stored procedure execution.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="connection"/> or <paramref name="procedure"/> is null.</exception>
        public static int Execute<TDataParameterDriver>(
            this IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null)
            where TDataParameterDriver : IDataParameter, new()
        {
            ValidateProcedure(connection, procedure);
            return ExecutionEngine.ExecuteNonQuery(
                procedure,
                new ExecutionOptions
                {
                    Connection = connection,
                    Transaction = transaction,
                    CommandTimeout = commandTimeout,
                    ParameterPrefix = HydrixConfiguration.Options.ParameterPrefix
                });
        }

        /// <summary>
        /// Asynchronously executes a SQL command using the specified database connection and returns the number of rows
        /// affected.
        /// </summary>
        /// <remarks>This method is an extension for IDbConnection that simplifies executing non-query SQL
        /// commands asynchronously, such as INSERT, UPDATE, or DELETE statements. The method supports both
        /// parameterized queries and stored procedures, and can participate in an existing transaction if
        /// provided.</remarks>
        /// <param name="connection">The database connection to use for executing the SQL command. This parameter cannot be null and must be open
        /// before calling this method.</param>
        /// <param name="sql">The SQL command to execute. This can be a text command or the name of a stored procedure, depending on the
        /// value of the commandType parameter.</param>
        /// <param name="parameters">An optional object containing the parameters to be used with the SQL command. The object's properties should
        /// match the expected parameters of the command. If null, the command is executed without parameters.</param>
        /// <param name="transaction">An optional database transaction within which the command should be executed. If null, the command is
        /// executed outside of a transaction.</param>
        /// <param name="commandType">Specifies how the command string is interpreted. Use CommandType.Text for raw SQL queries or
        /// CommandType.StoredProcedure for stored procedures. The default is CommandType.Text.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <param name="commandTimeout">An optional command timeout, in seconds, to apply to the execution of the command. If null, the default
        /// timeout for the connection is used.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the number of rows affected by the
        /// command.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the connection parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the sql parameter is null, empty, or consists only of white-space characters.</exception>
        public static async Task<int> ExecuteAsync(
            this IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
        {
            ValidateCommand(connection, sql);
            return await ExecutionEngine.ExecuteNonQueryAsync(
                sql,
                parameters,
                new ExecutionCommandOptions
                {
                    Connection = connection,
                    Transaction = transaction,
                    CommandType = commandType,
                    CommandTimeout = commandTimeout,
                    ParameterPrefix = HydrixConfiguration.Options.ParameterPrefix
                },
                cancellationToken)
            .ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously executes a stored procedure represented by the specified procedure object and returns the
        /// number of rows affected.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">The parameter driver type used by the procedure.</typeparam>
        /// <param name="connection">The database connection used to execute the procedure.</param>
        /// <param name="procedure">The stored procedure definition containing the command metadata and parameters.</param>
        /// <param name="transaction">An optional transaction in which to execute the procedure.</param>
        /// <param name="commandTimeout">An optional command timeout in seconds.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task containing the number of rows affected by the stored procedure execution.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="connection"/> or <paramref name="procedure"/> is null.</exception>
        public static async Task<int> ExecuteAsync<TDataParameterDriver>(
            this IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new()
        {
            ValidateProcedure(connection, procedure);
            return await ExecutionEngine.ExecuteNonQueryAsync(
                procedure,
                new ExecutionOptions
                {
                    Connection = connection,
                    Transaction = transaction,
                    CommandTimeout = commandTimeout,
                    ParameterPrefix = HydrixConfiguration.Options.ParameterPrefix
                },
                cancellationToken)
            .ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a SQL command and returns the value of the first column in the first row of the result set, cast to
        /// the specified type. If the result set is empty, returns the default value for the type.
        /// </summary>
        /// <remarks>Use this method to efficiently retrieve a single value from the database, such as an
        /// aggregate result or a specific field. The method automatically handles parameterization and transaction
        /// context if provided. If the result cannot be cast to the specified type, an InvalidCastException may be
        /// thrown at runtime.</remarks>
        /// <typeparam name="TResult">The type to which the result value is cast before being returned.</typeparam>
        /// <param name="connection">The database connection to use for executing the SQL command. This parameter cannot be null and must be open
        /// before calling the method.</param>
        /// <param name="sql">The SQL command to execute. This parameter cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="parameters">An optional object containing the parameters to be applied to the SQL command. The object's properties
        /// should match the named parameters in the SQL statement. If null, the command is executed without parameters.</param>
        /// <param name="transaction">An optional database transaction within which the command is executed. If null, the command executes outside
        /// of a transaction.</param>
        /// <param name="commandType">Specifies how the command string is interpreted. The default is CommandType.Text, indicating a raw SQL
        /// query.</param>
        /// <param name="commandTimeout">An optional command timeout, in seconds. If null, the default timeout for the connection is used.</param>
        /// <returns>The value of the first column in the first row of the result set, cast to the specified type. Returns the
        /// default value of the type if the result set is empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the connection parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the sql parameter is null, empty, or consists only of white-space characters.</exception>
        public static TResult ExecuteScalar<TResult>(
            this IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null)
        {
            ValidateCommand(connection, sql);
            return ExecutionEngine.ExecuteScalar(
                sql,
                parameters,
                new ExecutionCommandOptions
                {
                    Connection = connection,
                    Transaction = transaction,
                    CommandType = commandType,
                    CommandTimeout = commandTimeout
                })
            .As<TResult>();
        }

        /// <summary>
        /// Executes a stored procedure represented by the specified procedure object and returns the first column of
        /// the first row in the result set.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">The parameter driver type used by the procedure.</typeparam>
        /// <typeparam name="TResult">The type to which the result value is cast before being returned.</typeparam>
        /// <param name="connection">The database connection used to execute the procedure.</param>
        /// <param name="procedure">The stored procedure definition containing the command metadata and parameters.</param>
        /// <param name="transaction">An optional transaction in which to execute the procedure.</param>
        /// <param name="commandTimeout">An optional command timeout in seconds.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="connection"/> or <paramref name="procedure"/> is null.</exception>
        public static TResult ExecuteScalar<TDataParameterDriver, TResult>(
            this IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null)
            where TDataParameterDriver : IDataParameter, new()
        {
            ValidateProcedure(connection, procedure);
            return ExecutionEngine.ExecuteScalar(
                procedure,
                new ExecutionOptions
                {
                    Connection = connection,
                    Transaction = transaction,
                    CommandTimeout = commandTimeout,
                    ParameterPrefix = HydrixConfiguration.Options.ParameterPrefix
                })
            .As<TResult>();
        }

        /// <summary>
        /// Asynchronously executes a SQL command and returns the value of the first column in the first row of the
        /// result set, cast to the specified type.
        /// </summary>
        /// <remarks>If the result set is empty, the default value of the specified type is returned. This
        /// method is typically used for queries that return a single value, such as aggregate functions.</remarks>
        /// <typeparam name="TResult">The type to which the result value is cast before being returned.</typeparam>
        /// <param name="connection">The database connection to use for executing the SQL command. This parameter cannot be null and must be open
        /// before calling the method.</param>
        /// <param name="sql">The SQL command to execute. This parameter cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="parameters">An optional object containing the parameters to be passed to the SQL command. Can be null if the command
        /// does not require parameters.</param>
        /// <param name="transaction">An optional transaction within which to execute the command. If null, the command executes outside of a
        /// transaction.</param>
        /// <param name="commandType">Specifies how the command string is interpreted. The default is CommandType.Text.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <param name="commandTimeout">An optional command timeout, in seconds, to apply to the SQL command. If null, the default timeout for the
        /// connection is used.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the value of the first column in
        /// the first row of the result set, cast to the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the connection parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the sql parameter is null, empty, or consists only of white-space characters.</exception>
        public static async Task<TResult> ExecuteScalarAsync<TResult>(
            this IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
        {
            ValidateCommand(connection, sql);
            return (await ExecutionEngine.ExecuteScalarAsync(
                sql,
                parameters,
                new ExecutionCommandOptions
                {
                    Connection = connection,
                    Transaction = transaction,
                    CommandType = commandType,
                    CommandTimeout = commandTimeout,
                    ParameterPrefix = HydrixConfiguration.Options.ParameterPrefix
                },
                cancellationToken)
            .ConfigureAwait(false))
            .As<TResult>();
        }

        /// <summary>
        /// Asynchronously executes a stored procedure represented by the specified procedure object and returns the
        /// first column of the first row in the result set.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">The parameter driver type used by the procedure.</typeparam>
        /// <typeparam name="TResult">The type to which the result value is cast before being returned.</typeparam>
        /// <param name="connection">The database connection used to execute the procedure.</param>
        /// <param name="procedure">The stored procedure definition containing the command metadata and parameters.</param>
        /// <param name="transaction">An optional transaction in which to execute the procedure.</param>
        /// <param name="commandTimeout">An optional command timeout in seconds.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task containing the first column of the first row in the result set.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="connection"/> or <paramref name="procedure"/> is null.</exception>
        public static async Task<TResult> ExecuteScalarAsync<TDataParameterDriver, TResult>(
            this IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new()
        {
            ValidateProcedure(connection, procedure);

            return (await ExecutionEngine.ExecuteScalarAsync(
                procedure,
                new ExecutionOptions
                {
                    Connection = connection,
                    Transaction = transaction,
                    CommandTimeout = commandTimeout,
                    ParameterPrefix = HydrixConfiguration.Options.ParameterPrefix
                },
                cancellationToken)
            .ConfigureAwait(false))
            .As<TResult>();
        }

        /// <summary>
        /// Executes the specified SQL query using the provided database connection and returns a list of entities of
        /// type TEntity.
        /// </summary>
        /// <remarks>This method supports executing both raw SQL queries and stored procedures. The
        /// returned entities are materialized based on the mapping defined by TEntity. Ensure that the connection is
        /// open before calling this method.</remarks>
        /// <typeparam name="TEntity">The type of entities to return. TEntity must implement the ITable interface and have a parameterless
        /// constructor.</typeparam>
        /// <param name="connection">The database connection to use for executing the SQL query. This parameter cannot be null and must be open
        /// before calling the method.</param>
        /// <param name="sql">The SQL command to execute. This parameter cannot be null or empty.</param>
        /// <param name="parameters">An optional object containing the parameters to be used in the SQL command. Can be null if the query does
        /// not require parameters.</param>
        /// <param name="transaction">An optional database transaction within which to execute the command. If null, the command executes outside
        /// of a transaction.</param>
        /// <param name="commandType">Specifies how the command string is interpreted. Use CommandType.Text for raw SQL queries or
        /// CommandType.StoredProcedure for stored procedures.</param>
        /// <param name="limit">An optional limit on the number of records to process. If not specified, all records are processed.</param>
        /// <param name="commandTimeout">An optional timeout value, in seconds, for the command execution. If null, the default timeout for the
        /// connection is used.</param>
        /// <returns>A list of entities of type TEntity that represent the result set of the executed query.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the connection parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the sql parameter is null or empty.</exception>
        public static IList<TEntity> Query<TEntity>(
            this IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int limit = 0,
            int? commandTimeout = null)
            where TEntity : ITable, new()
        {
            ValidateCommand(connection, sql);
            return MaterializationEngine.Query<TEntity>(
                sql,
                parameters,
                new MaterializationCommandOptions
                {
                    Connection = connection,
                    Transaction = transaction,
                    CommandType = commandType,
                    CommandTimeout = commandTimeout,
                    ParameterPrefix = HydrixConfiguration.Options.ParameterPrefix,
                    Limit = limit
                });
        }

        /// <summary>
        /// Executes the specified stored procedure and returns a list of entities of type TEntity.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to return.</typeparam>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="connection">The database connection to use for executing the procedure.</param>
        /// <param name="procedure">The stored procedure definition containing the command metadata and parameters.</param>
        /// <param name="transaction">An optional database transaction within which to execute the command.</param>
        /// <param name="limit">An optional limit on the number of records to process. If not specified, all records are processed.</param>
        /// <param name="commandTimeout">An optional timeout value, in seconds, for the command execution.</param>
        /// <returns>A list of entities of type TEntity that represent the result set of the executed procedure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the connection or procedure parameter is null.</exception>
        public static IList<TEntity> Query<TEntity, TDataParameterDriver>(
            this IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int limit = 0,
            int? commandTimeout = null)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            ValidateProcedure(connection, procedure);
            return MaterializationEngine.Query<TEntity, TDataParameterDriver>(
                procedure,
                new MaterializationOptions
                {
                    Connection = connection,
                    Transaction = transaction,
                    CommandTimeout = commandTimeout,
                    ParameterPrefix = HydrixConfiguration.Options.ParameterPrefix,
                    Limit = limit
                });
        }

        /// <summary>
        /// Asynchronously executes a SQL query and returns a list of entities of type TEntity mapped from the result
        /// set.
        /// </summary>
        /// <remarks>The method uses the provided connection to execute the query asynchronously and
        /// materializes the results into instances of TEntity. The connection must be open before calling this method.
        /// If a transaction is provided, the query executes within that transaction context. The method supports both
        /// parameterized queries and stored procedures.</remarks>
        /// <typeparam name="TEntity">The type of entities to return. Must implement the ITable interface and have a parameterless constructor.</typeparam>
        /// <param name="connection">The database connection to use for executing the query. Cannot be null and must be open before calling this
        /// method.</param>
        /// <param name="sql">The SQL command to execute. Cannot be null or empty.</param>
        /// <param name="parameters">An optional object containing the parameters to be used with the SQL command. Can be null if the query does
        /// not require parameters.</param>
        /// <param name="transaction">An optional database transaction within which to execute the command. If null, the command executes outside
        /// of a transaction.</param>
        /// <param name="commandType">Specifies how the command string is interpreted. Use CommandType.Text for raw SQL queries or
        /// CommandType.StoredProcedure for stored procedures.</param>
        /// <param name="limit">An optional limit on the number of records to process. If not specified, all records are processed.</param>
        /// <param name="commandTimeout">An optional timeout value, in seconds, for executing the command. If null, the default timeout for the
        /// connection is used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a list of entities of type TEntity
        /// mapped from the query results.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the connection parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the sql parameter is null or empty.</exception>
        public static async Task<IList<TEntity>> QueryAsync<TEntity>(
            this IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int limit = 0,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
        {
            ValidateCommand(connection, sql);
            return await MaterializationEngine.QueryAsync<TEntity>(
                sql,
                parameters,
                new MaterializationCommandOptions
                {
                    Connection = connection,
                    Transaction = transaction,
                    CommandType = commandType,
                    CommandTimeout = commandTimeout,
                    ParameterPrefix = HydrixConfiguration.Options.ParameterPrefix,
                    Limit = limit
                },
                cancellationToken)
            .ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously executes the specified stored procedure and returns a list of entities of type TEntity.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to return.</typeparam>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="connection">The database connection to use for executing the procedure.</param>
        /// <param name="procedure">The stored procedure definition containing the command metadata and parameters.</param>
        /// <param name="transaction">An optional database transaction within which to execute the command.</param>
        /// <param name="limit">An optional limit on the number of records to process. If not specified, all records are processed.</param>
        /// <param name="commandTimeout">An optional timeout value, in seconds, for executing the command.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation with the mapped entity list.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the connection or procedure parameter is null.</exception>
        public static async Task<IList<TEntity>> QueryAsync<TEntity, TDataParameterDriver>(
            this IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int limit = 0,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            ValidateProcedure(connection, procedure);
            return await MaterializationEngine.QueryAsync<TEntity, TDataParameterDriver>(
                procedure,
                new MaterializationOptions
                {
                    Connection = connection,
                    Transaction = transaction,
                    CommandTimeout = commandTimeout,
                    ParameterPrefix = HydrixConfiguration.Options.ParameterPrefix,
                    Limit = limit
                },
                cancellationToken)
            .ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves the first entity of type TEntity that matches the specified SQL query from the database
        /// connection.
        /// </summary>
        /// <remarks>Use this method when you expect the query to return at least one result. If no
        /// results are found, an InvalidOperationException is thrown. To avoid exceptions when no results are expected,
        /// consider using a method that returns a default value instead.</remarks>
        /// <typeparam name="TEntity">The type of the entity to retrieve. Must implement the ITable interface and have a parameterless
        /// constructor.</typeparam>
        /// <param name="connection">The database connection to use for executing the query. The connection must be open before calling this
        /// method.</param>
        /// <param name="sql">The SQL query to execute in order to retrieve the entity.</param>
        /// <param name="parameters">An optional object containing the parameters to be passed to the SQL query. May be null if the query does
        /// not require parameters.</param>
        /// <param name="transaction">An optional transaction within which the command executes. May be null if no transaction is required.</param>
        /// <param name="commandType">Specifies how the command string is interpreted. The default is CommandType.Text.</param>
        /// <param name="commandTimeout">An optional command timeout in seconds. If null, the default timeout for the connection is used.</param>
        /// <returns>The first entity of type TEntity returned by the query.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the query does not return any results.</exception>
        public static TEntity QueryFirst<TEntity>(
            this IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null)
            where TEntity : ITable, new()
        {
            var result = connection.Query<TEntity>(
                sql,
                parameters,
                transaction,
                commandType,
                Constants.FirstRecordLimit,
                commandTimeout);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                throw new InvalidOperationException(SequenceContainsNoElementsMessage);

            return enumerator.Current;
        }

        /// <summary>
        /// Retrieves the first entity of type TEntity from the specified stored procedure result set.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to retrieve.</typeparam>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="connection">The database connection to use for executing the procedure.</param>
        /// <param name="procedure">The stored procedure definition containing the command metadata and parameters.</param>
        /// <param name="transaction">An optional transaction within which the command executes.</param>
        /// <param name="commandTimeout">An optional command timeout in seconds.</param>
        /// <returns>The first entity of type TEntity returned by the procedure.</returns>
        public static TEntity QueryFirst<TEntity, TDataParameterDriver>(
            this IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            var result = Query<TEntity, TDataParameterDriver>(
                connection,
                procedure,
                transaction,
                Constants.FirstRecordLimit,
                commandTimeout);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                throw new InvalidOperationException(SequenceContainsNoElementsMessage);

            return enumerator.Current;
        }

        /// <summary>
        /// Asynchronously executes a SQL query and returns the first entity of the specified type from the result set.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to return. Must implement the ITable interface and have a parameterless constructor.</typeparam>
        /// <param name="connection">The database connection to use for executing the SQL query. The connection must be open before calling this
        /// method.</param>
        /// <param name="sql">The SQL query to execute against the database.</param>
        /// <param name="parameters">An optional object containing the parameters to be passed to the SQL query. May be null if the query does
        /// not require parameters.</param>
        /// <param name="transaction">An optional database transaction within which to execute the query. May be null if no transaction is
        /// required.</param>
        /// <param name="commandType">Specifies how the command string is interpreted. The default is CommandType.Text.</param>
        /// <param name="commandTimeout">An optional timeout, in seconds, to wait before terminating the command execution. If null, the default
        /// timeout for the connection is used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first entity of type TEntity
        /// returned by the query.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the result set does not contain any elements.</exception>
        public static async Task<TEntity> QueryFirstAsync<TEntity>(
            this IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
        {
            var result = await connection.QueryAsync<TEntity>(
                sql,
                parameters,
                transaction,
                commandType,
                Constants.FirstRecordLimit,
                commandTimeout,
                cancellationToken)
            .ConfigureAwait(false);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                throw new InvalidOperationException(SequenceContainsNoElementsMessage);

            return enumerator.Current;
        }

        /// <summary>
        /// Asynchronously retrieves the first entity of type TEntity from the specified stored procedure result set.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to return.</typeparam>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="connection">The database connection to use for executing the procedure.</param>
        /// <param name="procedure">The stored procedure definition containing the command metadata and parameters.</param>
        /// <param name="transaction">An optional database transaction within which to execute the query.</param>
        /// <param name="commandTimeout">An optional timeout, in seconds, to wait before terminating the command execution.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task containing the first entity of type TEntity returned by the procedure.</returns>
        public static async Task<TEntity> QueryFirstAsync<TEntity, TDataParameterDriver>(
            this IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            var result = await QueryAsync<TEntity, TDataParameterDriver>(
                connection,
                procedure,
                transaction,
                Constants.FirstRecordLimit,
                commandTimeout,
                cancellationToken)
            .ConfigureAwait(false);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                throw new InvalidOperationException(SequenceContainsNoElementsMessage);

            return enumerator.Current;
        }

        /// <summary>
        /// Executes the specified SQL query and returns the first result mapped to an entity of type TEntity, or the
        /// default value for TEntity if no results are found.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to return. TEntity must implement the ITable interface and have a parameterless
        /// constructor.</typeparam>
        /// <param name="connection">The database connection used to execute the query. The connection must be open before calling this method.</param>
        /// <param name="sql">The SQL query to execute against the database.</param>
        /// <param name="parameters">An optional object containing the parameters to be passed to the SQL query. May be null if the query does
        /// not require parameters.</param>
        /// <param name="transaction">An optional transaction within which the command executes. May be null if no transaction is required.</param>
        /// <param name="commandType">Specifies how the command string is interpreted. The default is CommandType.Text.</param>
        /// <param name="commandTimeout">An optional command timeout, in seconds. If null, the default timeout for the connection is used.</param>
        /// <returns>The first entity of type TEntity returned by the query, or the default value for TEntity if the query
        /// returns no results.</returns>
        public static TEntity QueryFirstOrDefault<TEntity>(
            this IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null)
            where TEntity : ITable, new()
        {
            var result = connection.Query<TEntity>(
                sql,
                parameters,
                transaction,
                commandType,
                Constants.FirstRecordLimit,
                commandTimeout);

            using var enumerator = result.GetEnumerator();

            return enumerator.MoveNext()
                ? enumerator.Current
                : default;
        }

        /// <summary>
        /// Executes the specified stored procedure and returns the first result mapped to an entity of type TEntity,
        /// or the default value for TEntity if no results are found.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to return.</typeparam>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="connection">The database connection used to execute the procedure.</param>
        /// <param name="procedure">The stored procedure definition containing the command metadata and parameters.</param>
        /// <param name="transaction">An optional transaction within which the command executes.</param>
        /// <param name="commandTimeout">An optional command timeout, in seconds.</param>
        /// <returns>The first entity of type TEntity returned by the procedure, or the default value for TEntity.</returns>
        public static TEntity QueryFirstOrDefault<TEntity, TDataParameterDriver>(
            this IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            var result = Query<TEntity, TDataParameterDriver>(
                connection,
                procedure,
                transaction,
                Constants.FirstRecordLimit,
                commandTimeout);

            using var enumerator = result.GetEnumerator();

            return enumerator.MoveNext()
                ? enumerator.Current
                : default;
        }

        /// <summary>
        /// Asynchronously executes a SQL query and returns the first result mapped to the specified entity type, or the
        /// default value if no results are found.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to return. Must implement the ITable interface and have a parameterless constructor.</typeparam>
        /// <param name="connection">The database connection used to execute the query. Must be open before calling this method.</param>
        /// <param name="sql">The SQL query to execute against the database.</param>
        /// <param name="parameters">An optional object containing the parameters to be passed to the SQL query. May be null if the query does
        /// not require parameters.</param>
        /// <param name="transaction">An optional transaction within which the command executes. May be null if no transaction is required.</param>
        /// <param name="commandType">Specifies how the command string is interpreted. The default is CommandType.Text.</param>
        /// <param name="commandTimeout">An optional command timeout in seconds. If null, the default timeout for the connection is used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the first entity of type TEntity
        /// returned by the query, or the default value for TEntity if no results are found.</returns>
        public static async Task<TEntity> QueryFirstOrDefaultAsync<TEntity>(
            this IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
        {
            var result = await connection.QueryAsync<TEntity>(
                sql,
                parameters,
                transaction,
                commandType,
                Constants.FirstRecordLimit,
                commandTimeout,
                cancellationToken)
            .ConfigureAwait(false);

            using var enumerator = result.GetEnumerator();

            return enumerator.MoveNext()
                ? enumerator.Current
                : default;
        }

        /// <summary>
        /// Asynchronously executes the specified stored procedure and returns the first result mapped to TEntity,
        /// or the default value if no results are found.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to return.</typeparam>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="connection">The database connection used to execute the procedure.</param>
        /// <param name="procedure">The stored procedure definition containing the command metadata and parameters.</param>
        /// <param name="transaction">An optional transaction within which the command executes.</param>
        /// <param name="commandTimeout">An optional command timeout in seconds.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task containing the first entity or default value.</returns>
        public static async Task<TEntity> QueryFirstOrDefaultAsync<TEntity, TDataParameterDriver>(
            this IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            var result = await QueryAsync<TEntity, TDataParameterDriver>(
                connection,
                procedure,
                transaction,
                Constants.FirstRecordLimit,
                commandTimeout,
                cancellationToken)
            .ConfigureAwait(false);

            using var enumerator = result.GetEnumerator();

            return enumerator.MoveNext()
                ? enumerator.Current
                : default;
        }

        /// <summary>
        /// Executes the specified SQL query and returns a single entity of type TEntity. Throws an exception if the
        /// query returns no results or more than one result.
        /// </summary>
        /// <remarks>Use this method when the query is expected to return exactly one result. If the query
        /// may return zero or multiple results, consider using a different method such as QuerySingleOrDefault or
        /// Query.</remarks>
        /// <typeparam name="TEntity">The type of the entity to return. Must implement the ITable interface and have a parameterless constructor.</typeparam>
        /// <param name="connection">The database connection used to execute the query.</param>
        /// <param name="sql">The SQL query to execute against the database.</param>
        /// <param name="parameters">An optional object containing the parameters to be passed to the SQL query. May be null if the query does
        /// not require parameters.</param>
        /// <param name="transaction">An optional transaction within which the command executes. May be null if no transaction is required.</param>
        /// <param name="commandType">Specifies how the command string is interpreted. The default is CommandType.Text.</param>
        /// <param name="commandTimeout">An optional command timeout in seconds. If null, the default timeout for the connection is used.</param>
        /// <returns>A single entity of type TEntity that matches the query.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the query returns no results or more than one result.</exception>
        public static TEntity QuerySingle<TEntity>(
            this IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null)
            where TEntity : ITable, new()
        {
            var result = connection.Query<TEntity>(
                sql,
                parameters,
                transaction,
                commandType,
                Constants.SingleRecordLimit,
                commandTimeout);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                throw new InvalidOperationException(SequenceContainsNoElementsMessage);

            var first = enumerator.Current;

            if (enumerator.MoveNext())
                throw new InvalidOperationException(SequenceContainsMoreThanOneElementMessage);

            return first;
        }

        /// <summary>
        /// Executes the specified stored procedure and returns a single entity of type TEntity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to return.</typeparam>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="connection">The database connection used to execute the procedure.</param>
        /// <param name="procedure">The stored procedure definition containing the command metadata and parameters.</param>
        /// <param name="transaction">An optional transaction within which the command executes.</param>
        /// <param name="commandTimeout">An optional command timeout in seconds.</param>
        /// <returns>A single entity of type TEntity that matches the procedure result.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the procedure returns no results or more than one result.</exception>
        public static TEntity QuerySingle<TEntity, TDataParameterDriver>(
            this IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            var result = Query<TEntity, TDataParameterDriver>(
                connection,
                procedure,
                transaction,
                Constants.SingleRecordLimit,
                commandTimeout);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                throw new InvalidOperationException(SequenceContainsNoElementsMessage);

            var first = enumerator.Current;

            if (enumerator.MoveNext())
                throw new InvalidOperationException(SequenceContainsMoreThanOneElementMessage);

            return first;
        }

        /// <summary>
        /// Asynchronously executes a SQL query and returns a single entity of type TEntity. Throws an exception if the
        /// query returns no results or more than one result.
        /// </summary>
        /// <remarks>Use this method when you expect the query to return exactly one result. If the query
        /// returns zero or multiple results, an exception is thrown. This method is typically used for queries that
        /// select by a unique key or otherwise guarantee a single result.</remarks>
        /// <typeparam name="TEntity">The type of the entity to return. Must implement the ITable interface and have a parameterless constructor.</typeparam>
        /// <param name="connection">The database connection to use for executing the query. The connection must be open before calling this
        /// method.</param>
        /// <param name="sql">The SQL query to execute. The query should be written to return exactly one result.</param>
        /// <param name="parameters">An optional object containing the parameters to be passed to the SQL query. Can be null if the query does
        /// not require parameters.</param>
        /// <param name="transaction">An optional transaction within which the command executes. Can be null if no transaction is required.</param>
        /// <param name="commandType">Specifies how the command string is interpreted. The default is CommandType.Text.</param>
        /// <param name="commandTimeout">The command timeout in seconds. If null, the default timeout for the connection is used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the single entity of type TEntity
        /// returned by the query.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the query returns no results or more than one result.</exception>
        public static async Task<TEntity> QuerySingleAsync<TEntity>(
            this IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
        {
            var result = await connection.QueryAsync<TEntity>(
                sql,
                parameters,
                transaction,
                commandType,
                Constants.SingleRecordLimit,
                commandTimeout,
                cancellationToken)
            .ConfigureAwait(false);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                throw new InvalidOperationException(SequenceContainsNoElementsMessage);

            var first = enumerator.Current;

            if (enumerator.MoveNext())
                throw new InvalidOperationException(SequenceContainsMoreThanOneElementMessage);

            return first;
        }

        /// <summary>
        /// Asynchronously executes the specified stored procedure and returns a single entity of type TEntity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to return.</typeparam>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="connection">The database connection to use for executing the procedure.</param>
        /// <param name="procedure">The stored procedure definition containing the command metadata and parameters.</param>
        /// <param name="transaction">An optional transaction within which the command executes.</param>
        /// <param name="commandTimeout">The command timeout in seconds.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task containing the single entity of type TEntity returned by the procedure.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the procedure returns no results or more than one result.</exception>
        public static async Task<TEntity> QuerySingleAsync<TEntity, TDataParameterDriver>(
            this IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            var result = await QueryAsync<TEntity, TDataParameterDriver>(
                connection,
                procedure,
                transaction,
                Constants.SingleRecordLimit,
                commandTimeout,
                cancellationToken)
            .ConfigureAwait(false);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                throw new InvalidOperationException(SequenceContainsNoElementsMessage);

            var first = enumerator.Current;

            if (enumerator.MoveNext())
                throw new InvalidOperationException(SequenceContainsMoreThanOneElementMessage);

            return first;
        }

        /// <summary>
        /// Executes the specified SQL query and returns a single entity of type TEntity, or the default value if no
        /// result is found.
        /// </summary>
        /// <remarks>Use this method when you expect the query to return at most one result. If the query
        /// returns more than one row, an InvalidOperationException is thrown. If no rows are returned, the default
        /// value for TEntity is returned.</remarks>
        /// <typeparam name="TEntity">The type of the entity to return. TEntity must implement the ITable interface and have a parameterless
        /// constructor.</typeparam>
        /// <param name="connection">The database connection used to execute the query. The connection must be open before calling this method.</param>
        /// <param name="sql">The SQL query to execute. The query should be written to return at most one result.</param>
        /// <param name="parameters">An optional object containing the parameters to be passed to the SQL query. May be null if the query does
        /// not require parameters.</param>
        /// <param name="transaction">An optional transaction within which the query is executed. May be null if no transaction is required.</param>
        /// <param name="commandType">Specifies how the command string is interpreted. The default is CommandType.Text.</param>
        /// <param name="commandTimeout">An optional command timeout, in seconds. If null, the default timeout for the connection is used.</param>
        /// <returns>A single entity of type TEntity if exactly one result is returned by the query; the default value for
        /// TEntity if no results are found.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the query returns more than one result.</exception>
        public static TEntity QuerySingleOrDefault<TEntity>(
            this IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null)
            where TEntity : ITable, new()
        {
            var result = connection.Query<TEntity>(
                sql,
                parameters,
                transaction,
                commandType,
                Constants.SingleRecordLimit,
                commandTimeout);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                return default;

            var first = enumerator.Current;

            if (enumerator.MoveNext())
                throw new InvalidOperationException(SequenceContainsMoreThanOneElementMessage);

            return first;
        }

        /// <summary>
        /// Executes the specified stored procedure and returns a single entity of type TEntity, or the default value if no result is found.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to return.</typeparam>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="connection">The database connection used to execute the procedure.</param>
        /// <param name="procedure">The stored procedure definition containing the command metadata and parameters.</param>
        /// <param name="transaction">An optional transaction within which the query is executed.</param>
        /// <param name="commandTimeout">An optional command timeout, in seconds.</param>
        /// <returns>A single entity of type TEntity, or the default value if no results are found.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the procedure returns more than one result.</exception>
        public static TEntity QuerySingleOrDefault<TEntity, TDataParameterDriver>(
            this IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            var result = Query<TEntity, TDataParameterDriver>(
                connection,
                procedure,
                transaction,
                Constants.SingleRecordLimit,
                commandTimeout);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                return default;

            var first = enumerator.Current;

            if (enumerator.MoveNext())
                throw new InvalidOperationException(SequenceContainsMoreThanOneElementMessage);

            return first;
        }

        /// <summary>
        /// Asynchronously retrieves a single entity of the specified type from the database, or returns the default
        /// value if no entity is found. Throws an exception if more than one entity matches the query.
        /// </summary>
        /// <remarks>Use this method when you expect at most one result from the query. If no rows are
        /// returned, the default value for TEntity is returned. If more than one row is returned, an
        /// InvalidOperationException is thrown to indicate that the result set was not unique.</remarks>
        /// <typeparam name="TEntity">The type of the entity to retrieve. Must implement the ITable interface and have a parameterless
        /// constructor.</typeparam>
        /// <param name="connection">The database connection to use for executing the query. Must be open before calling this method.</param>
        /// <param name="sql">The SQL query to execute against the database. Should be written to return at most one result.</param>
        /// <param name="parameters">An optional object containing the parameters to be passed to the SQL query. Can be null if the query does
        /// not require parameters.</param>
        /// <param name="transaction">An optional database transaction within which the command executes. Can be null if no transaction is
        /// required.</param>
        /// <param name="commandType">Specifies how the command string is interpreted. Use CommandType.Text for raw SQL queries or
        /// CommandType.StoredProcedure for stored procedures.</param>
        /// <param name="commandTimeout">An optional command execution timeout, in seconds. If null, the default timeout for the connection is used.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation is canceled if the token is triggered.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the single entity of type TEntity
        /// if found; otherwise, the default value for TEntity.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the query returns more than one entity.</exception>
        public static async Task<TEntity> QuerySingleOrDefaultAsync<TEntity>(
            this IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
        {
            var result = await connection.QueryAsync<TEntity>(
                sql,
                parameters,
                transaction,
                commandType,
                Constants.SingleRecordLimit,
                commandTimeout,
                cancellationToken)
            .ConfigureAwait(false);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                return default;

            var first = enumerator.Current;

            if (enumerator.MoveNext())
                throw new InvalidOperationException(SequenceContainsMoreThanOneElementMessage);

            return first;
        }

        /// <summary>
        /// Asynchronously retrieves a single entity of the specified type from the stored procedure result, or returns default if no entity is found.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to retrieve.</typeparam>
        /// <typeparam name="TDataParameterDriver">The procedure parameter driver type.</typeparam>
        /// <param name="connection">The database connection to use for executing the procedure.</param>
        /// <param name="procedure">The stored procedure definition containing the command metadata and parameters.</param>
        /// <param name="transaction">An optional database transaction within which the command executes.</param>
        /// <param name="commandTimeout">An optional command execution timeout, in seconds.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task containing the single entity if found; otherwise, the default value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the procedure returns more than one entity.</exception>
        public static async Task<TEntity> QuerySingleOrDefaultAsync<TEntity, TDataParameterDriver>(
            this IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            var result = await QueryAsync<TEntity, TDataParameterDriver>(
                connection,
                procedure,
                transaction,
                Constants.SingleRecordLimit,
                commandTimeout,
                cancellationToken)
            .ConfigureAwait(false);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                return default;

            var first = enumerator.Current;

            if (enumerator.MoveNext())
                throw new InvalidOperationException(SequenceContainsMoreThanOneElementMessage);

            return first;
        }

        /// <summary>
        /// Validates the provided database connection and SQL command for correctness before execution.
        /// </summary>
        /// <param name="connection">The database connection to validate. This parameter cannot be null.</param>
        /// <param name="sql">The SQL command to validate. This parameter cannot be null or empty.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connection"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="sql"/> is null or empty.</exception>
        private static void ValidateCommand(
            IDbConnection connection,
            string sql)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(connection);
#else
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
#endif
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL command cannot be null or empty.", nameof(sql));
        }

        /// <summary>
        /// Validates the provided database connection and procedure instance for correctness before execution.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">The parameter driver type used by the procedure.</typeparam>
        /// <param name="connection">The database connection to validate. This parameter cannot be null.</param>
        /// <param name="procedure">The procedure instance to validate. This parameter cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connection"/> or <paramref name="procedure"/> is null.</exception>
        private static void ValidateProcedure<TDataParameterDriver>(
            IDbConnection connection,
            IProcedure<TDataParameterDriver> procedure)
            where TDataParameterDriver : IDataParameter, new()
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(connection);
            ArgumentNullException.ThrowIfNull(procedure);
#else
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (procedure == null)
                throw new ArgumentNullException(nameof(procedure));
#endif
        }
    }
}
