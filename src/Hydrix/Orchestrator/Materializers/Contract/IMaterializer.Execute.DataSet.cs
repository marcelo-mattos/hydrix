using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrix.Orchestrator.Materializers.Contract
{
    /// <summary>
    /// Defines a contract for executing SQL commands against a data source and materializing the results as a DataSet,
    /// with support for both synchronous and asynchronous operations, transactions, and cancellation.
    /// </summary>
    /// <remarks>Implementations of this interface enable flexible execution of SQL queries and stored
    /// procedures, allowing callers to specify command types, parameters, and transactions as needed. Both synchronous
    /// and asynchronous methods are provided to accommodate different application requirements, including support for
    /// cancellation tokens in asynchronous scenarios. Proper resource management and exception handling are expected
    /// from implementers, especially regarding connection state and disposal. This interface is intended for scenarios
    /// where callers need to retrieve tabular data from a database in the form of a DataSet, with optional transaction
    /// and parameterization support.</remarks>
    public partial interface IMaterializer :
        IDisposable
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
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataSet ExecuteDataSet(
            string sql,
            object parameters);

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
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataSet ExecuteDataSet(
            string sql,
            object parameters,
            IDbTransaction transaction);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataSet ExecuteDataSet(
            string sql);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataSet ExecuteDataSet(
            string sql,
            IDbTransaction transaction);

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
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataSet ExecuteDataSet(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters);

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
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataSet ExecuteDataSet(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataSet ExecuteDataSet(
            CommandType commandType,
            string sql);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        DataSet ExecuteDataSet(
            CommandType commandType,
            string sql,
            IDbTransaction transaction);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
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
        Task<DataSet> ExecuteDataSetAsync(
            string sql,
            object parameters,
            CancellationToken cancellationToken = default);

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
        Task<DataSet> ExecuteDataSetAsync(
            string sql,
            object parameters,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
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
        Task<DataSet> ExecuteDataSetAsync(
            string sql,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
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
        Task<DataSet> ExecuteDataSetAsync(
            string sql,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default);

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
        Task<DataSet> ExecuteDataSetAsync(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            CancellationToken cancellationToken = default);

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
        Task<DataSet> ExecuteDataSetAsync(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
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
        Task<DataSet> ExecuteDataSetAsync(
            CommandType commandType,
            string sql,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
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
        Task<DataSet> ExecuteDataSetAsync(
            CommandType commandType,
            string sql,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default);

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
        DataSet ExecuteDataSet<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure)
            where TDataParameterDriver : IDataParameter, new();

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
        DataSet ExecuteDataSet<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction)
            where TDataParameterDriver : IDataParameter, new();

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
        Task<DataSet> ExecuteDataSetAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new();

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
        Task<DataSet> ExecuteDataSetAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new();
    }
}