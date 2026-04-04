using Hydrix.Caching;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Xunit;

namespace Hydrix.UnitTests.Caching
{
    /// <summary>
    /// Provides unit tests for the DataColumnMapCache class, ensuring correct mapping and behavior of entity
    /// properties.
    /// </summary>
    /// <remarks>This class contains tests that verify the mapping of properties from the EntityWithColumn
    /// class to the DataColumnMapCache, including checks for property inclusion, data types, and getter functionality.
    /// It also ensures that the same binder instance is returned for the same entity type.</remarks>
    public class DataColumnMapCacheTests
    {
        /// <summary>
        /// Represents an entity with various properties, including an identifier, a name, and additional fields that
        /// may or may not be mapped to a database.
        /// </summary>
        /// <remarks>The 'Id' property is mapped to a database column named 'CustomName'. The 'Ignored'
        /// property is not mapped to any database column. The 'NullableInt' property can hold a null value. The class
        /// also includes an indexer that returns a constant value and a write-only property.</remarks>
        private class EntityWithColumn
        {
            /// <summary>
            /// Gets or sets the identifier for the entity, which is mapped to a database column named 'CustomName' using the Column attribute.
            /// </summary>
            [Column("CustomName")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name of the entity.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets a value that is not mapped to any database column.
            /// </summary>
            [NotMapped]
            public string Ignored { get; set; }

            /// <summary>
            /// Gets or sets a nullable integer value.
            /// </summary>
            public int? NullableInt { get; set; }

            /// <summary>
            /// Gets the value of the indexer, which always returns 42.
            /// </summary>
            public int this[int i] => 42;

            /// <summary>
            /// Sets a value that is not readable.
            /// </summary>
            public int WriteOnly
            { set { } }
        }

        /// <summary>
        /// Verifies that the property mapping for the entity includes expected columns and excludes ignored or
        /// write-only properties.
        /// </summary>
        /// <remarks>This test ensures that the mapping produced by the DataColumnMapCache for
        /// EntityWithColumn contains the correct set of data columns. It checks that properties with custom names and
        /// standard properties are mapped, while properties marked as ignored, write-only, or indexers are not
        /// included. This helps maintain accurate data mapping and prevents unintended columns from being
        /// processed.</remarks>
        [Fact]
        public void GetOrCreate_MapsPropertiesCorrectly()
        {
            var binder = DataColumnMapCache<EntityWithColumn>.GetOrCreate();
            var columns = binder.Columns;
            Assert.Contains(columns, c => c.ColumnName == "CustomName");
            Assert.Contains(columns, c => c.ColumnName == "Name");
            Assert.Contains(columns, c => c.ColumnName == "NullableInt");
            Assert.DoesNotContain(columns, c => c.ColumnName == "Ignored");
            Assert.DoesNotContain(columns, c => c.ColumnName == "WriteOnly");
            Assert.DoesNotContain(columns, c => c.ColumnName == "Item"); // Indexer
        }

        /// <summary>
        /// Verifies that the data type of the 'NullableInt' column is correctly resolved to 'int'.
        /// </summary>
        /// <remarks>This test ensures that the data type mapping for columns in the DataColumnMapCache is
        /// functioning as expected, specifically for nullable integer types.</remarks>
        [Fact]
        public void GetOrCreate_ColumnDataType_IsCorrect()
        {
            var binder = DataColumnMapCache<EntityWithColumn>.GetOrCreate();
            var col = binder.Columns.FirstOrDefault(c => c.ColumnName == "NullableInt");
            Assert.Equal(typeof(int), col.DataType); // NullableInt should resolve to int
        }

        /// <summary>
        /// Verifies that the getter returned by DataColumnMapCache for EntityWithColumn correctly retrieves both value
        /// and reference type properties.
        /// </summary>
        /// <remarks>This test ensures that the cache's getter functions handle different property types,
        /// such as integers and strings, and return the expected values from the entity. It validates that the mapping
        /// works for both value types and reference types, confirming the reliability of property access through the
        /// cache.</remarks>
        [Fact]
        public void GetOrCreate_Getter_WorksForValueAndReferenceTypes()
        {
            var binder = DataColumnMapCache<EntityWithColumn>.GetOrCreate();
            var entity = new EntityWithColumn { Id = 7, Name = "abc", NullableInt = 42 };
            var idCol = binder.Columns.FirstOrDefault(c => c.ColumnName == "CustomName");
            var nameCol = binder.Columns.FirstOrDefault(c => c.ColumnName == "Name");
            var nullableCol = binder.Columns.FirstOrDefault(c => c.ColumnName == "NullableInt");
            Assert.Equal(7, idCol.Getter(entity));
            Assert.Equal("abc", nameCol.Getter(entity));
            Assert.Equal(42, nullableCol.Getter(entity));
        }

        /// <summary>
        /// Verifies that the getter for a nullable integer column returns null when the property is not set on the
        /// entity.
        /// </summary>
        /// <remarks>This test ensures that the column binder correctly handles entities with unset
        /// nullable properties, returning null as expected. It is useful for validating the behavior of data mapping
        /// components when dealing with nullable types.</remarks>
        [Fact]
        public void GetOrCreate_Getter_WorksForNullNullable()
        {
            var binder = DataColumnMapCache<EntityWithColumn>.GetOrCreate();
            var entity = new EntityWithColumn { NullableInt = null };
            var nullableCol = binder.Columns.FirstOrDefault(c => c.ColumnName == "NullableInt");
            Assert.Null(nullableCol.Getter(entity));
        }

        /// <summary>
        /// Verifies that the GetOrCreate method returns the same binder instance for repeated calls with the same type.
        /// </summary>
        /// <remarks>This test ensures that the caching mechanism in DataColumnMapCache provides
        /// consistent binder instances for identical type requests, which is important for maintaining state and
        /// performance across multiple usages.</remarks>
        [Fact]
        public void GetOrCreate_ReturnsSameBinder_ForSameType()
        {
            var binder1 = DataColumnMapCache<EntityWithColumn>.GetOrCreate();
            var binder2 = DataColumnMapCache<EntityWithColumn>.GetOrCreate();
            Assert.Same(binder1, binder2);
        }
    }
}
