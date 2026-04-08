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
    /// Compares flat user projections across Dapper, Hydrix, Entity Framework Core, and manual ADO.NET materialization.
    /// </summary>
    /// <remarks>
    /// Every benchmark in this suite reads the same seeded SQLite data set and projects it into <see cref="UserFlat"/>
    /// so the measurements emphasize data-access and materialization costs rather than differences in result shape.
    /// </remarks>
    [MemoryDiagnoser]
    [RankColumn]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class FlatBenchmarks
    {
        /// <summary>
        /// Caches the Entity Framework Core query plan used to materialize flat user rows without change tracking.
        /// </summary>
        /// <remarks>
        /// Reusing a compiled query keeps the Entity Framework benchmark focused on execution and projection costs instead
        /// of repeatedly recompiling the LINQ expression.
        /// </remarks>
        private static readonly Func<BenchmarkDbContext, int, IEnumerable<UserFlat>> EntityFrameworkFlatQuery =
            EF.CompileQuery(
                (BenchmarkDbContext context, int take) => context.Users
                    .AsNoTracking()
                    .OrderBy(
                        user => user.Id)
                    .Select(
                        user => new UserFlat
                        {
                            Id = user.Id,
                            Name = user.Name,
                            Age = user.Age,
                            Status = user.Status,
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
        /// Stores the SQL command used by the flat Dapper, Hydrix, and manual ADO.NET benchmarks.
        /// </summary>
        private string _sql = null!;

        /// <summary>
        /// Gets or sets the number of rows that must exist in the benchmark database before each run starts.
        /// </summary>
        /// <remarks>
        /// The fixture reseeds the in-memory database whenever the current row count differs from this value so all
        /// benchmark methods execute against the same data volume.
        /// </remarks>
        [Params(100_000)]
        public int RowCount { get; set; }

        /// <summary>
        /// Gets or sets the number of rows each benchmark query should return.
        /// </summary>
        /// <remarks>
        /// Varying this parameter lets the suite compare how each data-access strategy behaves across smaller and larger
        /// result sets while keeping the SQL shape constant.
        /// </remarks>
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
            _sql = "SELECT Id, Name, Age, Status FROM Users ORDER BY Id LIMIT @take";
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
        /// Materializes flat user rows through Dapper.
        /// </summary>
        /// <returns>
        /// A list of <see cref="UserFlat"/> instances returned by Dapper for the configured <see cref="Take"/> value.
        /// </returns>
        [Benchmark(Baseline = true)]
        public List<UserFlat> Dapper_Flat()
        {
            return SqlMapper
                .Query<UserFlat>(
                    _connection,
                    _sql,
                    new { take = Take })
                .AsList();
        }

        /// <summary>
        /// Materializes flat user rows through Hydrix.
        /// </summary>
        /// <returns>
        /// A list of <see cref="UserFlat"/> instances returned by Hydrix for the configured <see cref="Take"/> value.
        /// </returns>
        [Benchmark]
        public List<UserFlat> Hydrix_Flat()
        {
            return HydrixDataCore
                .Query<UserFlat>(
                    _connection,
                    _sql,
                    new { take = Take })
                .AsList();
        }

        /// <summary>
        /// Materializes flat user rows through Entity Framework Core using the compiled no-tracking query.
        /// </summary>
        /// <returns>
        /// A list of <see cref="UserFlat"/> instances projected by Entity Framework Core for the configured
        /// <see cref="Take"/> value.
        /// </returns>
        [Benchmark]
        public List<UserFlat> EntityFramework_Flat()
        {
            using var context = new BenchmarkDbContext(
                _entityFrameworkOptions);
            return EntityFrameworkFlatQuery(
                    context,
                    Take)
                .ToList();
        }

        /// <summary>
        /// Materializes flat user rows through manual ADO.NET code.
        /// </summary>
        /// <returns>
        /// A list of <see cref="UserFlat"/> instances created by manually reading the SQLite data reader.
        /// </returns>
        [Benchmark]
        public List<UserFlat> AdoNet_Manual()
        {
            using var command = _connection.CreateCommand();
            command.CommandText = _sql;

            var takeParameter = command.CreateParameter();
            takeParameter.ParameterName = "@take";
            takeParameter.Value = Take;
            command.Parameters.Add(
                takeParameter);

            using var reader = command.ExecuteReader();

            var idOrdinal = reader.GetOrdinal(
                "Id");
            var nameOrdinal = reader.GetOrdinal(
                "Name");
            var ageOrdinal = reader.GetOrdinal(
                "Age");
            var statusOrdinal = reader.GetOrdinal(
                "Status");

            var results = new List<UserFlat>(
                Take);

            while (reader.Read())
            {
                results.Add(
                    new UserFlat
                    {
                        Id = reader.GetInt32(
                            idOrdinal),
                        Name = reader.GetString(
                            nameOrdinal),
                        Age = reader.GetInt32(
                            ageOrdinal),
                        Status = (UserStatus)reader.GetInt32(
                            statusOrdinal),
                    });
            }

            return results;
        }
    }
}
