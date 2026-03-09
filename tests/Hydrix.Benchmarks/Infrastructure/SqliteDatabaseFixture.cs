using Microsoft.Data.Sqlite;
using System;

namespace Hydrix.Benchmarks.Infrastructure
{
    /// <summary>
    /// SQLite in-memory database fixture for benchmarks.
    /// Keeps a single open connection alive for the duration of the benchmark run.
    /// </summary>
    /// <remarks>This class is designed to provide a consistent and isolated SQLite in-memory database environment for benchmarking purposes.
    /// It ensures that the database schema is created and seeded with data before any benchmarks are executed. The single open connection
    /// approach helps maintain the state of the in-memory database throughout the benchmark lifecycle.</remarks>
    public sealed class SqliteDatabaseFixture :
        IDisposable
    {
        /// <summary>
        /// Gets the active SQLite connection used for database operations.
        /// </summary>
        /// <remarks>This property provides access to the underlying SqliteConnection instance, which is
        /// essential for executing commands and managing transactions. Ensure that the connection is open before
        /// performing any database operations.</remarks>
        public SqliteConnection Connection { get; }

        /// <summary>
        /// Initializes a new instance of the SqliteDatabaseFixture class and establishes a connection to an in-memory
        /// SQLite database using a shared cache.
        /// </summary>
        /// <remarks>This constructor opens a single connection to the in-memory database and creates the
        /// required schema. The shared cache mode allows multiple connections to access the same database instance, but
        /// this fixture maintains a single open connection for efficiency and consistency during testing.</remarks>
        public SqliteDatabaseFixture()
        {
            // Shared cache allows multiple connections, but here we keep a single open connection.
            Connection = new SqliteConnection("Data Source=:memory:;Cache=Shared");
            Connection.Open();

            CreateSchema(Connection);
        }

        /// <summary>
        /// Ensures that the database contains the specified number of user records by reseeding if necessary.
        /// </summary>
        /// <remarks>If the number of user records in the database does not match the specified count,
        /// this method removes all existing user data and inserts new records to reach the desired total. Use with
        /// caution, as this operation is destructive to existing user data.</remarks>
        /// <param name="rowCount">The required number of user records to be present in the database. If the current count does not match this
        /// value, all existing user data will be cleared and new records will be seeded.</param>
        public void EnsureSeeded(
            int rowCount)
        {
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM Users";
            var existing = Convert.ToInt32(cmd.ExecuteScalar());

            if (existing == rowCount)
                return;

            ClearData(Connection);
            Seed(Connection, rowCount);
        }

        /// <summary>
        /// Creates the Users and Orders tables in the SQLite database if they do not already exist, including an index
        /// on the UserId column of the Orders table.
        /// </summary>
        /// <remarks>This method ensures that the required database schema is present for user and order
        /// management. It is safe to call multiple times, as existing tables and indexes will not be
        /// recreated.</remarks>
        /// <param name="conn">The open SqliteConnection used to execute the schema creation commands. Must not be null.</param>
        private static void CreateSchema(
            SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                  Id   INTEGER NOT NULL,
                  Name TEXT    NOT NULL,
                  Age  INTEGER NOT NULL,
                  Status INTEGER NOT NULL,
                  PRIMARY KEY (Id)
                );

                CREATE TABLE IF NOT EXISTS Orders (
                  Id     INTEGER NOT NULL,
                  UserId INTEGER NOT NULL,
                  Total  REAL    NOT NULL,
                  PRIMARY KEY (Id)
                );

                CREATE INDEX IF NOT EXISTS IX_Orders_UserId ON Orders(UserId);
                ";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Removes all records from the Orders and Users tables in the database.
        /// </summary>
        /// <remarks>This operation permanently deletes all data from the specified tables. Use with
        /// caution, as the deleted data cannot be recovered.</remarks>
        /// <param name="conn">The open SqliteConnection used to execute the deletion commands. The connection must be valid and open when
        /// this method is called.</param>
        private static void ClearData(
            SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                DELETE FROM Orders;
                DELETE FROM Users;
                ";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Populates the database with a specified number of user and order records for testing or benchmarking
        /// purposes.
        /// </summary>
        /// <remarks>Each user is assigned a unique identifier, a generated name, an age, and a status
        /// value. For every user, a corresponding order is created with a unique identifier and a total value. All
        /// insert operations are performed within a single transaction to ensure atomicity.</remarks>
        /// <param name="conn">The SQLite connection to the database where the records will be inserted. The connection must be open and
        /// valid.</param>
        /// <param name="rowCount">The number of user and order records to insert into the database. Must be a positive integer.</param>
        private static void Seed(
            SqliteConnection conn,
            int rowCount)
        {
            using var tx = conn.BeginTransaction();

            // Users
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "INSERT INTO Users (Id, Name, Age, Status) VALUES ($id, $name, $age, $status)";

                var pId = cmd.CreateParameter(); pId.ParameterName = "$id"; cmd.Parameters.Add(pId);
                var pName = cmd.CreateParameter(); pName.ParameterName = "$name"; cmd.Parameters.Add(pName);
                var pAge = cmd.CreateParameter(); pAge.ParameterName = "$age"; cmd.Parameters.Add(pAge);
                var pStatus = cmd.CreateParameter(); pStatus.ParameterName = "$status"; cmd.Parameters.Add(pStatus);

                for (int i = 1; i <= rowCount; i++)
                {
                    pId.Value = i;
                    pName.Value = "User " + i;
                    pAge.Value = (i % 70) + 10;
                    pStatus.Value = i % 3; // 0..2

                    cmd.ExecuteNonQuery();
                }
            }

            // Orders: 1 order per user to keep JOIN deterministic and avoid 1:N blow-ups.
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "INSERT INTO Orders (Id, UserId, Total) VALUES ($id, $userId, $total)";

                var pId = cmd.CreateParameter(); pId.ParameterName = "$id"; cmd.Parameters.Add(pId);
                var pUserId = cmd.CreateParameter(); pUserId.ParameterName = "$userId"; cmd.Parameters.Add(pUserId);
                var pTotal = cmd.CreateParameter(); pTotal.ParameterName = "$total"; cmd.Parameters.Add(pTotal);

                for (int i = 1; i <= rowCount; i++)
                {
                    pId.Value = i;
                    pUserId.Value = i;
                    pTotal.Value = (i % 1000) + 0.99;

                    cmd.ExecuteNonQuery();
                }
            }

            tx.Commit();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the class, including the underlying database
        /// connection.
        /// </summary>
        /// <remarks>Call this method when the instance is no longer needed to ensure that all associated
        /// resources are properly released. Failing to call this method may result in resource leaks.</remarks>
        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}