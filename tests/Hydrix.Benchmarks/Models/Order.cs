using Hydrix.Schemas.Contract;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hydrix.Benchmarks.Models
{
    /// <summary>
    /// Represents the order projection used by the nested benchmark scenarios.
    /// </summary>
    /// <remarks>
    /// The benchmark suite keeps this type intentionally small so nested materialization focuses on join mapping costs
    /// instead of unrelated domain behavior.
    /// </remarks>
    [Table("Orders")]
    public sealed class Order :
        ITable
    {
        /// <summary>
        /// Gets or sets the primary key value read from the <c>Orders.Id</c> column.
        /// </summary>
        [Column("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the order total read from the <c>Orders.Total</c> column.
        /// </summary>
        [Column("Total")]
        public double Total { get; set; }
    }
}
