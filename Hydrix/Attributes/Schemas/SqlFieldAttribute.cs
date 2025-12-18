using System;

namespace Hydrix.Attributes.Schemas
{
    /// <summary>
    /// Represents an entity field from a database table and its mapping to System.Data.DataSet
    /// columns; and is implemented by .NET Framework data providers that access data sources.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SqlFieldAttribute :
        Base.SqlAttribute, Contract.ISqlFieldAttribute
    {
        /// <summary>
        /// Gets or sets the name of the table field.
        /// </summary>
        public string FieldName { get; private set; } = string.Empty;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SqlFieldAttribute() :
            this(string.Empty)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fieldName">Gets or sets the name of the table field.</param>
        public SqlFieldAttribute(
            string fieldName)
            => this.FieldName = fieldName;
    }
}