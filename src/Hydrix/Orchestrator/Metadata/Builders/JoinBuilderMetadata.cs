using System;
using System.Collections.Generic;

namespace Hydrix.Orchestrator.Metadata.Builders
{
    /// <summary>
    /// Represents metadata describing a join operation between database tables, including table names, schema, key
    /// relationships, and join requirements.
    /// </summary>
    /// <remarks>This class is used to define the structure and requirements of a join when constructing
    /// database queries. It specifies how tables are related through primary and foreign keys, whether the join is
    /// mandatory, and provides information about the navigation property used for object mapping. Use this metadata to
    /// guide query generation and ensure correct table relationships in data access scenarios.</remarks>
    public sealed class JoinBuilderMetadata
    {
        /// <summary>
        /// Gets the identifier of the entity associated with this instance.
        /// </summary>
        /// <remarks>This property provides the unique identifier for the entity, which can be used for
        /// reference in various operations. It is important to note that the value is read-only and cannot be modified
        /// after the instance is created.</remarks>
        public string Entity { get; }

        /// <summary>
        /// The name of the table participating in the join operation.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// The schema that identifies the location of the table within the database.
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// An array containing the names of the primary key columns that uniquely identify records in the table.
        /// </summary>
        public string[] PrimaryKeys { get; }

        /// <summary>
        /// An array containing the names of the foreign key columns used to establish relationships with other tables.
        /// </summary>
        public string[] ForeignKeys { get; }

        /// <summary>
        /// A value indicating whether the join is required.
        /// </summary>
        public bool IsRequiredJoin { get; }

        /// <summary>
        /// Gets the collection of foreign column metadata associated with this entity.
        /// </summary>
        /// <remarks>This property provides read-only access to the metadata of foreign columns, which can
        /// be useful for understanding the relationships between entities in a database context.</remarks>
        public IReadOnlyList<ForeignColumnMetadata> Columns { get; }

        /// <summary>
        /// Initializes a new instance of the JoinBuilderMetadata class, providing metadata for configuring a join
        /// operation between database tables.
        /// </summary>
        /// <remarks>Use this constructor when defining join relationships in an object-relational mapping
        /// (ORM) context. Specifying primary and foreign keys, as well as join requirements, helps ensure correct
        /// mapping and query generation.</remarks>
        /// <param name="entity">The identifier of the entity associated with this join metadata.</param>
        /// <param name="table">The name of the table participating in the join operation.</param>
        /// <param name="schema">The schema that identifies the location of the table within the database.</param>
        /// <param name="primaryKeys">An array containing the names of the primary key columns that uniquely identify records in the table.</param>
        /// <param name="foreignKeys">An array containing the names of the foreign key columns used to establish relationships with other tables.</param>
        /// <param name="isRequiredJoin">A value indicating whether the join is required (<see langword="true"/>) or optional (<see
        /// langword="false"/>).</param>
        /// <param name="columns">A collection of foreign column metadata associated with this entity. Cannot be null.</param>
        public JoinBuilderMetadata(
            string entity,
            string table,
            string schema,
            string[] primaryKeys,
            string[] foreignKeys,
            bool isRequiredJoin,
            IReadOnlyList<ForeignColumnMetadata> columns)
        {
            Entity = entity;
            Table = table;
            Schema = schema;
            PrimaryKeys = primaryKeys;
            ForeignKeys = foreignKeys;
            IsRequiredJoin = isRequiredJoin;
            Columns = columns ?? throw new ArgumentNullException(nameof(columns), "Columns must not be null.");
        }
    }
}
