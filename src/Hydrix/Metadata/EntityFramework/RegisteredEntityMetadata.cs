using Hydrix.Metadata.Builders;
using Hydrix.Metadata.Materializers;
using System;

namespace Hydrix.Metadata.EntityFramework
{
    /// <summary>
    /// Represents the translated metadata registered for a single CLR type.
    /// </summary>
    /// <remarks>Instances of this class bridge Entity Framework model metadata into the same internal
    /// contracts Hydrix already uses for validation, query building, and data materialization.</remarks>
    internal sealed class RegisteredEntityMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisteredEntityMetadata"/> class.
        /// </summary>
        /// <param name="type">The CLR type represented by this metadata entry.</param>
        /// <param name="materializeMetadata">The materialization metadata translated for the CLR type.</param>
        /// <param name="builderMetadata">The query-builder metadata translated for the CLR type.</param>
        /// <exception cref="ArgumentNullException">Thrown when any constructor argument is null.</exception>
        public RegisteredEntityMetadata(
            Type type,
            TableMaterializeMetadata materializeMetadata,
            EntityBuilderMetadata builderMetadata)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            MaterializeMetadata = materializeMetadata ?? throw new ArgumentNullException(nameof(materializeMetadata));
            BuilderMetadata = builderMetadata ?? throw new ArgumentNullException(nameof(builderMetadata));
        }

        /// <summary>
        /// Gets the CLR type represented by this registration.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the materialization metadata translated for the registered CLR type.
        /// </summary>
        public TableMaterializeMetadata MaterializeMetadata { get; }

        /// <summary>
        /// Gets the query-builder metadata translated for the registered CLR type.
        /// </summary>
        public EntityBuilderMetadata BuilderMetadata { get; }

        /// <summary>
        /// Gets a value indicating whether the translated metadata is valid for Hydrix materialization.
        /// </summary>
        /// <remarks>The current validation rule mirrors the existing Hydrix expectation that at least one
        /// scalar field must be available for materialization.</remarks>
        public bool IsValid
            => MaterializeMetadata.Fields.Count > 0;
    }
}
