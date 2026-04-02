using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrix.Orchestrator.Materializers.Contract
{
    /// <summary>
    /// Defines methods for executing SQL commands against a database connection and materializing the results as
    /// collections of ITable instances. Supports both synchronous and asynchronous operations, as well as transaction
    /// and cancellation token support for advanced scenarios.
    /// </summary>
    /// <remarks>IMaterializer provides a flexible API for querying relational data sources and mapping the
    /// results to strongly-typed entities that implement the ITable interface. It supports a variety of query patterns,
    /// including raw SQL, parameterized commands, and stored procedures, with optional transaction and cancellation
    /// support. Implementations are responsible for managing the underlying database connection and ensuring proper
    /// resource disposal. Callers should ensure that entity and procedure types are correctly attributed to enable
    /// mapping.</remarks>
    public partial interface IMaterializer :
        IDisposable
    {
        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        IList<TEntity> Query<TEntity>(
            string sql,
            object parameters,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        IList<TEntity> Query<TEntity>(
            string sql,
            object parameters,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        IList<TEntity> Query<TEntity>(
            string sql,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        IList<TEntity> Query<TEntity>(
            string sql,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        IList<TEntity> Query<TEntity>(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        IList<TEntity> Query<TEntity>(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        IList<TEntity> Query<TEntity>(
            CommandType commandType,
            string sql,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        IList<TEntity> Query<TEntity>(
            CommandType commandType,
            string sql,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<IList<TEntity>> QueryAsync<TEntity>(
            string sql,
            object parameters,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<IList<TEntity>> QueryAsync<TEntity>(
            string sql,
            object parameters,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<IList<TEntity>> QueryAsync<TEntity>(
            string sql,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<IList<TEntity>> QueryAsync<TEntity>(
            string sql,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<IList<TEntity>> QueryAsync<TEntity>(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<IList<TEntity>> QueryAsync<TEntity>(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<IList<TEntity>> QueryAsync<TEntity>(
            CommandType commandType,
            string sql,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<IList<TEntity>> QueryAsync<TEntity>(
            CommandType commandType,
            string sql,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to
        /// System.Data.DataSet columns; and is implemented by .NET Framework data providers that
        /// access data sources.
        /// </typeparam>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="procedure">
        /// Represents a Sql Entity that holds the data parameters to be executed by the connection command.
        /// </param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
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
        IList<TEntity> Query<TEntity, TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to
        /// System.Data.DataSet columns; and is implemented by .NET Framework data providers that
        /// access data sources.
        /// </typeparam>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="procedure">
        /// Represents a Sql Entity that holds the data parameters to be executed by the connection command.
        /// </param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
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
        IList<TEntity> Query<TEntity, TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to
        /// System.Data.DataSet columns; and is implemented by .NET Framework data providers that
        /// access data sources.
        /// </typeparam>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="procedure">
        /// Represents a Sql Entity that holds the data parameters to be executed by the connection command.
        /// </param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<IList<TEntity>> QueryAsync<TEntity, TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new();

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the
        /// System.Data.IDbCommand.Connection and builds an System.Data.DataSet. Then parse its
        /// result into a ITable array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to
        /// System.Data.DataSet columns; and is implemented by .NET Framework data providers that
        /// access data sources.
        /// </typeparam>
        /// <typeparam name="TEntity">
        /// Represents a Sql Table that holds the data to be parsed from the DataSet result.
        /// </typeparam>
        /// <param name="procedure">
        /// Represents a Sql Entity that holds the data parameters to be executed by the connection command.
        /// </param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute a command
        /// and generating an error. If null, the default timeout is used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ITable array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The entity does not have a TableAttribute decorating itself.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The connection does not exist. -or- The connection is not open.
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        Task<IList<TEntity>> QueryAsync<TEntity, TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            int limit = 0,
            int? timeout = null,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
            where TDataParameterDriver : IDataParameter, new();
    }
}
