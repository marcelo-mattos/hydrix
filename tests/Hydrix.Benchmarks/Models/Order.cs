using Hydrix.Schemas.Contract;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hydrix.Benchmarks.Models
{
    /// <summary>
    /// Represents an order in the system, including its unique identifier and total amount.
    /// </summary>
    /// <remarks>This class is mapped to the 'Orders' table in the database. Each instance corresponds to a
    /// single order record, with properties for the order's ID and total cost.</remarks>
    [Table("Orders")]
    public sealed class Order :
        ITable
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [Column("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the total monetary amount for the transaction.
        /// </summary>
        [Column("Total")]
        public double Total { get; set; }
    }
}