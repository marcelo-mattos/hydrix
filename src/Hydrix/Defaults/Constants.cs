namespace Hydrix.Defaults
{
    /// <summary>
    /// Provides default constant values used across Hydrix.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Represents the maximum number of records that can be processed when only the first record is needed.
        /// </summary>
        internal const int FirstRecordLimit = 1;

        /// <summary>
        /// Represents the maximum number of records that can be processed when a single record is expected.
        /// </summary>
        internal const int SingleRecordLimit = 2;
    }
}