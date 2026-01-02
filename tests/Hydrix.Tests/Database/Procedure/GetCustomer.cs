using Hydrix.Attributes.Schemas;
using Hydrix.Schemas;
using Microsoft.Data.SqlClient;

namespace Hydrix.Tests.Database.Procedure
{
    /// <summary>
    /// GetCustomer Procedure
    /// </summary>
    [SqlProcedure("[dbo]", "[GetCustomers]")]
    public class GetCustomer :
        ISqlProcedure<SqlParameter>
    { }
}