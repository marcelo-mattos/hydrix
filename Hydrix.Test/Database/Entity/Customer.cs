using Hydrix.Attributes.Parameters;
using Hydrix.Attributes.Schemas;
using Hydrix.Schemas;
using System;
using System.Data;

namespace Hydrix.Test.Database.Entity
{
    /// <summary>
    /// Customer Entity
    /// </summary>
    [SqlEntity("[dbo]", nameof(Customer), nameof(Id))]
    public class Customer :
        ISqlEntity
    {
        /// <summary>
        /// Id field
        /// </summary>
        [SqlField]
        [SqlParameter("@p_Id", DbType.Guid)]
        public Guid Id { get; set; }

        /// <summary>
        /// Name field
        /// </summary>
        [SqlField]
        [SqlParameter("@p_Name", DbType.String)]
        public String Name { get; set; }

        /// <summary>
        /// Birthdate field
        /// </summary>
        [SqlField]
        [SqlParameter("@p_Birthdate", DbType.DateTime)]
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// Level field
        /// </summary>
        [SqlField]
        [SqlParameter("@p_Level", DbType.Int32)]
        public Int32 Level { get; set; }

        /// <summary>
        /// Salary field
        /// </summary>
        [SqlField]
        [SqlParameter("@p_Salary", DbType.Decimal)]
        public Decimal? Salary { get; set; }

        /// <summary>
        /// IsActive field
        /// </summary>
        [SqlField]
        [SqlParameter("@p_IsActive", DbType.Boolean)]
        public Boolean? IsActive { get; set; }
    }
}