using Hydrix.Mapper.Attributes;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Mapping;
using System;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Mapping
{
    /// <summary>
    /// Validates the numeric conversion behaviors exposed by the Hydrix mapper.
    /// </summary>
    /// <remarks>
    /// Each destination type is unique for its option combination so cached plans do not reuse a rounding or overflow rule
    /// selected by a previous test.
    /// </remarks>
    public class HydrixMapperNumericTests
    {
        /// <summary>
        /// Represents the decimal source model used by the decimal conversion scenarios.
        /// </summary>
        private sealed class DecimalSource
        {
            /// <summary>
            /// Gets or sets the decimal value converted by the mapper.
            /// </summary>
            public decimal Value { get; set; }
        }

        /// <summary>
        /// Represents the double source model used by the double conversion scenarios.
        /// </summary>
        private sealed class DoubleSource
        {
            /// <summary>
            /// Gets or sets the double value converted by the mapper.
            /// </summary>
            public double Value { get; set; }
        }

        /// <summary>
        /// Represents the float source model used by the float conversion scenarios.
        /// </summary>
        private sealed class FloatSource
        {
            /// <summary>
            /// Gets or sets the float value converted by the mapper.
            /// </summary>
            public float Value { get; set; }
        }

        /// <summary>
        /// Represents the nullable decimal source model used by nullable conversion scenarios.
        /// </summary>
        private sealed class NullableDecimalSource
        {
            /// <summary>
            /// Gets or sets the nullable decimal value converted by the mapper.
            /// </summary>
            public decimal? Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by decimal truncation scenarios.
        /// </summary>
        private sealed class DecimalTruncateDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by decimal ceiling scenarios.
        /// </summary>
        private sealed class DecimalCeilingDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by decimal floor scenarios.
        /// </summary>
        private sealed class DecimalFloorDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by decimal nearest-rounding scenarios.
        /// </summary>
        private sealed class DecimalNearestDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by decimal banker-rounding scenarios.
        /// </summary>
        private sealed class DecimalBankerDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by decimal-to-long scenarios.
        /// </summary>
        private sealed class DecimalToLongDto
        {
            /// <summary>
            /// Gets or sets the mapped long value.
            /// </summary>
            public long Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by double truncation scenarios.
        /// </summary>
        private sealed class DoubleTruncateDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by float truncation scenarios.
        /// </summary>
        private sealed class FloatTruncateDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used when a null nullable decimal should fall back to zero.
        /// </summary>
        private sealed class NullableDecimalDefaultDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used when a nullable decimal value should be rounded with ceiling behavior.
        /// </summary>
        private sealed class NullableDecimalCeilingDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by negative decimal truncation scenarios.
        /// </summary>
        private sealed class NegativeDecimalTruncateDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces decimal ceiling behavior.
        /// </summary>
        private sealed class AttributeCeilingDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            [MapConversion(NumericRounding = NumericRounding.Ceiling, OverrideNumeric = true)]
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces decimal floor behavior.
        /// </summary>
        private sealed class AttributeFloorDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            [MapConversion(NumericRounding = NumericRounding.Floor, OverrideNumeric = true)]
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces decimal nearest rounding.
        /// </summary>
        private sealed class AttributeNearestDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            [MapConversion(NumericRounding = NumericRounding.Nearest, OverrideNumeric = true)]
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces decimal banker rounding.
        /// </summary>
        private sealed class AttributeBankerDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            [MapConversion(NumericRounding = NumericRounding.Banker, OverrideNumeric = true)]
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces overflow exceptions.
        /// </summary>
        private sealed class AttributeOverflowThrowDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            [MapConversion(NumericRounding = NumericRounding.Truncate, NumericOverflow = NumericOverflow.Throw, OverrideNumeric = true)]
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces byte clamping on overflow.
        /// </summary>
        private sealed class AttributeClampByteDto
        {
            /// <summary>
            /// Gets or sets the mapped byte value.
            /// </summary>
            [MapConversion(NumericRounding = NumericRounding.Truncate, NumericOverflow = NumericOverflow.Clamp, OverrideNumeric = true)]
            public byte Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by double ceiling scenarios.
        /// </summary>
        private sealed class DoubleCeilingDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by double floor scenarios.
        /// </summary>
        private sealed class DoubleFloorDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by double nearest-rounding scenarios.
        /// </summary>
        private sealed class DoubleNearestDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by double banker-rounding scenarios.
        /// </summary>
        private sealed class DoubleBankerDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used when clamping doubles to <see cref="sbyte"/>.
        /// </summary>
        private sealed class DoubleClampSByteDto
        {
            /// <summary>
            /// Gets or sets the mapped sbyte value.
            /// </summary>
            [MapConversion(NumericOverflow = NumericOverflow.Clamp, OverrideNumeric = true)]
            public sbyte Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used when clamping doubles to <see cref="ushort"/>.
        /// </summary>
        private sealed class DoubleClampUShortDto
        {
            /// <summary>
            /// Gets or sets the mapped unsigned short value.
            /// </summary>
            [MapConversion(NumericOverflow = NumericOverflow.Clamp, OverrideNumeric = true)]
            public ushort Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used when clamping doubles to <see cref="uint"/>.
        /// </summary>
        private sealed class DoubleClampUIntDto
        {
            /// <summary>
            /// Gets or sets the mapped unsigned integer value.
            /// </summary>
            [MapConversion(NumericOverflow = NumericOverflow.Clamp, OverrideNumeric = true)]
            public uint Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used when clamping doubles to <see cref="ulong"/>.
        /// </summary>
        private sealed class DoubleClampULongDto
        {
            /// <summary>
            /// Gets or sets the mapped unsigned long value.
            /// </summary>
            [MapConversion(NumericOverflow = NumericOverflow.Clamp, OverrideNumeric = true)]
            public ulong Value { get; set; }
        }

        /// <summary>
        /// Creates a mapper configured with the requested numeric rounding and overflow rules.
        /// </summary>
        /// <param name="rounding">The rounding rule that should be applied to decimal, double, and float conversions.</param>
        /// <param name="overflow">The overflow strategy that should be applied during the conversion.</param>
        /// <returns>A mapper instance configured with the requested numeric options.</returns>
        private static HydrixMapper CreateMapper(
            NumericRounding rounding,
            NumericOverflow overflow = NumericOverflow.Truncate)
        {
            var options = new HydrixMapperOptions();
            options.Numeric.DecimalToIntRounding = rounding;
            options.Numeric.Overflow = overflow;

            return new HydrixMapper(
                options);
        }

        /// <summary>
        /// Creates a mapper configured with the default Hydrix numeric options.
        /// </summary>
        /// <returns>A mapper instance using the default numeric behavior.</returns>
        private static HydrixMapper CreateDefaultMapper() =>
            new HydrixMapper(
                new HydrixMapperOptions());

        /// <summary>
        /// Verifies that decimal truncation removes the fractional component toward zero.
        /// </summary>
        /// <param name="input">The decimal value represented as a double for inline-data convenience.</param>
        /// <param name="expected">The integer value expected after truncation.</param>
        [Theory]
        [InlineData(3.9, 3)]
        [InlineData(3.1, 3)]
        [InlineData(-3.9, -3)]
        [InlineData(-3.1, -3)]
        [InlineData(0.0, 0)]
        public void Map_Decimal_Truncate_TruncatesTowardZero(
            double input,
            int expected)
        {
            var dto = CreateMapper(
                NumericRounding.Truncate).Map<DecimalTruncateDto>(
                new DecimalSource
                {
                    Value = (decimal)input,
                });

            Assert.Equal(
                expected,
                dto.Value);
        }

        /// <summary>
        /// Verifies that double truncation removes the fractional component toward zero.
        /// </summary>
        [Fact]
        public void Map_Double_Truncate_TruncatesTowardZero()
        {
            var dto = CreateMapper(
                NumericRounding.Truncate).Map<DoubleTruncateDto>(
                new DoubleSource
                {
                    Value = 4.9d,
                });

            Assert.Equal(
                4,
                dto.Value);
        }

        /// <summary>
        /// Verifies that float truncation removes the fractional component toward zero.
        /// </summary>
        [Fact]
        public void Map_Float_Truncate_TruncatesTowardZero()
        {
            var dto = CreateMapper(
                NumericRounding.Truncate).Map<FloatTruncateDto>(
                new FloatSource
                {
                    Value = 4.9f,
                });

            Assert.Equal(
                4,
                dto.Value);
        }

        /// <summary>
        /// Verifies that decimal ceiling rounds values toward positive infinity.
        /// </summary>
        /// <param name="input">The decimal value represented as a double for inline-data convenience.</param>
        /// <param name="expected">The integer value expected after ceiling rounding.</param>
        [Theory]
        [InlineData(3.1, 4)]
        [InlineData(3.9, 4)]
        [InlineData(-3.1, -3)]
        [InlineData(-3.9, -3)]
        public void Map_Decimal_Ceiling_RoundsUp(
            double input,
            int expected)
        {
            var dto = CreateMapper(
                NumericRounding.Ceiling).Map<DecimalCeilingDto>(
                new DecimalSource
                {
                    Value = (decimal)input,
                });

            Assert.Equal(
                expected,
                dto.Value);
        }

        /// <summary>
        /// Verifies that a destination attribute can override the global truncation rule with ceiling rounding.
        /// </summary>
        [Fact]
        public void Map_Attribute_Ceiling_OverridesGlobalTruncate()
        {
            var dto = CreateDefaultMapper().Map<AttributeCeilingDto>(
                new DecimalSource
                {
                    Value = 3.1m,
                });

            Assert.Equal(
                4,
                dto.Value);
        }

        /// <summary>
        /// Verifies that decimal floor rounds values toward negative infinity.
        /// </summary>
        /// <param name="input">The decimal value represented as a double for inline-data convenience.</param>
        /// <param name="expected">The integer value expected after floor rounding.</param>
        [Theory]
        [InlineData(3.9, 3)]
        [InlineData(3.1, 3)]
        [InlineData(-3.1, -4)]
        [InlineData(-3.9, -4)]
        public void Map_Decimal_Floor_RoundsDown(
            double input,
            int expected)
        {
            var dto = CreateMapper(
                NumericRounding.Floor).Map<DecimalFloorDto>(
                new DecimalSource
                {
                    Value = (decimal)input,
                });

            Assert.Equal(
                expected,
                dto.Value);
        }

        /// <summary>
        /// Verifies that a destination attribute can override the global truncation rule with floor rounding.
        /// </summary>
        [Fact]
        public void Map_Attribute_Floor_OverridesGlobalTruncate()
        {
            var dto = CreateDefaultMapper().Map<AttributeFloorDto>(
                new DecimalSource
                {
                    Value = 3.9m,
                });

            Assert.Equal(
                3,
                dto.Value);
        }

        /// <summary>
        /// Verifies that decimal nearest rounding resolves midpoint values away from zero.
        /// </summary>
        /// <param name="input">The decimal value represented as a double for inline-data convenience.</param>
        /// <param name="expected">The integer value expected after nearest rounding.</param>
        [Theory]
        [InlineData(2.5, 3)]
        [InlineData(3.5, 4)]
        [InlineData(-2.5, -3)]
        public void Map_Decimal_Nearest_RoundsAwayFromZeroOnTie(
            double input,
            int expected)
        {
            var dto = CreateMapper(
                NumericRounding.Nearest).Map<DecimalNearestDto>(
                new DecimalSource
                {
                    Value = (decimal)input,
                });

            Assert.Equal(
                expected,
                dto.Value);
        }

        /// <summary>
        /// Verifies that a destination attribute can override the global rule with nearest rounding.
        /// </summary>
        [Fact]
        public void Map_Attribute_Nearest_RoundsCorrectly()
        {
            var dto = CreateDefaultMapper().Map<AttributeNearestDto>(
                new DecimalSource
                {
                    Value = 2.6m,
                });

            Assert.Equal(
                3,
                dto.Value);
        }

        /// <summary>
        /// Verifies that decimal banker rounding resolves midpoint values to the nearest even integer.
        /// </summary>
        /// <param name="input">The decimal value represented as a double for inline-data convenience.</param>
        /// <param name="expected">The integer value expected after banker rounding.</param>
        [Theory]
        [InlineData(2.5, 2)]
        [InlineData(3.5, 4)]
        public void Map_Decimal_Banker_RoundsToEven(
            double input,
            int expected)
        {
            var dto = CreateMapper(
                NumericRounding.Banker).Map<DecimalBankerDto>(
                new DecimalSource
                {
                    Value = (decimal)input,
                });

            Assert.Equal(
                expected,
                dto.Value);
        }

        /// <summary>
        /// Verifies that a destination attribute can override the global rule with banker rounding.
        /// </summary>
        [Fact]
        public void Map_Attribute_Banker_RoundsToEven()
        {
            var dto = CreateDefaultMapper().Map<AttributeBankerDto>(
                new DecimalSource
                {
                    Value = 2.5m,
                });

            Assert.Equal(
                2,
                dto.Value);
        }

        /// <summary>
        /// Verifies that a destination attribute configured for overflow throwing raises an exception when the value exceeds the target range.
        /// </summary>
        [Fact]
        public void Map_Attribute_OverflowThrow_ThrowsOnOverflow()
        {
            var source = new DecimalSource
            {
                Value = (decimal)int.MaxValue + 1,
            };

            Assert.ThrowsAny<Exception>(
                () => CreateDefaultMapper().Map<AttributeOverflowThrowDto>(
                    source));
        }

        /// <summary>
        /// Verifies that overflow throwing does not reject values that remain within the destination range.
        /// </summary>
        [Fact]
        public void Map_Attribute_OverflowThrow_DoesNotThrowInRange()
        {
            var source = new DecimalSource
            {
                Value = 100m,
            };

            var dto = CreateDefaultMapper().Map<AttributeOverflowThrowDto>(
                source);

            Assert.Equal(
                100,
                dto.Value);
        }

        /// <summary>
        /// Verifies that overflow clamping limits values above the byte range to <see cref="byte.MaxValue"/>.
        /// </summary>
        [Fact]
        public void Map_Attribute_OverflowClamp_ClampsToMax()
        {
            var source = new DecimalSource
            {
                Value = 300m,
            };

            var dto = CreateDefaultMapper().Map<AttributeClampByteDto>(
                source);

            Assert.Equal(
                byte.MaxValue,
                dto.Value);
        }

        /// <summary>
        /// Verifies that overflow clamping limits values below the byte range to <see cref="byte.MinValue"/>.
        /// </summary>
        [Fact]
        public void Map_Attribute_OverflowClamp_ClampsToMin()
        {
            var source = new DecimalSource
            {
                Value = -1m,
            };

            var dto = CreateDefaultMapper().Map<AttributeClampByteDto>(
                source);

            Assert.Equal(
                byte.MinValue,
                dto.Value);
        }

        /// <summary>
        /// Verifies that overflow clamping leaves in-range byte values untouched.
        /// </summary>
        [Fact]
        public void Map_Attribute_OverflowClamp_InRange_PassesThrough()
        {
            var source = new DecimalSource
            {
                Value = 100m,
            };

            var dto = CreateDefaultMapper().Map<AttributeClampByteDto>(
                source);

            Assert.Equal(
                (byte)100,
                dto.Value);
        }

        /// <summary>
        /// Verifies that a null nullable decimal falls back to zero when targeting a non-nullable integer.
        /// </summary>
        [Fact]
        public void Map_NullableDecimal_WhenNull_UsesDefaultZero()
        {
            var dto = CreateDefaultMapper().Map<NullableDecimalDefaultDto>(
                new NullableDecimalSource
                {
                    Value = null,
                });

            Assert.Equal(
                0,
                dto.Value);
        }

        /// <summary>
        /// Verifies that a nullable decimal with a value still respects the configured rounding rule.
        /// </summary>
        [Fact]
        public void Map_NullableDecimal_WhenHasValue_Converts()
        {
            var dto = CreateMapper(
                NumericRounding.Ceiling).Map<NullableDecimalCeilingDto>(
                new NullableDecimalSource
                {
                    Value = 2.3m,
                });

            Assert.Equal(
                3,
                dto.Value);
        }

        /// <summary>
        /// Verifies that truncation of negative decimals still proceeds toward zero.
        /// </summary>
        [Fact]
        public void Map_Decimal_Negative_Truncate_TruncatesTowardZero()
        {
            var dto = CreateMapper(
                NumericRounding.Truncate).Map<NegativeDecimalTruncateDto>(
                new DecimalSource
                {
                    Value = -9.9m,
                });

            Assert.Equal(
                -9,
                dto.Value);
        }

        /// <summary>
        /// Verifies that decimal values can be converted to <see cref="long"/> using the configured rounding rule.
        /// </summary>
        [Fact]
        public void Map_Decimal_ToLong_Works()
        {
            var dto = CreateMapper(
                NumericRounding.Nearest).Map<DecimalToLongDto>(
                new DecimalSource
                {
                    Value = 1234567890.6m,
                });

            Assert.Equal(
                1234567891L,
                dto.Value);
        }

        /// <summary>
        /// Verifies that double ceiling rounds values toward positive infinity.
        /// </summary>
        [Fact]
        public void Map_Double_Ceiling_RoundsUp()
        {
            var dto = CreateMapper(
                NumericRounding.Ceiling).Map<DoubleCeilingDto>(
                new DoubleSource
                {
                    Value = 4.1d,
                });

            Assert.Equal(
                5,
                dto.Value);
        }

        /// <summary>
        /// Verifies that double floor rounds values toward negative infinity.
        /// </summary>
        [Fact]
        public void Map_Double_Floor_RoundsDown()
        {
            var dto = CreateMapper(
                NumericRounding.Floor).Map<DoubleFloorDto>(
                new DoubleSource
                {
                    Value = 4.9d,
                });

            Assert.Equal(
                4,
                dto.Value);
        }

        /// <summary>
        /// Verifies that double nearest rounding resolves midpoint values away from zero.
        /// </summary>
        [Fact]
        public void Map_Double_Nearest_RoundsAwayFromZero()
        {
            var dto = CreateMapper(
                NumericRounding.Nearest).Map<DoubleNearestDto>(
                new DoubleSource
                {
                    Value = 2.5d,
                });

            Assert.Equal(
                3,
                dto.Value);
        }

        /// <summary>
        /// Verifies that double banker rounding resolves midpoint values to the nearest even integer.
        /// </summary>
        [Fact]
        public void Map_Double_Banker_RoundsToEven()
        {
            var dto = CreateMapper(
                NumericRounding.Banker).Map<DoubleBankerDto>(
                new DoubleSource
                {
                    Value = 2.5d,
                });

            Assert.Equal(
                2,
                dto.Value);
        }

        /// <summary>
        /// Verifies that clamping a negative double to <see cref="sbyte"/> produces the minimum representable value.
        /// </summary>
        [Fact]
        public void Map_Double_Clamp_ToSByte_UsesMinimum()
        {
            var dto = CreateDefaultMapper().Map<DoubleClampSByteDto>(
                new DoubleSource
                {
                    Value = -500d,
                });

            Assert.Equal(
                sbyte.MinValue,
                dto.Value);
        }

        /// <summary>
        /// Verifies that clamping a negative double to <see cref="ushort"/> produces the minimum representable value.
        /// </summary>
        [Fact]
        public void Map_Double_Clamp_ToUShort_UsesMinimum()
        {
            var dto = CreateDefaultMapper().Map<DoubleClampUShortDto>(
                new DoubleSource
                {
                    Value = -1d,
                });

            Assert.Equal(
                ushort.MinValue,
                dto.Value);
        }

        /// <summary>
        /// Verifies that clamping a negative double to <see cref="uint"/> produces the minimum representable value.
        /// </summary>
        [Fact]
        public void Map_Double_Clamp_ToUInt_UsesMinimum()
        {
            var dto = CreateDefaultMapper().Map<DoubleClampUIntDto>(
                new DoubleSource
                {
                    Value = -1d,
                });

            Assert.Equal(
                uint.MinValue,
                dto.Value);
        }

        /// <summary>
        /// Verifies that clamping a negative double to <see cref="ulong"/> produces the minimum representable value.
        /// </summary>
        [Fact]
        public void Map_Double_Clamp_ToULong_UsesMinimum()
        {
            var dto = CreateDefaultMapper().Map<DoubleClampULongDto>(
                new DoubleSource
                {
                    Value = -1d,
                });

            Assert.Equal(
                ulong.MinValue,
                dto.Value);
        }

        /// <summary>
        /// Verifies that clamping a very large double to <see cref="ulong"/> is capped at the supported maximum intermediate long value.
        /// </summary>
        [Fact]
        public void Map_Double_Clamp_ToULong_UsesCappedMaximum()
        {
            var dto = CreateDefaultMapper().Map<DoubleClampULongDto>(
                new DoubleSource
                {
                    Value = double.MaxValue,
                });

            Assert.Equal(
                (ulong)long.MaxValue,
                dto.Value);
        }
    }
}
