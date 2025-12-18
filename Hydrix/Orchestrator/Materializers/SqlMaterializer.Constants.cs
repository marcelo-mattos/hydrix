namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// SQL Data Handler Class
    /// </summary>
    public partial class SqlMaterializer :
        Contract.ISqlMaterializer
    {
        /// <summary>
        /// The default wait time (in seconds) before terminating the attempt to execute a command and generating an error.
        /// </summary>
        private const int DefaultTimeout = 30;

        /// <summary>
        /// The default prefix used for SQL parameters.
        /// </summary>
        private const string DefaultParameterPrefix = "@";
    }
}