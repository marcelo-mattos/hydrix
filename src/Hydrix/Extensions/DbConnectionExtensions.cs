using Hydrix.Configuration;
using Hydrix.Orchestrator.Materializers;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrix.Extensions
{
    /// <summary>
    /// Provides extension methods for executing SQL commands against a database connection.
    /// </summary>
    /// <remarks>These methods allow for synchronous and asynchronous execution of SQL commands with optional
    /// parameters, transaction support, and command type specification. The methods throw an ArgumentNullException if
    /// the connection is null.</remarks>
    public static class DbConnectionExtensions
    {
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
            var materializer = CreateMaterializer(
                connection,
                commandTimeout);

            if (parameters != null)
            {
                if (commandType == CommandType.Text)
                    return transaction == null
                        ? materializer.ExecuteNonQuery(sql, parameters)
                        : materializer.ExecuteNonQuery(sql, parameters, transaction);

                var @params = parameters.AsIDataParameters();
                return transaction == null
                    ? materializer.ExecuteNonQuery(commandType, sql, @params)
                    : materializer.ExecuteNonQuery(commandType, sql, @params, transaction);
            }

            if (commandType == CommandType.Text)
                return transaction == null
                    ? materializer.ExecuteNonQuery(sql)
                    : materializer.ExecuteNonQuery(sql, transaction);

            return transaction == null
                ? materializer.ExecuteNonQuery(commandType, sql)
                : materializer.ExecuteNonQuery(commandType, sql, transaction);
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
        public static Task<int> ExecuteAsync(
            this IDbConnection connection,
            string sql,
            object parameters = null,
            IDbTransaction transaction = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
        {
            ValidateCommand(connection, sql);
            var materializer = CreateMaterializer(
                connection,
                commandTimeout);

            if (parameters != null)
            {
                if (commandType == CommandType.Text)
                    return transaction == null
                        ? materializer.ExecuteNonQueryAsync(sql, parameters, cancellationToken)
                        : materializer.ExecuteNonQueryAsync(sql, parameters, transaction, cancellationToken);

                var @params = parameters.AsIDataParameters();
                return transaction == null
                    ? materializer.ExecuteNonQueryAsync(commandType, sql, @params, cancellationToken)
                    : materializer.ExecuteNonQueryAsync(commandType, sql, @params, transaction, cancellationToken);
            }

            if (commandType == CommandType.Text)
                return transaction == null
                    ? materializer.ExecuteNonQueryAsync(sql, cancellationToken)
                    : materializer.ExecuteNonQueryAsync(sql, transaction, cancellationToken);

            return transaction == null
                ? materializer.ExecuteNonQueryAsync(commandType, sql, cancellationToken)
                : materializer.ExecuteNonQueryAsync(commandType, sql, transaction, cancellationToken);
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
            var materializer = CreateMaterializer(
                connection,
                commandTimeout);

            if (parameters != null)
            {
                if (commandType == CommandType.Text)
                    return transaction == null
                        ? materializer.ExecuteScalar(sql, parameters).As<TResult>()
                        : materializer.ExecuteScalar(sql, parameters, transaction).As<TResult>();

                var @params = parameters.AsIDataParameters();
                return transaction == null
                    ? materializer.ExecuteScalar(commandType, sql, @params).As<TResult>()
                    : materializer.ExecuteScalar(commandType, sql, @params, transaction).As<TResult>();
            }

            if (commandType == CommandType.Text)
                return transaction == null
                    ? materializer.ExecuteScalar(sql).As<TResult>()
                    : materializer.ExecuteScalar(sql, transaction).As<TResult>();

            return transaction == null
                ? materializer.ExecuteScalar(commandType, sql).As<TResult>()
                : materializer.ExecuteScalar(commandType, sql, transaction).As<TResult>();
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
            var materializer = CreateMaterializer(
                connection,
                commandTimeout);

            if (parameters != null)
            {
                if (commandType == CommandType.Text)
                    return transaction == null
                        ? (await materializer.ExecuteScalarAsync(sql, parameters, cancellationToken)).As<TResult>()
                        : (await materializer.ExecuteScalarAsync(sql, parameters, transaction, cancellationToken)).As<TResult>();

                var @params = parameters.AsIDataParameters();
                return transaction == null
                    ? (await materializer.ExecuteScalarAsync(commandType, sql, @params, cancellationToken)).As<TResult>()
                    : (await materializer.ExecuteScalarAsync(commandType, sql, @params, transaction, cancellationToken)).As<TResult>();
            }

            if (commandType == CommandType.Text)
                return transaction == null
                    ? (await materializer.ExecuteScalarAsync(sql, cancellationToken)).As<TResult>()
                    : (await materializer.ExecuteScalarAsync(sql, transaction, cancellationToken)).As<TResult>();

            return transaction == null
                ? (await materializer.ExecuteScalarAsync(commandType, sql, cancellationToken)).As<TResult>()
                : (await materializer.ExecuteScalarAsync(commandType, sql, transaction, cancellationToken)).As<TResult>();
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
            var materializer = CreateMaterializer(
                connection,
                commandTimeout);

            if (parameters != null)
            {
                if (commandType == CommandType.Text)
                    return transaction == null
                        ? materializer.Query<TEntity>(sql, parameters, limit)
                        : materializer.Query<TEntity>(sql, parameters, transaction, limit);

                var @params = parameters.AsIDataParameters();
                return transaction == null
                    ? materializer.Query<TEntity>(commandType, sql, @params, limit)
                    : materializer.Query<TEntity>(commandType, sql, @params, transaction, limit);
            }

            if (commandType == CommandType.Text)
                return transaction == null
                    ? materializer.Query<TEntity>(sql, limit)
                    : materializer.Query<TEntity>(sql, transaction, limit);

            return transaction == null
                ? materializer.Query<TEntity>(commandType, sql, limit)
                : materializer.Query<TEntity>(commandType, sql, transaction, limit);
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
            var materializer = CreateMaterializer(
                connection,
                commandTimeout);

            if (parameters != null)
            {
                if (commandType == CommandType.Text)
                    return transaction == null
                        ? await materializer.QueryAsync<TEntity>(sql, parameters, limit, cancellationToken)
                        : await materializer.QueryAsync<TEntity>(sql, parameters, transaction, limit, cancellationToken);

                var @params = parameters.AsIDataParameters();
                return transaction == null
                    ? await materializer.QueryAsync<TEntity>(commandType, sql, @params, limit, cancellationToken)
                    : await materializer.QueryAsync<TEntity>(commandType, sql, @params, transaction, limit, cancellationToken);
            }

            if (commandType == CommandType.Text)
                return transaction == null
                    ? await materializer.QueryAsync<TEntity>(sql, limit, cancellationToken)
                    : await materializer.QueryAsync<TEntity>(sql, transaction, limit, cancellationToken);

            return transaction == null
                ? await materializer.QueryAsync<TEntity>(commandType, sql, limit, cancellationToken)
                : await materializer.QueryAsync<TEntity>(commandType, sql, transaction, limit, cancellationToken);
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
                Materializer.FirstRecordLimit,
                commandTimeout);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                throw new InvalidOperationException("Sequence contains no elements.");

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
                Materializer.FirstRecordLimit,
                commandTimeout,
                cancellationToken);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                throw new InvalidOperationException("Sequence contains no elements.");

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
                Materializer.FirstRecordLimit,
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
                Materializer.FirstRecordLimit,
                commandTimeout,
                cancellationToken);

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
                Materializer.SingleRecordLimit,
                commandTimeout);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                throw new InvalidOperationException("Sequence contains no elements.");

            var first = enumerator.Current;

            if (enumerator.MoveNext())
                throw new InvalidOperationException("Sequence contains more than one element.");

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
                Materializer.SingleRecordLimit,
                commandTimeout,
                cancellationToken);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                throw new InvalidOperationException("Sequence contains no elements.");

            var first = enumerator.Current;

            if (enumerator.MoveNext())
                throw new InvalidOperationException("Sequence contains more than one element.");

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
                Materializer.SingleRecordLimit,
                commandTimeout);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                return default;

            var first = enumerator.Current;

            if (enumerator.MoveNext())
                throw new InvalidOperationException("Sequence contains more than one element.");

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
                Materializer.SingleRecordLimit,
                commandTimeout,
                cancellationToken);

            using var enumerator = result.GetEnumerator();

            if (!enumerator.MoveNext())
                return default;

            var first = enumerator.Current;

            if (enumerator.MoveNext())
                throw new InvalidOperationException("Sequence contains more than one element.");

            return first;
        }

        /// <summary>
        /// Creates a new instance of the Materializer class using the specified database connection and command timeout
        /// settings.
        /// </summary>
        /// <remarks>The caller is responsible for managing the lifetime of the provided database
        /// connection. The Materializer uses the specified timeout for all executed commands.</remarks>
        /// <param name="connection">The open database connection to be used by the Materializer. This connection must remain valid for the
        /// lifetime of the Materializer instance.</param>
        /// <param name="commandTimeout">An optional command timeout, in seconds, to apply to database operations. If null, the default timeout from
        /// configuration is used.</param>
        /// <returns>A Materializer instance configured with the provided connection and timeout.</returns>
        private static Materializer CreateMaterializer(
            IDbConnection connection,
            int? commandTimeout)
        {
            var options = HydrixConfiguration.Options;

            return new Materializer(
                connection,
                options.Logger,
                commandTimeout ?? options.CommandTimeout,
                options.ParameterPrefix);
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
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL command cannot be null or empty.", nameof(sql));
        }
    }
}
