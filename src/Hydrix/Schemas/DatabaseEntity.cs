using FluentValidation;
using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Builders;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

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
        private void ValidateInternal(List<ValidationResult> results)
        {
            var properties = GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p =>
                    p.GetCustomAttribute<NotMappedAttribute>() is null &&
                    p.GetCustomAttribute<ForeignTableAttribute>() is null)
                .ToArray();

            foreach (var property in properties)
            {
                var value = property.GetValue(this);

                var context = new ValidationContext(this)
                {
                    MemberName = property.Name
                };

                Validator.TryValidateProperty(
                    value,
                    context,
                    results);
            }
        }

        /// <summary>
        /// Constructs the full table name by combining the specified schema and table name, separated by a period.
        /// </summary>
        /// <param name="schema">The schema name to prepend to the table name. If null or consists only of white-space characters, the schema
        /// is omitted.</param>
        /// <param name="table">The name of the table to which the schema is applied.</param>
        /// <returns>A string representing the full table name in the format 'schema.table'. If the schema is null or white
        /// space, returns just the table name.</returns>
        private static string GetFullTableName(
            string schema,
            string table)
        {
            if (string.IsNullOrWhiteSpace(schema))
                return table;

            return $"{schema}.{table}";
        }

        /// <summary>
        /// Resolves the schema name, primary key columns, and foreign key columns for a specified navigation property
        /// using the provided foreign table attributes.
        /// </summary>
        /// <remarks>If the schema is not specified in the attribute or table metadata, the method
        /// defaults to using "[dbo]" as the schema name. The method ensures that both primary and foreign keys are
        /// present and that their counts match, enforcing referential integrity for the navigation property.</remarks>
        /// <param name="navigationProperty">The navigation property representing the relationship for which foreign metadata is being resolved. Must not
        /// be null.</param>
        /// <param name="attr">The attribute containing schema, primary key, and foreign key information for the foreign table. Must not be
        /// null.</param>
        /// <returns>A tuple containing the resolved schema name, an array of primary key column names, and an array of foreign
        /// key column names associated with the navigation property.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the primary keys or foreign keys cannot be resolved, or if the number of primary keys does not
        /// match the number of foreign keys.</exception>
        private (string schema, string[] primaryKeys, string[] foreignKeys) ResolveForeignMetadata(
            PropertyInfo navigationProperty,
            ForeignTableAttribute attr)
        {
            var mainType = GetType();
            var foreignType = navigationProperty.PropertyType;

            var mainTable = mainType.GetCustomAttribute<TableAttribute>();
            var foreignTable = foreignType.GetCustomAttribute<TableAttribute>();

            var primaryKeys = attr.PrimaryKeys
                ?? foreignType.GetProperties()
                    .Where(p => p.GetCustomAttribute<KeyAttribute>() != null)
                    .Select(p => p.Name)
                    .ToArray();

            var foreignKeys = attr.ForeignKeys
                ?? mainType.GetProperties()
                    .Where(p =>
                        p.GetCustomAttribute<ForeignKeyAttribute>()?.Name ==
                        navigationProperty.Name)
                    .Select(p => p.Name)
                    .ToArray();

            if (primaryKeys.Length == 0)
                throw new InvalidOperationException(
                    $"Primary key not resolved for {foreignType.Name}");

            if (foreignKeys.Length == 0)
                throw new InvalidOperationException(
                    $"Foreign key not resolved for {mainType.Name}");

            if (primaryKeys.Length != foreignKeys.Length)
                throw new InvalidOperationException(
                    $"PrimaryKeys and ForeignKeys count mismatch.");

            return (attr.Schema, primaryKeys, foreignKeys);
        }

        /// <summary>
        /// Retrieves a collection of column names for the specified type, each prefixed with the provided alias,
        /// excluding properties that are not mapped to a database column or represent foreign tables.
        /// </summary>
        /// <remarks>This method filters out properties that are not mapped to a database column, ensuring
        /// that only relevant properties are included in the result.</remarks>
        /// <param name="type">The type whose public instance properties are examined to determine selectable columns. Only properties not
        /// marked with <see cref="NotMappedAttribute"/> or <see cref="ForeignTableAttribute"/> are included.</param>
        /// <param name="alias">The alias to prefix each column name in the returned collection. This is typically used to qualify column
        /// names in SQL queries.</param>
        /// <returns>An enumerable collection of strings representing the selectable column names, formatted as
        /// '[alias].[columnName]'.</returns>
        private static IEnumerable<string> GetSelectableColumns(
            Type type,
            string alias)
        {
            return type
                .GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public)
                .Where(p =>
                    p.GetCustomAttribute<NotMappedAttribute>() == null &&
                    p.GetCustomAttribute<ForeignTableAttribute>() == null)
                .Select(p =>
                {
                    var columnAttr = p.GetCustomAttribute<ColumnAttribute>();
                    var columnName = columnAttr?.Name ?? p.Name;

                    return $"\t{alias}.{columnName}";
                });
        }

        /// <summary>
        /// Retrieves the fully qualified column names for selectable properties of a foreign entity referenced by the
        /// specified navigation property.
        /// </summary>
        /// <remarks>Only properties of the foreign entity that are not marked with the NotMapped or
        /// ForeignTable attributes are included in the result. The column name is determined by the ColumnAttribute if
        /// present; otherwise, the property name is used.</remarks>
        /// <param name="navigationProperty">The navigation property that identifies the foreign entity from which to select columns. Must be a property
        /// of a type representing a database entity.</param>
        /// <param name="foreignAlias">The alias to use for the foreign table in the generated column names. This value is used as the prefix in
        /// the resulting column expressions.</param>
        /// <param name="foreignName">The name to use for the foreign table in the generated column names. This value is used as the alias in the
        /// resulting column expressions.</param>
        /// <returns>An enumerable collection of strings, each representing a fully qualified column name in the format
        /// 'foreignAlias.columnName AS foreignName.columnName' for each selectable property of the foreign entity.</returns>
        private static IEnumerable<string> GetForeignSelectableColumns(
            PropertyInfo navigationProperty,
            string foreignAlias,
            string foreignName)
        {
            var foreignType = navigationProperty.PropertyType;

            return foreignType
                .GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public)
                .Where(p =>
                    p.GetCustomAttribute<NotMappedAttribute>() == null &&
                    p.GetCustomAttribute<ForeignTableAttribute>() == null)
                .Select(p =>
                {
                    var columnAttr = p.GetCustomAttribute<ColumnAttribute>();
                    var columnName = columnAttr?.Name ?? p.Name;

                    return $"\t{foreignAlias}.{columnName} AS \"{foreignName}.{columnName}\"";
                });
        }

        /// <summary>
        /// Generates a SQL SELECT query string for the current entity type, including optional table aliases and left
        /// joins for related foreign tables.
        /// </summary>
        /// <remarks>The generated query reflects the table and relationship structure defined by
        /// attributes on the entity type. Foreign tables referenced by properties with the ForeignTableAttribute are
        /// automatically joined using LEFT JOIN clauses. Table and schema names are determined by the TableAttribute
        /// and ForeignTableAttribute, if present. This method is intended for use with entity types that follow the
        /// expected attribute-based schema conventions.</remarks>
        /// <param name="where">A SqlWhereBuilder instance that specifies the conditions to apply to the WHERE clause of the generated SQL
        /// query.</param>
        /// <param name="alias">An optional string to use as a table alias for the main entity in the generated SQL query. If not provided, no alias is used.</param>
        /// <returns>A string containing the complete SQL SELECT query, including any specified WHERE conditions and left joins
        /// for foreign tables.</returns>
        public string BuildQuery(
            string alias = "",
            SqlWhereBuilder where = null)
        {
            var type = GetType();
            var tableAttr = type.GetCustomAttribute<TableAttribute>();

            var mainTableName = GetFullTableName(
                tableAttr?.Schema,
                tableAttr?.Name ?? type.Name);

            var sql = new StringBuilder();

            string mainAlias = string.IsNullOrEmpty(alias)
                ? "t0"
                : alias;

            var columns = new List<string>();
            columns.AddRange(GetSelectableColumns(type, mainAlias));

            var joinIndex = 1;
            var foreignProps = type
                .GetProperties()
                .Where(p => p.GetCustomAttribute<ForeignTableAttribute>() != null);

            foreach (var prop in foreignProps)
            {
                var attr = prop.GetCustomAttribute<ForeignTableAttribute>();
                var (schema, primaryKeys, foreignKeys) = ResolveForeignMetadata(prop, attr);

                var foreignTableName = GetFullTableName(
                    schema,
                    attr.Name);

                var foreignAlias = string.IsNullOrEmpty(attr.Alias)
                    ? $"t{joinIndex++}"
                    : attr.Alias;

                columns.AddRange(
                    GetForeignSelectableColumns(
                        prop,
                        foreignAlias,
                        attr.Name));

                var isRequiredJoin = foreignKeys.All(fk =>
                {
                    var fkProp = type.GetProperty(fk);
                    return fkProp?.GetCustomAttribute<RequiredAttribute>() != null;
                });

                sql.AppendLine();
                sql.Append(isRequiredJoin ? "INNER JOIN " : "LEFT JOIN ");
                sql.Append(foreignTableName);
                sql.Append($" {foreignAlias}");

                sql.Append(" ON ");

                var conditions = new List<string>();

                for (int i = 0; i < primaryKeys.Length; i++)
                {
                    var left = $"{mainAlias}.{foreignKeys[i]}";
                    var right = $"{foreignAlias}.{primaryKeys[i]}";

                    conditions.Add($"{left} = {right}");
                }

                sql.Append(string.Join(" AND ", conditions));
            }

            var finalSql = new StringBuilder();

            finalSql.AppendLine("SELECT ");
            finalSql.AppendLine(string.Join($", {Environment.NewLine}", columns));
            finalSql.Append("FROM ");
            finalSql.Append(mainTableName);
            finalSql.Append($" {mainAlias}");
            finalSql.Append(sql);

            var whereConditions = where?.Build();
            if (!string.IsNullOrWhiteSpace(whereConditions))
            {
                finalSql.AppendLine();
                finalSql.Append(whereConditions);
            }

            return finalSql.ToString();
        }
    }
}
