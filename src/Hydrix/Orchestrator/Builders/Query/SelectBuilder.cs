using Hydrix.Orchestrator.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
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
    internal static class SelectBuilder
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
        /// <param name="aliasContext">The context for managing table aliases in the SQL query. Must not be null.</param>
        /// <returns>A string containing the constructed SQL SELECT statement, including columns from the main entity and any
        /// joined tables.</returns>
        public static string Build(
            EntityBuilderMetadata metadata,
            string mainAlias,
            AliasContext aliasContext)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata), "Metadata must not be null.");

            if (string.IsNullOrWhiteSpace(mainAlias))
                throw new ArgumentNullException(nameof(mainAlias), "Main alias must not be null or whitespace.");

            var columns = new List<string>();

            columns.AddRange(
                metadata.Columns
                    .Select(c => $"\t{mainAlias}.{c.ColumnName}"));

            foreach (var join in metadata.Joins)
            {
                var joinAlias = aliasContext.GetAlias(join.Entity);

                foreach (var column in join.Columns)
                    columns.Add(
                        $"\t{joinAlias}.{column.ColumnName} AS \"{column.ProjectedName}\"");
            }

            var builder = new StringBuilder();
            if (columns.Count > 0)
            {
                builder.AppendLine("SELECT");
                builder.AppendLine(string.Join($",{Environment.NewLine}", columns));
            }
            else
            {
                builder.AppendLine("SELECT *");
            }

            return builder.ToString();
        }
    }
}