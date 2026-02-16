using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hydrix.Orchestrator.Builders.Query
{
    /// <summary>
    /// Provides static methods for constructing SQL SELECT statements based on entity metadata and join relationships.
    /// </summary>
    /// <remarks>This class cannot be instantiated. It is designed to generate SQL SELECT statements that
    /// include only properties mapped to database columns, excluding those marked with NotMappedAttribute or
    /// ForeignTableAttribute. The generated SQL includes aliases for columns from joined tables to ensure clarity in
    /// the result set.</remarks>
    public static class SelectBuilder
    {
        /// <summary>
        /// Generates a SQL SELECT statement for the specified entity and its related joins using the provided metadata
        /// and main table alias.
        /// </summary>
        /// <remarks>Only properties mapped to database columns are included in the SELECT statement.
        /// Properties marked with NotMappedAttribute or ForeignTableAttribute are excluded. The generated SQL includes
        /// aliases for joined table columns to distinguish them in the result set.</remarks>
        /// <param name="metadata">The metadata describing the entity, including its columns and join relationships. Must not be null.</param>
        /// <param name="mainAlias">The alias to use for the main entity table in the generated SQL statement. Cannot be null or empty.</param>
        /// <returns>A string containing the constructed SQL SELECT statement, including columns from the main entity and any
        /// joined tables.</returns>
        public static string Build(
            EntityBuilderMetadata metadata,
            string mainAlias)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata), "Metadata must not be null.");

            if (string.IsNullOrWhiteSpace(mainAlias))
                throw new ArgumentNullException(nameof(mainAlias), "Main alias must not be null or whitespace.");

            var columns = new List<string>();

            columns.AddRange(
                metadata.Columns
                    .Select(c => $"\t{mainAlias}.{c.ColumnName}"));

            foreach (var join in metadata.Joins.Where(x => x.NavigationProperty != null))
            {
                var alias = AliasGenerator.FromName(join.Table);

                columns.AddRange(
                    join.NavigationProperty.PropertyType
                        .GetProperties(
                            BindingFlags.Instance |
                            BindingFlags.Public)
                        .Where(p =>
                            p.GetCustomAttribute<NotMappedAttribute>() == null &&
                            p.GetCustomAttribute<ForeignTableAttribute>() == null)
                        .Select(p =>
                        {
                            var columnAttr = p.GetCustomAttribute<ColumnAttribute>();
                            var columnName = columnAttr?.Name ?? p.Name;

                            return $"\t{alias}.{columnName} AS \"{join.Table}.{columnName}\"";
                        }));
            }

            var builder = new StringBuilder();
            builder.AppendLine("SELECT");
            builder.AppendLine(string.Join($",{Environment.NewLine}", columns));

            return builder.ToString();
        }
    }
}
