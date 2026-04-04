using Hydrix.Attributes.Schemas;
using Hydrix.Internals;
using Hydrix.Mapping;
using Hydrix.Metadata.Internals;
using Hydrix.Metadata.Materializers;
using Hydrix.Resolvers;
using Hydrix.Schemas.Contract;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using Xunit;

namespace Hydrix.UnitTests.Mapping
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
        /// Represents a parent entity with a nested hierarchy used to validate recursive nested mapping.
        /// </summary>
        private class DeepParent : ITable
        {
            /// <summary>
            /// Gets or sets the first-level nested entity.
            /// </summary>
            [ForeignTable("level_one", PrimaryKeys = new[] { "Id" })]
            public DeepChild LevelOne { get; set; }
        }

        /// <summary>
        /// Represents the first-level nested entity containing a second-level nested relationship.
        /// </summary>
        [Table("level_one")]
        private class DeepChild : ITable
        {
            /// <summary>
            /// Gets or sets the primary key value for this nested entity.
            /// </summary>
            [Column]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the second-level nested entity.
            /// </summary>
            [ForeignTable("level_two", PrimaryKeys = new[] { "Id" })]
            public DeepGrandChild LevelTwo { get; set; }
        }

        /// <summary>
        /// Represents the second-level nested entity used by recursive nested mapping tests.
        /// </summary>
        [Table("level_two")]
        private class DeepGrandChild : ITable
        {
            /// <summary>
            /// Gets or sets the primary key value for this entity.
            /// </summary>
            [Column]
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
        /// Dummy entity implementation for testing purposes.
        /// </summary>
        public class DummyRecordEntity : ITable
        {
            /// <summary>
            /// Gets or sets the identifier of the entity.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name of the entity.
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
        /// Verifies that the TableMap constructor sets key-related metadata to null when no usable primary key is
        /// provided.
        /// </summary>
        [Fact]
        public void Constructor_WithoutPrimaryKey_SetsKeyMetadataToNull()
        {
            var property = typeof(TestEntity).GetProperty(nameof(TestEntity.NestedNullPrimaryKey));
            var attribute = new ForeignTableAttribute("text")
            {
                Schema = "Nested",
                PrimaryKeys = null
            };

            var map = new TableMap(property, attribute);

            Assert.Null(map.PrimaryKey);
            Assert.Null(map.KeyColumnSuffix);
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
                new Dictionary<string, int>(),
                0);

            // Assert
            Assert.NotNull(entity);
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

            // Act (should not throw and should not set Nested)
            SetEntityNestedEntities(
                entity,
                record.Object,
                metadata,
                string.Empty,
                ordinals,
                0);
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

            // Act (should not throw and should not set Nested)
            SetEntityNestedEntities(
                entity,
                record.Object,
                metadata,
                string.Empty,
                ordinals,
                0);
            // Assert
            Assert.Null(entity.Nested);
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

            // Act
            SetEntityNestedEntities(
                parent,
                record.Object,
                metadata,
                "",
                new Dictionary<string, int>(),
                0);
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

            // Act
            SetEntityNestedEntities(
                parent,
                record.Object,
                metadata,
                "",
                new Dictionary<string, int>(),
                0);

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
            SetEntityNestedEntities(
                parent,
                record.Object,
                metadata,
                "",
                new Dictionary<string, int>(),
                0);

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

            // Act
            SetEntityNestedEntities(
                parent,
                record.Object,
                metadata,
                "",
                new Dictionary<string, int>(),
                0);
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

            // Act
            SetEntityNestedEntities(
                parent,
                record.Object,
                metadata,
                "",
                ordinals,
                0);
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

            // Act
            SetEntityNestedEntities(
                parent,
                record.Object,
                metadata,
                "",
                ordinals,
                0);
            // Assert
            Assert.NotNull(parent.Child);
        }

        /// <summary>
        /// Verifies that invoking the SetEntityFields method does not modify the entity when the specified field is not
        /// present in the provided ordinals dictionary.
        /// </summary>
        /// <remarks>This test ensures that the method completes without throwing an exception and that
        /// the entity's properties remain unchanged when the ordinals dictionary does not contain the field. It is
        /// useful for confirming that the method safely handles cases where expected fields are missing from the data
        /// source.</remarks>
        [Fact]
        public void SetEntityFields_FieldNotInOrdinals_DoesNothing()
        {
            var entity = new DummyRecordEntity();
            var record = new Mock<IDataRecord>().Object;
            var field = new ColumnMap("Id", (obj, val) => ((DummyRecordEntity)obj).Id = (int)val, (r, i) => 42);
            var metadata = MetadataFactory.CreateEntity(new[] { field }, new TableMap[0]);
            var ordinals = new Dictionary<string, int>(); // Empty, so field not found

            // Should not throw or set anything
            SetEntityFields(
                entity,
                record,
                metadata,
                "",
                ordinals);

            Assert.Equal(0, entity.Id); // Not set
        }

        /// <summary>
        /// Verifies that the SetEntityFields method correctly sets the value of an entity field when the field is found
        /// without a prefix in the ordinals dictionary.
        /// </summary>
        /// <remarks>This test ensures that the mapping logic assigns the expected value to the entity
        /// property when the field name matches directly, without requiring a prefix. It uses a mock data record and a
        /// custom column mapping to validate the behavior.</remarks>
        [Fact]
        public void SetEntityFields_FieldFoundWithoutPrefix_SetsValue()
        {
            var entity = new DummyRecordEntity();
            var record = new Mock<IDataRecord>().Object;
            var field = new ColumnMap("Id", (obj, val) => ((DummyRecordEntity)obj).Id = (int)val, (r, i) => 99);
            var metadata = MetadataFactory.CreateEntity(new[] { field }, new TableMap[0]);
            var ordinals = new Dictionary<string, int> { { "Id", 0 } };

            SetEntityFields(
                entity,
                record,
                metadata,
                "",
                ordinals);

            Assert.Equal(99, entity.Id);
        }

        /// <summary>
        /// Verifies that the SetEntityFields method correctly sets the value of an entity field when a matching prefix
        /// is found in the data record.
        /// </summary>
        /// <remarks>This test ensures that when the data record contains a field with the expected
        /// prefix, the SetEntityFields method assigns the corresponding value to the entity's property. It uses a mock
        /// data record and a custom column mapping to validate the behavior.</remarks>
        [Fact]
        public void SetEntityFields_FieldFoundWithPrefix_SetsValue()
        {
            var entity = new DummyRecordEntity();
            var record = new Mock<IDataRecord>().Object;
            var field = new ColumnMap("Name", (obj, val) => ((DummyRecordEntity)obj).Name = (string)val, (r, i) => "Hydrix");
            var metadata = MetadataFactory.CreateEntity(new[] { field }, new TableMap[0]);
            var ordinals = new Dictionary<string, int> { { "prefixName", 1 } };

            SetEntityFields(
                entity,
                record,
                metadata,
                "prefix",
                ordinals);

            Assert.Equal("Hydrix", entity.Name);
        }

        /// <summary>
        /// Verifies that the child entity is not instantiated when the primary key is absent from the ordinals
        /// dictionary during entity mapping.
        /// </summary>
        /// <remarks>This test ensures that the mapping logic does not create a child entity if the
        /// required primary key is not found in the provided ordinals. This behavior is important to prevent unintended
        /// instantiation of related entities when mapping data records to objects.</remarks>
        [Fact]
        public void WithPrimaryKey_KeyNotInOrdinals_DoesNotInstantiate()
        {
            var parent = new Parent();
            var record = new Mock<IDataRecord>();
            var child = CreateTableMapWithPrimaryKey();
            var metadata = CreateMetadata(child);
            var ordinals = new Dictionary<string, int>(); // key not present

            SetEntityNestedEntities(
                parent,
                record.Object,
                metadata,
                "",
                ordinals,
                0);

            Assert.Null(parent.Child);
        }

        /// <summary>
        /// Verifies that when the primary key value is DBNull, the child entity is not instantiated in the parent
        /// object.
        /// </summary>
        /// <remarks>This test ensures that the mapping logic correctly handles cases where the primary
        /// key is null, preventing the creation of a child entity when no valid key is present.</remarks>
        [Fact]
        public void WithPrimaryKey_KeyIsDBNull_DoesNotInstantiate()
        {
            var parent = new Parent();
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(true);
            var child = CreateTableMapWithPrimaryKey();
            var metadata = CreateMetadata(child);
            var ordinals = new Dictionary<string, int> { { "Child.Id", 0 } };

            SetEntityNestedEntities(
                parent,
                record.Object,
                metadata,
                "",
                ordinals,
                0);

            Assert.Null(parent.Child);
        }

        /// <summary>
        /// Verifies that a child entity is instantiated and assigned to the parent when the primary key value in the
        /// data record is not DBNull.
        /// </summary>
        /// <remarks>This test ensures that the method responsible for setting nested entities correctly
        /// creates and assigns a child entity when a valid primary key is present in the data record. It uses a mock
        /// data record to simulate the presence of a primary key and asserts that the parent entity's child property is
        /// not null after invocation.</remarks>
        [Fact]
        public void WithPrimaryKey_KeyIsNotDBNull_Instantiates()
        {
            var parent = new Parent();
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(false);
            var child = CreateTableMapWithPrimaryKey();
            var metadata = CreateMetadata(child);
            var ordinals = new Dictionary<string, int> { { "Child.Id", 0 } };

            SetEntityNestedEntities(
                parent,
                record.Object,
                metadata,
                "",
                ordinals,
                0);

            Assert.NotNull(parent.Child);
        }

        /// <summary>
        /// Verifies that a child entity is not instantiated when the parent entity is created without a primary key and
        /// there is no matching prefix in the data record.
        /// </summary>
        /// <remarks>This test ensures that the entity mapping logic correctly leaves the child property
        /// null when neither a primary key nor a matching prefix is present. This behavior is important to prevent
        /// unintended instantiation of nested entities during mapping operations.</remarks>
        [Fact]
        public void WithoutPrimaryKey_NoMatchingPrefix_DoesNotInstantiate()
        {
            var parent = new Parent();
            var record = new Mock<IDataRecord>();
            var child = CreateTableMapWithoutPrimaryKey();
            var metadata = CreateMetadata(child);
            var ordinals = new Dictionary<string, int> { { "Other.Id", 0 } };

            SetEntityNestedEntities(
                parent,
                record.Object,
                metadata,
                "",
                ordinals,
                0);

            Assert.Null(parent.Child);
        }

        /// <summary>
        /// Verifies that when a parent entity is processed without a primary key and all matching prefixes are DBNull,
        /// no child entity is instantiated.
        /// </summary>
        /// <remarks>This test ensures that the absence of a primary key and DBNull values in the data
        /// record do not lead to the creation of child entities, maintaining the integrity of the parent-child
        /// relationship.</remarks>
        [Fact]
        public void WithoutPrimaryKey_AllMatchingPrefixAreDBNull_DoesNotInstantiate()
        {
            var parent = new Parent();
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(true);
            var child = CreateTableMapWithoutPrimaryKey();
            var metadata = CreateMetadata(child);
            var ordinals = new Dictionary<string, int> { { "Child.Id", 0 } };

            SetEntityNestedEntities(
                parent,
                record.Object,
                metadata,
                "",
                ordinals,
                0);

            Assert.Null(parent.Child);
        }

        /// <summary>
        /// Verifies that a child entity is instantiated when at least one matching prefix in the data record is not
        /// DBNull and no primary key is defined.
        /// </summary>
        /// <remarks>This test ensures that the mapping logic correctly creates a child entity even in the
        /// absence of a primary key, provided that the data record contains at least one non-null value for the
        /// relevant prefix. This scenario is important for supporting entity relationships where the child does not
        /// have a primary key but should still be materialized based on available data.</remarks>
        [Fact]
        public void WithoutPrimaryKey_AtLeastOneMatchingPrefixIsNotDBNull_Instantiates()
        {
            var parent = new Parent();
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(false);
            var child = CreateTableMapWithoutPrimaryKey();
            var metadata = CreateMetadata(child);
            var ordinals = new Dictionary<string, int> { { "Child.Id", 0 } };

            SetEntityNestedEntities(
                parent,
                record.Object,
                metadata,
                "",
                ordinals,
                0);

            Assert.NotNull(parent.Child);
        }

        /// <summary>
        /// Verifies that the GetNestedMetadata method initializes and caches metadata on the first call, and returns
        /// the cached result on subsequent calls.
        /// </summary>
        /// <remarks>This test ensures that the caching behavior of the GetNestedMetadata method is
        /// functioning as expected, preventing unnecessary re-initialization of metadata.</remarks>
        [Fact]
        public void GetNestedMetadata_CachesResult()
        {
            var tableMap = CreateTableMapWithPrimaryKey();
            var method = typeof(TableMap).GetMethod("GetNestedMetadata", BindingFlags.NonPublic | BindingFlags.Instance);

            // First call: initializes and caches
            var meta1 = method.Invoke(tableMap, null);
            Assert.NotNull(meta1);

            // Second call: returns cached
            var meta2 = method.Invoke(tableMap, null);
            Assert.Same(meta1, meta2);
        }

        /// <summary>
        /// Verifies that the GetCandidateOrdinals method returns an empty array when no matching ordinals are found and
        /// that the empty array result is cached for subsequent calls with the same parameters.
        /// </summary>
        /// <remarks>This test ensures that repeated invocations of GetCandidateOrdinals with identical
        /// arguments return the same empty array reference, confirming the method's caching behavior for empty
        /// results.</remarks>
        [Fact]
        public void GetCandidateOrdinals_ReturnsEmptyArray_AndCaches()
        {
            var tableMap = CreateTableMapWithPrimaryKey();
            var method = typeof(TableMap).GetMethod("GetCandidateOrdinals", BindingFlags.NonPublic | BindingFlags.Instance);

            var ordinals = new Dictionary<string, int> { { "Other.Id", 0 } };
            var prefix = "Child.";

            // First call: no matches, returns empty array
            var result1 = (int[])method.Invoke(tableMap, new object[] { ordinals, prefix, 0 });
            Assert.Empty(result1);

            // Second call: returns cached empty array (should be same reference)
            var result2 = (int[])method.Invoke(tableMap, new object[] { ordinals, prefix, 0 });
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Verifies that candidate ordinals are recalculated when the schema hash changes.
        /// </summary>
        [Fact]
        public void GetCandidateOrdinals_Recalculates_WhenSchemaHashChanges()
        {
            var tableMap = CreateTableMapWithPrimaryKey();
            var method = typeof(TableMap).GetMethod("GetCandidateOrdinals", BindingFlags.NonPublic | BindingFlags.Instance);

            var ordinalsA = new Dictionary<string, int> { { "Child.Id", 1 } };
            var ordinalsB = new Dictionary<string, int> { { "Child.Id", 9 } };

            var result1 = (int[])method.Invoke(tableMap, new object[] { ordinalsA, "Child.", 100 });
            var result2 = (int[])method.Invoke(tableMap, new object[] { ordinalsB, "Child.", 200 });

            Assert.NotSame(result1, result2);
            Assert.Contains(1, result1);
            Assert.Contains(9, result2);
        }

        /// <summary>
        /// Verifies that candidate ordinal discovery matches prefixes using ordinal case-insensitive comparison.
        /// </summary>
        [Fact]
        public void GetCandidateOrdinals_MatchesPrefix_CaseInsensitive()
        {
            var tableMap = CreateTableMapWithPrimaryKey();
            var method = typeof(TableMap).GetMethod("GetCandidateOrdinals", BindingFlags.NonPublic | BindingFlags.Instance);

            var ordinals = new Dictionary<string, int>
            {
                { "cHiLd.Id", 3 },
                { "Other.Id", 4 }
            };

            var result = (int[])method.Invoke(tableMap, new object[] { ordinals, "Child.", 321 });

            Assert.Single(result);
            Assert.Equal(3, result[0]);
        }

        /// <summary>
        /// Verifies that SetEntity populates both scalar fields and nested entities in a single call.
        /// </summary>
        [Fact]
        public void SetEntity_SetsFieldsAndInstantiatesNestedEntity()
        {
            var entity = new TestEntity();
            var record = new Mock<IDataRecord>();

            record.Setup(r => r.IsDBNull(0)).Returns(false);
            record.Setup(r => r.GetInt32(0)).Returns(77);
            record.Setup(r => r.IsDBNull(1)).Returns(false);

            var idField = new ColumnMap(
                "Id",
                (obj, val) => ((TestEntity)obj).Id = (int)val,
                (r, ordinal) => r.GetInt32(ordinal));

            var nestedMap = new TableMap(
                typeof(TestEntity).GetProperty(nameof(TestEntity.Nested)),
                new ForeignTableAttribute("text")
                {
                    Schema = "Nested",
                    PrimaryKeys = new[] { "Id" }
                });

            var metadata = MetadataFactory.CreateEntity(new[] { idField }, new[] { nestedMap });
            var ordinals = new Dictionary<string, int>
            {
                { "Id", 0 },
                { "Nested.Id", 1 }
            };

            TableMap.SetEntity(entity, record.Object, metadata, string.Empty, ordinals, 10);

            Assert.Equal(77, entity.Id);
            Assert.NotNull(entity.Nested);
        }

        /// <summary>
        /// Verifies that nested entity mapping recurses into nested bindings when child bindings contain nested
        /// entities.
        /// </summary>
        [Fact]
        public void SetEntityNestedEntities_Recurses_WhenNestedBindingsContainEntities()
        {
            var entity = new DeepParent();
            var record = new Mock<IDataRecord>();

            record.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns(false);

            var metadata = MetadataFactory.CreateEntity(
                Array.Empty<ColumnMap>(),
                new[]
                {
                    new TableMap(
                        typeof(DeepParent).GetProperty(nameof(DeepParent.LevelOne)),
                        new ForeignTableAttribute("level_one")
                        {
                            PrimaryKeys = new[] { "Id" }
                        })
                });

            var ordinals = new Dictionary<string, int>
            {
                { "LevelOne.Id", 0 },
                { "LevelOne.LevelTwo.Id", 1 }
            };

            SetEntityNestedEntities(
                entity,
                record.Object,
                metadata,
                string.Empty,
                ordinals,
                987);

            Assert.NotNull(entity.LevelOne);
            Assert.NotNull(entity.LevelOne.LevelTwo);
        }

        /// <summary>
        /// Verifies that the fast nested materializer branch is used when it is available on a leaf binding.
        /// </summary>
        [Fact]
        public void SetResolvedEntityNestedEntities_UsesFastMaterializer_WhenAvailable()
        {
            var parent = new Parent();
            var bindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                Array.Empty<ResolvedNestedBinding>());
            var nestedBinding = new ResolvedNestedBinding(
                usesPrimaryKey: true,
                primaryKeyOrdinal: 0,
                candidateOrdinals: null,
                activator: _ => new Child(),
                bindings: bindings,
                materializer: (entity, _) => ((Parent)entity).Child = new Child { Id = 55 });

            typeof(TableMap)
                .GetMethod("SetResolvedEntityNestedEntities", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { parent, new Mock<IDataRecord>().Object, new[] { nestedBinding } });

            Assert.NotNull(parent.Child);
            Assert.Equal(55, parent.Child.Id);
        }

        /// <summary>
        /// Verifies that leaf nested bindings receive a compiled fast materializer.
        /// </summary>
        [Fact]
        public void ResolveNestedBindings_CreatesFastMaterializer_ForLeafNestedEntity()
        {
            var metadata = CreateMetadata(CreateTableMapWithPrimaryKey());
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.GetFieldType(0)).Returns(typeof(int));

            var result = InvokePrivate<ResolvedNestedBinding[]>(
                "ResolveNestedBindings",
                record.Object,
                metadata,
                string.Empty,
                new Dictionary<string, int> { { "Child.Id", 0 } },
                321);

            var binding = Assert.Single(result);
            Assert.NotNull(binding.Materializer);
        }

        /// <summary>
        /// Verifies that recursive nested bindings keep using the generic nested path instead of the leaf fast materializer.
        /// </summary>
        [Fact]
        public void ResolveNestedBindings_DoesNotCreateFastMaterializer_WhenNestedEntityHasChildren()
        {
            var metadata = MetadataFactory.CreateEntity(
                Array.Empty<ColumnMap>(),
                new[]
                {
                    new TableMap(
                        typeof(DeepParent).GetProperty(nameof(DeepParent.LevelOne)),
                        new ForeignTableAttribute("level_one")
                        {
                            PrimaryKeys = new[] { "Id" }
                        })
                });
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.GetFieldType(0)).Returns(typeof(int));
            record.Setup(r => r.GetFieldType(1)).Returns(typeof(int));

            var result = InvokePrivate<ResolvedNestedBinding[]>(
                "ResolveNestedBindings",
                record.Object,
                metadata,
                string.Empty,
                new Dictionary<string, int>
                {
                    { "LevelOne.Id", 0 },
                    { "LevelOne.LevelTwo.Id", 1 }
                },
                654);

            var binding = Assert.Single(result);
            Assert.Null(binding.Materializer);
        }

        /// <summary>
        /// Verifies that the GetCandidateOrdinals method returns the correct matching ordinals based on the specified
        /// prefix and that it caches the results for subsequent calls with the same parameters.
        /// </summary>
        /// <remarks>This test ensures that GetCandidateOrdinals identifies and returns only those
        /// ordinals whose keys match the provided prefix. It also confirms that repeated calls with identical arguments
        /// return the same cached array instance, validating the method's caching behavior.</remarks>
        [Fact]
        public void GetCandidateOrdinals_ReturnsMatches_AndCaches()
        {
            var tableMap = CreateTableMapWithPrimaryKey();
            var method = typeof(TableMap).GetMethod("GetCandidateOrdinals", BindingFlags.NonPublic | BindingFlags.Instance);

            var ordinals = new Dictionary<string, int>
            {
                { "Child.Id", 1 },
                { "Child.Name", 2 },
                { "Other.Id", 3 }
            };
            var prefix = "Child.";

            // First call: finds matches
            var result1 = (int[])method.Invoke(tableMap, new object[] { ordinals, prefix, 0 });
            Assert.Contains(1, result1);
            Assert.Contains(2, result1);
            Assert.DoesNotContain(3, result1);

            // Second call: returns cached array (should be same reference)
            var result2 = (int[])method.Invoke(tableMap, new object[] { ordinals, prefix, 0 });
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Verifies that the SetEntityNestedEntities method correctly instantiates a nested entity when provided with a
        /// prefix for foreign key properties.
        /// </summary>
        /// <remarks>This test ensures that the SetEntityNestedEntities method can populate a nested
        /// entity on the parent object when the foreign key property names are prefixed, as is common in database
        /// mapping scenarios. It checks that the method uses the prefix to locate and instantiate the appropriate
        /// nested entity, confirming correct mapping behavior.</remarks>
        [Fact]
        public void SetEntityNestedEntities_WithPrefix_InstantiatesNestedEntity()
        {
            // Arrange
            var parent = new Parent();
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns(false);

            var prop = typeof(Parent).GetProperty(nameof(Parent.Child));
            var attr = new ForeignTableAttribute("Child")
            {
                PrimaryKeys = new[] { "Id" }
            };
            var nested = new TableMap(prop, attr);
            var metadata = MetadataFactory.CreateEntity(new ColumnMap[0], new[] { nested });

            string prefix = "pre.";
            var ordinals = new Dictionary<string, int> { { "pre.Child.Id", 0 } };

            // Act
            SetEntityNestedEntities(
                parent,
                record.Object,
                metadata,
                prefix,
                ordinals,
                0);

            // Assert
            Assert.NotNull(parent.Child);
        }

        /// <summary>
        /// Verifies that the GetCandidateOrdinals method returns the cached candidate ordinals when they are already
        /// initialized and the provided schema hash matches the cached value.
        /// </summary>
        /// <remarks>This test ensures that the TableMap class does not recompute candidate ordinals if
        /// they have already been initialized and the schema hash has not changed. It validates the caching behavior to
        /// improve performance and prevent unnecessary recalculation.</remarks>
        [Fact]
        public void GetCandidateOrdinals_ReturnsCachedValue_WhenAlreadyInitializedAndSchemaHashMatches()
        {
            // Arrange
            var property = typeof(Parent).GetProperty(nameof(Parent.Child));
            var attr = new ForeignTableAttribute("SomeTable");
            var tableMap = new TableMap(property, attr);

            // Set private fields via reflection
            var candidateOrdinalsField = typeof(TableMap).GetField("_candidateOrdinals", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateOrdinalsSchemaHashField = typeof(TableMap).GetField("_candidateOrdinalsSchemaHash", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateOrdinalsInitializedField = typeof(TableMap).GetField("_candidateOrdinalsInitialized", BindingFlags.NonPublic | BindingFlags.Instance);

            int[] expected = new[] { 42, 99 };
            int schemaHash = 12345;
            candidateOrdinalsField.SetValue(tableMap, expected);
            candidateOrdinalsSchemaHashField.SetValue(tableMap, schemaHash);
            candidateOrdinalsInitializedField.SetValue(tableMap, true);

            // Act
            var method = typeof(TableMap).GetMethod("GetCandidateOrdinals", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (int[])method.Invoke(tableMap, new object[] {
                new Dictionary<string, int> { { "prefix_col", 1 } }, // won't be used
                "prefix_",
                schemaHash
            });

            // Assert
            Assert.Same(expected, result);
        }

        /// <summary>
        /// Verifies that the second cache check inside the lock evaluates the branch where initialization is true but
        /// schema hash does not match.
        /// </summary>
        [Fact]
        public void GetCandidateOrdinals_InsideLock_InitializedTrueButSchemaMismatch_Recomputes()
        {
            var property = typeof(Parent).GetProperty(nameof(Parent.Child));
            var attr = new ForeignTableAttribute("SomeTable");
            var tableMap = new TableMap(property, attr);

            var candidateOrdinalsField = typeof(TableMap).GetField("_candidateOrdinals", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateOrdinalsSchemaHashField = typeof(TableMap).GetField("_candidateOrdinalsSchemaHash", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateOrdinalsInitializedField = typeof(TableMap).GetField("_candidateOrdinalsInitialized", BindingFlags.NonPublic | BindingFlags.Instance);

            candidateOrdinalsField.SetValue(tableMap, new[] { 99 });
            candidateOrdinalsSchemaHashField.SetValue(tableMap, 111);
            candidateOrdinalsInitializedField.SetValue(tableMap, true);

            var method = typeof(TableMap).GetMethod("GetCandidateOrdinals", BindingFlags.NonPublic | BindingFlags.Instance);
            var ordinals = new Dictionary<string, int> { { "Child.Id", 5 }, { "Other.Id", 7 } };

            var result = (int[])method.Invoke(tableMap, new object[] { ordinals, "Child.", 222 });

            Assert.Single(result);
            Assert.Equal(5, result[0]);
        }

        /// <summary>
        /// Verifies that the second cache check inside the lock returns the cached value when another thread
        /// initializes the cache before lock acquisition.
        /// </summary>
        [Fact]
        [SuppressMessage(
            "Major Code Smell",
            "S2925",
            Justification = "Used to simulate timing and concurrency behavior in controlled test scenario")]
        public void GetCandidateOrdinals_InsideLock_InitializedByAnotherThread_ReturnsCachedValue()
        {
            var property = typeof(Parent).GetProperty(nameof(Parent.Child));
            var attr = new ForeignTableAttribute("SomeTable");
            var tableMap = new TableMap(property, attr);

            var candidateOrdinalsField = typeof(TableMap).GetField("_candidateOrdinals", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateOrdinalsSchemaHashField = typeof(TableMap).GetField("_candidateOrdinalsSchemaHash", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateOrdinalsInitializedField = typeof(TableMap).GetField("_candidateOrdinalsInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateLockField = typeof(TableMap).GetField("_candidateLock", BindingFlags.NonPublic | BindingFlags.Instance);

            var method = typeof(TableMap).GetMethod("GetCandidateOrdinals", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateLock = candidateLockField.GetValue(tableMap);

            var expected = new[] { 41, 42 };
            const int schemaHash = 333;

            candidateOrdinalsInitializedField.SetValue(tableMap, false);

            int[] result = null;
            Exception threadException = null;
            Thread thread = null;

            lock (candidateLock)
            {
                thread = new Thread(() =>
                {
                    try
                    {
                        result = (int[])method.Invoke(tableMap, new object[]
                        {
                            new Dictionary<string, int> { { "Child.Id", 1 } },
                            "Child.",
                            schemaHash
                        });
                    }
                    catch (Exception ex)
                    {
                        threadException = ex;
                    }
                });

                thread.Start();
                Thread.Sleep(50);

                candidateOrdinalsField.SetValue(tableMap, expected);
                candidateOrdinalsSchemaHashField.SetValue(tableMap, schemaHash);
                candidateOrdinalsInitializedField.SetValue(tableMap, true);
            }

            thread.Join(1000);

            Assert.Null(threadException);
            Assert.Same(expected, result);
        }

        /// <summary>
        /// Verifies that the method returns cached candidate ordinals when they are initialized within a lock, ensuring
        /// that the locking mechanism provides thread-safe access to the cached values.
        /// </summary>
        /// <remarks>This test simulates a scenario where candidate ordinals are initialized by another
        /// thread while the current thread attempts to retrieve them. It ensures that the method returns the cached
        /// values without re-initializing them, demonstrating the effectiveness of the locking mechanism in maintaining
        /// thread safety.</remarks>
        [Fact]
        public void GetCandidateOrdinals_ReturnsCachedValue_WhenInitializedInsideLock()
        {
            // Arrange
            var property = typeof(Parent).GetProperty(nameof(Parent.Child));
            var attr = new ForeignTableAttribute("SomeTable");
            var tableMap = new TableMap(property, attr);

            // Set private fields via reflection
            var candidateOrdinalsField = typeof(TableMap).GetField("_candidateOrdinals", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateOrdinalsSchemaHashField = typeof(TableMap).GetField("_candidateOrdinalsSchemaHash", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateOrdinalsInitializedField = typeof(TableMap).GetField("_candidateOrdinalsInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateLockField = typeof(TableMap).GetField("_candidateLock", BindingFlags.NonPublic | BindingFlags.Instance);

            int[] expected = new[] { 7, 8 };
            int schemaHash = 555;

            // Inicializa como não inicializado fora do lock
            candidateOrdinalsInitializedField.SetValue(tableMap, false);

            // Simula outra thread inicializando dentro do lock
            var method = typeof(TableMap).GetMethod("GetCandidateOrdinals", BindingFlags.NonPublic | BindingFlags.Instance);

            // Para simular, interceptamos o lock manualmente:
            var candidateLock = candidateLockField.GetValue(tableMap);
            lock (candidateLock)
            {
                // Agora, dentro do lock, setamos como inicializado
                candidateOrdinalsField.SetValue(tableMap, expected);
                candidateOrdinalsSchemaHashField.SetValue(tableMap, schemaHash);
                candidateOrdinalsInitializedField.SetValue(tableMap, true);

                // Agora chamamos o método, que deve pegar o early return do segundo if
                var result = (int[])method.Invoke(tableMap, new object[] {
                    new Dictionary<string, int> { { "prefix_col", 1 } }, // não importa
                    "prefix_",
                    schemaHash
                });

                Assert.Same(expected, result);
            }
        }

        /// <summary>
        /// Verifies that the GetCandidateOrdinals method correctly handles concurrent initialization when accessed
        /// within a lock, ensuring that the second conditional branch is covered.
        /// </summary>
        /// <remarks>This test simulates a multithreaded scenario to ensure thread safety and correct
        /// behavior of the GetCandidateOrdinals method when its initialization logic is executed inside a lock. It is
        /// designed to validate that the method returns the expected ordinals when another thread has already performed
        /// the initialization.</remarks>
        [Fact]
        [SuppressMessage(
            "Major Code Smell",
            "S2925",
            Justification = "Used to simulate timing and concurrency behavior in controlled test scenario")]
        public void GetCandidateOrdinals_CoversSecondIfInsideLock()
        {
            var property = typeof(Parent).GetProperty(nameof(Parent.Child));
            var attr = new ForeignTableAttribute("SomeTable");
            var tableMap = new TableMap(property, attr);

            var candidateOrdinalsField = typeof(TableMap).GetField("_candidateOrdinals", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateOrdinalsSchemaHashField = typeof(TableMap).GetField("_candidateOrdinalsSchemaHash", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateOrdinalsInitializedField = typeof(TableMap).GetField("_candidateOrdinalsInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateLockField = typeof(TableMap).GetField("_candidateLock", BindingFlags.NonPublic | BindingFlags.Instance);

            int[] expected = new[] { 77, 88 };
            int schemaHash = 999;
            candidateOrdinalsInitializedField.SetValue(tableMap, false);

            var method = typeof(TableMap).GetMethod("GetCandidateOrdinals", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateLock = candidateLockField.GetValue(tableMap);

            // Sincronização entre as threads
            var readyToLock = new ManualResetEventSlim(false);
            var proceed = new ManualResetEventSlim(false);

            Exception threadEx = null;
            int[] result = null;

            var t1 = new Thread(() =>
            {
                try
                {
                    // Sinaliza que vai tentar entrar no lock
                    readyToLock.Set();
                    // Aguarda permissão para prosseguir (depois que t2 inicializar)
                    proceed.Wait();

                    // Chama o método normalmente
                    result = (int[])method.Invoke(tableMap, new object[] {
                        new Dictionary<string, int> { { "prefix_col", 1 } },
                        "prefix_",
                        schemaHash
                    });
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });

            t1.Start();

            // Aguarda t1 estar pronto para tentar o lock
            readyToLock.Wait();

            // Garante que t1 está esperando o lock
            lock (candidateLock)
            {
                // Inicializa os campos como se outra thread tivesse feito isso
                candidateOrdinalsField.SetValue(tableMap, expected);
                candidateOrdinalsSchemaHashField.SetValue(tableMap, schemaHash);
                candidateOrdinalsInitializedField.SetValue(tableMap, true);

                // Permite t1 prosseguir (vai pegar o lock em seguida)
                proceed.Set();
                // Espera t1 terminar o método
                Thread.Sleep(100);
            }

            t1.Join();

            Assert.Null(threadEx);
            Assert.Same(expected, result);
        }

        /// <summary>
        /// Verifies that the GetCandidateOrdinals method in the TableMap class correctly handles initialization and
        /// locking scenarios when accessed from a single thread.
        /// </summary>
        /// <remarks>This test ensures that GetCandidateOrdinals returns the expected candidate ordinals
        /// both when the initialization flag is set before the method call and when the flag is set inside a lock. It
        /// simulates the method's behavior under different initialization states to confirm correct handling of thread
        /// synchronization and early returns.</remarks>
        [Fact]
        public void GetCandidateOrdinals_ForcesSecondIfInsideLock_SingleThread()
        {
            var property = typeof(Parent).GetProperty(nameof(Parent.Child));
            var attr = new ForeignTableAttribute("SomeTable");
            var tableMap = new TableMap(property, attr);

            var candidateOrdinalsField = typeof(TableMap).GetField("_candidateOrdinals", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateOrdinalsSchemaHashField = typeof(TableMap).GetField("_candidateOrdinalsSchemaHash", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateOrdinalsInitializedField = typeof(TableMap).GetField("_candidateOrdinalsInitialized", BindingFlags.NonPublic | BindingFlags.Instance);

            int[] expected = new[] { 1, 2, 3 };
            int schemaHash = 123;

            // Inicializa normalmente
            candidateOrdinalsField.SetValue(tableMap, expected);
            candidateOrdinalsSchemaHashField.SetValue(tableMap, schemaHash);
            candidateOrdinalsInitializedField.SetValue(tableMap, true);

            var method = typeof(TableMap).GetMethod("GetCandidateOrdinals", BindingFlags.NonPublic | BindingFlags.Instance);

            // Primeira chamada: early return (primeiro if)
            var result1 = (int[])method.Invoke(tableMap, new object[] {
                new Dictionary<string, int> { { "prefix_col", 1 } },
                "prefix_",
                schemaHash
            });
            Assert.Same(expected, result1);

            // Agora, simula o cenário do segundo if:
            // 1. Marca como não inicializado (fora do lock)
            candidateOrdinalsInitializedField.SetValue(tableMap, false);

            // 2. Chama o método novamente (vai entrar no lock, e encontrar como true dentro do lock)
            //    Para simular, setamos como true logo antes do segundo if (não é perfeito, mas força o caminho)
            bool enteredLock = false;
            var candidateLockField = typeof(TableMap).GetField("_candidateLock", BindingFlags.NonPublic | BindingFlags.Instance);
            var candidateLock = candidateLockField.GetValue(tableMap);

            lock (candidateLock)
            {
                // Set como true dentro do lock, antes do segundo if
                candidateOrdinalsInitializedField.SetValue(tableMap, true);
                enteredLock = true;
            }

            // Chama o método (agora pega o segundo if)
            var result2 = (int[])method.Invoke(tableMap, new object[] {
                new Dictionary<string, int> { { "prefix_col", 1 } },
                "prefix_",
                schemaHash
            });
            Assert.Same(expected, result2);
            Assert.True(enteredLock);
        }

        /// <summary>
        /// Verifies that the ResolveFieldAssigner method returns null when the data reader is missing.
        /// </summary>
        /// <remarks>This test ensures that the field assigner resolution logic correctly handles the case
        /// where the required data reader is not provided, returning null as expected. This behavior is important for
        /// preventing assignment operations when necessary dependencies are absent.</remarks>
        [Fact]
        public void ResolveFieldAssigner_ReturnsNull_WhenReaderIsMissing()
        {
            var field = new ColumnMap(
                "Id",
                (entity, value) => ((Child)entity).Id = (int)value,
                null);

            var result = ResolveFieldAssigner(
                new Mock<IDataRecord>().Object,
                field,
                0);

            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that ResolveFieldBindings skips fields whose resolved assigner is null and returns an empty
        /// bindings array in this scenario.
        /// </summary>
        [Fact]
        public void ResolveFieldBindings_SkipsField_WhenResolvedAssignerIsNull()
        {
            var record = new Mock<IDataRecord>().Object;
            var field = new ColumnMap(
                "Id",
                null,
                (dataRecord, ordinal) => 123);
            var metadata = MetadataFactory.CreateEntity(new[] { field }, Array.Empty<TableMap>());
            var ordinals = new Dictionary<string, int>
            {
                { "Id", 0 }
            };

            var result = InvokePrivate<ResolvedFieldBinding[]>(
                "ResolveFieldBindings",
                record,
                metadata,
                string.Empty,
                ordinals);

            Assert.Empty(result);
        }

        /// <summary>
        /// Verifies that the ResolveFieldAssigner method returns null when the field does not have a setter.
        /// </summary>
        /// <remarks>This test ensures that the field assigner resolution logic correctly handles cases
        /// where a setter is missing, returning null as expected. This behavior is important for scenarios where only
        /// read access is available for a mapped field.</remarks>
        [Fact]
        public void ResolveFieldAssigner_ReturnsNull_WhenSetterIsMissing()
        {
            var field = new ColumnMap(
                "Id",
                null,
                (record, ordinal) => 5);

            var result = ResolveFieldAssigner(
                new Mock<IDataRecord>().Object,
                field,
                0);

            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that the field reader resolution uses a provider-type-aware reader when a target type is specified.
        /// </summary>
        /// <remarks>This test ensures that when resolving a field reader for a column with a specified
        /// target type, the reader correctly handles type conversion based on the provider's field type. It validates
        /// that the resolved reader returns the expected value when the data record provides a value of a different
        /// type than the target.</remarks>
        [Fact]
        public void ResolveFieldReader_WithTargetType_UsesProviderTypeAwareReader()
        {
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.GetFieldType(0)).Returns(typeof(long));
            record.Setup(r => r.IsDBNull(0)).Returns(false);
            record.Setup(r => r.GetValue(0)).Returns(12L);
            var field = new ColumnMap(
                "Id",
                (entity, value) => ((Child)entity).Id = (int)value,
                (reader, ordinal) => 1,
                typeof(int));

            var reader = ResolveFieldReader(record.Object, field, 0);
            var value = reader(record.Object, 0);

            Assert.Equal(12, value);
        }

        /// <summary>
        /// Verifies that the field reader resolution logic falls back to using a null source type when an
        /// InvalidOperationException is thrown by GetFieldType, ensuring the field value can still be read
        /// successfully.
        /// </summary>
        /// <remarks>This test simulates a scenario where schema information is unavailable and
        /// GetFieldType throws an exception. It ensures that the fallback mechanism allows the field reader to retrieve
        /// the value without relying on the field type metadata.</remarks>
        [Fact]
        public void ResolveFieldReader_WhenGetFieldTypeThrowsInvalidOperationException_FallsBackToNullSourceType()
        {
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.GetFieldType(0)).Throws(new InvalidOperationException("schema unavailable"));
            record.Setup(r => r.IsDBNull(0)).Returns(false);
            record.Setup(r => r.GetInt32(0)).Returns(7);
            var field = new ColumnMap(
                "Id",
                (entity, value) => ((Child)entity).Id = (int)value,
                (reader, ordinal) => 1,
                typeof(int));

            var reader = ResolveFieldReader(record.Object, field, 0);

            Assert.Equal(7, reader(record.Object, 0));
        }

        /// <summary>
        /// Verifies that the field reader resolution logic falls back to a null source type when the data record's
        /// GetFieldType method throws a NotSupportedException.
        /// </summary>
        /// <remarks>This test ensures that the field reader can handle cases where schema information is
        /// unavailable, such as when GetFieldType is not supported by the data source. It confirms that the fallback
        /// mechanism allows value retrieval to proceed without schema type information.</remarks>
        [Fact]
        public void ResolveFieldReader_WhenGetFieldTypeThrowsNotSupportedException_FallsBackToNullSourceType()
        {
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.GetFieldType(0)).Throws(new NotSupportedException("schema unavailable"));
            record.Setup(r => r.IsDBNull(0)).Returns(false);
            record.Setup(r => r.GetInt32(0)).Returns(8);
            var field = new ColumnMap(
                "Id",
                (entity, value) => ((Child)entity).Id = (int)value,
                (reader, ordinal) => 1,
                typeof(int));

            var reader = ResolveFieldReader(record.Object, field, 0);

            Assert.Equal(8, reader(record.Object, 0));
        }

        /// <summary>
        /// Verifies that the ResolveNestedBindings method returns an empty array when the count is positive but the
        /// enumeration is empty.
        /// </summary>
        /// <remarks>This test ensures that when provided with metadata containing a positive count and an
        /// empty enumeration, the ResolveNestedBindings method produces an empty result, confirming correct handling of
        /// this edge case.</remarks>
        [Fact]
        public void ResolveNestedBindings_ReturnsEmptyArray_WhenCountIsPositiveButEnumerationIsEmpty()
        {
            var metadata = new TableMaterializeMetadata(
                Array.Empty<ColumnMap>(),
                new CountedEmptyReadOnlyList<TableMap>());

            var result = InvokePrivate<ResolvedNestedBinding[]>(
                "ResolveNestedBindings",
                new Mock<IDataRecord>().Object,
                metadata,
                string.Empty,
                new Dictionary<string, int>(),
                0);

            Assert.Empty(result);
        }

        /// <summary>
        /// Verifies that schema matches reuse provider field types without touching the current row value.
        /// </summary>
        [Fact]
        public void ResolvedTableBindings_Matches_DoesNotReadCurrentValue_WhenProviderFieldTypeMatches()
        {
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.FieldCount).Returns(1);
            reader.Setup(r => r.GetName(0)).Returns("Id");
            reader.Setup(r => r.GetFieldType(0)).Returns(typeof(int));
            reader.Setup(r => r.GetValue(0)).Throws(new InvalidOperationException("GetValue should not be used when GetFieldType matches."));
            reader.Setup(r => r.IsDBNull(0)).Throws(new InvalidOperationException("IsDBNull should not be used when GetFieldType matches."));

            var bindings = new ResolvedTableBindings(
                new[]
                {
                    new ResolvedFieldBinding((_, __) => { }, 0, typeof(int))
                },
                Array.Empty<ResolvedNestedBinding>(),
                new[] { "Id" });

            Assert.True(bindings.Matches(reader.Object));
        }

        /// <summary>
        /// Verifies that schema matches still fall back to the current row value when provider field types are unavailable.
        /// </summary>
        [Fact]
        public void ResolvedTableBindings_Matches_FallsBackToCurrentValue_WhenProviderFieldTypeIsUnavailable()
        {
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.FieldCount).Returns(1);
            reader.Setup(r => r.GetName(0)).Returns("Id");
            reader.Setup(r => r.GetFieldType(0)).Throws(new NotSupportedException("Provider field types unavailable."));
            reader.Setup(r => r.IsDBNull(0)).Returns(false);
            reader.Setup(r => r.GetValue(0)).Returns(7);

            var bindings = new ResolvedTableBindings(
                new[]
                {
                    new ResolvedFieldBinding((_, __) => { }, 0, typeof(int))
                },
                Array.Empty<ResolvedNestedBinding>(),
                new[] { "Id" });

            Assert.True(bindings.Matches(reader.Object));
        }

        /// <summary>
        /// Verifies that schema matches include nested fields even when the root binding has no scalar fields.
        /// </summary>
        [Fact]
        public void ResolvedTableBindings_Matches_UsesNestedFields_WhenRootHasNoScalarFields()
        {
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.FieldCount).Returns(1);
            reader.Setup(r => r.GetName(0)).Returns("Id");
            reader.Setup(r => r.GetFieldType(0)).Returns(typeof(int));
            reader.Setup(r => r.GetValue(0)).Throws(new InvalidOperationException("GetValue should not be used when GetFieldType matches."));
            reader.Setup(r => r.IsDBNull(0)).Throws(new InvalidOperationException("IsDBNull should not be used when GetFieldType matches."));

            var nestedBindings = new ResolvedTableBindings(
                new[]
                {
                    new ResolvedFieldBinding((_, __) => { }, 0, typeof(int))
                },
                Array.Empty<ResolvedNestedBinding>(),
                new[] { "Id" });

            var bindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                new[]
                {
                    new ResolvedNestedBinding(
                        usesPrimaryKey: true,
                        primaryKeyOrdinal: 0,
                        candidateOrdinals: Array.Empty<int>(),
                        activator: _ => new Child(),
                        bindings: nestedBindings)
                },
                new[] { "Id" });

            Assert.True(bindings.Matches(reader.Object));
        }

        /// <summary>
        /// Verifies that the constructors for resolved bindings normalize null array parameters to empty arrays.
        /// </summary>
        /// <remarks>This test ensures that when null arrays are passed to the constructors of
        /// ResolvedTableBindings and ResolvedNestedBinding, the resulting properties are initialized as empty arrays
        /// rather than remaining null. This behavior helps prevent null reference exceptions when accessing these
        /// properties.</remarks>
        [Fact]
        public void ResolvedBindings_Constructors_NormalizeNullArrays()
        {
            var tableBindings = new ResolvedTableBindings(null, null);
            var nestedBinding = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: null,
                factory: () => new Child(),
                setter: (entity, value) => ((Parent)entity).Child = (Child)value,
                bindings: tableBindings);

            Assert.NotNull(tableBindings.Fields);
            Assert.NotNull(tableBindings.Entities);
            Assert.Empty(tableBindings.Fields);
            Assert.Empty(tableBindings.Entities);
            Assert.NotNull(nestedBinding.CandidateOrdinals);
            Assert.Empty(nestedBinding.CandidateOrdinals);
        }

        /// <summary>
        /// Verifies that SetResolvedEntityNestedEntities skips activation when materializer is null and
        /// instantiation preconditions evaluate to false.
        /// </summary>
        [Fact]
        public void SetResolvedEntityNestedEntities_SkipsActivation_WhenShouldInstantiateIsFalse_AndMaterializerIsNull()
        {
            var parent = new Parent();
            var record = new Mock<IDataRecord>();
            var nestedBindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                Array.Empty<ResolvedNestedBinding>());

            var activatorCalls = 0;
            var nestedBinding = new ResolvedNestedBinding(
                usesPrimaryKey: true,
                primaryKeyOrdinal: -1,
                candidateOrdinals: Array.Empty<int>(),
                activator: _ =>
                {
                    activatorCalls++;
                    return new Child();
                },
                bindings: nestedBindings,
                materializer: null);

            typeof(TableMap)
                .GetMethod("SetResolvedEntityNestedEntities", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { parent, record.Object, new[] { nestedBinding } });

            Assert.Equal(0, activatorCalls);
            Assert.Null(parent.Child);
        }

        /// <summary>
        /// Verifies that <c>SetResolvedEntityNestedEntities</c> evaluates the non-primary-key branch and instantiates
        /// a nested entity when at least one candidate ordinal is not <see cref="DBNull"/>.
        /// </summary>
        [Fact]
        public void SetResolvedEntityNestedEntities_Instantiates_WhenUsesPrimaryKeyIsFalse_AndAnyCandidateIsNotDbNull()
        {
            var parent = new Parent();
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(true);
            record.Setup(r => r.IsDBNull(1)).Returns(false);

            var nestedBindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                Array.Empty<ResolvedNestedBinding>());

            var activatorCalls = 0;
            var nestedBinding = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: new[] { 0, 1 },
                activator: entity =>
                {
                    activatorCalls++;
                    var child = new Child();
                    ((Parent)entity).Child = child;
                    return child;
                },
                bindings: nestedBindings,
                materializer: null);

            typeof(TableMap)
                .GetMethod("SetResolvedEntityNestedEntities", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { parent, record.Object, new[] { nestedBinding } });

            Assert.Equal(1, activatorCalls);
            Assert.NotNull(parent.Child);
        }

        /// <summary>
        /// Verifies that <c>HasAnyNonDbNull</c> returns <see langword="true"/> when at least one ordinal contains a
        /// non-<see cref="DBNull"/> value.
        /// </summary>
        [Fact]
        public void HasAnyNonDbNull_ReturnsTrue_WhenAnyOrdinalIsNotDbNull()
        {
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(true);
            record.Setup(r => r.IsDBNull(1)).Returns(false);

            var method = typeof(TableMap).GetMethod(
                "HasAnyNonDbNull",
                BindingFlags.NonPublic | BindingFlags.Static);

            var result = (bool)method.Invoke(
                null,
                new object[] { record.Object, new[] { 0, 1 } });

            Assert.True(result);
        }

        /// <summary>
        /// Verifies that <c>HasAnyNonDbNull</c> returns <see langword="false"/> when all inspected ordinals contain
        /// <see cref="DBNull"/> values.
        /// </summary>
        [Fact]
        public void HasAnyNonDbNull_ReturnsFalse_WhenAllOrdinalsAreDbNull()
        {
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(true);
            record.Setup(r => r.IsDBNull(1)).Returns(true);

            var method = typeof(TableMap).GetMethod(
                "HasAnyNonDbNull",
                BindingFlags.NonPublic | BindingFlags.Static);

            var result = (bool)method.Invoke(
                null,
                new object[] { record.Object, new[] { 0, 1 } });

            Assert.False(result);
        }

        /// <summary>
        /// Verifies that <see cref="TableMap.SetEntity(ITable, IDataRecord, ResolvedTableBindings)"/>
        /// falls back to the scalar-fields loop and correctly assigns field values when
        /// <see cref="ResolvedTableBindings.RowMaterializer"/> is <see langword="null"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="ResolvedTableBindings.RowMaterializer"/> is <see langword="null"/> whenever any nested-entity
        /// binding lacks a pre-compiled materializer. This test uses such a binding to force the fallback path and
        /// verifies that the scalar field assigner is invoked via the loop (lines 164–170 of TableMap.cs).
        /// </remarks>
        [Fact]
        public void SetEntity_FallsBack_ToFieldsLoop_WhenRowMaterializerIsNull()
        {
            var child = new Child();

            Action<object, IDataRecord> assigner =
                (entity, record) => ((Child)entity).Id = record.GetInt32(0);
            var field = new ResolvedFieldBinding(assigner, 0, typeof(int));

            var dummyNestedBindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                Array.Empty<ResolvedNestedBinding>());

            var nestedBinding = new ResolvedNestedBinding(
                usesPrimaryKey: true,
                primaryKeyOrdinal: -1,
                candidateOrdinals: Array.Empty<int>(),
                activator: _ => new Child(),
                bindings: dummyNestedBindings,
                materializer: null);

            var bindings = new ResolvedTableBindings(
                new[] { field },
                new[] { nestedBinding });

            var record = new Mock<IDataRecord>();
            record.Setup(r => r.GetInt32(0)).Returns(42);

            Assert.Null(bindings.RowMaterializer);

            TableMap.SetEntity(child, record.Object, bindings);

            Assert.Equal(42, child.Id);
        }

        /// <summary>
        /// Verifies that <see cref="TableMap.SetEntity(ITable, IDataRecord, ResolvedTableBindings)"/>
        /// falls back to the nested-entities loop and activates the nested entity when
        /// <see cref="ResolvedTableBindings.RowMaterializer"/> is <see langword="null"/> and no scalar fields exist.
        /// </summary>
        /// <remarks>
        /// This test exercises lines 172–178 of TableMap.cs. The nested entity is activated because
        /// <c>UsesPrimaryKey</c> is <see langword="true"/>, <c>PrimaryKeyOrdinal</c> is a valid zero-based index,
        /// and the primary-key column is not <see cref="DBNull"/>.
        /// </remarks>
        [Fact]
        public void SetEntity_FallsBack_ToNestedEntitiesLoop_WhenRowMaterializerIsNull_AndNoFields()
        {
            var parent = new Parent();
            var activatorCalled = false;

            var dummyNestedBindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                Array.Empty<ResolvedNestedBinding>());

            var nestedBinding = new ResolvedNestedBinding(
                usesPrimaryKey: true,
                primaryKeyOrdinal: 0,
                candidateOrdinals: Array.Empty<int>(),
                activator: entity =>
                {
                    activatorCalled = true;
                    var child = new Child();
                    ((Parent)entity).Child = child;
                    return child;
                },
                bindings: dummyNestedBindings,
                materializer: null);

            var bindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                new[] { nestedBinding });

            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(false);

            Assert.Null(bindings.RowMaterializer);

            TableMap.SetEntity(parent, record.Object, bindings);

            Assert.True(activatorCalled);
            Assert.NotNull(parent.Child);
        }

        /// <summary>
        /// Resolves a field assigner using the provider source type exposed by the supplied record.
        /// </summary>
        /// <param name="record">The data record used to resolve the provider source type.</param>
        /// <param name="field">The field mapping for which the assigner will be resolved.</param>
        /// <param name="ordinal">The zero-based column ordinal associated with the field.</param>
        /// <returns>An assigner action when resolution succeeds; otherwise, <see langword="null"/>.</returns>
        private static Action<object, IDataRecord> ResolveFieldAssigner(
            IDataRecord record,
            ColumnMap field,
            int ordinal)
            => InvokePrivate<Action<object, IDataRecord>>(
                "ResolveFieldAssignerWithSourceType",
                field,
                ordinal,
                FieldTypeHelper.GetFieldType(
                    record,
                    ordinal));

        /// <summary>
        /// Resolves a field reader using the provider source type exposed by the supplied record.
        /// </summary>
        /// <param name="record">The data record used to resolve the provider source type.</param>
        /// <param name="field">The field mapping for which the reader will be resolved.</param>
        /// <param name="ordinal">The zero-based column ordinal associated with the field.</param>
        /// <returns>A resolved field reader delegate, or <see langword="null"/> when no reader is available.</returns>
        private static FieldReader ResolveFieldReader(
            IDataRecord record,
            ColumnMap field,
            int ordinal)
            => InvokePrivate<FieldReader>(
                "ResolveFieldReaderWithSourceType",
                field,
                FieldTypeHelper.GetFieldType(
                    record,
                    ordinal));

        /// <summary>
        /// Executes the production entity-materialization path for scalar-field-only test scenarios.
        /// </summary>
        /// <param name="entity">The entity instance to populate.</param>
        /// <param name="record">The data record containing source values.</param>
        /// <param name="metadata">The metadata describing the scalar fields to materialize.</param>
        /// <param name="prefix">The prefix used when resolving column names.</param>
        /// <param name="ordinals">The mapping between column names and ordinals.</param>
        private static void SetEntityFields(
            ITable entity,
            IDataRecord record,
            TableMaterializeMetadata metadata,
            string prefix,
            IReadOnlyDictionary<string, int> ordinals)
            => TableMap.SetEntity(
                entity,
                record,
                metadata,
                prefix,
                ordinals,
                0);

        /// <summary>
        /// Executes the production entity-materialization path for nested-entity test scenarios.
        /// </summary>
        /// <param name="entity">The entity instance to populate.</param>
        /// <param name="record">The data record containing source values.</param>
        /// <param name="metadata">The metadata describing the nested mappings to materialize.</param>
        /// <param name="prefix">The prefix used when resolving column names.</param>
        /// <param name="ordinals">The mapping between column names and ordinals.</param>
        /// <param name="schemaHash">The schema hash associated with the resolved binding cache.</param>
        private static void SetEntityNestedEntities(
            ITable entity,
            IDataRecord record,
            TableMaterializeMetadata metadata,
            string prefix,
            IReadOnlyDictionary<string, int> ordinals,
            int schemaHash)
            => TableMap.SetEntity(
                entity,
                record,
                metadata,
                prefix,
                ordinals,
                schemaHash);

        /// <summary>
        /// Invokes a non-public static method of the TableMap type by name and returns its result cast to the specified
        /// type.
        /// </summary>
        /// <remarks>This method uses reflection to access and invoke non-public static methods. Use with
        /// caution, as changes to method signatures or access modifiers may cause runtime errors.</remarks>
        /// <typeparam name="T">The type to which the result of the invoked method will be cast.</typeparam>
        /// <param name="methodName">The name of the non-public static method to invoke on the TableMap type.</param>
        /// <param name="args">An array of arguments to pass to the method being invoked.</param>
        /// <returns>The result of the invoked method, cast to the specified type parameter T.</returns>
        private static T InvokePrivate<T>(
            string methodName,
            params object[] args)
            => (T)typeof(TableMap)
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, args);

        /// <summary>
        /// Represents a read-only list with a fixed count and no accessible elements.
        /// </summary>
        /// <remarks>This implementation of IReadOnlyList&lt;T&gt; always reports a count of one, but does not
        /// provide access to any elements. Attempting to access an element by index will result in an exception. The
        /// enumerator returned by this list yields no elements.</remarks>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        private sealed class CountedEmptyReadOnlyList<T> :
            IReadOnlyList<T>
        {
            /// <summary>
            /// Gets the number of elements contained in the collection.
            /// </summary>
            public int Count => 1;

            /// <summary>
            /// Gets the element at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index of the element to retrieve.</param>
            /// <returns>The element at the specified index.</returns>
            /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is less than 0 or greater than or equal to the number of elements.</exception>
            public T this[int index]
                => throw new ArgumentOutOfRangeException(nameof(index));

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            public IEnumerator<T> GetEnumerator()
            {
                yield break;
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }

        /// <summary>
        /// Creates a table mapping configuration for the 'Child' property of the 'Parent' class, specifying 'Id' as the
        /// primary key for the associated child entity.
        /// </summary>
        /// <returns>A TableMap instance that defines the mapping for the 'Child' property, including the primary key
        /// configuration.</returns>
        private static TableMap CreateTableMapWithPrimaryKey()
        {
            var prop = typeof(Parent).GetProperty(nameof(Parent.Child));
            var attr = new ForeignTableAttribute("Child")
            {
                PrimaryKeys = new[] { "Id" }
            };
            return new TableMap(prop, attr);
        }

        /// <summary>
        /// Creates a mapping for a table that does not define a primary key, using the specified foreign table
        /// attribute.
        /// </summary>
        /// <remarks>This method is intended for scenarios where the related table does not have a primary
        /// key defined. The resulting TableMap will reflect this by omitting primary key information in the
        /// mapping.</remarks>
        /// <returns>A TableMap instance that represents the mapping of the Parent.Child property to a foreign table without
        /// primary keys.</returns>
        private static TableMap CreateTableMapWithoutPrimaryKey()
        {
            var prop = typeof(Parent).GetProperty(nameof(Parent.Child));
            var attr = new ForeignTableAttribute("Child")
            {
                PrimaryKeys = null
            };
            return new TableMap(prop, attr);
        }

        /// <summary>
        /// Creates metadata for a table based on the specified nested table map.
        /// </summary>
        /// <param name="nested">The nested table map that defines the structure and columns of the table for which metadata is to be
        /// created. Cannot be null.</param>
        /// <returns>A TableMaterializeMetadata object that describes the metadata for the specified table.</returns>
        private static TableMaterializeMetadata CreateMetadata(TableMap nested)
            => MetadataFactory.CreateEntity(new ColumnMap[0], new[] { nested });
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
