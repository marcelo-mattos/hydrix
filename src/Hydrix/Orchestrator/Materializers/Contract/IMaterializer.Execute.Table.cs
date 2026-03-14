using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrix.Orchestrator.Materializers.Contract
{
    /// <summary>
    /// Defines a contract for executing SQL commands against a data source and materializing the results as a
    /// DataTable, with support for both synchronous and asynchronous operations, parameterization, transactions, and
    /// cancellation.
    /// </summary>
    /// <remarks>Implementations of this interface enable flexible execution of SQL queries and stored
    /// procedures, allowing callers to specify command type, parameters, and transaction context. Both synchronous and
    /// asynchronous methods are provided to accommodate different application requirements, including support for
    /// cancellation tokens in asynchronous scenarios. Proper resource management is expected, and implementers should
    /// ensure that database connections and commands are disposed of appropriately. Exceptions are thrown for invalid
    /// operations, such as using a disposed connection or providing invalid arguments. This interface is typically used
    /// in data access layers to abstract the details of command execution and result materialization.</remarks>
    public partial interface IMaterializer :
        IDisposable
    {
        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataTable ExecuteTable(
            string sql,
            object parameters,
            int? timeout = null);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataTable ExecuteTable(
            string sql,
            object parameters,
            IDbTransaction transaction,
            int? timeout = null);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataTable ExecuteTable(
            string sql,
            int? timeout = null);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataTable ExecuteTable(
            string sql,
            IDbTransaction transaction,
            int? timeout = null);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
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
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataTable ExecuteTable(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            int? timeout = null);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
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
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataTable ExecuteTable(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            int? timeout = null);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataTable ExecuteTable(
            CommandType commandType,
            string sql,
            int? timeout = null);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataTable ExecuteTable(
            CommandType commandType,
            string sql,
            IDbTransaction transaction,
            int? timeout = null);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<DataTable> ExecuteTableAsync(
            string sql,
            object parameters,
            int? timeout = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
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
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<DataTable> ExecuteTableAsync(
            string sql,
            object parameters,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<DataTable> ExecuteTableAsync(
            string sql,
            int? timeout = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<DataTable> ExecuteTableAsync(
            string sql,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
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
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<DataTable> ExecuteTableAsync(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            int? timeout = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
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
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<DataTable> ExecuteTableAsync(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<DataTable> ExecuteTableAsync(
            CommandType commandType,
            string sql,
            int? timeout = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<DataTable> ExecuteTableAsync(
            CommandType commandType,
            string sql,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
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
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The SqlEntity does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataTable ExecuteTable<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            int? timeout = null)
            where TDataParameterDriver : IDataParameter, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
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
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The SqlEntity does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataTable ExecuteTable<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            int? timeout = null)
            where TDataParameterDriver : IDataParameter, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
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
        /// <returns>An System.Data.DataTable object.</returns>
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
        Task<DataTable> ExecuteTableAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
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
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
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
        Task<DataTable> ExecuteTableAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new();
    }
}