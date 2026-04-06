using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Mapping;
using System.Collections.Generic;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Mapping
{
    /// <summary>
    /// Covers release-blocking regression scenarios for heterogeneous lists and non-public property accessors.
    /// </summary>
    public class HydrixMapperRegressionTests
    {
        /// <summary>
        /// Represents the first concrete source type used by the heterogeneous-list scenario.
        /// </summary>
        private sealed class PersonEntityA
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
        /// Represents the second concrete source type used by the heterogeneous-list scenario.
        /// </summary>
        private sealed class PersonEntityB
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
        /// Represents the destination model shared by the heterogeneous-list scenario.
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
        /// Represents a source type that exposes a property with a non-public getter.
        /// </summary>
        private sealed class SourceWithPrivateGetter
        {
            /// <summary>
            /// Gets or sets the identifier copied to the destination model.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the hidden value that should be ignored because the getter is not public.
            /// </summary>
            public string Hidden { private get; set; }
        }

        /// <summary>
        /// Represents a destination type that can receive the value from <see cref="SourceWithPrivateGetter.Hidden"/> if
        /// the source property is not filtered correctly.
        /// </summary>
        private sealed class DestinationWithVisibleHidden
        {
            /// <summary>
            /// Gets or sets the mapped identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the mapped hidden value.
            /// </summary>
            public string Hidden { get; set; }
        }

        /// <summary>
        /// Represents a source type used to validate that non-public destination setters are ignored.
        /// </summary>
        private sealed class SourceWithVisibleHidden
        {
            /// <summary>
            /// Gets or sets the identifier copied to the destination model.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the hidden value that should be ignored because the destination setter is not public.
            /// </summary>
            public string Hidden { get; set; }
        }

        /// <summary>
        /// Represents a destination type that exposes a non-public setter.
        /// </summary>
        private sealed class DestinationWithPrivateSetter
        {
            /// <summary>
            /// Gets or sets the mapped identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets the hidden value that should keep its initializer because the setter is not public.
            /// </summary>
            public string Hidden { get; private set; } = "kept";
        }

        /// <summary>
        /// Creates a mapper configured with the default Hydrix options.
        /// </summary>
        /// <returns>A mapper instance ready for the regression scenarios in this class.</returns>
        private static HydrixMapper CreateMapper() =>
            new HydrixMapper(
                new HydrixMapperOptions());

        /// <summary>
        /// Verifies that list mapping resolves and reuses plans per concrete source type instead of assuming a homogeneous
        /// sequence shape.
        /// </summary>
        [Fact]
        public void MapList_MapsHeterogeneousConcreteSourceTypes()
        {
            var sources = new List<object>
            {
                new PersonEntityA
                {
                    Id = 1,
                    Name = "A",
                },
                new PersonEntityB
                {
                    Id = 2,
                    Name = "B",
                },
            };

            var result = CreateMapper().MapList<PersonDto>(
                sources);

            Assert.Equal(
                2,
                result.Count);
            Assert.Equal(
                1,
                result[0].Id);
            Assert.Equal(
                "A",
                result[0].Name);
            Assert.Equal(
                2,
                result[1].Id);
            Assert.Equal(
                "B",
                result[1].Name);
        }

        /// <summary>
        /// Verifies that source properties whose getter is not public are ignored during plan compilation.
        /// </summary>
        [Fact]
        public void Map_IgnoresSourceProperty_WithNonPublicGetter()
        {
            var source = new SourceWithPrivateGetter
            {
                Id = 7,
                Hidden = "secret",
            };

            var destination = CreateMapper().Map<DestinationWithVisibleHidden>(
                source);

            Assert.Equal(
                7,
                destination.Id);
            Assert.Null(
                destination.Hidden);
        }

        /// <summary>
        /// Verifies that destination properties whose setter is not public are ignored during plan compilation.
        /// </summary>
        [Fact]
        public void Map_IgnoresDestinationProperty_WithNonPublicSetter()
        {
            var source = new SourceWithVisibleHidden
            {
                Id = 9,
                Hidden = "secret",
            };

            var destination = CreateMapper().Map<DestinationWithPrivateSetter>(
                source);

            Assert.Equal(
                9,
                destination.Id);
            Assert.Equal(
                "kept",
                destination.Hidden);
        }
    }
}
