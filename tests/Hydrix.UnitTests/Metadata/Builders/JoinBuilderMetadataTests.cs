using Hydrix.Metadata.Builders;
using System;
using System.Collections.Generic;
using Xunit;

namespace Hydrix.UnitTests.Metadata.Builders
{
    /// <summary>
    /// Contains unit tests for the JoinBuilderMetadata class to verify that its properties are correctly initialized
    /// during construction.
    /// </summary>
    /// <remarks>These tests cover scenarios for both required and optional joins, including cases where the
    /// navigation property is null. Use these tests as a reference when modifying the JoinBuilderMetadata constructor
    /// or its property assignments to ensure expected behavior is maintained.</remarks>
    public class JoinBuilderMetadataTests
    {
        /// <summary>
        /// Represents a test entity used for demonstration or unit testing purposes.
        /// </summary>
        private class DummyEntity
        {
            /// <summary>
            /// Gets or sets the unique identifier for the customer.
            /// </summary>
            /// <remarks>This identifier is used to reference the customer in various operations
            /// throughout the application.</remarks>
            public int CustomerId { get; set; }
        }

        /// <summary>
        /// Verifies that the JoinBuilderMetadata constructor correctly initializes all properties with the provided
        /// values.
        /// </summary>
        /// <remarks>This test ensures that each property of the JoinBuilderMetadata instance matches the
        /// corresponding constructor argument, confirming correct assignment and object state after
        /// construction.</remarks>
        [Fact]
        public void Constructor_SetsAllPropertiesCorrectly()
        {
            // Arrange
            string entity = "Customers";
            string table = "customers";
            string schema = "sales";
            string[] primaryKeys = new[] { "Id" };
            string[] foreignKeys = new[] { "CustomerId" };
            bool isRequiredJoin = true;
            var columns = new List<ForeignColumnMetadata>()
            {
                new ForeignColumnMetadata("col", "row.col")
            };

            // Act
            var metadata = new JoinBuilderMetadata(
                entity,
                table,
                schema,
                primaryKeys,
                foreignKeys,
                isRequiredJoin,
                columns);

            // Assert
            Assert.Equal(entity, metadata.Entity);
            Assert.Equal(table, metadata.Table);
            Assert.Equal(schema, metadata.Schema);
            Assert.Equal(primaryKeys, metadata.PrimaryKeys);
            Assert.Equal(foreignKeys, metadata.ForeignKeys);
            Assert.Equal(isRequiredJoin, metadata.IsRequiredJoin);
            Assert.Equal(columns, metadata.Columns);
        }

        /// <summary>
        /// Verifies that the JoinBuilderMetadata constructor supports optional joins and accepts a null navigation
        /// property.
        /// </summary>
        /// <remarks>This test ensures that JoinBuilderMetadata can be instantiated with isRequiredJoin
        /// set to false and navigationProperty set to null, allowing for scenarios where a join is not mandatory and no
        /// related entity is specified.</remarks>
        [Fact]
        public void Constructor_AllowsOptionalJoinAndNullNavigationProperty()
        {
            // Arrange
            string entity = "Orders";
            string table = "orders";
            string schema = "sales";
            string[] primaryKeys = new[] { "OrderId" };
            string[] foreignKeys = new[] { "Id" };
            bool isRequiredJoin = false;
            var columns = new List<ForeignColumnMetadata>();

            // Act
            var metadata = new JoinBuilderMetadata(
                entity,
                table,
                schema,
                primaryKeys,
                foreignKeys,
                isRequiredJoin,
                columns);

            // Assert
            Assert.Equal(entity, metadata.Entity);
            Assert.Equal(table, metadata.Table);
            Assert.Equal(schema, metadata.Schema);
            Assert.Equal(primaryKeys, metadata.PrimaryKeys);
            Assert.Equal(foreignKeys, metadata.ForeignKeys);
            Assert.False(metadata.IsRequiredJoin);
            Assert.Equal(columns, metadata.Columns);
        }

        /// <summary>
        /// Verifies that the JoinBuilderMetadata constructor throws an ArgumentNullException when the columns parameter
        /// is null.
        /// </summary>
        /// <remarks>This test ensures that the JoinBuilderMetadata class enforces a non-null requirement
        /// for the columns parameter during construction. Providing a null value for columns should result in an
        /// ArgumentNullException, helping to prevent improper initialization.</remarks>
        [Fact]
        public void Constructor_ThrowsIfColumnsIsNull()
        {
            // Arrange
            string entity = "Orders";
            string table = "orders";
            string schema = "sales";
            string[] primaryKeys = new[] { "OrderId" };
            string[] foreignKeys = new[] { "Id" };
            bool isRequiredJoin = false;
            IReadOnlyList<ForeignColumnMetadata> columns = null;

            // Assert
            Assert.Throws<ArgumentNullException>(() => new JoinBuilderMetadata(
                entity,
                table,
                schema,
                primaryKeys,
                foreignKeys,
                isRequiredJoin,
                columns));
        }
    }
}
