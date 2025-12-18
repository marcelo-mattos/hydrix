using Hydrix.Orchestrator.Builders;
using Hydrix.Orchestrator.Materializers;
using Hydrix.Test.Database.Entity;
using Hydrix.Test.Database.Procedure;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Hydrix.Test
{
    /// <summary>
    /// TLS Data SQL DbClient Test Program
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Program Entry point
        /// </summary>
        private static async Task Main(string[] _)
        {
            var sqlMaterializer =
                new SqlMaterializer(
                    new SqlConnection("Data Source=localhost;Database=HydrixTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"));

            sqlMaterializer.OpenConnection();

            // ----------------- INSERT DATA -----------------

            for (var i = 0; i < 10; i++)
            {
                sqlMaterializer.ExecuteNonQuery($@"
                INSERT INTO Customer (
                    Id,
                    Name,
                    BirthDate,
                    Level,
                    Salary,
                    IsActive
                ) VALUES (
                    @Id,
                    @Name,
                    @BirthDate,
                    @Level,
                    @Salary,
                    @IsActive
                );",
                    new
                    {
                        Id = Guid.NewGuid(),
                        Name = Faker.NameFaker.Name(),
                        Birthdate = Faker.DateTimeFaker.BirthDay(18, 65),
                        Level = Faker.NumberFaker.Number(1, 7),
                        Salary = (decimal)Faker.NumberFaker.Number(12000, 75000),
                        IsActive = Faker.BooleanFaker.Boolean()
                    });
            }

            // ----------------- SELECT DATA -----------------

            bool? isActive = true;
            DateTime? startDate = new DateTime(1980, 1, 1);
            DateTime? endDate = new DateTime(1999, 12, 31);
            int[] levels = new int[] { 3, 5, 7 };

            var where = SqlWhereBuilder
                .Create()
                .AndIf(isActive.HasValue, "c.IsActive = @IsActive")
                .AndIf(startDate.HasValue, "c.BirthDate >= @StartDate")
                .AndIf(endDate.HasValue, "c.BirthDate <= @EndDate")
                .AndIf(levels != null && levels.Length > 0, "c.Level IN (@Levels)")
                .Build();

            var sql = $@"
                SELECT
                    c.Id,
                    c.Name,
                    c.BirthDate,
                    c.Level,
                    c.Salary,
                    c.IsActive
                FROM Customer c
                {where}
                ORDER BY
                    c.Name;";

            var result = await sqlMaterializer.QueryAsync<Customer>(
                sql,
                new
                {
                    IsActive = isActive,
                    StartDate = startDate,
                    EndDate = endDate,
                    Levels = levels
                });

            // ----------------- DELETE DATA -----------------

            sqlMaterializer.ExecuteNonQuery(new DelCustomers());

            // ----------------- INSERT DATA -----------------

            var addCustomer = new AddCustomer()
            {
                Id = Guid.NewGuid(),
                Name = Faker.NameFaker.Name(),
                Birthdate = Faker.DateTimeFaker.BirthDay(18, 65),
                Level = Faker.NumberFaker.Number(1, 7),
                Salary = Faker.NumberFaker.Number(12000, 75000),
                IsActive = Faker.BooleanFaker.Boolean()
            };

            sqlMaterializer.ExecuteNonQuery(addCustomer);

            addCustomer = new AddCustomer()
            {
                Id = Guid.NewGuid(),
                Name = Faker.NameFaker.Name(),
                Birthdate = null,
                Level = Faker.NumberFaker.Number(1, 7),
                Salary = null,
                IsActive = null
            };

            sqlMaterializer.ExecuteNonQuery(addCustomer);

            // ----------------- SELECT DATA -----------------

            var customer = sqlMaterializer.Query<Customer, SqlParameter>(new GetCustomer());

            // ----------------- DELETE DATA -----------------

            sqlMaterializer.ExecuteNonQuery(new DelCustomers());

            Console.ReadKey();
        }
    }
}