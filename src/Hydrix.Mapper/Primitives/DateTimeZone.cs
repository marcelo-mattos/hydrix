namespace Hydrix.Mapper.Primitives
{
    /// <summary>
    /// Defines the timezone normalization performed before a date or time value is formatted as a string.
    /// </summary>
    public enum DateTimeZone
    {
        /// <summary>
        /// Leaves the value unchanged before formatting.
        /// </summary>
        None,

        /// <summary>
        /// Converts the value to Coordinated Universal Time before formatting.
        /// </summary>
        ToUtc,

        /// <summary>
        /// Converts the value to local time before formatting.
        /// </summary>
        ToLocal,
    }
}
