using Hydrix.Benchmarks.Models;
using Microsoft.EntityFrameworkCore;

namespace Hydrix.Benchmarks.Infrastructure
{
    /// <summary>
    /// Provides the Entity Framework Core context used exclusively by the benchmark suites.
    /// </summary>
    /// <remarks>
    /// The context models only the tables and relationships required by the benchmark project so query materialization
    /// costs stay focused on the access strategy being measured instead of unrelated domain configuration.
    /// </remarks>
    internal sealed class BenchmarkDbContext :
        DbContext
    {
        /// <summary>
        /// Initializes a new benchmark DbContext instance with the options prepared by the benchmark fixture.
        /// </summary>
        /// <param name="options">
        /// The configured Entity Framework Core options that define the SQLite connection, tracking behavior, and other
        /// services required to execute the benchmark queries.
        /// </param>
        public BenchmarkDbContext(
            DbContextOptions<BenchmarkDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets the benchmark user entity set mapped to the <c>Users</c> table.
        /// </summary>
        public DbSet<BenchmarkUserEntity> Users =>
            Set<BenchmarkUserEntity>();

        /// <summary>
        /// Gets the benchmark order entity set mapped to the <c>Orders</c> table.
        /// </summary>
        public DbSet<BenchmarkOrderEntity> Orders =>
            Set<BenchmarkOrderEntity>();

        /// <summary>
        /// Configures the entity mappings, keys, value conversions, and one-to-one relationship used by the benchmarks.
        /// </summary>
        /// <param name="modelBuilder">
        /// The model builder used to register every entity shape that Entity Framework Core must understand before the
        /// benchmark queries can execute.
        /// </param>
        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BenchmarkUserEntity>(
                entity =>
                {
                    entity.ToTable("Users");
                    entity.HasKey(
                        user => user.Id);
                    entity.Property(
                        user => user.Id)
                        .ValueGeneratedNever();
                    entity.Property(
                        user => user.Name);
                    entity.Property(
                        user => user.Age);
                    entity.Property(
                        user => user.Status)
                        .HasConversion<int>();
                    entity.HasOne(
                            user => user.Order)
                        .WithOne(
                            order => order.User)
                        .HasForeignKey<BenchmarkOrderEntity>(
                            order => order.UserId)
                        .OnDelete(
                            DeleteBehavior.NoAction);
                });

            modelBuilder.Entity<BenchmarkOrderEntity>(
                entity =>
                {
                    entity.ToTable("Orders");
                    entity.HasKey(
                        order => order.Id);
                    entity.Property(
                        order => order.Id)
                        .ValueGeneratedNever();
                    entity.Property(
                        order => order.UserId);
                    entity.Property(
                        order => order.Total);
                });
        }
    }

    /// <summary>
    /// Represents the internal Entity Framework projection of a benchmark user row.
    /// </summary>
    /// <remarks>
    /// This type exists only so Entity Framework Core can materialize the in-memory SQLite schema used by the benchmark
    /// suites before projecting into the public DTO models.
    /// </remarks>
    internal sealed class BenchmarkUserEntity
    {
        /// <summary>
        /// Gets or sets the primary key value stored in the <c>Users.Id</c> column.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user name stored in the <c>Users.Name</c> column.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the age stored in the <c>Users.Age</c> column.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the logical user status stored in the <c>Users.Status</c> column.
        /// </summary>
        public UserStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the single related order linked through the benchmark one-to-one association.
        /// </summary>
        public BenchmarkOrderEntity Order { get; set; } = null!;
    }

    /// <summary>
    /// Represents the internal Entity Framework projection of a benchmark order row.
    /// </summary>
    internal sealed class BenchmarkOrderEntity
    {
        /// <summary>
        /// Gets or sets the primary key value stored in the <c>Orders.Id</c> column.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the foreign key value pointing back to the related user row.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the monetary total stored in the <c>Orders.Total</c> column.
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// Gets or sets the parent user entity associated with this order.
        /// </summary>
        public BenchmarkUserEntity User { get; set; } = null!;
    }
}
