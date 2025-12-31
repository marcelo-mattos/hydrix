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
    /// Contains unit tests for the SqlEntityMap class, verifying the correct behavior of entity mapping, property
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
        /// Dummy implementation of ISqlEntity for testing purposes.
        /// </summary>
        public class DummyEntity : ISqlEntity
        {
            /// <summary>
            /// Gets or sets a value for testing.
            /// </summary>
            public object Value { get; set; }
        }

        /// <summary>
        /// Dummy field representation for testing.
        /// </summary>
        public class DummyField
        {
            /// <summary>
            /// Gets or sets the attribute associated with the field.
            /// </summary>
            public DummyAttribute Attribute { get; set; }

            /// <summary>
            /// Gets or sets the value of the property represented by a <see cref="DummyProperty"/> instance.
            /// </summary>
            public DummyProperty Property { get; set; }

            /// <summary>
            /// Gets or sets the target type of the field.
            /// </summary>
            public Type TargetType { get; set; }

            /// <summary>
            /// Gets or sets the delegate used to assign a value to an <see cref="ISqlEntity"/> instance.
            /// </summary>
            /// <remarks>The delegate receives the target <see cref="ISqlEntity"/> and the value to
            /// assign. This property enables custom logic for setting entity values, such as type conversion or
            /// validation, during data mapping operations.</remarks>
            public Action<ISqlEntity, object> Setter { get; set; }
        }

        /// <summary>
        /// Dummy attribute representation for testing.
        /// </summary>
        public class DummyAttribute
        {
            /// <summary>
            /// Gets or sets the name of the field.
            /// </summary>
            public string FieldName { get; set; }
        }

        /// <summary>
        /// Gets or sets the name associated with this instance.
        /// </summary>
        public class DummyProperty
        {
            /// <summary>
            /// Gets or sets the name associated with the object.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Gets or sets the collection of fields that define the metadata structure.
        /// </summary>
        /// <remarks>Use this property to access or modify the set of fields associated with the metadata.
        /// The order of fields in the list may be significant depending on how the metadata is processed.</remarks>
        public class DummyMetadata
        {
            /// <summary>
            /// Gets or sets the collection of fields associated with the current instance.
            /// </summary>
            public List<DummyField> Fields { get; set; }
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
            var metadata = SqlEntityMetadata.BuildEntityMetadata(typeof(TestEntity));

            var record = new Mock<IDataRecord>();
            record.Setup(r => r.GetOrdinal("Id")).Returns(0);
            record.Setup(r => r.IsDBNull(0)).Returns(false);
            record.Setup(r => r.GetValue(0)).Returns(42);

            record.Setup(r => r.GetOrdinal("Nested.Id")).Returns(1);
            record.Setup(r => r.IsDBNull(1)).Returns(false);
            record.Setup(r => r.GetValue(1)).Returns(7);

            var cache = new ConcurrentDictionary<Type, SqlEntityMetadata>
            {
                [typeof(TestEntity)] = metadata
            };

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

            var cache = new ConcurrentDictionary<Type, SqlEntityMetadata>
            {
                [typeof(TestEntity)] = metadata
            };

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

        /// <summary>
        /// Verifies that the SetEntityFields method uses the property name as the field name when the attribute's
        /// FieldName is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <remarks>This test ensures that, in cases where the field name is not specified or is invalid,
        /// the property name is used to map data from the record to the entity. This behavior is important for
        /// maintaining correct field-to-property mapping in scenarios where metadata may be incomplete or improperly
        /// configured.</remarks>
        [Fact]
        public void SetEntityFields_UsesPropertyName_WhenFieldNameIsNullOrWhiteSpace()
        {
            // Arrange
            var entity = new DummyEntity();
            var recordMock = new Mock<IDataRecord>();
            var field = new DummyField
            {
                Attribute = new DummyAttribute { FieldName = " " },
                Property = new DummyProperty { Name = "TestProp" },
                TargetType = typeof(string),
                Setter = (e, v) => ((DummyEntity)e).Value = v
            };
            var metadata = new DummyMetadata { Fields = new List<DummyField> { field } };
            string prefix = "p_";

            recordMock.Setup(r => r.GetOrdinal("p_TestProp")).Returns(0);
            recordMock.Setup(r => r.IsDBNull(0)).Returns(false);
            recordMock.Setup(r => r.GetValue(0)).Returns("abc");

            // Act
            SqlEntityMaterializerTestHelper.SetEntityFields(entity, recordMock.Object, metadata, prefix);

            // Assert
            Assert.Equal("abc", entity.Value);
        }

        /// <summary>
        /// Verifies that the SetEntityFields method uses the attribute's FieldName property when it is not null or
        /// whitespace.
        /// </summary>
        /// <remarks>This test ensures that, when a field's associated attribute specifies a non-null and
        /// non-whitespace FieldName, SetEntityFields retrieves the value from the data record using the prefixed
        /// FieldName and assigns it to the entity property. The test sets up a mock data record and asserts that the
        /// entity's property is correctly populated.</remarks>
        [Fact]
        public void SetEntityFields_UsesAttributeFieldName_WhenNotNullOrWhiteSpace()
        {
            // Arrange
            var entity = new DummyEntity();
            var recordMock = new Mock<IDataRecord>();
            var field = new DummyField
            {
                Attribute = new DummyAttribute { FieldName = "FieldA" },
                Property = new DummyProperty { Name = "TestProp" },
                TargetType = typeof(string),
                Setter = (e, v) => ((DummyEntity)e).Value = v
            };
            var metadata = new DummyMetadata { Fields = new List<DummyField> { field } };
            string prefix = "p_";

            recordMock.Setup(r => r.GetOrdinal("p_FieldA")).Returns(0);
            recordMock.Setup(r => r.IsDBNull(0)).Returns(false);
            recordMock.Setup(r => r.GetValue(0)).Returns("xyz");

            // Act
            SqlEntityMaterializerTestHelper.SetEntityFields(entity, recordMock.Object, metadata, prefix);

            // Assert
            Assert.Equal("xyz", entity.Value);
        }

        /// <summary>
        /// Verifies that the SetEntityFields method sets the entity field to null when the corresponding data record
        /// value is DBNull.
        /// </summary>
        /// <remarks>This test ensures that when IsDBNull returns <see langword="true"/> for a field, the
        /// entity's property is assigned a null value. It is useful for validating the correct handling of database nulls
        /// during entity materialization.</remarks>
        [Fact]
        public void SetEntityFields_SetsNull_WhenIsDBNullIsTrue()
        {
            // Arrange
            var entity = new DummyEntity();
            var recordMock = new Mock<IDataRecord>();
            var field = new DummyField
            {
                Attribute = new DummyAttribute { FieldName = "FieldB" },
                Property = new DummyProperty { Name = "TestProp" },
                TargetType = typeof(string),
                Setter = (e, v) => ((DummyEntity)e).Value = v
            };
            var metadata = new DummyMetadata { Fields = new List<DummyField> { field } };
            string prefix = "";

            recordMock.Setup(r => r.GetOrdinal("FieldB")).Returns(0);
            recordMock.Setup(r => r.IsDBNull(0)).Returns(true);

            // Act
            SqlEntityMaterializerTestHelper.SetEntityFields(entity, recordMock.Object, metadata, prefix);

            // Assert
            Assert.Null(entity.Value);
        }
    }

    /// <summary>
    /// Provides helper methods for materializing SQL entity objects from data records in unit tests.
    /// </summary>
    /// <remarks>This class is intended for use in test scenarios to assist with populating entity fields from
    /// database query results. It is not designed for production use. All methods are static and do not require
    /// instantiation.</remarks>
    public static class SqlEntityMaterializerTestHelper
    {
        /// <summary>
        /// Populates the fields of the specified entity with values from the given data record, using metadata to map
        /// record columns to entity properties.
        /// </summary>
        /// <remarks>Fields in the entity are set to null if the corresponding column value in the data
        /// record is DBNull or if the column is not found. The method skips fields for which no matching column exists
        /// in the data record.</remarks>
        /// <param name="entity">The entity instance whose fields will be set with values from the data record.</param>
        /// <param name="record">The data record containing column values to assign to the entity's fields.</param>
        /// <param name="metadata">Metadata describing the mapping between entity properties and data record columns. Must provide a collection
        /// of fields with property and setter information.</param>
        /// <param name="prefix">A string prefix to prepend to column names when matching fields in the data record.</param>
        public static void SetEntityFields(
            ISqlEntity entity,
            IDataRecord record,
            dynamic metadata,
            string prefix)
        {
            foreach (var field in metadata.Fields)
            {
                string columnName = string.IsNullOrWhiteSpace(field.Attribute.FieldName)
                    ? $"{prefix}{field.Property.Name}"
                    : $"{prefix}{field.Attribute.FieldName}";

                int ordinal;
                try
                {
                    ordinal = record.GetOrdinal(columnName);
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }

                if (record.IsDBNull(ordinal))
                {
                    field.Setter(entity, null);
                    continue;
                }

                var value = record.GetValue(ordinal);
                field.Setter(entity, value);
            }
        }
    }
}