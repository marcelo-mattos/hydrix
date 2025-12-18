using System;
using System.Reflection;
using Hydrix.Attributes.Schemas;
using Hydrix.Schemas;

namespace Hydrix.Orchestrator.Mappers
{
    /// <summary>
    /// Represents the mapping definition of a scalar field between a SQL result set and a CLR
    /// entity property.
    /// </summary>
    /// <remarks>
    /// This class encapsulates all metadata required to safely and efficiently assign a column
    /// value from a <see cref="System.Data.DataRow"/> to a property of an <see cref="ISqlEntity"/>.
    ///
    /// Each instance of <see cref="SqlFieldMap"/> is created during metadata discovery and cached
    /// for reuse, avoiding repeated reflection operations during high-volume data mapping.
    ///
    /// The mapping is driven by the presence of the <see cref="SqlFieldAttribute"/>, which defines
    /// how a database column is associated with a specific entity property.
    /// </remarks>
    internal sealed class SqlFieldMap
    {
        /// <summary>
        /// Gets the reflected property associated with the SQL field mapping.
        /// </summary>
        /// <remarks>
        /// This property represents the writable CLR property on the entity type that will receive
        /// the value extracted from the SQL result set.
        ///
        /// The underlying <see cref="System.Reflection.PropertyInfo"/> is used to:
        /// <list type="bullet">
        /// <item>
        /// <description>Read metadata such as name and type.</description>
        /// </item>
        /// <item>
        /// <description>Assign converted values dynamically at runtime.</description>
        /// </item>
        /// </list>
        /// Only properties that are writable and decorated with <see cref="SqlFieldAttribute"/> are
        /// included in this mapping.
        /// </remarks>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// Gets the SQL field attribute that defines the mapping behavior.
        /// </summary>
        /// <remarks>
        /// The <see cref="SqlFieldAttribute"/> provides additional mapping information, such as:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// The explicit column name in the SQL result set (when different from the property name).
        /// </description>
        /// </item>
        /// </list>
        /// This attribute allows the mapping layer to remain decoupled from SQL aliases and naming
        /// conventions, enabling flexible projections and refactoring safety.
        /// </remarks>
        public SqlFieldAttribute Attribute { get; private set; }

        /// <summary>
        /// Gets the resolved target CLR type used for value conversion.
        /// </summary>
        /// <remarks>
        /// This type represents the effective destination type for the mapped value after normalization.
        ///
        /// In particular:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <see cref="Nullable{T}"/> types are unwrapped to their underlying type to allow safe
        /// conversion via <see cref="System.Convert"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// The resolved type is used consistently during value conversion to avoid runtime
        /// exceptions caused by incompatible types.
        /// </description>
        /// </item>
        /// </list>
        /// The original property type can still be inferred from <see cref="Property"/>, but <see
        /// cref="TargetType"/> ensures conversion correctness and performance.
        /// </remarks>
        public Type TargetType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlFieldMap"/> class.
        /// </summary>
        /// <param name="property">The reflected entity property that will receive the mapped value.</param>
        /// <param name="attribute">
        /// The <see cref="SqlFieldAttribute"/> instance associated with the property, defining the
        /// SQL column mapping behavior.
        /// </param>
        /// <param name="targetType">
        /// The resolved CLR type used during value conversion, with nullable wrappers already
        /// removed when applicable.
        /// </param>
        /// <remarks>
        /// This constructor is intended to be called exclusively during metadata discovery and caching.
        ///
        /// Once instantiated, the <see cref="SqlFieldMap"/> should be treated as immutable and
        /// reused across all mapping operations for the associated entity type.
        /// </remarks>
        public SqlFieldMap(
            PropertyInfo property,
            SqlFieldAttribute attribute,
            Type targetType)
        {
            this.Property = property;
            this.Attribute = attribute;
            this.TargetType = targetType;
        }
    }
}