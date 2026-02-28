namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// Represents a materializer that executes commands and manages SQL parameters.
    /// </summary>
    /// <remarks>This class implements the IMaterializer interface and provides default configurations for
    /// command execution, including timeout settings and parameter prefixes.</remarks>
    public partial class Materializer :
        Contract.IMaterializer
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