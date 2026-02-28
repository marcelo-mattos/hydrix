namespace Hydrix.Schemas.Contract
{
    /// <summary>
    /// Defines a contract for table entities within the data model.
    /// </summary>
    /// <remarks>Implementing this interface indicates that the class represents a table entity and must also
    /// fulfill the requirements of IEntity. Use this interface as a base for defining specific table-related behaviors
    /// in the data context.</remarks>
    public interface ITable :
        IEntity
    { }
}