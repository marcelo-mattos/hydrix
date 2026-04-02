using Hydrix.Orchestrator.Metadata.Builders;
using System.Collections.Generic;
using System.Text;

namespace Hydrix.Orchestrator.Builders.Query
{
    /// <summary>
    /// Provides static methods for constructing SQL JOIN clauses using table metadata and aliases.
    /// </summary>
    /// <remarks>This class enables the generation of SQL JOIN statements, supporting both INNER and LEFT
    /// JOINs. It utilizes primary and foreign key mappings defined in the provided metadata to build JOIN conditions.
    /// Use this class to automate JOIN clause creation when working with relational table structures in SQL
    /// queries.</remarks>
    internal static class JoinBuilder
    {
        /// <summary>
        /// Generates a SQL JOIN clause based on the provided metadata and main table alias.
        /// </summary>
        /// <remarks>The method supports both INNER and LEFT JOINs based on the configuration in the
        /// metadata. It constructs the JOIN conditions using primary and foreign key mappings defined in the
        /// metadata.</remarks>
        /// <param name="metadata">The metadata containing information about the tables and their relationships used to construct the JOIN
        /// clause. Cannot be null.</param>
        /// <param name="mainAlias">The alias for the main table in the SQL query. Used to reference its columns in the JOIN conditions. Cannot
        /// be null or empty.</param>
        /// <param name="aliasContext">The context for managing table aliases in the SQL query. Must not be null.</param>
        /// <returns>A string representing the constructed SQL JOIN clause, including the necessary JOIN type and conditions.</returns>
        public static string Build(
            EntityBuilderMetadata metadata,
            string mainAlias,
            AliasContext aliasContext)
        {
            var sql = new StringBuilder();

            foreach (var join in metadata.Joins)
            {
                var foreignAlias = aliasContext.GetAlias(join.Entity);

                sql.Append(join.IsRequiredJoin ? "INNER JOIN " : "LEFT JOIN ");
                sql.Append(FormatTable(join));
                sql.Append($" {foreignAlias}");
                sql.Append(" ON ");

                var conditions = new List<string>();

                for (int i = 0; i < join.PrimaryKeys.Length; i++)
                    conditions.Add(
                        $"{mainAlias}.{join.ForeignKeys[i]} = {foreignAlias}.{join.PrimaryKeys[i]}");

                sql.AppendLine(string.Join(" AND ", conditions));
            }

            return sql.ToString();
        }

        /// <summary>
        /// Formats the table name by including the schema if it is specified.
        /// </summary>
        /// <remarks>If the schema is not specified, only the table name is returned.</remarks>
        /// <param name="join">The metadata containing the schema and table name to format.</param>
        /// <returns>A string representing the formatted table name, including the schema if provided.</returns>
        private static string FormatTable(JoinBuilderMetadata join)
            => (string.IsNullOrWhiteSpace(join.Schema))
                ? join.Table
                : $"{join.Schema}.{join.Table}";
    }
}
