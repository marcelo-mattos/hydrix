using Hydrix.Attributes.Schemas;
using Hydrix.Schemas;
using Hydrix.Schemas.Contract;
using Hydrix.Tests.Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hydrix.Tests.Database.Entity
{
    /// <summary>
    /// Order Entity
    /// </summary>
#if SQLSERVER_ENV_ENABLED
    [Table(nameof(Product), Schema = "[dbo]")]
    public class Product :
#else

    [Table(nameof(Product), Schema = "public")]
    public class Product :
#endif
        DatabaseEntity, ITable
    {
        /// <summary>
        /// Id field
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column]
        [Required(ErrorMessage = "Id é obrigatório.")]
        public Guid Id { get; set; }

        /// <summary>
        /// CustomerId field
        /// </summary>
#if SQLSERVER_ENV_ENABLED
        [ForeignKey("CustomerId")]
        [Column]
        public Guid? CustomerId { get; set; }
#else

        [ForeignKey("customer_id")]
        [Column("customer_id")]
        public Guid? CustomerId { get; set; }

#endif

        /// <summary>
        /// Customer data
        /// </summary>
#if SQLSERVER_ENV_ENABLED
        [ForeignTable("Customer", Schema = "[dbo]", PrimaryKeys = new[] { "Id" }, ForeignKeys = new[] { "CustomerId" })]
        public Customer Customer { get; set; }
#else

        [ForeignTable("customer", Schema = "public", PrimaryKeys = new[] { "Id" }, ForeignKeys = new[] { "customer_id" })]
        public Customer Customer { get; set; }

#endif

        /// <summary>
        /// Name field
        /// </summary>
        [Column]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "NameRequired", ErrorMessageResourceType = typeof(Shared))]
        [MaxLength(50, ErrorMessageResourceName = "NameMaxLength", ErrorMessageResourceType = typeof(Shared))]
        public String Name { get; set; }

        /// <summary>
        /// Ean field
        /// </summary>
        [Column]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "EanRequired", ErrorMessageResourceType = typeof(Shared))]
        [MaxLength(13, ErrorMessageResourceName = "EanMaxLength", ErrorMessageResourceType = typeof(Shared))]
        public String Ean { get; set; }

        /// <summary>
        /// Quantity field
        /// </summary>
        [Column]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "QuantityRequired", ErrorMessageResourceType = typeof(Shared))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "QuantityRange", ErrorMessageResourceType = typeof(Shared))]
        public Decimal Quantity { get; set; }

        /// <summary>
        /// Price field
        /// </summary>
        [Column]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "PriceRequired", ErrorMessageResourceType = typeof(Shared))]
        [Range(0.01, double.MaxValue, ErrorMessageResourceName = "PriceRange", ErrorMessageResourceType = typeof(Shared))]
        public Decimal Price { get; set; }

        /// <summary>
        /// Type field
        /// </summary>
        [Column]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "TypeRequired", ErrorMessageResourceType = typeof(Shared))]
        [MaxLength(1, ErrorMessageResourceName = "TypeMaxLength", ErrorMessageResourceType = typeof(Shared))]
        public string Type { get; set; }

        /// <summary>
        /// Token field
        /// </summary>
        [Column]
        public string Token { get; set; }
    }
}
