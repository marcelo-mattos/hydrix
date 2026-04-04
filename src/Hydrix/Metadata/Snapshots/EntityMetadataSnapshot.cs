using Hydrix.Metadata.Builders;
using System;

namespace Hydrix.Metadata.Snapshots
{
    /// <summary>
    /// Represents a snapshot of metadata for a cached entity type, including its type information, associated
    /// metadata, and the Entity Framework cache version observed when the snapshot was created.
    /// </summary>
    /// <remarks>This snapshot is used by the hot cache in <c>DatabaseEntity</c> so the cache can detect when
    /// metadata registered from Entity Framework has changed and refresh the builder metadata accordingly.</remarks>
    internal class EntityMetadataSnapshot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMetadataSnapshot"/> class.
        /// </summary>
        /// <param name="entityType">The cached entity type.</param>
        /// <param name="metadata">The metadata associated with the cached entity type.</param>
        /// <param name="version">The Entity Framework metadata cache version observed when the snapshot was created.</param>
        public EntityMetadataSnapshot(
            Type entityType,
            EntityBuilderMetadata metadata,
            int version)
        {
            EntityType = entityType;
            Metadata = metadata;
            Version = version;
        }

        /// <summary>
        /// Gets the cached entity type.
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// Gets the cached entity metadata.
        /// </summary>
        public EntityBuilderMetadata Metadata { get; }

        /// <summary>
        /// Gets the Entity Framework metadata cache version observed when the snapshot was created.
        /// </summary>
        public int Version { get; }
    }
}
