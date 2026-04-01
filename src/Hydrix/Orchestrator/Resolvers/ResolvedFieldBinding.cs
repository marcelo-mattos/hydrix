using System;
using System.Data;

namespace Hydrix.Orchestrator.Resolvers
{
    /// <summary>
    /// Represents a binding that assigns a value from an IDataRecord to a target object using a specified delegate.
    /// </summary>
    /// <remarks>The ResolvedFieldBinding struct encapsulates the logic required to map a value from a data
    /// record, such as a row returned from a database query, to a property or field of a target object. It is typically
    /// used in data materialization scenarios where efficient assignment of values from data sources to object members
    /// is required.</remarks>
    internal readonly struct ResolvedFieldBinding
    {
        /// <summary>
        /// Gets the delegate that assigns a value from an IDataRecord to a target object.
        /// </summary>
        /// <remarks>The assigner delegate is typically used to map data from a data record, such as a row
        /// returned from a database query, to a property or field of a target object. The first parameter is the target
        /// object to assign to, and the second parameter is the data record containing the source value.</remarks>
        public Action<object, IDataRecord> Assigner { get; }

        /// <summary>
        /// Initializes a new instance of the ResolvedFieldBinding class with the specified field assignment action.
        /// </summary>
        /// <param name="assigner">An action that assigns a value to an object based on data from an IDataRecord. Cannot be null.</param>
        public ResolvedFieldBinding(
            Action<object, IDataRecord> assigner)
        {
            Assigner = assigner;
        }
    }
}
