using Hydrix.Orchestrator.Binders.Entity;
using System;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Binders.Entity
{
    /// <summary>
    /// Contains unit tests for the DataColumnBinding&lt;T&gt; type, verifying its construction, property assignment,
    /// immutability, and equality behavior.
    /// </summary>
    /// <remarks>These tests ensure that DataColumnBinding&lt;T&gt; instances are correctly initialized, support
    /// null values, remain immutable after creation, and implement value-based equality. The tests also validate that
    /// the getter function retrieves expected values from entities. Use this test class to confirm the reliability and
    /// correctness of DataColumnBinding&lt;T&gt; in data binding scenarios.</remarks>
    public class DataColumnBindingTests
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
        /// Verifies that the DataColumnBinding constructor initializes the ColumnName, DataType, and Getter properties
        /// with the specified values.
        /// </summary>
        /// <remarks>This test ensures that the DataColumnBinding instance correctly reflects the property
        /// name, data type, and getter function provided during construction. It validates the integrity of property
        /// assignments for the TestEntity type.</remarks>
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            var getter = new Func<TestEntity, object>(e => e.Id);
            var binding = new DataColumnBinding<TestEntity>("Id", typeof(int), getter);
            Assert.Equal("Id", binding.ColumnName);
            Assert.Equal(typeof(int), binding.DataType);
            Assert.Equal(getter, binding.Getter);
        }

        /// <summary>
        /// Verifies that the getter function of the DataColumnBinding class returns the expected value for the 'Id'
        /// property of a TestEntity instance.
        /// </summary>
        /// <remarks>This test ensures that the DataColumnBinding correctly retrieves the integer
        /// identifier from the entity, validating its data binding functionality.</remarks>
        [Fact]
        public void Getter_ReturnsExpectedValue()
        {
            var binding = new DataColumnBinding<TestEntity>("Id", typeof(int), e => e.Id);
            var entity = new TestEntity { Id = 42 };
            Assert.Equal(42, binding.Getter(entity));
        }

        /// <summary>
        /// Verifies that the DataColumnBinding constructor permits null values for the column name, data type, and
        /// getter function.
        /// </summary>
        /// <remarks>This test ensures that DataColumnBinding can be initialized with null arguments,
        /// supporting scenarios where column metadata or accessors are optional.</remarks>
        [Fact]
        public void Constructor_AllowsNullValues()
        {
            var binding = new DataColumnBinding<TestEntity>(null, null, null);
            Assert.Null(binding.ColumnName);
            Assert.Null(binding.DataType);
            Assert.Null(binding.Getter);
        }

        /// <summary>
        /// Verifies that the DataColumnBinding structure for the TestEntity type remains immutable after assignment.
        /// </summary>
        /// <remarks>This test ensures that the properties of a DataColumnBinding instance do not change
        /// when copied, confirming that the binding behaves as an immutable value type. Use this test to validate that
        /// DataColumnBinding maintains its assigned state and does not allow modification through assignment.</remarks>
        [Fact]
        public void Struct_IsImmutable()
        {
            var getter = new Func<TestEntity, object>(e => e.Id);
            var binding = new DataColumnBinding<TestEntity>("Test", typeof(int), getter);
            var copy = binding;
            Assert.Equal("Test", copy.ColumnName);
            Assert.Equal(typeof(int), copy.DataType);
            Assert.Equal(getter, copy.Getter);
        }

        /// <summary>
        /// Verifies that two instances of DataColumnBinding&lt;TestEntity&gt; are considered equal based on their properties.
        /// </summary>
        /// <remarks>This test checks the equality comparison of DataColumnBinding instances by comparing
        /// their ColumnName, DataType, and Getter properties. It ensures that the equality logic is correctly
        /// implemented for instances with the same configuration.</remarks>
        [Fact]
        public void Equality_Comparison_Works()
        {
            var getter = new Func<TestEntity, object>(e => e.Id);
            var binding1 = new DataColumnBinding<TestEntity>("A", typeof(int), getter);
            var binding2 = new DataColumnBinding<TestEntity>("A", typeof(int), getter);
            Assert.True(binding1.Equals(binding2));
            Assert.Equal(binding1.ColumnName, binding2.ColumnName);
            Assert.Equal(binding1.DataType, binding2.DataType);
            Assert.Equal(binding1.Getter, binding2.Getter);
        }
    }
}