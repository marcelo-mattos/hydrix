using Xunit;

namespace Hydrix.Mapper.UnitTests
{
    /// <summary>
    /// Defines the xUnit collection used by tests that mutate the process-wide Hydrix mapper configuration.
    /// </summary>
    /// <remarks>
    /// The Hydrix mapper stores its default configuration in a global singleton. Tests that replace that state must run
    /// sequentially so the assertions remain deterministic across the full suite.
    /// </remarks>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class GlobalStateTestCollection
    {
        /// <summary>
        /// Identifies the sequential xUnit collection that isolates tests touching the global mapper configuration.
        /// </summary>
        public const string Name = "GlobalState";
    }
}