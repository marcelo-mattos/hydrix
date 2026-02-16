using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Hydrix.Orchestrator.Builders.Query
{
    /// <summary>
    /// Provides methods for generating deterministic, lowercase aliases from PascalCase names, ensuring uniqueness by
    /// appending incremental numbers when necessary.
    /// </summary>
    /// <remarks>This class is intended for internal use in scenarios where unique, concise aliases are
    /// required, such as query building or mapping operations. It supports collision avoidance by tracking used aliases
    /// in a thread-safe manner when a concurrent dictionary is supplied.</remarks>
    internal static class AliasGenerator
    {
        /// <summary>
        /// Generates a deterministic alias from a PascalCase name.
        /// Uses uppercase letters and converts them to lowercase.
        /// Handles collision by appending incremental numbers.
        /// </summary>
        public static string FromName(
            string name,
            ConcurrentDictionary<string, byte> usedAliases = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty.");

            var baseAlias = BuildBaseAlias(name);

            if (usedAliases == null)
                return baseAlias;

            var alias = baseAlias;
            var counter = 1;

            while (!usedAliases.TryAdd(alias, 0))
                alias = $"{baseAlias}{counter++}";

            return alias;
        }

        /// <summary>
        /// Builds the base alias using uppercase letters of the name.
        /// </summary>
        private static string BuildBaseAlias(
            string name)
        {
            var capitals = name
                .Where(char.IsUpper)
                .ToArray();

            if (capitals.Length > 0)
                return new string(capitals)
                    .ToLowerInvariant();

            return name
                .Substring(0, 1)
                .ToLowerInvariant();
        }
    }
}
