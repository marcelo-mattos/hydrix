using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Dapper;
using Hydrix.Benchmarks.Infrastructure;
using Hydrix.Benchmarks.Models;
using Hydrix.Orchestrator.Materializers;
using System.Collections.Generic;
using System.Data;

namespace Hydrix.Benchmarks.Benchmarks
{
    /// <summary>
    /// Provides performance benchmarks for various data retrieval strategies using a SQLite database, comparing Dapper,
    /// Hydrix, and manual ADO.NET implementations.
    /// </summary>
    /// <remarks>This class uses the BenchmarkDotNet library to measure and compare the efficiency of
    /// different data access approaches when retrieving nested data structures. It sets up and tears down a SQLite
    /// database fixture for each benchmark run. The benchmarks include Dapper's multi-mapping, Hydrix's nested query
    /// support, and manual data mapping with ADO.NET. Adjust the RowCount and Take parameters to control the dataset
    /// size and the number of records retrieved in each test.</remarks>
    [MemoryDiagnoser]
    [RankColumn]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class NestedBenchmarks
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
        /// Represents the materializer instance used for processing data within the application.
        /// </summary>
        /// <remarks>This field must be assigned a valid Materializer object before use. Ensure that the
        /// materializer is properly configured to meet the application's data processing requirements.</remarks>
        private Materializer _hydrix = null!;

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
        /// Stores the SQL query string formatted for Dapper multi-mapping to retrieve User and Order data with flat
        /// columns.
        /// </summary>
        /// <remarks>This SQL string is structured to support Dapper's multi-mapping feature, enabling
        /// efficient retrieval of related User and Order records in a single query. The query is designed so that the
        /// result set can be split on the second Id column, which is required by Dapper to correctly map the results to
        /// multiple objects. Ensure that the SQL syntax is compatible with the target database system.</remarks>
        private string _sqlDapper = null!;

        /// <summary>
        /// Stores the SQL query string formatted to comply with Hydrix's column aliasing requirements.
        /// </summary>
        /// <remarks>Order columns in the SQL query must be aliased using the format
        /// "Order.&lt;Column&gt;" to ensure compatibility with Hydrix, which expects property prefixes followed by a
        /// period. Failure to use the correct aliasing convention may result in runtime errors when processing
        /// queries.</remarks>
        private string _sqlHydrix = null!;

        /// <summary>
        /// Initializes the test environment by setting up the database and seeding it with data.
        /// </summary>
        /// <remarks>This method is called before any tests are run to ensure that the database is in a
        /// known state. It establishes a connection to the database and prepares SQL queries for use in
        /// tests.</remarks>
        [GlobalSetup]
        public void GlobalSetup()
        {
            _db = new SqliteDatabaseFixture();
            _db.EnsureSeeded(RowCount);

            _conn = _db.Connection;
            _hydrix = new Materializer(_conn, parameterPrefix: "$");

            _sqlDapper = @"
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
                LIMIT $take";

            _sqlHydrix = @"
                SELECT
                  u.Id,
                  u.Name,
                  u.Age,
                  u.Status,
                  o.Id   AS ""Order.Id\"",
                  o.Total AS ""Order.Total""
                FROM Users u
                LEFT JOIN Orders o ON o.UserId = u.Id
                ORDER BY u.Id
                LIMIT $take";
        }

        /// <summary>
        /// Releases resources used by the test class after all benchmarks have completed execution.
        /// </summary>
        /// <remarks>Call this method after all benchmark tests have run to ensure that resources such as
        /// the database context are properly disposed. This helps prevent resource leaks and ensures consistent test
        /// environment cleanup.</remarks>
        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _db.Dispose();
        }

        /// <summary>
        /// Retrieves a list of users and their associated orders from the database using Dapper's multi-mapping
        /// feature.
        /// </summary>
        /// <remarks>This method uses Dapper's multi-mapping capability to join user and order data based
        /// on the provided SQL query. The splitOn parameter is set to "Id" to indicate where the
        /// mapping for the order object begins.</remarks>
        /// <returns>A list of <see cref="UserWithOrder"/> objects, each containing a user and their corresponding order.</returns>
        [Benchmark(Baseline = true)]
        public List<UserWithOrder> Dapper_MultiMapping()
        {
            // splitOn uses the column name where the second object starts.
            // Here, the second object starts at the second Id column (o.Id).
            var rows = _conn.Query<UserWithOrder, Order, UserWithOrder>(
                _sqlDapper,
                (u, o) => { u.Order = o; return u; },
                new { take = Take },
                splitOn: "Id");

            return rows.AsList();
        }

        /// <summary>
        /// Retrieves a list of users along with their associated orders from the database.
        /// </summary>
        /// <remarks>This method executes a query against the database using the specified SQL command and
        /// takes a predefined number of records as specified by the Take parameter.</remarks>
        /// <returns>A list of <see cref="UserWithOrder"/> objects representing users and their orders. The list will be empty if
        /// no users are found.</returns>
        [Benchmark]
        public List<UserWithOrder> Hydrix_Nested()
        {
            return _hydrix.Query<UserWithOrder>(_sqlHydrix, new { take = Take }).AsList();
        }

        /// <summary>
        /// Retrieves a list of users along with their associated orders from the database using ADO.NET.
        /// </summary>
        /// <remarks>This method executes a SQL command to fetch user data and their orders, utilizing
        /// parameters to limit the number of results returned. Ensure that the Take property is set
        /// to a valid positive integer to avoid unexpected results.</remarks>
        /// <returns>A list of <see cref="UserWithOrder"/> objects, each containing user details and their corresponding order
        /// information.</returns>
        [Benchmark]
        public List<UserWithOrder> AdoNet_Manual()
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = _sqlDapper;

            var p = cmd.CreateParameter();
            p.ParameterName = "$take";
            p.Value = Take;
            cmd.Parameters.Add(p);

            using var reader = cmd.ExecuteReader();

            // Cache ordinals once.
            var uId = reader.GetOrdinal("Id");
            var uName = reader.GetOrdinal("Name");
            var uAge = reader.GetOrdinal("Age");
            var uStatus = reader.GetOrdinal("Status");

            // For the second object, columns are positional duplicates (Id, Total), so use ordinals by index.
            var oId = 4;
            var oTotal = 5;

            var list = new List<UserWithOrder>(Take);

            while (reader.Read())
            {
                var user = new UserWithOrder
                {
                    Id = reader.GetInt32(uId),
                    Name = reader.GetString(uName),
                    Age = reader.GetInt32(uAge),
                    Status = (UserStatus)reader.GetInt32(uStatus),
                    Order = new Order
                    {
                        Id = reader.GetInt32(oId),
                        Total = reader.GetDouble(oTotal)
                    }
                };

                list.Add(user);
            }

            return list;
        }
    }
}