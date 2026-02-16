using System;
using System.Linq;

namespace Hydrix.Orchestrator.Builders.Query.Conditions
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
    public sealed partial class ConditionBuilder
    {
        /// <summary>
        /// Adds a grouped set of conditions combined using AND.
        /// The group is enclosed in parentheses.
        /// </summary>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="ConditionBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="ConditionBuilder"/> instance.</returns>
        public ConditionBuilder AndGroup(
            Action<ConditionBuilder> groupBuilder)
            => AddGroup(groupBuilder);

        /// <summary>
        /// Adds a grouped set of conditions combined using AND NOT.
        /// The group is enclosed in parentheses.
        /// </summary>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="ConditionBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="ConditionBuilder"/> instance.</returns>
        public ConditionBuilder AndNotGroup(
            Action<ConditionBuilder> groupBuilder)
            => AddGroup(groupBuilder, isNot: true);

        /// <summary>
        /// Conditionally adds a grouped set of conditions combined using AND.
        /// The group is enclosed in parentheses.
        /// </summary>
        /// <param name="predicate">Determines whether the condition should be added.</param>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="ConditionBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="ConditionBuilder"/> instance.</returns>
        public ConditionBuilder AndGroupIf(
            bool predicate,
            Action<ConditionBuilder> groupBuilder)
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
        /// An action that receives a new <see cref="ConditionBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="ConditionBuilder"/> instance.</returns>
        public ConditionBuilder AndGroupIf(
            bool[] predicates,
            Action<ConditionBuilder> groupBuilder)
            => AndGroupIf(
                predicates?.All(p => p) ?? false,
                groupBuilder);

        /// <summary>
        /// Conditionally adds a grouped set of conditions combined using AND NOT.
        /// The group is enclosed in parentheses.
        /// </summary>
        /// <param name="predicate">Determines whether the condition should be added.</param>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="ConditionBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="ConditionBuilder"/> instance.</returns>
        public ConditionBuilder AndNotGroupIf(
            bool predicate,
            Action<ConditionBuilder> groupBuilder)
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
        /// An action that receives a new <see cref="ConditionBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="ConditionBuilder"/> instance.</returns>
        public ConditionBuilder AndNotGroupIf(
            bool[] predicates,
            Action<ConditionBuilder> groupBuilder)
            => AndNotGroupIf(
                predicates?.All(p => p) ?? false,
                groupBuilder);
    }
}