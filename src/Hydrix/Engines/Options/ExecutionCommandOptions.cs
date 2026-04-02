using System.Data;

namespace Hydrix.Engines.Options
{
    /// <summary>
    /// Represents options for configuring the execution of a database command, including how the command text is
    /// interpreted by the data provider.
    /// </summary>
    /// <remarks>Use this class to specify execution-related settings, such as the command type, when
    /// executing database commands. The options provided may affect how the command is processed and how parameters are
    /// handled by the underlying data provider.</remarks>
    public class ExecutionCommandOptions :
        ExecutionOptions
    {
        /// <summary>
        /// Gets or sets a value indicating how the command string is interpreted by the data provider.
        /// </summary>
        /// <remarks>Set this property to specify whether the command text represents a raw SQL query, a
        /// stored procedure, or another command type supported by the provider. The default is CommandType.Text, which
        /// treats the command as a SQL statement. Changing this property may affect how parameters are handled and how
        /// the command is executed.</remarks>
        public CommandType CommandType { get; set; } = CommandType.Text;
    }
}
