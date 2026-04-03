using Hydrix.Mapping;
using System.Collections.Generic;
using Xunit;

namespace Hydrix.UnitTests.Mapping
{
    /// <summary>
    /// Contains unit tests for the OrdinalMap class, verifying its construction and property behaviors.
    /// </summary>
    /// <remarks>These tests ensure that the OrdinalMap class correctly initializes its properties and returns
    /// expected values for its Ordinals and SchemaHash properties. The tests are intended to validate the public API
    /// and contract of OrdinalMap.</remarks>
    public class OrdinalMapTests
    {
        /// <summary>
        /// Verifies that the OrdinalMap constructor correctly initializes the Ordinals and SchemaHash properties with
        /// the provided values.
        /// </summary>
        /// <remarks>This test ensures that when an OrdinalMap is instantiated with a dictionary of
        /// ordinals and a schema hash, the corresponding properties reflect those values as expected.</remarks>
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var ordinals = new Dictionary<string, int>
            {
                { "ColA", 0 },
                { "ColB", 1 }
            };
            int schemaHash = 1234;

            // Act
            var map = new OrdinalMap(ordinals, schemaHash);

            // Assert
            Assert.Equal(ordinals, map.Ordinals);
            Assert.Equal(schemaHash, map.SchemaHash);
        }

        /// <summary>
        /// Verifies that the Ordinals property returns the same dictionary instance that was provided during the
        /// initialization of the OrdinalMap.
        /// </summary>
        /// <remarks>This test ensures that the Ordinals property does not create a copy of the
        /// dictionary, but instead exposes the original instance, allowing callers to observe or modify its contents as
        /// needed.</remarks>
        [Fact]
        public void Ordinals_Property_ReturnsExpectedDictionary()
        {
            var ordinals = new Dictionary<string, int> { { "X", 7 } };
            var map = new OrdinalMap(ordinals, 42);

            Assert.Same(ordinals, map.Ordinals);
        }

        /// <summary>
        /// Verifies that the SchemaHash property returns the expected schema hash value when initialized with a
        /// specific value.
        /// </summary>
        /// <remarks>This test ensures that the OrdinalMap constructor correctly assigns the schema hash
        /// and that the SchemaHash property accurately reflects the provided value.</remarks>
        [Fact]
        public void SchemaHash_Property_ReturnsExpectedValue()
        {
            var map = new OrdinalMap(new Dictionary<string, int>(), 99);

            Assert.Equal(99, map.SchemaHash);
        }
    }
}
