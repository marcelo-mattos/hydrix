using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Primitives;
using System;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Mapping
{
    /// <summary>
    /// Validates the mapper failures exposed during plan construction and incompatible conversion scenarios.
    /// </summary>
    /// <remarks>
    /// The tests in this class intentionally use distinct source and destination types so each scenario builds an isolated
    /// mapping plan without inheriting a cached result from a previous assertion.
    /// </remarks>
    public class HydrixMapperFailureTests
    {
        /// <summary>
        /// Represents the string source used by the unsupported conversion scenario.
        /// </summary>
        private sealed class StringSource1
        {
            /// <summary>
            /// Gets or sets the string value used by the unsupported conversion scenario.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the first integer source used by constructor-validation scenarios.
        /// </summary>
        private sealed class IntegerSource1
        {
            /// <summary>
            /// Gets or sets the integer value used by the scenario.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the second integer source used by constructor-validation scenarios.
        /// </summary>
        private sealed class IntegerSource2
        {
            /// <summary>
            /// Gets or sets the integer value used by the scenario.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the integer source used by the widening conversion scenario.
        /// </summary>
        private sealed class IntegerSource3
        {
            /// <summary>
            /// Gets or sets the integer value used by the scenario.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the decimal source used by the global overflow-throw scenario.
        /// </summary>
        private sealed class DecimalSource1
        {
            /// <summary>
            /// Gets or sets the decimal value used by the scenario.
            /// </summary>
            public decimal Value { get; set; }
        }

        /// <summary>
        /// Represents the decimal source used by the in-range overflow-throw scenario.
        /// </summary>
        private sealed class DecimalSource2
        {
            /// <summary>
            /// Gets or sets the decimal value used by the scenario.
            /// </summary>
            public decimal Value { get; set; }
        }

        /// <summary>
        /// Represents the unsupported destination that expects an integer.
        /// </summary>
        private sealed class IntDestination
        {
            /// <summary>
            /// Gets or sets the integer value expected from the mapper.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the widening destination that expects a decimal.
        /// </summary>
        private sealed class DecimalDestination
        {
            /// <summary>
            /// Gets or sets the decimal value expected from the mapper.
            /// </summary>
            public decimal Value { get; set; }
        }

        /// <summary>
        /// Represents a destination type with no accessible parameterless constructor.
        /// </summary>
        private sealed class NoPublicParameterlessConstructor
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="NoPublicParameterlessConstructor"/> class.
            /// </summary>
            private NoPublicParameterlessConstructor()
            {
            }
        }

        /// <summary>
        /// Represents a destination type that exposes only a non-default constructor.
        /// </summary>
        private sealed class OnlyNonDefaultConstructor
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OnlyNonDefaultConstructor"/> class.
            /// </summary>
            /// <param name="value">The value required by the constructor.</param>
            private OnlyNonDefaultConstructor(
                int value)
            {
                _ = value;
            }
        }

        /// <summary>
        /// Represents the integer destination used when overflow exceptions are expected.
        /// </summary>
        private sealed class OverflowThrowDestination
        {
            /// <summary>
            /// Gets or sets the integer value expected from the mapper.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the integer destination used when an in-range conversion should succeed.
        /// </summary>
        private sealed class OverflowSafeDestination
        {
            /// <summary>
            /// Gets or sets the integer value expected from the mapper.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Creates a mapper configured with the default Hydrix options.
        /// </summary>
        /// <returns>A mapper instance ready for the failure scenarios in this test class.</returns>
        private static HydrixMapper CreateMapper() =>
            new HydrixMapper(
                new HydrixMapperOptions());

        /// <summary>
        /// Verifies that the mapper constructor rejects a <see langword="null"/> options instance.
        /// </summary>
        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenOptionsIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new HydrixMapper(
                    null));

            Assert.Equal(
                "options",
                exception.ParamName);
        }

        /// <summary>
        /// Verifies that an unsupported built-in conversion produces a descriptive invalid-operation exception.
        /// </summary>
        [Fact]
        public void Map_ThrowsInvalidOperationException_WhenConversionNotSupported()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => CreateMapper().Map<IntDestination>(
                    new StringSource1
                    {
                        Value = "hello",
                    }));

            Assert.Contains(
                "no built-in conversion",
                exception.Message,
                StringComparison.OrdinalIgnoreCase);
            Assert.Contains(
                "String",
                exception.Message);
            Assert.Contains(
                "Int32",
                exception.Message);
        }

        /// <summary>
        /// Verifies that destination types without a public parameterless constructor are rejected.
        /// </summary>
        [Fact]
        public void Map_ThrowsInvalidOperationException_WhenDestHasNoPublicParameterlessCtor()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => CreateMapper().Map<NoPublicParameterlessConstructor>(
                    new IntegerSource1
                    {
                        Value = 1,
                    }));

            Assert.Contains(
                "parameterless constructor",
                exception.Message);
            Assert.Contains(
                nameof(NoPublicParameterlessConstructor),
                exception.Message);
        }

        /// <summary>
        /// Verifies that destination types exposing only non-default constructors are rejected.
        /// </summary>
        [Fact]
        public void Map_ThrowsInvalidOperationException_WhenDestHasOnlyNonDefaultCtor()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => CreateMapper().Map<OnlyNonDefaultConstructor>(
                    new IntegerSource2
                    {
                        Value = 1,
                    }));

            Assert.Contains(
                "parameterless constructor",
                exception.Message);
        }

        /// <summary>
        /// Verifies that the mapper rejects a <see langword="null"/> source object.
        /// </summary>
        [Fact]
        public void Map_ThrowsArgumentNullException_WhenSourceIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => CreateMapper().Map<DecimalDestination>(
                    null));

            Assert.Equal(
                "source",
                exception.ParamName);
        }

        /// <summary>
        /// Verifies that a supported widening conversion from <see cref="int"/> to <see cref="decimal"/> succeeds.
        /// </summary>
        [Fact]
        public void Map_IntToDecimal_Succeeds()
        {
            var dto = CreateMapper().Map<DecimalDestination>(
                new IntegerSource3
                {
                    Value = 42,
                });

            Assert.Equal(
                42m,
                dto.Value);
        }

        /// <summary>
        /// Verifies that the global overflow-throw mode propagates an <see cref="OverflowException"/> when the rounded
        /// value exceeds the target range.
        /// </summary>
        [Fact]
        public void Map_GlobalOverflowThrow_ThrowsOnOverflow()
        {
            var options = new HydrixMapperOptions();
            options.Numeric.Overflow = NumericOverflow.Throw;
            options.Numeric.DecimalToIntRounding = NumericRounding.Truncate;

            var mapper = new HydrixMapper(
                options);
            var source = new DecimalSource1
            {
                Value = (decimal)int.MaxValue + 1,
            };

            Assert.Throws<OverflowException>(
                () => mapper.Map<OverflowThrowDestination>(
                    source));
        }

        /// <summary>
        /// Verifies that the global overflow-throw mode still allows in-range values to be converted successfully.
        /// </summary>
        [Fact]
        public void Map_GlobalOverflowThrow_DoesNotThrowInRange()
        {
            var options = new HydrixMapperOptions();
            options.Numeric.Overflow = NumericOverflow.Throw;
            options.Numeric.DecimalToIntRounding = NumericRounding.Truncate;

            var dto = new HydrixMapper(
                options).Map<OverflowSafeDestination>(
                new DecimalSource2
                {
                    Value = 100m,
                });

            Assert.Equal(
                100,
                dto.Value);
        }
    }
}
