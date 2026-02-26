using System;

namespace Hydrix.Orchestrator.Binders.Entity
{
    /// <summary>
    /// Represents a binding for a data column, including its name, data type, and a function to retrieve values from
    /// entities of type TEntity.
    /// </summary>
    /// <remarks>This struct is useful for defining the schema of a data column in a data table, allowing for
    /// dynamic access to property values from entities.</remarks>
    /// <typeparam name="TEntity">The type of the entity from which the column values are retrieved.</typeparam>
    internal readonly struct DataColumnBinding<TEntity>
    {
        /// <summary>
        /// Gets the name of the column, which may be derived from a property name or a custom attribute. This name is used
        /// to identify the column in the data table.
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// Gets the data type associated with the current instance.
        /// </summary>
        public Type DataType { get; }

        /// <summary>
        /// Gets a delegate that retrieves the value of a specified property from an entity of type TEntity.
        /// </summary>
        /// <remarks>This property provides a function for dynamic property access, which is useful in
        /// scenarios such as reflection or mapping where property values need to be obtained without compile-time
        /// knowledge of the property name.</remarks>
        public Func<TEntity, object> Getter { get; }

        /// <summary>
        /// Initializes a new instance of the DataColumnBinding class for the specified column name, data type, and
        /// value accessor function.
        /// </summary>
        /// <remarks>Use this constructor to define the schema of a data column, including how to access
        /// its values from entities. The getter function should return the value corresponding to the column for each
        /// entity instance.</remarks>
        /// <param name="columnName">The name used to identify the column within the data structure. Cannot be null or empty.</param>
        /// <param name="dataType">The type of data stored in the column. Determines the kind of values that can be assigned to this column.
        /// Cannot be null.</param>
        /// <param name="getter">A function that retrieves the value of the column for a given entity of type TEntity. Must not be null.</param>
        public DataColumnBinding(
            string columnName,
            Type dataType,
            Func<TEntity, object> getter)
        {
            ColumnName = columnName;
            DataType = dataType;
            Getter = getter;
        }
    }
}