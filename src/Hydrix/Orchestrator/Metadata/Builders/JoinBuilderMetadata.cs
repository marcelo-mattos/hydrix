using System.Reflection;

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
        /// The name of the table participating in the join operation.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// The schema that identifies the location of the table within the database.
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// The alias used for the column in SQL queries, typically in the format "TableName.ColumnName". Cannot be null or empty.
        /// </summary>
        public string Alias { get; }

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
        /// The navigation property associated with the join, represented by a PropertyInfo object.
        /// </summary>
        public PropertyInfo NavigationProperty { get; }

        /// <summary>
        /// Initializes a new instance of the JoinBuilderMetadata class, providing metadata for configuring a join
        /// operation between database tables.
        /// </summary>
        /// <remarks>Use this constructor when defining join relationships in an object-relational mapping
        /// (ORM) context. Specifying primary and foreign keys, as well as join requirements, helps ensure correct
        /// mapping and query generation.</remarks>
        /// <param name="table">The name of the table participating in the join operation.</param>
        /// <param name="schema">The schema that identifies the location of the table within the database.</param>
        /// <param name="alias">The alias used for the column in SQL queries, typically in the format "TableName.ColumnName". Cannot be null or empty.</param>
        /// <param name="primaryKeys">An array containing the names of the primary key columns that uniquely identify records in the table.</param>
        /// <param name="foreignKeys">An array containing the names of the foreign key columns used to establish relationships with other tables.</param>
        /// <param name="isRequiredJoin">A value indicating whether the join is required (<see langword="true"/>) or optional (<see
        /// langword="false"/>).</param>
        /// <param name="navigationProperty">The navigation property associated with the join, represented by a PropertyInfo object.</param>
        public JoinBuilderMetadata(
            string table,
            string schema,
            string alias,
            string[] primaryKeys,
            string[] foreignKeys,
            bool isRequiredJoin,
            PropertyInfo navigationProperty)
        {
            Table = table;
            Schema = schema;
            Alias = alias;
            PrimaryKeys = primaryKeys;
            ForeignKeys = foreignKeys;
            IsRequiredJoin = isRequiredJoin;
            NavigationProperty = navigationProperty;
        }
    }
}
