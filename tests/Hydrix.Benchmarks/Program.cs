using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Hydrix.Benchmarks.Infrastructure;

namespace Hydrix.Benchmarks
{
    /// <summary>
    /// Hosts the benchmark executable and dispatches BenchmarkDotNet with the shared benchmark configuration.
    /// </summary>
    /// <remarks>
    /// This entry point keeps the benchmark startup path intentionally small so every benchmark suite uses the same
    /// diagnosers and runtime options regardless of the command-line filter passed by the operator.
    /// </remarks>
    internal static class Program
    {
        /// <summary>
        /// Builds the benchmark configuration and runs the selected benchmarks from the current assembly.
        /// </summary>
        /// <param name="args">
        /// The command-line arguments forwarded to BenchmarkDotNet so callers can filter suites, choose exporters,
        /// or customize the run without changing source code.
        /// </param>
        private static void Main(
            string[] args)
        {
            var config = ManualConfig
                .Create(
                    DefaultConfig.Instance)
                .AddDiagnoser(
                    BenchmarkConfig.Memory)
                .WithOptions(
                    ConfigOptions.DisableOptimizationsValidator);

            BenchmarkSwitcher
                .FromAssembly(
                    typeof(Program).Assembly)
                .Run(
                    args,
                    config);
        }
    }
}
