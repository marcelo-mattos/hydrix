using System;
using System.Data;

namespace Hydrix.Attributes.Commands
{
    /// <summary>
    /// Command attribute to decorates a class that specifies how a command string is interpreted.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public abstract class SqlCommandAttribute :
        Attribute, Contract.ISqlCommandAttribute
    {
        /// <summary>
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </summary>
        public CommandType CommandType { get; private set; } = CommandType.StoredProcedure;

        /// <summary>
        /// Gets or sets the text command to run against the data source.
        /// </summary>
        public string CommandText { get; private set; } = string.Empty;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="commandText">Gets or sets the text command to run against the data source.</param>
        protected SqlCommandAttribute(
            CommandType commandType,
            string commandText)
        {
            this.CommandType = commandType;
            this.CommandText = commandText;
        }
    }
}