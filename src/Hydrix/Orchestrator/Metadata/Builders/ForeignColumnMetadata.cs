namespace Hydrix.Orchestrator.Metadata.Builders
{
    /// <summary>
    /// Represents metadata for a foreign column, including its name and projected name.
    /// </summary>
    /// <remarks>This class is used to encapsulate information about a foreign column in a database context,
    /// allowing for clear identification and mapping of column names to their projected representations.</remarks>
    public sealed class ForeignColumnMetadata
    {
        /// <summary>
        /// Gets the name of the database column associated with this property.
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// Gets the projected name of the entity, which may be used for display purposes.
        /// </summary>
        public string ProjectedName { get; }

        /// <summary>
        /// Initializes a new instance of the ForeignColumnMetadata class with the specified column name and projected
        /// name.
        /// </summary>
        /// <param name="columnName">The name of the column that this metadata represents. This value cannot be null or empty.</param>
        /// <param name="projectedName">The name used for the column in projections. This value cannot be null or empty.</param>
        public ForeignColumnMetadata(
            string columnName,
            string projectedName)
        {
            ColumnName = columnName;
            ProjectedName = projectedName;
        }
    }
}
