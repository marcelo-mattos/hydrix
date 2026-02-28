using FluentValidation;
using Hydrix.Orchestrator.Builders.Query;
using Hydrix.Orchestrator.Builders.Query.Conditions;
using Hydrix.Orchestrator.Caching;
using Hydrix.Schemas.Contract;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hydrix.Schemas
{
    /// <summary>
    /// Represents the abstract base class for all database entities, providing a common contract for database-related
    /// objects.
    /// </summary>
    /// <remarks>Classes that derive from DatabaseEntity must implement the IEntity interface to define
    /// specific behaviors and properties required for database entities.</remarks>
    public abstract class DatabaseEntity :
        IEntity
    {
        /// <summary>
        /// Validates the current object and returns a value that indicates whether the object is in a valid state.
        /// </summary>
        /// <remarks>Use this method to determine whether the object's state meets all defined validation
        /// criteria. The method populates the provided list with detailed validation errors, if any are
        /// found.</remarks>
        /// <param name="results">When this method returns, contains a list of <see cref="ValidationResult"/> objects that describe any
        /// validation errors encountered. The list is empty if the object is valid.</param>
        /// <param name="fluentValidator">An FluentValidation validator to use for additional validation logic.</param>
        /// <returns>true if the object passes all validation checks; otherwise, false.</returns>
        public virtual bool IsValid<T>(
            out List<ValidationResult> results,
            AbstractValidator<T> fluentValidator)
            where T : IEntity
        {
            IsValid(out results);

            if (fluentValidator != null && this is T entity)
            {
                var fluentResult = fluentValidator.Validate(entity);

                foreach (var error in fluentResult.Errors)
                {
                    results.Add(new ValidationResult(
                        error.ErrorMessage,
                        new string[] { error.PropertyName }));
                }
            }
            return results.Count == 0;
        }

        /// <summary>
        /// Validates the current object and returns a value that indicates whether the object is in a valid state.
        /// </summary>
        /// <remarks>Use this method to determine whether the object's state meets all defined validation
        /// criteria. The method populates the provided list with detailed validation errors, if any are
        /// found.</remarks>
        /// <param name="results">When this method returns, contains a list of <see cref="ValidationResult"/> objects that describe any
        /// validation errors encountered. The list is empty if the object is valid.</param>
        /// <returns>true if the object passes all validation checks; otherwise, false.</returns>
        public virtual bool IsValid(
            out List<ValidationResult> results)
        {
            results = new List<ValidationResult>();
            ValidateInternal(results);
            return results.Count == 0;
        }

        /// <summary>
        /// Validates the public instance properties of the current object and adds any validation errors to the
        /// specified results list.
        /// </summary>
        /// <remarks>This method examines all public instance properties of the current object, excluding
        /// those marked with the NotMappedAttribute, and validates their values using any associated validation
        /// attributes. Use this method to collect property-level validation errors when implementing custom validation
        /// logic.</remarks>
        /// <param name="results">A list that receives the validation results for each property that fails validation. Must not be null.</param>
        private void ValidateInternal(
            List<ValidationResult> results)
        {
            var metadata = EntityBuilderMetadataCache.GetMetadata(GetType());

            foreach (var column in metadata.Columns)
            {
                var value = column.Getter(this);

                var context = new ValidationContext(this)
                {
                    MemberName = column.PropertyName
                };

                Validator.TryValidateProperty(
                    value,
                    context,
                    results);
            }
        }

        /// <summary>
        /// Generates a SQL SELECT query string for the current entity type, including optional table aliases and left
        /// joins for related foreign tables.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity for which the SQL query is being generated. Must implement <see cref="ITable"/>.</typeparam>
        /// <remarks>The generated query reflects the table and relationship structure defined by
        /// attributes on the entity type. Foreign tables referenced by properties with the ForeignTableAttribute are
        /// automatically joined using LEFT JOIN clauses. Table and schema names are determined by the TableAttribute
        /// and ForeignTableAttribute, if present. This method is intended for use with entity types that follow the
        /// expected attribute-based schema conventions.</remarks>
        /// <param name="where">A WhereBuilder instance that specifies the conditions to apply to the WHERE clause of the generated SQL
        /// query.</param>
        /// <returns>A string containing the complete SQL SELECT query, including any specified WHERE conditions and left joins
        /// for foreign tables.</returns>
        public static string BuildQuery<TEntity>(
            WhereBuilder where = null)
            where TEntity : ITable
            => QueryBuilder.Build<TEntity>(where);
    }
}