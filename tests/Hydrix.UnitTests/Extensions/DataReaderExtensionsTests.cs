using Hydrix.Extensions;
using Hydrix.Schemas.Contract;
using Moq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Hydrix.UnitTests.Extensions
{
    /// <summary>
    /// Contains unit tests for verifying the behavior of data reader extension methods that map data from an
    /// IDataReader to entity objects.
    /// </summary>
    /// <remarks>These tests validate various scenarios for the MapTo extension method, including correct
    /// mapping of data, handling of limits, empty readers, null column names, and null reader arguments. The tests use
    /// mocked IDataReader instances to simulate different data and edge cases.</remarks>
    public class DataReaderExtensionsTests
    {
        /// <summary>
        /// Represents an entity used to validate data reader mapping behavior.
        /// </summary>
        [Table("Test")]
        private class TestEntity : ITable
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            [Column("Id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name for the entity.
            /// </summary>
            [Column("Name")]
            public string Name { get; set; }
        }

        /// <summary>
        /// Verifies that the MapTo method correctly maps data from an IDataReader to a collection of TestEntity
        /// instances.
        /// </summary>
        /// <remarks>This test ensures that the MapTo extension method produces the expected number of
        /// entities and that each entity's properties are set according to the data provided by the mocked
        /// IDataReader.</remarks>
        [Fact]
        public void MapTo_ReturnsEntities()
        {
            var mockReader = new Mock<IDataReader>();
            var callCount = 0;

            mockReader.Setup(r => r.Read()).Returns(() => callCount++ < 2);
            mockReader.Setup(r => r.FieldCount).Returns(2);
            mockReader.Setup(r => r.GetOrdinal("Id")).Returns(0);
            mockReader.Setup(r => r.GetInt32(0)).Returns(0);
            mockReader.Setup(r => r.IsDBNull(0)).Returns(true);
            mockReader.Setup(r => r.GetValue(0)).Returns(DBNull.Value);
            mockReader.Setup(r => r.GetName(0)).Returns("Id");
            mockReader.Setup(r => r.GetOrdinal("Name")).Returns(1);
            mockReader.Setup(r => r.GetString(1)).Returns("Test");
            mockReader.Setup(r => r.GetValue(1)).Returns("Test");
            mockReader.Setup(r => r.GetName(1)).Returns("Name");

            var entities = mockReader.Object.MapTo<TestEntity>();

            Assert.Equal(2, entities.Count);
            Assert.All(entities, e =>
            {
                Assert.Equal(0, e.Id);
                Assert.Equal("Test", e.Name);
            });
        }

        /// <summary>
        /// Verifies that schema inspection is deferred until the reader is positioned on the first row.
        /// </summary>
        [Fact]
        public void MapTo_ReaderThatExposesSchemaOnlyAfterRead_MapsSuccessfully()
        {
            var mockReader = new Mock<IDataReader>();
            var currentRowAvailable = false;
            var readCount = 0;

            mockReader.Setup(r => r.Read()).Returns(() =>
            {
                if (readCount++ > 0)
                    return false;

                currentRowAvailable = true;
                return true;
            });

            mockReader.Setup(r => r.FieldCount).Returns(() =>
                currentRowAvailable
                    ? 2
                    : throw new InvalidOperationException("Schema is unavailable before the first row is read."));

            mockReader.Setup(r => r.GetName(0)).Returns("Id");
            mockReader.Setup(r => r.GetName(1)).Returns("Name");
            mockReader.Setup(r => r.IsDBNull(0)).Returns(false);
            mockReader.Setup(r => r.IsDBNull(1)).Returns(false);
            mockReader.Setup(r => r.GetInt32(0)).Returns(7);
            mockReader.Setup(r => r.GetString(1)).Returns("Deferred");
            mockReader.Setup(r => r.GetValue(0)).Returns(7);
            mockReader.Setup(r => r.GetValue(1)).Returns("Deferred");

            var entities = mockReader.Object.MapTo<TestEntity>();

            var entity = Assert.Single(entities);
            Assert.Equal(7, entity.Id);
            Assert.Equal("Deferred", entity.Name);
        }

        /// <summary>
        /// Verifies that the MapTo method returns only the specified number of entities when a positive limit is
        /// provided.
        /// </summary>
        /// <remarks>This test ensures that when the limit parameter is set to a positive value, the MapTo
        /// method does not return more entities than the specified limit. It uses a mocked IDataReader to simulate data
        /// retrieval.</remarks>
        [Fact]
        public void MapTo_WithPositiveLimit_ReturnsLimitedEntities()
        {
            var mockReader = new Mock<IDataReader>();
            var callCount = 0;

            mockReader.Setup(r => r.Read()).Returns(() => callCount++ < 5);
            mockReader.Setup(r => r.FieldCount).Returns(2);
            mockReader.Setup(r => r.GetName(0)).Returns("Id");
            mockReader.Setup(r => r.GetName(1)).Returns("Name");
            mockReader.Setup(r => r.GetValue(0)).Returns(7);
            mockReader.Setup(r => r.GetValue(1)).Returns("Limited");
            mockReader.Setup(r => r.GetInt32(0)).Returns(7);
            mockReader.Setup(r => r.GetString(1)).Returns("Limited");
            mockReader.Setup(r => r.IsDBNull(0)).Returns(false);
            mockReader.Setup(r => r.IsDBNull(1)).Returns(false);

            var entities = mockReader.Object.MapTo<TestEntity>(limit: 1);

            Assert.Single(entities);
            Assert.Equal(7, entities[0].Id);
            Assert.Equal("Limited", entities[0].Name);
        }

        /// <summary>
        /// Verifies that mapping an empty data reader returns an empty list of entities.
        /// </summary>
        /// <remarks>This test ensures that when the data reader contains no rows, the MapTo method
        /// produces an empty collection, confirming correct handling of empty data sources.</remarks>
        [Fact]
        public void MapTo_Empty_ReturnsEmptyList()
        {
            var mockReader = new Mock<IDataReader>();
            mockReader.Setup(r => r.Read()).Returns(false);

            var entities = mockReader.Object.MapTo<TestEntity>();

            Assert.Empty(entities);
        }

        /// <summary>
        /// Verifies that the mapping operation uses an empty string as a fallback when a column name is null during
        /// entity mapping.
        /// </summary>
        /// <remarks>This test ensures that the mapping logic correctly handles cases where the data
        /// reader returns a null column name, preventing potential errors or unexpected behavior when mapping to entity
        /// properties.</remarks>
        [Fact]
        public void MapTo_NullColumnName_UsesEmptyStringFallback()
        {
            var mockReader = new Mock<IDataReader>();
            mockReader.Setup(r => r.Read()).Returns(false);
            mockReader.Setup(r => r.FieldCount).Returns(1);
            mockReader.Setup(r => r.GetName(0)).Returns((string)null);

            var entities = mockReader.Object.MapTo<TestEntity>();

            Assert.Empty(entities);
        }

        /// <summary>
        /// Verifies that the MapTo extension method throws an ArgumentNullException when called on a null IDataReader
        /// instance.
        /// </summary>
        /// <remarks>This test ensures that the MapTo method enforces its null argument precondition and
        /// provides appropriate exception feedback to callers.</remarks>
        [Fact]
        public void MapTo_Null_Throws()
        {
            IDataReader reader = null;

            Assert.Throws<ArgumentNullException>(() =>
                reader.MapTo<TestEntity>());
        }

        /// <summary>
        /// Verifies that the asynchronous mapping method correctly maps all rows from a DbDataReader-backed
        /// IDataReader into entities.
        /// </summary>
        /// <remarks>This test uses a DataTable reader (which is a DbDataReader implementation) to validate
        /// the asynchronous execution path and property materialization.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task MapToAsync_ReturnsEntities()
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Rows.Add(1, "Alpha");
            table.Rows.Add(2, "Beta");

            using var reader = table.CreateDataReader();

            var entities = await reader.MapToAsync<TestEntity>();

            Assert.Equal(2, entities.Count);
            Assert.Equal(1, entities[0].Id);
            Assert.Equal("Alpha", entities[0].Name);
            Assert.Equal(2, entities[1].Id);
            Assert.Equal("Beta", entities[1].Name);
        }

        /// <summary>
        /// Verifies that the asynchronous mapping operation is canceled when a canceled CancellationToken is provided.
        /// </summary>
        /// <remarks>This test ensures that the MapToAsync method throws a TaskCanceledException when the
        /// provided CancellationToken is already canceled, confirming correct cancellation behavior.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task MapToAsync_CancellationToken_CancelsOperation()
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Rows.Add(1, "Alpha");

            using var reader = table.CreateDataReader();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await reader.MapToAsync<TestEntity>(cancellationToken: cts.Token));
        }

        /// <summary>
        /// Verifies that the asynchronous mapping method respects a positive row limit and returns only the requested
        /// number of entities.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task MapToAsync_WithPositiveLimit_ReturnsLimitedEntities()
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Rows.Add(10, "First");
            table.Rows.Add(20, "Second");

            using var reader = table.CreateDataReader();

            var entities = await reader.MapToAsync<TestEntity>(limit: 1);

            Assert.Single(entities);
            Assert.Equal(10, entities[0].Id);
            Assert.Equal("First", entities[0].Name);
        }

        /// <summary>
        /// Verifies that the asynchronous mapping method throws when the provided IDataReader is not a DbDataReader.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task MapToAsync_WithNonDbDataReader_Throws()
        {
            var mockReader = new Mock<IDataReader>();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await mockReader.Object.MapToAsync<TestEntity>());
        }

        /// <summary>
        /// Verifies that the asynchronous mapping method throws an ArgumentNullException when called with a null
        /// reader.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task MapToAsync_Null_Throws()
        {
            IDataReader reader = null;

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await reader.MapToAsync<TestEntity>());
        }

        /// <summary>
        /// Verifies that mapping converts compatible provider CLR types into the destination property type.
        /// </summary>
        [Fact]
        public void MapTo_ConvertsCompatibleProviderTypes()
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(long));
            table.Columns.Add("Name", typeof(string));
            table.Rows.Add(42L, "Converted");

            using var reader = table.CreateDataReader();

            var entities = reader.MapTo<TestEntity>();

            var entity = Assert.Single(entities);
            Assert.Equal(42, entity.Id);
            Assert.Equal("Converted", entity.Name);
        }

        /// <summary>
        /// Verifies that mapping handles null column names on a materialized row by applying an empty-string fallback
        /// during schema processing.
        /// </summary>
        [Fact]
        public void MapTo_WhenNullNamedColumnExistsOnMaterializedRow_UsesEmptyStringFallback()
        {
            var reader = CreateReader();
            reader.Setup(r => r.FieldCount).Returns(3);
            reader.Setup(r => r.GetName(0)).Returns((string)null);
            reader.Setup(r => r.GetName(1)).Returns("Id");
            reader.Setup(r => r.GetName(2)).Returns("Name");
            reader.Setup(r => r.GetFieldType(0)).Returns(typeof(string));
            reader.Setup(r => r.GetFieldType(1)).Returns(typeof(int));
            reader.Setup(r => r.GetFieldType(2)).Returns(typeof(string));
            reader.Setup(r => r.IsDBNull(1)).Returns(false);
            reader.Setup(r => r.IsDBNull(2)).Returns(false);
            reader.Setup(r => r.GetInt32(1)).Returns(9);
            reader.Setup(r => r.GetString(2)).Returns("NullName");

            var entities = reader.Object.MapTo<TestEntity>();

            var entity = Assert.Single(entities);
            Assert.Equal(9, entity.Id);
            Assert.Equal("NullName", entity.Name);
        }

        /// <summary>
        /// Verifies that the mapping operation completes successfully even when the data reader's GetFieldType method
        /// throws an InvalidOperationException.
        /// </summary>
        /// <remarks>This test ensures that the MapTo method can handle scenarios where schema information
        /// is unavailable and still correctly maps data to the target entity type.</remarks>
        [Fact]
        public void MapTo_WhenGetFieldTypeThrowsInvalidOperationException_StillMaps()
        {
            var reader = CreateReader();
            reader.Setup(r => r.FieldCount).Returns(2);
            reader.Setup(r => r.GetName(0)).Returns("Id");
            reader.Setup(r => r.GetName(1)).Returns("Name");
            reader.Setup(r => r.GetFieldType(It.IsAny<int>())).Throws(new InvalidOperationException("schema unavailable"));
            reader.Setup(r => r.IsDBNull(0)).Returns(false);
            reader.Setup(r => r.IsDBNull(1)).Returns(false);
            reader.Setup(r => r.GetInt32(0)).Returns(7);
            reader.Setup(r => r.GetString(1)).Returns("InvalidOperation");

            var entities = reader.Object.MapTo<TestEntity>();

            var entity = Assert.Single(entities);
            Assert.Equal(7, entity.Id);
            Assert.Equal("InvalidOperation", entity.Name);
        }

        /// <summary>
        /// Verifies that the mapping operation completes successfully even when the data reader's GetFieldType method
        /// throws a NotSupportedException.
        /// </summary>
        /// <remarks>This test ensures that the MapTo method can handle scenarios where schema information
        /// is unavailable from the data reader, and still correctly maps data to the target entity type.</remarks>
        [Fact]
        public void MapTo_WhenGetFieldTypeThrowsNotSupportedException_StillMaps()
        {
            var reader = CreateReader();
            reader.Setup(r => r.FieldCount).Returns(2);
            reader.Setup(r => r.GetName(0)).Returns("Id");
            reader.Setup(r => r.GetName(1)).Returns("Name");
            reader.Setup(r => r.GetFieldType(It.IsAny<int>())).Throws(new NotSupportedException("schema unavailable"));
            reader.Setup(r => r.IsDBNull(0)).Returns(false);
            reader.Setup(r => r.IsDBNull(1)).Returns(false);
            reader.Setup(r => r.GetInt32(0)).Returns(11);
            reader.Setup(r => r.GetString(1)).Returns("NotSupported");

            var entities = reader.Object.MapTo<TestEntity>();

            var entity = Assert.Single(entities);
            Assert.Equal(11, entity.Id);
            Assert.Equal("NotSupported", entity.Name);
        }

        /// <summary>
        /// Creates a configured <see cref="Mock{IDataReader}"/> that returns a single readable row.
        /// </summary>
        /// <returns>A mock data reader configured for one successful <see cref="IDataReader.Read"/> call.</returns>
        private static Mock<IDataReader> CreateReader()
        {
            var readCount = 0;
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(() => readCount++ == 0);
            return reader;
        }
    }
}
