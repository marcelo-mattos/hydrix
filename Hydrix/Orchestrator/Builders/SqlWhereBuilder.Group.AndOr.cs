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
        /// Adds a grouped OR expression to the WHERE clause, prefixed with AND.
        /// Each condition is treated as a complete SQL expression.
        ///
        /// Example:
        /// AND (condition1 OR condition2 OR condition3)
        /// </summary>
        /// <param name="conditions">SQL expressions to be combined using OR.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder AndOrGroup(
            params string[] conditions)
            => AddAndOrGroup(
                false,
                conditions);

        /// <summary>
        /// Adds a grouped OR expression to the WHERE clause, prefixed with AND NOT.
        /// Each condition is treated as a complete SQL expression.
        ///
        /// Example:
        /// AND (condition1 OR condition2 OR condition3)
        /// </summary>
        /// <param name="conditions">SQL expressions to be combined using OR.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder AndOrNotGroup(
            params string[] conditions)
            => AddAndOrGroup(
                true,
                conditions);

        /// <summary>
        /// Conditionally adds a grouped OR expression to the WHERE clause, prefixed with AND.
        /// Each condition is treated as a complete SQL expression.
        ///
        /// Example:
        /// AND (condition1 OR condition2 OR condition3)
        /// </summary>
        /// <param name="predicate">Determines whether the condition should be added.</param>
        /// <param name="conditions">SQL expressions to be combined using OR.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder AndOrGroupIf(
            bool predicate,
            params string[] conditions)
        {
            if (predicate)
                AndOrGroup(conditions);

            return this;
        }

        /// <summary>
        /// Conditionally adds a grouped OR expression to the WHERE clause, prefixed with AND NOT.
        /// Each condition is treated as a complete SQL expression.
        ///
        /// Example:
        /// AND (condition1 OR condition2 OR condition3)
        /// </summary>
        /// <param name="predicate">Determines whether the condition should be added.</param>
        /// <param name="conditions">SQL expressions to be combined using OR.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder AndOrNotGroupIf(
            bool predicate,
            params string[] conditions)
        {
            if (predicate)
                AndOrNotGroup(conditions);

            return this;
        }
    }
}