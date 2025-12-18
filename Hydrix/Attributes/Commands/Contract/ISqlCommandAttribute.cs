using System.Data;

namespace Hydrix.Attributes.Commands.Contract
{
    /// <summary>
    /// Command attribute to decarates a class that holds table fields or procedures parameters.
    /// </summary>
    internal interface ISqlCommandAttribute
    {
        /// <summary>
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </summary>
        CommandType CommandType { get; }

        /// <summary>
        /// Gets or sets the text command to run against the data source.
        /// </summary>
        string CommandText { get; }
    }
}