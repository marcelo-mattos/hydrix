using System;

namespace Hydrix.Attributes.Schemas.Base
{
    /// <summary>
    /// Represents an entity field from a database table and its mapping to System.Data.DataSet
    /// columns; and is implemented by .NET Framework data providers that access data sources.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class SqlAttribute :
        Attribute, Contract.Base.ISqlAttribute
    { }
}