using Hydrix.Attributes.Schemas;
using Hydrix.Schemas;
using Hydrix.Schemas.Contract;

#if SQLSERVER_ENV_ENABLED
using Microsoft.Data.SqlClient;
#else

using Npgsql;

#endif

namespace Hydrix.Tests.Database.Procedure
{
    /// <summary>
    /// GetCustomer Procedure
    /// </summary>
#if SQLSERVER_ENV_ENABLED
    [Procedure("[GetCustomers]", Schema = "[dbo]")]
    public class GetCustomer :
        DatabaseEntity, IProcedure<SqlParameter>
#else

    [Procedure("get_customers")]
    public class GetCustomer :
        DatabaseEntity, IProcedure<NpgsqlParameter>
#endif
    { }
}
