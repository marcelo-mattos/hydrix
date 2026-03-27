using Hydrix.Defaults;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// Provides methods to execute SQL commands against a data source and parse the results into ITable objects.
    /// </summary>
    /// <remarks>The Materializer class is responsible for executing commands and handling the retrieval of
    /// data from a database, ensuring that the results are properly mapped to the specified entity types. It supports
    /// both synchronous and asynchronous operations, as well as transaction management. This class is typically used to
    /// retrieve single entities or default values from query results, and is designed to work with various command
    /// types and parameter configurations.</remarks>
    public partial class Materializer :
        Contract.IMaterializer
    {
        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public TEntity SingleOrDefault<TEntity>(
            string sql,
            object parameters,
            int? timeout = null)
            where TEntity : ITable, new()
        {
            var result = Query<TEntity>(
                sql,
                parameters,
                Constants.SingleRecordLimit,
                timeout);

            return result.Count != 1
                ? default
                : result[0];
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public TEntity SingleOrDefault<TEntity>(
            string sql,
            object parameters,
            IDbTransaction transaction,
            int? timeout = null)
            where TEntity : ITable, new()
        {
            var result = Query<TEntity>(
                sql,
                parameters,
                transaction,
                Constants.SingleRecordLimit,
                timeout);

            return result.Count != 1
                ? default
                : result[0];
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public TEntity SingleOrDefault<TEntity>(
            string sql,
            int? timeout = null)
            where TEntity : ITable, new()
            => SingleOrDefault<TEntity>(
                sql,
                (object)null,
                timeout);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public TEntity SingleOrDefault<TEntity>(
            string sql,
            IDbTransaction transaction,
            int? timeout = null)
            where TEntity : ITable, new()
            => SingleOrDefault<TEntity>(
                sql,
                (object)null,
                transaction,
                timeout);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public TEntity SingleOrDefault<TEntity>(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            int? timeout = null)
            where TEntity : ITable, new()
        {
            var result = Query<TEntity>(
                commandType,
                sql,
                parameters,
                Constants.SingleRecordLimit,
                timeout);

            return result.Count != 1
                ? default
                : result[0];
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public TEntity SingleOrDefault<TEntity>(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            int? timeout = null)
            where TEntity : ITable, new()
        {
            var result = Query<TEntity>(
                commandType,
                sql,
                parameters,
                transaction,
                Constants.SingleRecordLimit,
                timeout);

            return result.Count != 1
                ? default
                : result[0];
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public TEntity SingleOrDefault<TEntity>(
            CommandType commandType,
            string sql,
            int? timeout = null)
            where TEntity : ITable, new()
            => SingleOrDefault<TEntity>(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                timeout);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public TEntity SingleOrDefault<TEntity>(
            CommandType commandType,
            string sql,
            IDbTransaction transaction,
            int? timeout = null)
            where TEntity : ITable, new()
            => SingleOrDefault<TEntity>(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                transaction,
                timeout);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The entity does not have a TableAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<TEntity> SingleOrDefaultAsync<TEntity>(
            string sql,
            object parameters,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
        {
            var result = await this
                .QueryAsync<TEntity>(
                    sql,
                    parameters,
                Constants.SingleRecordLimit,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

            return result.Count != 1
                ? default
                : result[0];
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
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
        public async Task<TEntity> SingleOrDefaultAsync<TEntity>(
            string sql,
            object parameters,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
        {
            var result = await this
                .QueryAsync<TEntity>(
                    sql,
                    parameters,
                    transaction,
                Constants.SingleRecordLimit,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

            return result.Count != 1
                ? default
                : result[0];
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
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
        public async Task<TEntity> SingleOrDefaultAsync<TEntity>(
            string sql,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            => await this
                .SingleOrDefaultAsync<TEntity>(
                    sql,
                    (object)null,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
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
        public async Task<TEntity> SingleOrDefaultAsync<TEntity>(
            string sql,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            => await this
                .SingleOrDefaultAsync<TEntity>(
                    sql,
                    (object)null,
                    transaction,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
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
        public async Task<TEntity> SingleOrDefaultAsync<TEntity>(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
        {
            var result = await this
                .QueryAsync<TEntity>(
                    commandType,
                    sql,
                    parameters,
                Constants.SingleRecordLimit,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

            return result.Count != 1
                ? default
                : result[0];
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
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
        public async Task<TEntity> SingleOrDefaultAsync<TEntity>(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
        {
            var result = await this
                .QueryAsync<TEntity>(
                    commandType,
                    sql,
                    parameters,
                    transaction,
                Constants.SingleRecordLimit,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

            return result.Count != 1
                ? default
                : result[0];
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
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
        public async Task<TEntity> SingleOrDefaultAsync<TEntity>(
            CommandType commandType,
            string sql,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            => await this
                .SingleOrDefaultAsync<TEntity>(
                    commandType,
                    sql,
                    (IEnumerable<IDataParameter>)null,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
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
        public async Task<TEntity> SingleOrDefaultAsync<TEntity>(
            CommandType commandType,
            string sql,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            => await this
                .SingleOrDefaultAsync<TEntity>(
                    commandType,
                    sql,
                    (IEnumerable<IDataParameter>)null,
                    transaction,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlEntity does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public TEntity SingleOrDefault<TEntity, TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            int? timeout = null)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            var result = Query<TEntity, TDataParameterDriver>(
                procedure,
                Constants.SingleRecordLimit,
                timeout);

            return result.Count != 1
                ? default
                : result[0];
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlEntity does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public TEntity SingleOrDefault<TEntity, TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            int? timeout = null)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            var result = Query<TEntity, TDataParameterDriver>(
                procedure,
                transaction,
                Constants.SingleRecordLimit,
                timeout);

            return result.Count != 1
                ? default
                : result[0];
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
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
        public async Task<TEntity> SingleOrDefaultAsync<TEntity, TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            var result = await this
                .QueryAsync<TEntity, TDataParameterDriver>(
                    procedure,
                Constants.SingleRecordLimit,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

            return result.Count != 1
                ? default
                : result[0];
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ITable object returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
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
        public async Task<TEntity> SingleOrDefaultAsync<TEntity, TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            var result = await this
                .QueryAsync<TEntity, TDataParameterDriver>(
                    procedure,
                    transaction,
                Constants.SingleRecordLimit,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

            return result.Count != 1
                ? default
                : result[0];
        }
    }
}