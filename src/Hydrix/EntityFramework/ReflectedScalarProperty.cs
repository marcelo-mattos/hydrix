using System.Reflection;

namespace Hydrix.EntityFramework
{
    /// <summary>
    /// Represents a scalar property translated from Entity Framework metadata.
    /// </summary>
    /// <remarks>This intermediate model is used while the translator builds the final Hydrix metadata
    /// objects.</remarks>
    internal sealed class ReflectedScalarProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectedScalarProperty"/> class.
        /// </summary>
        /// <param name="property">The CLR property represented by the scalar metadata.</param>
        /// <param name="columnName">The resolved database column name.</param>
        /// <param name="isPrimaryKey">Indicates whether the property participates in the primary key.</param>
        /// <param name="isRequired">Indicates whether the property is required.</param>
        public ReflectedScalarProperty(
            PropertyInfo property,
            string columnName,
            bool isPrimaryKey,
            bool isRequired)
        {
            Property = property;
            ColumnName = columnName;
            IsPrimaryKey = isPrimaryKey;
            IsRequired = isRequired;
        }

        /// <summary>
        /// Gets the CLR property represented by the scalar metadata.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets the resolved database column name.
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// Gets a value indicating whether the property participates in the primary key.
        /// </summary>
        public bool IsPrimaryKey { get; }

        /// <summary>
        /// Gets a value indicating whether the property is required.
        /// </summary>
        public bool IsRequired { get; }
    }
}
