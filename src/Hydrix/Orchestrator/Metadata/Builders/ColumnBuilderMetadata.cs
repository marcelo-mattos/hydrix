using System;

namespace Hydrix.Orchestrator.Metadata.Builders
{
    /// <summary>
    /// Represents metadata describing a database column, including its associated property name, column name, key
    /// status, and requirement status.
    /// </summary>
    /// <remarks>This class is used to define the characteristics of a column within a database schema for
    /// dynamic configuration scenarios. The <see langword="PropertyName"/> and <see langword="ColumnName"/> properties
    /// specify the names used in code and the database, respectively. The <see langword="IsKey"/> property indicates
    /// whether the column is a primary key, and <see langword="IsRequired"/> specifies if the column must contain a
    /// value. The <see langword="PropertyInfo"/> property provides reflection information about the associated
    /// property, enabling advanced operations such as dynamic mapping or validation.</remarks>
    public sealed class ColumnBuilderMetadata
    {
        /// <summary>
        /// The name of the property in the source object that is mapped to the database column. Cannot be null or empty.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// The name of the column in the database schema that corresponds to the property. Cannot be null or empty.
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// A value indicating whether the column is part of the primary key for the table.
        /// Set to <see langword="true"/> if the column is a key; otherwise, <see langword="false"/>.
        /// </summary>
        public bool IsKey { get; }

        /// <summary>
        /// A value indicating whether the column is required in the database schema.
        /// Set to <see langword="true"/> if the column cannot be null; otherwise, <see langword="false"/>.
        /// </summary>
        public bool IsRequired { get; }

        /// <summary>
        /// Gets a function that retrieves the value of a specified property from an object.
        /// </summary>
        /// <remarks>The getter function takes an object as input and returns the corresponding property
        /// value. This is useful for dynamically accessing properties in scenarios such as reflection or data
        /// binding.</remarks>
        public Func<object, object> Getter { get; }

        /// <summary>
        /// Initializes a new instance of the ColumnBuilderMetadata class, representing metadata for a database column.
        /// </summary>
        /// <param name="propertyName">The name of the property in the source object that is mapped to the database column. Cannot be null or
        /// empty.</param>
        /// <param name="columnName">The name of the column in the database schema that corresponds to the property. Cannot be null or empty.</param>
        /// <param name="isKey">A value indicating whether the column is part of the primary key for the table. Set to <see
        /// langword="true"/> if the column is a key; otherwise, <see langword="false"/>.</param>
        /// <param name="isRequired">A value indicating whether the column is required in the database schema. Set to <see langword="true"/> if
        /// the column cannot be null; otherwise, <see langword="false"/>.</param>
        /// <param name="getter">A function that retrieves the value of the property from an object.</param>
        public ColumnBuilderMetadata(
            string propertyName,
            string columnName,
            bool isKey,
            bool isRequired,
            Func<object, object> getter)
        {
            PropertyName = propertyName;
            ColumnName = columnName;
            IsKey = isKey;
            IsRequired = isRequired;
            Getter = getter;
        }
    }
}
