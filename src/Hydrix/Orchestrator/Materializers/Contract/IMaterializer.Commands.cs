using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;

namespace Hydrix.Orchestrator.Materializers.Contract
{
    /// <summary>
    /// Defines an interface for creating database command objects associated with a connection, supporting multiple
    /// overloads for specifying SQL text, parameters, command type, and transaction context.
    /// </summary>
    /// <remarks>IMaterializer provides flexible methods for generating IDbCommand instances tailored to
    /// various database operations. Implementations should ensure that commands are constructed in accordance with the
    /// requirements of the underlying database provider. The interface supports both direct SQL execution and
    /// procedure-based commands, allowing for parameterization and transaction management. All methods may throw
    /// exceptions if the connection is disposed, parameters are invalid, or required attributes are missing from
    /// procedure entities.</remarks>
    public partial interface IMaterializer :
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
            IProcedure<TDataParameterDriver> sqlProcedure)
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
            IProcedure<TDataParameterDriver> sqlProcedure,
            IDbTransaction transaction)
            where TDataParameterDriver : IDataParameter, new();
    }
}