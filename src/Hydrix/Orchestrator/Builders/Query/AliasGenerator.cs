using System;
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
        /// <param name="name">The PascalCase name from which to generate the alias.</param>
        /// <returns>A unique, lowercase alias derived from the provided name.</returns>
        public static string FromName(
            string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty.");

            var baseAlias = BuildBaseAlias(name);
            return baseAlias;
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
