using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrix.Orchestrator.Materializers.Contract
{
    /// <summary>
    /// Defines a contract for executing SQL queries and retrieving the first column of the first row in the result set,
    /// supporting both synchronous and asynchronous operations. Provides methods for executing scalar commands with
    /// optional parameters, transactions, and cancellation support.
    /// </summary>
    /// <remarks>Implementations of this interface enable efficient execution of SQL commands against a data
    /// source, allowing for parameterized queries and transaction management to enhance security and consistency.
    /// Asynchronous methods support cancellation via tokens, making them suitable for responsive or scalable
    /// applications. The interface is designed to be used with disposable resources; callers should ensure proper
    /// disposal to release underlying connections and resources.</remarks>
    public partial interface IMaterializer :
        IDisposable
    {
        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        object ExecuteScalar(
            string sql,
            object parameters);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        object ExecuteScalar(
            string sql,
            object parameters,
            IDbTransaction transaction);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        object ExecuteScalar(
            string sql);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        object ExecuteScalar(
            string sql,
            IDbTransaction transaction);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        object ExecuteScalar(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
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
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        object ExecuteScalar(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        object ExecuteScalar(
            CommandType commandType,
            string sql);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        object ExecuteScalar(
            CommandType commandType,
            string sql,
            IDbTransaction transaction);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<object> ExecuteScalarAsync(
            string sql,
            object parameters,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<object> ExecuteScalarAsync(
            string sql,
            object parameters,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<object> ExecuteScalarAsync(
            string sql,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<object> ExecuteScalarAsync(
            string sql,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
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
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<object> ExecuteScalarAsync(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
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
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<object> ExecuteScalarAsync(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<object> ExecuteScalarAsync(
            CommandType commandType,
            string sql,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<object> ExecuteScalarAsync(
            CommandType commandType,
            string sql,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to
        /// System.Data.DataSet columns; and is implemented by .NET Framework data providers that
        /// access data sources.
        /// </typeparam>
        /// <param name="procedure">
        /// Represents a Sql Entity that holds the data parameters to be executed by the connection command.
        /// </param>
        /// <returns>The first column of the first row in the resultset.</returns>
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
        object ExecuteScalar<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure)
            where TDataParameterDriver : IDataParameter, new();

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
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
        /// <returns>The first column of the first row in the resultset.</returns>
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
        object ExecuteScalar<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction)
            where TDataParameterDriver : IDataParameter, new();

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
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
        /// <returns>The first column of the first row in the resultset.</returns>
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
        Task<object> ExecuteScalarAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new();

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset
        /// returned by the query. Extra columns or rows are ignored.
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
        /// <returns>The first column of the first row in the resultset.</returns>
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
        Task<object> ExecuteScalarAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new();
    }
}