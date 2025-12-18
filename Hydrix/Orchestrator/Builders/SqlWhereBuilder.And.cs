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
        /// Adds the first condition or an AND condition to the WHERE clause.
        /// If this is the first condition, no logical operator is prepended.
        /// </summary>
        /// <param name="condition">The SQL condition to add.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder Where(
            string condition)
            => Add(condition);

        /// <summary>
        /// Adds an AND condition to the WHERE clause.
        /// </summary>
        /// <param name="condition">The SQL condition to add.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder And(
            string condition)
            => Add(condition);

        /// <summary>
        /// Adds an AND NOT condition to the WHERE clause.
        /// </summary>
        /// <param name="condition">The SQL condition to add.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder AndNot(
            string condition)
            => Add(condition,
                   isNot: true);

        /// <summary>
        /// Conditionally adds an AND condition to the WHERE clause.
        /// The condition is added only if the predicate evaluates to true.
        /// </summary>
        /// <param name="predicate">Determines whether the condition should be added.</param>
        /// <param name="condition">The SQL condition to add.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder AndIf(
            bool predicate,
            string condition)
        {
            if (predicate)
                Add(condition);

            return this;
        }

        /// <summary>
        /// Conditionally adds an AND NOT condition to the WHERE clause.
        /// The condition is added only if the predicate evaluates to true.
        /// </summary>
        /// <param name="predicate">Determines whether the condition should be added.</param>
        /// <param name="condition">The SQL condition to add.</param>
        /// <returns>The current <see cref="SqlWhereBuilder"/> instance.</returns>
        public SqlWhereBuilder AndNotIf(
            bool predicate,
            string condition)
        {
            if (predicate)
                Add(condition,
                    isNot: true);

            return this;
        }
    }
}