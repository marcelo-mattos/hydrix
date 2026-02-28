using System;
using System.Data;

namespace Hydrix.Orchestrator.Binders.Procedure
{
    /// <summary>
    /// Represents the binding configuration for a parameter used in a stored procedure, including its name, direction,
    /// data type, size, and a delegate for retrieving its value from a context object.
    /// </summary>
    /// <remarks>Use this class to define how individual parameters are mapped and supplied when executing a
    /// stored procedure. The getter delegate enables dynamic retrieval of parameter values from a provided context,
    /// supporting flexible data binding scenarios. This type is typically used internally by procedure execution
    /// frameworks to manage parameter metadata and value extraction.</remarks>
    internal class ProcedureParameterBinding
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the direction of the parameter, indicating whether it is an input, output, or input/output parameter.
        /// </summary>
        /// <remarks>This property is useful for determining how the parameter is used in a method call,
        /// particularly in database operations or API requests where parameter direction affects behavior.</remarks>
        public ParameterDirection Direction { get; }

        /// <summary>
        /// Gets the database type associated with the current instance.
        /// </summary>
        /// <remarks>This property returns a nullable <see cref="DbType"/> value that indicates the type
        /// of the database field. It is useful for determining how to handle data interactions with the
        /// database.</remarks>
        public int DbType { get; }

        /// <summary>
        /// Gets a function that retrieves the value of a specified property from an object.
        /// </summary>
        /// <remarks>The getter function takes an object as input and returns the corresponding property
        /// value. This is useful for dynamically accessing properties in a generic manner.</remarks>
        public Func<object, object> Getter { get; }

        /// <summary>
        /// Initializes a new instance of the ProcedureParameterBinding class, representing a binding for a stored
        /// procedure parameter with the specified characteristics and value accessor.
        /// </summary>
        /// <param name="name">The name of the parameter as it appears in the stored procedure. Cannot be null.</param>
        /// <param name="direction">The direction of the parameter, indicating whether it is used for input, output, or both.</param>
        /// <param name="dbType">The database type of the parameter. Specify null to use the default type mapping.</param>
        /// <param name="getter">A function that retrieves the value of the parameter from a source object at runtime.</param>
        public ProcedureParameterBinding(
            string name,
            ParameterDirection direction,
            int dbType,
            Func<object, object> getter)
        {
            Name = name;
            Direction = direction;
            Getter = getter;
            DbType = dbType;
        }
    }
}