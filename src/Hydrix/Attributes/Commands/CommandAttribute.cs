using System;
using System.Data;

namespace Hydrix.Attributes.Commands
{
    /// <summary>
    /// Command attribute to decorates a class that specifies how a command string is interpreted.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class CommandAttribute :
        Attribute
    {
        /// <summary>
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </summary>
        public CommandType CommandType { get; private set; }

        /// <summary>
        /// Gets or sets the text command to run against the data source.
        /// </summary>
        public string CommandText { get; protected set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="commandType">Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.</param>
        /// <param name="commandText">Gets or sets the text command to run against the data source.</param>
        protected CommandAttribute(
            CommandType commandType,
            string commandText)
        {
            CommandType = commandType;
            CommandText = commandText;
        }
    }
}