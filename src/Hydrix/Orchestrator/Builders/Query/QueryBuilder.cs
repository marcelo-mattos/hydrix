using Hydrix.Orchestrator.Builders.Query.Conditions;
using Hydrix.Orchestrator.Caching;
using Hydrix.Orchestrator.Metadata.Builders;
using Hydrix.Schemas.Contract;
using System.Text;

namespace Hydrix.Orchestrator.Builders.Query
{
    /// <summary>
    /// Provides static methods for constructing SQL query strings for entities based on metadata, including support for
    /// SELECT, FROM, JOIN, and optional WHERE clauses.
    /// </summary>
    /// <remarks>Use the methods in this class to generate SQL queries for entities represented by metadata
    /// objects. The class ensures that table aliases are valid and applies schema formatting when necessary. All
    /// methods are thread-safe and intended for use in query orchestration scenarios.</remarks>
    internal static class QueryBuilder
    {
        /// <summary>
        /// Generates a SQL query string for the specified entity, including SELECT, FROM, and optional WHERE clauses.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity for which the SQL query is being generated. Must implement <see cref="ITable"/>.</typeparam>
        /// <remarks>If the 'where' parameter is provided and contains conditions, its output is appended
        /// as a WHERE clause to the generated SQL query. The method ensures that the table alias is valid and generates
        /// a default if necessary.</remarks>
        /// <param name="where">An optional condition builder specifying additional filtering criteria for the SQL query. If null, no WHERE
        /// clause is included.</param>
        /// <returns>A string containing the complete SQL query for the entity, including SELECT, FROM, and any specified WHERE
        /// conditions.</returns>
        public static string Build<TEntity>(
            WhereBuilder where)
            where TEntity : ITable
        {
            var aliasContext = new AliasContext();

            var metadata = EntityBuilderMetadataCache.GetMetadata(typeof(TEntity));
            var mainAlias = aliasContext.GetAlias(metadata.Entity);

            var selectSql = SelectBuilder.Build(
                metadata,
                mainAlias,
                aliasContext);

            var joinSql = JoinBuilder.Build(
                metadata,
                mainAlias,
                aliasContext);

            var sql = new StringBuilder();

            sql.Append(selectSql);
            sql.AppendLine($"FROM {FormatTable(metadata)} {mainAlias}");
            sql.Append(joinSql);

            var whereClause = where?.Build();
            if (!string.IsNullOrWhiteSpace(whereClause))
                sql.AppendLine(whereClause);

            return sql.ToString();
        }

        /// <summary>
        /// Formats the table name by prepending the schema name if it is provided.
        /// </summary>
        /// <remarks>If the schema is null or whitespace, only the table name is returned.</remarks>
        /// <param name="metadata">The metadata containing the schema and table information to format.</param>
        /// <returns>A string representing the formatted table name, which includes the schema if specified.</returns>
        private static string FormatTable(EntityBuilderMetadata metadata)
            => (string.IsNullOrWhiteSpace(metadata.Schema))
                ? metadata.Table
                : $"{metadata.Schema}.{metadata.Table}";
    }
}