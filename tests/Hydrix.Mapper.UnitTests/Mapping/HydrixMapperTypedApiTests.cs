using Hydrix.Mapper.Configuration;
using System;
using System.Collections.Generic;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Mapping
{
    /// <summary>
    /// Validates the strongly typed mapper overloads used for homogeneous source shapes.
    /// </summary>
    public class HydrixMapperTypedApiTests
    {
        /// <summary>
        /// Represents the source model shared by the typed API scenarios.
        /// </summary>
        private sealed class PersonEntity
        {
            /// <summary>
            /// Gets or sets the identifier copied to the destination model.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name copied to the destination model.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents the destination model shared by the typed API scenarios.
        /// </summary>
        private sealed class PersonDto
        {
            /// <summary>
            /// Gets or sets the mapped identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the mapped name.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Creates a mapper configured with the default Hydrix options.
        /// </summary>
        /// <returns>A mapper instance ready for the typed API scenarios in this class.</returns>
        private static HydrixMapper CreateMapper() =>
            new HydrixMapper(
                new HydrixMapperOptions());

        /// <summary>
        /// Verifies that the strongly typed single-object overload maps matching properties correctly.
        /// </summary>
        [Fact]
        public void Map_TypedOverload_MapsMatchingPropertiesByExactName()
        {
            var entity = new PersonEntity
            {
                Id = 1,
                Name = "Alice",
            };

            var dto = CreateMapper().Map<PersonEntity, PersonDto>(
                entity);

            Assert.Equal(
                1,
                dto.Id);
            Assert.Equal(
                "Alice",
                dto.Name);
        }

        /// <summary>
        /// Verifies that the strongly typed single-object overload rejects a <see langword="null"/> source.
        /// </summary>
        [Fact]
        public void Map_TypedOverload_ThrowsArgumentNullException_WhenSourceIsNull()
        {
            PersonEntity source = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => CreateMapper().Map<PersonEntity, PersonDto>(
                    source));

            Assert.Equal(
                "source",
                exception.ParamName);
        }

        /// <summary>
        /// Verifies that the strongly typed list overload returns an empty result when the source sequence is
        /// <see langword="null"/>.
        /// </summary>
        [Fact]
        public void MapList_TypedOverload_ReturnsEmptyList_WhenSourcesAreNull()
        {
            IEnumerable<PersonEntity> sources = null;

            var result = CreateMapper().MapList<PersonEntity, PersonDto>(
                sources);

            Assert.Empty(
                result);
        }

        /// <summary>
        /// Verifies that the strongly typed list overload skips <see langword="null"/> elements while mapping the rest of
        /// the sequence with a single cached plan.
        /// </summary>
        [Fact]
        public void MapList_TypedOverload_SkipsNullElements()
        {
            var sources = new List<PersonEntity>
            {
                new PersonEntity
                {
                    Id = 1,
                },
                null,
                new PersonEntity
                {
                    Id = 2,
                },
            };

            var result = CreateMapper().MapList<PersonEntity, PersonDto>(
                sources);

            Assert.Equal(
                2,
                result.Count);
            Assert.Equal(
                1,
                result[0].Id);
            Assert.Equal(
                2,
                result[1].Id);
        }

        /// <summary>
        /// Verifies that the strongly typed list overload maps every element in order.
        /// </summary>
        [Fact]
        public void MapList_TypedOverload_MapsAllElements()
        {
            var sources = new List<PersonEntity>
            {
                new PersonEntity
                {
                    Id = 1,
                    Name = "A",
                },
                new PersonEntity
                {
                    Id = 2,
                    Name = "B",
                },
                new PersonEntity
                {
                    Id = 3,
                    Name = "C",
                },
            };

            var result = CreateMapper().MapList<PersonEntity, PersonDto>(
                sources);

            Assert.Equal(
                3,
                result.Count);
            Assert.Equal(
                "A",
                result[0].Name);
            Assert.Equal(
                "B",
                result[1].Name);
            Assert.Equal(
                "C",
                result[2].Name);
        }
    }
}
