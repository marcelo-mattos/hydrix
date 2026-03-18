using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Dapper;
using Hydrix.Benchmarks.Infrastructure;
using Hydrix.Benchmarks.Models;
using System.Collections.Generic;
using System.Data;

namespace Hydrix.Benchmarks.Benchmarks
{
    /// <summary>
    /// Provides benchmarking methods for comparing the performance of different data access strategies using a SQLite
    /// database.
    /// </summary>
    /// <remarks>This class sets up a SQLite database with a specified number of rows and executes benchmarks
    /// for Dapper, Hydrix, and ADO.NET manual data retrieval methods. It is designed to measure the performance of
    /// these methods under varying conditions defined by the parameters.</remarks>
    [MemoryDiagnoser]
    [RankColumn]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class FlatBenchmarks
    {
        /// <summary>
        /// Gets the database fixture used for testing database interactions.
        /// </summary>
        /// <remarks>This field is initialized to a non-null value before any tests are run, ensuring that
        /// the database context is always available during test execution.</remarks>
        private SqliteDatabaseFixture _db = null!;

        /// <summary>
        /// Represents the database connection used for data operations.
        /// </summary>
        private IDbConnection _conn = null!;

        /// <summary>
        /// Gets or sets the total number of rows to use as the seed size for database operations in benchmarks.
        /// </summary>
        /// <remarks>Set this field to define the initial dataset size for performance testing or seeding
        /// scenarios. Adjust the value as needed to match the scale of the benchmark or test case.</remarks>
        [Params(100_000)]
        public int RowCount;

        /// <summary>
        /// Gets or sets the number of rows to return per query. This value is used to control the size of each result
        /// set, enabling pagination or batch processing of data.
        /// </summary>
        /// <remarks>Adjusting this value can help optimize performance based on the expected size of the
        /// result set. A higher value may improve throughput for large datasets, while a lower value can reduce memory
        /// usage and improve responsiveness for smaller queries.</remarks>
        [Params(1_000, 10_000)]
        public int Take;

        /// <summary>
        /// Stores the SQL query string used for benchmarking data retrieval methods. The query selects specific columns from
        /// the Users table and applies a limit based on the Take parameter.
        /// </summary>
        private string _sql = null!;

        /// <summary>
        /// Initializes the database connection and prepares the environment for benchmarking by seeding the database
        /// with a predefined number of rows.
        /// </summary>
        /// <remarks>This method is executed before any benchmarks are run to ensure that the database is
        /// in a consistent and known state. It sets up the database connection and
        /// prepares the SQL query used for retrieving user data during benchmarks.</remarks>
        [GlobalSetup]
        public void GlobalSetup()
        {
            _db = new SqliteDatabaseFixture();
            _db.EnsureSeeded(RowCount);

            _conn = _db.Connection;

            _sql = "SELECT Id, Name, Age, Status FROM Users ORDER BY Id LIMIT $take";
        }

        /// <summary>
        /// Releases resources used by the test class after all benchmarks have completed.
        /// </summary>
        /// <remarks>Call this method to ensure that any unmanaged resources held by the database context
        /// are properly disposed, preventing resource leaks after benchmark execution.</remarks>
        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _db.Dispose();
        }

        /// <summary>
        /// Retrieves a list of user data in a flat structure from the database using Dapper.
        /// </summary>
        /// <remarks>This method executes a SQL query defined in the <see langword="_sql"/> variable,
        /// using a parameter to limit the number of results returned. Ensure that the <see langword="Take"/> parameter
        /// is set appropriately to control the number of records fetched.</remarks>
        /// <returns>A list of <see cref="UserFlat"/> objects representing the user data retrieved from the database.</returns>
        [Benchmark(Baseline = true)]
        public List<UserFlat> Dapper_Flat()
        {
            return SqlMapper
                .Query<UserFlat>(_conn, _sql, new { take = Take })
                .AsList();
        }

        /// <summary>
        /// Executes a query to retrieve a list of user data in a flat structure.
        /// </summary>
        /// <remarks>This method utilizes a parameters object to support named parameters in the SQL
        /// query, even when using SQLite. Ensure that the <see cref="Take"/> property is set to define the number of
        /// records to retrieve.</remarks>
        /// <returns>A list of <see cref="UserFlat"/> objects representing the user data retrieved from the database.</returns>
        [Benchmark]
        public List<UserFlat> Hydrix_Flat()
        {
            // Hydrix expects a parameters object even when using SQLite named params.
            return HydrixDataCore
                .Query<UserFlat>(_conn, _sql, new { take = Take })
                .AsList();
        }

        /// <summary>
        /// Retrieves a list of user records from the database using ADO.NET with manual parameter handling.
        /// </summary>
        /// <remarks>This method executes a SQL command to fetch user data, applying a limit on the number
        /// of records returned. Ensure that the <see cref="Take"/> property is set to a valid positive integer to avoid
        /// unexpected results.</remarks>
        /// <returns>A list of <see cref="UserFlat"/> objects representing the users retrieved from the database. The number of
        /// users returned is limited by the value of the <see cref="Take"/> property.</returns>
        [Benchmark]
        public List<UserFlat> AdoNet_Manual()
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = _sql;

            var p = cmd.CreateParameter();
            p.ParameterName = "$take";
            p.Value = Take;
            cmd.Parameters.Add(p);

            using var reader = cmd.ExecuteReader();

            // Cache ordinals once.
            var ordId = reader.GetOrdinal("Id");
            var ordName = reader.GetOrdinal("Name");
            var ordAge = reader.GetOrdinal("Age");
            var ordStatus = reader.GetOrdinal("Status");

            var list = new List<UserFlat>(Take);

            while (reader.Read())
            {
                list.Add(new UserFlat
                {
                    Id = reader.GetInt32(ordId),
                    Name = reader.GetString(ordName),
                    Age = reader.GetInt32(ordAge),
                    Status = (UserStatus)reader.GetInt32(ordStatus)
                });
            }

            return list;
        }
    }
}