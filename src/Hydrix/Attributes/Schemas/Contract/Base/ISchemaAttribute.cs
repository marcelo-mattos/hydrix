namespace Hydrix.Attributes.Schemas.Contract.Base
{
    /// <summary>
    /// Define the basics of a Sql object.
    /// </summary>
    public interface ISchemaAttribute :
        ISqlAttribute
    {
        /// <summary>
        /// Gets or sets the procedure schema.
        /// </summary>
        string Schema { get; }

        /// <summary>
        /// Gets or sets the procedure name.
        /// </summary>
        string Name { get; }
    }
}