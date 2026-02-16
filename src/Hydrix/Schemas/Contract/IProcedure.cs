using System.Data;

namespace Hydrix.Schemas.Contract
{
    /// <summary>
    /// Defines a contract for a procedure that operates on data parameters using a specified data parameter driver.
    /// </summary>
    /// <typeparam name="TDataParameterDriver">The type of data parameter driver that must implement the IDataParameter interface and provide a parameterless
    /// constructor.</typeparam>
    public interface IProcedure<TDataParameterDriver> :
        IEntity
        where TDataParameterDriver : IDataParameter, new()
    { }
}