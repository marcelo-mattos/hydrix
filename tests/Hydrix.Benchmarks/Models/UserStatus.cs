namespace Hydrix.Benchmarks.Models
{
    /// <summary>
    /// Specifies the possible statuses that a user account can have within the system.
    /// </summary>
    /// <remarks>Use this enumeration to represent and manage the current state of a user, such as whether the
    /// user is active, blocked, or deleted. This can be useful for controlling access, displaying user information, or
    /// enforcing business rules based on user status.</remarks>
    public enum UserStatus :
        int
    {
        /// <summary>
        /// Indicates that the entity is currently active.
        /// </summary>
        Active = 0,

        /// <summary>
        /// Indicates that the entity is in a blocked state.
        /// </summary>
        Blocked = 1,

        /// <summary>
        /// Represents the state indicating that an item has been deleted.
        /// </summary>
        Deleted = 2
    }
}