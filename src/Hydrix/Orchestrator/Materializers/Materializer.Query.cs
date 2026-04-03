using Hydrix.Engines;
using Hydrix.Engines.Options;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// Provides methods to execute SQL commands against a data source and retrieve the results as a collection of
    /// entities.
    /// </summary>
    /// <remarks>The Materializer class implements the IMaterializer interface and is designed to facilitate
    /// data retrieval from a database using various query methods. It supports both synchronous and asynchronous
    /// operations, allowing for flexible data access patterns. The class enables mapping of query results to
    /// strongly-typed entities, and supports parameterized queries, transactions, and cancellation tokens for advanced
    /// scenarios.</remarks>
    public partial class Materializer :
        Contract.IMaterializer
    {
        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public IList<TEntity> Query<TEntity>(
            string sql,
            object parameters,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new()
            => MaterializationEngine.Query<TEntity>(
                sql,
                parameters,
                new MaterializationCommandOptions
                {
                    Connection = DbConnection,
                    CommandType = CommandType.Text,
                    CommandTimeout = timeout,
                    ParameterPrefix = _parameterPrefix,
                    Limit = limit
                });

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public IList<TEntity> Query<TEntity>(
            string sql,
            object parameters,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new()
            => MaterializationEngine.Query<TEntity>(
                sql,
                parameters,
                new MaterializationCommandOptions
                {
                    Connection = DbConnection,
                    Transaction = transaction,
                    CommandType = CommandType.Text,
                    CommandTimeout = timeout,
                    ParameterPrefix = _parameterPrefix,
                    Limit = limit
                });

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public IList<TEntity> Query<TEntity>(
            string sql,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new()
            => Query<TEntity>(
                sql,
                (object)null,
                limit,
                timeout);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public IList<TEntity> Query<TEntity>(
            string sql,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new()
            => Query<TEntity>(
                sql,
                (object)null,
                transaction,
                limit,
                timeout);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public IList<TEntity> Query<TEntity>(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new()
            => MaterializationEngine.Query<TEntity>(
                sql,
                parameters,
                new MaterializationCommandOptions
                {
                    Connection = DbConnection,
                    CommandType = commandType,
                    CommandTimeout = timeout,
                    ParameterPrefix = _parameterPrefix,
                    Limit = limit
                });

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public IList<TEntity> Query<TEntity>(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new()
            => MaterializationEngine.Query<TEntity>(
                sql,
                parameters,
                new MaterializationCommandOptions
                {
                    Connection = DbConnection,
                    Transaction = transaction,
                    CommandType = commandType,
                    CommandTimeout = timeout,
                    ParameterPrefix = _parameterPrefix,
                    Limit = limit
                });

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public IList<TEntity> Query<TEntity>(
            CommandType commandType,
            string sql,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new()
            => Query<TEntity>(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                limit,
                timeout);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public IList<TEntity> Query<TEntity>(
            CommandType commandType,
            string sql,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new()
            => Query<TEntity>(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                transaction,
                limit,
                timeout);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public IList<TEntity> Query<TEntity, TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
            => MaterializationEngine.Query<TEntity, TDataParameterDriver>(
                procedure,
                new MaterializationOptions
                {
                    Connection = DbConnection,
                    CommandTimeout = timeout,
                    ParameterPrefix = _parameterPrefix,
                    Limit = limit
                });

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public IList<TEntity> Query<TEntity, TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
            => MaterializationEngine.Query<TEntity, TDataParameterDriver>(
                procedure,
                new MaterializationOptions
                {
                    Connection = DbConnection,
                    Transaction = transaction,
                    CommandTimeout = timeout,
                    ParameterPrefix = _parameterPrefix,
                    Limit = limit
                });

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<IList<TEntity>> QueryAsync<TEntity>(
            string sql,
            object parameters,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            => await MaterializationEngine.QueryAsync<TEntity>(
                    sql,
                    parameters,
                    new MaterializationCommandOptions
                    {
                        Connection = DbConnection,
                        CommandType = CommandType.Text,
                        CommandTimeout = timeout,
                        ParameterPrefix = _parameterPrefix,
                        Limit = limit
                    },
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<IList<TEntity>> QueryAsync<TEntity>(
            string sql,
            object parameters,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            => await MaterializationEngine.QueryAsync<TEntity>(
                    sql,
                    parameters,
                    new MaterializationCommandOptions
                    {
                        Connection = DbConnection,
                        Transaction = transaction,
                        CommandType = CommandType.Text,
                        CommandTimeout = timeout,
                        ParameterPrefix = _parameterPrefix,
                        Limit = limit
                    },
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<IList<TEntity>> QueryAsync<TEntity>(
            string sql,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            => await QueryAsync<TEntity>(
                    sql,
                    (object)null,
                    limit,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<IList<TEntity>> QueryAsync<TEntity>(
            string sql,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            => await QueryAsync<TEntity>(
                    sql,
                    (object)null,
                    transaction,
                    limit,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<IList<TEntity>> QueryAsync<TEntity>(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            => await MaterializationEngine.QueryAsync<TEntity>(
                    sql,
                    parameters,
                    new MaterializationCommandOptions
                    {
                        Connection = DbConnection,
                        CommandType = commandType,
                        CommandTimeout = timeout,
                        ParameterPrefix = _parameterPrefix,
                        Limit = limit
                    },
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<IList<TEntity>> QueryAsync<TEntity>(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            => await MaterializationEngine.QueryAsync<TEntity>(
                    sql,
                    parameters,
                    new MaterializationCommandOptions
                    {
                        Connection = DbConnection,
                        Transaction = transaction,
                        CommandType = commandType,
                        CommandTimeout = timeout,
                        ParameterPrefix = _parameterPrefix,
                        Limit = limit
                    },
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<IList<TEntity>> QueryAsync<TEntity>(
            CommandType commandType,
            string sql,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            => await QueryAsync<TEntity>(
                    commandType,
                    sql,
                    (IEnumerable<IDataParameter>)null,
                    limit,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<IList<TEntity>> QueryAsync<TEntity>(
            CommandType commandType,
            string sql,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            => await QueryAsync<TEntity>(
                    commandType,
                    sql,
                    (IEnumerable<IDataParameter>)null,
                    transaction,
                    limit,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<IList<TEntity>> QueryAsync<TEntity, TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
            => await MaterializationEngine.QueryAsync<TEntity, TDataParameterDriver>(
                    procedure,
                    new MaterializationOptions
                    {
                        Connection = DbConnection,
                        CommandTimeout = timeout,
                        ParameterPrefix = _parameterPrefix,
                        Limit = limit
                    },
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<IList<TEntity>> QueryAsync<TEntity, TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
            => await MaterializationEngine.QueryAsync<TEntity, TDataParameterDriver>(
                    procedure,
                    new MaterializationOptions
                    {
                        Connection = DbConnection,
                        Transaction = transaction,
                        CommandTimeout = timeout,
                        ParameterPrefix = _parameterPrefix,
                        Limit = limit
                    },
                    cancellationToken)
                .ConfigureAwait(false);
    }
}
