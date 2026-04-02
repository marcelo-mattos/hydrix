using System.Data;

namespace Hydrix.Engines.Options
{
    /// <summary>
    /// Represents a set of options for configuring the execution of database commands, including connection,
    /// transaction, command timeout, and parameter prefix settings.
    /// </summary>
    /// <remarks>Use this class to specify execution context details when performing database operations. By
    /// configuring these options, you can control aspects such as which connection and transaction are used, how long
    /// commands are allowed to run before timing out, and how parameters are named in SQL statements. This is useful
    /// for ensuring consistent behavior across multiple database operations and for integrating with different database
    /// providers.</remarks>
    public class ExecutionOptions
    {
        /// <summary>
        /// Gets or sets the database connection used to execute commands.
        /// </summary>
        public IDbConnection Connection { get; set; }

        /// <summary>
        /// Gets or sets the database transaction to be used for executing commands.
        /// </summary>
        /// <remarks>Assigning a transaction to this property ensures that all database operations
        /// performed through this instance are executed within the specified transaction context. If the property is
        /// not set, commands will execute outside of any explicit transaction.</remarks>
        public IDbTransaction Transaction { get; set; } = null;

        /// <summary>
        /// Gets or sets the command timeout value, in seconds, for database operations.
        /// </summary>
        /// <remarks>If the value is null, the default timeout defined by the underlying database provider
        /// is used. Setting this property allows customization of how long a command is allowed to execute before
        /// timing out.</remarks>
        public int? CommandTimeout { get; set; } = null;

        /// <summary>
        /// Gets or sets the prefix string used for parameter names in database commands.
        /// </summary>
        /// <remarks>This property is typically used to specify the character or string that precedes
        /// parameter names in SQL statements, such as '@' for SQL Server or ':' for Oracle. Setting the correct prefix
        /// ensures that parameters are recognized correctly by the underlying database provider.</remarks>
        public string ParameterPrefix { get; set; } = null;
    }
}
