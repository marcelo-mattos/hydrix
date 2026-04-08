using Hydrix.Mapper.Attributes;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Primitives;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Mapping
{
    /// <summary>
    /// Validates the bool and enum conversion behaviors exposed by the Hydrix mapper.
    /// </summary>
    /// <remarks>
    /// Each destination type is unique for its option combination so cached plans do not reuse an earlier formatting choice.
    /// </remarks>
    public class HydrixMapperBoolEnumTests
    {
        /// <summary>
        /// Defines the sample enum used by the enum conversion scenarios.
        /// </summary>
        private enum Status
        {
            /// <summary>
            /// Represents the active state.
            /// </summary>
            Active,

            /// <summary>
            /// Represents the inactive state.
            /// </summary>
            Inactive,

            /// <summary>
            /// Represents the pending state.
            /// </summary>
            Pending,
        }

        /// <summary>
        /// Represents the source model used by the bool conversion scenarios.
        /// </summary>
        private sealed class BoolSource
        {
            /// <summary>
            /// Gets or sets the bool value converted by the mapper.
            /// </summary>
            public bool Value { get; set; }
        }

        /// <summary>
        /// Represents the source model used by enum conversion scenarios.
        /// </summary>
        private sealed class StatusSource
        {
            /// <summary>
            /// Gets or sets the enum value converted by the mapper.
            /// </summary>
            public Status Value { get; set; }
        }

        /// <summary>
        /// Represents the source model used when converting integers to enum values.
        /// </summary>
        private sealed class IntSource
        {
            /// <summary>
            /// Gets or sets the integer value converted by the mapper.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the default True/False bool format.
        /// </summary>
        private sealed class BoolTrueFalseDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the lowercase bool format.
        /// </summary>
        private sealed class BoolLowercaseDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the Yes/No bool format.
        /// </summary>
        private sealed class BoolYesNoDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the Y/N bool format.
        /// </summary>
        private sealed class BoolYnDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the 1/0 bool format.
        /// </summary>
        private sealed class BoolOneZeroDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the S/N bool format.
        /// </summary>
        private sealed class BoolSnDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the Sim/Nao bool format.
        /// </summary>
        private sealed class BoolSimNaoDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the T/F bool format.
        /// </summary>
        private sealed class BoolTfDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the custom bool format with explicit true and false labels.
        /// </summary>
        private sealed class BoolCustomDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the custom bool format when the labels are missing and fall back to defaults.
        /// </summary>
        private sealed class BoolCustomFallbackDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces the Y/N bool format.
        /// </summary>
        private sealed class BoolAttributeYnDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            [MapConversion(BoolFormat = BoolStringFormat.YOrN, OverrideBool = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces the 1/0 bool format.
        /// </summary>
        private sealed class BoolAttributeOneZeroDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            [MapConversion(BoolFormat = BoolStringFormat.OneOrZero, OverrideBool = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces the custom bool format.
        /// </summary>
        private sealed class BoolAttributeCustomDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            [MapConversion(BoolFormat = BoolStringFormat.Custom, TrueValue = "ON", FalseValue = "OFF", OverrideBool = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces the lowercase bool format.
        /// </summary>
        private sealed class BoolAttributeLowercaseDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            [MapConversion(BoolFormat = BoolStringFormat.LowercaseTrueOrFalse, OverrideBool = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used when mapping enums to their string name.
        /// </summary>
        private sealed class EnumStringDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used when mapping enums to their integral value.
        /// </summary>
        private sealed class EnumIntDto
        {
            /// <summary>
            /// Gets or sets the mapped integer value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used when mapping integers back to enum values.
        /// </summary>
        private sealed class EnumStatusDto
        {
            /// <summary>
            /// Gets or sets the mapped enum value.
            /// </summary>
            public Status Value { get; set; }
        }

        /// <summary>
        /// Creates a mapper configured with the requested global bool format.
        /// </summary>
        /// <param name="format">The bool string format that the mapper should apply.</param>
        /// <param name="trueValue">The optional custom true label used when <paramref name="format"/> is <see cref="BoolStringFormat.Custom"/>.</param>
        /// <param name="falseValue">The optional custom false label used when <paramref name="format"/> is <see cref="BoolStringFormat.Custom"/>.</param>
        /// <returns>A mapper instance configured with the requested bool formatting behavior.</returns>
        private static HydrixMapper CreateBoolMapper(
            BoolStringFormat format,
            string trueValue = null,
            string falseValue = null)
        {
            var options = new HydrixMapperOptions();
            options.Bool.StringFormat = format;

            if (trueValue != null)
            {
                options.Bool.TrueValue = trueValue;
            }

            if (falseValue != null)
            {
                options.Bool.FalseValue = falseValue;
            }

            return new HydrixMapper(
                options);
        }

        /// <summary>
        /// Creates a mapper configured with the default Hydrix options.
        /// </summary>
        /// <returns>A mapper instance using the default bool and enum conversion behavior.</returns>
        private static HydrixMapper CreateDefaultMapper() =>
            new HydrixMapper(
                new HydrixMapperOptions());

        /// <summary>
        /// Verifies that the default bool configuration formats values as True and False.
        /// </summary>
        [Fact]
        public void Map_Bool_DefaultFormat_TrueFalse()
        {
            Assert.Equal(
                "True",
                CreateDefaultMapper().Map<BoolTrueFalseDto>(
                    new BoolSource
                    {
                        Value = true,
                    }).Value);
            Assert.Equal(
                "False",
                CreateDefaultMapper().Map<BoolTrueFalseDto>(
                    new BoolSource
                    {
                        Value = false,
                    }).Value);
        }

        /// <summary>
        /// Verifies that the lowercase bool format converts both boolean states to lowercase strings.
        /// </summary>
        [Fact]
        public void Map_Bool_GlobalLowerCase()
        {
            var mapper = CreateBoolMapper(
                BoolStringFormat.LowercaseTrueOrFalse);

            Assert.Equal(
                "true",
                mapper.Map<BoolLowercaseDto>(
                    new BoolSource
                    {
                        Value = true,
                    }).Value);
            Assert.Equal(
                "false",
                mapper.Map<BoolLowercaseDto>(
                    new BoolSource
                    {
                        Value = false,
                    }).Value);
        }

        /// <summary>
        /// Verifies that the Yes/No bool format produces the expected labels.
        /// </summary>
        [Fact]
        public void Map_Bool_GlobalYesNo()
        {
            var mapper = CreateBoolMapper(
                BoolStringFormat.YesOrNo);

            Assert.Equal(
                "Yes",
                mapper.Map<BoolYesNoDto>(
                    new BoolSource
                    {
                        Value = true,
                    }).Value);
            Assert.Equal(
                "No",
                mapper.Map<BoolYesNoDto>(
                    new BoolSource
                    {
                        Value = false,
                    }).Value);
        }

        /// <summary>
        /// Verifies that the Y/N bool format produces the expected labels.
        /// </summary>
        [Fact]
        public void Map_Bool_GlobalYN()
        {
            var mapper = CreateBoolMapper(
                BoolStringFormat.YOrN);

            Assert.Equal(
                "Y",
                mapper.Map<BoolYnDto>(
                    new BoolSource
                    {
                        Value = true,
                    }).Value);
            Assert.Equal(
                "N",
                mapper.Map<BoolYnDto>(
                    new BoolSource
                    {
                        Value = false,
                    }).Value);
        }

        /// <summary>
        /// Verifies that the 1/0 bool format produces numeric string labels.
        /// </summary>
        [Fact]
        public void Map_Bool_GlobalOneZero()
        {
            var mapper = CreateBoolMapper(
                BoolStringFormat.OneOrZero);

            Assert.Equal(
                "1",
                mapper.Map<BoolOneZeroDto>(
                    new BoolSource
                    {
                        Value = true,
                    }).Value);
            Assert.Equal(
                "0",
                mapper.Map<BoolOneZeroDto>(
                    new BoolSource
                    {
                        Value = false,
                    }).Value);
        }

        /// <summary>
        /// Verifies that the T/F bool format produces the expected labels.
        /// </summary>
        [Fact]
        public void Map_Bool_GlobalTF()
        {
            var mapper = CreateBoolMapper(
                BoolStringFormat.TOrF);

            Assert.Equal(
                "T",
                mapper.Map<BoolTfDto>(
                    new BoolSource
                    {
                        Value = true,
                    }).Value);
            Assert.Equal(
                "F",
                mapper.Map<BoolTfDto>(
                    new BoolSource
                    {
                        Value = false,
                    }).Value);
        }

        /// <summary>
        /// Verifies that the custom bool format honors the supplied true and false labels.
        /// </summary>
        [Fact]
        public void Map_Bool_GlobalCustom()
        {
            var mapper = CreateBoolMapper(
                BoolStringFormat.Custom,
                "ON",
                "OFF");

            Assert.Equal(
                "ON",
                mapper.Map<BoolCustomDto>(
                    new BoolSource
                    {
                        Value = true,
                    }).Value);
            Assert.Equal(
                "OFF",
                mapper.Map<BoolCustomDto>(
                    new BoolSource
                    {
                        Value = false,
                    }).Value);
        }

        /// <summary>
        /// Verifies that a destination attribute can force the Y/N format.
        /// </summary>
        [Fact]
        public void Map_Bool_AttributeYN_OverridesGlobal()
        {
            Assert.Equal(
                "Y",
                CreateDefaultMapper().Map<BoolAttributeYnDto>(
                    new BoolSource
                    {
                        Value = true,
                    }).Value);
        }

        /// <summary>
        /// Verifies that a destination attribute can force the 1/0 format.
        /// </summary>
        [Fact]
        public void Map_Bool_AttributeOneZero_OverridesGlobal()
        {
            Assert.Equal(
                "0",
                CreateDefaultMapper().Map<BoolAttributeOneZeroDto>(
                    new BoolSource
                    {
                        Value = false,
                    }).Value);
        }

        /// <summary>
        /// Verifies that a destination attribute can supply custom bool labels.
        /// </summary>
        [Fact]
        public void Map_Bool_AttributeCustom_UsesCustomValues()
        {
            Assert.Equal(
                "ON",
                CreateDefaultMapper().Map<BoolAttributeCustomDto>(
                    new BoolSource
                    {
                        Value = true,
                    }).Value);
            Assert.Equal(
                "OFF",
                CreateDefaultMapper().Map<BoolAttributeCustomDto>(
                    new BoolSource
                    {
                        Value = false,
                    }).Value);
        }

        /// <summary>
        /// Verifies that a destination attribute can force lowercase bool output.
        /// </summary>
        [Fact]
        public void Map_Bool_AttributeLowerCase_OverridesGlobal()
        {
            Assert.Equal(
                "true",
                CreateDefaultMapper().Map<BoolAttributeLowercaseDto>(
                    new BoolSource
                    {
                        Value = true,
                    }).Value);
        }

        /// <summary>
        /// Verifies that enums map to their declared member names when targeting strings.
        /// </summary>
        [Fact]
        public void Map_Enum_ToString_UsesEnumName()
        {
            Assert.Equal(
                "Active",
                CreateDefaultMapper().Map<EnumStringDto>(
                    new StatusSource
                    {
                        Value = Status.Active,
                    }).Value);
            Assert.Equal(
                "Inactive",
                CreateDefaultMapper().Map<EnumStringDto>(
                    new StatusSource
                    {
                        Value = Status.Inactive,
                    }).Value);
            Assert.Equal(
                "Pending",
                CreateDefaultMapper().Map<EnumStringDto>(
                    new StatusSource
                    {
                        Value = Status.Pending,
                    }).Value);
        }

        /// <summary>
        /// Verifies that enums map to their underlying integer values when targeting integers.
        /// </summary>
        [Fact]
        public void Map_Enum_ToInt_UsesUnderlyingValue()
        {
            Assert.Equal(
                0,
                CreateDefaultMapper().Map<EnumIntDto>(
                    new StatusSource
                    {
                        Value = Status.Active,
                    }).Value);
            Assert.Equal(
                1,
                CreateDefaultMapper().Map<EnumIntDto>(
                    new StatusSource
                    {
                        Value = Status.Inactive,
                    }).Value);
            Assert.Equal(
                2,
                CreateDefaultMapper().Map<EnumIntDto>(
                    new StatusSource
                    {
                        Value = Status.Pending,
                    }).Value);
        }

        /// <summary>
        /// Verifies that integers cast directly back to enum values when the destination type is an enum.
        /// </summary>
        [Fact]
        public void Map_Int_ToEnum_CastsDirectly()
        {
            Assert.Equal(
                Status.Active,
                CreateDefaultMapper().Map<EnumStatusDto>(
                    new IntSource
                    {
                        Value = 0,
                    }).Value);
            Assert.Equal(
                Status.Inactive,
                CreateDefaultMapper().Map<EnumStatusDto>(
                    new IntSource
                    {
                        Value = 1,
                    }).Value);
            Assert.Equal(
                Status.Pending,
                CreateDefaultMapper().Map<EnumStatusDto>(
                    new IntSource
                    {
                        Value = 2,
                    }).Value);
        }

        /// <summary>
        /// Verifies that the custom bool format falls back to lowercase true and false when explicit labels are absent.
        /// </summary>
        [Fact]
        public void Map_Bool_GlobalCustom_WithoutExplicitValues_UsesDefaultFallbackStrings()
        {
            var options = new HydrixMapperOptions();
            options.Bool.StringFormat = BoolStringFormat.Custom;
            options.Bool.TrueValue = null;
            options.Bool.FalseValue = null;

            var mapper = new HydrixMapper(
                options);

            Assert.Equal(
                "true",
                mapper.Map<BoolCustomFallbackDto>(
                    new BoolSource
                    {
                        Value = true,
                    }).Value);
            Assert.Equal(
                "false",
                mapper.Map<BoolCustomFallbackDto>(
                    new BoolSource
                    {
                        Value = false,
                    }).Value);
        }
    }
}
