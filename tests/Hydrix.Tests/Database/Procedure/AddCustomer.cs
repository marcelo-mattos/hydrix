using Hydrix.Attributes.Parameters;
using Hydrix.Attributes.Schemas;
using Hydrix.Schemas;
using Hydrix.Schemas.Contract;

#if SQLSERVER_ENV_ENABLED
using Microsoft.Data.SqlClient;
#else

using Npgsql;

#endif

using System;
using System.Data;

namespace Hydrix.Tests.Database.Procedure
{
    /// <summary>
    /// GetCustomer Procedure
    /// </summary>
#if SQLSERVER_ENV_ENABLED
    [Procedure("[AddCustomer]", Schema = "[dbo]")]
    public class AddCustomer :
        DatabaseEntity, IProcedure<SqlParameter>
#else

    [Procedure("add_customer()", Schema = "public")]
    public class AddCustomer :
        DatabaseEntity, IProcedure<NpgsqlParameter>
#endif
    {
        /// <summary>
        /// Id parameter
        /// </summary>
        [Parameter("p_Id", DbType.Guid)]
        public Guid Id { get; set; }

        /// <summary>
        /// Name parameter
        /// </summary>
        [Parameter("p_Name", DbType.String)]
        public String Name { get; set; }

        /// <summary>
        /// Birthdate parameter
        /// </summary>
        [Parameter("p_Birthdate", DbType.DateTime)]
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// Level parameter
        /// </summary>
        [Parameter("p_Level", DbType.Int32)]
        public Int32 Level { get; set; }

        /// <summary>
        /// Salary parameter
        /// </summary>
        [Parameter("p_Salary", DbType.Decimal)]
        public Decimal? Salary { get; set; }

        /// <summary>
        /// IsActive parameter
        /// </summary>
#if SQLSERVER_ENV_ENABLED
        [Parameter("p_IsActive", DbType.Boolean)]
        public Boolean? IsActive { get; set; }
#else

        [Parameter("p_is_active", DbType.Boolean)]
        public Boolean? IsActive { get; set; }

#endif
    }
}
