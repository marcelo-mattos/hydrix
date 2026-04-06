using Microsoft.Data.Sqlite;
using System;

namespace Hydrix.Benchmarks.Infrastructure
{
    /// <summary>
    /// Maintains the in-memory SQLite database used by the benchmark suites for the duration of a benchmark run.
    /// </summary>
    /// <remarks>
    /// The fixture keeps a single open connection alive so the in-memory database survives across setup, execution, and
    /// cleanup, allowing every benchmark implementation to query the same seeded data set.
    /// </remarks>
    public sealed class SqliteDatabaseFixture :
        IDisposable
    {
        /// <summary>
        /// Gets the open SQLite connection backing the benchmark database.
        /// </summary>
        /// <remarks>
        /// Consumers reuse this connection directly when executing raw SQL through Dapper, Hydrix, or manual ADO.NET
        /// commands, while Entity Framework Core builds its context options around the same connection.
        /// </remarks>
        public SqliteConnection Connection { get; }

        /// <summary>
        /// Initializes a new fixture instance, opens the shared in-memory SQLite connection, and creates the schema.
        /// </summary>
        public SqliteDatabaseFixture()
        {
            Connection = new SqliteConnection(
                "Data Source=:memory:;Cache=Shared");
            Connection.Open();

            CreateSchema(
                Connection);
        }

        /// <summary>
        /// Ensures that the database contains exactly the requested number of benchmark rows.
        /// </summary>
        /// <param name="rowCount">
        /// The number of user rows, and therefore order rows, that must exist after seeding completes.
        /// </param>
        /// <remarks>
        /// When the current row count already matches the requested value the method does nothing; otherwise it clears
        /// the existing data set and reseeds the database from scratch so all benchmark suites run against a deterministic
        /// volume of data.
        /// </remarks>
        public void EnsureSeeded(
            int rowCount)
        {
            using var command = Connection.CreateCommand();
            command.CommandText = "SELECT COUNT(1) FROM Users";
            var existing = Convert.ToInt32(
                command.ExecuteScalar());

            if (existing == rowCount)
            {
                return;
            }

            ClearData(
                Connection);
            Seed(
                Connection,
                rowCount);
        }

        /// <summary>
        /// Creates the SQLite tables and supporting index required by the benchmark queries.
        /// </summary>
        /// <param name="connection">
        /// The already-open SQLite connection on which the schema creation script should execute.
        /// </param>
        private static void CreateSchema(
            SqliteConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                  Id     INTEGER NOT NULL,
                  Name   TEXT    NOT NULL,
                  Age    INTEGER NOT NULL,
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
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Removes every existing row from the benchmark tables before reseeding.
        /// </summary>
        /// <param name="connection">
        /// The already-open SQLite connection on which the delete script should execute.
        /// </param>
        private static void ClearData(
            SqliteConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM Orders;
                DELETE FROM Users;
                ";
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Inserts deterministic user and order rows into the in-memory benchmark database.
        /// </summary>
        /// <param name="connection">
        /// The already-open SQLite connection that will receive the seeded data.
        /// </param>
        /// <param name="rowCount">
        /// The number of user rows, and matching order rows, that must be generated.
        /// </param>
        /// <remarks>
        /// The fixture writes one order per user so nested join benchmarks do not multiply rows unexpectedly, keeping the
        /// workload stable across Dapper, Hydrix, Entity Framework Core, and manual ADO.NET implementations.
        /// </remarks>
        private static void Seed(
            SqliteConnection connection,
            int rowCount)
        {
            using var transaction = connection.BeginTransaction();

            using (var userCommand = connection.CreateCommand())
            {
                userCommand.Transaction = transaction;
                userCommand.CommandText =
                    "INSERT INTO Users (Id, Name, Age, Status) VALUES ($id, $name, $age, $status)";

                var idParameter = userCommand.CreateParameter();
                idParameter.ParameterName = "$id";
                userCommand.Parameters.Add(
                    idParameter);

                var nameParameter = userCommand.CreateParameter();
                nameParameter.ParameterName = "$name";
                userCommand.Parameters.Add(
                    nameParameter);

                var ageParameter = userCommand.CreateParameter();
                ageParameter.ParameterName = "$age";
                userCommand.Parameters.Add(
                    ageParameter);

                var statusParameter = userCommand.CreateParameter();
                statusParameter.ParameterName = "$status";
                userCommand.Parameters.Add(
                    statusParameter);

                for (var index = 1; index <= rowCount; index++)
                {
                    idParameter.Value = index;
                    nameParameter.Value = "User " + index;
                    ageParameter.Value = (index % 70) + 10;
                    statusParameter.Value = index % 3;

                    userCommand.ExecuteNonQuery();
                }
            }

            using (var orderCommand = connection.CreateCommand())
            {
                orderCommand.Transaction = transaction;
                orderCommand.CommandText =
                    "INSERT INTO Orders (Id, UserId, Total) VALUES ($id, $userId, $total)";

                var idParameter = orderCommand.CreateParameter();
                idParameter.ParameterName = "$id";
                orderCommand.Parameters.Add(
                    idParameter);

                var userIdParameter = orderCommand.CreateParameter();
                userIdParameter.ParameterName = "$userId";
                orderCommand.Parameters.Add(
                    userIdParameter);

                var totalParameter = orderCommand.CreateParameter();
                totalParameter.ParameterName = "$total";
                orderCommand.Parameters.Add(
                    totalParameter);

                for (var index = 1; index <= rowCount; index++)
                {
                    idParameter.Value = index;
                    userIdParameter.Value = index;
                    totalParameter.Value = (index % 1000) + 0.99;

                    orderCommand.ExecuteNonQuery();
                }
            }

            transaction.Commit();
        }

        /// <summary>
        /// Releases the open SQLite connection held by the fixture.
        /// </summary>
        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}
