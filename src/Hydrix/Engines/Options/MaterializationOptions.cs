namespace Hydrix.Engines.Options
{
    /// <summary>
    /// Represents options for configuring how a materialization command is executed, including the maximum number
    /// of items to return.
    /// </summary>
    /// <remarks>Use this class to specify command execution details such as the maximum number of items to return
    /// when materializing data. Inheriting from ExecutionOptions, it extends configuration capabilities for command
    /// execution scenarios.</remarks>
    public class MaterializationOptions :
        ExecutionOptions
    {
        /// <summary>
        /// Gets or sets the maximum number of items to return in a query or operation.
        /// </summary>
        public int Limit { get; set; } = 0;
    }
}
