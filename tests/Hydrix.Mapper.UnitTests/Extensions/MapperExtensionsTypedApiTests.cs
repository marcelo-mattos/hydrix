using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Extensions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Extensions
{
    /// <summary>
    /// Validates the strongly typed convenience extension methods that route through the default mapper.
    /// </summary>
    [Collection(GlobalStateTestCollection.Name)]
    public class MapperExtensionsTypedApiTests : IDisposable
    {
        /// <summary>
        /// Represents the source type shared by the typed extension scenarios.
        /// </summary>
        private sealed class EntityModel
        {
            /// <summary>
            /// Gets or sets the identifier copied into the destination model.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the text copied into the destination model.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents the destination type shared by the typed extension scenarios.
        /// </summary>
        private sealed class DestinationModel
        {
            /// <summary>
            /// Gets or sets the mapped identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the mapped text.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Restores the process-wide mapper configuration before each test instance performs assertions.
        /// </summary>
        public MapperExtensionsTypedApiTests()
        {
            HydrixMapperConfiguration.Configure(
                new HydrixMapperOptions());
        }

        /// <summary>
        /// Restores the process-wide mapper configuration after each test completes.
        /// </summary>
        public void Dispose()
        {
            HydrixMapperConfiguration.Configure(
                new HydrixMapperOptions());
        }

        /// <summary>
        /// Verifies that the strongly typed single-object extension maps matching properties correctly.
        /// </summary>
        [Fact]
        public void ToDto_TypedOverload_MapsEntityToDto()
        {
            var entity = new EntityModel
            {
                Id = 1,
                Name = "Test",
            };

            var dto = entity.ToDto<EntityModel, DestinationModel>();

            Assert.Equal(
                1,
                dto.Id);
            Assert.Equal(
                "Test",
                dto.Name);
        }

        /// <summary>
        /// Verifies that the strongly typed list extension maps every element in order.
        /// </summary>
        [Fact]
        public void ToDtoList_TypedOverload_MapsCollection()
        {
            var entities = new List<EntityModel>
            {
                new EntityModel
                {
                    Id = 1,
                    Name = "A",
                },
                new EntityModel
                {
                    Id = 2,
                    Name = "B",
                },
            };

            var dtos = entities.ToDtoList<EntityModel, DestinationModel>();

            Assert.Equal(
                2,
                dtos.Count);
            Assert.Equal(
                "A",
                dtos[0].Name);
            Assert.Equal(
                "B",
                dtos[1].Name);
        }

        /// <summary>
        /// Verifies that the strongly typed list extension returns an empty list when the source sequence is
        /// <see langword="null"/>.
        /// </summary>
        [Fact]
        public void ToDtoList_TypedOverload_ReturnsEmptyList_WhenSourceIsNull()
        {
            IEnumerable<EntityModel> source = null;

            var result = source.ToDtoList<EntityModel, DestinationModel>();

            Assert.Empty(
                result);
        }

        /// <summary>
        /// Verifies that the strongly typed single-object extension rejects a <see langword="null"/> source.
        /// </summary>
        [Fact]
        public void ToDto_TypedOverload_ThrowsArgumentNullException_WhenSourceIsNull()
        {
            EntityModel source = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => source.ToDto<EntityModel, DestinationModel>());

            Assert.Equal(
                "source",
                exception.ParamName);
        }
    }
}
