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
    /// Provides methods for executing SQL statements and stored procedures against a data provider, supporting both
    /// synchronous and asynchronous operations.
    /// </summary>
    /// <remarks>The Materializer class implements the IMaterializer interface and offers a variety of
    /// overloads for executing non-query SQL commands, such as INSERT, UPDATE, and DELETE, as well as stored
    /// procedures. It supports execution with or without parameters, transactions, and cancellation tokens, enabling
    /// flexible integration with different database providers. Both synchronous and asynchronous methods are available
    /// to accommodate different application requirements. Exceptions are thrown for invalid arguments, disposed
    /// connections, or unsupported operations, so callers should ensure that connections are open and parameters are
    /// valid before invoking these methods.</remarks>
    public partial class Materializer :
        Contract.IMaterializer
    {
        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery(
            string sql,
            object parameters,
            int? timeout = null)
            => ExecutionEngine.ExecuteNonQuery(
                sql,
                parameters,
                new ExecutionCommandOptions
                {
                    Connection = DbConnection,
                    CommandType = CommandType.Text,
                    CommandTimeout = timeout,
                    ParameterPrefix = _parameterPrefix
                });

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery(
            string sql,
            object parameters,
            IDbTransaction transaction,
            int? timeout = null)
            => ExecutionEngine.ExecuteNonQuery(
                sql,
                parameters,
                new ExecutionCommandOptions
                {
                    Connection = DbConnection,
                    Transaction = transaction,
                    CommandType = CommandType.Text,
                    CommandTimeout = timeout,
                    ParameterPrefix = _parameterPrefix
                });

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery(
            string sql,
            int? timeout = null)
            => ExecuteNonQuery(
                sql,
                (object)null,
                timeout);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery(
            string sql,
            IDbTransaction transaction,
            int? timeout = null)
            => ExecuteNonQuery(
                sql,
                (object)null,
                transaction,
                timeout);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            int? timeout = null)
            => ExecutionEngine.ExecuteNonQuery(
                sql,
                parameters,
                new ExecutionCommandOptions
                {
                    Connection = DbConnection,
                    CommandType = commandType,
                    CommandTimeout = timeout,
                    ParameterPrefix = _parameterPrefix
                });

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            int? timeout = null)
            => ExecutionEngine.ExecuteNonQuery(
                sql,
                parameters,
                new ExecutionCommandOptions
                {
                    Connection = DbConnection,
                    Transaction = transaction,
                    CommandType = commandType,
                    CommandTimeout = timeout,
                    ParameterPrefix = _parameterPrefix
                });

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery(
            CommandType commandType,
            string sql,
            int? timeout = null)
            => ExecuteNonQuery(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                timeout);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery(
            CommandType commandType,
            string sql,
            IDbTransaction transaction,
            int? timeout = null)
            => ExecuteNonQuery(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                transaction,
                timeout);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            int? timeout = null)
            where TDataParameterDriver : IDataParameter, new()
            => ExecutionEngine.ExecuteNonQuery(
                procedure,
                new ExecutionOptions
                {
                    Connection = DbConnection,
                    CommandTimeout = timeout,
                    ParameterPrefix = _parameterPrefix
                });

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            int? timeout = null)
            where TDataParameterDriver : IDataParameter, new()
            => ExecutionEngine.ExecuteNonQuery(
                procedure,
                new ExecutionOptions
                {
                    Connection = DbConnection,
                    Transaction = transaction,
                    CommandTimeout = timeout,
                    ParameterPrefix = _parameterPrefix
                });

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<int> ExecuteNonQueryAsync(
            string sql,
            object parameters,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            => await ExecutionEngine.ExecuteNonQueryAsync(
                    sql,
                    parameters,
                    new ExecutionCommandOptions
                    {
                        Connection = DbConnection,
                        CommandType = CommandType.Text,
                        CommandTimeout = timeout,
                        ParameterPrefix = _parameterPrefix
                    },
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<int> ExecuteNonQueryAsync(
            string sql,
            object parameters,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            => await ExecutionEngine.ExecuteNonQueryAsync(
                    sql,
                    parameters,
                    new ExecutionCommandOptions
                    {
                        Connection = DbConnection,
                        Transaction = transaction,
                        CommandType = CommandType.Text,
                        CommandTimeout = timeout,
                        ParameterPrefix = _parameterPrefix
                    },
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<int> ExecuteNonQueryAsync(
            string sql,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            => await ExecuteNonQueryAsync(
                    sql,
                    (object)null,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<int> ExecuteNonQueryAsync(
            string sql,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            => await ExecuteNonQueryAsync(
                    sql,
                    (object)null,
                    transaction,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<int> ExecuteNonQueryAsync(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            => await ExecutionEngine.ExecuteNonQueryAsync(
                    sql,
                    parameters,
                    new ExecutionCommandOptions
                    {
                        Connection = DbConnection,
                        CommandType = commandType,
                        CommandTimeout = timeout,
                        ParameterPrefix = _parameterPrefix
                    },
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<int> ExecuteNonQueryAsync(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            => await ExecutionEngine.ExecuteNonQueryAsync(
                    sql,
                    parameters,
                    new ExecutionCommandOptions
                    {
                        Connection = DbConnection,
                        Transaction = transaction,
                        CommandType = commandType,
                        CommandTimeout = timeout,
                        ParameterPrefix = _parameterPrefix
                    },
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<int> ExecuteNonQueryAsync(
            CommandType commandType,
            string sql,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            => await ExecuteNonQueryAsync(
                    commandType,
                    sql,
                    (IEnumerable<IDataParameter>)null,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<int> ExecuteNonQueryAsync(
            CommandType commandType,
            string sql,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            => await ExecuteNonQueryAsync(
                    commandType,
                    sql,
                    (IEnumerable<IDataParameter>)null,
                    transaction,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<int> ExecuteNonQueryAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new()
            => await ExecutionEngine.ExecuteNonQueryAsync(
                    procedure,
                    new ExecutionOptions
                    {
                        Connection = DbConnection,
                        CommandTimeout = timeout,
                        ParameterPrefix = _parameterPrefix
                    },
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="transaction">The database transaction to use.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<int> ExecuteNonQueryAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new()
            => await ExecutionEngine.ExecuteNonQueryAsync(
                    procedure,
                    new ExecutionOptions
                    {
                        Connection = DbConnection,
                        Transaction = transaction,
                        CommandTimeout = timeout,
                        ParameterPrefix = _parameterPrefix
                    },
                    cancellationToken)
                .ConfigureAwait(false);
    }
}
