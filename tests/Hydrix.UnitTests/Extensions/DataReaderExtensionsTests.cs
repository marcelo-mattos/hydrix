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
        [Fact]
        public async Task MapToAsync_Null_Throws()
        {
            IDataReader reader = null;

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await reader.MapToAsync<TestEntity>());
        }
    }
}