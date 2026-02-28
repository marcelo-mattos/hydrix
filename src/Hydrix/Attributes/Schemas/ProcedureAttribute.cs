using System;
using System.Data;

namespace Hydrix.Attributes.Schemas
{
    /// <summary>
    /// Represents a procedure object from a database and its mapping to System.Data.DataSet
    /// columns; and is implemented by .NET Framework data providers that access data sources.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ProcedureAttribute :
        Commands.CommandAttribute
    {
        /// <summary>
        /// The procedure schema.
        /// </summary>
        private string _schema = string.Empty;

        /// <summary>
        /// Gets or sets the procedure schema.
        /// </summary>
        public string Schema
        {
            get => _schema;
            set
            {
                _schema = value;
                CommandText = $"{_schema}.{Name}";
            }
        }

        /// <summary>
        /// Gets or sets the procedure name.
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Gets or sets the procedure name.</param>
        public ProcedureAttribute(
            string name) :
            base(CommandType.StoredProcedure,
                name)
        {
            this.Name = name;
        }
    }
}