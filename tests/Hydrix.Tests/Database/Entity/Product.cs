using Hydrix.Attributes.Schemas;
using Hydrix.Schemas;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hydrix.Tests.Database.Entity
{
    /// <summary>
    /// Order Entity
    /// </summary>
    [Table(nameof(Product), Schema = "[dbo]")]
    public class Product :
        ITable
    {
        /// <summary>
        /// Id field
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column]
        public Guid Id { get; set; }

        /// <summary>
        /// CustomerId field
        /// </summary>
        [ForeignKey("FK_Product_Customer")]
        [Column]
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Customer data
        /// </summary>
        [NestedTable("Customer", Schema = "[dbo]", Key = "Id")]
        public Customer Customer { get; set; }

        /// <summary>
        /// Name field
        /// </summary>
        [Column]
        public String Name { get; set; }

        /// <summary>
        /// Ean field
        /// </summary>
        [Column]
        public String Ean { get; set; }

        /// <summary>
        /// Quantity field
        /// </summary>
        [Column]
        public Decimal Quantity { get; set; }

        /// <summary>
        /// Price field
        /// </summary>
        [Column]
        public Decimal Price { get; set; }
    }
}