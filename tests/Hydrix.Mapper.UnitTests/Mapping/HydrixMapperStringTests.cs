using Hydrix.Mapper.Attributes;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Mapping;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Mapping
{
    /// <summary>
    /// Validates the string-to-string transformation behaviors exposed by the Hydrix mapper.
    /// </summary>
    /// <remarks>
    /// Each destination type is unique for its option combination so cached plans do not reuse a transform selected by a
    /// previous test.
    /// </remarks>
    public class HydrixMapperStringTests
    {
        /// <summary>
        /// Represents the source model used by the string transformation scenarios.
        /// </summary>
        private sealed class StringSource
        {
            /// <summary>
            /// Gets or sets the source string that should be transformed by the mapper.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used when no global string transform should be applied.
        /// </summary>
        private sealed class GlobalNoneDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the global trim scenario.
        /// </summary>
        private sealed class GlobalTrimDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the global trim-start scenario.
        /// </summary>
        private sealed class GlobalTrimStartDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the global trim-end scenario.
        /// </summary>
        private sealed class GlobalTrimEndDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the global uppercase scenario.
        /// </summary>
        private sealed class GlobalUppercaseDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the global lowercase scenario.
        /// </summary>
        private sealed class GlobalLowercaseDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the global trim-plus-uppercase scenario.
        /// </summary>
        private sealed class GlobalTrimUppercaseDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the global trim-plus-lowercase scenario.
        /// </summary>
        private sealed class GlobalTrimLowercaseDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces trimming regardless of the global configuration.
        /// </summary>
        private sealed class AttributeTrimDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            [MapConversion(StringTransform = StringTransform.Trim, OverrideStringTransform = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces trim-start behavior regardless of the global configuration.
        /// </summary>
        private sealed class AttributeTrimStartDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            [MapConversion(StringTransform = StringTransform.TrimStart, OverrideStringTransform = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces trim-end behavior regardless of the global configuration.
        /// </summary>
        private sealed class AttributeTrimEndDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            [MapConversion(StringTransform = StringTransform.TrimEnd, OverrideStringTransform = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces uppercase output regardless of the global configuration.
        /// </summary>
        private sealed class AttributeUppercaseDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            [MapConversion(StringTransform = StringTransform.Uppercase, OverrideStringTransform = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces lowercase output regardless of the global configuration.
        /// </summary>
        private sealed class AttributeLowercaseDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            [MapConversion(StringTransform = StringTransform.Lowercase, OverrideStringTransform = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces trim-plus-uppercase behavior.
        /// </summary>
        private sealed class AttributeTrimUppercaseDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            [MapConversion(StringTransform = StringTransform.Trim | StringTransform.Uppercase, OverrideStringTransform = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces trim-plus-lowercase behavior.
        /// </summary>
        private sealed class AttributeTrimLowercaseDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            [MapConversion(StringTransform = StringTransform.Trim | StringTransform.Lowercase, OverrideStringTransform = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces lowercase output to verify attribute precedence over a global transform.
        /// </summary>
        private sealed class AttributeLowercaseOverrideGlobalDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            [MapConversion(StringTransform = StringTransform.Lowercase, OverrideStringTransform = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member clears the global transform by overriding it with <see cref="StringTransform.None"/>.
        /// </summary>
        private sealed class AttributeNoneOverrideGlobalDto
        {
            /// <summary>
            /// Gets or sets the mapped string value.
            /// </summary>
            [MapConversion(OverrideStringTransform = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Creates a mapper configured with the requested global string transform.
        /// </summary>
        /// <param name="transform">The global string transform that should be applied by the mapper.</param>
        /// <returns>A mapper instance configured with the requested string transform.</returns>
        private static HydrixMapper CreateMapper(
            StringTransform transform)
        {
            var options = new HydrixMapperOptions();
            options.String.Transform = transform;

            return new HydrixMapper(
                options);
        }

        /// <summary>
        /// Creates a mapper configured with the default Hydrix options.
        /// </summary>
        /// <returns>A mapper instance using the default string transformation behavior.</returns>
        private static HydrixMapper CreateDefaultMapper() =>
            new HydrixMapper(
                new HydrixMapperOptions());

        /// <summary>
        /// Verifies that the default configuration preserves the original string content.
        /// </summary>
        [Fact]
        public void Map_String_None_PreservesValue()
        {
            var dto = CreateDefaultMapper().Map<GlobalNoneDto>(
                new StringSource
                {
                    Value = "  Hello  ",
                });

            Assert.Equal(
                "  Hello  ",
                dto.Value);
        }

        /// <summary>
        /// Verifies that the default configuration preserves null strings.
        /// </summary>
        [Fact]
        public void Map_String_None_NullValue_RemainsNull()
        {
            var dto = CreateDefaultMapper().Map<GlobalNoneDto>(
                new StringSource
                {
                    Value = null,
                });

            Assert.Null(
                dto.Value);
        }

        /// <summary>
        /// Verifies that the default configuration preserves empty strings.
        /// </summary>
        [Fact]
        public void Map_String_None_EmptyString_RemainsEmpty()
        {
            var dto = CreateDefaultMapper().Map<GlobalNoneDto>(
                new StringSource
                {
                    Value = string.Empty,
                });

            Assert.Equal(
                string.Empty,
                dto.Value);
        }

        /// <summary>
        /// Verifies that the global trim transform removes leading and trailing whitespace.
        /// </summary>
        [Fact]
        public void Map_String_GlobalTrim_TrimsWhitespace()
        {
            var dto = CreateMapper(
                StringTransform.Trim).Map<GlobalTrimDto>(
                new StringSource
                {
                    Value = "  hello  ",
                });

            Assert.Equal(
                "hello",
                dto.Value);
        }

        /// <summary>
        /// Verifies that the global trim transform preserves null strings.
        /// </summary>
        [Fact]
        public void Map_String_GlobalTrim_NullValue_RemainsNull()
        {
            var dto = CreateMapper(
                StringTransform.Trim).Map<GlobalTrimDto>(
                new StringSource
                {
                    Value = null,
                });

            Assert.Null(
                dto.Value);
        }

        /// <summary>
        /// Verifies that a destination attribute can force trimming when the global configuration applies no transform.
        /// </summary>
        [Fact]
        public void Map_String_AttributeTrim_OverridesGlobalNone()
        {
            var dto = CreateDefaultMapper().Map<AttributeTrimDto>(
                new StringSource
                {
                    Value = "  hello  ",
                });

            Assert.Equal(
                "hello",
                dto.Value);
        }

        /// <summary>
        /// Verifies that an attribute-based trim transform preserves null strings.
        /// </summary>
        [Fact]
        public void Map_String_AttributeTrim_NullValue_RemainsNull()
        {
            var dto = CreateDefaultMapper().Map<AttributeTrimDto>(
                new StringSource
                {
                    Value = null,
                });

            Assert.Null(
                dto.Value);
        }

        /// <summary>
        /// Verifies that the global trim-start transform removes only leading whitespace.
        /// </summary>
        [Fact]
        public void Map_String_GlobalTrimStart_TrimsLeading()
        {
            var dto = CreateMapper(
                StringTransform.TrimStart).Map<GlobalTrimStartDto>(
                new StringSource
                {
                    Value = "  hello  ",
                });

            Assert.Equal(
                "hello  ",
                dto.Value);
        }

        /// <summary>
        /// Verifies that an attribute-based trim-start transform removes only leading whitespace.
        /// </summary>
        [Fact]
        public void Map_String_AttributeTrimStart_TrimsLeadingOnly()
        {
            var dto = CreateDefaultMapper().Map<AttributeTrimStartDto>(
                new StringSource
                {
                    Value = "  hello  ",
                });

            Assert.Equal(
                "hello  ",
                dto.Value);
        }

        /// <summary>
        /// Verifies that the global trim-end transform removes only trailing whitespace.
        /// </summary>
        [Fact]
        public void Map_String_GlobalTrimEnd_TrimsTrailing()
        {
            var dto = CreateMapper(
                StringTransform.TrimEnd).Map<GlobalTrimEndDto>(
                new StringSource
                {
                    Value = "  hello  ",
                });

            Assert.Equal(
                "  hello",
                dto.Value);
        }

        /// <summary>
        /// Verifies that an attribute-based trim-end transform removes only trailing whitespace.
        /// </summary>
        [Fact]
        public void Map_String_AttributeTrimEnd_TrimsTrailingOnly()
        {
            var dto = CreateDefaultMapper().Map<AttributeTrimEndDto>(
                new StringSource
                {
                    Value = "  hello  ",
                });

            Assert.Equal(
                "  hello",
                dto.Value);
        }

        /// <summary>
        /// Verifies that the global uppercase transform converts all characters to uppercase.
        /// </summary>
        [Fact]
        public void Map_String_GlobalUppercase_ConvertsToUpper()
        {
            var dto = CreateMapper(
                StringTransform.Uppercase).Map<GlobalUppercaseDto>(
                new StringSource
                {
                    Value = "hello",
                });

            Assert.Equal(
                "HELLO",
                dto.Value);
        }

        /// <summary>
        /// Verifies that an attribute-based uppercase transform overrides the default behavior.
        /// </summary>
        [Fact]
        public void Map_String_AttributeUppercase_ConvertsToUpper()
        {
            var dto = CreateDefaultMapper().Map<AttributeUppercaseDto>(
                new StringSource
                {
                    Value = "hello",
                });

            Assert.Equal(
                "HELLO",
                dto.Value);
        }

        /// <summary>
        /// Verifies that the global lowercase transform converts all characters to lowercase.
        /// </summary>
        [Fact]
        public void Map_String_GlobalLowercase_ConvertsToLower()
        {
            var dto = CreateMapper(
                StringTransform.Lowercase).Map<GlobalLowercaseDto>(
                new StringSource
                {
                    Value = "HELLO",
                });

            Assert.Equal(
                "hello",
                dto.Value);
        }

        /// <summary>
        /// Verifies that an attribute-based lowercase transform overrides the default behavior.
        /// </summary>
        [Fact]
        public void Map_String_AttributeLowercase_ConvertsToLower()
        {
            var dto = CreateDefaultMapper().Map<AttributeLowercaseDto>(
                new StringSource
                {
                    Value = "HELLO",
                });

            Assert.Equal(
                "hello",
                dto.Value);
        }

        /// <summary>
        /// Verifies that an attribute combining trim and uppercase applies trimming before casing.
        /// </summary>
        [Fact]
        public void Map_String_TrimPlusUppercase_TrimsFirstThenUppercases()
        {
            var dto = CreateDefaultMapper().Map<AttributeTrimUppercaseDto>(
                new StringSource
                {
                    Value = "  hello world  ",
                });

            Assert.Equal(
                "HELLO WORLD",
                dto.Value);
        }

        /// <summary>
        /// Verifies that an attribute combining trim and lowercase applies trimming before casing.
        /// </summary>
        [Fact]
        public void Map_String_TrimPlusLowercase_TrimsFirstThenLowercases()
        {
            var dto = CreateDefaultMapper().Map<AttributeTrimLowercaseDto>(
                new StringSource
                {
                    Value = "  HELLO WORLD  ",
                });

            Assert.Equal(
                "hello world",
                dto.Value);
        }

        /// <summary>
        /// Verifies that the global trim-plus-uppercase transform applies both operations in sequence.
        /// </summary>
        [Fact]
        public void Map_String_GlobalTrimUppercase_TrimsAndUppercases()
        {
            var dto = CreateMapper(
                StringTransform.Trim | StringTransform.Uppercase).Map<GlobalTrimUppercaseDto>(
                new StringSource
                {
                    Value = "  hello  ",
                });

            Assert.Equal(
                "HELLO",
                dto.Value);
        }

        /// <summary>
        /// Verifies that the global trim-plus-lowercase transform applies both operations in sequence.
        /// </summary>
        [Fact]
        public void Map_String_GlobalTrimLowercase_TrimsAndLowercases()
        {
            var dto = CreateMapper(
                StringTransform.Trim | StringTransform.Lowercase).Map<GlobalTrimLowercaseDto>(
                new StringSource
                {
                    Value = "  HELLO  ",
                });

            Assert.Equal(
                "hello",
                dto.Value);
        }

        /// <summary>
        /// Verifies that a destination attribute takes precedence over an incompatible global string transform.
        /// </summary>
        [Fact]
        public void Map_String_AttributeOverridesGlobal_UsesAttributeTransform()
        {
            var options = new HydrixMapperOptions();
            options.String.Transform = StringTransform.Uppercase;

            var dto = new HydrixMapper(
                options).Map<AttributeLowercaseOverrideGlobalDto>(
                new StringSource
                {
                    Value = "HELLO",
                });

            Assert.Equal(
                "hello",
                dto.Value);
        }

        /// <summary>
        /// Verifies that overriding the global configuration without specifying a transform clears the global transform.
        /// </summary>
        [Fact]
        public void Map_String_AttributeOverrideWithNone_DisablesGlobalTransform()
        {
            var options = new HydrixMapperOptions();
            options.String.Transform = StringTransform.Uppercase;

            var dto = new HydrixMapper(
                options).Map<AttributeNoneOverrideGlobalDto>(
                new StringSource
                {
                    Value = "Hello",
                });

            Assert.Equal(
                "Hello",
                dto.Value);
        }
    }
}
