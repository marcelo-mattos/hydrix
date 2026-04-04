using System.Reflection;

namespace Hydrix.EntityFramework
{
    /// <summary>
    /// Represents a reference navigation translated from Entity Framework metadata.
    /// </summary>
    /// <remarks>This intermediate model stores the information required to create nested Hydrix table
    /// maps and join metadata.</remarks>
    internal sealed class ReflectedNavigation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectedNavigation"/> class.
        /// </summary>
        /// <param name="property">The CLR navigation property.</param>
        /// <param name="target">The reflected target entity model.</param>
        /// <param name="primaryKeyColumn">The target primary-key column used by Hydrix nested materialization when available.</param>
        /// <param name="mainColumns">The column names on the main side of the relationship.</param>
        /// <param name="targetColumns">The column names on the target side of the relationship.</param>
        /// <param name="isRequiredJoin">Indicates whether the relationship should be materialized as a required join.</param>
        public ReflectedNavigation(
            PropertyInfo property,
            ReflectedEntityModel target,
            string primaryKeyColumn,
            string[] mainColumns,
            string[] targetColumns,
            bool isRequiredJoin)
        {
            Property = property;
            Target = target;
            PrimaryKeyColumn = primaryKeyColumn;
            MainColumns = mainColumns;
            TargetColumns = targetColumns;
            IsRequiredJoin = isRequiredJoin;
        }

        /// <summary>
        /// Gets the CLR navigation property.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets the reflected target entity model.
        /// </summary>
        public ReflectedEntityModel Target { get; }

        /// <summary>
        /// Gets the target primary-key column used by Hydrix nested materialization when available.
        /// </summary>
        public string PrimaryKeyColumn { get; }

        /// <summary>
        /// Gets the column names on the main side of the relationship.
        /// </summary>
        public string[] MainColumns { get; }

        /// <summary>
        /// Gets the column names on the target side of the relationship.
        /// </summary>
        public string[] TargetColumns { get; }

        /// <summary>
        /// Gets a value indicating whether the relationship should be materialized as a required join.
        /// </summary>
        public bool IsRequiredJoin { get; }
    }
}
