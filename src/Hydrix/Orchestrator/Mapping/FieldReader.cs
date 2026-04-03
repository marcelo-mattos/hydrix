using System.Data;

namespace Hydrix.Orchestrator.Mapping
{
    /// <summary>
    /// Delegate that defines a method signature for reading a field value from an IDataRecord given its ordinal position.
    /// </summary>
    /// <param name="record">The IDataRecord from which to read the field value.</param>
    /// <param name="ordinal">The zero-based ordinal position of the field to read.</param>
    /// <returns>The value of the field.</returns>
    internal delegate object FieldReader(
        IDataRecord record,
        int ordinal);
}
