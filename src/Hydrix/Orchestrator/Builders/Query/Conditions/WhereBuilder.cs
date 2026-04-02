using System.Collections.Generic;

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
    public sealed partial class WhereBuilder
    {
        /// <summary>
        /// Tokens that make up the WHERE clause.
        /// </summary>
        private readonly List<string> _tokens = new List<string>();

        /// <summary>
        /// Constructor is private to enforce the use of the Create method.
        /// </summary>
        private WhereBuilder()
        { }

        /// <summary>
        /// Creates a new instance of <see cref="WhereBuilder"/>.
        /// </summary>
        /// <returns>A new <see cref="WhereBuilder"/> instance.</returns>
        public static WhereBuilder Create()
            => new WhereBuilder();

        /// <summary>
        /// Clears all SQL conditions from the builder.
        /// </summary>
        /// <returns>A new <see cref="WhereBuilder"/> instance.</returns>
        public WhereBuilder Clear()
        {
            _tokens.Clear();
            return this;
        }

        /// <summary>
        /// Builds the complete SQL WHERE clause.
        /// If no conditions were added, an empty string is returned.
        /// </summary>
        /// <returns>
        /// A SQL WHERE clause starting with the keyword WHERE,
        /// or an empty string if no conditions exist.
        /// </returns>
        public string Build()
        {
            var sql = BuildInternal();
            return string.IsNullOrEmpty(sql)
                ? string.Empty
                : $"WHERE {sql}";
        }
    }
}
