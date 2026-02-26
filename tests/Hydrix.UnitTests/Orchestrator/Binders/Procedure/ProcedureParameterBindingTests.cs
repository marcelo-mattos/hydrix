using Hydrix.Orchestrator.Binders.Procedure;
using System;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Binders.Procedure
{
    /// <summary>
    /// Contains unit tests for the ProcedureParameterBinding struct, verifying its construction, property assignments,
    /// immutability, and behavior when handling various input values.
    /// </summary>
    /// <remarks>These tests ensure that ProcedureParameterBinding correctly sets its properties, supports
    /// null values for certain parameters, accepts any DbType, and remains immutable. The tests also validate the
    /// functionality of the Getter delegate and its expected output.</remarks>
    public class ProcedureParameterBindingTests
    {
        /// <summary>
        /// A simple class used for testing the Getter function of ProcedureParameterBinding, containing a single integer property.
        /// </summary>
        private class TestObj
        {
            /// <summary>
            /// Integer property used to verify that the Getter function correctly retrieves values from an object instance.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Verifies that the ProcedureParameterBinding constructor correctly initializes all properties with the
        /// provided arguments.
        /// </summary>
        /// <remarks>This unit test ensures that the Name, Direction, DbType, and Getter properties of a
        /// ProcedureParameterBinding instance are set as expected when the constructor is called. Use this test to
        /// confirm that property assignment logic in the constructor remains consistent after code changes.</remarks>
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            var getter = new Func<object, object>(o => o);
            var binding = new ProcedureParameterBinding("Id", ParameterDirection.Input, (int)DbType.Int32, getter);
            Assert.Equal("Id", binding.Name);
            Assert.Equal(ParameterDirection.Input, binding.Direction);
            Assert.Equal((int)DbType.Int32, binding.DbType);
            Assert.Equal(getter, binding.Getter);
        }

        /// <summary>
        /// Verifies that the getter function of a ProcedureParameterBinding instance returns the expected value from a
        /// TestObj object.
        /// </summary>
        /// <remarks>This test ensures that the ProcedureParameterBinding correctly retrieves the value of
        /// the specified property when used with an object of type TestObj. It is useful for validating the binding
        /// logic in scenarios where property values are accessed dynamically.</remarks>
        [Fact]
        public void Getter_ReturnsExpectedValue()
        {
            var binding = new ProcedureParameterBinding("Value", ParameterDirection.Output, (int)DbType.String, o => ((TestObj)o).Value);
            var obj = new TestObj { Value = 123 };
            Assert.Equal(123, binding.Getter(obj));
        }

        /// <summary>
        /// Verifies that the ProcedureParameterBinding constructor allows null values for the name and getter
        /// parameters without throwing exceptions.
        /// </summary>
        /// <remarks>This test ensures that ProcedureParameterBinding can be instantiated with null values
        /// for both the name and getter, which may be required in scenarios where these values are optional or
        /// unavailable.</remarks>
        [Fact]
        public void Constructor_AllowsNullNameAndGetter()
        {
            var binding = new ProcedureParameterBinding(null, ParameterDirection.Input, 0, null);
            Assert.Null(binding.Name);
            Assert.Null(binding.Getter);
        }

        /// <summary>
        /// Verifies that the ProcedureParameterBinding constructor accepts any value for the DbType parameter.
        /// </summary>
        /// <remarks>This test ensures that the constructor does not restrict the DbType argument,
        /// allowing flexibility when binding procedure parameters in database operations.</remarks>
        [Fact]
        public void Constructor_AllowsAnyDbType()
        {
            var binding = new ProcedureParameterBinding("Test", ParameterDirection.Input, 999, o => o);
            Assert.Equal(999, binding.DbType);
        }

        /// <summary>
        /// Verifies that the ProcedureParameterBinding struct is immutable by ensuring its properties remain unchanged
        /// after assignment.
        /// </summary>
        /// <remarks>This test confirms that copying a ProcedureParameterBinding instance preserves its
        /// state, demonstrating that the struct does not allow modification of its properties after creation.</remarks>
        [Fact]
        public void Struct_IsImmutable()
        {
            var getter = new Func<object, object>(o => o);
            var binding = new ProcedureParameterBinding("Test", ParameterDirection.Input, 1, getter);
            var copy = binding;
            Assert.Equal("Test", copy.Name);
            Assert.Equal(ParameterDirection.Input, copy.Direction);
            Assert.Equal(1, copy.DbType);
            Assert.Equal(getter, copy.Getter);
        }
    }
}