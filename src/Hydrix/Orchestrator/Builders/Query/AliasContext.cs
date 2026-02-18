using System;
using System.Collections.Generic;

namespace Hydrix.Orchestrator.Builders.Query
{
    /// <summary>
    /// Provides a context for managing table aliases, ensuring unique aliases are generated for each table name.
    /// </summary>
    /// <remarks>This class is used internally to maintain a mapping of table names to their corresponding
    /// aliases, preventing alias collisions during usage. It generates aliases based on the table name and appends an
    /// index if the alias is already in use.</remarks>
    internal sealed class AliasContext
    {
        /// <summary>
        /// Stores the mapping between database table names and their corresponding aliases.
        /// </summary>
        /// <remarks>This dictionary is used internally to associate each table name with a unique alias,
        /// which can be used to generate more readable or concise database queries. The dictionary is initialized as
        /// empty and populated as aliases are assigned.</remarks>
        private readonly Dictionary<string, string> _tableAliases =
            new Dictionary<string, string>();

        /// <summary>
        /// Stores the set of aliases that have been used to ensure uniqueness and prevent conflicts.
        /// </summary>
        /// <remarks>This collection is initialized as an empty set and is used internally to track which
        /// aliases have already been assigned. It helps avoid duplicate or conflicting alias assignments within the
        /// context.</remarks>
        private readonly HashSet<string> _usedAliases =
            new HashSet<string>();

        /// <summary>
        /// Generates and returns a unique alias for the specified table name.
        /// </summary>
        /// <remarks>If an alias for the given table name already exists, it is returned. Otherwise, a new
        /// alias is generated based on the table name, and a numeric suffix is appended if necessary to ensure
        /// uniqueness among previously generated aliases.</remarks>
        /// <param name="tableName">The name of the table for which to generate an alias. This value must not be null or empty.</param>
        /// <returns>A unique alias string for the specified table name. If an alias for the table name has already been
        /// generated, the existing alias is returned.</returns>
        public string GetAlias(
            string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName), "TableName must not be null or whitespace.");

            if (_tableAliases.TryGetValue(tableName, out var existing))
                return existing;

            var baseAlias = AliasGenerator.FromName(tableName);
            var alias = baseAlias;
            var index = 0;

            while (!_usedAliases.Add(alias))
                alias = $"{baseAlias}{++index}";

            _tableAliases[tableName] = alias;
            return alias;
        }
    }
}