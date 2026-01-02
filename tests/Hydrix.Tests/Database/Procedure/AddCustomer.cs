using Hydrix.Attributes.Schemas;
using Hydrix.Schemas;
using Microsoft.Data.SqlClient;

namespace Hydrix.Tests.Database.Procedure
{
    /// <summary>
    /// GetCustomer Procedure
    /// </summary>
    [SqlProcedure("[dbo]", "[AddCustomer]")]
    public class AddCustomer :
        Entity.Customer, ISqlProcedure<SqlParameter>
    { }
}