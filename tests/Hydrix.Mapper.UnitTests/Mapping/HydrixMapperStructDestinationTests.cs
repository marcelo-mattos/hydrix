using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Mapping;
using System;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Mapping
{
    /// <summary>
    /// Validates the explicit rejection of value-type destinations, which are not supported by the boxed plan pipeline.
    /// </summary>
    public class HydrixMapperStructDestinationTests
    {
        /// <summary>
        /// Represents the reference-type source model shared by the struct-destination scenarios.
        /// </summary>
        private sealed class SourceModel
        {
            /// <summary>
            /// Gets or sets the identifier copied to the destination model.
            /// </summary>
            public int Id { get; set; }
        }

        /// <summary>
        /// Represents the unsupported destination value type used by the guard-clause scenarios.
        /// </summary>
        private struct DestinationStruct
        {
            /// <summary>
            /// Gets or sets the mapped identifier.
            /// </summary>
            public int Id { get; set; }
        }

        /// <summary>
        /// Creates a mapper configured with the default Hydrix options.
        /// </summary>
        /// <returns>A mapper instance ready for the struct-destination scenarios in this class.</returns>
        private static HydrixMapper CreateMapper() =>
            new HydrixMapper(
                new HydrixMapperOptions());

        /// <summary>
        /// Verifies that the object-based mapping API rejects destination structs with a clear error message.
        /// </summary>
        [Fact]
        public void Map_ThrowsInvalidOperationException_WhenDestinationIsValueType()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => CreateMapper().Map<DestinationStruct>(
                    new SourceModel()));

            Assert.Contains(
                "value type",
                exception.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that the strongly typed mapping API rejects destination structs with the same guard clause.
        /// </summary>
        [Fact]
        public void Map_TypedOverload_ThrowsInvalidOperationException_WhenDestinationIsValueType()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => CreateMapper().Map<SourceModel, DestinationStruct>(
                    new SourceModel()));

            Assert.Contains(
                "value type",
                exception.Message,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
