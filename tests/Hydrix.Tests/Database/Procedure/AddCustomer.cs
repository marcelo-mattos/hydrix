using Hydrix.Attributes.Parameters;
using Hydrix.Attributes.Schemas;
using Hydrix.Schemas;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Hydrix.Tests.Database.Procedure
{
    /// <summary>
    /// GetCustomer Procedure
    /// </summary>
    [Procedure("[AddCustomer]", Schema = "[dbo]")]
    public class AddCustomer :
        IProcedure<SqlParameter>
    {
        /// <summary>
        /// Id parameter
        /// </summary>
        [Parameter("@p_Id", DbType.Guid)]
        public Guid Id { get; set; }

        /// <summary>
        /// Name parameter
        /// </summary>
        [Parameter("@p_Name", DbType.String)]
        public String Name { get; set; }

        /// <summary>
        /// Birthdate parameter
        /// </summary>
        [Parameter("@p_Birthdate", DbType.DateTime)]
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// Level parameter
        /// </summary>
        [Parameter("@p_Level", DbType.Int32)]
        public Int32 Level { get; set; }

        /// <summary>
        /// Salary parameter
        /// </summary>
        [Parameter("@p_Salary", DbType.Decimal)]
        public Decimal? Salary { get; set; }

        /// <summary>
        /// IsActive parameter
        /// </summary>
        [Parameter("@p_IsActive", DbType.Boolean)]
        public Boolean? IsActive { get; set; }
    }
}