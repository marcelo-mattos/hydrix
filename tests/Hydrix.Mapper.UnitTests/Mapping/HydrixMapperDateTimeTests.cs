using Hydrix.Mapper.Attributes;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Mapping;
using System;
using System.Globalization;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Mapping
{
    /// <summary>
    /// Validates the DateTime, DateTimeOffset, and DateOnly string-formatting behaviors exposed by the Hydrix mapper.
    /// </summary>
    /// <remarks>
    /// Each destination type is unique for its option combination so cached plans do not reuse formatting decisions created by
    /// previous tests.
    /// </remarks>
    public class HydrixMapperDateTimeTests
    {
        /// <summary>
        /// Represents the non-nullable DateTime source model used by the formatting scenarios.
        /// </summary>
        private sealed class DateTimeSource
        {
            /// <summary>
            /// Gets or sets the DateTime value formatted by the mapper.
            /// </summary>
            public DateTime Value { get; set; }
        }

        /// <summary>
        /// Represents the nullable DateTime source model used by the nullable formatting scenarios.
        /// </summary>
        private sealed class NullableDateTimeSource
        {
            /// <summary>
            /// Gets or sets the nullable DateTime value formatted by the mapper.
            /// </summary>
            public DateTime? Value { get; set; }
        }

        /// <summary>
        /// Represents the non-nullable DateTimeOffset source model used by the offset formatting scenarios.
        /// </summary>
        private sealed class DateTimeOffsetSource
        {
            /// <summary>
            /// Gets or sets the DateTimeOffset value formatted by the mapper.
            /// </summary>
            public DateTimeOffset Value { get; set; }
        }

        /// <summary>
        /// Represents the nullable DateTimeOffset source model used by the nullable offset formatting scenarios.
        /// </summary>
        private sealed class NullableDateTimeOffsetSource
        {
            /// <summary>
            /// Gets or sets the nullable DateTimeOffset value formatted by the mapper.
            /// </summary>
            public DateTimeOffset? Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the default DateTime formatting scenario.
        /// </summary>
        private sealed class DateTimeDefaultFormatDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the custom DateTime format scenario.
        /// </summary>
        private sealed class DateTimeCustomFormatDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the global UTC conversion scenario.
        /// </summary>
        private sealed class DateTimeUtcDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the global local-time conversion scenario.
        /// </summary>
        private sealed class DateTimeLocalDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used when no time-zone conversion should occur.
        /// </summary>
        private sealed class DateTimeNoTimeZoneDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used when a nullable DateTime source is null.
        /// </summary>
        private sealed class NullableDateTimeNullDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used when a nullable DateTime source contains a value.
        /// </summary>
        private sealed class NullableDateTimeValueDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the default DateTimeOffset formatting scenario.
        /// </summary>
        private sealed class DateTimeOffsetDefaultDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the UTC DateTimeOffset formatting scenario.
        /// </summary>
        private sealed class DateTimeOffsetUtcDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the local DateTimeOffset formatting scenario.
        /// </summary>
        private sealed class DateTimeOffsetLocalDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the nullable DateTimeOffset scenario.
        /// </summary>
        private sealed class NullableDateTimeOffsetDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member overrides the global format with a custom date pattern.
        /// </summary>
        private sealed class DateTimeAttributeCustomFormatDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            [MapConversion(DateFormat = "yyyy-MM-dd", OverrideDateTime = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces UTC conversion before formatting.
        /// </summary>
        private sealed class DateTimeAttributeUtcDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            [MapConversion(DateTimeZone = DateTimeZone.ToUtc, DateFormat = "O", OverrideDateTime = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces local-time conversion before formatting.
        /// </summary>
        private sealed class DateTimeAttributeLocalDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            [MapConversion(DateTimeZone = DateTimeZone.ToLocal, DateFormat = "O", OverrideDateTime = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member forces invariant formatting for a custom date pattern.
        /// </summary>
        private sealed class DateTimeAttributeInvariantDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            [MapConversion(DateFormat = "dd/MM/yyyy", Culture = "", OverrideDateTime = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member overrides only the time-zone behavior and relies on global format fallbacks.
        /// </summary>
        private sealed class DateTimeAttributeFallbackDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            [MapConversion(DateTimeZone = DateTimeZone.None, OverrideDateTime = true)]
            public string Value { get; set; }
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Represents the DateOnly source model used by the .NET 6+ formatting scenarios.
        /// </summary>
        private sealed class DateOnlySource
        {
            /// <summary>
            /// Gets or sets the DateOnly value formatted by the mapper.
            /// </summary>
            public DateOnly Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by the default DateOnly formatting scenario.
        /// </summary>
        private sealed class DateOnlyDefaultDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the destination model whose member overrides the global DateOnly format settings.
        /// </summary>
        private sealed class DateOnlyAttributeDto
        {
            /// <summary>
            /// Gets or sets the formatted string produced by the mapper.
            /// </summary>
            [MapConversion(DateFormat = "dd/MM/yyyy", Culture = "pt-BR", OverrideDateTime = true)]
            public string Value { get; set; }
        }
#endif

        /// <summary>
        /// Creates a mapper configured with the default Hydrix options.
        /// </summary>
        /// <returns>A mapper instance using the default DateTime settings.</returns>
        private static HydrixMapper CreateDefaultMapper() =>
            new HydrixMapper(
                new HydrixMapperOptions());

        /// <summary>
        /// Creates a mapper configured with the requested DateTime formatting settings.
        /// </summary>
        /// <param name="format">The date format string that should be applied by the mapper.</param>
        /// <param name="timeZone">The time-zone normalization strategy that should be applied before formatting.</param>
        /// <param name="culture">The culture name used when formatting the value.</param>
        /// <returns>A mapper instance configured with the requested DateTime options.</returns>
        private static HydrixMapper CreateMapper(
            string format,
            DateTimeZone timeZone = DateTimeZone.None,
            string culture = "")
        {
            var options = new HydrixMapperOptions();
            options.DateTime.StringFormat = format;
            options.DateTime.TimeZone = timeZone;
            options.DateTime.Culture = culture;

            return new HydrixMapper(
                options);
        }

        /// <summary>
        /// Verifies that the default DateTime configuration formats values using the round-trip pattern and invariant culture.
        /// </summary>
        [Fact]
        public void Map_DateTime_DefaultFormat_IsRoundTrip()
        {
            var value = new DateTime(
                2024,
                6,
                15,
                10,
                30,
                0,
                DateTimeKind.Utc);

            var dto = CreateDefaultMapper().Map<DateTimeDefaultFormatDto>(
                new DateTimeSource
                {
                    Value = value,
                });

            Assert.Equal(
                value.ToString(
                    "O",
                    CultureInfo.InvariantCulture),
                dto.Value);
        }

        /// <summary>
        /// Verifies that a custom global DateTime format is honored during mapping.
        /// </summary>
        [Fact]
        public void Map_DateTime_CustomFormat_UsesFormat()
        {
            var value = new DateTime(
                2024,
                6,
                15);

            var dto = CreateMapper(
                "yyyy-MM-dd").Map<DateTimeCustomFormatDto>(
                new DateTimeSource
                {
                    Value = value,
                });

            Assert.Equal(
                "2024-06-15",
                dto.Value);
        }

        /// <summary>
        /// Verifies that a destination attribute can override the global DateTime format.
        /// </summary>
        [Fact]
        public void Map_DateTime_AttributeFormat_OverridesGlobal()
        {
            var value = new DateTime(
                2024,
                6,
                15);

            var dto = CreateMapper(
                "O").Map<DateTimeAttributeCustomFormatDto>(
                new DateTimeSource
                {
                    Value = value,
                });

            Assert.Equal(
                "2024-06-15",
                dto.Value);
        }

        /// <summary>
        /// Verifies that the global UTC conversion is applied before DateTime formatting.
        /// </summary>
        [Fact]
        public void Map_DateTime_ToUtc_ConvertsBeforeFormatting()
        {
            var value = new DateTime(
                2024,
                1,
                1,
                12,
                0,
                0,
                DateTimeKind.Local);

            var dto = CreateMapper(
                "O",
                DateTimeZone.ToUtc).Map<DateTimeUtcDto>(
                new DateTimeSource
                {
                    Value = value,
                });

            Assert.Equal(
                value.ToUniversalTime().ToString(
                    "O",
                    CultureInfo.InvariantCulture),
                dto.Value);
        }

        /// <summary>
        /// Verifies that the global local-time conversion is applied before DateTime formatting.
        /// </summary>
        [Fact]
        public void Map_DateTime_ToLocal_ConvertsBeforeFormatting()
        {
            var value = new DateTime(
                2024,
                1,
                1,
                12,
                0,
                0,
                DateTimeKind.Utc);

            var dto = CreateMapper(
                "O",
                DateTimeZone.ToLocal).Map<DateTimeLocalDto>(
                new DateTimeSource
                {
                    Value = value,
                });

            Assert.Equal(
                value.ToLocalTime().ToString(
                    "O",
                    CultureInfo.InvariantCulture),
                dto.Value);
        }

        /// <summary>
        /// Verifies that disabling time-zone conversion preserves the original DateTime value during formatting.
        /// </summary>
        [Fact]
        public void Map_DateTime_NoneTimezone_DoesNotConvert()
        {
            var value = new DateTime(
                2024,
                6,
                15,
                10,
                0,
                0,
                DateTimeKind.Utc);

            var dto = CreateMapper(
                "O",
                DateTimeZone.None).Map<DateTimeNoTimeZoneDto>(
                new DateTimeSource
                {
                    Value = value,
                });

            Assert.Equal(
                value.ToString(
                    "O",
                    CultureInfo.InvariantCulture),
                dto.Value);
        }

        /// <summary>
        /// Verifies that a destination attribute can force UTC conversion even when the global setting does not.
        /// </summary>
        [Fact]
        public void Map_DateTime_AttributeUtc_OverridesGlobalNone()
        {
            var value = new DateTime(
                2024,
                1,
                1,
                12,
                0,
                0,
                DateTimeKind.Local);

            var dto = CreateDefaultMapper().Map<DateTimeAttributeUtcDto>(
                new DateTimeSource
                {
                    Value = value,
                });

            Assert.Equal(
                value.ToUniversalTime().ToString(
                    "O",
                    CultureInfo.InvariantCulture),
                dto.Value);
        }

        /// <summary>
        /// Verifies that an invariant-culture destination attribute produces the expected localized date pattern.
        /// </summary>
        [Fact]
        public void Map_DateTime_InvariantCulture_FormatsCorrectly()
        {
            var value = new DateTime(
                2024,
                6,
                15);

            var dto = CreateDefaultMapper().Map<DateTimeAttributeInvariantDto>(
                new DateTimeSource
                {
                    Value = value,
                });

            Assert.Equal(
                "15/06/2024",
                dto.Value);
        }

        /// <summary>
        /// Verifies that a null nullable DateTime source maps to a null destination string.
        /// </summary>
        [Fact]
        public void Map_NullableDateTime_WhenNull_ProducesNull()
        {
            var dto = CreateDefaultMapper().Map<NullableDateTimeNullDto>(
                new NullableDateTimeSource
                {
                    Value = null,
                });

            Assert.Null(
                dto.Value);
        }

        /// <summary>
        /// Verifies that a nullable DateTime source with a value is formatted normally.
        /// </summary>
        [Fact]
        public void Map_NullableDateTime_WhenHasValue_Formats()
        {
            var value = new DateTime(
                2024,
                6,
                15,
                0,
                0,
                0,
                DateTimeKind.Utc);

            var dto = CreateMapper(
                "yyyy-MM-dd").Map<NullableDateTimeValueDto>(
                new NullableDateTimeSource
                {
                    Value = value,
                });

            Assert.Equal(
                "2024-06-15",
                dto.Value);
        }

        /// <summary>
        /// Verifies that the default DateTimeOffset configuration formats values using the round-trip pattern.
        /// </summary>
        [Fact]
        public void Map_DateTimeOffset_DefaultFormat_IsRoundTrip()
        {
            var value = new DateTimeOffset(
                2024,
                6,
                15,
                10,
                30,
                0,
                TimeSpan.Zero);

            var dto = CreateDefaultMapper().Map<DateTimeOffsetDefaultDto>(
                new DateTimeOffsetSource
                {
                    Value = value,
                });

            Assert.Equal(
                value.ToString(
                    "O",
                    CultureInfo.InvariantCulture),
                dto.Value);
        }

        /// <summary>
        /// Verifies that global UTC normalization is applied to DateTimeOffset values before formatting.
        /// </summary>
        [Fact]
        public void Map_DateTimeOffset_ToUtc_NormalizesOffset()
        {
            var value = new DateTimeOffset(
                2024,
                6,
                15,
                12,
                0,
                0,
                TimeSpan.FromHours(
                    3));

            var dto = CreateMapper(
                "O",
                DateTimeZone.ToUtc).Map<DateTimeOffsetUtcDto>(
                new DateTimeOffsetSource
                {
                    Value = value,
                });

            Assert.Equal(
                value.ToUniversalTime().ToString(
                    "O",
                    CultureInfo.InvariantCulture),
                dto.Value);
        }

        /// <summary>
        /// Verifies that a null nullable DateTimeOffset source maps to a null destination string.
        /// </summary>
        [Fact]
        public void Map_NullableDateTimeOffset_WhenNull_ProducesNull()
        {
            var dto = CreateDefaultMapper().Map<NullableDateTimeOffsetDto>(
                new NullableDateTimeOffsetSource
                {
                    Value = null,
                });

            Assert.Null(
                dto.Value);
        }

        /// <summary>
        /// Verifies that global local-time normalization is applied to DateTimeOffset values before formatting.
        /// </summary>
        [Fact]
        public void Map_DateTimeOffset_ToLocal_NormalizesOffset()
        {
            var value = new DateTimeOffset(
                2024,
                6,
                15,
                12,
                0,
                0,
                TimeSpan.FromHours(
                    -3));

            var dto = CreateMapper(
                "O",
                DateTimeZone.ToLocal).Map<DateTimeOffsetLocalDto>(
                new DateTimeOffsetSource
                {
                    Value = value,
                });

            Assert.Equal(
                value.ToLocalTime().ToString(
                    "O",
                    CultureInfo.InvariantCulture),
                dto.Value);
        }

        /// <summary>
        /// Verifies that a destination attribute without an explicit format falls back to the global format and culture.
        /// </summary>
        [Fact]
        public void Map_DateTime_AttributeWithoutFormat_UsesGlobalFallbacks()
        {
            var options = new HydrixMapperOptions();
            options.DateTime.StringFormat = "D";
            options.DateTime.Culture = "pt-BR";

            var mapper = new HydrixMapper(
                options);
            var value = new DateTime(
                2024,
                6,
                15,
                0,
                0,
                0,
                DateTimeKind.Utc);
            var dto = mapper.Map<DateTimeAttributeFallbackDto>(
                new DateTimeSource
                {
                    Value = value,
                });

            Assert.Equal(
                value.ToString(
                    "D",
                    CultureInfo.GetCultureInfo(
                        "pt-BR")),
                dto.Value);
        }

        /// <summary>
        /// Verifies that null global format and culture settings fall back to the invariant round-trip representation.
        /// </summary>
        [Fact]
        public void Map_DateTime_GlobalNullFormatAndCulture_FallBackToRoundTripInvariant()
        {
            var options = new HydrixMapperOptions();
            options.DateTime.StringFormat = null;
            options.DateTime.Culture = null;

            var mapper = new HydrixMapper(
                options);
            var value = new DateTime(
                2024,
                6,
                15,
                10,
                30,
                0,
                DateTimeKind.Utc);
            var dto = mapper.Map<DateTimeNoTimeZoneDto>(
                new DateTimeSource
                {
                    Value = value,
                });

            Assert.Equal(
                value.ToString(
                    "O",
                    CultureInfo.InvariantCulture),
                dto.Value);
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Verifies that DateOnly values honor the global format and culture settings.
        /// </summary>
        [Fact]
        public void Map_DateOnly_DefaultFormat_UsesGlobalSettings()
        {
            var mapper = CreateMapper(
                "yyyy-MM-dd",
                culture: "en-US");
            var value = new DateOnly(
                2024,
                6,
                15);
            var dto = mapper.Map<DateOnlyDefaultDto>(
                new DateOnlySource
                {
                    Value = value,
                });

            Assert.Equal(
                value.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.GetCultureInfo(
                        "en-US")),
                dto.Value);
        }

        /// <summary>
        /// Verifies that destination attributes can override the global DateOnly format and culture settings.
        /// </summary>
        [Fact]
        public void Map_DateOnly_AttributeFormat_OverridesGlobalSettings()
        {
            var mapper = CreateMapper(
                "O",
                culture: "en-US");
            var value = new DateOnly(
                2024,
                6,
                15);
            var dto = mapper.Map<DateOnlyAttributeDto>(
                new DateOnlySource
                {
                    Value = value,
                });

            Assert.Equal(
                value.ToString(
                    "dd/MM/yyyy",
                    CultureInfo.GetCultureInfo(
                        "pt-BR")),
                dto.Value);
        }
#endif
    }
}
