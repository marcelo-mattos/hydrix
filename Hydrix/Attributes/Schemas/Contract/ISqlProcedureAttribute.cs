namespace Hydrix.Attributes.Schemas.Contract
{
    /// <summary>
    /// Represents a procedure object from a database and its mapping to System.Data.DataSet
    /// columns; and is implemented by .NET Framework data providers that access data sources.
    /// </summary>
    public interface ISqlProcedureAttribute :
        Base.ISqlSchemaAttribute
    { }
}