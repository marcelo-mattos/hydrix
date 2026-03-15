using Hydrix.Engines;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// Provides a centralized facility for creating and configuring database command objects, supporting parameter
    /// binding, transaction management, and extensibility for various command execution scenarios.
    /// </summary>
    /// <remarks>The Materializer class implements the Contract.IMaterializer interface and serves as the
    /// primary entry point for generating IDbCommand instances tailored to specific SQL operations. It abstracts common
    /// command creation logic, including validation of connection state, transaction association, and parameter
    /// materialization, to promote consistency and code reuse. The class supports multiple overloads for command
    /// creation, enabling flexible parameter binding strategies such as object-based, attribute-driven, or
    /// provider-specific approaches. Diagnostic features, such as SQL command logging, are integrated to aid debugging
    /// and traceability. This design facilitates the introduction of cross-cutting concerns (e.g., logging, metrics) in
    /// a single, maintainable location. Thread safety is ensured for connection and transaction operations. Users
    /// should avoid enabling SQL logging in production environments that handle sensitive data unless appropriate
    /// controls are in place.</remarks>
    public partial class Materializer :
        Contract.IMaterializer
    {
        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="transaction">
        /// Represents the transaction to be used for the command.
        /// </param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute
        /// a command and generating an error.</param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        IDbCommand Contract.IMaterializer.CreateCommand(
            string sql,
            object parameters,
            IDbTransaction transaction,
            int? timeout)
            => CommandEngine.CreateCommand(
                this.DbConnection,
                transaction,
                sql,
                parameters,
                _parameterPrefix,
                timeout,
                this._logger);

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute
        /// a command and generating an error.</param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        IDbCommand Contract.IMaterializer.CreateCommand(
            string sql,
            object parameters,
            int? timeout)
        {
            IDbTransaction transaction = null;

            if (this.IsTransactionActive)
                transaction = this.DbTransaction;

            return CommandEngine.CreateCommand(
                this.DbConnection,
                transaction,
                sql,
                parameters,
                _parameterPrefix,
                timeout,
                this._logger);
        }

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <param name="commandType">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </param>
        /// <param name="sql">Sets the text command to run against the data source.</param>
        /// <param name="transaction">
        /// Represents the transaction to be used for the command.
        /// </param>
        /// <param name="parameters">
        /// Sets the System.Data.IDataParameterCollection with the parameters of the SQL statement
        /// or stored procedure.
        /// </param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute
        /// a command and generating an error.</param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        IDbCommand Contract.IMaterializer.CreateCommand(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            int? timeout)
            => CommandEngine.CreateCommand(
                this.DbConnection,
                transaction,
                commandType,
                sql,
                parameters,
                timeout,
                this._logger);

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
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute
        /// a command and generating an error.</param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        IDbCommand Contract.IMaterializer.CreateCommand(
            CommandType commandType,
            string sql,
            IEnumerable<IDataParameter> parameters,
            int? timeout)
        {
            IDbTransaction transaction = null;

            if (this.IsTransactionActive)
                transaction = this.DbTransaction;

            return CommandEngine.CreateCommand(
                this.DbConnection,
                transaction,
                commandType,
                sql,
                parameters,
                timeout,
                this._logger);
        }

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to
        /// System.Data.DataSet columns; and is implemented by .NET Framework data providers that
        /// access data sources.
        /// </typeparam>
        /// <param name="procedure">
        /// Represents a Sql Entity that holds the data parameters to be executed by the connection command.
        /// </param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute
        /// a command and generating an error.</param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        IDbCommand Contract.IMaterializer.CreateCommand<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            int? timeout)
        {
            IDbTransaction transaction = null;

            if (this.IsTransactionActive)
                transaction = this.DbTransaction;

            return CommandEngine.CreateCommand(
               this.DbConnection,
               transaction,
               procedure,
               _parameterPrefix,
               timeout,
               this._logger);
        }

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <typeparam name="TDataParameterDriver">
        /// Represents a parameter to a Command object, and optionally, its mapping to
        /// System.Data.DataSet columns; and is implemented by .NET Framework data providers that
        /// access data sources.
        /// </typeparam>
        /// <param name="procedure">
        /// Represents a Sql Entity that holds the data parameters to be executed by the connection command.
        /// </param>
        /// <param name="transaction">
        /// Represents the transaction to be used for the command.
        /// </param>
        /// <param name="timeout">Sets the wait time (in seconds) before terminating the attempt to execute
        /// a command and generating an error.</param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="ArgumentException">The property value assigned is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// The System.Collections.IList is read-only. -or- The System.Collections.IList has a fixed size.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The Procedure does not have a ProcedureAttribute decorating itself.
        /// </exception>
        IDbCommand Contract.IMaterializer.CreateCommand<TDataParameterDriver>(
            IProcedure<TDataParameterDriver> procedure,
            IDbTransaction transaction,
            int? timeout)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(
                IsDisposed,
                "The connection has been disposed.");
#else
            if (this.IsDisposed)
                throw new ObjectDisposedException("The connection has been disposed.");
#endif

            return CommandEngine.CreateCommand(
                this.DbConnection,
                transaction,
                procedure,
                _parameterPrefix,
                timeout,
                this._logger);
        }
    }
}