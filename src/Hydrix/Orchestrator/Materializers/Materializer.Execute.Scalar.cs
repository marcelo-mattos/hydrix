using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// Provides methods to execute SQL queries and retrieve the first column of the first row in the result set,
    /// ignoring any additional columns or rows.
    /// </summary>
    /// <remarks>The Materializer class implements the IMaterializer interface and is designed to facilitate
    /// database operations, ensuring that the connection is properly managed and disposed of after use. It supports
    /// both synchronous and asynchronous execution patterns, as well as integration with database transactions and
    /// stored procedures. Exceptions are thrown for invalid connection states, disposed resources, or improper
    /// parameter usage. This class is intended for scenarios where only a single scalar value is required from a query
    /// result.</remarks>
    public partial class Materializer :
        Contract.IMaterializer
    {
        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public object ExecuteScalar(
            string sql,
            object parameters)
        {
            using var command = (this as Contract.IMaterializer)
                .CreateCommand(
                    sql,
                    parameters);

            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public object ExecuteScalar(
            string sql,
            object parameters,
            IDbTransaction transaction)
        {
            using var command = (this as Contract.IMaterializer)
                .CreateCommand(
                    sql,
                    parameters,
                    transaction);

            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public object ExecuteScalar(
            string sql)
            => this.ExecuteScalar(
                sql,
                (object)null);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public object ExecuteScalar(
            string sql,
            IDbTransaction transaction)
            => this.ExecuteScalar(
                sql,
                (object)null,
                transaction);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public object ExecuteScalar(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters)
        {
            using var command = (this as Contract.IMaterializer)
                .CreateCommand(
                    commandType,
                    sql,
                    parameters);

            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public object ExecuteScalar(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction)
        {
            using var command = (this as Contract.IMaterializer)
                .CreateCommand(
                    commandType,
                    sql,
                    parameters,
                    transaction);

            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public object ExecuteScalar(
            CommandType commandType,
            string sql)
            => this.ExecuteScalar(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public object ExecuteScalar(
            CommandType commandType,
            string sql,
            IDbTransaction transaction)
            => this.ExecuteScalar(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                transaction);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<object> ExecuteScalarAsync(
            string sql,
            object parameters,
            CancellationToken cancellationToken = default)
        {
            using var command = (this as Contract.IMaterializer)
                .CreateCommand(
                    sql,
                    parameters);

            if (command is DbCommand dbCommand)
                return await dbCommand
                    .ExecuteScalarAsync(
                        cancellationToken)
                    .ConfigureAwait(false);

            return await Task.Run(
                command.ExecuteScalar,
                cancellationToken);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<object> ExecuteScalarAsync(
            string sql,
            object parameters,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            using var command = (this as Contract.IMaterializer)
                .CreateCommand(
                    sql,
                    parameters,
                    transaction);

            if (command is DbCommand dbCommand)
                return await dbCommand
                    .ExecuteScalarAsync(
                        cancellationToken)
                    .ConfigureAwait(false);

            return await Task.Run(
                command.ExecuteScalar,
                cancellationToken);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<object> ExecuteScalarAsync(
            string sql,
            CancellationToken cancellationToken = default)
            => await this.ExecuteScalarAsync(
                sql,
                (object)null,
                cancellationToken)
            .ConfigureAwait(false);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<object> ExecuteScalarAsync(
            string sql,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default)
            => await this.ExecuteScalarAsync(
                sql,
                (object)null,
                transaction,
                cancellationToken)
            .ConfigureAwait(false);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<object> ExecuteScalarAsync(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            CancellationToken cancellationToken = default)
        {
            using var command = (this as Contract.IMaterializer)
                .CreateCommand(
                    commandType,
                    sql,
                    parameters);

            if (command is DbCommand dbCommand)
                return await dbCommand
                    .ExecuteScalarAsync(
                        cancellationToken)
                    .ConfigureAwait(false);

            return await Task.Run(
                command.ExecuteScalar,
                cancellationToken);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<object> ExecuteScalarAsync(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            using var command = (this as Contract.IMaterializer)
                .CreateCommand(
                    commandType,
                    sql,
                    parameters,
                    transaction);

            if (command is DbCommand dbCommand)
                return await dbCommand
                    .ExecuteScalarAsync(
                        cancellationToken)
                    .ConfigureAwait(false);

            return await Task.Run(
                command.ExecuteScalar,
                cancellationToken);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<object> ExecuteScalarAsync(
            CommandType commandType,
            string sql,
            CancellationToken cancellationToken = default)
            => await this.ExecuteScalarAsync(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                cancellationToken)
            .ConfigureAwait(false);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<object> ExecuteScalarAsync(
            CommandType commandType,
            string sql,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default)
            => await this.ExecuteScalarAsync(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                transaction,
                cancellationToken)
            .ConfigureAwait(false);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public object ExecuteScalar<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure)
            where TDataParameterDriver : IDataParameter, new()
        {
            using var command = (this as Contract.IMaterializer)
                .CreateCommand(
                    procedure);

            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public object ExecuteScalar<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction)
            where TDataParameterDriver : IDataParameter, new()
        {
            using var command = (this as Contract.IMaterializer)
                .CreateCommand(
                    procedure,
                    transaction);

            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<object> ExecuteScalarAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new()
        {
            using var command = (this as Contract.IMaterializer)
                .CreateCommand(
                    procedure);

            if (command is DbCommand dbCommand)
                return await dbCommand
                    .ExecuteScalarAsync(
                        cancellationToken)
                    .ConfigureAwait(false);

            return await Task.Run(
                command.ExecuteScalar,
                cancellationToken);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="transaction">The database transaction to use.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<object> ExecuteScalarAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new()
        {
            using var command = (this as Contract.IMaterializer)
                .CreateCommand(
                    procedure,
                    transaction);

            if (command is DbCommand dbCommand)
                return await dbCommand
                    .ExecuteScalarAsync(
                        cancellationToken)
                    .ConfigureAwait(false);

            return await Task.Run(
                command.ExecuteScalar,
                cancellationToken);
        }
    }
}