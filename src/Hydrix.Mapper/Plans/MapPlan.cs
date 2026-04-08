using System;

namespace Hydrix.Mapper.Plans
{
    /// <summary>
    /// Represents the immutable compiled mapping plan for a single source and destination type pair.
    /// </summary>
    /// <remarks>
    /// A plan contains two precompiled delegates: a strongly typed one used by the generic typed API to eliminate
    /// boundary casts from the hot path, and an object-based one used by the polymorphic untyped API. Both delegates
    /// create the destination instance, transfer every mapped property, and return the populated destination. Once
    /// created, the instance is safe to reuse concurrently.
    /// </remarks>
    internal sealed class MapPlan
    {
        /// <summary>
        /// Stores the compiled delegate that accepts the boxed source and returns the fully populated boxed destination.
        /// Used by the untyped <c>Map&lt;TTarget&gt;(object)</c> overload.
        /// </summary>
        internal readonly Func<object, object> Execute;

        /// <summary>
        /// Stores the strongly typed compiled delegate (<c>Func&lt;TSource, TTarget&gt;</c>) that accepts and returns
        /// strongly typed instances. Used by the typed API to eliminate <c>castclass</c> instructions from the hot path.
        /// </summary>
        /// <remarks>
        /// The stored delegate is always a <c>Func&lt;TSource, TTarget&gt;</c> for the source and destination types
        /// encoded in the plan key. Call sites retrieve it via a direct cast to the expected delegate type.
        /// </remarks>
        internal readonly Delegate TypedExecute;

        /// <summary>
        /// Initializes a new <see cref="MapPlan"/> instance with the compiled execute delegates.
        /// </summary>
        /// <param name="execute">
        /// The compiled untyped delegate that accepts the boxed source and returns the fully populated boxed destination.
        /// </param>
        /// <param name="typedExecute">
        /// The compiled typed delegate (<c>Func&lt;TSource, TTarget&gt;</c>) that accepts the strongly typed source and
        /// returns the strongly typed destination without boundary casts.
        /// </param>
        internal MapPlan(
            Func<object, object> execute,
            Delegate typedExecute)
        {
            Execute = execute;
            TypedExecute = typedExecute;
        }
    }
}
