using System.Data;

namespace Hydrix.Attributes.Parameters.Contract
{
    /// <summary>
    /// Represents a parameter to a Command object, and optionally, its mapping to System.Data.DataSet
    /// columns; and is implemented by .NET Framework data providers that access data sources.
    /// </summary>
    public interface ISqlParameterAttribute
    {
        /// <summary>
        /// Gets or sets the name of the System.Data.IDataParameter.
        /// </summary>
        string ParameterName { get; }

        /// <summary>
        /// Gets or sets the System.Data.DbType of the parameter.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">The property was not set to a valid System.Data.DbType.</exception>
        DbType DbType { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is input-only, output-only,
        /// bidirectional, or a stored procedure return value parameter.
        /// </summary>
        /// <exception cref="System.ArgumentException">The property was not set to one of the valid System.Data.ParameterDirection values.</exception>
        ParameterDirection Direction { get; }
    }
}