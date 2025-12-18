using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Hydrix.Schemas;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// SQL Data Handler Class
    /// </summary>
    public partial class SqlMaterializer :
        Contract.ISqlMaterializer
    {
        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        DataTable Contract.ISqlMaterializer.ExecuteTable(
            string sql,
            object parameters)
        {
            using (IDataReader dataReader = (this as Contract.ISqlMaterializer).ExecuteReader(
                sql,
                parameters))
            {
                var dataTable = new DataTable(nameof(SqlMaterializer));
                dataTable.Load(dataReader);
                return dataTable;
            }
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        DataTable Contract.ISqlMaterializer.ExecuteTable(
            string sql)
            => (this as Contract.ISqlMaterializer).ExecuteTable(
                sql,
                (object)null);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        DataTable Contract.ISqlMaterializer.ExecuteTable(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters)
        {
            using (IDataReader dataReader = (this as Contract.ISqlMaterializer).ExecuteReader(
                commandType,
                sql,
                parameters))
            {
                var dataTable = new DataTable(nameof(SqlMaterializer));
                dataTable.Load(dataReader);
                return dataTable;
            }
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        DataTable Contract.ISqlMaterializer.ExecuteTable(
            CommandType commandType,
            string sql)
            => (this as Contract.ISqlMaterializer).ExecuteTable(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        async Task<DataTable> Contract.ISqlMaterializer.ExecuteTableAsync(
            string sql,
            object parameters,
            CancellationToken cancellationToken)
        {
            using (var dataReader = await (this as Contract.ISqlMaterializer).ExecuteReaderAsync(
                sql,
                parameters,
                cancellationToken)
                .ConfigureAwait(false))
            {
                var dataTable = new DataTable(nameof(SqlMaterializer));
                dataTable.Load(dataReader);
                return dataTable;
            }
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        async Task<DataTable> Contract.ISqlMaterializer.ExecuteTableAsync(
            string sql,
            CancellationToken cancellationToken)
            => await (this as Contract.ISqlMaterializer).ExecuteTableAsync(
                sql,
                (object)null,
                cancellationToken)
            .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        async Task<DataTable> Contract.ISqlMaterializer.ExecuteTableAsync(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            CancellationToken cancellationToken)
        {
            using (var dataReader = await (this as Contract.ISqlMaterializer).ExecuteReaderAsync(
                commandType,
                sql,
                parameters,
                cancellationToken)
                .ConfigureAwait(false))
            {
                var dataTable = new DataTable(nameof(SqlMaterializer));
                dataTable.Load(dataReader);
                return dataTable;
            }
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        async Task<DataTable> Contract.ISqlMaterializer.ExecuteTableAsync(
            CommandType commandType,
            string sql,
            CancellationToken cancellationToken)
            => await (this as Contract.ISqlMaterializer).ExecuteTableAsync(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                cancellationToken)
            .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="sqlProcedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlProcedure does not have a SqlProcedureAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        DataTable Contract.ISqlMaterializer.ExecuteTable<TDataParameterDriver>(
            ISqlProcedure<TDataParameterDriver> sqlProcedure)
        {
            DataTable dataTable = null;

            using (var dataReader = (this as Contract.ISqlMaterializer).ExecuteReader(sqlProcedure))
            {
                dataTable = new DataTable(nameof(SqlMaterializer));
                dataTable.Load(dataReader);
            }

            return dataTable;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="sqlProcedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlProcedure does not have a SqlProcedureAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        async Task<DataTable> Contract.ISqlMaterializer.ExecuteTableAsync<TDataParameterDriver>(
            ISqlProcedure<TDataParameterDriver> sqlProcedure,
            CancellationToken cancellationToken)
        {
            using (var dataReader = await (this as Contract.ISqlMaterializer).ExecuteReaderAsync(
                sqlProcedure,
                cancellationToken)
                .ConfigureAwait(false))
            {
                var dataTable = new DataTable(nameof(SqlMaterializer));
                dataTable.Load(dataReader);
                return dataTable;
            }
        }
    }
}