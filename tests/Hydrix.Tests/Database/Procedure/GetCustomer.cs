using Hydrix.Attributes.Schemas;
using Hydrix.Schemas;
using Microsoft.Data.SqlClient;

namespace Hydrix.Tests.Database.Procedure
{
    /// <summary>
    /// GetCustomer Procedure
    /// </summary>
    [Procedure("[GetCustomers]", Schema = "[dbo]")]
    public class GetCustomer :
        IProcedure<SqlParameter>
    { }
}