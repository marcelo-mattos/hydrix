using System;
using System.Collections.Generic;

namespace Hydrix.Orchestrator.Metadata.Builders
{
    /// <summary>
    /// Provides metadata describing an entity's type, database table, schema, columns, and joins for use in entity
    /// mapping and configuration.
    /// </summary>
    /// <remarks>This class is typically used in database context scenarios to define the structure and
    /// relationships of an entity. It supplies information necessary for building and configuring entity mappings,
    /// including the entity's type, associated table and schema, and its columns and joins. Instances of this class are
    /// immutable after construction.</remarks>
    public sealed class EntityBuilderMetadata
    {
        /// <summary>
        /// Gets the identifier of the entity associated with this instance.
        /// </summary>
        /// <remarks>This property provides the unique identifier for the entity, which can be used for
        /// reference in various operations. It is important to note that the value is read-only and cannot be modified
        /// after the instance is created.</remarks>
        public string Entity { get; }

        /// <summary>
        /// The type that defines the structure and behavior of the entity being represented. Cannot be null.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The name of the database table associated with the entity. Cannot be null or empty.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// The name of the database schema in which the table resides. May be null or empty if the default schema is used.
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// A list of ColumnBuilderMetadata objects that describe the columns mapped to the entity. Cannot be null.
        /// </summary>
        public IReadOnlyList<ColumnBuilderMetadata> Columns { get; }

        /// <summary>
        /// A list of JoinBuilderMetadata objects that specify relationships between the entity and other tables. Cannot be null.
        /// </summary>
        public IReadOnlyList<JoinBuilderMetadata> Joins { get; }

        /// <summary>
        /// Initializes a new instance of the EntityBuilderMetadata class, which encapsulates metadata for configuring
        /// an entity and its database mapping.
        /// </summary>
        /// <remarks>Use this constructor to create an EntityBuilderMetadata instance that provides the
        /// necessary details for building and managing entity configurations, including table mapping and
        /// relationships.</remarks>
        /// <param name="entity">The identifier of the entity associated with this metadata. Cannot be null or empty.</param>
        /// <param name="type">The type that defines the structure and behavior of the entity being represented. Cannot be null.</param>
        /// <param name="table">The name of the database table associated with the entity. Cannot be null or empty.</param>
        /// <param name="schema">The name of the database schema in which the table resides. May be null or empty if the default schema is used.</param>
        /// <param name="columns">A list of ColumnBuilderMetadata objects that describe the columns mapped to the entity. Cannot be null.</param>
        /// <param name="joins">A list of JoinBuilderMetadata objects that specify relationships between the entity and other tables. Cannot be null.</param>
        public EntityBuilderMetadata(
            string entity,
            Type type,
            string table,
            string schema,
            IReadOnlyList<ColumnBuilderMetadata> columns,
            IReadOnlyList<JoinBuilderMetadata> joins)
        {
            Entity = entity;
            Type = type;
            Table = table;
            Schema = schema;
            Columns = columns;
            Joins = joins;
        }
    }
}
