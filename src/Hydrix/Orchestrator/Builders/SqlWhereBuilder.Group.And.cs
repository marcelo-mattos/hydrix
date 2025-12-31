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
        /// Adds a grouped set of conditions combined using AND.
        /// The group is enclosed in parentheses.
        /// </summary>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="SqlWhereBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder AndGroup(
            Action<SqlWhereBuilder> groupBuilder)
            => AddGroup(groupBuilder);

        /// <summary>
        /// Adds a grouped set of conditions combined using AND NOT.
        /// The group is enclosed in parentheses.
        /// </summary>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="SqlWhereBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder AndNotGroup(
            Action<SqlWhereBuilder> groupBuilder)
            => AddGroup(groupBuilder, isNot: true);

        /// <summary>
        /// Conditionally adds a grouped set of conditions combined using AND.
        /// The group is enclosed in parentheses.
        /// </summary>
        /// <param name="predicate">Determines whether the condition should be added.</param>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="SqlWhereBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder AndGroupIf(
            bool predicate,
            Action<SqlWhereBuilder> groupBuilder)
        {
            if (predicate)
                AndGroup(groupBuilder);

            return this;
        }

        /// <summary>
        /// Conditionally adds a grouped set of conditions combined using AND.
        /// The group is enclosed in parentheses.
        /// </summary>
        /// <param name="predicates">Determines whether the condition should be added.</param>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="SqlWhereBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder AndGroupIf(
            bool[] predicates,
            Action<SqlWhereBuilder> groupBuilder)
            => AndGroupIf(
                predicates?.All(p => p) ?? false,
                groupBuilder);

        /// <summary>
        /// Conditionally adds a grouped set of conditions combined using AND NOT.
        /// The group is enclosed in parentheses.
        /// </summary>
        /// <param name="predicate">Determines whether the condition should be added.</param>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="SqlWhereBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder AndNotGroupIf(
            bool predicate,
            Action<SqlWhereBuilder> groupBuilder)
        {
            if (predicate)
                AndNotGroup(groupBuilder);

            return this;
        }

        /// <summary>
        /// Conditionally adds a grouped set of conditions combined using AND NOT.
        /// The group is enclosed in parentheses.
        /// </summary>
        /// <param name="predicates">Determines whether the condition should be added.</param>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="SqlWhereBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder AndNotGroupIf(
            bool[] predicates,
            Action<SqlWhereBuilder> groupBuilder)
            => AndNotGroupIf(
                predicates?.All(p => p) ?? false,
                groupBuilder);
    }
}