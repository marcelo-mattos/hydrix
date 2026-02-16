using FluentValidation;
using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Builders;
using Hydrix.Schemas;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Xunit;

namespace Hydrix.UnitTests.Schemas
{
    /// <summary>
    /// Provides unit tests for validating the behavior of database entities, ensuring they meet specified validation
    /// criteria.
    /// </summary>
    /// <remarks>This class contains tests for the validity of a test entity, including scenarios for both
    /// valid and invalid states. It utilizes FluentValidation for additional validation rules and checks the results of
    /// the validation process.</remarks>
    public class DatabaseEntityTests
    {
        /// <summary>
        /// Represents a database entity with a required name and a value constrained to the range of 1 to 10.
        /// </summary>
        /// <remarks>The Name property must be provided and cannot be null or empty. The Value property
        /// must be between 1 and 10, inclusive.</remarks>
        private class TestEntity : DatabaseEntity
        {
            [Required]
            public string Name { get; set; }

            [Range(1, 10)]
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents a product in the inventory system, containing details such as the product's identifier, name, and
        /// price.
        /// </summary>
        /// <remarks>This class is mapped to the 'products' table in the database schema 'dbo'. It
        /// inherits from the base class 'DatabaseEntity', which provides common properties and methods for database
        /// entities.</remarks>
        [Table("products", Schema = "dbo")]
        private class Product : DatabaseEntity
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [Column("name")]
            public string Name { get; set; }

            [Column("price")]
            public decimal Price { get; set; }
        }

        /// <summary>
        /// Represents a sales order, linking a customer to their order details within the sales schema.
        /// </summary>
        /// <remarks>The Order class includes a foreign key relationship to the Customer entity. The Id
        /// property serves as the primary key for the order, while CustomerId identifies the associated customer. This
        /// class is mapped to the 'orders' table in the 'sales' schema.</remarks>
        [Table("orders", Schema = "sales")]
        private class Order : DatabaseEntity
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [ForeignTable("customers", Alias = "c", Schema = "sales", PrimaryKeys = new[] { "Id" }, ForeignKeys = new[] { "CustomerId" })]
            public Customer Customer { get; set; }

            [Column("customer_id")]
            public int CustomerId { get; set; }
        }

        /// <summary>
        /// Represents a customer in the sales database, containing essential customer information.
        /// </summary>
        /// <remarks>This class is mapped to the 'customers' table in the 'sales' schema. It inherits from
        /// the DatabaseEntity class, which provides common properties for database entities.</remarks>
        [Table("customers", Schema = "sales")]
        private class Customer : DatabaseEntity
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [Column("name")]
            public string Name { get; set; }

            [Column]
            public string Document { get; set; }

            [NotMapped]
            public string Phone { get; set; }
        }

        /// <summary>
        /// Represents a sales agent entity mapped to the 'agents' table in the 'sales' schema.
        /// </summary>
        /// <remarks>Inherits from DatabaseEntity, which may provide additional properties or methods
        /// relevant to database entities.</remarks>
        [Table("agents", Schema = "sales")]
        class Agent : DatabaseEntity
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [Column]
            public string Name { get; set; }

            [NotMapped]
            public string Document { get; set; }
        }

        /// <summary>
        /// Represents an order in the sales database that is associated with both a customer and an agent.
        /// </summary>
        /// <remarks>This class facilitates tracking sales activities by linking each order to a specific
        /// customer and agent. The properties provide access to related entities and their corresponding foreign keys,
        /// enabling integration with relational database operations.</remarks>
        [Table("orders", Schema = "sales")]
        class OrderWithAgent : DatabaseEntity
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [ForeignTable("customers", Alias = "c", Schema = "sales", PrimaryKeys = new[] { "Id" }, ForeignKeys = new[] { "CustomerId" })]
            public Customer Customer { get; set; }

            [ForeignTable("agents", Schema = "sales", PrimaryKeys = new[] { "Id" }, ForeignKeys = new[] { "AgentId" })]
            public Agent Agent { get; set; }

            [Column("customer_id")]
            [Required]
            public int CustomerId { get; set; }

            [Column("agent_id")]
            public int AgentId { get; set; }

            [NotMapped]
            public int Sequential { get; set; }

            [Column]
            public int Hash { get; set; }
        }

        /// <summary>
        /// Provides validation rules for instances of the TestEntity class using FluentValidation.
        /// </summary>
        /// <remarks>This validator ensures that the Name property is not empty and that the Value
        /// property is within the range of 1 to 10. User-friendly error messages are provided when validation fails.
        /// Use this class to enforce data integrity for TestEntity objects before processing or persisting
        /// them.</remarks>
        private class FluentTestValidator : AbstractValidator<TestEntity>
        {
            public FluentTestValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithMessage("Name must not be empty.");

                RuleFor(x => x.Value)
                    .InclusiveBetween(1, 10)
                    .WithMessage("Value must be between 1 and 10.");
            }
        }

        /// <summary>
        /// Verifies that the IsValid method returns true when the entity meets all validation criteria.
        /// </summary>
        /// <remarks>This unit test ensures that a properly initialized TestEntity instance is considered
        /// valid and that no validation errors are reported.</remarks>
        [Fact]
        public void IsValid_ReturnsTrue_WhenEntityIsValid()
        {
            var entity = new TestEntity { Name = "Valid", Value = 5 };
            var result = entity.IsValid(out List<ValidationResult> errors);
            Assert.True(result);
            Assert.Empty(errors);
        }

        /// <summary>
        /// Verifies that the IsValid method returns false when the entity contains invalid property values.
        /// </summary>
        /// <remarks>This test ensures that validation errors are correctly reported for properties that
        /// do not meet the required criteria, such as null or out-of-range values. It checks that the errors list
        /// includes entries for each invalid property.</remarks>
        [Fact]
        public void IsValid_ReturnsFalse_WhenEntityIsInvalid()
        {
            var entity = new TestEntity { Name = null, Value = 0 };
            var result = entity.IsValid(out List<ValidationResult> errors);
            Assert.False(result);
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.MemberNames.Contains("Name"));
            Assert.Contains(errors, e => e.MemberNames.Contains("Value"));
        }

        /// <summary>
        /// Verifies that a TestEntity instance is considered valid by the IsValid method when evaluated with a fluent
        /// validator.
        /// </summary>
        /// <remarks>This test ensures that the IsValid method returns true and produces no validation
        /// errors when the entity meets all validation criteria defined by FluentTestValidator.</remarks>
        [Fact]
        public void IsValid_WithFluentValidator_ReturnsTrue_WhenEntityIsValid()
        {
            var entity = new TestEntity { Name = "Valid", Value = 5 };
            var validator = new FluentTestValidator();
            var result = entity.IsValid<TestEntity>(out List<ValidationResult> errors, validator);
            Assert.True(result);
            Assert.Empty(errors);
        }

        /// <summary>
        /// Verifies that the IsValid method returns false and populates validation errors when the entity does not
        /// satisfy the fluent validation rules.
        /// </summary>
        /// <remarks>This unit test ensures that invalid property values on the entity trigger appropriate
        /// validation messages. It checks that the errors list contains messages for each violated rule, confirming the
        /// validator's effectiveness in identifying invalid input.</remarks>
        [Fact]
        public void IsValid_WithFluentValidator_ReturnsFalse_WhenEntityIsInvalid()
        {
            var entity = new TestEntity { Name = "", Value = 0 };
            var validator = new FluentTestValidator();
            var result = entity.IsValid<TestEntity>(out List<ValidationResult> errors, validator);
            Assert.False(result);
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.ErrorMessage.Contains("Name must not be empty"));
            Assert.Contains(errors, e => e.ErrorMessage.Contains("Value must be between 1 and 10"));
        }

        /// <summary>
        /// Verifies that the BuildQuery method generates a basic SQL SELECT statement for the Product entity without
        /// including any JOIN clauses.
        /// </summary>
        /// <remarks>This test ensures that the generated query retrieves the id, name, and price columns
        /// from the products table and does not reference other tables. It validates that the query structure matches
        /// expectations for a simple entity.</remarks>
        [Fact]
        public void BuildQuery_GeneratesBasicSelect_ForSimpleEntity()
        {
            var entity = new Product();
            var sql = entity.BuildQuery();

            Assert.Contains("SELECT", sql);
            Assert.Contains("FROM dbo.products p", sql);
            Assert.Contains("p.id", sql);
            Assert.Contains("p.name", sql);
            Assert.Contains("p.price", sql);
            Assert.DoesNotContain("JOIN", sql);
        }

        /// <summary>
        /// Verifies that the BuildQuery method generates a SQL SELECT statement for the Product entity using the
        /// specified table alias.
        /// </summary>
        /// <remarks>This test ensures that the generated query includes the correct table alias and
        /// references the expected fields ('id', 'name', and 'price') from the 'products' table. It validates that the
        /// alias is applied consistently throughout the query, which is important for scenarios involving multiple
        /// table joins or dynamic query generation.</remarks>
        [Fact]
        public void BuildQuery_GeneratesSelectWithAlias()
        {
            var entity = new Product();
            var sql = entity.BuildQuery(alias: "p");

            Assert.Contains("FROM dbo.products p", sql);
            Assert.Contains("p.id", sql);
            Assert.Contains("p.name", sql);
            Assert.Contains("p.price", sql);
        }

        /// <summary>
        /// Verifies that the BuildQuery method generates a SQL SELECT statement containing a WHERE clause with the
        /// specified filtering conditions.
        /// </summary>
        /// <remarks>This test ensures that the generated SQL query includes both price and name filters
        /// as defined by the SqlWhereBuilder. It validates that the WHERE clause is correctly constructed and that the
        /// expected conditions appear in the resulting SQL statement.</remarks>
        [Fact]
        public void BuildQuery_GeneratesSelectWithWhereClause()
        {
            var entity = new Product();
            var where = SqlWhereBuilder.Create()
                .And("t0.price > @minPrice")
                .And("t0.name LIKE @name");

            var sql = entity.BuildQuery(where: where);

            Assert.Contains("WHERE", sql);
            Assert.Contains("t0.price > @minPrice", sql);
            Assert.Contains("t0.name LIKE @name", sql);
        }

        /// <summary>
        /// Verifies that the BuildQuery method generates a SQL statement containing a LEFT JOIN between the orders and
        /// customers tables based on the CustomerId foreign key.
        /// </summary>
        /// <remarks>This test ensures that the generated query includes the expected SELECT clause, FROM
        /// clause referencing the sales.orders table, and a LEFT JOIN to the sales.customers table. It also checks that
        /// customer fields are correctly aliased in the result set, confirming that order details are joined with
        /// associated customer information.</remarks>
        [Fact]
        public void BuildQuery_GeneratesJoin_ForForeignTable()
        {
            var entity = new Order();
            var sql = entity.BuildQuery();

            Assert.Contains("SELECT", sql);
            Assert.Contains("FROM sales.orders o", sql);
            Assert.Contains("LEFT JOIN sales.customers c ON o.CustomerId = c.Id", sql);
            Assert.Contains("c.id AS \"customers.id\"", sql);
            Assert.Contains("c.name AS \"customers.name\"", sql);
        }

        /// <summary>
        /// Verifies that the BuildQuery method of the OrderWithAgent entity correctly generates SQL statements with
        /// multiple LEFT JOIN clauses for related foreign tables.
        /// </summary>
        /// <remarks>This test ensures that the generated SQL includes the necessary joins for both
        /// customers and agents, and that the selected columns are properly aliased. It validates that complex entity
        /// relationships are handled accurately in query generation.</remarks>
        [Fact]
        public void BuildQuery_HandlesMultipleForeignTables()
        {

            var entity = new OrderWithAgent();
            var sql = entity.BuildQuery();

            Assert.Contains("INNER JOIN sales.customers c ON owa.CustomerId = c.Id", sql);
            Assert.Contains("LEFT JOIN sales.agents a ON owa.AgentId = a.Id", sql);
            Assert.Contains("c.id AS \"customers.id\"", sql);
            Assert.Contains("a.id AS \"agents.id\"", sql);
        }
    }
}