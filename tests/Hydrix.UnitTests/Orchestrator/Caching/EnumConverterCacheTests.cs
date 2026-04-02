using Hydrix.Orchestrator.Caching;
using System;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Caching
{
    /// <summary>
    /// Provides unit tests for the EnumConverterCache class, verifying correct behavior when retrieving and using enum
    /// converters for various enum types.
    /// </summary>
    /// <remarks>These tests ensure that EnumConverterCache returns appropriate converters for different enum
    /// underlying types, consistently returns the same converter instance for repeated requests of the same type, and
    /// throws exceptions for invalid input values. The tests cover both int- and byte-based enums to validate type
    /// safety and conversion accuracy.</remarks>
    public class EnumConverterCacheTests
    {
        /// <summary>
        /// Represents a simple enumeration of integer values for testing purposes.
        /// </summary>
        private enum TestEnumInt : int
        {
            /// <summary>
            /// Represents the constant value zero.
            /// </summary>
            Zero = 0,

            /// <summary>
            /// Represents the numeric value one.
            /// </summary>
            One = 1
        }

        /// <summary>
        /// Represents a set of named constants with underlying byte values for testing purposes.
        /// </summary>
        /// <remarks>This enumeration is intended for scenarios where a byte-based enum is required, such
        /// as serialization or interoperability tests. Each member corresponds to a specific byte value, providing type
        /// safety and clarity in test code.</remarks>
        private enum TestEnumByte : byte
        {
            /// <summary>
            /// Represents the value 1 in the enumeration.
            /// </summary>
            A = 1,

            /// <summary>
            /// Represents the value 2 in the enumeration.
            /// </summary>
            B = 2
        }

        /// <summary>
        /// Verifies that the GetOrAdd method returns a valid converter function for an enumeration with integer values.
        /// </summary>
        /// <remarks>This test ensures that the converter returned by EnumConverterCache.GetOrAdd
        /// correctly maps integer values to their corresponding enumeration members for the TestEnumInt type. It checks
        /// both non-zero and zero values to confirm accurate conversion.</remarks>
        [Fact]
        public void GetOrAdd_ReturnsConverter_ForEnumInt()
        {
            var converter = EnumConverterCache.GetOrAdd(typeof(TestEnumInt));
            Assert.NotNull(converter);

            // int -> enum
            var result = converter(1);
            Assert.Equal(TestEnumInt.One, result);

            // 0 -> enum
            Assert.Equal(TestEnumInt.Zero, converter(0));
        }

        /// <summary>
        /// Verifies that the EnumConverterCache.GetOrAdd method returns a valid converter for an enum with an
        /// underlying byte type and that the converter correctly maps byte values to the corresponding enum members.
        /// </summary>
        /// <remarks>This test ensures that the converter returned by EnumConverterCache.GetOrAdd can
        /// handle byte values and convert them to the appropriate enum values for enums defined with a byte underlying
        /// type. It also checks that the converter is not null and produces expected results for valid byte
        /// inputs.</remarks>
        [Fact]
        public void GetOrAdd_ReturnsConverter_ForEnumByte()
        {
            var converter = EnumConverterCache.GetOrAdd(typeof(TestEnumByte));
            Assert.NotNull(converter);

            // byte -> enum
            var result = converter((byte)2);
            Assert.Equal(TestEnumByte.B, result);

            // 1 -> enum (int convertible to byte)
            Assert.Equal(TestEnumByte.A, converter((byte)1));
        }

        /// <summary>
        /// Verifies that the GetOrAdd method returns the same delegate instance when called multiple times with the
        /// same enum type.
        /// </summary>
        /// <remarks>This test ensures that the EnumConverterCache correctly caches delegates for enum
        /// types, improving performance by avoiding redundant delegate creation.</remarks>
        [Fact]
        public void GetOrAdd_ReturnsSameDelegate_ForSameType()
        {
            var c1 = EnumConverterCache.GetOrAdd(typeof(TestEnumInt));
            var c2 = EnumConverterCache.GetOrAdd(typeof(TestEnumInt));
            Assert.Same(c1, c2);
        }

        /// <summary>
        /// Verifies that the converter throws an exception when provided with invalid input.
        /// </summary>
        /// <remarks>This test ensures that the converter correctly handles inputs that cannot be
        /// converted to the expected enum type, thereby maintaining robustness in the conversion process.</remarks>
        [Fact]
        public void Converter_Throws_OnInvalidInput()
        {
            var converter = EnumConverterCache.GetOrAdd(typeof(TestEnumInt));
            Assert.ThrowsAny<Exception>(() => converter("not a number"));
        }
    }
}
