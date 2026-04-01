using System;

namespace Hydrix.Orchestrator.Resolvers
{
    /// <summary>
    /// Represents a set of resolved field and nested entity bindings for a table, providing access to the associated
    /// field and entity binding collections.
    /// </summary>
    internal sealed class ResolvedTableBindings
    {
        /// <summary>
        /// Gets the collection of resolved field bindings associated with the current instance.
        /// </summary>
        public ResolvedFieldBinding[] Fields { get; }

        /// <summary>
        /// Gets the collection of resolved nested bindings associated with this instance.
        /// </summary>
        public ResolvedNestedBinding[] Entities { get; }

        /// <summary>
        /// Initializes a new instance of the ResolvedTableBindings class with the specified field and entity bindings.
        /// </summary>
        /// <param name="fields">An array of ResolvedFieldBinding objects representing the field bindings to include. If null, an empty array
        /// is used.</param>
        /// <param name="entities">An array of ResolvedNestedBinding objects representing the nested entity bindings to include. If null, an
        /// empty array is used.</param>
        public ResolvedTableBindings(
            ResolvedFieldBinding[] fields,
            ResolvedNestedBinding[] entities)
        {
            Fields = fields ?? Array.Empty<ResolvedFieldBinding>();
            Entities = entities ?? Array.Empty<ResolvedNestedBinding>();
        }
    }
}
