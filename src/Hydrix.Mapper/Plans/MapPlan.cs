using System;

namespace Hydrix.Mapper.Plans
{
    /// <summary>
    /// Represents the immutable compiled mapping plan for a single source and destination type pair.
    /// </summary>
    /// <remarks>
    /// A plan contains the single precompiled delegate that creates the destination instance, transfers every mapped
    /// property, and returns the populated destination as a boxed object. Once created, the instance is safe to reuse
    /// concurrently.
    /// </remarks>
    internal sealed class MapPlan
    {
        /// <summary>
        /// Stores the compiled delegate that creates the destination instance, copies and converts every mapped property,
        /// and returns the populated destination.
        /// </summary>
        internal readonly Func<object, object> Execute;

        /// <summary>
        /// Initializes a new <see cref="MapPlan"/> instance with the compiled execute delegate.
        /// </summary>
        /// <param name="execute">
        /// The compiled delegate that accepts the boxed source and returns the fully populated boxed destination.
        /// </param>
        internal MapPlan(
            Func<object, object> execute)
        {
            Execute = execute;
        }
    }
}
