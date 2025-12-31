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
        /// Adds an OR condition to the WHERE clause.
        /// </summary>
        /// <param name="condition">The SQL condition to add.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder Or(
            string condition)
            => Add(condition,
                isNot: false,
                isOr: true);

        /// <summary>
        /// Adds an OR NOT condition to the WHERE clause.
        /// </summary>
        /// <param name="condition">The SQL condition to add.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder OrNot(
            string condition)
            => Add(condition,
                isNot: true,
                isOr: true);

        /// <summary>
        /// Conditionally adds an OR condition to the WHERE clause.
        /// The condition is added only if the predicate evaluates to true.
        /// </summary>
        /// <param name="predicate">Determines whether the condition should be added.</param>
        /// <param name="condition">The SQL condition to add.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder OrIf(
            bool predicate,
            string condition)
        {
            if (predicate)
                Add(condition,
                    isNot: false,
                    isOr: true);

            return this;
        }

        /// <summary>
        /// Conditionally adds an OR condition to the WHERE clause.
        /// The condition is added only if the predicate evaluates to true.
        /// </summary>
        /// <param name="predicates">Determines whether the condition should be added.</param>
        /// <param name="condition">The SQL condition to add.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder OrIf(
            bool[] predicates,
            string condition)
            => OrIf(
                predicates?.All(p => p) ?? false,
                condition);

        /// <summary>
        /// Conditionally adds an OR NOT condition to the WHERE clause.
        /// The condition is added only if the predicate evaluates to true.
        /// </summary>
        /// <param name="predicate">Determines whether the condition should be added.</param>
        /// <param name="condition">The SQL condition to add.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder OrNotIf(
            bool predicate,
            string condition)
        {
            if (predicate)
                Add(condition,
                    isNot: true,
                    isOr: true);

            return this;
        }

        /// <summary>
        /// Conditionally adds an OR NOT condition to the WHERE clause.
        /// The condition is added only if the predicate evaluates to true.
        /// </summary>
        /// <param name="predicates">Determines whether the condition should be added.</param>
        /// <param name="condition">The SQL condition to add.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder OrNotIf(
            bool[] predicates,
            string condition)
            => OrNotIf(
                predicates?.All(p => p) ?? false,
                condition);
    }
}