using System.Data;

namespace Hydrix.Schemas
{
    /// <summary>
    /// Represents a Sql Procedure that holds the data parameters to be executed by the connection command
    /// </summary>
    public interface IProcedure<TDataParameterDriver>
        where TDataParameterDriver : IDataParameter, new()
    { }
}