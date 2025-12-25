using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Metadata;
using System;
using System.Reflection;

internal sealed class SqlFieldMap
{
    /// <summary>
    /// Gets the reflected property associated with this field.
    /// Used only during metadata construction.
    /// </summary>
    public PropertyInfo Property { get; }

    /// <summary>
    /// Gets the SQL field mapping attribute.
    /// </summary>
    public SqlFieldAttribute Attribute { get; }

    /// <summary>
    /// Gets the normalized target CLR type for value conversion.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Gets the compiled setter delegate used to assign values
    /// to the entity property without reflection.
    /// </summary>
    public Action<object, object> Setter { get; }

    public SqlFieldMap(
        PropertyInfo property,
        SqlFieldAttribute attribute,
        Type targetType)
    {
        Property = property;
        Attribute = attribute;
        TargetType = targetType;
        Setter = SqlMetadataFactory.CreateSetter(property);
    }
}
