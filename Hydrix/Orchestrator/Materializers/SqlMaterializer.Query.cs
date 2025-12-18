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
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ISqlEntity array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <returns>An ISqlEntity array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlProcedure does not have a SqlProcedureAttibute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The SqlEntity does not have a SqlEntityAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public IList<TEntity> Query<TEntity>(
            string sql,
            object parameters)
            where TEntity : ISqlEntity, new()
        {
            var result = ValidateEntityRequest<TEntity>();
            if (!result)
                return new List<TEntity>();

            using var dataReader = (this as Contract.ISqlMaterializer).ExecuteReader(
                sql,
                parameters);

            return ConvertDataReaderToEntities<TEntity>(dataReader);
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ISqlEntity array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <returns>An ISqlEntity array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlProcedure does not have a SqlProcedureAttibute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The SqlEntity does not have a SqlEntityAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public IList<TEntity> Query<TEntity>(
            string sql)
            where TEntity : ISqlEntity, new()
            => Query<TEntity>(
                sql,
                (object)null);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ISqlEntity array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <returns>An ISqlEntity array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlProcedure does not have a SqlProcedureAttibute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The SqlEntity does not have a SqlEntityAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public IList<TEntity> Query<TEntity>(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters)
            where TEntity : ISqlEntity, new()
        {
            var result = ValidateEntityRequest<TEntity>();
            if (!result)
                return new List<TEntity>();

            using var dataReader = (this as Contract.ISqlMaterializer).ExecuteReader(
                commandType,
                sql,
                parameters);

            return ConvertDataReaderToEntities<TEntity>(dataReader);
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ISqlEntity array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <returns>An ISqlEntity array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlProcedure does not have a SqlProcedureAttibute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The SqlEntity does not have a SqlEntityAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public IList<TEntity> Query<TEntity>(
            CommandType commandType,
            string sql)
            where TEntity : ISqlEntity, new()
            => Query<TEntity>(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ISqlEntity array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ISqlEntity array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlProcedure does not have a SqlProcedureAttibute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The SqlEntity does not have a SqlEntityAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<IList<TEntity>> QueryAsync<TEntity>(
            string sql,
            object parameters,
            CancellationToken cancellationToken = default)
            where TEntity : ISqlEntity, new()
        {
            var result = ValidateEntityRequest<TEntity>();
            if (!result)
                return new List<TEntity>();

            using var dataTable = await (this as Contract.ISqlMaterializer).ExecuteReaderAsync(
                sql,
                parameters,
                cancellationToken)
                .ConfigureAwait(false);
            return ConvertDataReaderToEntities<TEntity>(dataTable);
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ISqlEntity array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ISqlEntity array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlProcedure does not have a SqlProcedureAttibute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The SqlEntity does not have a SqlEntityAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<IList<TEntity>> QueryAsync<TEntity>(
            string sql,
            CancellationToken cancellationToken = default)
            where TEntity : ISqlEntity, new()
            => await QueryAsync<TEntity>(
                sql,
                (object)null,
                cancellationToken)
            .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ISqlEntity array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ISqlEntity array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlProcedure does not have a SqlProcedureAttibute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The SqlEntity does not have a SqlEntityAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<IList<TEntity>> QueryAsync<TEntity>(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            CancellationToken cancellationToken = default)
            where TEntity : ISqlEntity, new()
        {
            var result = ValidateEntityRequest<TEntity>();
            if (!result)
                return new List<TEntity>();

            using var dataTable = await (this as Contract.ISqlMaterializer).ExecuteReaderAsync(
                commandType,
                sql,
                parameters,
                cancellationToken)
                .ConfigureAwait(false);

            return ConvertDataReaderToEntities<TEntity>(dataTable);
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ISqlEntity array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ISqlEntity array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlProcedure does not have a SqlProcedureAttibute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The SqlEntity does not have a SqlEntityAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<IList<TEntity>> QueryAsync<TEntity>(
            CommandType commandType,
            string sql,
            CancellationToken cancellationToken = default)
            where TEntity : ISqlEntity, new()
            => await QueryAsync<TEntity>(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                cancellationToken)
            .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ISqlEntity array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sqlProcedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <returns>An ISqlEntity array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlProcedure does not have a SqlProcedureAttibute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The SqlEntity does not have a SqlEntityAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        public IList<TEntity> Query<TEntity, TDataParameterDriver>(
            ISqlProcedure<TDataParameterDriver> sqlProcedure)
            where TEntity : ISqlEntity, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            var result = ValidateEntityRequest<TEntity>();
            if (!result)
                return new List<TEntity>();

            using var dataReader = (this as Contract.ISqlMaterializer).ExecuteReader(
                sqlProcedure);

            return ConvertDataReaderToEntities<TEntity>(dataReader);
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataSet.
        /// Then parse its result into a ISqlEntity array returning the processed data to the requester.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="sqlProcedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An ISqlEntity array filled with the DataSet result.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The SqlProcedure does not have a SqlProcedureAttibute decorating itself.</exception>
        /// <exception cref="MissingMemberException">The SqlEntity does not have a SqlEntityAttibute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task<IList<TEntity>> QueryAsync<TEntity, TDataParameterDriver>(
            ISqlProcedure<TDataParameterDriver> sqlProcedure,
            CancellationToken cancellationToken = default)
            where TEntity : ISqlEntity, new()
            where TDataParameterDriver : IDataParameter, new()
        {
            var result = ValidateEntityRequest<TEntity>();
            if (!result)
                return new List<TEntity>();

            using var dataReader = await (this as Contract.ISqlMaterializer).ExecuteReaderAsync(
                sqlProcedure,
                cancellationToken)
                .ConfigureAwait(false);

            return ConvertDataReaderToEntities<TEntity>(dataReader);
        }
    }
}