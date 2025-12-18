using System;
using System.Linq;

namespace Hydrix.Orchestrator.Builders
{
    /// <summary>
    /// Provides a fluent SQL builder responsible for composing the WHERE clause
    /// of a SQL statement in a safe, readable, and predictable manner.
    ///
    /// This builder does not execute SQL, validate syntax, or bind parameters.
    /// Its sole responsibility is to concatenate SQL conditions while ensuring
    /// correct logical operators (AND / OR) and grouping.
    ///
    /// If no conditions are added, the Build method returns an empty string.
    /// </summary>
    public sealed partial class SqlWhereBuilder
    {
        /// <summary>
        /// Builds the internal SQL expression without the WHERE keyword.
        /// This method is used internally to support grouping.
        /// </summary>
        /// <returns>The composed SQL expression.</returns>
        private string BuildInternal()
            => string.Join(" ", _tokens);

        /// <summary>
        /// Adds a SQL condition to the internal token list,
        /// automatically handling logical operators (AND / OR).
        /// </summary>
        /// <param name="condition">The SQL condition to add.</param>
        /// <param name="isNot">
        /// Indicates whether the grouped expression should be negated using NOT.
        /// </param>
        /// <param name="isOr">
        /// Indicates whether the condition should be prefixed with OR.
        /// When false, AND is used.
        /// </param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        private SqlWhereBuilder Add(
            string condition,
            bool isNot = false,
            bool isOr = false)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return this;

            condition = $"{(isNot ? "NOT " : string.Empty)}{condition.Trim()}";

            if (_tokens.Count == 0)
            {
                _tokens.Add(condition);
                return this;
            }

            _tokens.Add(isOr ? $"OR {condition}" : $"AND {condition}");
            return this;
        }

        /// <summary>
        /// Adds an internal grouped OR expression prefixed with AND.
        /// This method is responsible for composing expressions in the format:
        ///
        /// AND (condition1 OR condition2 OR conditionN)
        /// or, when the NOT modifier is enabled:
        /// AND (NOT condition1 OR condition2 OR conditionN)
        ///
        /// Only non-empty conditions are considered. If no valid conditions are provided,
        /// the builder remains unchanged.
        ///
        /// This method is intended for internal use only and serves as the core
        /// implementation for public methods that expose grouped OR behavior.
        /// </summary>
        /// <param name="isNot">
        /// Indicates whether the grouped expression should be negated using NOT.
        /// </param>
        /// <param name="conditions">
        /// A collection of SQL expressions to be combined using the OR operator.
        /// </param>
        /// <returns>
        /// The current <see cref="SqlWhereBuilder"/> instance.
        /// </returns>
        private SqlWhereBuilder AddAndOrGroup(
            bool isNot,
            params string[] conditions)
        {
            var valid = conditions?
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .ToList();

            if (valid == null || valid.Count == 0)
                return this;

            var group = string.Join(" OR ", valid);
            And($"{(isNot ? "NOT " : string.Empty)}({group})");

            return this;
        }

        /// <summary>
        /// Adds an internal grouped SQL expression composed by a nested
        /// <see cref="SqlWhereBuilder"/> instance.
        ///
        /// The resulting SQL is enclosed in parentheses and may optionally be:
        /// - negated using the NOT operator;
        /// - combined using either AND or OR in relation to the current builder state.
        ///
        /// This method is the core implementation behind public group-related
        /// fluent methods and ensures correct logical grouping and operator placement.
        /// </summary>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="SqlWhereBuilder"/> instance
        /// used to define the grouped SQL expression.
        /// </param>
        /// <param name="isNot">
        /// Indicates whether the grouped expression should be negated using NOT.
        /// </param>
        /// <param name="isOr">
        /// Indicates whether the grouped expression should be combined using OR.
        /// — When false, the AND operator is used.
        /// </param>
        /// <returns>
        /// The current <see cref="SqlWhereBuilder"/> instance.
        /// </returns>
        private SqlWhereBuilder AddGroup(
            Action<SqlWhereBuilder> groupBuilder,
            bool isNot = false,
            bool isOr = false)
        {
            var group = new SqlWhereBuilder();
            groupBuilder(group);

            var sql = group.BuildInternal();
            if (!string.IsNullOrEmpty(sql))
                Add($"{(isNot ? "NOT " : string.Empty)}({sql})", isOr);

            return this;
        }
    }
}