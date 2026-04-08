using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Dapper;
using Hydrix.Benchmarks.Infrastructure;
using Hydrix.Benchmarks.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Hydrix.Benchmarks.Benchmarks
{
    /// <summary>
    /// Compares nested user-and-order projections across Dapper, Hydrix, Entity Framework Core, and manual ADO.NET.
    /// </summary>
    /// <remarks>
    /// Every benchmark in this suite executes against the same one-to-one user/order data set so the measured cost comes
    /// from each library's nested materialization strategy rather than from differences in SQL shape or cardinality.
    /// </remarks>
    [MemoryDiagnoser]
    [RankColumn]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class NestedBenchmarks
    {
        /// <summary>
        /// Caches the Entity Framework Core query plan used to project users together with their related orders.
        /// </summary>
        /// <remarks>
        /// Reusing a compiled query keeps the Entity Framework benchmark focused on execution and object graph creation
        /// instead of recompiling the LINQ tree on every invocation.
        /// </remarks>
        private static readonly Func<BenchmarkDbContext, int, IEnumerable<UserWithOrder>> EntityFrameworkNestedQuery =
            EF.CompileQuery(
                (BenchmarkDbContext context, int take) => context.Users
                    .AsNoTracking()
                    .OrderBy(
                        user => user.Id)
                    .Select(
                        user => new UserWithOrder
                        {
                            Id = user.Id,
                            Name = user.Name,
                            Age = user.Age,
                            Status = user.Status,
                            Order = new Order
                            {
                                Id = user.Order.Id,
                                Total = user.Order.Total,
                            },
                        })
                    .Take(
                        take));

        /// <summary>
        /// Stores the in-memory SQLite fixture that owns the benchmark database and its open connection.
        /// </summary>
        private SqliteDatabaseFixture _database = null!;

        /// <summary>
        /// Stores the raw database connection shared by the Dapper, Hydrix, and manual ADO.NET benchmarks.
        /// </summary>
        private IDbConnection _connection = null!;

        /// <summary>
        /// Stores the Entity Framework Core options used to create short-lived benchmark DbContext instances.
        /// </summary>
        private DbContextOptions<BenchmarkDbContext> _entityFrameworkOptions = null!;

        /// <summary>
        /// Stores the SQL statement used by the Dapper and manual nested benchmarks.
        /// </summary>
        private string _dapperSql = null!;

        /// <summary>
        /// Stores the SQL statement used by Hydrix, including nested column aliases for the <see cref="Order"/> member.
        /// </summary>
        private string _hydrixSql = null!;

        /// <summary>
        /// Gets or sets the number of rows that must exist in the benchmark database before each run starts.
        /// </summary>
        [Params(100_000)]
        public int RowCount { get; set; }

        /// <summary>
        /// Gets or sets the number of joined rows each benchmark query should return.
        /// </summary>
        [Params(1_000, 10_000)]
        public int Take { get; set; }

        /// <summary>
        /// Creates the database fixture, seeds the requested number of rows, and prepares every query dependency.
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            _database = new SqliteDatabaseFixture();
            _database.EnsureSeeded(
                RowCount);

            _connection = _database.Connection;
            _dapperSql = @"
                SELECT
                  u.Id,
                  u.Name,
                  u.Age,
                  u.Status,
                  o.Id,
                  o.Total
                FROM Users u
                LEFT JOIN Orders o ON o.UserId = u.Id
                ORDER BY u.Id
                LIMIT @take";
            _hydrixSql = @"
                SELECT
                  u.Id,
                  u.Name,
                  u.Age,
                  u.Status,
                  o.Id   AS ""Order.Id"",
                  o.Total AS ""Order.Total""
                FROM Users u
                LEFT JOIN Orders o ON o.UserId = u.Id
                ORDER BY u.Id
                LIMIT @take";
            _entityFrameworkOptions = new DbContextOptionsBuilder<BenchmarkDbContext>()
                .UseSqlite(
                    _database.Connection)
                .UseQueryTrackingBehavior(
                    QueryTrackingBehavior.NoTracking)
                .Options;
        }

        /// <summary>
        /// Disposes the database fixture after BenchmarkDotNet finishes executing this suite.
        /// </summary>
        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _database.Dispose();
        }

        /// <summary>
        /// Materializes nested user/order graphs through Dapper multi-mapping.
        /// </summary>
        /// <returns>
        /// A list of <see cref="UserWithOrder"/> instances built by Dapper from the configured joined result set.
        /// </returns>
        [Benchmark(Baseline = true)]
        public List<UserWithOrder> Dapper_MultiMapping()
        {
            var rows = SqlMapper.Query<UserWithOrder, Order, UserWithOrder>(
                _connection,
                _dapperSql,
                (user, order) =>
                {
                    user.Order = order;
                    return user;
                },
                new { take = Take },
                splitOn: "Id");

            return rows.AsList();
        }

        /// <summary>
        /// Materializes nested user/order graphs through Hydrix.
        /// </summary>
        /// <returns>
        /// A list of <see cref="UserWithOrder"/> instances built by Hydrix from the configured joined result set.
        /// </returns>
        [Benchmark]
        public List<UserWithOrder> Hydrix_Nested()
        {
            return HydrixDataCore
                .Query<UserWithOrder>(
                    _connection,
                    _hydrixSql,
                    new { take = Take })
                .AsList();
        }

        /// <summary>
        /// Materializes nested user/order graphs through Entity Framework Core using the compiled no-tracking query.
        /// </summary>
        /// <returns>
        /// A list of <see cref="UserWithOrder"/> instances projected by Entity Framework Core for the configured
        /// <see cref="Take"/> value.
        /// </returns>
        [Benchmark]
        public List<UserWithOrder> EntityFramework_Nested()
        {
            using var context = new BenchmarkDbContext(
                _entityFrameworkOptions);
            return EntityFrameworkNestedQuery(
                    context,
                    Take)
                .ToList();
        }

        /// <summary>
        /// Materializes nested user/order graphs through manual ADO.NET code.
        /// </summary>
        /// <returns>
        /// A list of <see cref="UserWithOrder"/> instances created by reading the joined SQLite data reader manually.
        /// </returns>
        [Benchmark]
        public List<UserWithOrder> AdoNet_Manual()
        {
            using var command = _connection.CreateCommand();
            command.CommandText = _dapperSql;

            var takeParameter = command.CreateParameter();
            takeParameter.ParameterName = "@take";
            takeParameter.Value = Take;
            command.Parameters.Add(
                takeParameter);

            using var reader = command.ExecuteReader();

            var userIdOrdinal = reader.GetOrdinal(
                "Id");
            var userNameOrdinal = reader.GetOrdinal(
                "Name");
            var userAgeOrdinal = reader.GetOrdinal(
                "Age");
            var userStatusOrdinal = reader.GetOrdinal(
                "Status");
            var orderIdOrdinal = 4;
            var orderTotalOrdinal = 5;

            var results = new List<UserWithOrder>(
                Take);

            while (reader.Read())
            {
                results.Add(
                    new UserWithOrder
                    {
                        Id = reader.GetInt32(
                            userIdOrdinal),
                        Name = reader.GetString(
                            userNameOrdinal),
                        Age = reader.GetInt32(
                            userAgeOrdinal),
                        Status = (UserStatus)reader.GetInt32(
                            userStatusOrdinal),
                        Order = new Order
                        {
                            Id = reader.GetInt32(
                                orderIdOrdinal),
                            Total = reader.GetDouble(
                                orderTotalOrdinal),
                        },
                    });
            }

            return results;
        }
    }
}
