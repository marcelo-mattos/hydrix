using Hydrix.Schemas;
using Hydrix.Schemas.Contract;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hydrix.Tests.Database.Entity
{
    /// <summary>
    /// Customer Entity
    /// </summary>
#if SQLSERVER_ENV_ENABLED
    [Table(nameof(Customer), Schema = "[dbo]")]
    public class Customer :
#else

    [Table(nameof(Customer), Schema = "public")]
    public class Customer :
#endif
        DatabaseEntity, ITable
    {
        /// <summary>
        /// Id field
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column]
        public Guid Id { get; set; }

        /// <summary>
        /// Name field
        /// </summary>
        [Column]
        public String Name { get; set; }

        /// <summary>
        /// Birthdate field
        /// </summary>
        [Column]
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// Level field
        /// </summary>
        [Column]
        public Int32 Level { get; set; }

        /// <summary>
        /// Salary field
        /// </summary>
        [Column]
        public Decimal? Salary { get; set; }

        /// <summary>
        /// IsActive field
        /// </summary>
#if SQLSERVER_ENV_ENABLED
        [Column]
        public Boolean? IsActive { get; set; }
#else

        [Column("is_active")]
        public Boolean? IsActive { get; set; }

#endif
    }
}
