namespace Hydrix.Benchmarks.Models
{
    /// <summary>
    /// Describes the persisted lifecycle state of a benchmark user row.
    /// </summary>
    /// <remarks>
    /// The values are stored as integers in SQLite and are reused by the flat and nested projections so every data
    /// access strategy materializes the same semantic state.
    /// </remarks>
    public enum UserStatus
    {
        /// <summary>
        /// Indicates that the user row represents an active account.
        /// </summary>
        Active = 0,

        /// <summary>
        /// Indicates that the user row represents an account currently blocked from regular use.
        /// </summary>
        Blocked = 1,

        /// <summary>
        /// Indicates that the user row represents a logically deleted account retained only for benchmark data shape.
        /// </summary>
        Deleted = 2,
    }
}
