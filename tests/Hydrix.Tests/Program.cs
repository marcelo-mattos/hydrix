using Hydrix.Orchestrator.Builders;
using Hydrix.Orchestrator.Materializers;
using Hydrix.Tests.Database.Entity;
using Hydrix.Tests.Database.Procedure;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Hydrix.Tests
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
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            ILogger logger = loggerFactory.CreateLogger("Program");

            var sqlMaterializer =
                new SqlMaterializer(
                    new SqlConnection("Data Source=localhost;Database=HydrixTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"),
                    logger);

            sqlMaterializer.OpenConnection();

            // ----------------- INSERT DATA -----------------

            for (var i = 0; i < 10; i++)
            {
                var customerId = Guid.NewGuid();

                await sqlMaterializer.ExecuteNonQueryAsync(@"
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
                        Id = customerId,
                        Name = Faker.NameFaker.Name(),
                        Birthdate = Faker.DateTimeFaker.BirthDay(18, 65),
                        Level = Faker.NumberFaker.Number(1, 7),
                        Salary = (decimal)Faker.NumberFaker.Number(12000, 75000),
                        IsActive = Faker.BooleanFaker.Boolean()
                    });

                await sqlMaterializer.ExecuteNonQueryAsync(@"
                    INSERT INTO Product (
                        Id,
                        CustomerId,
                        Name,
                        Ean,
                        Quantity,
                        Price
                    ) VALUES (
                        @Id,
                        @CustomerId,
                        @Name,
                        @Ean,
                        @Quantity,
                        @Price  
                    );",
                    new
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = customerId,
                        Name = Faker.StringFaker.AlphaNumeric(50),
                        Ean = Faker.StringFaker.Numeric(13),
                        Quantity = (decimal)Faker.NumberFaker.Number(1, 35),
                        Price = (decimal)Faker.NumberFaker.Number(1, 500)
                    });
            }

            // ----------------- SELECT DATA -----------------

            bool? isActive = true;
            DateTime? startDate = new DateTime(1980, 1, 1);
            DateTime? endDate = new DateTime(1999, 12, 31);
            int[] levels = new int[] { 3, 5, 7 };

            var builder = SqlWhereBuilder.Create();
            var where = builder
                .AndIf(isActive.HasValue, "c.IsActive = @IsActive")
                .AndIf(startDate.HasValue, "c.BirthDate >= @StartDate")
                .AndIf(endDate.HasValue, "c.BirthDate <= @EndDate")
                .AndIf(levels != null && levels.Length > 0, "c.Level IN (@Levels)")
                .Build();

            where = builder
                .Clear()
                    .Where("1 = 1")
                    .AndOrGroupIf(
                        new[]
                        {
                            isActive.HasValue,
                            levels != null,
                            levels.Length > 0
                        },
                        new[]
                        {
                            "c.IsActive = @IsActive",
                            "c.Level IN (@Levels)"
                        })
                    .OrAndGroupIf(
                        new[]
                        {
                            startDate.HasValue,
                            endDate.HasValue,
                        },
                        new[]
                        {
                            "c.BirthDate >= @StartDate",
                            "c.BirthDate <= @EndDate"
                        })
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

            sql = $@"
                SELECT
                    p.Id,
                    p.CustomerId,
                    p.Name,
                    p.Ean,
                    p.Quantity,
                    p.Price,
                    c.Id        as [Customer.Id],
                    c.Name      as [Customer.Name],
                    c.BirthDate as [Customer.BirthDate],
                    c.Level     as [Customer.Level],
                    c.Salary    as [Customer.Salary],
                    c.IsActive  as [Customer.IsActive]
                FROM Product p
                LEFT JOIN Customer c ON p.CustomerId = c.Id
                ORDER BY
                    p.CustomerId;";

            var productResult = await sqlMaterializer.QueryAsync<Product>(
                sql);

            // ----------------- DELETE DATA -----------------

            await sqlMaterializer.ExecuteNonQueryAsync(@"
                DELETE
                FROM [dbo].[Product]
            ");

            await sqlMaterializer.ExecuteNonQueryAsync(new DelCustomers());

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

            await sqlMaterializer.ExecuteNonQueryAsync(addCustomer);

            addCustomer = new AddCustomer()
            {
                Id = Guid.NewGuid(),
                Name = Faker.NameFaker.Name(),
                Birthdate = null,
                Level = Faker.NumberFaker.Number(1, 7),
                Salary = null,
                IsActive = null
            };

            await sqlMaterializer.ExecuteNonQueryAsync(addCustomer);

            // ----------------- SELECT DATA -----------------

            var customer = await sqlMaterializer.QueryAsync<Customer, SqlParameter>(new GetCustomer());

            // ----------------- DELETE DATA -----------------

            await sqlMaterializer.ExecuteNonQueryAsync(new DelCustomers());

            Console.ReadKey();
        }
    }
}