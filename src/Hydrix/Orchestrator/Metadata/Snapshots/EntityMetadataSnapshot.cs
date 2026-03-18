using Hydrix.Orchestrator.Metadata.Builders;
using System;

namespace Hydrix.Orchestrator.Metadata.Snapshots
{
    /// <summary>
    /// Represents a snapshot of metadata for a cached entity type, including its type information and associated
    /// metadata.
    /// </summary>
    internal class EntityMetadataSnapshot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMetadataSnapshot"/> class.
        /// </summary>
        /// <param name="entityType">The cached entity type.</param>
        /// <param name="metadata">The metadata associated with the cached entity type.</param>
        public EntityMetadataSnapshot(
            Type entityType,
            EntityBuilderMetadata metadata)
        {
            EntityType = entityType;
            Metadata = metadata;
        }

        /// <summary>
        /// Gets the cached entity type.
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// Gets the cached entity metadata.
        /// </summary>
        public EntityBuilderMetadata Metadata { get; }
    }
}