namespace Hydrix.Benchmarks.Benchmarks
{
    /// <summary>
    /// Represents the smallest flat source model used by the mapper comparison benchmarks.
    /// </summary>
    /// <remarks>
    /// This type contains only five scalar members so the benchmark can isolate mapper overhead for a compact object
    /// graph before scaling up to wider payloads.
    /// </remarks>
    public class FlatSmallSrc :
        FlatSmallBase
    { }

    /// <summary>
    /// Represents the smallest flat destination model used by the mapper comparison benchmarks.
    /// </summary>
    public class FlatSmallDto :
        FlatSmallBase
    { }

    /// <summary>
    /// Represents the medium-width flat source model used by the mapper comparison benchmarks.
    /// </summary>
    /// <remarks>
    /// This type expands the number of mapped members to include strings, numerics, booleans, a date, and a GUID so the
    /// benchmarks can compare how each mapper behaves when the projection width increases.
    /// </remarks>
    public class FlatMediumSrc :
        FlatMediumBase
    { }

    /// <summary>
    /// Represents the medium-width flat destination model used by the mapper comparison benchmarks.
    /// </summary>
    public class FlatMediumDto :
        FlatMediumBase
    { }
}
