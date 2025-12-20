using Hydrix.Schemas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// SQL Data Handler Class
    /// </summary>
    public partial class SqlMaterializer :
        Contract.ISqlMaterializer
    {
        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery(
            string sql)
            => this.ExecuteNonQuery(
                sql,
                (object)null);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery(
            string sql,
            IDbTransaction transaction)
            => this.ExecuteNonQuery(
                sql,
                (object)null,
                transaction);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery(
            string sql,
            object parameters)
        {
            using (var command = (this as Contract.ISqlMaterializer)
                .CreateCommand(
                    sql,
                    parameters))
                return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery(
            string sql,
            object parameters,
            IDbTransaction transaction)
        {
            using (var command = (this as Contract.ISqlMaterializer)
                .CreateCommand(
                    sql,
                    parameters,
                    transaction))
                return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters)
        {
            using (var command = (this as Contract.ISqlMaterializer)
                .CreateCommand(
                    commandType,
                    sql,
                    parameters))
                return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction)
        {
            using (var command = (this as Contract.ISqlMaterializer)
                .CreateCommand(
                    commandType,
                    sql,
                    parameters,
                    transaction))
                return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery(
            CommandType commandType,
            string sql)
            => this.ExecuteNonQuery(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery(
            CommandType commandType,
            string sql,
            IDbTransaction transaction)
            => this.ExecuteNonQuery(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                transaction);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
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
            CancellationToken cancellationToken = default)
        {
            using (var command = (this as Contract.ISqlMaterializer)
                .CreateCommand(
                    sql,
                    parameters))
            {
                if (command is DbCommand dbCommand)
                    return await dbCommand.ExecuteNonQueryAsync(cancellationToken)
                        .ConfigureAwait(false);

                return await Task.Run(command.ExecuteNonQuery, cancellationToken);
            }
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
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
            CancellationToken cancellationToken = default)
        {
            using (var command = (this as Contract.ISqlMaterializer)
                .CreateCommand(
                    sql,
                    parameters,
                    transaction))
            {
                if (command is DbCommand dbCommand)
                    return await dbCommand.ExecuteNonQueryAsync(cancellationToken)
                        .ConfigureAwait(false);

                return await Task.Run(command.ExecuteNonQuery, cancellationToken);
            }
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<int> ExecuteNonQueryAsync(
            string sql,
            CancellationToken cancellationToken = default)
            => await ExecuteNonQueryAsync(
                sql,
                (object)null,
                cancellationToken)
            .ConfigureAwait(false);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
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
            CancellationToken cancellationToken = default)
            => await ExecuteNonQueryAsync(
                sql,
                (object)null,
                transaction,
                cancellationToken)
            .ConfigureAwait(false);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
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
            CancellationToken cancellationToken = default)
        {
            using (var command = (this as Contract.ISqlMaterializer)
                .CreateCommand(
                    commandType,
                    sql,
                    parameters))
            {
                if (command is DbCommand dbCommand)
                    return await dbCommand.ExecuteNonQueryAsync(cancellationToken)
                        .ConfigureAwait(false);

                return await Task.Run(command.ExecuteNonQuery, cancellationToken);
            }
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
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
            CancellationToken cancellationToken = default)
        {
            using (var command = (this as Contract.ISqlMaterializer)
                .CreateCommand(
                    commandType,
                    sql,
                    parameters,
                    transaction))
            {
                if (command is DbCommand dbCommand)
                    return await dbCommand.ExecuteNonQueryAsync(cancellationToken)
                        .ConfigureAwait(false);

                return await Task.Run(command.ExecuteNonQuery, cancellationToken);
            }
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
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
            CancellationToken cancellationToken = default)
            => await ExecuteNonQueryAsync(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                cancellationToken)
            .ConfigureAwait(false);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
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
            CancellationToken cancellationToken = default)
            => await ExecuteNonQueryAsync(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                transaction,
                cancellationToken)
            .ConfigureAwait(false);

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="sqlProcedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlProcedure does not have a SqlProcedureAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery<TDataParameterDriver>(
            ISqlProcedure<TDataParameterDriver> sqlProcedure)
            where TDataParameterDriver : IDataParameter, new()
        {
            using (var command = (this as Contract.ISqlMaterializer).CreateCommand(sqlProcedure))
                return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="sqlProcedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlProcedure does not have a SqlProcedureAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public int ExecuteNonQuery<TDataParameterDriver>(
            ISqlProcedure<TDataParameterDriver> sqlProcedure,
            IDbTransaction transaction)
            where TDataParameterDriver : IDataParameter, new()
        {
            using (var command = (this as Contract.ISqlMaterializer)
                .CreateCommand(
                    sqlProcedure,
                    transaction))
                return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="sqlProcedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlProcedure does not have a SqlProcedureAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<int> ExecuteNonQueryAsync<TDataParameterDriver>(
            ISqlProcedure<TDataParameterDriver> sqlProcedure,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new()
        {
            using (var command = (this as Contract.ISqlMaterializer)
                .CreateCommand(sqlProcedure))
            {
                if (command is DbCommand dbCommand)
                    return await dbCommand.ExecuteNonQueryAsync(cancellationToken)
                        .ConfigureAwait(false);

                return await Task.Run(command.ExecuteNonQuery, cancellationToken);
            }
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object data provider, and returns the number of rows affected.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="sqlProcedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="transaction">The database transaction to use.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlProcedure does not have a SqlProcedureAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<int> ExecuteNonQueryAsync<TDataParameterDriver>(
            ISqlProcedure<TDataParameterDriver> sqlProcedure,
            IDbTransaction transaction,
            CancellationToken cancellationToken = default)
            where TDataParameterDriver : IDataParameter, new()
        {
            using (var command = (this as Contract.ISqlMaterializer)
                .CreateCommand(
                    sqlProcedure,
                    transaction))
            {
                if (command is DbCommand dbCommand)
                    return await dbCommand.ExecuteNonQueryAsync(cancellationToken)
                        .ConfigureAwait(false);

                return await Task.Run(command.ExecuteNonQuery, cancellationToken);
            }
        }
    }
}