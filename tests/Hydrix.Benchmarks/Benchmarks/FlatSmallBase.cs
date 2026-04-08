namespace Hydrix.Benchmarks.Benchmarks
{
    /// <summary>
    /// Declares the five scalar properties shared by <see cref="FlatSmallSrc"/> and <see cref="FlatSmallDto"/>,
    /// eliminating the duplicated member declarations between the two benchmark model classes.
    /// </summary>
    public abstract class FlatSmallBase
    {
        /// <summary>
        /// Gets or sets the synthetic identifier value.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the synthetic display name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the synthetic email value.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the synthetic age value.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the synthetic activity flag.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
