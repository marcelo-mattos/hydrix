using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hydrix.Attributes.Schemas
{
    /// <summary>
    /// Represents an entity table from a database and its mapping to System.Data.DataSet
    /// columns; and is implemented by .NET Framework data providers that access data sources.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class NestedTableAttribute :
        TableAttribute, Contract.ITableAttribute
    {
        /// <summary>
        /// Initializes a new instance of the NestedTableAttribute class with the specified table name.
        /// </summary>
        /// <param name="name">The name of the nested table to associate with the attribute. Cannot be null or empty.</param>
        public NestedTableAttribute(string name) :
            base(name)
        { }

        /// <summary>
        /// Gets or sets the entity name.
        /// </summary>
        public string Key { get; set; } = string.Empty;
    }
}