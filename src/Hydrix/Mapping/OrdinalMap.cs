using System.Collections.Generic;

namespace Hydrix.Mapping
{
    /// <summary>
    /// Represents an immutable mapping between string keys and their corresponding ordinal values, along with a schema
    /// hash for validation purposes.
    /// </summary>
    /// <remarks>This struct is intended for internal use and provides efficient access to ordinal values
    /// associated with specific string keys. The schema hash can be used to verify the integrity of the mapping against
    /// a defined schema.</remarks>
    internal readonly struct OrdinalMap
    {
        /// <summary>
        /// Gets a read-only dictionary that maps string keys to their corresponding ordinal values.
        /// </summary>
        /// <remarks>The dictionary provides a way to access ordinal values associated with specific keys,
        /// ensuring that the keys are unique and immutable. This property is useful for scenarios where a fixed mapping
        /// of keys to ordinal values is required.</remarks>
        public IReadOnlyDictionary<string, int> Ordinals { get; }

        /// <summary>
        /// Gets the hash value that represents the schema of the current object.
        /// </summary>
        public int SchemaHash { get; }

        /// <summary>
        /// Initializes a new instance of the OrdinalMap class using the specified ordinal mappings and schema hash.
        /// </summary>
        /// <remarks>Use this constructor to create an OrdinalMap that efficiently manages and accesses
        /// ordinal mappings consistent with a specific schema. Ensure that the ordinals dictionary accurately reflects
        /// the schema represented by the schema hash.</remarks>
        /// <param name="ordinals">A read-only dictionary that maps string keys to their corresponding ordinal values. Cannot be null.</param>
        /// <param name="schemaHash">The hash value representing the schema associated with the provided ordinals. Must be a non-negative
        /// integer.</param>
        public OrdinalMap(
            IReadOnlyDictionary<string, int> ordinals,
            int schemaHash)
        {
            Ordinals = ordinals;
            SchemaHash = schemaHash;
        }
    }
}
