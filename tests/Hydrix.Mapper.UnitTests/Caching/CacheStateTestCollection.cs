using Xunit;

namespace Hydrix.Mapper.UnitTests.Caching
{
    /// <summary>
    /// Defines the xUnit collection used by tests that mutate the static mapping-plan cache.
    /// </summary>
    /// <remarks>
    /// The cache is shared process-wide inside the mapper assembly, so tests that clear or inspect it directly must run
    /// sequentially to avoid cross-test interference.
    /// </remarks>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class CacheStateTestCollection
    {
        /// <summary>
        /// Identifies the sequential xUnit collection that isolates tests touching the static mapping-plan cache.
        /// </summary>
        public const string Name = "CacheState";
    }
}
