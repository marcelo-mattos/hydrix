using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace Hydrix.Schemas.Contract
{
    /// <summary>
    /// Defines a contract for a procedure that operates on data parameters using a specified data parameter driver.
    /// </summary>
    /// <typeparam name="TDataParameterDriver">The type of data parameter driver that must implement the IDataParameter interface and provide a parameterless
    /// constructor.</typeparam>
    [SuppressMessage(
        "Major Code Smell",
        "S2326",
        Justification = "Generic parameter used for type-based resolution, DI binding, and provider-specific behavior")]
    public interface IProcedure<TDataParameterDriver> :
        IEntity
        where TDataParameterDriver : IDataParameter, new()
    { }
}
