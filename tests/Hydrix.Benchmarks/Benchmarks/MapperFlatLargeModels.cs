namespace Hydrix.Benchmarks.Benchmarks
{
    /// <summary>
    /// Represents the wide flat source model used to stress mappers with a larger number of members.
    /// </summary>
    /// <remarks>
    /// The property names are intentionally synthetic because the benchmark is interested in object width and value-type
    /// diversity rather than in domain semantics.
    /// </remarks>
    public class FlatLargeSrc :
        FlatLargeBase
    { }

    /// <summary>
    /// Represents the wide flat destination model used to receive mapped values from <see cref="FlatLargeSrc"/>.
    /// </summary>
    public class FlatLargeDto :
        FlatLargeBase
    { }
}
