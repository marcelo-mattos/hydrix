using Hydrix.Extensions;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace Hydrix.UnitTests.Extensions
{
    /// <summary>
    /// Contains unit tests for verifying the behavior of extension methods for converting entity lists to DataTable
    /// instances.
    /// </summary>
    /// <remarks>These tests ensure that the conversion handles populated lists, empty lists, null property
    /// values, and null lists correctly, matching the expected DataTable schema and data. The tests are intended to
    /// validate the public contract of the extension methods and do not cover internal implementation
    /// details.</remarks>
    public class ListExtensionsTests
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
        /// Verifies that the MapTo extension method returns a DataTable with rows and columns
        /// matching the provided entity list.
        /// </summary>
        [Fact]
        public void MapTo_ReturnsDataTable()
        {
            var entities = new List<TestEntity>
            {
                new TestEntity { Id = 1, Name = "A" },
                new TestEntity { Id = 2, Name = "B" }
            };

            var table = entities.MapTo();

            Assert.Equal(2, table.Rows.Count);
            Assert.Equal(2, table.Columns.Count);
            Assert.Equal(1, table.Rows[0]["Id"]);
            Assert.Equal("A", table.Rows[0]["Name"]);
            Assert.Equal(2, table.Rows[1]["Id"]);
            Assert.Equal("B", table.Rows[1]["Name"]);
        }

        /// <summary>
        /// Verifies that converting an empty entity list to a DataTable returns only the schema without rows.
        /// </summary>
        [Fact]
        public void MapTo_Empty_ReturnsSchemaOnly()
        {
            var table = new List<TestEntity>().MapTo();

            Assert.Empty(table.Rows);
            Assert.Equal(2, table.Columns.Count);
        }

        /// <summary>
        /// Verifies that null property values are converted to <see cref="DBNull.Value"/> when converting to a DataTable.
        /// </summary>
        [Fact]
        public void MapTo_NullProperty_UsesDBNullValue()
        {
            var entities = new List<TestEntity>
            {
                new TestEntity { Id = 10, Name = null }
            };

            var table = entities.MapTo();

            Assert.Single(table.Rows);
            Assert.Equal(10, table.Rows[0]["Id"]);
            Assert.Equal(DBNull.Value, table.Rows[0]["Name"]);
        }

        /// <summary>
        /// Verifies that converting a null entity list to a DataTable returns only the schema without rows.
        /// </summary>
        [Fact]
        public void MapTo_Null_ReturnsSchemaOnly()
        {
            IList<TestEntity> entities = null;

            var table = entities.MapTo();

            Assert.Empty(table.Rows);
            Assert.Equal(2, table.Columns.Count);
        }
    }
}
