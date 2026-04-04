using System;
using System.Collections.Generic;

namespace Hydrix.EntityFramework
{
    /// <summary>
    /// Represents the intermediate reflected model for a CLR entity type.
    /// </summary>
    /// <remarks>This object groups the scalar fields, primary-key columns, and navigations resolved
    /// from Entity Framework before the data is converted into the final Hydrix metadata objects.</remarks>
    internal sealed class ReflectedEntityModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectedEntityModel"/> class.
        /// </summary>
        /// <param name="entityType">The original Entity Framework entity metadata object.</param>
        /// <param name="clrType">The CLR type represented by the metadata.</param>
        /// <param name="table">The resolved database table name.</param>
        /// <param name="schema">The resolved database schema name.</param>
        /// <param name="fields">The reflected scalar field metadata.</param>
        /// <param name="primaryKeyColumns">The resolved primary-key column names.</param>
        public ReflectedEntityModel(
            object entityType,
            Type clrType,
            string table,
            string schema,
            IReadOnlyList<ReflectedScalarProperty> fields,
            string[] primaryKeyColumns)
        {
            EntityType = entityType;
            ClrType = clrType;
            Table = table;
            Schema = schema;
            Fields = fields;
            PrimaryKeyColumns = primaryKeyColumns;
            Navigations = Array.Empty<ReflectedNavigation>();
        }

        /// <summary>
        /// Gets the original Entity Framework entity metadata object.
        /// </summary>
        public object EntityType { get; }

        /// <summary>
        /// Gets the CLR type represented by the metadata.
        /// </summary>
        public Type ClrType { get; }

        /// <summary>
        /// Gets the resolved database table name.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// Gets the resolved database schema name.
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Gets the reflected scalar field metadata.
        /// </summary>
        public IReadOnlyList<ReflectedScalarProperty> Fields { get; }

        /// <summary>
        /// Gets the resolved primary-key column names.
        /// </summary>
        public string[] PrimaryKeyColumns { get; }

        /// <summary>
        /// Gets the reflected navigation metadata.
        /// </summary>
        public IReadOnlyList<ReflectedNavigation> Navigations { get; private set; }

        /// <summary>
        /// Replaces the reflected navigation metadata associated with the entity model.
        /// </summary>
        /// <param name="navigations">The navigation metadata that should be associated with the entity model.</param>
        public void SetNavigations(
            IReadOnlyList<ReflectedNavigation> navigations)
            => Navigations = navigations;
    }
}
