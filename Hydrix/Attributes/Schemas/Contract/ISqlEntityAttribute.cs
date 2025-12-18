namespace Hydrix.Attributes.Schemas.Contract
{
    /// <summary>
    /// Represents an entity from a database table and its mapping to System.Data.DataSet
    /// columns; and is implemented by .NET Framework data providers that access data sources.
    /// </summary>
    public interface ISqlEntityAttribute :
        Base.ISqlSchemaAttribute
    { }
}