using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// Provides methods to execute SQL commands against a data source and build a DataSet from the results.
    /// </summary>
    /// <remarks>The Materializer class implements the IMaterializer interface and is designed to facilitate
    /// data retrieval from a database using various command types and parameters. It supports both synchronous and
    /// asynchronous operations, allowing for flexible integration with different data access scenarios. Callers can
    /// specify SQL statements, command types, parameters, and transactions as needed. Exceptions are thrown for invalid
    /// arguments, disposed connections, or unsupported operations. Asynchronous methods accept a CancellationToken to
    /// support cooperative cancellation.</remarks>
    public partial class Materializer :
        Contract.IMaterializer
    {
        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        public DataSet ExecuteDataSet(
            string sql,
            object parameters,
            int? timeout = null)
        {
            using var dataTable = (this as Contract.IMaterializer)
                .ExecuteTable(
                    sql,
                    parameters,
                    timeout);

            var dataSet = new DataSet(nameof(Materializer));
            dataSet.Tables.Add(
                dataTable);

            return dataSet;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        public DataSet ExecuteDataSet(
            string sql,
            object parameters,
            IDbTransaction transaction,
            int? timeout = null)
        {
            using var dataTable = (this as Contract.IMaterializer)
                .ExecuteTable(
                    sql,
                    parameters,
                    transaction,
                    timeout);

            var dataSet = new DataSet(nameof(Materializer));
            dataSet.Tables.Add(
                dataTable);

            return dataSet;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        public DataSet ExecuteDataSet(
            string sql,
            int? timeout = null)
            => ExecuteDataSet(
                sql,
                (object)null,
                timeout);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        public DataSet ExecuteDataSet(
            string sql,
            IDbTransaction transaction,
            int? timeout = null)
            => ExecuteDataSet(
                sql,
                (object)null,
                transaction,
                timeout);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        public DataSet ExecuteDataSet(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            int? timeout = null)
        {
            using var dataTable = (this as Contract.IMaterializer)
                .ExecuteTable(
                    commandType,
                    sql,
                    parameters,
                    timeout);

            var dataSet = new DataSet(nameof(Materializer));
            dataSet.Tables.Add(
                dataTable);

            return dataSet;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        public DataSet ExecuteDataSet(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            int? timeout = null)
        {
            using var dataTable = (this as Contract.IMaterializer)
                .ExecuteTable(
                    commandType,
                    sql,
                    parameters,
                    transaction,
                    timeout);

            var dataSet = new DataSet(nameof(Materializer));
            dataSet.Tables.Add(
                dataTable);

            return dataSet;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        public DataSet ExecuteDataSet(
            CommandType commandType,
            string sql,
            int? timeout = null)
            => ExecuteDataSet(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                timeout);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        public DataSet ExecuteDataSet(
            CommandType commandType,
            string sql,
            IDbTransaction transaction,
            int? timeout = null)
            => ExecuteDataSet(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                transaction,
                timeout);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<DataSet> ExecuteDataSetAsync(
            string sql,
            object parameters,
            int? timeout = null,
            CancellationToken cancellationToken = default)
        {
            using var dataTable = await (this as Contract.IMaterializer)
                .ExecuteTableAsync(
                    sql,
                    parameters,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

            var dataSet = new DataSet(nameof(Materializer));
            dataSet.Tables.Add(
                dataTable);

            return dataSet;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<DataSet> ExecuteDataSetAsync(
            string sql,
            object parameters,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default)
        {
            using var dataTable = await (this as Contract.IMaterializer)
                .ExecuteTableAsync(
                    sql,
                    parameters,
                    transaction,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

            var dataSet = new DataSet(nameof(Materializer));
            dataSet.Tables.Add(
                dataTable);

            return dataSet;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<DataSet> ExecuteDataSetAsync(
            string sql,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            => await ExecuteDataSetAsync(
                sql,
                (object)null,
                timeout,
                cancellationToken)
            .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<DataSet> ExecuteDataSetAsync(
            string sql,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            => await ExecuteDataSetAsync(
                sql,
                (object)null,
                transaction,
                timeout,
                cancellationToken)
            .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<DataSet> ExecuteDataSetAsync(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            int? timeout = null,
            CancellationToken cancellationToken = default)
        {
            using var dataTable = await (this as Contract.IMaterializer)
                .ExecuteTableAsync(
                    commandType,
                    sql,
                    parameters,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

            var dataSet = new DataSet(nameof(Materializer));
            dataSet.Tables.Add(
                dataTable);

            return dataSet;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<DataSet> ExecuteDataSetAsync(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default)
        {
            using var dataTable = await (this as Contract.IMaterializer)
                .ExecuteTableAsync(
                    commandType,
                    sql,
                    parameters,
                    transaction,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

            var dataSet = new DataSet(nameof(Materializer));
            dataSet.Tables.Add(
                dataTable);

            return dataSet;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<DataSet> ExecuteDataSetAsync(
            CommandType commandType,
            string sql,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            => await ExecuteDataSetAsync(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                timeout,
                cancellationToken)
            .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<DataSet> ExecuteDataSetAsync(
            CommandType commandType,
            string sql,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            => await ExecuteDataSetAsync(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                transaction,
                timeout,
                cancellationToken)
            .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to
        /// System.Data.DataSet columns; and is implemented by .NET Framework data providers that
        /// access data sources.
        /// </typeparam>
        /// <param name="procedure">
        /// Represents a Sql Entity that holds the data parameters to be executed by the connection command.
        /// </param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        public DataSet ExecuteDataSet<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            int? timeout = null)
            where TDataParameterDriver : IDataParameter, new()
        {
            using var dataTable = (this as Contract.IMaterializer)
                .ExecuteTable(
                    procedure,
                    timeout);

            var dataSet = new DataSet(nameof(Materializer));
            dataSet.Tables.Add(
                dataTable);

            return dataSet;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to
        /// System.Data.DataSet columns; and is implemented by .NET Framework data providers that
        /// access data sources.
        /// </typeparam>
        /// <param name="procedure">
        /// Represents a Sql Entity that holds the data parameters to be executed by the connection command.
        /// </param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        public DataSet ExecuteDataSet<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            int? timeout = null)
            where TDataParameterDriver : IDataParameter, new()
        {
            using var dataTable = (this as Contract.IMaterializer)
                .ExecuteTable(
                    procedure,
                    transaction,
                    timeout);

            var dataSet = new DataSet(nameof(Materializer));
            dataSet.Tables.Add(
                dataTable);

            return dataSet;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to
        /// System.Data.DataSet columns; and is implemented by .NET Framework data providers that
        /// access data sources.
        /// </typeparam>
        /// <param name="procedure">
        /// Represents a Sql Entity that holds the data parameters to be executed by the connection command.
        /// </param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<DataSet> ExecuteDataSetAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new()
        {
            using var dataTable = await (this as Contract.IMaterializer)
                .ExecuteTableAsync(
                    procedure,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

            var dataSet = new DataSet(nameof(Materializer));
            dataSet.Tables.Add(
                dataTable);

            return dataSet;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to
        /// System.Data.DataSet columns; and is implemented by .NET Framework data providers that
        /// access data sources.
        /// </typeparam>
        /// <param name="procedure">
        /// Represents a Sql Entity that holds the data parameters to be executed by the connection command.
        /// </param>
        /// <param name="transaction">The database transaction to use.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<DataSet> ExecuteDataSetAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new()
        {
            using var dataTable = await (this as Contract.IMaterializer)
                .ExecuteTableAsync(
                    procedure,
                    transaction,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);

            var dataSet = new DataSet(nameof(Materializer));
            dataSet.Tables.Add(
                dataTable);

            return dataSet;
        }
    }
}
