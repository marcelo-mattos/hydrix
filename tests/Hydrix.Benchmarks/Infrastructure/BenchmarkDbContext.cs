#if NET8_0_OR_GREATER
using Hydrix.Benchmarks.Models;
using Microsoft.EntityFrameworkCore;

namespace Hydrix.Benchmarks.Infrastructure
{
    /// <summary>
    /// Entity Framework Core context used exclusively for performance benchmarks.
    /// </summary>
    internal sealed class BenchmarkDbContext :
        DbContext
    {
        /// <summary>
        /// Initializes a new benchmark DbContext with the specified options.
        /// </summary>
        public BenchmarkDbContext(
            DbContextOptions<BenchmarkDbContext> options)
            : base(options)
        { }

        /// <summary>
        /// Gets the benchmark user entity set.
        /// </summary>
        public DbSet<BenchmarkUserEntity> Users
            => Set<BenchmarkUserEntity>();

        /// <summary>
        /// Gets the benchmark order entity set.
        /// </summary>
        public DbSet<BenchmarkOrderEntity> Orders
            => Set<BenchmarkOrderEntity>();

        /// <summary>
        /// Configures the benchmark entity mappings.
        /// </summary>
        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BenchmarkUserEntity>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(user => user.Id);
                entity.Property(user => user.Id).ValueGeneratedNever();
                entity.Property(user => user.Name);
                entity.Property(user => user.Age);
                entity.Property(user => user.Status).HasConversion<int>();
                entity.HasOne(user => user.Order)
                    .WithOne(order => order.User)
                    .HasForeignKey<BenchmarkOrderEntity>(order => order.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<BenchmarkOrderEntity>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(order => order.Id);
                entity.Property(order => order.Id).ValueGeneratedNever();
                entity.Property(order => order.UserId);
                entity.Property(order => order.Total);
            });
        }
    }

    /// <summary>
    /// EF entity mapped to the Users table for benchmark projections.
    /// </summary>
    internal sealed class BenchmarkUserEntity
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user age.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the user status.
        /// </summary>
        public UserStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the user's single order.
        /// </summary>
        public BenchmarkOrderEntity Order { get; set; } = null!;
    }

    /// <summary>
    /// EF entity mapped to the Orders table for benchmark projections.
    /// </summary>
    internal sealed class BenchmarkOrderEntity
    {
        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the related user identifier.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the order total.
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// Gets or sets the parent user.
        /// </summary>
        public BenchmarkUserEntity User { get; set; } = null!;
    }
}
#endif
