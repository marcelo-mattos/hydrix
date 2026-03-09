using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// Provides methods to execute SQL commands against a data source and build a DataTable from the results.
    /// </summary>
    /// <remarks>The Materializer class implements the IMaterializer interface, enabling both synchronous and
    /// asynchronous execution of SQL queries, stored procedures, and commands. It supports parameterized queries,
    /// database transactions, and cancellation tokens for asynchronous operations. This class is designed to simplify
    /// the process of retrieving tabular data from a database and loading it into a DataTable for further processing or
    /// analysis.</remarks>
    public partial class Materializer :
        Contract.IMaterializer
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
        DataTable Contract.IMaterializer.ExecuteTable(
            string sql,
            object parameters)
        {
            using IDataReader dataReader = (this as Contract.IMaterializer)
                .ExecuteReader(
                    sql,
                    parameters);

            var dataTable = new DataTable(nameof(Materializer));
            dataTable.Load(dataReader);

            return dataTable;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        DataTable Contract.IMaterializer.ExecuteTable(
            string sql,
            object parameters,
            IDbTransaction transaction)
        {
            using IDataReader dataReader = (this as Contract.IMaterializer)
                .ExecuteReader(
                    sql,
                    parameters,
                    transaction);

            var dataTable = new DataTable(nameof(Materializer));
            dataTable.Load(dataReader);

            return dataTable;
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
        DataTable Contract.IMaterializer.ExecuteTable(
            string sql)
            => (this as Contract.IMaterializer).ExecuteTable(
                sql,
                (object)null);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        DataTable Contract.IMaterializer.ExecuteTable(
            string sql,
            IDbTransaction transaction)
            => (this as Contract.IMaterializer).ExecuteTable(
                sql,
                (object)null,
                transaction);

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
        DataTable Contract.IMaterializer.ExecuteTable(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters)
        {
            using IDataReader dataReader = (this as Contract.IMaterializer)
                .ExecuteReader(
                    commandType,
                    sql,
                    parameters);

            var dataTable = new DataTable(nameof(Materializer));
            dataTable.Load(dataReader);

            return dataTable;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        DataTable Contract.IMaterializer.ExecuteTable(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction)
        {
            using IDataReader dataReader = (this as Contract.IMaterializer)
                .ExecuteReader(
                    commandType,
                    sql,
                    parameters,
                    transaction);

            var dataTable = new DataTable(nameof(Materializer));
            dataTable.Load(dataReader);

            return dataTable;
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
        DataTable Contract.IMaterializer.ExecuteTable(
            CommandType commandType,
            string sql)
            => (this as Contract.IMaterializer).ExecuteTable(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        DataTable Contract.IMaterializer.ExecuteTable(
            CommandType commandType,
            string sql,
            IDbTransaction transaction)
            => (this as Contract.IMaterializer).ExecuteTable(
                commandType,
                sql,
                (IEnumerable<IDataParameter>)null,
                transaction);

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
        async Task<DataTable> Contract.IMaterializer.ExecuteTableAsync(
            string sql,
            object parameters,
            CancellationToken cancellationToken)
        {
            using var dataReader = await (this as Contract.IMaterializer)
                .ExecuteReaderAsync(
                    sql,
                    parameters,
                    cancellationToken)
                .ConfigureAwait(false);

            var dataTable = new DataTable(nameof(Materializer));
            dataTable.Load(dataReader);

            return dataTable;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        async Task<DataTable> Contract.IMaterializer.ExecuteTableAsync(
            string sql,
            object parameters,
            IDbTransaction transaction,
            CancellationToken cancellationToken)
        {
            using var dataReader = await (this as Contract.IMaterializer)
                .ExecuteReaderAsync(
                    sql,
                    parameters,
                    transaction,
                    cancellationToken)
                .ConfigureAwait(false);

            var dataTable = new DataTable(nameof(Materializer));
            dataTable.Load(dataReader);

            return dataTable;
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
        async Task<DataTable> Contract.IMaterializer.ExecuteTableAsync(
            string sql,
            CancellationToken cancellationToken)
            => await (this as Contract.IMaterializer)
                .ExecuteTableAsync(
                    sql,
                    (object)null,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        async Task<DataTable> Contract.IMaterializer.ExecuteTableAsync(
            string sql,
            IDbTransaction transaction,
            CancellationToken cancellationToken)
            => await (this as Contract.IMaterializer)
                .ExecuteTableAsync(
                    sql,
                    (object)null,
                    transaction,
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
        async Task<DataTable> Contract.IMaterializer.ExecuteTableAsync(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            CancellationToken cancellationToken)
        {
            using var dataReader = await (this as Contract.IMaterializer)
                .ExecuteReaderAsync(
                    commandType,
                    sql,
                    parameters,
                    cancellationToken)
                .ConfigureAwait(false);

            var dataTable = new DataTable(nameof(Materializer));
            dataTable.Load(dataReader);

            return dataTable;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement or stored procedure.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        async Task<DataTable> Contract.IMaterializer.ExecuteTableAsync(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            CancellationToken cancellationToken)
        {
            using var dataReader = await (this as Contract.IMaterializer)
                .ExecuteReaderAsync(
                    commandType,
                    sql,
                    parameters,
                    transaction,
                    cancellationToken)
                .ConfigureAwait(false);

            var dataTable = new DataTable(nameof(Materializer));
            dataTable.Load(dataReader);

            return dataTable;
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
        async Task<DataTable> Contract.IMaterializer.ExecuteTableAsync(
            CommandType commandType,
            string sql,
            CancellationToken cancellationToken)
            => await (this as Contract.IMaterializer)
                .ExecuteTableAsync(
                    commandType,
                    sql,
                    (IEnumerable<IDataParameter>)null,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        async Task<DataTable> Contract.IMaterializer.ExecuteTableAsync(
            CommandType commandType,
            string sql,
            IDbTransaction transaction,
            CancellationToken cancellationToken)
            => await (this as Contract.IMaterializer)
                .ExecuteTableAsync(
                    commandType,
                    sql,
                    (IEnumerable<IDataParameter>)null,
                    transaction,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        DataTable Contract.IMaterializer.ExecuteTable<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure)
        {
            DataTable dataTable = null;

            using var dataReader = (this as Contract.IMaterializer)
                .ExecuteReader(
                    procedure);

            dataTable = new DataTable(nameof(Materializer));
            dataTable.Load(dataReader);

            return dataTable;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="transaction">The transaction to use for the command.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        DataTable Contract.IMaterializer.ExecuteTable<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction)
        {
            DataTable dataTable = null;

            using var dataReader = (this as Contract.IMaterializer)
                .ExecuteReader(
                    procedure,
                    transaction);

            dataTable = new DataTable(nameof(Materializer));
            dataTable.Load(dataReader);

            return dataTable;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        async Task<DataTable> Contract.IMaterializer.ExecuteTableAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            CancellationToken cancellationToken)
        {
            using var dataReader = await (this as Contract.IMaterializer)
                .ExecuteReaderAsync(
                    procedure,
                    cancellationToken)
                .ConfigureAwait(false);

            var dataTable = new DataTable(nameof(Materializer));
            dataTable.Load(dataReader);

            return dataTable;
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.DataTable.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet columns;
        /// and is implemented by .NET Framework data providers that access data sources.
        /// </typeparam>
        /// <param name="procedure">Represents a Sql Entity that holds the data parameters to be executed by the connection command.</param>
        /// <param name="transaction">The database transaction to use.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.</exception>
        /// <exception cref="MissingMemberException">The Procedure does not have a ProcedureAttribute decorating itself.</exception>
        /// <exception cref="InvalidOperationException">The connection does not exist. -or- The connection is not open.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        async Task<DataTable> Contract.IMaterializer.ExecuteTableAsync<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            CancellationToken cancellationToken)
        {
            using var dataReader = await (this as Contract.IMaterializer)
                .ExecuteReaderAsync(
                    procedure,
                    transaction,
                    cancellationToken)
                .ConfigureAwait(false);

            var dataTable = new DataTable(nameof(Materializer));
            dataTable.Load(dataReader);

            return dataTable;
        }
    }
}