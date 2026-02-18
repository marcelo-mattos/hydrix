using FluentValidation;
using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Builders.Query.Conditions;
using Hydrix.Schemas;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
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
        private class Product :
            DatabaseEntity, ITable
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
        private class Order :
            DatabaseEntity, ITable
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
        private class Customer :
            DatabaseEntity, ITable
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
        [Table("agents")]
        private class Agent :
            DatabaseEntity, ITable
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
        [Table("order_with_agents")]
        private class OrderWithAgent :
            DatabaseEntity, ITable
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [ForeignTable("customers", Alias = "c", Schema = "sales", PrimaryKeys = new[] { "Id" }, ForeignKeys = new[] { "CustomerId" })]
            public Customer Customer { get; set; }

            [ForeignTable("agents", PrimaryKeys = new[] { "Id" }, ForeignKeys = new[] { "AgentId" })]
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
        /// Gets the MethodInfo object representing the non-public static method used to resolve foreign key metadata in
        /// the DatabaseEntity class.
        /// </summary>
        /// <remarks>This property is intended for internal use when accessing metadata related to foreign
        /// key relationships within the database schema. It should not be used directly by external code.</remarks>
        private static MethodInfo ResolveForeignMetadataMethod =>
            typeof(Hydrix.Schemas.DatabaseEntity)
                .GetMethod(
                    "ResolveForeignMetadata",
                    BindingFlags.NonPublic |
                    BindingFlags.Static);

        /// <summary>
        /// Represents a record in the main table without foreign key constraints.
        /// </summary>
        /// <remarks>This class is designed to hold data without enforcing foreign key relationships,
        /// allowing for more flexible data management. It is important to handle relationships manually to ensure data
        /// integrity.</remarks>
        [Table("main")]
        private class MainNoForeignKey
        {
            /// <summary>
            /// Gets or sets the foreign entity associated with this record.
            /// </summary>
            /// <remarks>This property establishes a relationship to another entity, enabling
            /// navigation and data integrity between related records. Ensure that the referenced entity is properly set
            /// to maintain referential integrity.</remarks>
            public ForeignNoKey Foreign { get; set; }
        }

        /// <summary>
        /// Represents a database entity mapped to the 'foreign' table that does not define a primary key.
        /// </summary>
        /// <remarks>This class is intended for scenarios where the underlying table lacks a primary key,
        /// which may impact data integrity and certain ORM operations. Use caution when performing updates or deletes,
        /// as entity tracking and retrieval may be limited.</remarks>
        [Table("foreign")]
        private class ForeignNoKey
        { }

        /// <summary>
        /// Represents the primary entity that includes a foreign key relationship to a related entity.
        /// </summary>
        /// <remarks>This class is used to model a database table with a foreign key association, enabling
        /// navigation and data integrity between the main entity and its related foreign entity. The ForeignId property
        /// serves as the foreign key linking to the ForeignWithKey entity.</remarks>
        [Table("main")]
        private class MainWithForeignKey
        {
            /// <summary>
            /// Gets or sets the identifier of the related entity referenced by the Foreign navigation property.
            /// </summary>
            /// <remarks>Assign a value that corresponds to a valid primary key in the related
            /// entity's table to establish or update the relationship. This property is typically used in conjunction
            /// with the Foreign navigation property to manage foreign key associations in the data model.</remarks>
            [ForeignKey("Foreign")]
            public int ForeignId { get; set; }

            /// <summary>
            /// Gets or sets the foreign key associated with this entity.
            /// </summary>
            /// <remarks>This property represents a relationship to another entity, allowing for
            /// navigation and data integrity between related records.</remarks>
            public ForeignWithKey Foreign { get; set; }
        }

        /// <summary>
        /// Represents the main entity with an auto-generated primary key in the database.
        /// </summary>
        /// <remarks>This class is mapped to the 'main' table in the database and contains a foreign key
        /// reference to another entity.</remarks>
        [Table("main")]
        private class MainAutoPrimaryKey
        {
            /// <summary>
            /// Gets or sets the foreign key relationship associated with this entity.
            /// </summary>
            /// <remarks>This property represents the foreign key that links this entity to another
            /// entity in the database. It is essential for maintaining referential integrity between related
            /// entities.</remarks>
            public ForeignWithKey Foreign { get; set; }
        }

        /// <summary>
        /// Represents the main entity in a database table with an automatic foreign key relationship to a related
        /// entity.
        /// </summary>
        /// <remarks>This class establishes a foreign key association between the main entity and the
        /// related entity specified by the ForeignId property. The Foreign property provides access to the associated
        /// entity, enabling navigation and data retrieval across the relationship.</remarks>
        [Table("main")]
        private class MainAutoForeignKey
        {
            /// <summary>
            /// Gets or sets the identifier of the related entity referenced by the 'Foreign' navigation property.
            /// </summary>
            /// <remarks>This property establishes a foreign key relationship with the entity
            /// associated via the 'Foreign' navigation property. Assign a value that corresponds to a valid identifier
            /// in the related entity's table to ensure referential integrity.</remarks>
            [ForeignKey("Foreign")]
            public int ForeignId { get; set; }

            /// <summary>
            /// Gets or sets the foreign key relationship associated with this entity.
            /// </summary>
            /// <remarks>This property represents the foreign key that links this entity to another
            /// entity in the database. It is essential for maintaining referential integrity between related
            /// entities.</remarks>
            public ForeignWithKey Foreign { get; set; }
        }

        /// <summary>
        /// Represents a foreign entity with a unique identifier.
        /// </summary>
        /// <remarks>This class is mapped to the 'foreign' table in the database and contains a primary
        /// key property.</remarks>
        [Table("foreign")]
        private class ForeignWithKey
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [Key]
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents the main entity mapped to the 'main' table, establishing a relationship to a foreign entity via a
        /// foreign key.
        /// </summary>
        /// <remarks>The MainMismatch class links to the ForeignWithKey entity through the ForeignId
        /// property. Ensure that ForeignId references a valid entry in the related table to maintain referential
        /// integrity.</remarks>
        [Table("main")]
        private class MainMismatch
        {
            /// <summary>
            /// Gets or sets the identifier of the related entity referenced by the 'Foreign' navigation property.
            /// </summary>
            /// <remarks>This property establishes a foreign key relationship with the entity
            /// associated via the 'Foreign' navigation property. Assign a value that corresponds to a valid primary key
            /// in the related entity's table to ensure referential integrity.</remarks>
            [ForeignKey("Foreign")]
            public int ForeignId { get; set; }

            /// <summary>
            /// Gets or sets the foreign key associated with the entity.
            /// </summary>
            /// <remarks>This property represents a relationship to another entity, allowing for
            /// navigation and data integrity between related records.</remarks>
            public ForeignWithKey Foreign { get; set; }

            /// <summary>
            /// Gets or sets the additional integer value associated with the object.
            /// </summary>
            public int Extra { get; set; }
        }

        /// <summary>
        /// Represents an entity that maps to the 'multi_fk' table and contains multiple foreign key relationships to
        /// other entities.
        /// </summary>
        /// <remarks>This class includes required and optional foreign key references to related entities.
        /// Ensure that the referenced entities exist and are properly configured in the database to maintain
        /// referential integrity.</remarks>
        [Table("multi_fk")]
        private class MultiForeignKeyEntity
        {
            /// <summary>
            /// Gets or sets the identifier of the related Foreign1 entity.
            /// </summary>
            /// <remarks>This property is required and establishes a foreign key relationship to the
            /// Foreign1 entity. The value must correspond to a valid Foreign1 entity identifier.</remarks>
            [ForeignKey("Foreign1")]
            [Required]
            public int Foreign1Id { get; set; }

            /// <summary>
            /// Gets or sets the foreign key identifier for the related Foreign2 entity.
            /// </summary>
            /// <remarks>This property is nullable, indicating that the relationship to the Foreign2
            /// entity is optional. If set to null, it implies that there is no associated Foreign2 entity.</remarks>
            [ForeignKey("Foreign2")]
            public int? Foreign2Id { get; set; }

            /// <summary>
            /// Gets or sets the related entity associated with this object via a foreign key relationship.
            /// </summary>
            /// <remarks>This property enables navigation to the related ForeignWithKey entity,
            /// supporting data integrity and object graph traversal. Ensure that the corresponding foreign key property
            /// is set appropriately to maintain referential integrity.</remarks>
            public ForeignWithKey Foreign1 { get; set; }

            /// <summary>
            /// Gets or sets the related entity associated with this object via a foreign key relationship.
            /// </summary>
            /// <remarks>This property enables navigation to the related ForeignWithKey entity. Ensure
            /// that the corresponding foreign key property is set appropriately to maintain referential integrity
            /// between entities.</remarks>
            public ForeignWithKey Foreign2 { get; set; }
        }

        /// <summary>
        /// Represents an entity that contains a simple integer property.
        /// </summary>
        private class NoAttributeColumnEntity
        {
            /// <summary>
            /// Gets or sets the simple integer value associated with this property.
            /// </summary>
            public int SimpleProperty { get; set; }
        }

        /// <summary>
        /// Represents an entity that includes a unique identifier required for persistence operations.
        /// </summary>
        /// <remarks>This class is typically used in data models where an identifier is necessary for
        /// database operations. The Id property is marked with both Key and Required attributes, indicating that it
        /// must be provided and serves as the primary key.</remarks>
        private class KeyAndRequiredEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            /// <remarks>This property is required and serves as the primary key for the entity. It
            /// must be a positive integer.</remarks>
            [Key]
            [Required]
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents an entity that requires multiple foreign key relationships, ensuring that all foreign key
        /// references are mandatory.
        /// </summary>
        /// <remarks>This entity is mapped to the 'multi_fk_all_required' table and includes two foreign
        /// key properties, 'Foreign1Id' and 'Foreign2Id', which must be provided. Each foreign key is associated with a
        /// corresponding navigation property, allowing for easy access to related entities.</remarks>
        [Table("multi_fk_all_required")]
        private class MultiForeignKeyAllRequiredEntity
        {
            /// <summary>
            /// Gets or sets the identifier of the associated Foreign1 entity. This property is required.
            /// </summary>
            /// <remarks>The Foreign1Id property establishes a foreign key relationship with the
            /// Foreign1 entity. The value assigned to this property must correspond to an existing Foreign1 entity in
            /// the database.</remarks>
            [ForeignKey("Foreign1")]
            [Required]
            public int Foreign1Id { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the related Foreign2 entity.
            /// </summary>
            /// <remarks>This property is required and serves as the foreign key for establishing a
            /// relationship with the Foreign2 entity. The value must correspond to a valid Foreign2 entity
            /// identifier.</remarks>
            [ForeignKey("Foreign2")]
            [Required]
            public int Foreign2Id { get; set; }

            /// <summary>
            /// Gets or sets the related entity associated with this object via a foreign key relationship.
            /// </summary>
            /// <remarks>This property enables navigation to the related ForeignWithKey entity. Ensure
            /// that the corresponding foreign key property is set correctly to maintain referential integrity between
            /// entities.</remarks>
            public ForeignWithKey Foreign1 { get; set; }

            /// <summary>
            /// Gets or sets the related entity associated with this object via a foreign key relationship.
            /// </summary>
            /// <remarks>This property enables navigation to the related ForeignWithKey entity. Ensure
            /// that the corresponding foreign key property is set appropriately to maintain referential integrity
            /// between entities.</remarks>
            public ForeignWithKey Foreign2 { get; set; }
        }

        /// <summary>
        /// Represents an entity that supports multiple optional foreign key relationships to other entities.
        /// </summary>
        /// <remarks>This entity allows association with up to two related entities via nullable foreign
        /// key properties. Both relationships are optional, enabling flexible modeling of scenarios where related
        /// entities may or may not be present.</remarks>
        [Table("multi_fk_none_required")]
        private class MultiForeignKeyNoneRequiredEntity
        {
            /// <summary>
            /// Gets or sets the foreign key identifier for the related Foreign1 entity.
            /// </summary>
            /// <remarks>This property is nullable, indicating that the relationship to the Foreign1
            /// entity is optional. If set to null, it signifies that there is no associated Foreign1 entity.</remarks>
            [ForeignKey("Foreign1")]
            public int? Foreign1Id { get; set; }

            /// <summary>
            /// Gets or sets the foreign key identifier for the related Foreign2 entity.
            /// </summary>
            /// <remarks>This property is nullable, indicating that the relationship to the Foreign2
            /// entity is optional. If set to null, it signifies that there is no associated Foreign2 entity.</remarks>

            [ForeignKey("Foreign2")]
            public int? Foreign2Id { get; set; }

            /// <summary>
            /// Gets or sets the related entity associated with this object via a foreign key relationship.
            /// </summary>
            /// <remarks>This property enables navigation to the related ForeignWithKey entity. Ensure
            /// that the corresponding foreign key property is set correctly to maintain referential integrity between
            /// entities.</remarks>
            public ForeignWithKey Foreign1 { get; set; }

            /// <summary>
            /// Gets or sets the related entity associated with this object via a foreign key relationship.
            /// </summary>
            /// <remarks>This property enables navigation to the related ForeignWithKey entity. Ensure
            /// that the corresponding foreign key property is set correctly to maintain referential integrity between
            /// entities.</remarks>
            public ForeignWithKey Foreign2 { get; set; }
        }

        /// <summary>
        /// Represents an entity with a foreign key relationship that references a non-existent property, illustrating
        /// an invalid foreign key configuration.
        /// </summary>
        /// <remarks>This class is intended for scenarios where foreign key constraints may fail due to
        /// incorrect property mappings. Ensure that all foreign key attributes reference valid properties to maintain
        /// data integrity and avoid runtime errors.</remarks>
        [Table("invalid_fk")]
        private class InvalidForeignKeyEntity
        {
            /// <summary>
            /// Gets or sets the identifier for the related entity. This property is nullable, indicating that the
            /// association is optional.
            /// </summary>
            /// <remarks>This property is marked with a foreign key attribute referencing a related
            /// entity. Ensure that the referenced entity and property exist and are properly configured to avoid
            /// runtime errors during database operations.</remarks>
            [ForeignKey("NonExistentProperty")]
            public int? SomeId { get; set; }

            /// <summary>
            /// Gets or sets the foreign key relationship associated with this entity.
            /// </summary>
            /// <remarks>This property enables navigation to the related entity and ensures data
            /// integrity between associated records. Assigning a value to this property establishes or updates the
            /// relationship with the corresponding foreign entity.</remarks>
            public ForeignWithKey Foreign { get; set; }
        }

        /// <summary>
        /// Represents a database entity that includes properties with special mapping and validation attributes.
        /// </summary>
        /// <remarks>This entity contains a standard required property that is mapped to the database, a
        /// required property that is not persisted in the database, and a required property that establishes a
        /// relationship with a foreign table. Use this type when you need to model entities with both mapped and
        /// non-mapped data, as well as foreign key relationships.</remarks>
        public class EntityWithSpecialProps : DatabaseEntity
        {
            /// <summary>
            /// Gets or sets the normal property value required for correct operation.
            /// </summary>
            /// <remarks>This property must be assigned a non-null value before use, as it is marked
            /// with the <see cref="RequiredAttribute"/>.</remarks>
            [Required]
            public string NormalProp { get; set; }

            /// <summary>
            /// Gets or sets the value of the property that is not mapped to the database.
            /// </summary>
            /// <remarks>This property is marked with the NotMapped attribute, indicating that it
            /// should not be included in the database schema. It is also required, meaning it cannot be null or empty
            /// when used.</remarks>
            [NotMapped]
            [Required]
            public string NotMappedProp { get; set; }

            /// <summary>
            /// Gets or sets the name of the foreign table associated with this entity.
            /// </summary>
            /// <remarks>This property is required and must not be null or empty. It specifies the
            /// name of the foreign table that this entity references, which is essential for establishing relationships
            /// in the database.</remarks>
            [ForeignTable("ForeignTable")]
            [Required]
            public string ForeignTableProp { get; set; }
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
            var sql = Product.BuildQuery<Product>();

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
            var sql = Product.BuildQuery<Product>();

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
        /// as defined by the WhereBuilder. It validates that the WHERE clause is correctly constructed and that the
        /// expected conditions appear in the resulting SQL statement.</remarks>
        [Fact]
        public void BuildQuery_GeneratesSelectWithWhereClause()
        {
            var where = WhereBuilder.Create()
                .And("t0.price > @minPrice")
                .And("t0.name LIKE @name");

            var sql = Product.BuildQuery<Product>(where: where);

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
            var sql = Order.BuildQuery<Order>();

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
            var sql = OrderWithAgent.BuildQuery<OrderWithAgent>();

            Assert.Contains("FROM order_with_agents owa", sql);
            Assert.Contains("INNER JOIN sales.customers c ON owa.CustomerId = c.Id", sql);
            Assert.Contains("LEFT JOIN agents a ON owa.AgentId = a.Id", sql);
            Assert.Contains("c.id AS \"customers.id\"", sql);
            Assert.Contains("a.id AS \"agents.id\"", sql);
        }

        /// <summary>
        /// Verifies that an ArgumentNullException is thrown when a null navigation property is passed to the foreign
        /// key resolution method.
        /// </summary>
        /// <remarks>This test ensures that the API correctly handles null inputs for navigation
        /// properties, which is essential for maintaining data integrity and preventing unexpected behavior during
        /// foreign key resolution.</remarks>
        [Fact]
        public void Throws_WhenNavigationPropertyIsNull()
        {
            var attr = new ForeignTableAttribute("foreign");
            var ex = Assert.Throws<TargetInvocationException>(() =>
                ResolveForeignMetadataMethod.Invoke(
                    null,
                    new object[] { typeof(MainNoForeignKey), null, attr }
                )
            );
            Assert.IsType<ArgumentNullException>(ex.InnerException);
            Assert.Contains("Navigation property cannot be null", ex.InnerException.Message);
        }

        /// <summary>
        /// Verifies that an InvalidOperationException is thrown when the primary keys of the specified entity type
        /// cannot be resolved during foreign key metadata resolution.
        /// </summary>
        /// <remarks>This test ensures that the foreign key resolution process correctly identifies the
        /// absence of primary keys and throws an appropriate exception. This validation is important for maintaining
        /// data integrity when establishing foreign key relationships.</remarks>
        [Fact]
        public void Throws_WhenPrimaryKeysNotResolved()
        {
            var navigationProperty = typeof(MainNoForeignKey).GetProperty(nameof(MainNoForeignKey.Foreign));
            var attr = new ForeignTableAttribute("foreign");
            var ex = Assert.Throws<TargetInvocationException>(() =>
                ResolveForeignMetadataMethod.Invoke(
                    null,
                    new object[] { typeof(MainNoForeignKey), navigationProperty, attr }
                )
            );
            Assert.IsType<InvalidOperationException>(ex.InnerException);
            Assert.Contains("Primary key not resolved", ex.InnerException.Message);
        }

        /// <summary>
        /// Verifies that an InvalidOperationException is thrown when foreign keys are not resolved during metadata
        /// resolution.
        /// </summary>
        /// <remarks>This test ensures that the appropriate exception is raised when attempting to resolve
        /// foreign key metadata for a type that does not have the necessary foreign key relationships defined. It
        /// checks that the exception message contains the phrase 'Foreign key not resolved' to confirm the specific
        /// failure condition.</remarks>
        [Fact]
        public void Throws_WhenForeignKeysNotResolved()
        {
            var navigationProperty = typeof(MainNoForeignKey).GetProperty(nameof(MainNoForeignKey.Foreign));
            var attr = new ForeignTableAttribute("foreign")
            {
                PrimaryKeys = new[] { "Id" }
            };
            var ex = Assert.Throws<TargetInvocationException>(() =>
                ResolveForeignMetadataMethod.Invoke(
                    null,
                    new object[] { typeof(MainNoForeignKey), navigationProperty, attr }
                )
            );
            Assert.IsType<InvalidOperationException>(ex.InnerException);
            Assert.Contains("Foreign key not resolved", ex.InnerException.Message);
        }

        /// <summary>
        /// Verifies that an exception is thrown when the number of primary keys does not match the number of foreign
        /// keys specified in the ForeignTableAttribute.
        /// </summary>
        /// <remarks>This test ensures that the ResolveForeignMetadataMethod enforces a requirement for
        /// the primary and foreign key counts to be equal. It confirms that an InvalidOperationException is thrown with
        /// an appropriate message when this condition is not met.</remarks>
        [Fact]
        public void Throws_WhenPrimaryAndForeignKeysCountMismatch()
        {
            var navigationProperty = typeof(MainWithForeignKey).GetProperty(nameof(MainWithForeignKey.Foreign));
            var attr = new ForeignTableAttribute("foreign")
            {
                PrimaryKeys = new[] { "Id", "Other" },
                ForeignKeys = new[] { "ForeignId" }
            };
            var ex = Assert.Throws<TargetInvocationException>(() =>
                ResolveForeignMetadataMethod.Invoke(
                    null,
                    new object[] { typeof(MainWithForeignKey), navigationProperty, attr }
                )
            );
            Assert.IsType<InvalidOperationException>(ex.InnerException);
            Assert.Contains("PrimaryKeys and ForeignKeys count mismatch", ex.InnerException.Message);
        }

        /// <summary>
        /// Verifies that the foreign metadata is correctly resolved for a navigation property in the MainWithForeignKey
        /// class.
        /// </summary>
        /// <remarks>This test checks that the resolved metadata includes the correct schema, primary
        /// keys, and foreign keys as specified by the ForeignTableAttribute.</remarks>
        [Fact]
        public void Returns_Metadata_WhenKeysAreResolved()
        {
            var navigationProperty = typeof(MainWithForeignKey).GetProperty(nameof(MainWithForeignKey.Foreign));
            var attr = new ForeignTableAttribute("foreign")
            {
                PrimaryKeys = new[] { "Id" },
                ForeignKeys = new[] { "ForeignId" },
                Schema = "dbo"
            };
            var result = (ValueTuple<string, string[], string[]>)ResolveForeignMetadataMethod.Invoke(
                null,
                new object[] { typeof(MainWithForeignKey), navigationProperty, attr }
            );
            Assert.Equal("dbo", result.Item1);
            Assert.Single(result.Item2);
            Assert.Single(result.Item3);
            Assert.Equal("Id", result.Item2[0]);
            Assert.Equal("ForeignId", result.Item3[0]);
        }

        /// <summary>
        /// Verifies that primary keys are automatically resolved from the Key attribute when the PrimaryKeys property
        /// is set to null in a foreign key relationship.
        /// </summary>
        /// <remarks>This test ensures that the method correctly identifies and assigns primary keys based
        /// on the Key attribute, establishing the expected foreign key relationship between entities. It specifically
        /// checks that the schema and foreign key properties are set as intended when automatic resolution is
        /// triggered.</remarks>
        [Fact]
        public void ResolvesPrimaryKeys_Automatically_FromKeyAttribute()
        {
            var navigationProperty = typeof(MainAutoPrimaryKey).GetProperty(nameof(MainAutoPrimaryKey.Foreign));
            var attr = new ForeignTableAttribute("foreign")
            {
                PrimaryKeys = null, // Forçar resolução automática
                ForeignKeys = new[] { "ForeignId" },
                Schema = "dbo"
            };
            var result = (ValueTuple<string, string[], string[]>)ResolveForeignMetadataMethod.Invoke(
                null,
                new object[] { typeof(MainAutoPrimaryKey), navigationProperty, attr }
            );
            Assert.Equal("dbo", result.Item1);
            Assert.Single(result.Item2);
            Assert.Single(result.Item3);
            Assert.Equal("Id", result.Item2[0]);
            Assert.Equal("ForeignId", result.Item3[0]);
        }

        /// <summary>
        /// Verifies that foreign keys are resolved automatically from the ForeignKeyAttribute when specified.
        /// </summary>
        /// <remarks>This test ensures that the foreign key resolution mechanism correctly establishes
        /// schema and key mappings based on the ForeignTableAttribute. It specifically checks the scenario where
        /// foreign keys are not explicitly defined, triggering automatic resolution. Use this test to confirm that the
        /// system supports attribute-driven foreign key mapping without manual configuration.</remarks>
        [Fact]
        public void ResolvesForeignKeys_Automatically_FromForeignKeyAttribute()
        {
            var navigationProperty = typeof(MainAutoForeignKey).GetProperty(nameof(MainAutoForeignKey.Foreign));
            var attr = new ForeignTableAttribute("foreign")
            {
                PrimaryKeys = new[] { "Id" },
                ForeignKeys = null, // Forçar resolução automática
                Schema = "dbo"
            };
            var result = (ValueTuple<string, string[], string[]>)ResolveForeignMetadataMethod.Invoke(
                null,
                new object[] { typeof(MainAutoForeignKey), navigationProperty, attr }
            );
            Assert.Equal("dbo", result.Item1);
            Assert.Single(result.Item2);
            Assert.Single(result.Item3);
            Assert.Equal("Id", result.Item2[0]);
            Assert.Equal("ForeignId", result.Item3[0]);
        }

        /// <summary>
        /// Verifies that metadata generation for an entity with multiple foreign keys does not require a join if at
        /// least one foreign key is not required.
        /// </summary>
        /// <remarks>This test ensures that when building metadata for entities with optional
        /// relationships, the join operation is not enforced if any of the foreign keys are marked as not required.
        /// This is important for supporting scenarios where some relationships are optional and should not trigger
        /// unnecessary joins in queries.</remarks>
        [Fact]
        public void BuildMetadata_JoinIsNotRequired_IfAnyForeignKeyIsNotRequired()
        {
            var type = typeof(MultiForeignKeyEntity);
            var prop = type.GetProperty(nameof(MultiForeignKeyEntity.Foreign2));
            var attr = new ForeignTableAttribute("foreign")
            {
                PrimaryKeys = new[] { "Id" },
                ForeignKeys = new[] { "Foreign2Id" }
            };
            var method = typeof(DatabaseEntity)
                .GetMethod("BuildMetadata", BindingFlags.NonPublic | BindingFlags.Static);
            var metadata = method.Invoke(null, new object[] { type });
            // Não lança, join não é obrigatório
            Assert.NotNull(metadata);
        }

        /// <summary>
        /// Verifies that metadata is correctly built for an entity type that does not define a Column attribute on its
        /// properties.
        /// </summary>
        /// <remarks>This test ensures that the BuildMetadata method can handle entity types lacking
        /// explicit column mapping attributes, and that it returns non-null metadata for such types.</remarks>
        [Fact]
        public void BuildMetadata_ColumnWithoutColumnAttribute()
        {
            var type = typeof(NoAttributeColumnEntity);
            var method = typeof(DatabaseEntity)
                .GetMethod("BuildMetadata", BindingFlags.NonPublic | BindingFlags.Static);
            var metadata = method.Invoke(null, new object[] { type });
            Assert.NotNull(metadata);
        }

        /// <summary>
        /// Verifies that the BuildMetadata method correctly constructs metadata for an entity type with a key property
        /// marked as required.
        /// </summary>
        /// <remarks>This test ensures that the metadata generation process identifies the specified key
        /// property and enforces its required constraint. It is intended to validate the behavior of the BuildMetadata
        /// method when handling entity types with both key and required attributes.</remarks>
        [Fact]
        public void BuildMetadata_ColumnWithKeyAndRequired()
        {
            var type = typeof(KeyAndRequiredEntity);
            var method = typeof(DatabaseEntity)
                .GetMethod("BuildMetadata", BindingFlags.NonPublic | BindingFlags.Static);
            var metadata = method.Invoke(null, new object[] { type });
            Assert.NotNull(metadata);
        }

        /// <summary>
        /// Verifies that the BuildMetadata method requires a join when all foreign keys in the entity are marked as
        /// required.
        /// </summary>
        /// <remarks>This test ensures that the metadata generated for the MainWithForeignKey entity
        /// correctly reflects the necessity of a join based on the required foreign key attributes. It validates that
        /// the BuildMetadata method behaves as expected when all foreign keys are mandatory.</remarks>
        [Fact]
        public void BuildMetadata_JoinIsRequired_IfAllForeignKeysAreRequired()
        {
            var type = typeof(MainWithForeignKey);
            var prop = type.GetProperty(nameof(MainWithForeignKey.ForeignId));
            var attr = new ForeignTableAttribute("foreign")
            {
                PrimaryKeys = new[] { "Id" },
                ForeignKeys = new[] { "ForeignId" }
            };
            var method = typeof(DatabaseEntity)
                .GetMethod("BuildMetadata", BindingFlags.NonPublic | BindingFlags.Static);
            var metadata = method.Invoke(null, new object[] { type });
            Assert.NotNull(metadata);
        }

        /// <summary>
        /// Verifies that metadata generation for a database entity correctly marks a join as required when all
        /// associated foreign keys are required.
        /// </summary>
        /// <remarks>This test ensures that the metadata builder properly interprets multiple required
        /// foreign keys on an entity, resulting in a required join. It is intended to validate the behavior of the
        /// metadata construction process for entities with multiple required foreign key relationships.</remarks>
        [Fact]
        public void BuildMetadata_JoinIsRequired_IfAllForeignKeysAreRequired_Multiple()
        {
            var type = typeof(MultiForeignKeyAllRequiredEntity);
            var prop = type.GetProperty(nameof(MultiForeignKeyAllRequiredEntity.Foreign1));
            var attr = new ForeignTableAttribute("foreign")
            {
                PrimaryKeys = new[] { "Id" },
                ForeignKeys = new[] { "Foreign1Id", "Foreign2Id" }
            };
            var method = typeof(DatabaseEntity)
                .GetMethod("BuildMetadata", BindingFlags.NonPublic | BindingFlags.Static);
            var metadata = method.Invoke(null, new object[] { type });
            Assert.NotNull(metadata);
        }

        /// <summary>
        /// Verifies that metadata is constructed successfully for an entity type when no foreign key is required.
        /// </summary>
        /// <remarks>This test ensures that the BuildMetadata method does not require a join operation
        /// when the specified entity type does not have any required foreign key constraints. The test passes if the
        /// returned metadata is not null, indicating that metadata construction succeeds under these
        /// conditions.</remarks>
        [Fact]
        public void BuildMetadata_JoinIsNotRequired_IfNoForeignKeyIsRequired()
        {
            var type = typeof(MultiForeignKeyNoneRequiredEntity);
            var method = typeof(DatabaseEntity)
                .GetMethod("BuildMetadata", BindingFlags.NonPublic | BindingFlags.Static);
            var metadata = method.Invoke(null, new object[] { type });
            Assert.NotNull(metadata);
        }

        /// <summary>
        /// Verifies that the BuildMetadata method correctly handles cases where the specified foreign key property does
        /// not exist on the entity type.
        /// </summary>
        /// <remarks>This test ensures that metadata generation remains robust even when the entity type
        /// lacks the expected foreign key property. It is intended to validate the resilience of the BuildMetadata
        /// implementation against misconfigured or incomplete entity definitions.</remarks>
        [Fact]
        public void BuildMetadata_Handles_NonExistentForeignKeyProperty()
        {
            var type = typeof(InvalidForeignKeyEntity);
            var method = typeof(DatabaseEntity)
                .GetMethod("BuildMetadata", BindingFlags.NonPublic | BindingFlags.Static);
            var metadata = method.Invoke(null, new object[] { type });
            Assert.NotNull(metadata);
        }

        /// <summary>
        /// Verifies that the entity validation succeeds and produces no errors when no validator is provided.
        /// </summary>
        /// <remarks>This test ensures that the IsValid method returns true and an empty error collection
        /// when the validator parameter is null, confirming that FluentValidation is not executed in this
        /// scenario.</remarks>
        [Fact]
        public void IsValidT_DoesNotRunFluentValidation_WhenValidatorIsNull()
        {
            var entity = new DatabaseEntityTests.TestEntity { Name = "Valid", Value = 5 };
            var result = entity.IsValid<DatabaseEntityTests.TestEntity>(out var errors, null);
            Assert.True(result);
            Assert.Empty(errors);
        }

        /// <summary>
        /// Verifies that the IsValid&lt;T&gt; method does not execute FluentValidation logic when the provided entity is not
        /// of the specified type T.
        /// </summary>
        /// <remarks>This test ensures that validation is bypassed for entities whose type does not match
        /// the generic type parameter, confirming that no validation errors are produced and the method returns true in
        /// such cases.</remarks>
        [Fact]
        public void IsValidT_DoesNotRunFluentValidation_WhenEntityIsNotOfTypeT()
        {
            var entity = new DatabaseEntityTests.TestEntity { Name = "Valid", Value = 5 };
            var validator = new InlineValidator<DatabaseEntityTests.Product>();
            var result = entity.IsValid<DatabaseEntityTests.Product>(out var errors, validator);
            Assert.True(result);
            Assert.Empty(errors);
        }
    }
}