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
        /// Initializes a new instance of the ResolvedTableBindings class with the specified field and entity bindings.
        /// </summary>
        /// <param name="fields">An array of ResolvedFieldBinding objects representing the field bindings to include. If null, an empty array
        /// is used.</param>
        /// <param name="entities">An array of ResolvedNestedBinding objects representing the nested entity bindings to include. If null, an
        /// empty array is used.</param>
        /// <param name="columnNames">An array of column names representing the schema layout captured at the time of binding resolution.
        /// This is used for hot-path schema matching to quickly determine if a cached binding plan can be reused. If null, an empty array is used.</param>
        public ResolvedTableBindings(
            ResolvedFieldBinding[] fields,
            ResolvedNestedBinding[] entities,
            string[] columnNames = null)
        {
            Fields = fields ?? Array.Empty<ResolvedFieldBinding>();
            Entities = entities ?? Array.Empty<ResolvedNestedBinding>();
            ColumnNames = columnNames ?? Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the provided reader still matches the cached schema layout and mapped provider types.
        /// </summary>
        /// <param name="reader">The data reader to compare against the cached schema and provider types.</param>
        /// <returns>true if the reader matches the cached schema and provider types; otherwise, false.</returns>
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
        /// Validates that mapped provider types remain compatible with the cached binding plan.
        /// </summary>
        /// <param name="reader">The data reader to compare against the cached provider types.</param>
        /// <returns>true if the provider types match the cached binding plan; otherwise, false.</returns>
        private bool MatchesFieldTypes(
            IDataReader reader)
        {
            for (var index = 0; index < Fields.Length; index++)
            {
                var field = Fields[index];
                if (field.Ordinal < 0)
                {
                    continue;
                }

                if (field.SourceType == null)
                {
                    return false;
                }

                var currentFieldType = GetFieldType(
                    reader,
                    field.Ordinal);
                if (currentFieldType != null &&
                    currentFieldType != field.SourceType)
                {
                    return false;
                }

                if (!reader.IsDBNull(field.Ordinal) &&
                    reader.GetValue(field.Ordinal)?.GetType() != field.SourceType)
                {
                    return false;
                }
            }

            for (var index = 0; index < Entities.Length; index++)
            {
                if (!Entities[index]
                    .Bindings
                    .MatchesFieldTypes(reader))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Retrieves the provider CLR type for the specified ordinal when the reader supports it.
        /// </summary>
        /// <param name="reader">The data reader to inspect.</param>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The CLR type of the field if available; otherwise, null.</returns>
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
