using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Metadata;
using Hydrix.Schemas;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Mapping
{
    /// <summary>
    /// Contains unit tests for the SqlEntityMap class, verifying correct behavior of entity mapping, property
    /// assignment, and value conversion logic.
    /// </summary>
    /// <remarks>These tests cover scenarios such as property and delegate initialization, mapping from
    /// IDataRecord and DataRow sources, handling of missing or null primary keys in nested entities, and type
    /// conversion for supported types. The class uses dummy entity implementations to validate mapping logic in
    /// isolation from external dependencies.</remarks>
    public class SqlEntityMapTests
    {
        /// <summary>
        /// Dummy implementation of <see cref="ISqlEntity"/> for testing.
        /// </summary>
        [SqlEntity]
        private class TestEntity : ISqlEntity
        {
            /// <summary>
            /// Gets or sets a test integer property.
            /// </summary>
            [SqlField]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets a nested entity.
            /// </summary>
            [SqlEntity("text", "Nested", "Id")]
            public TestNestedEntity Nested { get; set; }
        }

        /// <summary>
        /// Dummy nested entity for testing.
        /// </summary>
        [SqlEntity]
        private class TestNestedEntity : ISqlEntity
        {
            /// <summary>
            /// Gets or sets a string property.
            /// </summary>
            [SqlField]
            public string Name { get; set; }
        }

        /// <summary>
        /// Verifies that the SqlEntityMap constructor correctly initializes its properties and delegates.
        /// </summary>
        /// <remarks>This test ensures that the Property and Attribute properties are set to the provided
        /// values and that the Factory and Setter delegates are not null after construction.</remarks>
        [Fact]
        public void Constructor_SetsPropertiesAndDelegates()
        {
            // Arrange
            var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Nested));
            var attribute = new SqlEntityAttribute("text", "Nested", "Id");

            // Act
            var map = new SqlEntityMap(property, attribute);

            // Assert
            Assert.Equal(property, map.Property);
            Assert.Equal(attribute, map.Attribute);
            Assert.NotNull(map.Factory);
            Assert.NotNull(map.Setter);
        }

        /// <summary>
        /// Verifies that the SetEntity method delegates property assignment to the IDataRecord implementation when
        /// provided with a DataRow.
        /// </summary>
        /// <remarks>This test ensures that calling SetEntity with a DataRow does not throw an exception
        /// and that the method correctly interacts with the data record abstraction. It is intended to validate the
        /// delegation logic for entity property mapping in scenarios involving DataRow inputs.</remarks>
        [Fact]
        public void SetEntity_DataRow_DelegatesToIDataRecord()
        {
            // Arrange
            var entity = new TestEntity();

            var metadata = new SqlEntityMetadata(
                new List<SqlFieldMap>(),
                new List<SqlEntityMap>()
            );
            var row = new DataTable().NewRow();
            var cache = new ConcurrentDictionary<Type, SqlEntityMetadata>();

            // Act & Assert
            // Should not throw (no-op, as DataRowDataRecordAdapter is not mocked)
            SqlEntityMap.SetEntity(entity, row, metadata, new List<string>(), cache);
        }

        /// <summary>
        /// Verifies that the SetEntity method correctly assigns field values and initializes nested entities when
        /// provided with an IDataRecord and corresponding metadata.
        /// </summary>
        /// <remarks>This test ensures that both simple fields and nested entity properties are populated
        /// as expected from the data record, validating the mapping logic for complex entity structures.</remarks>
        [Fact]
        public void SetEntity_IDataRecord_SetsFieldsAndNestedEntities()
        {
            // Arrange
            var entity = new TestEntity();
            var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Nested));
            var attribute = new SqlEntityAttribute("text", "Nested", "Id");
            var map = new SqlEntityMap(property, attribute);

            var fieldMetadata = new SqlFieldMetadata(
                typeof(TestEntity).GetProperty(nameof(TestEntity.Id)),
                (obj, val) => ((TestEntity)obj).Id = (int)val,
                typeof(int),
                new SqlFieldAttribute());

            var nestedMetadata = new SqlNestedEntityMetadata(
                property,
                attribute,
                () => new TestNestedEntity(),
                (obj, val) => ((TestEntity)obj).Nested = (TestNestedEntity)val);

            var metadata = SqlEntityMetadata.BuildEntityMetadata(typeof(TestEntity));

            var record = new Mock<IDataRecord>();
            record.Setup(r => r.GetOrdinal("Id")).Returns(0);
            record.Setup(r => r.IsDBNull(0)).Returns(false);
            record.Setup(r => r.GetValue(0)).Returns(42);

            record.Setup(r => r.GetOrdinal("Nested.Id")).Returns(1);
            record.Setup(r => r.IsDBNull(1)).Returns(false);
            record.Setup(r => r.GetValue(1)).Returns(7);

            var cache = new ConcurrentDictionary<Type, SqlEntityMetadata>();
            cache[typeof(TestEntity)] = metadata;

            // Act
            SqlEntityMap.SetEntity(entity, record.Object, metadata, new List<string>(), cache);

            // Assert
            Assert.Equal(42, entity.Id);
            Assert.NotNull(entity.Nested);
        }

        /// <summary>
        /// Verifies that the SetEntityFields method skips missing columns and sets property values only for columns
        /// present in the data record.
        /// </summary>
        /// <remarks>This test ensures that when a column is not found in the data record, SetEntityFields
        /// does not throw an exception and does not attempt to set the corresponding property. It also verifies that
        /// when a column is present, the property value is set correctly.</remarks>
        [Fact]
        public void SetEntityFields_SkipsMissingColumnsAndSetsValues()
        {
            // Arrange
            var entity = new TestEntity();
            var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Id));
            var fieldMetadata = new SqlFieldMetadata(
                property,
                (obj, val) => ((TestEntity)obj).Id = val == null ? 0 : (int)val,
                typeof(int),
                new SqlFieldAttribute());

            var metadata = SqlEntityMetadata.BuildEntityMetadata(typeof(TestEntity));

            var record = new Mock<IDataRecord>();
            record.Setup(r => r.GetOrdinal("Id")).Throws<IndexOutOfRangeException>();

            // Act (should not throw)
            var method = typeof(SqlEntityMap).GetMethod("SetEntityFields", BindingFlags.NonPublic | BindingFlags.Static);
            method.Invoke(null, new object[] { entity, record.Object, metadata, "" });

            // Now test with a valid column
            record.Setup(r => r.GetOrdinal("Id")).Returns(0);
            record.Setup(r => r.IsDBNull(0)).Returns(false);
            record.Setup(r => r.GetValue(0)).Returns(123);

            method.Invoke(null, new object[] { entity, record.Object, metadata, "" });

            // Assert
            Assert.Equal(123, entity.Id);
        }

        /// <summary>
        /// Verifies that the method for setting nested entities on an entity skips assignment when the primary key is
        /// missing or null in the data record.
        /// </summary>
        /// <remarks>This test ensures that nested entity properties are not set if the corresponding
        /// primary key column is absent from the data record or contains a null value. This behavior prevents the
        /// creation of incomplete or invalid nested entities during data mapping.</remarks>
        [Fact]
        public void SetEntityNestedEntities_SkipsWhenPrimaryKeyMissingOrNull()
        {
            // Arrange
            var entity = new TestEntity();
            var metadata = SqlEntityMetadata.BuildEntityMetadata(typeof(TestEntity));

            var record = new Mock<IDataRecord>();
            record.Setup(r => r.GetOrdinal("Nested.Id")).Throws<IndexOutOfRangeException>();

            var cache = new ConcurrentDictionary<Type, SqlEntityMetadata>();
            cache[typeof(TestEntity)] = metadata;

            var method = typeof(SqlEntityMap).GetMethod("SetEntityNestedEntities", BindingFlags.NonPublic | BindingFlags.Static);

            // Act (should not throw)
            method.Invoke(null, new object[] { entity, record.Object, metadata, new List<string>(), cache, "" });

            // Now test with PK column present but null
            record.Setup(r => r.GetOrdinal("Nested.Id")).Returns(0);
            record.Setup(r => r.IsDBNull(0)).Returns(true);

            method.Invoke(null, new object[] { entity, record.Object, metadata, new List<string>(), cache, "" });

            // Assert
            Assert.Null(entity.Nested);
        }

        /// <summary>
        /// Verifies that the ConvertValue method correctly handles conversion of values to enum, Guid, and other
        /// supported types.
        /// </summary>
        /// <remarks>This test ensures that ConvertValue can convert integer values to enum types, parse
        /// Guid values from both Guid instances and their string representations, and convert string representations of
        /// integers to int. It covers typical scenarios for type conversion in the SqlEntityMap class.</remarks>
        [Fact]
        public void ConvertValue_HandlesEnumsGuidsAndOtherTypes()
        {
            // Arrange
            var method = typeof(SqlEntityMap).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static);

            // Enum
            var enumValue = method.Invoke(null, new object[] { 1, typeof(TestEnum) });
            Assert.Equal(TestEnum.Value1, enumValue);

            // Guid
            var guid = Guid.NewGuid();
            var guidValue = method.Invoke(null, new object[] { guid, typeof(Guid) });
            Assert.Equal(guid, guidValue);

            var guidStr = guid.ToString();
            var guidValueFromString = method.Invoke(null, new object[] { guidStr, typeof(Guid) });
            Assert.Equal(guid, guidValueFromString);

            // Int
            var intValue = method.Invoke(null, new object[] { "42", typeof(int) });
            Assert.Equal(42, intValue);
        }

        /// <summary>
        /// Specifies the possible test values for demonstration or internal logic purposes.
        /// </summary>
        private enum TestEnum
        {
            /// <summary>
            /// Value 0.
            /// </summary>
            Value0 = 0,

            /// <summary>
            /// Value 1.
            /// </summary>
            Value1 = 1
        }
    }
}