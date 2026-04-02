using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Hydrix.Benchmarks.Infrastructure;

namespace Hydrix.Benchmarks
{
    /// <summary>
    /// Contains the entry point for the benchmarking application and configures the benchmark execution environment.
    /// </summary>
    /// <remarks>This class is intended for internal use and is responsible for setting up the benchmark
    /// configuration, including memory diagnostics and disabling optimization validation. It launches the benchmark
    /// suite using the current assembly and the provided command-line arguments.</remarks>
    internal static class Program
    {
        /// <summary>
        /// Runs the benchmark tests defined in the assembly using the specified configuration.
        /// </summary>
        /// <remarks>This method initializes the benchmark configuration with memory diagnostics and
        /// disables optimizations validation to ensure accurate benchmarking results.</remarks>
        /// <param name="args">An array of command-line arguments that can be used to customize the benchmark execution.</param>
        /// <returns>A task that represents the asynchronous operation of running the benchmarks.</returns>
        private static void Main(
            string[] args)
        {
            // NOTE:
            // 1) Run in Release:
            //    dotnet run -c Release -f net8.0 -- --filter *
            // 2) For more stable results, pin CPU governor / disable turbo, close background apps.
            var config = ManualConfig
                .Create(DefaultConfig.Instance)
                .AddDiagnoser(BenchmarkConfig.Memory)
                .WithOptions(ConfigOptions.DisableOptimizationsValidator);

            BenchmarkSwitcher
                .FromAssembly(typeof(Program).Assembly)
                .Run(args, config);
        }
    }
}
