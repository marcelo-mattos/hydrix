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
        /// Adds a grouped AND expression to the WHERE clause, prefixed with OR.
        /// Each condition is treated as a complete SQL expression.
        ///
        /// Example:
        /// OR (condition1 AND condition2 AND condition3)
        /// </summary>
        /// <param name="conditions">SQL expressions to be combined using OR.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder OrAndGroup(
            params string[] conditions)
            => AddOrAndGroup(
                false,
                conditions);

        /// <summary>
        /// Adds a grouped AND expression to the WHERE clause, prefixed with NOT OR.
        /// Each condition is treated as a complete SQL expression.
        ///
        /// Example:
        /// NOT OR (condition1 AND condition2 AND condition3)
        /// </summary>
        /// <param name="conditions">SQL expressions to be combined using OR.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder OrAndNotGroup(
            params string[] conditions)
            => AddOrAndGroup(
                true,
                conditions);

        /// <summary>
        /// Conditionally adds a grouped AND expression to the WHERE clause, prefixed with OR.
        /// Each condition is treated as a complete SQL expression.
        ///
        /// Example:
        /// OR (condition1 AND condition2 AND condition3)
        /// </summary>
        /// <param name="predicate">Determines whether the condition should be added.</param>
        /// <param name="conditions">SQL expressions to be combined using OR.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder OrAndGroupIf(
            bool predicate,
            params string[] conditions)
        {
            if (predicate)
                OrAndGroup(conditions);

            return this;
        }

        /// <summary>
        /// Conditionally adds a grouped AND expression to the WHERE clause, prefixed with OR.
        /// Each condition is treated as a complete SQL expression.
        ///
        /// Example:
        /// OR (condition1 AND condition2 AND condition3)
        /// </summary>
        /// <param name="predicates">Determines whether the condition should be added.</param>
        /// <param name="conditions">SQL expressions to be combined using OR.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder OrAndGroupIf(
            bool[] predicates,
            params string[] conditions)
            => OrAndGroupIf(
                predicates?.All(p => p) ?? false,
                conditions);

        /// <summary>
        /// Conditionally adds a grouped AND expression to the WHERE clause, prefixed with NOT OR.
        /// Each condition is treated as a complete SQL expression.
        ///
        /// Example:
        /// NOT OR (condition1 AND condition2 AND condition3)
        /// </summary>
        /// <param name="predicate">Determines whether the condition should be added.</param>
        /// <param name="conditions">SQL expressions to be combined using OR.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder OrAndNotGroupIf(
            bool predicate,
            params string[] conditions)
        {
            if (predicate)
                OrAndNotGroup(conditions);

            return this;
        }

        /// <summary>
        /// Conditionally adds a grouped AND expression to the WHERE clause, prefixed with NOT OR.
        /// Each condition is treated as a complete SQL expression.
        ///
        /// Example:
        /// NOT OR (condition1 AND condition2 AND condition3)
        /// </summary>
        /// <param name="predicates">Determines whether the condition should be added.</param>
        /// <param name="conditions">SQL expressions to be combined using OR.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder OrAndNotGroupIf(
            bool[] predicates,
            params string[] conditions)
            => OrAndNotGroupIf(
                predicates?.All(p => p) ?? false,
                conditions);
    }
}