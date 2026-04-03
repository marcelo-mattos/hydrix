using System;
using System.Data;
using System.Reflection;

namespace Hydrix.Resolvers
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
        /// Gets the ordinal used by this binding.
        /// </summary>
        public int Ordinal { get; }

        /// <summary>
        /// Gets the provider CLR type captured when this binding was resolved.
        /// </summary>
        public Type SourceType { get; }

        /// <summary>
        /// Gets the reflected property metadata for the target property, used for
        /// building inlined expression trees in the fast-path row materializer.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Initializes a new instance of the ResolvedFieldBinding class with the specified field assignment action.
        /// </summary>
        /// <param name="assigner">An action that assigns a value to an object based on data from an IDataRecord. Cannot be null.</param>
        /// <param name="ordinal">The ordinal used by this binding, or -1 if not applicable.</param>
        /// <param name="sourceType">The provider CLR type captured when this binding was resolved or null if not applicable.</param>
        /// <param name="property">The reflected property metadata for inline expression building, or null if not available.</param>
        public ResolvedFieldBinding(
            Action<object, IDataRecord> assigner,
            int ordinal = -1,
            Type sourceType = null,
            PropertyInfo property = null)
        {
            Assigner = assigner;
            Ordinal = ordinal;
            SourceType = sourceType;
            Property = property;
        }
    }
}
