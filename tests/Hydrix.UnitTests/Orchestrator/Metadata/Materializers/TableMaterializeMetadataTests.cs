using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Metadata.Materializers;
using Hydrix.Orchestrator.Resolvers;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Metadata.Materializers
{
    /// <summary>
    /// Unit tests for <see cref="TableMaterializeMetadata"/>.
    /// </summary>
    public class TableMaterializeMetadataTests
    {
        /// <summary>
        /// Test entity with no mapping attributes.
        /// </summary>
        private class NoAttributesEntity : ITable
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with the object.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Test child entity for nested mapping.
        /// </summary>
        [Table("Child", Schema = "tests")]
        private class TestChildEntity : ITable
        {
            /// <summary>
            /// Gets or sets the identifier of the child entity associated with this record.
            /// </summary>
            [Column("ChildId")]
            public int ChildId { get; set; }

            /// <summary>
            /// Gets or sets the name of the child associated with this entity.
            /// </summary>
            [Column("ChildName")]
            public string ChildName { get; set; }
        }

        /// <summary>
        /// Test entity with scalar and nested mappings.
        /// </summary>
        [Table("Test", Schema = "tests")]
        private class TestEntity : ITable
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [Column("Id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with the entity.
            /// </summary>
            [Column("Name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the nullable integer value associated with this instance.
            /// </summary>
            [Column("NullableValue")]
            public int? NullableValue { get; set; }

            /// <summary>
            /// Gets or sets the child entity associated with this instance.
            /// </summary>
            [ForeignTable("Child", Schema = "tests", PrimaryKeys = new[] { "ChildId" })]
            public TestChildEntity Child { get; set; }

            /// <summary>
            /// Gets or sets the value that is not mapped to any database column.
            /// </summary>
            public string NotMapped { get; set; }
        }

        /// <summary>
        /// Represents a product in the inventory system, including its identification, name, price, and associated
        /// category.
        /// </summary>
        /// <remarks>The product's price is nullable, indicating that it may not be set. Each product is
        /// linked to a category, which provides context for its classification.</remarks>
        [Table("products")]
        private class Product
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [Column("id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with the entity.
            /// </summary>
            [Column("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the price of the item. This property may be null if the price is not specified.
            /// </summary>
            /// <remarks>The price is represented as a nullable decimal value, allowing for scenarios
            /// where the price may be unknown or not applicable.</remarks>
            [Column("price")]
            public decimal? Price { get; set; }

            /// <summary>
            /// Gets or sets the category associated with the current entity.
            /// </summary>
            /// <remarks>The category provides a way to classify the entity, allowing for better
            /// organization and retrieval of related items.</remarks>
            [ForeignTable("categories", Alias = "c", Schema = "dbo", PrimaryKeys = new[] { "Id" }, ForeignKeys = new[] { "CategoryId" })]
            public Category Category { get; set; }
        }

        /// <summary>
        /// Represents a category entity with an identifier and a name.
        /// </summary>
        /// <remarks>This class is typically used to organize or classify items within the application.
        /// Each instance corresponds to a record in the 'categories' database table.</remarks>
        [Table("categories")]
        private class Category
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [Column("id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with the entity.
            /// </summary>
            [Column("name")]
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents an entity with an immutable identifier and a reference to a related child entity.
        /// </summary>
        /// <remarks>All properties except 'NotMapped' are read-only and can only be set during object
        /// construction. The 'Id' property is mapped to a database column, while the 'Child' property represents a
        /// foreign relationship to another entity. The 'NotMapped' property is not persisted in the database.</remarks>
        private class NoSetterEntity
        {
            /// <summary>
            /// Gets the unique identifier for the entity.
            /// </summary>
            [Column("id")]
            public int Id { get; }

            /// <summary>
            /// Gets the child object associated with this instance.
            /// </summary>
            /// <remarks>The child object is retrieved from the foreign table 'child'. This property
            /// is read-only and cannot be set directly.</remarks>
            [ForeignTable("child")]
            public object Child { get; }

            /// <summary>
            /// Gets or sets the value indicating that this property is not mapped to a database column.
            /// </summary>
            /// <remarks>Use this property to mark data members that should be excluded from database
            /// persistence. This is useful for properties that are used only within the application and do not require
            /// storage in the database schema.</remarks>
            public string NotMapped { get; set; }
        }

        /// <summary>
        /// Represents a placeholder class with no fields or entities defined.
        /// </summary>
        private class NoFieldsOrEntities
        { }

        /// <summary>
        /// Verifies that the TableMaterializeMetadata constructor correctly assigns the provided fields and entities to
        /// the corresponding properties.
        /// </summary>
        /// <remarks>This test ensures that the constructor does not create copies of the input lists, but
        /// instead assigns the references directly. This behavior is important for scenarios where reference equality
        /// is required or when the caller expects changes to the original lists to be reflected in the metadata
        /// instance.</remarks>
        [Fact]
        public void Constructor_SetsFieldsAndEntities()
        {
            // Arrange
            var fields = new List<ColumnMap>();
            var entities = new List<TableMap>();

            // Act
            var metadata = new TableMaterializeMetadata(fields, entities);

            // Assert
            Assert.Same(fields, metadata.Fields);
            Assert.Same(entities, metadata.Entities);
        }

        /// <summary>
        /// Verifies that GetOrAddBindings caches the first binding for a schema hash and reuses it on subsequent
        /// requests.
        /// </summary>
        [Fact]
        public void GetOrAddBindings_ReusesCachedBinding_ForSameSchemaHash()
        {
            var metadata = new TableMaterializeMetadata(
                new List<ColumnMap>(),
                new List<TableMap>());

            var invocationCount = 0;

            var first = metadata.GetOrAddBindings(
                1,
                _ =>
                {
                    invocationCount++;
                    return new ResolvedTableBindings(null, null);
                });

            var second = metadata.GetOrAddBindings(
                1,
                _ =>
                {
                    invocationCount++;
                    return new ResolvedTableBindings(null, null);
                });

            Assert.Same(first, second);
            Assert.Equal(1, invocationCount);
        }

        /// <summary>
        /// Verifies that GetOrAddBindings stops caching new schema hashes once the configured cache cap is reached.
        /// </summary>
        [Fact]
        public void GetOrAddBindings_DoesNotCacheNewSchemaHash_WhenCacheCapIsReached()
        {
            var metadataType = typeof(TableMaterializeMetadata);
            var flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

            var maxCacheSizeField = metadataType.GetField("MaxBindingsCacheSize", flags);
            var bindingsCacheSizeField = metadataType.GetField("_bindingsCacheSize", flags);

            Assert.NotNull(maxCacheSizeField);
            Assert.NotNull(bindingsCacheSizeField);

            var maxCacheSize = (int)maxCacheSizeField.GetRawConstantValue();

            var metadata = new TableMaterializeMetadata(
                new List<ColumnMap>(),
                new List<TableMap>());

            for (var schemaHash = 0; schemaHash < maxCacheSize; schemaHash++)
            {
                metadata.GetOrAddBindings(
                    schemaHash,
                    _ => new ResolvedTableBindings(null, null));
            }

            var cacheSizeBefore = (int)bindingsCacheSizeField.GetValue(metadata);
            Assert.Equal(maxCacheSize, cacheSizeBefore);

            var missesFactoryInvocations = 0;

            var first = metadata.GetOrAddBindings(
                maxCacheSize + 1,
                _ =>
                {
                    missesFactoryInvocations++;
                    return new ResolvedTableBindings(null, null);
                });

            var second = metadata.GetOrAddBindings(
                maxCacheSize + 1,
                _ =>
                {
                    missesFactoryInvocations++;
                    return new ResolvedTableBindings(null, null);
                });

            Assert.NotSame(first, second);
            Assert.Equal(2, missesFactoryInvocations);
            Assert.False(metadata.TryGetBindings(maxCacheSize + 1, out _));
            Assert.Equal(maxCacheSize, (int)bindingsCacheSizeField.GetValue(metadata));
        }

        /// <summary>
        /// Verifies that GetOrAddBindings rolls back reserved cache size and returns the cached bindings when cache
        /// insertion loses a race for the same schema hash.
        /// </summary>
        [Fact]
        public void GetOrAddBindings_DecrementsCacheSizeAndReturnsCachedBindings_WhenTryAddLosesRace()
        {
            var metadata = new TableMaterializeMetadata(
                new List<ColumnMap>(),
                new List<TableMap>());

            var metadataType = typeof(TableMaterializeMetadata);
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var bindingsBySchemaHashField = metadataType.GetField("_bindingsBySchemaHash", flags);
            var bindingsCacheSizeField = metadataType.GetField("_bindingsCacheSize", flags);

            Assert.NotNull(bindingsBySchemaHashField);
            Assert.NotNull(bindingsCacheSizeField);

            var bindingsBySchemaHash =
                (ConcurrentDictionary<int, ResolvedTableBindings>)bindingsBySchemaHashField.GetValue(metadata);

            var schemaHash = 42;
            var cachedBindings = new ResolvedTableBindings(null, null);
            var currentBindings = new ResolvedTableBindings(null, null);

            var result = metadata.GetOrAddBindings(
                schemaHash,
                _ =>
                {
                    bindingsBySchemaHash.TryAdd(schemaHash, cachedBindings);
                    return currentBindings;
                });

            Assert.Same(cachedBindings, result);
            Assert.Equal(0, (int)bindingsCacheSizeField.GetValue(metadata));
            Assert.True(metadata.TryGetBindings(schemaHash, out var fromCache));
            Assert.Same(cachedBindings, fromCache);
        }

        /// <summary>
        /// Verifies that cache slot reservation core retries after a failed update attempt and succeeds on a later
        /// attempt.
        /// </summary>
        [Fact]
        public void TryReserveBindingsCacheSlotCore_RetriesAfterFailedUpdate_AndThenSucceeds()
        {
            var metadataType = typeof(TableMaterializeMetadata);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;

            var method = metadataType.GetMethod("TryReserveBindingsCacheSlotCore", flags);

            Assert.NotNull(method);

            var reads = new Queue<int>(new[] { 0, 0 });
            Func<int> readCacheSize = () => reads.Dequeue();

            var updateCalls = 0;
            Func<int, int, bool> tryUpdate = (current, updated) =>
            {
                updateCalls++;
                return updateCalls == 2;
            };

            var result = (bool)method.Invoke(
                null,
                new object[] { readCacheSize, tryUpdate });

            Assert.True(result);
            Assert.Equal(2, updateCalls);
        }

        /// <summary>
        /// Verifies that cache slot reservation core returns false when the cache size has reached the configured
        /// maximum.
        /// </summary>
        [Fact]
        public void TryReserveBindingsCacheSlotCore_ReturnsFalse_WhenCacheIsFull()
        {
            var metadataType = typeof(TableMaterializeMetadata);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;

            var method = metadataType.GetMethod("TryReserveBindingsCacheSlotCore", flags);
            var maxCacheSizeField = metadataType.GetField("MaxBindingsCacheSize", flags);

            Assert.NotNull(method);
            Assert.NotNull(maxCacheSizeField);

            var maxCacheSize = (int)maxCacheSizeField.GetRawConstantValue();

            Func<int> readCacheSize = () => maxCacheSize;

            var updateCalls = 0;
            Func<int, int, bool> tryUpdate = (current, updated) =>
            {
                updateCalls++;
                return true;
            };

            var result = (bool)method.Invoke(
                null,
                new object[] { readCacheSize, tryUpdate });

            Assert.False(result);
            Assert.Equal(0, updateCalls);
        }
    }
}
