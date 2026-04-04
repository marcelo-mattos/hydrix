using Xunit;

namespace Hydrix.UnitTests.EntityFramework
{
    /// <summary>
    /// Defines the xUnit collection used by the Entity Framework integration tests.
    /// </summary>
    /// <remarks>The tests in this collection mutate a process-wide metadata cache, so they must not
    /// run in parallel with each other.</remarks>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class HydrixEntityFrameworkTestCollection
    {
        /// <summary>
        /// Gets the collection name used by the Entity Framework integration tests.
        /// </summary>
        public const string Name = nameof(HydrixEntityFrameworkTestCollection);
    }
}
