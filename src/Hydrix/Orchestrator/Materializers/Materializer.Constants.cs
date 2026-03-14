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
        /// Represent the maximum number of records that can be processed when only the first record is needed,
        /// such as in scenarios where a single record is expected or sufficient.
        /// </summary>
        internal const int FirstRecordLimit = 1;

        /// <summary>
        /// Represents the maximum number of records that can be processed when a single record is expected,
        /// allowing for a small buffer to accommodate scenarios where multiple records may be returned but only one is needed.
        /// </summary>
        internal const int SingleRecordLimit = 2;
    }
}