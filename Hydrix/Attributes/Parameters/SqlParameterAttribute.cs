using System;
using System.Data;

namespace Hydrix.Attributes.Parameters
{
    /// <summary>
    /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet
    /// columns; and is implemented by .NET Framework data providers that access data sources.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SqlParameterAttribute :
        Attribute, Contract.ISqlParameterAttribute
    {
        /// <summary>
        /// Gets or sets the name of the System.Data.IDataParameter.
        /// </summary>
        public string ParameterName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets or sets the System.Data.DbType of the parameter.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">The property was not set to a valid System.Data.DbType.</exception>
        public DbType DbType { get; private set; } = DbType.String;

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is input-only, output-only,
        /// bidirectional, or a stored procedure return value parameter.
        /// </summary>
        /// <exception cref="System.ArgumentException">The property was not set to one of the valid System.Data.ParameterDirection values.</exception>
        public ParameterDirection Direction { get; private set; } = ParameterDirection.Input;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parameterName">Gets or sets the name of the System.Data.IDataParameter.</param>
        /// <param name="dbType">Gets or sets the System.Data.DbType of the parameter.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">The property was not set to a valid System.Data.DbType.</exception>
        public SqlParameterAttribute(
            string parameterName,
            DbType dbType) :
            this(
                parameterName,
                dbType,
                ParameterDirection.Input)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parameterName">Gets or sets the name of the System.Data.IDataParameter.</param>
        /// <param name="dbType">Gets or sets the System.Data.DbType of the parameter.</param>
        /// <param name="direction">Gets or sets a value indicating whether the parameter is input-only, output-only, bidirectional, or a stored procedure return value parameter.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">The property was not set to a valid System.Data.DbType.</exception>
        public SqlParameterAttribute(
            string parameterName,
            DbType dbType,
            ParameterDirection direction)
        {
            this.ParameterName = parameterName;
            this.DbType = dbType;
            this.Direction = direction;
        }
    }
}