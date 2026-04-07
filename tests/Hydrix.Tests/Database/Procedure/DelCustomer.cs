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
    [Procedure("[DelCustomers]", Schema = "[dbo]")]
    public class DelCustomers :
        DatabaseEntity, IProcedure<SqlParameter>
#else

    [Procedure("del_customers()", Schema = "public")]
    public class DelCustomers :
        DatabaseEntity, IProcedure<NpgsqlParameter>
#endif
    { }
}
