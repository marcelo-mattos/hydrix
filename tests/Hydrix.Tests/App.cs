using Hydrix.Builders.Query.Conditions;
using Hydrix.Mapper.Extensions;
using Hydrix.Tests.Database.Dto;
using Hydrix.Tests.Database.Entity;
using Hydrix.Tests.Database.Procedure;
using Hydrix.Tests.Resources;
using Hydrix.Tests.Validators;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hydrix.Tests
{
    internal class App
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new JsonSerializerOptions()
            {
                IgnoreNullValues = false,
                WriteIndented = true
            };

        private readonly ILogger<App> _logger;

        public App(ILogger<App> logger)
        {
            _logger = logger;
        }

        [SuppressMessage(
                "Security",
                "S2068:Credentials should not be hard-coded",
                Justification = "Connection string is used only for local development/testing environment. No sensitive production credentials are exposed.")]
        public async Task RunAsync()
        {
            var factory = new ResourceManagerStringLocalizerFactory(
                Options.Create(new LocalizationOptions { ResourcesPath = "Resources" }),
                NullLoggerFactory.Instance);

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

            var assemblyName = new AssemblyName(typeof(Shared).GetTypeInfo().Assembly.FullName);
            var localizer = factory.Create(nameof(Shared), assemblyName.Name);

#if SQLSERVER_ENV_ENABLED
            await using var connection = new SqlConnection(
                "Data Source=localhost;Database=HydrixTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

            const string IsActiveColumn = nameof(Customer.IsActive);
            const string CustomerIdColumn = nameof(Product.CustomerId);
#else
            using var connection = new NpgsqlConnection(
                "Host=localhost;Port=5432;Database=hydrix_test;Username=user_hydrix_test;Password=f77624cd-84c0-4ebd-b9de-3fe08f3401b2");

            const string IsActiveColumn = "is_active";
            const string CustomerIdColumn = "customer_id";
#endif
            await connection.OpenAsync();
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
                if (!product.IsValid<Product>(
                    out var validationResults,
                    entity => productValidator
                        .Validate(entity)
                        .Errors
                        .Select(error =>
                            new ValidationResult(
                                error.ErrorMessage,
                                new[] { error.PropertyName }))))
                {
                    _logger.LogError(
                        "Product validation failed: {Errors}",
                        string.Join(", ", validationResults));

                    Console.ReadKey();
                    continue;
                }

                var sqlQuery = @$"
                    INSERT INTO Customer (
                        Id,
                        Name,
                        BirthDate,
                        Level,
                        Salary,
                        {IsActiveColumn}
                    ) VALUES (
                        @Id,
                        @Name,
                        @BirthDate,
                        @Level,
                        @Salary,
                        @IsActive
                    );";

                await connection.ExecuteAsync(
                    sqlQuery,
                    new
                    {
                        Id = customerId,
                        Name = Faker.NameFaker.Name(),
                        Birthdate = Faker.DateTimeFaker.BirthDay(18, 65),
                        Level = Faker.NumberFaker.Number(1, 7),
                        Salary = (decimal)Faker.NumberFaker.Number(12000, 75000),
                        IsActive = Faker.BooleanFaker.Boolean()
                    });

                sqlQuery = @$"
                    INSERT INTO Product (
                        Id,
                        {CustomerIdColumn},
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
                    );";

                await connection.ExecuteAsync(
                    sqlQuery,
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
            DateTime? startDate = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
            DateTime? endDate = new DateTime(1999, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);
            int[] levels = RandomNumberGenerator.GetInt32(1, 8) == 1
                ? new int[] { 3, 5, 7 }
                : (int[])null;

            var builder = WhereBuilder.Create();
            var where = builder
                .AndIf(isActive.HasValue, $"c.{IsActiveColumn} = @IsActive")
                .AndIf(startDate.HasValue, "c.BirthDate >= @StartDate")
                .AndIf(endDate.HasValue, "c.BirthDate <= @EndDate")
                .AndIf(levels != null && levels.Length > 0, "c.Level IN (@Levels)")
                .Build();
            Console.WriteLine(where);

            levels = new int[] { 3, 5, 7 };
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
                        $"(c.{IsActiveColumn} IS NULL OR c.{IsActiveColumn} = @IsActive)",
                        "c.Level IN (@Levels)")
                    .OrAndGroupIf(
                        new[]
                        {
                            startDate.HasValue,
                            endDate.HasValue
                        },
                        "c.BirthDate >= @StartDate",
                        "c.BirthDate <= @EndDate")
                .Build();

            var sql = $@"
                SELECT
                    c.Id,
                    c.Name,
                    c.BirthDate,
                    c.Level,
                    c.Salary,
                    c.{IsActiveColumn}
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
            Console.WriteLine($"Customer Count: {result.Count()}");

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
            Console.WriteLine($"Product Count: {productResult.Count()}");

            sql = $@"
                SELECT
                    p.Id,
                    p.{CustomerIdColumn},
                    p.Name,
                    p.Ean,
                    p.Quantity,
                    p.Price,
                    p.Type,
                    p.Token,
                    c.Id        as ""Customer.Id"",
                    c.Name      as ""Customer.Name"",
                    c.BirthDate as ""Customer.BirthDate"",
                    c.Level     as ""Customer.Level"",
                    c.Salary    as ""Customer.Salary"",
                    c.{IsActiveColumn} as ""Customer.{IsActiveColumn}""
                FROM Product p
                LEFT JOIN Customer c ON p.{CustomerIdColumn} = c.Id
                ORDER BY
                    p.{CustomerIdColumn};";

            productResult = await connection.QueryAsync<Product>(
                sql);
            Console.WriteLine($"Product Count: {productResult.Count()}");

            sql = $@"
                SELECT
                    p.Id,
                    p.{CustomerIdColumn},
                    p.Name,
                    p.Ean,
                    p.Quantity,
                    p.Price,
                    p.Type,
                    p.Token,
                    c.Id        as ""Customer.Id"",
                    c.Name      as ""Customer.Name"",
                    c.BirthDate as ""Customer.BirthDate"",
                    c.Level     as ""Customer.Level"",
                    c.Salary    as ""Customer.Salary"",
                    c.{IsActiveColumn} as ""Customer.{IsActiveColumn}""
                FROM Product p
                LEFT JOIN Customer c ON p.{CustomerIdColumn} = c.Id
                WHERE
                    c.{IsActiveColumn} = @IsActive
                ORDER BY
                    p.{CustomerIdColumn};";
            productResult = await connection.QueryAsync<Product>(
                sql,
#if SQLSERVER_ENV_ENABLED
                new List<SqlParameter>()
                {
                    new SqlParameter()
#else
                new List<NpgsqlParameter>()
                {
                    new NpgsqlParameter()
#endif
                    {
                        ParameterName = "@IsActive",
                        DbType = System.Data.DbType.Boolean,
                        Value = false
                    }
                });

            var productDto = productResult.ToDtoList<ProductDto>();
            Console.WriteLine($"{JsonSerializer.Serialize(productDto, JsonOptions)}");

            // ----------------- DELETE DATA -----------------

#if SQLSERVER_ENV_ENABLED
            sql = @"
                DELETE
                FROM [dbo].[Product]";
#else
            sql = @"
                DELETE
                FROM public.product";
#endif
            await connection.ExecuteAsync(sql);

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

            await connection.ExecuteAsync(addCustomer);

            addCustomer = new AddCustomer()
            {
                Id = Guid.NewGuid(),
                Name = Faker.NameFaker.Name(),
                Birthdate = null,
                Level = Faker.NumberFaker.Number(1, 7),
                Salary = null,
                IsActive = null
            };

            await connection.ExecuteAsync(addCustomer);

            // ----------------- SELECT DATA -----------------

            var customers = await connection.QueryAsync<
                Customer,
#if SQLSERVER_ENV_ENABLED
                SqlParameter>(new GetCustomer());
#else
                NpgsqlParameter>(new GetCustomer());
#endif
            Console.WriteLine($"Customer Count: {customers.Count()}");

            await connection.ExecuteAsync(new DelCustomers());

            customers = await connection.QueryAsync<
                Customer,
#if SQLSERVER_ENV_ENABLED
                SqlParameter>(new GetCustomer());
#else
                NpgsqlParameter>(new GetCustomer());
#endif
            Console.WriteLine($"Customer Count: {customers.Count()}");

            // ----------------- DELETE DATA -----------------

            await connection.ExecuteAsync(new DelCustomers());

            Console.ReadKey();
        }
    }
}
