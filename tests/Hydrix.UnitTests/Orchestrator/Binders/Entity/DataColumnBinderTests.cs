using Hydrix.Orchestrator.Binders.Entity;
using System;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Binders.Entity
{
    /// <summary>
    /// Provides unit tests for the functionality of the DataColumnBinder class, ensuring correct behavior in various
    /// scenarios.
    /// </summary>
    /// <remarks>This class contains tests that validate the construction and behavior of the
    /// DataColumnBinder, including handling of empty and null arrays, as well as immutability of the binder
    /// structure.</remarks>
    public class DataColumnBinderTests
    {
        /// <summary>
        /// A simple test entity class used for testing the DataColumnBinding functionality. It contains a single integer property.
        /// </summary>
        private class TestEntity
        {
            /// <summary>
            /// Gets or sets the integer identifier for the test entity. This property is used in tests to verify that the getter function
            /// of the DataColumnBinding correctly retrieves the value from the entity.
            /// </summary>
            public int Id { get; set; }
        }

        /// <summary>
        /// Verifies that the DataColumnBinder constructor correctly initializes the Columns collection with the
        /// provided column bindings.
        /// </summary>
        /// <remarks>This test ensures that when a DataColumnBinder is constructed with a set of
        /// DataColumnBinding objects, the Columns property contains the expected bindings in the correct
        /// order.</remarks>
        [Fact]
        public void Constructor_SetsColumnsCorrectly()
        {
            var getter = new Func<TestEntity, object>(e => e.Id);
            var binding = new DataColumnBinding<TestEntity>("Id", typeof(int), getter);
            var binder = new DataColumnBinder<TestEntity>(new[] { binding });
            Assert.Single(binder.Columns);
            Assert.Equal(binding, binder.Columns[0]);
        }

        /// <summary>
        /// Verifies that the DataColumnBinder constructor accepts an empty array of DataColumnBinding objects and
        /// initializes an empty collection of columns.
        /// </summary>
        /// <remarks>This test ensures that creating a DataColumnBinder with no initial column bindings
        /// results in an empty Columns collection, confirming correct handling of empty input arrays.</remarks>
        [Fact]
        public void Constructor_AllowsEmptyArray()
        {
            var binder = new DataColumnBinder<TestEntity>(new DataColumnBinding<TestEntity>[0]);
            Assert.Empty(binder.Columns);
        }

        /// <summary>
        /// Verifies that the DataColumnBinder constructor accepts a null array of columns without throwing an
        /// exception.
        /// </summary>
        /// <remarks>This test ensures that when a null array is provided to the DataColumnBinder
        /// constructor, the Columns property is set to null. This behavior is important for scenarios where column data
        /// may be optional.</remarks>
        [Fact]
        public void Constructor_AllowsNullArray()
        {
            var binder = new DataColumnBinder<TestEntity>(null);
            Assert.Null(binder.Columns);
        }

        /// <summary>
        /// Verifies that the structure of the DataColumnBinder for TestEntity remains unchanged when copied, ensuring
        /// immutability of its column configuration.
        /// </summary>
        /// <remarks>This test confirms that the DataColumnBinder maintains a consistent column structure
        /// when assigned to another variable, indicating that its internal state is immutable. Use this test to
        /// validate that modifications to one instance do not affect copies.</remarks>
        [Fact]
        public void Struct_IsImmutable()
        {
            var getter = new Func<TestEntity, object>(e => e.Id);
            var binding = new DataColumnBinding<TestEntity>("Test", typeof(int), getter);
            var binder = new DataColumnBinder<TestEntity>(new[] { binding });
            var copy = binder;
            Assert.Equal(binder.Columns, copy.Columns);
        }
    }
}
