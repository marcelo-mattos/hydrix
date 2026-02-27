using System;

namespace Hydrix.Orchestrator.Mapping
{
    /// <summary>
    /// Represents the mapping between a CLR property and a SQL field, encapsulating metadata and value conversion
    /// information for use during entity metadata construction.
    /// </summary>
    /// <remarks>ColumnMap is used internally to facilitate the association of entity properties with their
    /// corresponding SQL fields. It provides access to mapping attributes, type normalization for value conversion,
    /// default value handling, and a compiled setter for efficient property assignment. This class is intended for
    /// internal metadata infrastructure and is not designed for direct use in application code.</remarks>
    internal sealed class ColumnMap
    {
        /// <summary>
        /// Gets the SQL field name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the field reader associated with this instance, which provides access to the underlying data fields.
        /// </summary>
        public FieldReader Reader { get; }

        /// <summary>
        /// Gets the compiled setter delegate used to assign values
        /// to the entity property without reflection.
        /// </summary>
        public Action<object, object> Setter { get; }

        /// <summary>
        /// Initializes a new instance of the SqlFieldMap class using the specified property, SQL field attribute, and
        /// target type.
        /// </summary>
        /// <param name="name">The name of the SQL field to which the property is mapped. This should be a valid column name in the database.</param>
        /// <param name="setter">The compiled setter delegate used to assign values to the entity property without reflection.</param>
        /// <param name="reader">The field reader delegate used to read values from the data record.</param>
        public ColumnMap(
            string name,
            Action<object, object> setter,
            FieldReader reader)
        {
            Name = name;
            Setter = setter;
            Reader = reader;
        }
    }
}