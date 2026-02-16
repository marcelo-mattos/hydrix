using Hydrix.Attributes.Schemas;
using Hydrix.Schemas;
using Hydrix.Schemas.Contract;
using Microsoft.Data.SqlClient;

namespace Hydrix.Tests.Database.Procedure
{
    /// <summary>
    /// GetCustomer Procedure
    /// </summary>
    [Procedure("[GetCustomers]", Schema = "[dbo]")]
    public class GetCustomer :
        DatabaseEntity, IProcedure<SqlParameter>
    { }
}