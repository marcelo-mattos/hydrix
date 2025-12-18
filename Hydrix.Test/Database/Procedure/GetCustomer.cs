using Hydrix.Attributes.Schemas;
using Hydrix.Schemas;
using System.Data.SqlClient;

namespace Hydrix.Test.Database.Procedure
{
    /// <summary>
    /// GetCustomer Procedure
    /// </summary>
    [SqlProcedure("[dbo]", "[GetCustomers]")]
    public class GetCustomer :
        ISqlProcedure<SqlParameter>
    { }
}