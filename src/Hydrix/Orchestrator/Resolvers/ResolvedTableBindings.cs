using System;
using System.Data;

namespace Hydrix.Orchestrator.Resolvers
{
    /// <summary>
    /// Represents a set of resolved field and nested entity bindings for a table, providing access to the associated
    /// field and entity binding collections.
    /// </summary>
    internal sealed class ResolvedTableBindings
    {
        /// <summary>
        /// Gets the collection of resolved field bindings associated with the current instance.
        /// </summary>
        public ResolvedFieldBinding[] Fields { get; }

        /// <summary>
        /// Gets the collection of resolved nested bindings associated with this instance.
        /// </summary>
        public ResolvedNestedBinding[] Entities { get; }

        /// <summary>
        /// Gets the captured schema column names used for hot-path schema matching.
        /// </summary>
        public string[] ColumnNames { get; }

        /// <summary>
        /// Gets the flattened field bindings used by schema matching to avoid recursive traversal on the hot path.
        /// </summary>
        private ResolvedFieldBinding[] MatchFields { get; }

        /// <summary>
        /// Initializes a new instance of the ResolvedTableBindings class with the specified field and entity bindings.
        /// </summary>
        /// <param name="fields">An array of ResolvedFieldBinding objects representing the field bindings to include. If null, an empty array
        /// is used.</param>
        /// <param name="entities">An array of ResolvedNestedBinding objects representing the nested entity bindings to include. If null, an
        /// empty array is used.</param>
        /// <param name="columnNames">The captured schema column names used for hot-path schema matching.</param>
        public ResolvedTableBindings(
            ResolvedFieldBinding[] fields,
            ResolvedNestedBinding[] entities,
            string[] columnNames = null)
        {
            Fields = fields ?? Array.Empty<ResolvedFieldBinding>();
            Entities = entities ?? Array.Empty<ResolvedNestedBinding>();
            ColumnNames = columnNames ?? Array.Empty<string>();
            MatchFields = BuildMatchFields(
                Fields,
                Entities);
        }

        /// <summary>
        /// Determines whether the specified data reader matches the expected column names and field types.
        /// </summary>
        /// <remarks>Column name comparisons are case-insensitive. The method returns false if the number
        /// of columns does not match or if any column name differs from the expected value.</remarks>
        /// <param name="reader">The data reader to compare against the expected column schema. Cannot be null.</param>
        /// <returns>true if the data reader's columns match the expected names and field types; otherwise, false.</returns>
        internal bool Matches(
            IDataReader reader)
        {
            if (reader == null ||
                ColumnNames.Length == 0 ||
                reader.FieldCount != ColumnNames.Length)
            {
                return false;
            }

            for (var index = 0; index < ColumnNames.Length; index++)
            {
                var currentName = reader.GetName(index) ?? string.Empty;
                if (!string.Equals(
                    currentName,
                    ColumnNames[index],
                    StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return MatchesFieldTypes(reader);
        }

        /// <summary>
        /// Determines whether the field types in the specified data reader match the expected types defined by the
        /// current bindings.
        /// </summary>
        /// <remarks>This method checks both the type information and the actual runtime values of the
        /// fields to ensure type consistency.</remarks>
        /// <param name="reader">The data reader to compare against the expected field types. Must not be null.</param>
        /// <returns>true if all fields in the data reader match the expected types; otherwise, false.</returns>
        private bool MatchesFieldTypes(
            IDataReader reader)
        {
            for (var index = 0; index < MatchFields.Length; index++)
            {
                if (!FieldMatches(
                    reader,
                    MatchFields[index]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Builds the flattened field bindings used by schema matching.
        /// </summary>
        private static ResolvedFieldBinding[] BuildMatchFields(
            ResolvedFieldBinding[] fields,
            ResolvedNestedBinding[] entities)
        {
            if (fields.Length == 0)
            {
                if (entities.Length == 0)
                    return Array.Empty<ResolvedFieldBinding>();

                if (entities.Length == 1)
                    return entities[0].Bindings.MatchFields;
            }
            else if (entities.Length == 0)
            {
                return fields;
            }

            var totalCount = fields.Length;
            for (var index = 0; index < entities.Length; index++)
                totalCount += entities[index].Bindings.MatchFields.Length;

            if (totalCount == 0)
                return Array.Empty<ResolvedFieldBinding>();

            var matchFields = new ResolvedFieldBinding[totalCount];
            var offset = 0;

            if (fields.Length != 0)
            {
                Array.Copy(
                    fields,
                    matchFields,
                    fields.Length);
                offset = fields.Length;
            }

            for (var index = 0; index < entities.Length; index++)
            {
                var nestedMatchFields = entities[index].Bindings.MatchFields;
                if (nestedMatchFields.Length == 0)
                    continue;

                Array.Copy(
                    nestedMatchFields,
                    0,
                    matchFields,
                    offset,
                    nestedMatchFields.Length);
                offset += nestedMatchFields.Length;
            }

            return matchFields;
        }

        /// <summary>
        /// Checks if a single field matches the expected type.
        /// </summary>
        /// <param name="reader">The data reader to compare against the expected field type.</param>
        /// <param name="field">The field binding to check.</param>
        /// <returns>true if the field matches; otherwise, false.</returns>
        private static bool FieldMatches(
            IDataReader reader,
            ResolvedFieldBinding field)
        {
            if (field.Ordinal < 0)
                return true;

            if (field.SourceType == null)
                return false;

            var currentFieldType = GetFieldType(reader, field.Ordinal);

            if (currentFieldType != null)
                return currentFieldType == field.SourceType;

            if (!reader.IsDBNull(field.Ordinal) &&
                reader.GetValue(field.Ordinal)?.GetType() != field.SourceType)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieves the data type of the specified column in the provided data reader.
        /// </summary>
        /// <remarks>If the data reader does not support retrieving the field type or if the operation is
        /// invalid for the current state of the reader, the method returns null instead of throwing an
        /// exception.</remarks>
        /// <param name="reader">The data reader from which to obtain the column type information. Cannot be null.</param>
        /// <param name="ordinal">The zero-based column ordinal indicating which column's data type to retrieve. Must be within the range of
        /// available columns in the reader.</param>
        /// <returns>A Type object representing the data type of the specified column, or null if the type cannot be determined.</returns>
        private static Type GetFieldType(
            IDataReader reader,
            int ordinal)
        {
            try
            {
                return reader.GetFieldType(ordinal);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }
    }
}
