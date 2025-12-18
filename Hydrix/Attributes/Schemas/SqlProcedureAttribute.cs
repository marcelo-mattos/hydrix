using System;
using System.Data;

namespace Hydrix.Attributes.Schemas
{
    /// <summary>
    /// Represents a procedure object from a database and its mapping to System.Data.DataSet
    /// columns; and is implemented by .NET Framework data providers that access data sources.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SqlProcedureAttribute :
        Commands.SqlCommandAttribute, Contract.ISqlProcedureAttribute
    {
        /// <summary>
        /// Gets or sets the procedure schema.
        /// </summary>
        public string Schema { get; private set; } = string.Empty;

        /// <summary>
        /// Gets or sets the procedure name.
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="schema">Gets or sets the procedure schema.</param>
        /// <param name="name">Gets or sets the procedure name.</param>
        public SqlProcedureAttribute(
            string schema,
            string name) :
            base(CommandType.StoredProcedure,
                $"{schema}.{name}")
        {
            this.Schema = schema;
            this.Name = name;
        }
    }
}