using BenchmarkDotNet.Diagnosers;

namespace Hydrix.Benchmarks.Infrastructure
{
    /// <summary>
    /// Provides centralized configuration settings for benchmarking, including predefined diagnosers for consistent and
    /// maintainable benchmark setup.
    /// </summary>
    /// <remarks>This static class is intended to simplify the configuration of benchmarking tools by exposing
    /// commonly used diagnosers as static members. Centralizing diagnoser configuration helps keep benchmark entry
    /// points, such as the Program class, clean and focused.</remarks>
    public static class BenchmarkConfig
    {
        /// <summary>
        /// Provides a centralized instance of the default memory diagnoser for performance benchmarking.
        /// </summary>
        /// <remarks>Use this static field to ensure consistent application of memory diagnostics across
        /// benchmarks. Centralizing diagnosers helps maintain a clean and organized program structure.</remarks>
        public static readonly MemoryDiagnoser Memory = MemoryDiagnoser.Default;
    }
}
