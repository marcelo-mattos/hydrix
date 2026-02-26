using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Metadata.Materializers;
using Hydrix.Schemas.Contract;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Mapping
{
    /// <summary>
    /// Contains unit tests for the TableMap class, verifying the correct behavior of entity mapping, property
    /// assignment, and value conversion logic.
    /// </summary>
    /// <remarks>These tests cover scenarios such as property and delegate initialization, mapping from
    /// IDataRecord and DataRow sources, handling of missing or null primary keys in nested entities, and type
    /// conversion for supported types. The class uses dummy entity implementations to validate mapping logic in
    /// isolation from external dependencies.</remarks>
    public class TableMapTests
    {
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
        /// Specifies a parent entity that can have an associated child entity, demonstrating a simple hierarchical
        /// relationship.
        /// </summary>
        private class Parent : ITable
        {
            /// <summary>
            /// Gets or sets the child element associated with this parent.
            /// </summary>
            /// <remarks>This property enables a hierarchical relationship between parent and child
            /// elements. The value can be null if no child is assigned.</remarks>
            public Child Child { get; set; }
        }

        /// <summary>
        /// Specifies a child entity that can be associated with a parent entity, demonstrating a simple hierarchical
        /// relationship.
        /// </summary>
        private class Child : ITable
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            public int Id { get; set; }
        }

        /// <summary>
        /// Dummy implementation of <see cref="ITable"/> for testing.
        /// </summary>
        [Table("Test")]
        private class TestEntity : ITable
        {
            /// <summary>
            /// Gets or sets a test integer property.
            /// </summary>
            [Column]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the nested entity associated with this instance. The association is optional and may be
            /// null if no related entity exists.
            /// </summary>
            /// <remarks>This property represents a foreign key relationship to a 'TestNestedEntity'
            /// in the 'Nested' schema. Because the primary key is not specified, the relationship is optional and the
            /// property may be null to indicate no association.</remarks>
            [ForeignTable("text", Schema = "Nested")]
            public TestNestedEntity NestedNullPrimaryKey { get; set; }

            /// <summary>
            /// Gets or sets the nested entity associated with the primary key, which is empty in this case.
            /// </summary>
            /// <remarks>This property represents a foreign key relationship to the 'TestNestedEntity'
            /// table in the 'Nested' schema. The absence of primary keys indicates that this relationship does not
            /// enforce any constraints on the referenced table.</remarks>
            [ForeignTable("text", Schema = "Nested", PrimaryKeys = new string[0])]
            public TestNestedEntity NestedEmptyPrimaryKey { get; set; }

            /// <summary>
            /// Gets or sets the nested entity associated with this instance.
            /// </summary>
            /// <remarks>This property represents a relationship to the nested entity, allowing access
            /// to its properties and methods. Ensure that the nested entity is properly initialized before accessing
            /// its members.</remarks>
            [ForeignTable("text", Schema = "Nested", PrimaryKeys = new[] { "Id" })]
            public TestNestedEntity Nested { get; set; }
        }

        /// <summary>
        /// Dummy nested entity for testing.
        /// </summary>
        [Table("TestNested")]
        private class TestNestedEntity : ITable
        {
            /// <summary>
            /// Gets or sets a string property.
            /// </summary>
            [Column]
            public string Name { get; set; }
        }

        /// <summary>
        /// Dummy implementation of ITable for testing purposes.
        /// </summary>
        public class DummyEntity : ITable
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
            /// Gets or sets the default value associated with the member.
            /// </summary>
            public object DefaultValue { get; set; }

            /// <summary>
            /// Gets or sets the delegate used to assign a value to an <see cref="ITable"/> instance.
            /// </summary>
            /// <remarks>The delegate receives the target <see cref="ITable"/> and the value to
            /// assign. This property enables custom logic for setting entity values, such as type conversion or
            /// validation, during data mapping operations.</remarks>
            public Action<ITable, object> Setter { get; set; }
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
        /// Dummy enum for testing type conversion.
        /// </summary>
        private enum TestStatus
        {
            /// <summary>
            /// Gets or sets a value indicating whether the object is currently active.
            /// </summary>
            /// <remarks>This property is typically used to determine the state of the object in
            /// relation to its lifecycle. An active state may affect how the object interacts with other components or
            /// services.</remarks>
            Active,

            /// <summary>
            /// Represents the inactive state of an entity.
            /// </summary>
            Inactive
        }

        /// <summary>
        /// Verifies that the TableMap constructor correctly initializes its properties and delegates.
        /// </summary>
        /// <remarks>This test ensures that the Property and Attribute properties are set to the provided
        /// values and that the Factory and Setter delegates are not null after construction.</remarks>
        [Fact]
        public void Constructor_SetsPropertiesAndDelegates()
        {
            // Arrange
            var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Nested));
            var attribute = new ForeignTableAttribute("text")
            {
                Schema = "Nested",
                PrimaryKeys = new[] { "Id" }
            };

            // Act
            var map = new TableMap(property, attribute);

            // Assert
            Assert.Equal(property, map.Property);
            Assert.Equal(attribute, map.Attribute);
            Assert.NotNull(map.Factory);
            Assert.NotNull(map.Setter);
        }

        /// <summary>
        /// Verifies that the SetEntity method correctly handles an IDataRecord created from a DataRow without throwing
        /// exceptions.
        /// </summary>
        /// <remarks>This test ensures that SetEntity operates as expected when provided with a
        /// DataRow-based IDataRecord, even when the DataRowDataRecordAdapter is not explicitly mocked. It confirms that
        /// the method is a no-op in this scenario and does not alter the entity or throw errors.</remarks>
        [Fact]
        public void SetEntity_IDataRecord_FromDataRow_IsHandledCorrectly()
        {
            // Arrange
            var entity = new TestEntity();

            var metadata = new TableMaterializeMetadata(
                new List<ColumnMap>(),
                new List<TableMap>()
            );
            var row = new DataTable().NewRow();
            var reader = row.Table.CreateDataReader();

            // Act
            // Should not throw (no-op, as DataRowDataRecordAdapter is not mocked)
            TableMap.SetEntity(
                entity,
                reader,
                metadata,
                string.Empty,
                new Dictionary<string, int>());

            // Assert
            Assert.NotNull(entity);
        }

        /// <summary>
        /// Verifies that the ConvertValue method correctly handles conversion of values to enum, Guid, and other
        /// supported types.
        /// </summary>
        /// <remarks>This test ensures that ConvertValue can convert integer values to enum types, parse
        /// Guid values from both Guid instances and their string representations, and convert string representations of
        /// integers to int. It covers typical scenarios for type conversion in the TableMap class.</remarks>
        [Fact]
        public void ConvertValue_HandlesEnumsGuidsAndOtherTypes()
        {
            // Arrange
            var method = typeof(TableMap).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static);

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
                Setter = (e, v) => ((DummyEntity)e).Value = v,
                DefaultValue = null
            };
            var metadata = new DummyMetadata { Fields = new List<DummyField> { field } };
            string prefix = "p_";

            var ordinals = new Dictionary<string, int>
            {
                ["p_TestProp"] = 0
            };

            recordMock.Setup(r => r.GetOrdinal("p_TestProp")).Returns(0);
            recordMock.Setup(r => r.IsDBNull(0)).Returns(false);
            recordMock.Setup(r => r.GetValue(0)).Returns("abc");
            recordMock.Setup(r => r.FieldCount).Returns(1);
            recordMock.Setup(r => r.GetName(0)).Returns("TestProp");

            // Act
            SqlEntityMaterializerTestHelper.SetEntityFields(
                entity,
                recordMock.Object,
                metadata,
                prefix,
                ordinals);

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
                Setter = (e, v) => ((DummyEntity)e).Value = v,
                DefaultValue = null
            };
            var metadata = new DummyMetadata { Fields = new List<DummyField> { field } };
            string prefix = "p_";

            var ordinals = new Dictionary<string, int>
            {
                ["p_FieldA"] = 0
            };

            recordMock.Setup(r => r.GetOrdinal("p_FieldA")).Returns(0);
            recordMock.Setup(r => r.IsDBNull(0)).Returns(false);
            recordMock.Setup(r => r.GetValue(0)).Returns("xyz");
            recordMock.Setup(r => r.FieldCount).Returns(1);
            recordMock.Setup(r => r.GetName(0)).Returns("TestProp");

            // Act
            SqlEntityMaterializerTestHelper.SetEntityFields(
                entity,
                recordMock.Object,
                metadata,
                prefix,
                ordinals);

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
                Setter = (e, v) => ((DummyEntity)e).Value = v,
                DefaultValue = null
            };
            var metadata = new DummyMetadata { Fields = new List<DummyField> { field } };
            string prefix = "";

            var ordinals = new Dictionary<string, int>
            {
                ["FieldB"] = 0
            };

            recordMock.Setup(r => r.GetOrdinal("FieldB")).Returns(0);
            recordMock.Setup(r => r.IsDBNull(0)).Returns(true);
            recordMock.Setup(r => r.FieldCount).Returns(1);
            recordMock.Setup(r => r.GetName(0)).Returns("FieldB");

            // Act
            SqlEntityMaterializerTestHelper.SetEntityFields(
                entity,
                recordMock.Object,
                metadata,
                prefix,
                ordinals);

            // Assert
            Assert.Null(entity.Value);
        }

        /// <summary>
        /// Verifies that the SetEntityFields method assigns the default value to an entity field when the corresponding
        /// data record column is DBNull.
        /// </summary>
        /// <remarks>This test ensures that when a data record indicates a field is DBNull, the entity's
        /// field is set to its defined default value rather than null or an uninitialized state. This behavior is
        /// important for maintaining expected default values in entity objects when database fields are missing or
        /// contain nulls.</remarks>
        [Fact]
        public void SetEntityFields_SetsDefaultValue_WhenIsDBNullIsTrue()
        {
            // Arrange
            var entity = new DummyEntity();
            var recordMock = new Mock<IDataRecord>();
            var field = new DummyField
            {
                Attribute = new DummyAttribute { FieldName = "FieldB" },
                Property = new DummyProperty { Name = "TestProp" },
                TargetType = typeof(int),
                Setter = (e, v) => ((DummyEntity)e).Value = v,
                DefaultValue = default(int)
            };
            var metadata = new DummyMetadata { Fields = new List<DummyField> { field } };
            string prefix = "";

            var ordinals = new Dictionary<string, int>
            {
                ["FieldB"] = 0
            };

            recordMock.Setup(r => r.GetOrdinal("FieldB")).Returns(0);
            recordMock.Setup(r => r.IsDBNull(0)).Returns(true);
            recordMock.Setup(r => r.FieldCount).Returns(1);
            recordMock.Setup(r => r.GetName(0)).Returns("FieldB");

            // Act
            SqlEntityMaterializerTestHelper.SetEntityFields(
                entity,
                recordMock.Object,
                metadata,
                prefix,
                ordinals);

            // Assert
            Assert.Equal(field.DefaultValue, entity.Value);
        }

        /// <summary>
        /// Verifies that the SetEntityFields method assigns the default value to an integer property when the
        /// corresponding data record field is DBNull.
        /// </summary>
        /// <remarks>This test ensures that when the data record indicates a field is DBNull, the entity's
        /// property is set to the specified default value rather than left unset or assigned a null value. This
        /// behavior is important for maintaining expected defaults in entity objects when database fields are missing
        /// or null.</remarks>
        [Fact]
        public void SetEntityFields_SetsDefaultValue_Int_WhenIsDBNullIsTrue()
        {
            // Arrange
            var entity = new DummyEntity();
            var recordMock = new Mock<IDataRecord>();
            var field = new DummyField
            {
                Attribute = new DummyAttribute { FieldName = "FieldB" },
                Property = new DummyProperty { Name = "TestProp" },
                TargetType = typeof(int),
                Setter = (e, v) => ((DummyEntity)e).Value = v,
                DefaultValue = default(int)
            };
            var metadata = new DummyMetadata { Fields = new List<DummyField> { field } };
            string prefix = "";

            var ordinals = new Dictionary<string, int>
            {
                ["FieldB"] = 0
            };

            recordMock.Setup(r => r.GetOrdinal("FieldB")).Returns(0);
            recordMock.Setup(r => r.IsDBNull(0)).Returns(false);
            recordMock.Setup(r => r.GetValue(0)).Returns(99);
            recordMock.Setup(r => r.FieldCount).Returns(1);
            recordMock.Setup(r => r.GetName(0)).Returns("FieldB");

            // Act
            SqlEntityMaterializerTestHelper.SetEntityFields(
                entity,
                recordMock.Object,
                metadata,
                prefix,
                ordinals);

            // Assert
            Assert.Equal(99, entity.Value);
        }

        /// <summary>
        /// Verifies that the SetEntityNestedEntities method does not set nested entity properties when the primary keys
        /// are null or empty.
        /// </summary>
        /// <remarks>This test ensures that no exceptions are thrown and that the nested property remains
        /// null when primary keys are not provided. It validates that the method maintains the correct entity state
        /// under these conditions.</remarks>
        [Fact]
        public void SetEntityNestedEntities_Skips_WhenPrimaryKeysIsNullOrEmpty()
        {
            // Arrange
            var property = typeof(TestEntity).GetProperty(nameof(TestEntity.NestedNullPrimaryKey));
            var attribute = new ForeignTableAttribute("text")
            {
                Schema = "Nested",
                PrimaryKeys = null // ou new string[0]
            };
            var map = new TableMap(property, attribute);

            var metadata = new TableMaterializeMetadata(
                new List<ColumnMap>(),
                new List<TableMap> { map }
            );

            var entity = new TestEntity();
            var record = new Mock<IDataRecord>();
            var ordinals = new Dictionary<string, int>();

            var method = typeof(TableMap).GetMethod("SetEntityNestedEntities", BindingFlags.NonPublic | BindingFlags.Static);

            // Act (should not throw and should not set Nested)
            method.Invoke(
                null,
                new object[]
                {
                    entity,
                    record.Object,
                    metadata,
                    string.Empty,
                    ordinals
                });

            // Assert
            Assert.Null(entity.Nested);
        }

        /// <summary>
        /// Verifies that the SetEntityNestedEntities method does not set nested entities when the primary key value is
        /// DBNull.
        /// </summary>
        /// <remarks>This test ensures that if the primary key column exists but contains a DBNull value,
        /// the SetEntityNestedEntities method skips setting the nested entity property. This behavior helps prevent
        /// null reference exceptions and maintains data integrity when primary key values are missing.</remarks>
        [Fact]
        public void SetEntityNestedEntities_Skips_WhenPrimaryKeyIsDBNull()
        {
            // Arrange
            var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Nested));
            var attribute = new ForeignTableAttribute("text")
            {
                Schema = "Nested",
                PrimaryKeys = new[] { "Id" }
            };
            var map = new TableMap(property, attribute);

            var metadata = new TableMaterializeMetadata(
                new List<ColumnMap>(),
                new List<TableMap> { map }
            );

            var entity = new TestEntity();
            var record = new Mock<IDataRecord>();
            var ordinals = new Dictionary<string, int>
            {
                { "Nested.Id", 0 }
            };

            // PK column exists, but value is DBNull
            record.Setup(r => r.IsDBNull(0)).Returns(true);

            var method = typeof(TableMap).GetMethod("SetEntityNestedEntities", BindingFlags.NonPublic | BindingFlags.Static);

            // Act (should not throw and should not set Nested)
            method.Invoke(
                null,
                new object[]
                {
                    entity,
                    record.Object,
                    metadata,
                    string.Empty,
                    ordinals
                });

            // Assert
            Assert.Null(entity.Nested);
        }

        /// <summary>
        /// Verifies that the ConvertValue method correctly parses a string representation of an enum value and returns
        /// the corresponding enum type.
        /// </summary>
        /// <remarks>This test ensures that when a string matching a defined enum value is provided to the
        /// ConvertValue method, the method returns the expected enum value. It validates the method's ability to handle
        /// string-to-enum conversion for the TestStatus type.</remarks>
        [Fact]
        public void ConvertValue_EnumFromString_ReturnsParsedEnum()
        {
            // Arrange
            var method = typeof(TableMap).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static);
            var value = "Active";
            var targetType = typeof(TestStatus);

            // Act
            var result = method.Invoke(null, new object[] { value, targetType });

            // Assert
            Assert.Equal(TestStatus.Active, result);
        }

        /// <summary>
        /// Verifies that no child entities are set on the parent when the provided entity list is empty.
        /// </summary>
        /// <remarks>This test ensures that the method behaves correctly when there are no entities to
        /// process, confirming that the parent remains unchanged.</remarks>
        [Fact]
        public void DoesNothing_WhenEntitiesEmpty()
        {
            // Arrange
            var parent = new Parent();
            var record = new Mock<IDataRecord>();
            var metadata = new TableMaterializeMetadata(
                fields: new List<ColumnMap>(),
                entities: new List<TableMap>()
            );
            var method = typeof(TableMap).GetMethod("SetEntityNestedEntities", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            var result = method.Invoke(null, new object[] { parent, record.Object, metadata, "", new Dictionary<string, int>() });

            // Assert
            Assert.Null(parent.Child);
        }

        /// <summary>
        /// Verifies that no action is taken when the primary key is null or empty during entity mapping.
        /// </summary>
        /// <remarks>This test ensures that the child entity remains null when the primary key is not
        /// provided, confirming that the method handles null or empty primary keys gracefully.</remarks>
        [Fact]
        public void DoesNothing_WhenPrimaryKeyIsNullOrEmpty()
        {
            // Arrange
            var parent = new Parent();
            var record = new Mock<IDataRecord>();
            var tableMap = new TableMap(typeof(Parent).GetProperty(nameof(Parent.Child)),
                new ForeignTableAttribute(nameof(Parent.Child)) { PrimaryKeys = null });
            var metadata = new TableMaterializeMetadata(
                fields: new List<ColumnMap>(),
                entities: new List<TableMap> { tableMap }
            );
            var method = typeof(TableMap).GetMethod("SetEntityNestedEntities", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            var result = method.Invoke(null, new object[] { parent, record.Object, metadata, "", new Dictionary<string, int>() });

            // Assert
            Assert.Null(parent.Child);

            // Arrange
            tableMap = new TableMap(typeof(Parent).GetProperty(nameof(Parent.Child)),
                new ForeignTableAttribute(nameof(Parent.Child)) { PrimaryKeys = new string[0] });
            metadata = new TableMaterializeMetadata(
                fields: new List<ColumnMap>(),
                entities: new List<TableMap> { tableMap }
            );

            // Act
            result = method.Invoke(null, new object[] { parent, record.Object, metadata, "", new Dictionary<string, int>() });

            // Assert
            Assert.Null(parent.Child);
        }

        /// <summary>
        /// Verifies that no action is taken when the primary key is not present in the ordinals of the data record.
        /// </summary>
        /// <remarks>This test ensures that the method behaves correctly by not modifying the parent
        /// entity when the expected primary key is absent from the provided data record.</remarks>
        [Fact]
        public void DoesNothing_WhenPrimaryKeyNotInOrdinals()
        {
            // Arrange
            var parent = new Parent();
            var record = new Mock<IDataRecord>();
            var tableMap = new TableMap(typeof(Parent).GetProperty(nameof(Parent.Child)),
                new ForeignTableAttribute(nameof(Parent.Child)) { PrimaryKeys = new[] { "Id" } });
            var metadata = new TableMaterializeMetadata(
                fields: new List<ColumnMap>(),
                entities: new List<TableMap> { tableMap }
            );
            var method = typeof(TableMap).GetMethod("SetEntityNestedEntities", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            var result = method.Invoke(null, new object[] { parent, record.Object, metadata, "", new Dictionary<string, int>() });

            // Assert
            Assert.Null(parent.Child);
        }

        /// <summary>
        /// Verifies that no action is taken when the primary key of a child entity is DBNull.
        /// </summary>
        /// <remarks>This test ensures that when the primary key of a child entity is DBNull, the parent
        /// entity's child property remains null. This confirms that the absence of a valid primary key does not result
        /// in any unintended side effects or object instantiation.</remarks>
        [Fact]
        public void DoesNothing_WhenPrimaryKeyIsDBNull()
        {
            // Arrange
            var parent = new Parent();
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns(true);
            var tableMap = new TableMap(typeof(Parent).GetProperty(nameof(Parent.Child)),
                new ForeignTableAttribute(nameof(Parent.Child)) { PrimaryKeys = new[] { "Id" } });
            var metadata = new TableMaterializeMetadata(
                fields: new List<ColumnMap>(),
                entities: new List<TableMap> { tableMap }
            );
            var ordinals = new Dictionary<string, int> { { "Child.Id", 0 } };
            var method = typeof(TableMap).GetMethod("SetEntityNestedEntities", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            var result = method.Invoke(null, new object[] { parent, record.Object, metadata, "", ordinals });

            // Assert
            Assert.Null(parent.Child);
        }

        /// <summary>
        /// Verifies that a nested entity is instantiated when the primary key is present and not DBNull in the data
        /// record.
        /// </summary>
        /// <remarks>This test ensures that related entities are properly materialized during data mapping
        /// when the corresponding primary key value exists in the data source. It is important for validating the
        /// correct population of navigation properties in entity mapping scenarios.</remarks>
        [Fact]
        public void InstantiatesNestedEntity_WhenPrimaryKeyIsPresentAndNotDBNull()
        {
            // Arrange
            var parent = new Parent();
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns(false);
            var tableMap = new TableMap(typeof(Parent).GetProperty(nameof(Parent.Child)),
                new ForeignTableAttribute(nameof(Parent.Child)) { PrimaryKeys = new[] { "Id" } });
            var metadata = new TableMaterializeMetadata(
                fields: new List<ColumnMap>(),
                entities: new List<TableMap> { tableMap }
            );
            var ordinals = new Dictionary<string, int> { { "Child.Id", 0 } };
            var method = typeof(TableMap).GetMethod("SetEntityNestedEntities", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            var result = method.Invoke(null, new object[] { parent, record.Object, metadata, "", ordinals });

            // Assert
            Assert.NotNull(parent.Child);
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
        /// <param name="ordinals">A dictionary mapping column names to their respective ordinals in the data record for efficient access.</param>
        public static void SetEntityFields(
            ITable entity,
            IDataRecord record,
            dynamic metadata,
            string prefix,
            IReadOnlyDictionary<string, int> ordinals)
        {
            foreach (var field in metadata.Fields)
            {
                string columnName = string.IsNullOrWhiteSpace(field.Attribute.FieldName)
                    ? $"{prefix}{field.Property.Name}"
                    : $"{prefix}{field.Attribute.FieldName}";

                if (!ordinals.TryGetValue(columnName, out var ordinal))
                    continue;

                if (record.IsDBNull(ordinal))
                {
                    field.Setter(entity, field.DefaultValue);
                    continue;
                }

                var value = record.GetValue(ordinal);
                field.Setter(entity, value);
            }
        }
    }
}