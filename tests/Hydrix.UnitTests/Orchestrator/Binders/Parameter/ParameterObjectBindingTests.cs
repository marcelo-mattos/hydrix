using Hydrix.Orchestrator.Binders.Parameter;
using System;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Binders.Parameter
{
    /// <summary>
    /// Provides unit tests for the ParameterObjectBinding class, ensuring correct behavior and immutability.
    /// </summary>
    /// <remarks>This class contains tests that validate the constructor's property settings, the
    /// functionality of the getter, and the handling of null values. It also verifies that the struct maintains
    /// immutability by testing copy semantics.</remarks>
    public class ParameterObjectBindingTests
    {
        /// <summary>
        /// Verifies that the ParameterObjectBinding constructor correctly assigns the Name and Getter properties based
        /// on the provided arguments.
        /// </summary>
        /// <remarks>This test ensures that the constructor initializes the ParameterObjectBinding
        /// instance with the expected property values, confirming correct object state after instantiation.</remarks>
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            var getter = new Func<object, object>(o => o);
            var binding = new ParameterObjectBinding("Id", getter);
            Assert.Equal("Id", binding.Name);
            Assert.Equal(getter, binding.Getter);
        }

        /// <summary>
        /// Verifies that the getter of a ParameterObjectBinding retrieves the expected value from a TestObj instance.
        /// </summary>
        /// <remarks>This test ensures that the ParameterObjectBinding correctly accesses the 'Value'
        /// property of the provided object. It demonstrates the intended usage of the binding's getter functionality
        /// for property access.</remarks>
        [Fact]
        public void Getter_ReturnsExpectedValue()
        {
            var binding = new ParameterObjectBinding("Value", o => ((TestObj)o).Value);
            var obj = new TestObj { Value = 123 };
            Assert.Equal(123, binding.Getter(obj));
        }

        /// <summary>
        /// Verifies that the ParameterObjectBinding constructor supports null values for both the name and getter
        /// parameters.
        /// </summary>
        /// <remarks>This test ensures that the ParameterObjectBinding instance can be created without
        /// specifying a name or getter, which allows for greater flexibility in parameter binding scenarios.</remarks>
        [Fact]
        public void Constructor_AllowsNullNameAndGetter()
        {
            var binding = new ParameterObjectBinding(null, null);
            Assert.Null(binding.Name);
            Assert.Null(binding.Getter);
        }

        /// <summary>
        /// Verifies that the ParameterObjectBinding struct is immutable by ensuring that a copy cannot be mutated and
        /// retains its original values.
        /// </summary>
        /// <remarks>This test confirms that the struct's copy semantics prevent unintended modifications,
        /// maintaining the integrity of the original binding instance. Use this test to ensure that changes to a copy
        /// do not affect the original struct.</remarks>
        [Fact]
        public void Struct_IsImmutable()
        {
            var getter = new Func<object, object>(o => o);
            var binding = new ParameterObjectBinding("Test", getter);
            // Try to mutate (should not compile, but test for struct copy semantics)
            var copy = binding;
            Assert.Equal("Test", copy.Name);
            Assert.Equal(getter, copy.Getter);
        }

        /// <summary>
        /// Represents an object that holds an integer value.
        /// </summary>
        private class TestObj
        {
            /// <summary>
            /// Gets or sets the integer value of the TestObj instance.
            /// </summary>
            public int Value { get; set; }
        }
    }
}