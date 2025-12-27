using Hydrix.Schemas;
using System;
using System.Collections.Generic;
using System.Data;

namespace Hydrix.Orchestrator.Materializers.Contract
{
    /// <summary>
    /// SQL Data Materializer Interface
    /// </summary>
    public partial interface ISqlMaterializer :
        IDisposable
    {
        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">Represents the transaction to be used for the command.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        IDbCommand CreateCommand(
            string sql,
            object parameters,
            IDbTransaction transaction);

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        IDbCommand CreateCommand(
            string sql,
            object parameters);

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="transaction">
        /// Represents the transaction to be used for the command.
        /// </param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        IDbCommand CreateCommand(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction);

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        IDbCommand CreateCommand(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters);

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to
        /// System.Data.DataSet columns; and is implemented by .NET Framework data providers that
        /// access data sources.
        /// </typeparam>
        /// <param name="sqlProcedure">
        /// Represents a Sql Entity that holds the data parameters to be executed by the connection command.
        /// </param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The SqlEntity does not have a SqlProcedureAttibute decorating itself.
        /// </exception>
        IDbCommand CreateCommand<TDataParameterDriver>(
            ISqlProcedure<TDataParameterDriver> sqlProcedure)
            where TDataParameterDriver : IDataParameter, new();

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to
        /// System.Data.DataSet columns; and is implemented by .NET Framework data providers that
        /// access data sources.
        /// </typeparam>
        /// <param name="sqlProcedure">
        /// Represents a Sql Entity that holds the data parameters to be executed by the connection command.
        /// </param>
        /// <param name="transaction">
        /// Represents the transaction to be used for the command.
        /// </param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The SqlEntity does not have a SqlProcedureAttibute decorating itself.
        /// </exception>
        IDbCommand CreateCommand<TDataParameterDriver>(
            ISqlProcedure<TDataParameterDriver> sqlProcedure,
            IDbTransaction transaction)
            where TDataParameterDriver : IDataParameter, new();
    }
}
