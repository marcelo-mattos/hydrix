using System;

namespace Hydrix.Attributes.Schemas
{
    /// <summary>
    /// Represents an entity table from a database and its mapping to System.Data.DataSet
    /// columns; and is implemented by .NET Framework data providers that access data sources.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SqlEntityAttribute :
        Base.SqlAttribute, Contract.ISqlEntityAttribute
    {
        /// <summary>
        /// Gets or sets the entity schema.
        /// </summary>
        public string Schema { get; private set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entity name.
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entity name.
        /// </summary>
        public string PrimaryKey { get; private set; } = string.Empty;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SqlEntityAttribute() :
            this(
                string.Empty,
                string.Empty,
                string.Empty)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="schema">Gets or sets the entity schema.</param>
        /// <param name="name">Gets or sets the entity name.</param>
        /// <param name="primaryKey">Gets or sets the entity primary key.</param>
        public SqlEntityAttribute(
            string schema,
            string name,
            string primaryKey)
        {
            this.Schema = schema;
            this.Name = name;
            this.PrimaryKey = primaryKey;
        }
    }
}