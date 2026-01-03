using Hydrix.Orchestrator.Materializers;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the ConvertDataReaderToEntities method returns a collection of entities populated from the
        /// provided IDataReader.
        /// </summary>
        /// <remarks>This test ensures that the method correctly materializes multiple entities from a
        /// data reader, mapping the expected fields to the entity properties.</remarks>
        [Fact]
        public void ConvertDataReaderToEntities_ReturnsEntities()
        {
            var mockReader = new Mock<IDataReader>();
            var callCount = 0;
            mockReader.Setup(r => r.Read()).Returns(() => callCount++ < 2);
            mockReader.Setup(r => r.FieldCount).Returns(2);
            mockReader.Setup(r => r.GetOrdinal("Id")).Returns(0);
            mockReader.Setup(r => r.IsDBNull(0)).Returns(true);
            mockReader.Setup(r => r.GetValue(0)).Returns(DBNull.Value);
            mockReader.Setup(r => r.GetName(0)).Returns("Id");
            mockReader.Setup(r => r.GetOrdinal("Name")).Returns(1);
            mockReader.Setup(r => r.GetValue(1)).Returns("Test");
            mockReader.Setup(r => r.GetName(1)).Returns("Name");

            var entities = SqlMaterializer.ConvertDataReaderToEntities<TestEntity>(mockReader.Object);

            Assert.Equal(2, entities.Count);
            Assert.All(entities, e =>
            {
                Assert.Equal(0, e.Id);
                Assert.Equal("Test", e.Name);
            });
        }

        /// <summary>
        /// Verifies that the ConvertDataReaderToEntities method returns an empty list when the provided IDataReader
        /// contains no rows.
        /// </summary>
        /// <remarks>This test ensures that the materializer does not return null or throw an exception
        /// when the data reader is empty, but instead returns an empty collection as expected.</remarks>
        [Fact]
        public void ConvertDataReaderToEntities_Empty_ReturnsEmptyList()
        {
            var mockReader = new Mock<IDataReader>();
            mockReader.Setup(r => r.Read()).Returns(false);

            var entities = SqlMaterializer.ConvertDataReaderToEntities<TestEntity>(mockReader.Object);

            Assert.Empty(entities);
        }

        /// <summary>
        /// Verifies that ConvertDataReaderToEntities&lt;T&gt; throws an ArgumentNullException when passed a null data reader.
        /// </summary>
        /// <remarks>This test ensures that the method enforces its null argument precondition and
        /// provides appropriate exception feedback to callers.</remarks>
        [Fact]
        public void ConvertDataReaderToEntities_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                SqlMaterializer.ConvertDataReaderToEntities<TestEntity>(null));
        }

        /// <summary>
        /// Verifies that the ConvertDataTableToEntity method correctly converts a DataTable into a list of entities
        /// with the expected property values.
        /// </summary>
        /// <remarks>This test ensures that each row in the DataTable is mapped to an entity of type
        /// TestEntity and that the entity properties match the corresponding column values. It checks both the count
        /// and the property values of the resulting entities.</remarks>
        [Fact]
        public void ConvertDataTableToEntity_ReturnsEntities()
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Rows.Add(1, "A");
            table.Rows.Add(2, "B");

            var entities = SqlMaterializer.ConvertDataTableToEntity<TestEntity>(table);

            Assert.Equal(2, entities.Count);
            Assert.Equal(1, entities[0].Id);
            Assert.Equal("A", entities[0].Name);
            Assert.Equal(2, entities[1].Id);
            Assert.Equal("B", entities[1].Name);
        }

        /// <summary>
        /// Verifies that ConvertDataTableToEntity returns an empty list when the input DataTable contains no rows.
        /// </summary>
        /// <remarks>This test ensures that the method correctly handles empty DataTable inputs by not
        /// returning any entities. It helps confirm that no default or placeholder entities are created when the source
        /// data is empty.</remarks>
        [Fact]
        public void ConvertDataTableToEntity_Empty_ReturnsEmptyList()
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));

            var entities = SqlMaterializer.ConvertDataTableToEntity<TestEntity>(table);

            Assert.Empty(entities);
        }

        /// <summary>
        /// Verifies that ConvertDataTableToEntity&lt;T&gt; returns an empty list when passed a null DataTable.
        /// </summary>
        /// <remarks>This test ensures that the method handles null input gracefully by returning an empty
        /// collection, rather than throwing an exception or returning null.</remarks>
        [Fact]
        public void ConvertDataTableToEntity_Null_ReturnsEmptyList()
        {
            var entities = SqlMaterializer.ConvertDataTableToEntity<TestEntity>(null);

            Assert.Empty(entities);
        }

        /// <summary>
        /// Verifies that the ConvertEntityToDataTable method returns a DataTable containing the expected rows and
        /// columns based on the provided entity list.
        /// </summary>
        /// <remarks>This test ensures that each entity in the input list is correctly represented as a
        /// row in the resulting DataTable, and that the DataTable columns match the entity properties.</remarks>
        [Fact]
        public void ConvertEntityToDataTable_ReturnsDataTable()
        {
            var entities = new List<TestEntity>
            {
                new TestEntity { Id = 1, Name = "A" },
                new TestEntity { Id = 2, Name = "B" }
            };

            var table = SqlMaterializer.ConvertEntityToDataTable(entities);

            Assert.Equal(2, table.Rows.Count);
            Assert.Equal(2, table.Columns.Count);
            Assert.Equal(1, table.Rows[0]["Id"]);
            Assert.Equal("A", table.Rows[0]["Name"]);
            Assert.Equal(2, table.Rows[1]["Id"]);
            Assert.Equal("B", table.Rows[1]["Name"]);
        }

        /// <summary>
        /// Verifies that converting an empty collection of TestEntity objects to a DataTable returns a table with the
        /// correct schema and no rows.
        /// </summary>
        /// <remarks>This test ensures that the DataTable produced by
        /// SqlMaterializer.ConvertEntityToDataTable contains the expected columns but does not include any data rows
        /// when the input collection is empty.</remarks>
        [Fact]
        public void ConvertEntityToDataTable_Empty_ReturnsSchemaOnly()
        {
            var table = SqlMaterializer.ConvertEntityToDataTable(new List<TestEntity>());

            Assert.Empty(table.Rows);
            Assert.Equal(2, table.Columns.Count);
        }

        /// <summary>
        /// Verifies that calling ConvertEntityToDataTable&lt;T&gt; with a null entity returns a DataTable containing only the
        /// schema, with no data rows.
        /// </summary>
        /// <remarks>This test ensures that when a null entity is provided to ConvertEntityToDataTable&lt;T&gt;,
        /// the resulting DataTable has the correct columns defined for the entity type but contains no rows. This
        /// behavior is important for scenarios where the schema is needed without any data.</remarks>
        [Fact]
        public void ConvertEntityToDataTable_Null_ReturnsSchemaOnly()
        {
            var table = SqlMaterializer.ConvertEntityToDataTable<TestEntity>(null);

            Assert.Empty(table.Rows);
            Assert.Equal(2, table.Columns.Count);
        }

        /// <summary>
        /// Verifies that the ValidateEntityRequest method returns true when invoked with a valid TestEntity type.
        /// </summary>
        /// <remarks>This unit test ensures that the ValidateEntityRequest method, when called for
        /// TestEntity, correctly identifies the entity as valid. The test uses reflection to access and invoke the
        /// non-public static method.</remarks>
        [Fact]
        public void ValidateEntityRequest_ValidEntity_ReturnsTrue()
        {
            var method = typeof(SqlMaterializer)
                .GetMethod("ValidateEntityRequest", BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(typeof(TestEntity));

            var result = (bool)method.Invoke(null, null);

            Assert.True(result);
        }

        /// <summary>
        /// Verifies that the ValidateEntityRequest method throws a MissingMemberException when the entity type lacks
        /// the required attribute.
        /// </summary>
        /// <remarks>This test ensures that the internal ValidateEntityRequest method enforces attribute
        /// requirements on entity types. It is intended to validate error handling for missing attributes in entity
        /// definitions.</remarks>
        [Fact]
        public void ValidateEntityRequest_MissingAttribute_Throws()
        {
            var method = typeof(SqlMaterializer)
                .GetMethod("ValidateEntityRequest", BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(typeof(NoAttributeEntity));

            var ex = Assert.Throws<TargetInvocationException>(() => method.Invoke(null, null));
            Assert.IsType<MissingMemberException>(ex.InnerException);
        }

        /// <summary>
        /// Verifies that the ValidateEntityRequest method returns false when invoked with an entity type that has no
        /// fields.
        /// </summary>
        /// <remarks>This test ensures that the ValidateEntityRequest method correctly identifies entity
        /// types lacking fields and returns false as expected. It uses reflection to access and invoke the non-public
        /// static method.</remarks>
        [Fact]
        public void ValidateEntityRequest_NoField_ReturnsFalse()
        {
            var method = typeof(SqlMaterializer)
                .GetMethod("ValidateEntityRequest", BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(typeof(NoFieldEntity));

            var result = (bool)method.Invoke(null, null);

            Assert.False(result);
        }
    }
}