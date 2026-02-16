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
        /// Adds a grouped set of conditions combined using OR.
        /// The group is enclosed in parentheses.
        /// </summary>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="ConditionBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="ConditionBuilder"/> instance.</returns>
        public ConditionBuilder OrGroup(
            Action<ConditionBuilder> groupBuilder)
            => AddGroup(
                groupBuilder,
                isNot: false,
                isOr: true);

        /// <summary>
        /// Adds a grouped set of conditions combined using OR NOT.
        /// The group is enclosed in parentheses.
        /// </summary>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="ConditionBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="ConditionBuilder"/> instance.</returns>
        public ConditionBuilder OrNotGroup(
            Action<ConditionBuilder> groupBuilder)
            => AddGroup(
                groupBuilder,
                isNot: true,
                isOr: true);

        /// <summary>
        /// Conditionally adds a grouped set of conditions combined using OR.
        /// The group is enclosed in parentheses.
        /// </summary>
        /// <param name="predicate">Determines whether the condition should be added.</param>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="ConditionBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="ConditionBuilder"/> instance.</returns>
        public ConditionBuilder OrGroupIf(
            bool predicate,
            Action<ConditionBuilder> groupBuilder)
        {
            if (predicate)
                OrGroup(groupBuilder);

            return this;
        }

        /// <summary>
        /// Conditionally adds a grouped set of conditions combined using OR.
        /// The group is enclosed in parentheses.
        /// </summary>
        /// <param name="predicates">Determines whether the condition should be added.</param>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="ConditionBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="ConditionBuilder"/> instance.</returns>
        public ConditionBuilder OrGroupIf(
            bool[] predicates,
            Action<ConditionBuilder> groupBuilder)
            => OrGroupIf(
                predicates?.All(p => p) ?? false,
                groupBuilder);

        /// <summary>
        /// Conditionally adds a grouped set of conditions combined using OR NOT.
        /// The group is enclosed in parentheses.
        /// </summary>
        /// <param name="predicate">Determines whether the condition should be added.</param>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="ConditionBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="ConditionBuilder"/> instance.</returns>
        public ConditionBuilder OrNotGroupIf(
            bool predicate,
            Action<ConditionBuilder> groupBuilder)
        {
            if (predicate)
                OrNotGroup(groupBuilder);

            return this;
        }

        /// <summary>
        /// Conditionally adds a grouped set of conditions combined using OR NOT.
        /// The group is enclosed in parentheses.
        /// </summary>
        /// <param name="predicates">Determines whether the condition should be added.</param>
        /// <param name="groupBuilder">
        /// An action that receives a new <see cref="ConditionBuilder"/> to define grouped conditions.
        /// </param>
        /// <returns>The current <see cref="ConditionBuilder"/> instance.</returns>
        public ConditionBuilder OrNotGroupIf(
            bool[] predicates,
            Action<ConditionBuilder> groupBuilder)
            => OrNotGroupIf(
                predicates?.All(p => p) ?? false,
                groupBuilder);
    }
}