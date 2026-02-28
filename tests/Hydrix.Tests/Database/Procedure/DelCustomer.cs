using Hydrix.Attributes.Schemas;
using Hydrix.Schemas;
using Hydrix.Schemas.Contract;
using Microsoft.Data.SqlClient;

namespace Hydrix.Tests.Database.Procedure
{
    /// <summary>
    /// GetCustomer Procedure
    /// </summary>
    [Procedure("[DelCustomers]", Schema = "[dbo]")]
    public class DelCustomers :
        DatabaseEntity, IProcedure<SqlParameter>
    { }
}