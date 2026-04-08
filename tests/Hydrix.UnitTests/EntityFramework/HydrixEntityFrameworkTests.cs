using Hydrix.Attributes.Schemas;
using Hydrix.Caching;
using Hydrix.DependencyInjection;
using Hydrix.EntityFramework;
using Hydrix.Extensions;
using Hydrix.Mapping;
using Hydrix.Metadata.Builders;
using Hydrix.Metadata.EntityFramework;
using Hydrix.Metadata.Internals;
using Hydrix.Schemas;
using Hydrix.Schemas.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.EntityFramework
{
    /// <summary>
    /// Contains integration-style tests for the public Hydrix registration flow that consumes Entity Framework models.
    /// </summary>
    /// <remarks>These tests exercise the additive registration path through a real Entity Framework
    /// <see cref="DbContext"/> so Hydrix can validate entities, build queries, and materialize nested entities from the
    /// translated metadata without disturbing the existing attribute-based pipeline.</remarks>
    [Collection(HydrixEntityFrameworkTestCollection.Name)]
    public sealed class HydrixEntityFrameworkTests : IDisposable
    {
        /// <summary>
        /// Clears the Entity Framework metadata cache after each test.
        /// </summary>
        public void Dispose()
            => EntityFrameworkMetadataCache.Clear();

        /// <summary>
        /// Ensures the public registration API rejects null arguments.
        /// </summary>
        [Fact]
        public void RegisterModel_Throws_WhenArgumentIsNull()
            => Assert.Throws<ArgumentNullException>(() =>
                HydrixEntityFramework.RegisterModel(null));

        /// <summary>
        /// Ensures the public registration API rejects objects that do not expose an Entity Framework model.
        /// </summary>
        [Fact]
        public void RegisterModel_Throws_WhenInstanceDoesNotExposeEntityFrameworkModel()
            => Assert.Throws<InvalidOperationException>(() =>
                HydrixEntityFramework.RegisterModel(new object()));

        /// <summary>
        /// Ensures Entity Framework registration enables Hydrix validation for entities that do not use Hydrix table
        /// attributes.
        /// </summary>
        [Fact]
        public void RegisterModel_AllowsValidation_ForEntitiesWithoutHydrixAttributes()
        {
            Assert.Throws<MissingMemberException>(() =>
                EntityRequestValidationCache.Validate(typeof(SalesOrder)));

            using var context = CreateContext();
            HydrixEntityFramework.RegisterModel(context);

            Assert.True(EntityRequestValidationCache.Validate(typeof(SalesOrder)));
        }

        /// <summary>
        /// Ensures the dependency-injection startup helper resolves queued DbContext registrations and applies them to Hydrix.
        /// </summary>
        [Fact]
        public void UseHydrixEntityFrameworkModels_RegistersQueuedDbContextsFromDependencyInjection()
        {
            Assert.Throws<MissingMemberException>(() =>
                EntityRequestValidationCache.Validate(typeof(SalesOrder)));

            var services = new ServiceCollection();
            services.AddDbContext<SalesDbContext>(options =>
                options.UseSqlite("Data Source=:memory:"));
            services.AddHydrix();
            services.AddHydrixEntityFrameworkModel<SalesDbContext>();

            using var serviceProvider = services.BuildServiceProvider();
            var result = serviceProvider.UseHydrixEntityFrameworkModels();

            Assert.Same(serviceProvider, result);
            Assert.True(EntityRequestValidationCache.Validate(typeof(SalesOrder)));
        }

        /// <summary>
        /// Ensures the query hot cache refreshes after Entity Framework metadata is registered.
        /// </summary>
        [Fact]
        public void RegisterModel_RefreshesBuildQuery_WhenHotCacheWasAlreadyWarm()
        {
            var beforeRegistration = SalesOrder.BuildQuery<SalesOrder>();
            Assert.Contains("FROM SalesOrder so", beforeRegistration);

            using var context = CreateContext();
            HydrixEntityFramework.RegisterModel(context);

            var sql = SalesOrder.BuildQuery<SalesOrder>();

            Assert.Contains("FROM sales.sales_orders so", sql);
            Assert.Contains("INNER JOIN sales.customers c ON so.customer_id = c.id", sql);
            Assert.Contains("c.id AS \"Customer.id\"", sql);
            Assert.Contains("c.full_name AS \"Customer.full_name\"", sql);
            Assert.DoesNotContain("so.Customer", sql);
        }

        /// <summary>
        /// Ensures Hydrix materialization uses the column names translated from Entity Framework.
        /// </summary>
        [Fact]
        public void RegisterModel_MapsReaderRows_UsingEntityFrameworkColumnNames()
        {
            using var context = CreateContext();
            HydrixEntityFramework.RegisterModel(context.Model);

            var table = new DataTable();
            table.Columns.Add("id", typeof(int));
            table.Columns.Add("order_number", typeof(string));
            table.Columns.Add("status_id", typeof(int));
            table.Columns.Add("customer_id", typeof(int));
            table.Columns.Add("Customer.id", typeof(int));
            table.Columns.Add("Customer.full_name", typeof(string));
            table.Rows.Add(10, "SO-10", 2, 5, 5, "Alice Smith");

            using var reader = table.CreateDataReader();
            var orders = reader.MapTo<SalesOrder>();

            var order = Assert.Single(orders);
            Assert.Equal(10, order.Id);
            Assert.Equal("SO-10", order.Number);
            Assert.Equal(2, order.Status);
            Assert.Equal(5, order.CustomerId);
            Assert.NotNull(order.Customer);
            Assert.Equal(5, order.Customer.Id);
            Assert.Equal("Alice Smith", order.Customer.FullName);
        }

        /// <summary>
        /// Ensures the cache rejects a null collection of metadata entries.
        /// </summary>
        [Fact]
        public void EntityFrameworkMetadataCache_Register_Throws_WhenEntriesCollectionIsNull()
            => Assert.Throws<ArgumentNullException>(() =>
                EntityFrameworkMetadataCache.Register(null));

        /// <summary>
        /// Ensures the cache skips null entries while preserving valid registrations.
        /// </summary>
        [Fact]
        public void EntityFrameworkMetadataCache_Register_SkipsNullEntries_And_Caches_Metadata()
        {
            var entry = CreateRegisteredMetadata(typeof(CoverageOrder));

            EntityFrameworkMetadataCache.Register(new RegisteredEntityMetadata[]
            {
                null,
                entry
            });

            Assert.True(EntityFrameworkMetadataCache.TryGet(typeof(CoverageOrder), out var cached));
            Assert.Same(entry, cached);
        }

        /// <summary>
        /// Ensures the translated registration object validates constructor arguments and exposes its validity state.
        /// </summary>
        [Fact]
        public void RegisteredEntityMetadata_ReportsValidity_And_Guards_ConstructorArguments()
        {
            var validEntry = CreateRegisteredMetadata(typeof(CoverageOrder));
            Assert.True(validEntry.IsValid);

            var invalidEntry = new RegisteredEntityMetadata(
                typeof(CoverageOrder),
                MetadataFactory.CreateEntity(
                    Array.Empty<ColumnMap>(),
                    Array.Empty<TableMap>()),
                new EntityBuilderMetadata(
                    nameof(CoverageOrder),
                    typeof(CoverageOrder),
                    "coverage_orders",
                    "sales",
                    Array.Empty<ColumnBuilderMetadata>(),
                    Array.Empty<JoinBuilderMetadata>()));
            Assert.False(invalidEntry.IsValid);

            Assert.Throws<ArgumentNullException>(() =>
                new RegisteredEntityMetadata(
                    null,
                    validEntry.MaterializeMetadata,
                    validEntry.BuilderMetadata));
            Assert.Throws<ArgumentNullException>(() =>
                new RegisteredEntityMetadata(
                    typeof(CoverageOrder),
                    null,
                    validEntry.BuilderMetadata));
            Assert.Throws<ArgumentNullException>(() =>
                new RegisteredEntityMetadata(
                    typeof(CoverageOrder),
                    validEntry.MaterializeMetadata,
                    null));
        }

        /// <summary>
        /// Ensures the cache builders return the metadata previously registered from Entity Framework.
        /// </summary>
        [Fact]
        public void BuildMetadata_Uses_Registered_Metadata_In_All_Caches()
        {
            var entry = CreateRegisteredMetadata(typeof(CoverageOrder));
            EntityFrameworkMetadataCache.Register(new[] { entry });

            var builderMetadata = InvokePrivateStatic<EntityBuilderMetadata>(
                typeof(EntityBuilderMetadataCache),
                "BuildMetadata",
                typeof(CoverageOrder));

            Assert.Same(entry.BuilderMetadata, builderMetadata);
            Assert.Same(entry.MaterializeMetadata, EntityMetadataCache.BuildMetadata(typeof(CoverageOrder)));
            Assert.True(EntityRequestValidationCache.BuildMetadata(typeof(CoverageOrder)));
        }

        /// <summary>
        /// Ensures the translator helper methods handle null inputs and fallback metadata paths correctly.
        /// </summary>
        [Fact]
        public void Translator_PrivateHelpers_Handle_Null_And_Fallback_Paths()
        {
            Assert.Null(InvokeTranslator<object>("InvokeMethod", null, "Anything"));
            Assert.Null(InvokeTranslator<object>("InvokeMethod", new MethodHost(), "MissingMethod"));
            Assert.Null(InvokeTranslator<string>("InvokeMethod", new NullReturningMethodHost(), nameof(NullReturningMethodHost.GetNothing)));
            Assert.Equal(42, InvokeTranslator<int>("InvokeMethod", new MethodHost(), nameof(MethodHost.GetScalar)));
            Assert.Empty(InvokeTranslator<IEnumerable<object>>("InvokeEnumerable", new MethodHost(), nameof(MethodHost.GetScalar)));
            Assert.Empty(InvokeTranslator<IEnumerable<object>>("GetSequence", new object()));
            Assert.Null(InvokeTranslator<string>("GetAnnotationValue", null, "Relational:ColumnName"));
            Assert.Null(InvokeTranslator<string>("GetAnnotationValue", new AnnotationlessMetadata(), "Relational:ColumnName"));
            Assert.Null(InvokeTranslator<string>("GetAnnotationValue", new ExplicitAnnotationHost(), "Relational:ColumnName"));
            Assert.Null(InvokeTranslator<string>("GetAnnotationValue", new MultiInterfaceAnnotationHost(), "Relational:ColumnName"));
            Assert.Null(InvokeTranslator<object>("GetPropertyValue", null, "Model"));
            Assert.True(InvokeTranslator<bool>("IsNavigationOnDependent", new NavigationWithDependentFlag { IsOnDependent = true }, new object(), typeof(CoverageOrder)));
            Assert.True(InvokeTranslator<bool>("IsNavigationOnDependent", new AnnotationlessMetadata(), new FakeForeignKeyWithDeclaringEntityType { DeclaringEntityType = new FakeEntityTypeWithClrType { ClrType = typeof(CoverageOrder) } }, typeof(CoverageOrder)));
            Assert.False(InvokeTranslator<bool>("IsNavigationOnDependent", new AnnotationlessMetadata(), new FakeForeignKeyWithDeclaringEntityType { DeclaringEntityType = new FakeEntityTypeWithClrType { ClrType = typeof(string) } }, typeof(CoverageOrder)));
            Assert.True(InvokeTranslator<bool>("IsCollectionNavigation", new NavigationWithCollectionFlag { IsCollection = true }, typeof(CoverageOrder).GetProperty(nameof(CoverageOrder.RelatedCollection))));
            Assert.False(InvokeTranslator<bool>("IsCollectionNavigation", new AnnotationlessMetadata(), typeof(CoverageOrder).GetProperty(nameof(CoverageOrder.Name))));
            Assert.False(InvokeTranslator<bool>("IsRequired", new AnnotationlessMetadata()));
            Assert.True(InvokeTranslator<bool>("HasMethod", new MethodHost(), nameof(MethodHost.GetScalar)));
            Assert.False(InvokeTranslator<bool>("HasMethod", new MethodHost(), "MissingMethod"));
            Assert.False(InvokeTranslator<bool>("HasMethod", null, "MissingMethod"));
            Assert.Empty(InvokeTranslator<IEnumerable<string>>("GetPrimaryKeyPropertyNames", new EntityTypeWithoutPrimaryKey()));
            Assert.Empty(InvokeTranslator<string[]>("GetPrimaryKeyColumns", new EntityTypeWithoutPrimaryKey()));
            Assert.Empty(InvokeTranslator<IEnumerable<string>>("GetPrimaryKeyPropertyNames", new EntityTypeWithUnnamedPrimaryKey()));
            Assert.Equal("FallbackColumn", InvokeTranslator<string>("ResolveColumnName", new MetadataNameOnly { Name = "FallbackColumn" }, null));
            Assert.Equal(nameof(CoverageOrder.Name), InvokeTranslator<string>("ResolveColumnName", new AnnotationlessMetadata(), typeof(CoverageOrder).GetProperty(nameof(CoverageOrder.Name))));
            Assert.Null(InvokeTranslator<string>("ResolveColumnName", new AnnotationlessMetadata(), null));
            Assert.Equal(nameof(CoverageOrder.Id), InvokeTranslator<PropertyInfo>("ResolvePropertyInfo", new MetadataNameOnly { Name = nameof(CoverageOrder.Id) }, typeof(CoverageOrder)).Name);
            Assert.Null(InvokeTranslator<PropertyInfo>("ResolvePropertyInfo", new MetadataNameOnly { Name = "MissingProperty" }, typeof(CoverageOrder)));
            Assert.Null(InvokeTranslator<PropertyInfo>("ResolvePropertyInfo", new MetadataNameOnly(), typeof(CoverageOrder)));
        }

        /// <summary>
        /// Ensures the translator can build reflected entity metadata when relational table annotations are absent.
        /// </summary>
        [Fact]
        public void BuildEntityModel_UsesClrTypeName_WhenTableAnnotationIsMissing()
        {
            var property = new FakePropertyMetadata(
                typeof(CoverageOrder).GetProperty(nameof(CoverageOrder.Id)),
                null,
                isNullable: false);
            var entityType = new FakeEntityTypeMetadata(
                typeof(CoverageOrder),
                new[] { property },
                Array.Empty<object>(),
                new FakeKey(property),
                annotations: null);

            var reflected = InvokeTranslator<object>(
                "BuildEntityModel",
                entityType,
                typeof(CoverageOrder));
            var table = reflected.GetType()
                .GetProperty("Table", BindingFlags.Instance | BindingFlags.Public)
                .GetValue(reflected) as string;

            Assert.Equal(nameof(CoverageOrder), table);
        }

        /// <summary>
        /// Ensures the translator handles principal-side navigations when translating a synthetic Entity Framework model.
        /// </summary>
        [Fact]
        public void Translate_Handles_PrincipalSideNavigation_Metadata()
        {
            var customerIdProperty = new FakePropertyMetadata(
                typeof(PrincipalCustomer).GetProperty(nameof(PrincipalCustomer.Id)),
                "customer_id",
                isNullable: false);
            var orderIdProperty = new FakePropertyMetadata(
                typeof(PrincipalOrder).GetProperty(nameof(PrincipalOrder.Id)),
                "order_id",
                isNullable: false);
            var orderCustomerIdProperty = new FakePropertyMetadata(
                typeof(PrincipalOrder).GetProperty(nameof(PrincipalOrder.CustomerId)),
                "customer_id",
                isNullable: true);

            var customerKey = new FakeKey(customerIdProperty);
            var orderKey = new FakeKey(orderIdProperty);
            var orderEntity = new FakeEntityTypeMetadata(
                typeof(PrincipalOrder),
                new[]
                {
                    orderIdProperty,
                    orderCustomerIdProperty
                },
                Array.Empty<object>(),
                orderKey,
                new Dictionary<string, string>
                {
                    ["Relational:TableName"] = "orders",
                    ["Relational:Schema"] = "sales"
                });
            var navigation = new FakeNavigationMetadata(
                typeof(PrincipalCustomer).GetProperty(nameof(PrincipalCustomer.LatestOrder)),
                new FakeForeignKeyMetadata(
                    new[] { orderCustomerIdProperty },
                    customerKey,
                    orderEntity,
                    isRequired: false),
                isOnDependent: false);
            var customerEntity = new FakeEntityTypeMetadata(
                typeof(PrincipalCustomer),
                new[] { customerIdProperty },
                new object[] { navigation },
                customerKey,
                annotations: null);
            var model = new FakeModel(
                customerEntity,
                orderEntity);

            var registrations = EntityFrameworkModelTranslator.Translate(model);
            var customerRegistration = registrations.Single(registration => registration.Type == typeof(PrincipalCustomer));

            Assert.Single(customerRegistration.BuilderMetadata.Joins);
            Assert.Single(customerRegistration.MaterializeMetadata.Entities);
        }

        /// <summary>
        /// Ensures the translator ignores navigations that do not expose foreign-key metadata.
        /// </summary>
        [Fact]
        public void Translate_Ignores_Navigation_WithoutForeignKey_Metadata()
        {
            var customerIdProperty = new FakePropertyMetadata(
                typeof(PrincipalCustomer).GetProperty(nameof(PrincipalCustomer.Id)),
                "customer_id",
                isNullable: false);
            var orderIdProperty = new FakePropertyMetadata(
                typeof(PrincipalOrder).GetProperty(nameof(PrincipalOrder.Id)),
                "order_id",
                isNullable: false);
            var customerKey = new FakeKey(customerIdProperty);
            var orderEntity = new FakeEntityTypeMetadata(
                typeof(PrincipalOrder),
                new[] { orderIdProperty },
                Array.Empty<object>(),
                new FakeKey(orderIdProperty),
                annotations: null);
            var customerEntity = new FakeEntityTypeMetadata(
                typeof(PrincipalCustomer),
                new[] { customerIdProperty },
                new object[]
                {
                    new FakeNavigationMetadata(
                        typeof(PrincipalCustomer).GetProperty(nameof(PrincipalCustomer.LatestOrder)),
                        foreignKey: null,
                        isOnDependent: false)
                },
                customerKey,
                annotations: null);

            var registrations = EntityFrameworkModelTranslator.Translate(
                new FakeModel(
                    customerEntity,
                    orderEntity));
            var customerRegistration = registrations.Single(registration => registration.Type == typeof(PrincipalCustomer));

            Assert.Empty(customerRegistration.BuilderMetadata.Joins);
            Assert.Empty(customerRegistration.MaterializeMetadata.Entities);
        }

        /// <summary>
        /// Ensures the translator skips entity types when CLR metadata is missing or the CLR type is not compatible
        /// with <see cref="ITable"/>.
        /// </summary>
        [Fact]
        public void Translate_Skips_EntityTypes_WithNullOrNonITableClrType()
        {
            var validProperty = new FakePropertyMetadata(
                typeof(CoverageOrder).GetProperty(nameof(CoverageOrder.Id)),
                "id",
                isNullable: false);

            var validEntity = new FakeEntityTypeMetadata(
                typeof(CoverageOrder),
                new[] { validProperty },
                Array.Empty<object>(),
                new FakeKey(validProperty),
                annotations: null);

            var nullClrEntity = new FakeEntityTypeMetadata(
                clrType: null,
                properties: Array.Empty<object>(),
                navigations: Array.Empty<object>(),
                primaryKey: null,
                annotations: null);

            var nonITableEntity = new FakeEntityTypeMetadata(
                typeof(SalesAudit),
                properties: Array.Empty<object>(),
                navigations: Array.Empty<object>(),
                primaryKey: null,
                annotations: null);

            var registrations = EntityFrameworkModelTranslator.Translate(
                new FakeModel(
                    nullClrEntity,
                    nonITableEntity,
                    validEntity));

            var registration = Assert.Single(registrations);
            Assert.Equal(typeof(CoverageOrder), registration.Type);
        }

        /// <summary>
        /// Ensures the scalar-property translation branch skips properties that cannot be resolved, are read-only,
        /// or are indexers.
        /// </summary>
        [Fact]
        public void BuildEntityModel_Skips_InvalidScalarProperties()
        {
            var unresolved = new MetadataNameOnly { Name = "MissingProperty" };
            var readOnly = new MetadataNameOnly { Name = nameof(TranslatorScalarCoverageEntity.ReadOnlyValue) };
            var indexer = new MetadataNameOnly { Name = "Item" };
            var valid = new MetadataNameOnly { Name = nameof(TranslatorScalarCoverageEntity.WritableValue) };

            var entityType = new FakeEntityTypeMetadata(
                typeof(TranslatorScalarCoverageEntity),
                new object[] { unresolved, readOnly, indexer, valid },
                Array.Empty<object>(),
                primaryKey: null,
                annotations: null);

            var reflected = InvokeTranslator<object>(
                "BuildEntityModel",
                entityType,
                typeof(TranslatorScalarCoverageEntity));

            var fields = (System.Collections.IEnumerable)reflected.GetType()
                .GetProperty("Fields", BindingFlags.Instance | BindingFlags.Public)
                .GetValue(reflected);

            var fieldList = fields.Cast<object>().ToArray();
            Assert.Single(fieldList);

            var property = (PropertyInfo)fieldList[0].GetType()
                .GetProperty("Property", BindingFlags.Instance | BindingFlags.Public)
                .GetValue(fieldList[0]);

            Assert.Equal(nameof(TranslatorScalarCoverageEntity.WritableValue), property.Name);
        }

        /// <summary>
        /// Ensures navigation-property resolution rejects missing, read-only, and indexer properties while accepting
        /// a valid writable reference navigation.
        /// </summary>
        [Fact]
        public void ResolveNavigationProperty_RejectsInvalidProperties_AndAcceptsWritableReference()
        {
            var missing = InvokeTranslator<PropertyInfo>(
                "ResolveNavigationProperty",
                new MetadataNameOnly { Name = "MissingNavigation" },
                typeof(TranslatorNavigationCoverageEntity));
            Assert.Null(missing);

            var readOnly = InvokeTranslator<PropertyInfo>(
                "ResolveNavigationProperty",
                new MetadataNameOnly { Name = nameof(TranslatorNavigationCoverageEntity.ReadOnlyReference) },
                typeof(TranslatorNavigationCoverageEntity));
            Assert.Null(readOnly);

            var indexer = InvokeTranslator<PropertyInfo>(
                "ResolveNavigationProperty",
                new MetadataNameOnly { Name = "Item" },
                typeof(TranslatorNavigationCoverageEntity));
            Assert.Null(indexer);

            var valid = InvokeTranslator<PropertyInfo>(
                "ResolveNavigationProperty",
                new MetadataNameOnly { Name = nameof(TranslatorNavigationCoverageEntity.WritableReference) },
                typeof(TranslatorNavigationCoverageEntity));
            Assert.NotNull(valid);
            Assert.Equal(nameof(TranslatorNavigationCoverageEntity.WritableReference), valid.Name);
        }

        /// <summary>
        /// Ensures the translated navigation column resolver returns false when the dependent-side foreign-key columns are absent.
        /// </summary>
        [Fact]
        public void TryResolveNavigationColumns_ReturnsFalse_WhenMainColumnsAreEmpty()
        {
            var customerIdProperty = new FakePropertyMetadata(
                typeof(PrincipalCustomer).GetProperty(nameof(PrincipalCustomer.Id)),
                "customer_id",
                isNullable: false);
            var foreignKey = new FakeForeignKeyMetadata(
                Array.Empty<object>(),
                new FakeKey(customerIdProperty),
                declaringEntityType: new object(),
                isRequired: false);
            var navigation = new FakeNavigationMetadata(
                typeof(SalesOrder).GetProperty(nameof(SalesOrder.Customer)),
                foreignKey,
                isOnDependent: true);

            var resolved = InvokeTryResolveNavigationColumns(
                navigation,
                foreignKey,
                typeof(SalesOrder),
                out var mainColumns,
                out var targetColumns,
                out var isRequiredJoin);

            Assert.False(resolved);
            Assert.Empty(mainColumns);
            Assert.Single(targetColumns);
            Assert.False(isRequiredJoin);
        }

        /// <summary>
        /// Ensures the translated navigation column resolver returns false when the principal-side target columns are absent.
        /// </summary>
        [Fact]
        public void TryResolveNavigationColumns_ReturnsFalse_WhenTargetColumnsAreEmpty()
        {
            var customerIdProperty = new FakePropertyMetadata(
                typeof(PrincipalCustomer).GetProperty(nameof(PrincipalCustomer.Id)),
                "customer_id",
                isNullable: false);
            var foreignKey = new FakeForeignKeyMetadata(
                Array.Empty<object>(),
                new FakeKey(customerIdProperty),
                declaringEntityType: new object(),
                isRequired: false);
            var navigation = new FakeNavigationMetadata(
                typeof(PrincipalCustomer).GetProperty(nameof(PrincipalCustomer.LatestOrder)),
                foreignKey,
                isOnDependent: false);

            var resolved = InvokeTryResolveNavigationColumns(
                navigation,
                foreignKey,
                typeof(PrincipalCustomer),
                out var mainColumns,
                out var targetColumns,
                out var isRequiredJoin);

            Assert.False(resolved);
            Assert.Single(mainColumns);
            Assert.Empty(targetColumns);
            Assert.False(isRequiredJoin);
        }

        /// <summary>
        /// Ensures the nested table map rejects a null property descriptor.
        /// </summary>
        [Fact]
        public void TableMap_Throws_WhenPropertyIsNull()
            => Assert.Throws<ArgumentNullException>(() =>
                new TableMap(
                    null,
                    new ForeignTableAttribute("child_table")));

        /// <summary>
        /// Invokes a private static method and returns its strongly typed result.
        /// </summary>
        /// <typeparam name="TResult">The expected return type.</typeparam>
        /// <param name="targetType">The type that declares the private static method.</param>
        /// <param name="methodName">The name of the private static method.</param>
        /// <param name="arguments">The arguments passed to the method.</param>
        /// <returns>The result returned by the invoked method.</returns>
        private static TResult InvokePrivateStatic<TResult>(
            Type targetType,
            string methodName,
            params object[] arguments)
        {
            var method = targetType
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Single(candidate =>
                    candidate.Name == methodName &&
                    candidate.GetParameters().Length == arguments.Length);

            return (TResult)method.Invoke(null, arguments);
        }

        /// <summary>
        /// Invokes a private static method on the Entity Framework translator and returns its strongly typed result.
        /// </summary>
        /// <typeparam name="TResult">The expected return type.</typeparam>
        /// <param name="methodName">The name of the private static translator method.</param>
        /// <param name="arguments">The arguments passed to the method.</param>
        /// <returns>The result returned by the invoked method.</returns>
        private static TResult InvokeTranslator<TResult>(
            string methodName,
            params object[] arguments)
            => InvokePrivateStatic<TResult>(
                typeof(HydrixEntityFramework).Assembly.GetType("Hydrix.EntityFramework.EntityFrameworkModelTranslator"),
                methodName,
                arguments);

        /// <summary>
        /// Invokes the translated navigation-column resolver and returns its out parameters.
        /// </summary>
        /// <param name="navigation">The synthetic navigation metadata object.</param>
        /// <param name="foreignKey">The synthetic foreign-key metadata object.</param>
        /// <param name="currentClrType">The CLR type currently being translated.</param>
        /// <param name="mainColumns">When this method returns, contains the resolved main-side columns.</param>
        /// <param name="targetColumns">When this method returns, contains the resolved target-side columns.</param>
        /// <param name="isRequiredJoin">When this method returns, indicates whether the navigation should be emitted as a required join.</param>
        /// <returns><see langword="true"/> when the translator resolved both sides of the relationship; otherwise,
        /// <see langword="false"/>.</returns>
        private static bool InvokeTryResolveNavigationColumns(
            object navigation,
            object foreignKey,
            Type currentClrType,
            out string[] mainColumns,
            out string[] targetColumns,
            out bool isRequiredJoin)
        {
            var method = typeof(HydrixEntityFramework).Assembly
                .GetType("Hydrix.EntityFramework.EntityFrameworkModelTranslator")
                .GetMethod(
                    "TryResolveNavigationColumns",
                    BindingFlags.Static | BindingFlags.NonPublic);
            var arguments = new object[]
            {
                navigation,
                foreignKey,
                currentClrType,
                null,
                null,
                false
            };

            var result = (bool)method.Invoke(
                null,
                arguments);

            mainColumns = arguments[3] as string[] ?? Array.Empty<string>();
            targetColumns = arguments[4] as string[] ?? Array.Empty<string>();
            isRequiredJoin = arguments[5] is bool value && value;
            return result;
        }

        /// <summary>
        /// Creates a manual metadata registration used by the coverage tests.
        /// </summary>
        /// <param name="entityType">The CLR type that should be represented by the registration.</param>
        /// <returns>A <see cref="RegisteredEntityMetadata"/> instance that can be inserted into the cache.</returns>
        private static RegisteredEntityMetadata CreateRegisteredMetadata(
            Type entityType)
        {
            var property = entityType.GetProperty(nameof(CoverageOrder.Id));
            var materializeMetadata = MetadataFactory.CreateEntity(
                new[]
                {
                    new ColumnMap(
                        "registered_id",
                        MetadataFactory.CreateSetter(property),
                        FieldReaderFactory.Create(property.PropertyType),
                        property.PropertyType,
                        property)
                },
                Array.Empty<TableMap>());
            var builderMetadata = new EntityBuilderMetadata(
                entityType.Name,
                entityType,
                "coverage_orders",
                "sales",
                new[]
                {
                    new ColumnBuilderMetadata(
                        property.Name,
                        "registered_id",
                        true,
                        true,
                        MetadataFactory.CreateGetter(property))
                },
                Array.Empty<JoinBuilderMetadata>());

            return new RegisteredEntityMetadata(
                entityType,
                materializeMetadata,
                builderMetadata);
        }

        /// <summary>
        /// Represents an entity type used to build manual metadata registrations during the coverage tests.
        /// </summary>
        private sealed class CoverageOrder : DatabaseEntity, ITable
        {
            /// <summary>
            /// Gets or sets the entity identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the entity name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets a related collection property used when exercising collection-navigation checks.
            /// </summary>
            public ICollection<CoverageOrderItem> RelatedCollection { get; set; }
        }

        /// <summary>
        /// Represents a collection item used by the coverage tests.
        /// </summary>
        private sealed class CoverageOrderItem
        {
            /// <summary>
            /// Gets or sets the item identifier.
            /// </summary>
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents an entity used to exercise scalar-property filtering branches in translator coverage tests.
        /// </summary>
        private sealed class TranslatorScalarCoverageEntity : DatabaseEntity, ITable
        {
            /// <summary>
            /// Gets or sets a writable scalar value.
            /// </summary>
            public int WritableValue { get; set; }

            /// <summary>
            /// Gets a read-only scalar value.
            /// </summary>
            public int ReadOnlyValue => WritableValue;

            /// <summary>
            /// Gets or sets an indexer value used to exercise indexer filtering.
            /// </summary>
            /// <param name="index">The index position.</param>
            /// <returns>The value at the specified index.</returns>
            public int this[int index]
            {
                get => index;
                set { }
            }
        }

        /// <summary>
        /// Represents an entity used to exercise navigation-property filtering branches in translator coverage tests.
        /// </summary>
        private sealed class TranslatorNavigationCoverageEntity : DatabaseEntity, ITable
        {
            /// <summary>
            /// Gets or sets a writable reference navigation.
            /// </summary>
            public SalesCustomer WritableReference { get; set; }

            /// <summary>
            /// Gets a read-only reference navigation.
            /// </summary>
            public SalesCustomer ReadOnlyReference => null;

            /// <summary>
            /// Gets or sets an indexer navigation used to exercise indexer filtering.
            /// </summary>
            /// <param name="index">The index position.</param>
            /// <returns>The navigation value at the specified index.</returns>
            public SalesCustomer this[int index]
            {
                get => null;
                set { }
            }
        }

        /// <summary>
        /// Represents a helper object that exposes a scalar-returning method.
        /// </summary>
        private sealed class MethodHost
        {
            /// <summary>
            /// Returns a scalar value so the translator can exercise the non-enumerable path.
            /// </summary>
            /// <returns>The scalar value returned by the method.</returns>
            public int GetScalar()
                => 42;
        }

        /// <summary>
        /// Represents a helper object that exposes a method returning null.
        /// </summary>
        private sealed class NullReturningMethodHost
        {
            /// <summary>
            /// Returns null so the translator can exercise the null-return path after reflection invocation.
            /// </summary>
            /// <returns><see langword="null"/>.</returns>
            public string GetNothing()
                => null;
        }

        /// <summary>
        /// Represents metadata with only a <see cref="Name"/> property.
        /// </summary>
        private sealed class MetadataNameOnly
        {
            /// <summary>
            /// Gets or sets the metadata name.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents metadata that intentionally does not expose annotations.
        /// </summary>
        private sealed class AnnotationlessMetadata
        { }

        /// <summary>
        /// Contract that exposes FindAnnotation only as an explicit interface implementation.
        /// </summary>
        private interface IExplicitAnnotatable
        {
            object FindAnnotation(string name);
        }

        /// <summary>
        /// An unrelated marker interface that does not expose FindAnnotation.
        /// </summary>
        private interface IUnrelatedMarker
        { }

        /// <summary>
        /// Implements FindAnnotation only via explicit interface — not visible as a public instance method.
        /// </summary>
        private sealed class ExplicitAnnotationHost : IExplicitAnnotatable
        {
            object IExplicitAnnotatable.FindAnnotation(string name) => null;
        }

        /// <summary>
        /// Implements an unrelated interface first, then FindAnnotation via explicit interface,
        /// so the interface-fallback loop must iterate past a non-matching interface before finding it.
        /// </summary>
        private sealed class MultiInterfaceAnnotationHost : IUnrelatedMarker, IExplicitAnnotatable
        {
            object IExplicitAnnotatable.FindAnnotation(string name) => null;
        }

        /// <summary>
        /// Simulates an EF entity-type metadata object that exposes a CLR type.
        /// </summary>
        private sealed class FakeEntityTypeWithClrType
        {
            /// <summary>
            /// Gets or sets the CLR type represented by this entity type.
            /// </summary>
            public Type ClrType { get; set; }
        }

        /// <summary>
        /// Simulates an EF foreign-key metadata object that exposes its declaring entity type.
        /// </summary>
        private sealed class FakeForeignKeyWithDeclaringEntityType
        {
            /// <summary>
            /// Gets or sets the entity type that declares this foreign key.
            /// </summary>
            public FakeEntityTypeWithClrType DeclaringEntityType { get; set; }
        }

        /// <summary>
        /// Represents a navigation metadata object with an explicit dependent-side flag.
        /// </summary>
        private sealed class NavigationWithDependentFlag
        {
            /// <summary>
            /// Gets or sets a value indicating whether the navigation is on the dependent side.
            /// </summary>
            public bool IsOnDependent { get; set; }
        }

        /// <summary>
        /// Represents a navigation metadata object with an explicit collection flag.
        /// </summary>
        private sealed class NavigationWithCollectionFlag
        {
            /// <summary>
            /// Gets or sets a value indicating whether the navigation is a collection.
            /// </summary>
            public bool IsCollection { get; set; }
        }

        /// <summary>
        /// Represents an entity metadata object whose primary key is intentionally absent.
        /// </summary>
        private sealed class EntityTypeWithoutPrimaryKey
        {
            /// <summary>
            /// Returns null to simulate an Entity Framework entity type without a primary key.
            /// </summary>
            /// <returns><see langword="null"/>.</returns>
            public object FindPrimaryKey()
                => null;
        }

        /// <summary>
        /// Represents an entity metadata object whose primary-key properties do not expose a usable name.
        /// </summary>
        private sealed class EntityTypeWithUnnamedPrimaryKey
        {
            /// <summary>
            /// Returns a key object whose properties do not expose a readable name.
            /// </summary>
            /// <returns>A <see cref="FakeKey"/> instance.</returns>
            public object FindPrimaryKey()
                => new FakeKey(new AnnotationlessMetadata());
        }

        /// <summary>
        /// Represents a customer entity used by the synthetic principal-side navigation test.
        /// </summary>
        private sealed class PrincipalCustomer : DatabaseEntity, ITable
        {
            /// <summary>
            /// Gets or sets the customer identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the principal-side navigation to the latest order.
            /// </summary>
            public PrincipalOrder LatestOrder { get; set; }
        }

        /// <summary>
        /// Represents an order entity used by the synthetic principal-side navigation test.
        /// </summary>
        private sealed class PrincipalOrder : DatabaseEntity, ITable
        {
            /// <summary>
            /// Gets or sets the order identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the foreign-key identifier of the customer.
            /// </summary>
            public int CustomerId { get; set; }
        }

        /// <summary>
        /// Represents a synthetic Entity Framework model used by the translator tests.
        /// </summary>
        private sealed class FakeModel
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FakeModel"/> class.
            /// </summary>
            /// <param name="entityTypes">The synthetic entity types exposed by the model.</param>
            public FakeModel(
                params object[] entityTypes)
                => EntityTypes = entityTypes;

            /// <summary>
            /// Gets the synthetic entity types exposed by the model.
            /// </summary>
            public object[] EntityTypes { get; }

            /// <summary>
            /// Returns the synthetic entity types exposed by the model.
            /// </summary>
            /// <returns>The synthetic entity types exposed by the model.</returns>
            public IEnumerable<object> GetEntityTypes()
                => EntityTypes;
        }

        /// <summary>
        /// Represents a synthetic Entity Framework entity-type metadata object.
        /// </summary>
        private sealed class FakeEntityTypeMetadata
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FakeEntityTypeMetadata"/> class.
            /// </summary>
            /// <param name="clrType">The CLR type represented by the synthetic metadata.</param>
            /// <param name="properties">The synthetic scalar properties.</param>
            /// <param name="navigations">The synthetic navigations.</param>
            /// <param name="primaryKey">The synthetic primary key.</param>
            /// <param name="annotations">The synthetic relational annotations.</param>
            public FakeEntityTypeMetadata(
                Type clrType,
                IEnumerable<object> properties,
                IEnumerable<object> navigations,
                object primaryKey,
                IReadOnlyDictionary<string, string> annotations)
            {
                ClrType = clrType;
                Properties = properties?.ToArray() ?? Array.Empty<object>();
                Navigations = navigations?.ToArray() ?? Array.Empty<object>();
                PrimaryKey = primaryKey;
                Annotations = annotations;
            }

            /// <summary>
            /// Gets the CLR type represented by the synthetic metadata.
            /// </summary>
            public Type ClrType { get; }

            /// <summary>
            /// Gets the synthetic scalar properties.
            /// </summary>
            public object[] Properties { get; }

            /// <summary>
            /// Gets the synthetic navigations.
            /// </summary>
            public object[] Navigations { get; }

            /// <summary>
            /// Gets the synthetic primary key.
            /// </summary>
            public object PrimaryKey { get; }

            /// <summary>
            /// Gets the synthetic relational annotations.
            /// </summary>
            public IReadOnlyDictionary<string, string> Annotations { get; }

            /// <summary>
            /// Returns the synthetic scalar properties.
            /// </summary>
            /// <returns>The synthetic scalar properties.</returns>
            public IEnumerable<object> GetProperties()
                => Properties;

            /// <summary>
            /// Returns the synthetic navigations.
            /// </summary>
            /// <returns>The synthetic navigations.</returns>
            public IEnumerable<object> GetNavigations()
                => Navigations;

            /// <summary>
            /// Returns the synthetic primary key.
            /// </summary>
            /// <returns>The synthetic primary key.</returns>
            public object FindPrimaryKey()
                => PrimaryKey;

            /// <summary>
            /// Finds the synthetic relational annotation with the specified name.
            /// </summary>
            /// <param name="annotationName">The annotation name to search for.</param>
            /// <returns>A <see cref="FakeAnnotation"/> instance when the annotation exists; otherwise, <see langword="null"/>.</returns>
            public object FindAnnotation(
                string annotationName)
            {
                if (Annotations == null ||
                    !Annotations.TryGetValue(annotationName, out var value))
                {
                    return null;
                }

                return new FakeAnnotation(value);
            }
        }

        /// <summary>
        /// Represents a synthetic Entity Framework property metadata object.
        /// </summary>
        private sealed class FakePropertyMetadata
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FakePropertyMetadata"/> class.
            /// </summary>
            /// <param name="propertyInfo">The CLR property represented by the metadata.</param>
            /// <param name="columnName">The synthetic relational column name.</param>
            /// <param name="isNullable">Indicates whether the property is nullable.</param>
            public FakePropertyMetadata(
                PropertyInfo propertyInfo,
                string columnName,
                bool isNullable)
            {
                PropertyInfo = propertyInfo;
                Name = propertyInfo?.Name;
                ColumnName = columnName;
                IsNullable = isNullable;
            }

            /// <summary>
            /// Gets the CLR property represented by the metadata.
            /// </summary>
            public PropertyInfo PropertyInfo { get; }

            /// <summary>
            /// Gets the CLR property name.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets the synthetic relational column name.
            /// </summary>
            public string ColumnName { get; }

            /// <summary>
            /// Gets a value indicating whether the property is nullable.
            /// </summary>
            public bool IsNullable { get; }

            /// <summary>
            /// Finds the synthetic relational annotation with the specified name.
            /// </summary>
            /// <param name="annotationName">The annotation name to search for.</param>
            /// <returns>A <see cref="FakeAnnotation"/> instance when the annotation exists; otherwise, <see langword="null"/>.</returns>
            public object FindAnnotation(
                string annotationName)
            {
                if (annotationName == "Relational:ColumnName" && !string.IsNullOrWhiteSpace(ColumnName))
                    return new FakeAnnotation(ColumnName);

                return null;
            }
        }

        /// <summary>
        /// Represents a synthetic Entity Framework navigation metadata object.
        /// </summary>
        private sealed class FakeNavigationMetadata
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FakeNavigationMetadata"/> class.
            /// </summary>
            /// <param name="propertyInfo">The CLR property represented by the navigation.</param>
            /// <param name="foreignKey">The synthetic foreign-key metadata.</param>
            /// <param name="isOnDependent">Indicates whether the navigation is on the dependent side.</param>
            public FakeNavigationMetadata(
                PropertyInfo propertyInfo,
                object foreignKey,
                bool isOnDependent)
            {
                PropertyInfo = propertyInfo;
                Name = propertyInfo?.Name;
                ForeignKey = foreignKey;
                IsOnDependent = isOnDependent;
            }

            /// <summary>
            /// Gets the CLR property represented by the navigation.
            /// </summary>
            public PropertyInfo PropertyInfo { get; }

            /// <summary>
            /// Gets the CLR property name.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets the synthetic foreign-key metadata.
            /// </summary>
            public object ForeignKey { get; }

            /// <summary>
            /// Gets a value indicating whether the navigation is on the dependent side.
            /// </summary>
            public bool IsOnDependent { get; }
        }

        /// <summary>
        /// Represents synthetic foreign-key metadata.
        /// </summary>
        private sealed class FakeForeignKeyMetadata
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FakeForeignKeyMetadata"/> class.
            /// </summary>
            /// <param name="properties">The foreign-key properties.</param>
            /// <param name="principalKey">The principal key metadata.</param>
            /// <param name="declaringEntityType">The entity type that declares the foreign key.</param>
            /// <param name="isRequired">Indicates whether the foreign key is required.</param>
            public FakeForeignKeyMetadata(
                IEnumerable<object> properties,
                object principalKey,
                object declaringEntityType,
                bool isRequired)
            {
                Properties = properties?.ToArray() ?? Array.Empty<object>();
                PrincipalKey = principalKey;
                DeclaringEntityType = declaringEntityType;
                IsRequired = isRequired;
            }

            /// <summary>
            /// Gets the foreign-key properties.
            /// </summary>
            public object[] Properties { get; }

            /// <summary>
            /// Gets the principal key metadata.
            /// </summary>
            public object PrincipalKey { get; }

            /// <summary>
            /// Gets the entity type that declares the foreign key.
            /// </summary>
            public object DeclaringEntityType { get; }

            /// <summary>
            /// Gets a value indicating whether the foreign key is required.
            /// </summary>
            public bool IsRequired { get; }
        }

        /// <summary>
        /// Represents synthetic key metadata.
        /// </summary>
        private sealed class FakeKey
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FakeKey"/> class.
            /// </summary>
            /// <param name="properties">The properties that compose the key.</param>
            public FakeKey(
                params object[] properties)
                => Properties = properties ?? Array.Empty<object>();

            /// <summary>
            /// Gets the properties that compose the key.
            /// </summary>
            public object[] Properties { get; }
        }

        /// <summary>
        /// Represents a synthetic annotation object.
        /// </summary>
        private sealed class FakeAnnotation
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FakeAnnotation"/> class.
            /// </summary>
            /// <param name="value">The annotation value.</param>
            public FakeAnnotation(
                object value)
                => Value = value;

            /// <summary>
            /// Gets the annotation value.
            /// </summary>
            public object Value { get; }
        }

        /// <summary>
        /// Creates the Entity Framework context used by the tests in this class.
        /// </summary>
        /// <returns>A configured <see cref="SalesDbContext"/> instance.</returns>
        private static SalesDbContext CreateContext()
            => new SalesDbContext(
                new DbContextOptionsBuilder<SalesDbContext>()
                    .UseSqlite("Data Source=:memory:")
                    .Options);

        /// <summary>
        /// Represents a customer entity mapped only through Entity Framework.
        /// </summary>
        private sealed class SalesCustomer : DatabaseEntity, ITable
        {
            /// <summary>
            /// Gets or sets the customer identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the full customer name.
            /// </summary>
            public string FullName { get; set; }
        }

        /// <summary>
        /// Represents an order entity mapped only through Entity Framework.
        /// </summary>
        private sealed class SalesOrder : DatabaseEntity, ITable
        {
            /// <summary>
            /// Gets or sets the order identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the order number.
            /// </summary>
            public string Number { get; set; }

            /// <summary>
            /// Gets or sets the order status identifier.
            /// </summary>
            public int Status { get; set; }

            /// <summary>
            /// Gets or sets the foreign-key identifier of the customer.
            /// </summary>
            public int CustomerId { get; set; }

            /// <summary>
            /// Gets or sets the reference navigation to the customer.
            /// </summary>
            public SalesCustomer Customer { get; set; }

            /// <summary>
            /// Gets or sets the collection navigation to the order lines.
            /// </summary>
            public ICollection<SalesOrderLine> Lines { get; set; }
        }

        /// <summary>
        /// Represents an order-line entity used to keep a collection navigation in the Entity Framework model.
        /// </summary>
        private sealed class SalesOrderLine : DatabaseEntity, ITable
        {
            /// <summary>
            /// Gets or sets the order-line identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the parent-order identifier.
            /// </summary>
            public int OrderId { get; set; }

            /// <summary>
            /// Gets or sets the ordered quantity.
            /// </summary>
            public int Quantity { get; set; }

            /// <summary>
            /// Gets or sets the reference navigation to the parent order.
            /// </summary>
            public SalesOrder Order { get; set; }
        }

        /// <summary>
        /// Represents a keyless projection entity used to exercise Entity Framework metadata without a primary key.
        /// </summary>
        private sealed class SalesProjection : DatabaseEntity, ITable
        {
            /// <summary>
            /// Gets or sets the projection label.
            /// </summary>
            public string Label { get; set; }
        }

        /// <summary>
        /// Represents an Entity Framework entity that is intentionally not compatible with Hydrix.
        /// </summary>
        private sealed class SalesAudit
        {
            /// <summary>
            /// Gets or sets the audit identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the associated order identifier.
            /// </summary>
            public int OrderId { get; set; }
        }

        /// <summary>
        /// Represents the Entity Framework context used by the tests in this class.
        /// </summary>
        private sealed class SalesDbContext : DbContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SalesDbContext"/> class.
            /// </summary>
            /// <param name="options">The Entity Framework options used to configure the context.</param>
            public SalesDbContext(
                DbContextOptions<SalesDbContext> options)
                : base(options)
            { }

            /// <summary>
            /// Configures the Entity Framework model used by the tests.
            /// </summary>
            /// <param name="modelBuilder">The model builder that receives the mappings.</param>
            protected override void OnModelCreating(
                ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<SalesCustomer>(entity =>
                {
                    entity.ToTable("customers", "sales");
                    entity.HasKey(customer => customer.Id);
                    entity.Property(customer => customer.Id)
                        .HasColumnName("id");
                    entity.Property(customer => customer.FullName)
                        .HasColumnName("full_name");
                });

                modelBuilder.Entity<SalesOrder>(entity =>
                {
                    entity.ToTable("sales_orders", "sales");
                    entity.HasKey(order => order.Id);
                    entity.Property(order => order.Id)
                        .HasColumnName("id");
                    entity.Property(order => order.Number)
                        .HasColumnName("order_number");
                    entity.Property(order => order.Status)
                        .HasColumnName("status_id");
                    entity.Property(order => order.CustomerId)
                        .HasColumnName("customer_id");

                    entity.HasOne(order => order.Customer)
                        .WithMany()
                        .HasForeignKey(order => order.CustomerId)
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    entity.HasMany(order => order.Lines)
                        .WithOne(line => line.Order)
                        .HasForeignKey(line => line.OrderId)
                        .OnDelete(DeleteBehavior.NoAction);
                });

                modelBuilder.Entity<SalesOrderLine>(entity =>
                {
                    entity.ToTable("sales_order_lines", "sales");
                    entity.HasKey(line => line.Id);
                    entity.Property(line => line.Id)
                        .HasColumnName("id");
                    entity.Property(line => line.OrderId)
                        .HasColumnName("order_id");
                    entity.Property(line => line.Quantity);
                });

                modelBuilder.Entity<SalesProjection>(entity =>
                {
                    entity.ToView("sales_projection", "sales");
                    entity.HasNoKey();
                    entity.Property(projection => projection.Label);
                });

                modelBuilder.Entity<SalesAudit>(entity =>
                {
                    entity.ToTable("sales_audits", "sales");
                    entity.HasKey(audit => audit.Id);
                    entity.Property(audit => audit.Id)
                        .HasColumnName("id");
                    entity.Property(audit => audit.OrderId)
                        .HasColumnName("order_id");
                });
            }
        }
    }
}
