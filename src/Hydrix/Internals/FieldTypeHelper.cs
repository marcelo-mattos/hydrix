using System;
using System.Data;

namespace Hydrix.Internals
{
    /// <summary>
    /// Provides helpers for safely retrieving provider field-type metadata from data records.
    /// </summary>
    /// <remarks>This class centralizes the guarded <see cref="IDataRecord.GetFieldType(int)"/> access used by
    /// Hydrix internals so unsupported providers can be handled consistently without repeating the same exception
    /// handling logic across multiple hot-path components.</remarks>
    internal static class FieldTypeHelper
    {
        /// <summary>
        /// Retrieves the provider CLR type for the specified ordinal when the record supports it.
        /// </summary>
        /// <remarks>If the underlying provider does not expose field-type metadata for the requested ordinal,
        /// this method returns <see langword="null"/> instead of propagating <see cref="InvalidOperationException"/>
        /// or <see cref="NotSupportedException"/>.</remarks>
        /// <param name="record">The data record from which to retrieve field-type metadata. Cannot be null.</param>
        /// <param name="ordinal">The zero-based field ordinal whose provider CLR type should be retrieved.</param>
        /// <returns>The provider CLR type for the requested ordinal, or <see langword="null"/> when the record does not support the metadata lookup.</returns>
        internal static Type GetFieldType(
            IDataRecord record,
            int ordinal)
        {
            try
            {
                return record.GetFieldType(ordinal);
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
