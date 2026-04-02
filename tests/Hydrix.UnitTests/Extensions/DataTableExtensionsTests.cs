using Hydrix.Extensions;
using Hydrix.Schemas.Contract;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Extensions
{
    /// <summary>
    /// Contains unit tests for verifying the behavior of data table extension methods related to entity mapping.
    /// </summary>
    /// <remarks>These tests ensure that the mapping functionality correctly converts data table rows into
    /// entity instances, and handles edge cases such as empty or null tables. The class is intended for use with
    /// automated test frameworks and validates the expected outcomes of mapping operations.</remarks>
    public class DataTableExtensionsTests
    {
        /// <summary>
        /// Represents an entity used to validate data table mapping behavior.
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
            /// Gets or sets the name associated with the entity.
            /// </summary>
            [Column("Name")]
            public string Name { get; set; }
        }

        /// <summary>
        /// Verifies that the MapTo extension method correctly maps rows from a DataTable to a list of TestEntity
        /// objects.
        /// </summary>
        /// <remarks>This test ensures that each DataRow is accurately converted into a TestEntity
        /// instance with the expected property values. It validates both the count and the property mapping of the
        /// resulting entities.</remarks>
        [Fact]
        public void MapTo_ReturnsEntities()
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Rows.Add(1, "A");
            table.Rows.Add(2, "B");

            var entities = table.MapTo<TestEntity>();

            Assert.Equal(2, entities.Count);
            Assert.Equal(1, entities[0].Id);
            Assert.Equal("A", entities[0].Name);
            Assert.Equal(2, entities[1].Id);
            Assert.Equal("B", entities[1].Name);
        }

        /// <summary>
        /// Verifies that mapping an empty DataTable to a list of entities returns an empty list.
        /// </summary>
        /// <remarks>This test ensures that the MapTo method correctly handles the case where the source
        /// DataTable contains no rows, resulting in an empty collection of the target entity type.</remarks>
        [Fact]
        public void MapTo_Empty_ReturnsEmptyList()
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));

            var entities = table.MapTo<TestEntity>();

            Assert.Empty(entities);
        }

        /// <summary>
        /// Verifies that calling the MapTo method with a null DataTable returns an empty list.
        /// </summary>
        /// <remarks>This test ensures that the MapTo extension method handles null input gracefully by
        /// returning an empty collection, rather than throwing an exception.</remarks>
        [Fact]
        public void MapTo_Null_ReturnsEmptyList()
        {
            DataTable table = null;

            var entities = table.MapTo<TestEntity>();

            Assert.Empty(entities);
        }
    }
}
