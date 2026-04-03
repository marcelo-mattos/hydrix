namespace Hydrix.Engines.Options
{
    /// <summary>
    /// Represents options for configuring how a materialization command is executed, including the interpretation of
    /// the command text by the data provider.
    /// </summary>
    /// <remarks>Use this class to specify command execution details such as the command type (e.g., text,
    /// stored procedure) and the maximum number of records to materialize. Inheriting from
    /// <see cref="ExecutionCommandOptions"/>, it extends command execution configuration with materialization-specific
    /// options.</remarks>
    public class MaterializationCommandOptions :
        ExecutionCommandOptions
    {
        /// <summary>
        /// Gets or sets the maximum number of records to materialize.
        /// </summary>
        public int Limit { get; set; } = 0;
    }
}
