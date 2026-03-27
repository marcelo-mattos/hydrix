using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hydrix.Attributes.Schemas
{
    /// <summary>
    /// Represents an entity table from a database and its mapping to System.Data.DataSet
    /// columns; and is implemented by .NET Framework data providers that access data sources.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class ForeignTableAttribute :
        TableAttribute
    {
        /// <summary>
        /// Initializes a new instance of the ForeignTableAttribute class with the specified table name.
        /// </summary>
        /// <param name="name">The name of the nested table to associate with the attribute. Cannot be null or empty.</param>
        public ForeignTableAttribute(string name) :
            base(name)
        { }

        /// <summary>
        /// Gets or sets the alias used to identify the entity in a user-friendly manner.
        /// </summary>
        /// <remarks>The alias can be used for display purposes in user interfaces or logs, providing a
        /// more recognizable name than the entity's identifier.</remarks>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the primary keys for the data entity, represented as an array of strings.
        /// </summary>
        /// <remarks>The primary keys are used to uniquely identify records within the data entity. Ensure
        /// that the array contains valid key values that correspond to the entity's schema.</remarks>
        public string[] PrimaryKeys { get; set; }

        /// <summary>
        /// Gets or sets the names of the foreign keys associated with the entity.
        /// </summary>
        /// <remarks>Use this property to specify or retrieve the foreign key names that define
        /// relationships between this entity and others in the database. Maintaining accurate foreign key names is
        /// important for ensuring referential integrity and for understanding entity relationships.</remarks>
        public string[] ForeignKeys { get; set; }
    }
}