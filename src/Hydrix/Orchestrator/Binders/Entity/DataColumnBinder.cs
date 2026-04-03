namespace Hydrix.Orchestrator.Binders.Entity
{
    /// <summary>
    /// Provides a binding mechanism for mapping entity properties to data columns in a data table.
    /// </summary>
    /// <remarks>This class is designed to facilitate the mapping of entity properties to their corresponding
    /// data columns, ensuring that the structure and constraints of the data are maintained.</remarks>
    /// <typeparam name="TEntity">The type of the entity that the data columns are bound to.</typeparam>
    internal sealed class DataColumnBinder<TEntity>
    {
        /// <summary>
        /// Gets the array of data column bindings associated with the entity.
        /// </summary>
        /// <remarks>This property provides access to the data column bindings that define how the
        /// entity's properties are mapped to data columns. Each binding specifies the relationship between the entity
        /// and its corresponding data column, allowing for effective data manipulation and retrieval.</remarks>
        public DataColumnBinding<TEntity>[] Columns { get; }

        /// <summary>
        /// Initializes a new instance of the DataColumnBinder class with the specified array of data column bindings.
        /// </summary>
        /// <param name="columns">An array of DataColumnBinding&lt;TEntity&gt; that defines the columns to be bound. Each binding specifies how a
        /// property of TEntity is mapped to a data column.</param>
        public DataColumnBinder(
            DataColumnBinding<TEntity>[] columns)
            => Columns = columns;
    }
}
