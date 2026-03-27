using System;
using System.Data;

namespace Hydrix.Attributes.Parameters
{
    /// <summary>
    /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet
    /// columns; and is implemented by .NET Framework data providers that access data sources.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ParameterAttribute :
        Attribute
    {
        /// <summary>
        /// Gets or sets the name of the System.Data.IDataParameter.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the System.Data.DbType of the parameter.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">The property was not set to a valid System.Data.DbType.</exception>
        public DbType DbType { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is input-only, output-only,
        /// bidirectional, or a stored procedure return value parameter.
        /// </summary>
        /// <exception cref="System.ArgumentException">The property was not set to one of the valid System.Data.ParameterDirection values.</exception>
        public ParameterDirection Direction { get; set; } = ParameterDirection.Input;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parameterName">Gets or sets the name of the System.Data.IDataParameter.</param>
        /// <param name="dbType">Gets or sets the System.Data.DbType of the parameter.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">The property was not set to a valid System.Data.DbType.</exception>
        public ParameterAttribute(
            string parameterName,
            DbType dbType)
        {
            Name = parameterName;
            DbType = dbType;
        }
    }
}