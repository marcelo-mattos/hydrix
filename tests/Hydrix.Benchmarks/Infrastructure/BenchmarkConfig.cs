using BenchmarkDotNet.Diagnosers;

namespace Hydrix.Benchmarks.Infrastructure
{
    /// <summary>
    /// Centralizes reusable BenchmarkDotNet configuration components shared by the benchmark entry point and suites.
    /// </summary>
    /// <remarks>
    /// Keeping reusable diagnosers in one place prevents each benchmark class from instantiating its own infrastructure
    /// objects and helps every suite report results through the same diagnostic lens.
    /// </remarks>
    public static class BenchmarkConfig
    {
        /// <summary>
        /// Gets the shared memory diagnoser used by the benchmark host to capture allocation and garbage-collection data.
        /// </summary>
        /// <remarks>
        /// The default <see cref="MemoryDiagnoser"/> instance is sufficient for the current benchmark project and avoids
        /// duplicating diagnoser creation logic across the executable entry point.
        /// </remarks>
        public static MemoryDiagnoser Memory { get; } =
            MemoryDiagnoser.Default;
    }
}
