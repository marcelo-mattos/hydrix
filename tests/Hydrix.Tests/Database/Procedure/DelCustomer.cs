using Hydrix.Attributes.Schemas;
using Hydrix.Schemas;
using Microsoft.Data.SqlClient;

namespace Hydrix.Tests.Database.Procedure
{
    /// <summary>
    /// GetCustomer Procedure
    /// </summary>
    [Procedure("[DelCustomers]", Schema = "[dbo]")]
    public class DelCustomers :
        IProcedure<SqlParameter>
    { }
}