using System;

namespace Hydrix.Orchestrator.Resolvers
{
    /// <summary>
    /// Represents the resolved binding information for a nested object within a table mapping, including primary key
    /// usage, candidate ordinals, object factory, property setter, and associated table bindings.
    /// </summary>
    /// <remarks>This type is used internally to encapsulate the metadata and delegates required to
    /// materialize and assign nested objects during data mapping operations. It provides details about how the nested
    /// object is constructed and how its properties are set, as well as information about primary key handling and
    /// candidate columns.</remarks>
    internal sealed class ResolvedNestedBinding
    {
        /// <summary>
        /// Gets a value indicating whether the primary key is used in the current context.
        /// </summary>
        public bool UsesPrimaryKey { get; }

        /// <summary>
        /// Gets the ordinal position of the column within the primary key, if the column is part of a primary key.
        /// </summary>
        /// <remarks>The ordinal is typically a one-based index indicating the order of the column in the
        /// primary key definition. If the column is not part of a primary key, the value may be zero or undefined
        /// depending on the implementation.</remarks>
        public int PrimaryKeyOrdinal { get; }

        /// <summary>
        /// Gets the collection of candidate ordinals associated with the current instance.
        /// </summary>
        public int[] CandidateOrdinals { get; }

        /// <summary>
        /// Gets the factory function used to create new instances of the associated object type.
        /// </summary>
        /// <remarks>The returned delegate encapsulates the logic for constructing objects, which may be
        /// used for dependency injection, object pooling, or other scenarios requiring dynamic instantiation.</remarks>
        public Func<object> Factory { get; }

        /// <summary>
        /// Gets the delegate used to set the value of a property or field on a target object.
        /// </summary>
        /// <remarks>The delegate accepts two parameters: the target object whose property or field is to
        /// be set, and the value to assign. This is typically used for dynamic property or field assignment scenarios,
        /// such as serialization or object mapping.</remarks>
        public Action<object, object> Setter { get; }

        /// <summary>
        /// Gets the resolved table bindings associated with the current context.
        /// </summary>
        public ResolvedTableBindings Bindings { get; }

        /// <summary>
        /// Initializes a new instance of the ResolvedNestedBinding class with the specified primary key usage,
        /// ordinals, factory, setter, and table bindings.
        /// </summary>
        /// <param name="usesPrimaryKey">true to indicate that the primary key is used for binding; otherwise, false.</param>
        /// <param name="primaryKeyOrdinal">The ordinal position of the primary key within the data source. Must be non-negative if usesPrimaryKey is
        /// true.</param>
        /// <param name="candidateOrdinals">An array of ordinal positions that are considered as candidates for binding. If null, an empty array is
        /// used.</param>
        /// <param name="factory">A delegate that creates an instance of the nested object to be bound. Cannot be null.</param>
        /// <param name="setter">A delegate that sets the value of the nested object on the parent object. Cannot be null.</param>
        /// <param name="bindings">The resolved table bindings that define how nested objects are mapped. Cannot be null.</param>
        public ResolvedNestedBinding(
            bool usesPrimaryKey,
            int primaryKeyOrdinal,
            int[] candidateOrdinals,
            Func<object> factory,
            Action<object, object> setter,
            ResolvedTableBindings bindings)
        {
            UsesPrimaryKey = usesPrimaryKey;
            PrimaryKeyOrdinal = primaryKeyOrdinal;
            CandidateOrdinals = candidateOrdinals ?? Array.Empty<int>();
            Factory = factory;
            Setter = setter;
            Bindings = bindings;
        }
    }
}
