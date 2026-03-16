using Hydrix.Extensions;
using Hydrix.Orchestrator.Builders.Query.Conditions;
using Hydrix.Orchestrator.Materializers;
using Hydrix.Tests.Database.Entity;
using Hydrix.Tests.Database.Procedure;
using Hydrix.Tests.Resources;
using Hydrix.Tests.Validators;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

namespace Hydrix.Tests
{
    internal class App
    {
        private readonly ILogger<App> _logger;

        public App(ILogger<App> logger)
        {
            _logger = logger;
        }

        public async Task RunAsync()
        {
            var factory = new ResourceManagerStringLocalizerFactory(
                Options.Create(new LocalizationOptions { ResourcesPath = "Resources" }),
                NullLoggerFactory.Instance);

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

            var assemblyName = new AssemblyName(typeof(Shared).GetTypeInfo().Assembly.FullName);
            var localizer = factory.Create(nameof(Shared), assemblyName.Name);

            var connection = new SqlConnection("Data Source=localhost;Database=HydrixTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            connection.Open();

            // ----------------- INSERT DATA -----------------

            for (var i = 0; i < 10; i++)
            {
                var customerId = Guid.NewGuid();
                var type = Faker.StringFaker.SelectFrom(1, "OXK");
                var token = type switch
                {
                    "O" => Faker.NumberFaker.Number(100000, 999999).ToString().PadLeft(6, '0'),
                    "X" => string.Empty,
                    "K" => Faker.DateTimeFaker.BirthDay(18, 65).ToString("yyyy-MM-dd"),
                    _ => throw new InvalidOperationException("Invalid type")
                };

                var product = new Product()
                {
                    Id = Guid.NewGuid(),
                    CustomerId = i % 3 == 0 ? (Guid?)null : customerId,
                    Name = Faker.StringFaker.AlphaNumeric(50),
                    Ean = Faker.StringFaker.Numeric(13),
                    Quantity = (decimal)Faker.NumberFaker.Number(1, 35),
                    Price = (decimal)Faker.NumberFaker.Number(1, 500),
                    Type = type,
                    Token = token
                };

                var productValidator = new ProductValidator(localizer);
                if (!product.IsValid(out var validationResults, productValidator))
                {
                    _logger.LogError(
                        "Product validation failed: {Errors}",
                        string.Join(", ", validationResults));

                    Console.ReadKey();
                    continue;
                }

                await connection.ExecuteAsync(@"
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

                await connection.ExecuteAsync(@"
                    INSERT INTO Product (
                        Id,
                        CustomerId,
                        Name,
                        Ean,
                        Quantity,
                        Price,
                        Type,
                        Token
                    ) VALUES (
                        @Id,
                        @CustomerId,
                        @Name,
                        @Ean,
                        @Quantity,
                        @Price,
                        @Type,
                        @Token
                    );",
                    new
                    {
                        product.Id,
                        product.CustomerId,
                        product.Name,
                        product.Ean,
                        product.Quantity,
                        product.Price,
                        product.Type,
                        product.Token
                    });
            }

            // ----------------- SELECT DATA -----------------

            bool? isActive = true;
            DateTime? startDate = new DateTime(1980, 1, 1);
            DateTime? endDate = new DateTime(1999, 12, 31);
            int[] levels = new int[] { 3, 5, 7 };

            var builder = WhereBuilder.Create();
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
                            "(c.IsActive IS NULL OR c.IsActive = @IsActive)",
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

            var result = await connection.QueryAsync<Customer>(
                sql,
                new
                {
                    IsActive = isActive,
                    StartDate = startDate,
                    EndDate = endDate,
                    Levels = levels
                });

            sql = Product.BuildQuery<Product>(builder);
            var productResult = await connection.QueryAsync<Product>(
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
                    p.Type,
                    p.Token,
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

            productResult = await connection.QueryAsync<Product>(
                sql);

            // ----------------- DELETE DATA -----------------

            await connection.ExecuteAsync(@"
                DELETE
                FROM [dbo].[Product]
            ");

            var sqlMaterializer = new Materializer(
                connection,
                logger: _logger);
            sqlMaterializer.OpenConnection();

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
