using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Extensions;
using Hydrix.Mapper.Primitives;
using System;
using System.Collections.Generic;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Extensions
{
    /// <summary>
    /// Validates the convenience extension methods that map objects and collections through the globally configured mapper.
    /// </summary>
    /// <remarks>
    /// These tests run sequentially because they intentionally replace the process-wide default mapper configuration.
    /// Distinct source and destination types are used across scenarios so the compiled plan cache cannot leak between tests.
    /// </remarks>
    [Collection(GlobalStateTestCollection.Name)]
    public class MapperExtensionsTests : IDisposable
    {
        /// <summary>
        /// Represents the primary entity type used by direct ToDto mapping scenarios.
        /// </summary>
        private sealed class EntityA
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
        /// Represents the primary DTO type used by direct ToDto mapping scenarios.
        /// </summary>
        private sealed class DtoA
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
        /// Represents the entity type used by collection-mapping scenarios.
        /// </summary>
        private sealed class EntityB
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
        /// Represents the DTO type used by collection-mapping scenarios.
        /// </summary>
        private sealed class DtoB
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
        /// Represents an entity used to validate explicit global uppercase behavior without reusing an older plan.
        /// </summary>
        private sealed class EntityUpper
        {
            /// <summary>
            /// Gets or sets the source text consumed by the uppercase scenario.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents the DTO used to validate explicit global uppercase behavior without reusing an older plan.
        /// </summary>
        private sealed class DtoUpper
        {
            /// <summary>
            /// Gets or sets the mapped text.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Resets the process-wide mapper configuration before each test instance performs assertions.
        /// </summary>
        public MapperExtensionsTests()
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
        /// Verifies that <see cref="MapperExtensions.ToDto{TDestination}(object)"/> maps matching properties by using the
        /// default global mapper.
        /// </summary>
        [Fact]
        public void ToDto_MapsEntityToDto()
        {
            var entity = new EntityA
            {
                Id = 1,
                Name = "Test",
            };

            var dto = entity.ToDto<DtoA>();

            Assert.Equal(
                1,
                dto.Id);
            Assert.Equal(
                "Test",
                dto.Name);
        }

        /// <summary>
        /// Verifies that <see cref="MapperExtensions.ToDto{TDestination}(object)"/> rejects a <see langword="null"/> source.
        /// </summary>
        [Fact]
        public void ToDto_ThrowsArgumentNullException_WhenSourceIsNull()
        {
            object source = null;

            Assert.Throws<ArgumentNullException>(
                () => source.ToDto<DtoA>());
        }

        /// <summary>
        /// Verifies that <see cref="MapperExtensions.ToDtoList{TDestination}(IEnumerable{object})"/> maps every source item
        /// in order.
        /// </summary>
        [Fact]
        public void ToDtoList_MapsCollection()
        {
            var entities = new List<object>
            {
                new EntityB
                {
                    Id = 1,
                    Name = "A",
                },
                new EntityB
                {
                    Id = 2,
                    Name = "B",
                },
            };

            var dtos = entities.ToDtoList<DtoB>();

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
        /// Verifies that <see cref="MapperExtensions.ToDtoList{TDestination}(IEnumerable{object})"/> returns an empty list
        /// when the source sequence is <see langword="null"/>.
        /// </summary>
        [Fact]
        public void ToDtoList_ReturnsEmptyList_WhenSourceIsNull()
        {
            IEnumerable<object> source = null;

            var result = source.ToDtoList<DtoA>();

            Assert.Empty(
                result);
        }

        /// <summary>
        /// Verifies that the global mapper configured explicitly is used by the ToDto extension.
        /// </summary>
        [Fact]
        public void ToDto_UsesDefaultMapper_ConfiguredExplicitly()
        {
            HydrixMapperGlobalConfiguration.Configure(
                options => options.String.Transform = StringTransforms.Uppercase);

            var entity = new EntityUpper
            {
                Name = "hello",
            };

            var dto = entity.ToDto<DtoUpper>();

            Assert.Equal(
                "HELLO",
                dto.Name);
        }

        /// <summary>
        /// Verifies that the explicit global configuration API also honors the direct options overload and captures a
        /// snapshot rather than observing later mutations.
        /// </summary>
        [Fact]
        public void ToDto_UsesDefaultMapper_ConfiguredViaOptionsSnapshot()
        {
            var options = new HydrixMapperOptions();
            options.String.Transform = StringTransforms.Lowercase;

            HydrixMapperGlobalConfiguration.Configure(
                options);

            options.String.Transform = StringTransforms.Uppercase;

            var dto = new EntityUpper
            {
                Name = "Hello",
            }.ToDto<DtoUpper>();

            Assert.Equal(
                "hello",
                dto.Name);
        }

        /// <summary>
        /// Verifies that repeated ToDto calls reuse the cached default mapper after it has been configured explicitly.
        /// </summary>
        [Fact]
        public void ToDto_ReusesCachedDefaultMapper_OnSubsequentCalls()
        {
            HydrixMapperGlobalConfiguration.Configure(
                options => options.String.Transform = StringTransforms.Uppercase);

            var first = new EntityUpper
            {
                Name = "first",
            }.ToDto<DtoUpper>();
            var second = new EntityUpper
            {
                Name = "second",
            }.ToDto<DtoUpper>();

            Assert.Equal(
                "FIRST",
                first.Name);
            Assert.Equal(
                "SECOND",
                second.Name);
        }
    }
}
