using Hydrix.Mapper.Attributes;
using Hydrix.Mapper.Configuration;
using System;
using System.Collections.Generic;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Mapping
{
    /// <summary>
    /// Validates the nested DTO mapping feature, including direct object nesting, collection nesting, null propagation,
    /// attribute-driven mapping, and explicit registration via <see cref="HydrixMapperOptions.MapNested{TSource,TDest}"/>.
    /// </summary>
    public class HydrixMapperNestedMappingTests
    {
        // -----------------------------------------------------------------------------------------
        // Source models
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Represents a source address entity with basic location fields.
        /// </summary>
        private sealed class AddressEntity
        {
            /// <summary>Gets or sets the street address line.</summary>
            public string Street { get; set; }

            /// <summary>Gets or sets the city name.</summary>
            public string City { get; set; }

            /// <summary>Gets or sets the country name.</summary>
            public string Country { get; set; }
        }

        /// <summary>
        /// Represents a source customer entity with a nested address and a collection of orders.
        /// </summary>
        private sealed class CustomerEntity
        {
            /// <summary>Gets or sets the customer identifier.</summary>
            public int Id { get; set; }

            /// <summary>Gets or sets the customer name.</summary>
            public string Name { get; set; }

            /// <summary>Gets or sets the nested address entity.</summary>
            public AddressEntity Address { get; set; }

            /// <summary>Gets or sets the list of orders belonging to this customer.</summary>
            public List<OrderEntity> Orders { get; set; }
        }

        /// <summary>
        /// Represents a source order entity with a nested list of line items.
        /// </summary>
        private sealed class OrderEntity
        {
            /// <summary>Gets or sets the order identifier.</summary>
            public int OrderId { get; set; }

            /// <summary>Gets or sets the order total amount.</summary>
            public decimal Total { get; set; }

            /// <summary>Gets or sets the line items belonging to this order.</summary>
            public List<LineItemEntity> Items { get; set; }
        }

        /// <summary>
        /// Represents a source line item entity.
        /// </summary>
        private sealed class LineItemEntity
        {
            /// <summary>Gets or sets the product identifier.</summary>
            public int ProductId { get; set; }

            /// <summary>Gets or sets the item quantity.</summary>
            public int Quantity { get; set; }
        }

        // -----------------------------------------------------------------------------------------
        // Destination models
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Represents a destination address DTO decorated with <see cref="MapFromAttribute"/> pointing to
        /// <see cref="AddressEntity"/>.
        /// </summary>
        [MapFrom(typeof(AddressEntity))]
        private sealed class AddressDto
        {
            /// <summary>Gets or sets the street address line.</summary>
            public string Street { get; set; }

            /// <summary>Gets or sets the city name.</summary>
            public string City { get; set; }

            /// <summary>Gets or sets the country name.</summary>
            public string Country { get; set; }
        }

        /// <summary>
        /// Represents a destination customer DTO that includes a nested <see cref="AddressDto"/> and a list of
        /// <see cref="OrderDto"/> items.
        /// </summary>
        private sealed class CustomerDto
        {
            /// <summary>Gets or sets the customer identifier.</summary>
            public int Id { get; set; }

            /// <summary>Gets or sets the customer name.</summary>
            public string Name { get; set; }

            /// <summary>Gets or sets the nested address DTO.</summary>
            public AddressDto Address { get; set; }

            /// <summary>Gets or sets the list of order DTOs.</summary>
            public List<OrderDto> Orders { get; set; }
        }

        /// <summary>
        /// Represents a destination order DTO with a nested list of line item DTOs.
        /// </summary>
        private sealed class OrderDto
        {
            /// <summary>Gets or sets the order identifier.</summary>
            public int OrderId { get; set; }

            /// <summary>Gets or sets the order total amount.</summary>
            public decimal Total { get; set; }

            /// <summary>Gets or sets the list of line item DTOs.</summary>
            public List<LineItemDto> Items { get; set; }
        }

        /// <summary>
        /// Represents a destination line item DTO.
        /// </summary>
        private sealed class LineItemDto
        {
            /// <summary>Gets or sets the product identifier.</summary>
            public int ProductId { get; set; }

            /// <summary>Gets or sets the item quantity.</summary>
            public int Quantity { get; set; }
        }

        /// <summary>
        /// Represents a simplified destination customer DTO that only carries the nested address property, used by
        /// tests that exercise address-only mapping without registering an order mapping.
        /// </summary>
        private sealed class CustomerAddressOnlyDto
        {
            /// <summary>Gets or sets the customer identifier.</summary>
            public int Id { get; set; }

            /// <summary>Gets or sets the customer name.</summary>
            public string Name { get; set; }

            /// <summary>Gets or sets the nested address DTO.</summary>
            public AddressDto Address { get; set; }
        }

        /// <summary>
        /// Represents a destination customer DTO whose orders property is typed as
        /// <see cref="IReadOnlyList{T}"/> to verify covariant collection mapping.
        /// </summary>
        private sealed class CustomerDtoWithIReadOnlyList
        {
            /// <summary>Gets or sets the customer identifier.</summary>
            public int Id { get; set; }

            /// <summary>Gets or sets the customer name.</summary>
            public string Name { get; set; }

            /// <summary>Gets or sets the read-only list of order DTOs.</summary>
            public IReadOnlyList<OrderDto> Orders { get; set; }
        }

        // -----------------------------------------------------------------------------------------
        // Tests
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Verifies that a nested object property is mapped correctly when the relationship is registered via
        /// <see cref="HydrixMapperOptions.MapNested{TSource,TDest}"/>.
        /// </summary>
        [Fact]
        public void MapNested_Object_MapsNestedProperty_ViaRegistration()
        {
            var options = new HydrixMapperOptions();
            options.MapNested<AddressEntity, AddressDto>();

            var mapper = new HydrixMapper(options);

            var entity = new CustomerEntity
            {
                Id = 1,
                Name = "Alice",
                Address = new AddressEntity
                {
                    Street = "123 Main St",
                    City = "Springfield",
                    Country = "US",
                },
            };

            var dto = mapper.Map<CustomerAddressOnlyDto>(entity);

            Assert.NotNull(dto.Address);
            Assert.Equal("123 Main St", dto.Address.Street);
            Assert.Equal("Springfield", dto.Address.City);
            Assert.Equal("US", dto.Address.Country);
        }

        /// <summary>
        /// Verifies that a nested object property is mapped correctly when the destination type carries
        /// <see cref="MapFromAttribute"/> without any explicit registration.
        /// </summary>
        [Fact]
        public void MapNested_Object_MapsNestedProperty_ViaMapFromAttribute()
        {
            var mapper = new HydrixMapper(new HydrixMapperOptions());

            var entity = new CustomerEntity
            {
                Id = 2,
                Name = "Bob",
                Address = new AddressEntity
                {
                    Street = "456 Oak Ave",
                    City = "Shelbyville",
                    Country = "CA",
                },
            };

            var dto = mapper.Map<CustomerAddressOnlyDto>(entity);

            Assert.NotNull(dto.Address);
            Assert.Equal("456 Oak Ave", dto.Address.Street);
            Assert.Equal("Shelbyville", dto.Address.City);
            Assert.Equal("CA", dto.Address.Country);
        }

        /// <summary>
        /// Verifies that when the source nested property is null the destination nested property is also null.
        /// </summary>
        [Fact]
        public void MapNested_Object_NullNestedProperty_YieldsNullInDestination()
        {
            var mapper = new HydrixMapper(new HydrixMapperOptions());

            var entity = new CustomerEntity
            {
                Id = 3,
                Name = "Carol",
                Address = null,
            };

            var dto = mapper.Map<CustomerAddressOnlyDto>(entity);

            Assert.Null(dto.Address);
        }

        /// <summary>
        /// Verifies that nested collections are fully mapped including deeply nested sub-collections when all
        /// relationships are registered via <see cref="HydrixMapperOptions.MapNested{TSource,TDest}"/>.
        /// </summary>
        [Fact]
        public void MapNested_Collection_MapsList_ViaRegistration()
        {
            var options = new HydrixMapperOptions();
            options.MapNested<LineItemEntity, LineItemDto>();
            options.MapNested<OrderEntity, OrderDto>();

            var mapper = new HydrixMapper(options);

            var entity = new CustomerEntity
            {
                Id = 4,
                Name = "Dave",
                Orders = new List<OrderEntity>
                {
                    new OrderEntity
                    {
                        OrderId = 10,
                        Total = 99.95m,
                        Items = new List<LineItemEntity>
                        {
                            new LineItemEntity { ProductId = 1, Quantity = 2 },
                            new LineItemEntity { ProductId = 2, Quantity = 3 },
                        },
                    },
                    new OrderEntity
                    {
                        OrderId = 11,
                        Total = 49.50m,
                        Items = new List<LineItemEntity>
                        {
                            new LineItemEntity { ProductId = 3, Quantity = 1 },
                        },
                    },
                },
            };

            var dto = mapper.Map<CustomerDto>(entity);

            Assert.NotNull(dto.Orders);
            Assert.Equal(2, dto.Orders.Count);
            Assert.Equal(10, dto.Orders[0].OrderId);
            Assert.Equal(99.95m, dto.Orders[0].Total);
            Assert.Equal(2, dto.Orders[0].Items.Count);
            Assert.Equal(1, dto.Orders[0].Items[0].ProductId);
            Assert.Equal(2, dto.Orders[0].Items[0].Quantity);
            Assert.Equal(2, dto.Orders[0].Items[1].ProductId);
            Assert.Equal(3, dto.Orders[0].Items[1].Quantity);
            Assert.Equal(11, dto.Orders[1].OrderId);
            Assert.Equal(49.50m, dto.Orders[1].Total);
            Assert.Single(dto.Orders[1].Items);
            Assert.Equal(3, dto.Orders[1].Items[0].ProductId);
            Assert.Equal(1, dto.Orders[1].Items[0].Quantity);
        }

        /// <summary>
        /// Verifies that an <see cref="IReadOnlyList{T}"/> destination collection property is populated correctly
        /// from a <see cref="List{T}"/> source collection.
        /// </summary>
        [Fact]
        public void MapNested_Collection_MapsIReadOnlyListDestination()
        {
            var options = new HydrixMapperOptions();
            options.MapNested<LineItemEntity, LineItemDto>();
            options.MapNested<OrderEntity, OrderDto>();

            var mapper = new HydrixMapper(options);

            var entity = new CustomerEntity
            {
                Id = 5,
                Name = "Eve",
                Orders = new List<OrderEntity>
                {
                    new OrderEntity
                    {
                        OrderId = 20,
                        Total = 10.00m,
                        Items = new List<LineItemEntity>(),
                    },
                },
            };

            var dto = mapper.Map<CustomerDtoWithIReadOnlyList>(entity);

            Assert.NotNull(dto.Orders);
            Assert.Equal(1, dto.Orders.Count);
            Assert.Equal(20, dto.Orders[0].OrderId);
            Assert.Equal(10.00m, dto.Orders[0].Total);
        }

        /// <summary>
        /// Verifies that a null source collection yields a null destination collection.
        /// </summary>
        [Fact]
        public void MapNested_Collection_NullCollection_YieldsNullList()
        {
            var options = new HydrixMapperOptions();
            options.MapNested<LineItemEntity, LineItemDto>();
            options.MapNested<OrderEntity, OrderDto>();

            var mapper = new HydrixMapper(options);

            var entity = new CustomerEntity
            {
                Id = 6,
                Name = "Frank",
                Orders = null,
            };

            var dto = mapper.Map<CustomerDto>(entity);

            Assert.Null(dto.Orders);
        }

        /// <summary>
        /// Verifies that null elements inside a source collection are skipped and do not appear in the destination list.
        /// </summary>
        [Fact]
        public void MapNested_Collection_NullElementsSkipped()
        {
            var options = new HydrixMapperOptions();
            options.MapNested<LineItemEntity, LineItemDto>();
            options.MapNested<OrderEntity, OrderDto>();

            var mapper = new HydrixMapper(options);

            var entity = new CustomerEntity
            {
                Id = 7,
                Name = "Grace",
                Orders = new List<OrderEntity>
                {
                    new OrderEntity
                    {
                        OrderId = 30,
                        Total = 5.00m,
                        Items = null,
                    },
                    null,
                    new OrderEntity
                    {
                        OrderId = 31,
                        Total = 15.00m,
                        Items = null,
                    },
                },
            };

            var dto = mapper.Map<CustomerDto>(entity);

            Assert.NotNull(dto.Orders);
            Assert.Equal(2, dto.Orders.Count);
            Assert.Equal(30, dto.Orders[0].OrderId);
            Assert.Equal(31, dto.Orders[1].OrderId);
        }

        /// <summary>
        /// Verifies that primitive properties, a nested object property, and nested collection properties are all
        /// mapped correctly in a single mapping call.
        /// </summary>
        [Fact]
        public void MapNested_PrimitivesAndNestedMixed_AllPropertiesMapped()
        {
            var options = new HydrixMapperOptions();
            options.MapNested<LineItemEntity, LineItemDto>();
            options.MapNested<OrderEntity, OrderDto>();

            var mapper = new HydrixMapper(options);

            var entity = new CustomerEntity
            {
                Id = 8,
                Name = "Hank",
                Address = new AddressEntity
                {
                    Street = "789 Pine Rd",
                    City = "Ogdenville",
                    Country = "US",
                },
                Orders = new List<OrderEntity>
                {
                    new OrderEntity
                    {
                        OrderId = 40,
                        Total = 200.00m,
                        Items = new List<LineItemEntity>
                        {
                            new LineItemEntity { ProductId = 5, Quantity = 10 },
                        },
                    },
                },
            };

            var dto = mapper.Map<CustomerDto>(entity);

            Assert.Equal(8, dto.Id);
            Assert.Equal("Hank", dto.Name);
            Assert.NotNull(dto.Address);
            Assert.Equal("789 Pine Rd", dto.Address.Street);
            Assert.Equal("Ogdenville", dto.Address.City);
            Assert.Equal("US", dto.Address.Country);
            Assert.NotNull(dto.Orders);
            Assert.Single(dto.Orders);
            Assert.Equal(40, dto.Orders[0].OrderId);
            Assert.Equal(200.00m, dto.Orders[0].Total);
            Assert.Single(dto.Orders[0].Items);
            Assert.Equal(5, dto.Orders[0].Items[0].ProductId);
            Assert.Equal(10, dto.Orders[0].Items[0].Quantity);
        }

        /// <summary>
        /// Verifies that mapping the same type pair twice produces correct results both times, confirming that the
        /// compiled plan cached after the first call is reused without error.
        /// </summary>
        [Fact]
        public void MapNested_TwiceSameTypePair_ReusesCompiledPlan()
        {
            var mapper = new HydrixMapper(new HydrixMapperOptions());

            var entity = new CustomerEntity
            {
                Id = 9,
                Name = "Iris",
                Address = new AddressEntity
                {
                    Street = "1 First St",
                    City = "Brockway",
                    Country = "US",
                },
            };

            var dto1 = mapper.Map<CustomerAddressOnlyDto>(entity);
            var dto2 = mapper.Map<CustomerAddressOnlyDto>(entity);

            Assert.Equal(9, dto1.Id);
            Assert.Equal("Brockway", dto1.Address.City);
            Assert.Equal(9, dto2.Id);
            Assert.Equal("Brockway", dto2.Address.City);
        }

        /// <summary>
        /// Represents a source entity used exclusively to trigger an incompatible-type mapping failure.
        /// </summary>
        private sealed class IncompatibleSourceEntity
        {
            /// <summary>Gets or sets a property whose type does not match the corresponding destination property.</summary>
            public AddressEntity Address { get; set; }
        }

        /// <summary>
        /// Represents a destination DTO used exclusively to trigger an incompatible-type mapping failure.
        /// </summary>
        private sealed class IncompatibleDestDto
        {
            /// <summary>Gets or sets a property whose type does not match the corresponding source property.</summary>
            public OrderDto Address { get; set; }
        }

        // -----------------------------------------------------------------------------------------
        // Circular collection element detection
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Represents a self-referential entity whose <c>Children</c> collection is typed as
        /// <see cref="IEnumerable{T}"/> rather than <see cref="List{T}"/>, which forces the enumerable-fallback code
        /// path in the builder and ensures <c>GetOrAddElementDelegate</c> is reached during plan compilation.
        /// </summary>
        private sealed class CircularCollectionElementEntity
        {
            /// <summary>Gets or sets the element value.</summary>
            public int Value { get; set; }

            /// <summary>
            /// Gets or sets the recursive child collection. Typed as <see cref="IEnumerable{T}"/> so that the source
            /// property bypasses the indexed-loop fast path and routes through <c>BuildEnumerableFallback</c>.
            /// </summary>
            public IEnumerable<CircularCollectionElementEntity> Children { get; set; }
        }

        /// <summary>
        /// Destination DTO for <see cref="CircularCollectionElementEntity"/>. The <c>Children</c> property mirrors the
        /// source structure, closing the circular element-mapping path during plan compilation.
        /// </summary>
        private sealed class CircularCollectionElementDto
        {
            /// <summary>Gets or sets the mapped element value.</summary>
            public int Value { get; set; }

            /// <summary>Gets or sets the recursive child DTO collection.</summary>
            public List<CircularCollectionElementDto> Children { get; set; }
        }

        /// <summary>
        /// Represents an outer container source entity whose collection property is typed as
        /// <see cref="IEnumerable{T}"/>, forcing the enumerable-fallback path so that the element-delegate lazy is
        /// added to <c>ElementDelegateCache</c> before the self-referential cycle inside the element type is reached.
        /// </summary>
        private sealed class CircularCollectionContainerEntity
        {
            /// <summary>Gets or sets the container identifier.</summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the nested element collection. Typed as <see cref="IEnumerable{T}"/> to bypass the
            /// indexed-loop fast path and ensure <c>GetOrAddElementDelegate</c> populates the lazy cache entry before
            /// the recursive compilation of the element type encounters the cycle.
            /// </summary>
            public IEnumerable<CircularCollectionElementEntity> Items { get; set; }
        }

        /// <summary>
        /// Destination DTO for <see cref="CircularCollectionContainerEntity"/>. Its <c>Items</c> element type maps to
        /// <see cref="CircularCollectionElementDto"/>, which is self-referential, triggering the fault-removal paths in
        /// <c>GetElementDelegateValue</c> and <c>TryRemoveFaultedElementDelegate</c> during compilation.
        /// </summary>
        private sealed class CircularCollectionContainerDto
        {
            /// <summary>Gets or sets the mapped container identifier.</summary>
            public int Id { get; set; }

            /// <summary>Gets or sets the mapped element collection.</summary>
            public List<CircularCollectionElementDto> Items { get; set; }
        }

        // -----------------------------------------------------------------------------------------
        // Circular mapping detection
        // -----------------------------------------------------------------------------------------

        // -----------------------------------------------------------------------------------------
        // Indirect circular mapping detection (A → B → C → B)
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// First leg of the indirect cycle: entity A contains entity B.
        /// </summary>
        private sealed class IndirectCycleEntityA
        {
            /// <summary>Gets or sets the nested B entity.</summary>
            public IndirectCycleEntityB B { get; set; }
        }

        /// <summary>
        /// Second leg of the indirect cycle: entity B contains entity C.
        /// </summary>
        private sealed class IndirectCycleEntityB
        {
            /// <summary>Gets or sets the nested C entity.</summary>
            public IndirectCycleEntityC C { get; set; }
        }

        /// <summary>
        /// Third leg of the indirect cycle: entity C contains entity B, closing the cycle B → C → B.
        /// </summary>
        private sealed class IndirectCycleEntityC
        {
            /// <summary>Gets or sets the back-reference to entity B, which closes the cycle.</summary>
            public IndirectCycleEntityB Back { get; set; }
        }

        /// <summary>
        /// Destination DTO for <see cref="IndirectCycleEntityA"/>.
        /// </summary>
        [MapFrom(typeof(IndirectCycleEntityA))]
        private sealed class IndirectCycleDtoA
        {
            /// <summary>Gets or sets the nested B DTO.</summary>
            public IndirectCycleDtoB B { get; set; }
        }

        /// <summary>
        /// Destination DTO for <see cref="IndirectCycleEntityB"/>.
        /// </summary>
        [MapFrom(typeof(IndirectCycleEntityB))]
        private sealed class IndirectCycleDtoB
        {
            /// <summary>Gets or sets the nested C DTO.</summary>
            public IndirectCycleDtoC C { get; set; }
        }

        /// <summary>
        /// Destination DTO for <see cref="IndirectCycleEntityC"/>. Its <c>Back</c> property references
        /// <see cref="IndirectCycleDtoB"/>, which is already being compiled when C's body is built, closing
        /// the indirect cycle B → C → B.
        /// </summary>
        [MapFrom(typeof(IndirectCycleEntityC))]
        private sealed class IndirectCycleDtoC
        {
            /// <summary>Gets or sets the back-reference DTO that closes the cycle.</summary>
            public IndirectCycleDtoB Back { get; set; }
        }

        /// <summary>
        /// Represents a self-referential source entity used to trigger circular mapping detection.
        /// </summary>
        private sealed class CircularEntity
        {
            /// <summary>Gets or sets the identifier.</summary>
            public int Id { get; set; }

            /// <summary>Gets or sets the self-referential child, whose mapping creates a compile-time cycle.</summary>
            public CircularEntity Child { get; set; }
        }

        /// <summary>
        /// Represents a self-referential destination DTO whose nested property creates a circular plan-compile cycle.
        /// </summary>
        [MapFrom(typeof(CircularEntity))]
        private sealed class CircularDto
        {
            /// <summary>Gets or sets the identifier.</summary>
            public int Id { get; set; }

            /// <summary>Gets or sets the self-referential child DTO, which triggers the circular detection guard.</summary>
            public CircularDto Child { get; set; }
        }

        /// <summary>
        /// Represents an outer source entity whose nested property type does not match the source type registered
        /// for the corresponding destination property — used to exercise the type-mismatch branch in
        /// <c>TryBuildNestedExpression</c>.
        /// </summary>
        private sealed class MismatchedOuterEntity
        {
            /// <summary>Gets or sets a nested property whose type differs from the registered nested source.</summary>
            public OrderEntity Inner { get; set; }
        }

        /// <summary>
        /// Represents the destination DTO for <see cref="MismatchedOuterEntity"/> whose <c>Inner</c> property is typed
        /// as <see cref="AddressDto"/>. Because <see cref="AddressDto"/> declares
        /// <c>[MapFrom(typeof(AddressEntity))]</c> while the source property is <see cref="OrderEntity"/>, the
        /// registered source type does not match the actual source property type.
        /// </summary>
        [MapFrom(typeof(MismatchedOuterEntity))]
        private sealed class MismatchedOuterDto
        {
            /// <summary>Gets or sets the nested address DTO whose registered source type is <see cref="AddressEntity"/>,
            /// not the actual <see cref="OrderEntity"/> property on the source.</summary>
            public AddressDto Inner { get; set; }
        }

        /// <summary>
        /// Verifies that attempting to compile a plan for a type pair that creates a circular nested mapping cycle
        /// throws <see cref="InvalidOperationException"/> with a descriptive message.
        /// </summary>
        [Fact]
        public void MapNested_CircularMapping_ThrowsInvalidOperationException()
        {
            var mapper = new HydrixMapper(new HydrixMapperOptions());
            var entity = new CircularEntity { Id = 1, Child = new CircularEntity { Id = 2 } };

            var ex = Assert.Throws<InvalidOperationException>(
                () => mapper.Map<CircularDto>(entity));

            Assert.Contains(
                "circular",
                ex.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that an indirect cycle (A → B → C → B) is detected during plan compilation and throws
        /// <see cref="InvalidOperationException"/> with a descriptive message. This exercises the
        /// <c>WithCompilingPair</c> guard applied to <c>BuildInlineNestedBodyBlock</c>, which must detect
        /// cycles that do not re-enter <c>Build()</c> directly.
        /// </summary>
        [Fact]
        public void MapNested_IndirectCycleDetection_ThrowsInvalidOperationException()
        {
            var mapper = new HydrixMapper(new HydrixMapperOptions());
            var entity = new IndirectCycleEntityA
            {
                B = new IndirectCycleEntityB
                {
                    C = new IndirectCycleEntityC(),
                },
            };

            var ex = Assert.Throws<InvalidOperationException>(
                () => mapper.Map<IndirectCycleDtoA>(entity));

            Assert.Contains(
                "circular",
                ex.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that directly mapping a type whose collection element type is itself throws
        /// <see cref="InvalidOperationException"/> with a descriptive message. This exercises the
        /// <c>IsCompilingPair</c> guard inside <c>GetOrAddElementDelegate</c> at the branch where the element
        /// delegate has not yet been added to the cache (i.e., the pair is in <c>_compilingPairs</c> via the
        /// outer <c>Build</c> call but the lazy is absent from <c>ElementDelegateCache</c>).
        /// </summary>
        [Fact]
        public void MapNested_DirectCircularCollectionElement_ThrowsInvalidOperationException()
        {
            var options = new HydrixMapperOptions();
            options.MapNested<CircularCollectionElementEntity, CircularCollectionElementDto>();
            var mapper = new HydrixMapper(
                options);

            var ex = Assert.Throws<InvalidOperationException>(
                () => mapper.Map<CircularCollectionElementDto>(
                    new CircularCollectionElementEntity()));

            Assert.Contains(
                "circular",
                ex.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that mapping a container type whose collection element type is self-referential throws
        /// <see cref="InvalidOperationException"/> with a descriptive message. This exercises the
        /// <c>IsCompilingPair</c> guard inside <c>GetOrAddElementDelegate</c> at the branch where the element
        /// delegate lazy is already present in the cache but its value has not yet been created (because the
        /// factory is still executing on the current thread), and exercises the fault-removal paths inside
        /// <c>GetElementDelegateValue</c> and <c>TryRemoveFaultedElementDelegate</c>.
        /// </summary>
        [Fact]
        public void MapNested_NestedCircularCollectionElement_ThrowsInvalidOperationException()
        {
            var options = new HydrixMapperOptions();
            options.MapNested<CircularCollectionElementEntity, CircularCollectionElementDto>();
            var mapper = new HydrixMapper(
                options);

            var ex = Assert.Throws<InvalidOperationException>(
                () => mapper.Map<CircularCollectionContainerDto>(
                    new CircularCollectionContainerEntity()));

            Assert.Contains(
                "circular",
                ex.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that when a destination property's registered source type (resolved via
        /// <see cref="MapFromAttribute"/>) does not match the actual source property type, the mapper rejects
        /// the mapping with an <see cref="InvalidOperationException"/>.
        /// </summary>
        [Fact]
        public void MapNested_SourceTypeRegistrationMismatch_ThrowsInvalidOperationException()
        {
            var mapper = new HydrixMapper(new HydrixMapperOptions());
            var entity = new MismatchedOuterEntity
            {
                Inner = new OrderEntity { OrderId = 1 },
            };

            Assert.Throws<InvalidOperationException>(
                () => mapper.Map<MismatchedOuterDto>(entity));
        }

        /// <summary>
        /// Verifies that attempting to map incompatible property types without any nested registration or attribute
        /// throws <see cref="InvalidOperationException"/> at plan-compile time.
        /// </summary>
        [Fact]
        public void MapNested_IncompatibleTypes_NoRegistration_Throws()
        {
            var mapper = new HydrixMapper(new HydrixMapperOptions());

            // IncompatibleSourceEntity.Address is AddressEntity; IncompatibleDestDto.Address is OrderDto.
            // AddressEntity and OrderDto share no [MapFrom] relationship and are not registered,
            // so the mapper cannot build a conversion and must throw.
            var entity = new IncompatibleSourceEntity
            {
                Address = new AddressEntity
                {
                    Street = "1 Test St",
                    City = "Testville",
                    Country = "US",
                },
            };

            Assert.Throws<InvalidOperationException>(
                () => mapper.Map<IncompatibleDestDto>(entity));
        }

        /// <summary>
        /// Verifies that the <see cref="MapFromAttribute.SourceType"/> property returns the type supplied at
        /// construction.
        /// </summary>
        [Fact]
        public void MapFromAttribute_SourceType_IsCorrect()
        {
            var attribute = new MapFromAttribute(typeof(AddressEntity));

            Assert.Equal(
                typeof(AddressEntity),
                attribute.SourceType);
        }

        /// <summary>
        /// Verifies that an empty source collection produces an empty (not null) destination list.
        /// </summary>
        [Fact]
        public void MapNested_Collection_EmptyList_YieldsEmptyList()
        {
            var options = new HydrixMapperOptions();
            options.MapNested<LineItemEntity, LineItemDto>();
            options.MapNested<OrderEntity, OrderDto>();

            var mapper = new HydrixMapper(options);

            var entity = new CustomerEntity
            {
                Id = 10,
                Name = "Jack",
                Orders = new List<OrderEntity>(),
            };

            var dto = mapper.Map<CustomerDto>(entity);

            Assert.NotNull(dto.Orders);
            Assert.Empty(dto.Orders);
        }

        // -----------------------------------------------------------------------------------------
        // IEnumerable source — exercises the BuildEnumerableFallback path (non-IList source)
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Represents a source entity whose collection property is typed as <see cref="IEnumerable{T}"/> to exercise the
        /// <c>BuildEnumerableFallback</c> path in the expression builder when the property type matches the enumerable
        /// type exactly (no conversion node required).
        /// </summary>
        private sealed class TaggedEntity
        {
            /// <summary>Gets or sets the entity identifier.</summary>
            public int Id { get; set; }

            /// <summary>Gets or sets the tag items as a deferred enumerable (not List or IList).</summary>
            public IEnumerable<TagItemEntity> Tags { get; set; }
        }

        /// <summary>
        /// Represents a source entity whose collection property is typed as <see cref="HashSet{T}"/>, an
        /// <see cref="IEnumerable{T}"/> implementation that is NOT <see cref="IList{T}"/>, to exercise the
        /// <c>BuildEnumerableFallback</c> convert path (property type ≠ <c>IEnumerable&lt;T&gt;</c>).
        /// </summary>
        private sealed class HashSetTaggedEntity
        {
            /// <summary>Gets or sets the entity identifier.</summary>
            public int Id { get; set; }

            /// <summary>Gets or sets the tag items stored in a hash set.</summary>
            public HashSet<TagItemEntity> Tags { get; set; }
        }

        /// <summary>
        /// Represents a destination DTO whose tags collection is typed as <see cref="IReadOnlyList{T}"/> to exercise the
        /// destination-type convert path inside <c>BuildEnumerableFallback</c>.
        /// </summary>
        private sealed class TaggedReadOnlyDto
        {
            /// <summary>Gets or sets the entity identifier.</summary>
            public int Id { get; set; }

            /// <summary>Gets or sets the mapped tag DTOs as a read-only list.</summary>
            public IReadOnlyList<TagItemDto> Tags { get; set; }
        }

        /// <summary>
        /// Represents a source entity whose collection property is typed as an array, which implements
        /// <see cref="IList{T}"/> but is neither <see cref="List{T}"/> nor <see cref="IList{T}"/> itself.
        /// This exercises the indexed-loop convert path (source type ≠ <c>indexedType</c>).
        /// </summary>
        private sealed class ArrayTaggedEntity
        {
            /// <summary>Gets or sets the entity identifier.</summary>
            public int Id { get; set; }

            /// <summary>Gets or sets the tag items stored in an array.</summary>
            public TagItemEntity[] Tags { get; set; }
        }

        /// <summary>
        /// Represents a source entity whose collection property is typed as <see cref="IList{T}"/>, which exercises the
        /// <c>ilistSrcType</c> branch in the indexed-loop dispatcher (source type == <c>indexedType</c>, no convert).
        /// </summary>
        private sealed class IListTaggedEntity
        {
            /// <summary>Gets or sets the entity identifier.</summary>
            public int Id { get; set; }

            /// <summary>Gets or sets the tag items stored behind an IList interface.</summary>
            public IList<TagItemEntity> Tags { get; set; }
        }

        /// <summary>
        /// Represents a single tag source element.
        /// </summary>
        private sealed class TagItemEntity
        {
            /// <summary>Gets or sets the tag label.</summary>
            public string Label { get; set; }
        }

        /// <summary>
        /// Represents the destination DTO whose tags collection comes from an <see cref="IEnumerable{T}"/> source.
        /// </summary>
        private sealed class TaggedDto
        {
            /// <summary>Gets or sets the entity identifier.</summary>
            public int Id { get; set; }

            /// <summary>Gets or sets the mapped tag DTOs.</summary>
            public List<TagItemDto> Tags { get; set; }
        }

        /// <summary>
        /// Represents the destination DTO for a single tag element.
        /// </summary>
        [MapFrom(typeof(TagItemEntity))]
        private sealed class TagItemDto
        {
            /// <summary>Gets or sets the tag label.</summary>
            public string Label { get; set; }
        }

        /// <summary>
        /// Verifies that a source collection typed as <see cref="IEnumerable{T}"/> (not <see cref="List{T}"/> or
        /// <see cref="IList{T}"/>) is mapped correctly through the enumerable fallback path.
        /// </summary>
        [Fact]
        public void MapNested_Collection_IEnumerableSource_UsesEnumerableFallback()
        {
            var mapper = new HydrixMapper(new HydrixMapperOptions());

            var entity = new TaggedEntity
            {
                Id = 99,
                Tags = new HashSet<TagItemEntity>
                {
                    new TagItemEntity { Label = "alpha" },
                    new TagItemEntity { Label = "beta" },
                },
            };

            var dto = mapper.Map<TaggedDto>(entity);

            Assert.Equal(99, dto.Id);
            Assert.NotNull(dto.Tags);
            Assert.Equal(2, dto.Tags.Count);
        }

        /// <summary>
        /// Verifies that a null <see cref="IEnumerable{T}"/> source collection maps to a null destination collection
        /// through the enumerable fallback path.
        /// </summary>
        [Fact]
        public void MapNested_Collection_IEnumerableNullSource_YieldsNullDestination()
        {
            var mapper = new HydrixMapper(new HydrixMapperOptions());

            var entity = new TaggedEntity
            {
                Id = 100,
                Tags = null,
            };

            var dto = mapper.Map<TaggedDto>(entity);

            Assert.Equal(100, dto.Id);
            Assert.Null(dto.Tags);
        }

        /// <summary>
        /// Verifies that a <see cref="HashSet{T}"/> source collection (not <see cref="IList{T}"/>, not
        /// <see cref="IEnumerable{T}"/> property) is mapped through the enumerable fallback convert path and produces a
        /// correct destination list.
        /// </summary>
        [Fact]
        public void MapNested_Collection_HashSetSource_UsesEnumerableFallbackConvertPath()
        {
            var mapper = new HydrixMapper(new HydrixMapperOptions());

            var entity = new HashSetTaggedEntity
            {
                Id = 101,
                Tags = new HashSet<TagItemEntity>
                {
                    new TagItemEntity { Label = "x" },
                },
            };

            var dto = mapper.Map<TaggedDto>(entity);

            Assert.Equal(101, dto.Id);
            Assert.NotNull(dto.Tags);
            Assert.Single(dto.Tags);
        }

        /// <summary>
        /// Verifies that a <see cref="HashSet{T}"/> source collection maps to an <see cref="IReadOnlyList{T}"/>
        /// destination property through the enumerable fallback path, exercising the destination-type convert node.
        /// </summary>
        [Fact]
        public void MapNested_Collection_HashSetSource_IReadOnlyListDest_UsesEnumerableFallbackBothConvertPaths()
        {
            var mapper = new HydrixMapper(new HydrixMapperOptions());

            var entity = new HashSetTaggedEntity
            {
                Id = 102,
                Tags = new HashSet<TagItemEntity>
                {
                    new TagItemEntity { Label = "y" },
                    new TagItemEntity { Label = "z" },
                },
            };

            var dto = mapper.Map<TaggedReadOnlyDto>(entity);

            Assert.Equal(102, dto.Id);
            Assert.NotNull(dto.Tags);
            Assert.Equal(2, dto.Tags.Count);
        }

        /// <summary>
        /// Verifies that a source collection typed as <see cref="IList{T}"/> (not <see cref="List{T}"/>) is mapped
        /// using the indexed loop via the <c>ilistSrcType</c> branch without a source-to-indexedType conversion.
        /// </summary>
        [Fact]
        public void MapNested_Collection_IListSource_UsesIndexedLoopIListBranch()
        {
            var mapper = new HydrixMapper(new HydrixMapperOptions());

            var entity = new IListTaggedEntity
            {
                Id = 104,
                Tags = new List<TagItemEntity>
                {
                    new TagItemEntity { Label = "p" },
                    new TagItemEntity { Label = "q" },
                },
            };

            var dto = mapper.Map<TaggedDto>(entity);

            Assert.Equal(104, dto.Id);
            Assert.NotNull(dto.Tags);
            Assert.Equal(2, dto.Tags.Count);
        }

        /// <summary>
        /// Verifies that an array source collection (which implements <see cref="IList{T}"/> but is not
        /// <see cref="List{T}"/> or <see cref="IList{T}"/> itself) is mapped using the indexed loop with a convert node,
        /// exercising both the <c>ilistSrcType</c> branch and the source-to-indexedType cast path.
        /// </summary>
        [Fact]
        public void MapNested_Collection_ArraySource_UsesIndexedLoopWithConvert()
        {
            var mapper = new HydrixMapper(new HydrixMapperOptions());

            var entity = new ArrayTaggedEntity
            {
                Id = 103,
                Tags = new[]
                {
                    new TagItemEntity { Label = "a" },
                    new TagItemEntity { Label = "b" },
                    new TagItemEntity { Label = "c" },
                },
            };

            var dto = mapper.Map<TaggedDto>(entity);

            Assert.Equal(103, dto.Id);
            Assert.NotNull(dto.Tags);
            Assert.Equal(3, dto.Tags.Count);
        }
    }
}
