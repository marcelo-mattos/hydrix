using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Mapping
{
    /// <summary>
    /// Validates the baseline object, nullable, collection, and strict-mode behaviors exposed by the Hydrix mapper.
    /// </summary>
    /// <remarks>
    /// The strict-mode scenarios use dedicated source and destination types so the cached map plans created by previous
    /// tests do not affect the assertions in this class.
    /// </remarks>
    public class HydrixMapperBasicTests
    {
        /// <summary>
        /// Represents the source entity used by the baseline property-mapping scenarios.
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

            /// <summary>
            /// Gets or sets the optional age copied to the destination model.
            /// </summary>
            public int? Age { get; set; }

            /// <summary>
            /// Gets or sets the extra source-only value used to verify ignored source members.
            /// </summary>
            public string ExtraOnSource { get; set; }
        }

        /// <summary>
        /// Represents the source model used to verify that write-only source members are ignored when building the source lookup.
        /// </summary>
        private sealed class PersonEntityWriteOnlySource
        {
            /// <summary>
            /// Stores the value assigned through <see cref="IgnoredWriteOnly"/>.
            /// </summary>
            private string _ignoredWriteOnly;

            /// <summary>
            /// Gets or sets the identifier copied to the destination model.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name copied to the destination model.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Sets an auxiliary value that should be ignored because the property has no public getter.
            /// </summary>
            public string IgnoredWriteOnly
            {
                set => _ignoredWriteOnly = value;
            }
        }

        /// <summary>
        /// Represents the destination model used by the baseline property-mapping scenarios.
        /// </summary>
        private sealed class PersonDto
        {
            /// <summary>
            /// Gets or sets the identifier received from the source model.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name received from the source model.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the optional age received from the source model.
            /// </summary>
            public int? Age { get; set; }

            /// <summary>
            /// Gets or sets the extra destination-only member used to validate default-value behavior.
            /// </summary>
            public string ExtraOnDestination { get; set; }
        }

        /// <summary>
        /// Represents the strict-mode source model used by validations that require a fully matched destination shape.
        /// </summary>
        private sealed class PersonEntityStrict
        {
            /// <summary>
            /// Gets or sets the identifier copied to the strict destination model.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name copied to the strict destination model.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents the strict destination model that intentionally contains an unmatched property.
        /// </summary>
        private sealed class PersonDtoStrictExtra
        {
            /// <summary>
            /// Gets or sets the identifier copied from the strict source model.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name copied from the strict source model.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the extra unmatched destination property used to trigger strict-mode validation.
            /// </summary>
            public string ExtraOnDestination { get; set; }
        }

        /// <summary>
        /// Represents the strict destination model whose shape exactly matches the strict source model.
        /// </summary>
        private sealed class PersonDtoStrictNoExtra
        {
            /// <summary>
            /// Gets or sets the identifier copied from the strict source model.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name copied from the strict source model.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents the simplified destination model used to verify that extra source members are ignored.
        /// </summary>
        private sealed class PersonDtoNoExtra
        {
            /// <summary>
            /// Gets or sets the identifier copied from the source model.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name copied from the source model.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents the destination model that exposes a read-only property.
        /// </summary>
        private sealed class PersonDtoReadOnly
        {
            /// <summary>
            /// Gets or sets the identifier copied from the source model.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name copied from the source model.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets the read-only member that should retain its initializer value.
            /// </summary>
            public string ReadOnly { get; } = "readonly";
        }

        /// <summary>
        /// Represents the destination model that exposes an indexer which should be ignored during binding generation.
        /// </summary>
        private sealed class PersonDtoWithIndexer
        {
            /// <summary>
            /// Stores the values assigned through the destination indexer.
            /// </summary>
            private readonly Dictionary<int, string> _values = new Dictionary<int, string>();

            /// <summary>
            /// Gets or sets the identifier copied from the source model.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name copied from the source model.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the value associated with the specified index.
            /// </summary>
            /// <param name="index">The integer key used to address the indexed value.</param>
            /// <value>The stored string associated with <paramref name="index"/>, or <see langword="null"/> when none exists.</value>
            public string this[int index]
            {
                get => _values.TryGetValue(
                    index,
                    out var value)
                    ? value
                    : null;
                set => _values[index] = value;
            }
        }

        /// <summary>
        /// Represents the destination model that marks one property with <see cref="NotMappedAttribute"/>.
        /// </summary>
        private sealed class PersonDtoNotMapped
        {
            /// <summary>
            /// Gets or sets the identifier copied from the source model.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the ignored destination name that should not be populated by the mapper.
            /// </summary>
            [NotMapped]
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents the source model used by nullable-to-non-nullable conversion scenarios.
        /// </summary>
        private sealed class EntityWithNullableSource
        {
            /// <summary>
            /// Gets or sets the nullable integer copied to the destination model.
            /// </summary>
            public int? NullableInt { get; set; }

            /// <summary>
            /// Gets or sets the nullable string copied to the destination model.
            /// </summary>
            public string NullableString { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by nullable-to-non-nullable conversion scenarios.
        /// </summary>
        private sealed class DtoWithNonNullableDest
        {
            /// <summary>
            /// Gets or sets the non-nullable integer that receives the nullable source value.
            /// </summary>
            public int NullableInt { get; set; }

            /// <summary>
            /// Gets or sets the string that receives the nullable source value.
            /// </summary>
            public string NullableString { get; set; }
        }

        /// <summary>
        /// Represents the source model used by non-nullable-to-nullable conversion scenarios.
        /// </summary>
        private sealed class EntityWithValue
        {
            /// <summary>
            /// Gets or sets the count copied to the nullable destination property.
            /// </summary>
            public int Count { get; set; }
        }

        /// <summary>
        /// Represents the destination model used by non-nullable-to-nullable conversion scenarios.
        /// </summary>
        private sealed class DtoWithNullableDest
        {
            /// <summary>
            /// Gets or sets the nullable count receiving the non-nullable source value.
            /// </summary>
            public int? Count { get; set; }
        }

        /// <summary>
        /// Represents a destination type without a public parameterless constructor.
        /// </summary>
        private sealed class NoCtor
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="NoCtor"/> class.
            /// </summary>
            /// <param name="value">The constructor argument used only to make the type non-instantiable by the mapper.</param>
            private NoCtor(
                int value)
            {
                _ = value;
            }
        }

        /// <summary>
        /// Creates a mapper configured with the default Hydrix options.
        /// </summary>
        /// <returns>A mapper instance ready for the baseline scenarios in this class.</returns>
        private static HydrixMapper CreateMapper() =>
            new HydrixMapper(
                new HydrixMapperOptions());

        /// <summary>
        /// Verifies that members with matching names and compatible types are copied to the destination model.
        /// </summary>
        [Fact]
        public void Map_MapsMatchingPropertiesByExactName()
        {
            var entity = new PersonEntity
            {
                Id = 1,
                Name = "Alice",
                Age = 30,
            };

            var dto = CreateMapper().Map<PersonDto>(
                entity);

            Assert.Equal(
                1,
                dto.Id);
            Assert.Equal(
                "Alice",
                dto.Name);
            Assert.Equal(
                30,
                dto.Age);
        }

        /// <summary>
        /// Verifies that unmatched destination members remain at their default value when the source does not provide them.
        /// </summary>
        [Fact]
        public void Map_LeavesExtraDestinationPropertyAtDefault()
        {
            var entity = new PersonEntity
            {
                Id = 1,
                Name = "Alice",
            };

            var dto = CreateMapper().Map<PersonDto>(
                entity);

            Assert.Null(
                dto.ExtraOnDestination);
        }

        /// <summary>
        /// Verifies that extra source members do not prevent matching destination members from being populated.
        /// </summary>
        [Fact]
        public void Map_IgnoresExtraSourceProperty()
        {
            var entity = new PersonEntity
            {
                Id = 1,
                Name = "Alice",
                ExtraOnSource = "extra",
            };

            var dto = CreateMapper().Map<PersonDtoNoExtra>(
                entity);

            Assert.Equal(
                1,
                dto.Id);
            Assert.Equal(
                "Alice",
                dto.Name);
        }

        /// <summary>
        /// Verifies that write-only source properties are ignored while the remaining readable members are still mapped.
        /// </summary>
        [Fact]
        public void Map_IgnoresWriteOnlySourceProperty()
        {
            var entity = new PersonEntityWriteOnlySource
            {
                Id = 1,
                Name = "Alice",
                IgnoredWriteOnly = "secret",
            };

            var dto = CreateMapper().Map<PersonDtoNoExtra>(
                entity);

            Assert.Equal(
                1,
                dto.Id);
            Assert.Equal(
                "Alice",
                dto.Name);
        }

        /// <summary>
        /// Verifies that read-only destination properties keep their existing initialized value.
        /// </summary>
        [Fact]
        public void Map_IgnoresReadOnlyDestinationProperty()
        {
            var entity = new PersonEntity
            {
                Id = 1,
                Name = "Alice",
            };

            var dto = CreateMapper().Map<PersonDtoReadOnly>(
                entity);

            Assert.Equal(
                1,
                dto.Id);
            Assert.Equal(
                "Alice",
                dto.Name);
            Assert.Equal(
                "readonly",
                dto.ReadOnly);
        }

        /// <summary>
        /// Verifies that destination indexers are ignored while ordinary writable members continue to map successfully.
        /// </summary>
        [Fact]
        public void Map_IgnoresDestinationIndexerProperty()
        {
            var entity = new PersonEntity
            {
                Id = 1,
                Name = "Alice",
            };

            var dto = CreateMapper().Map<PersonDtoWithIndexer>(
                entity);

            Assert.Equal(
                1,
                dto.Id);
            Assert.Equal(
                "Alice",
                dto.Name);
            Assert.Null(
                dto[0]);
        }

        /// <summary>
        /// Verifies that members marked with <see cref="NotMappedAttribute"/> are skipped during mapping.
        /// </summary>
        [Fact]
        public void Map_IgnoresNotMappedDestinationProperty()
        {
            var entity = new PersonEntity
            {
                Id = 1,
                Name = "Alice",
            };

            var dto = CreateMapper().Map<PersonDtoNotMapped>(
                entity);

            Assert.Equal(
                1,
                dto.Id);
            Assert.Null(
                dto.Name);
        }

        /// <summary>
        /// Verifies that null nullable values targeting non-nullable members are replaced by the destination default value.
        /// </summary>
        [Fact]
        public void Map_NullableSourceToNonNullableDest_WhenNull_UsesDefault()
        {
            var entity = new EntityWithNullableSource
            {
                NullableInt = null,
                NullableString = null,
            };

            var dto = CreateMapper().Map<DtoWithNonNullableDest>(
                entity);

            Assert.Equal(
                0,
                dto.NullableInt);
            Assert.Null(
                dto.NullableString);
        }

        /// <summary>
        /// Verifies that nullable source values with content are copied to compatible non-nullable destination members.
        /// </summary>
        [Fact]
        public void Map_NullableSourceToNonNullableDest_WhenHasValue_MapsValue()
        {
            var entity = new EntityWithNullableSource
            {
                NullableInt = 42,
                NullableString = "hello",
            };

            var dto = CreateMapper().Map<DtoWithNonNullableDest>(
                entity);

            Assert.Equal(
                42,
                dto.NullableInt);
            Assert.Equal(
                "hello",
                dto.NullableString);
        }

        /// <summary>
        /// Verifies that non-nullable source values can be assigned to nullable destination members.
        /// </summary>
        [Fact]
        public void Map_NonNullableSourceToNullableDest_WrapsValue()
        {
            var entity = new EntityWithValue
            {
                Count = 7,
            };

            var dto = CreateMapper().Map<DtoWithNullableDest>(
                entity);

            Assert.Equal(
                7,
                dto.Count);
        }

        /// <summary>
        /// Verifies that mapping a null source throws an <see cref="ArgumentNullException"/> with the expected parameter name.
        /// </summary>
        [Fact]
        public void Map_ThrowsArgumentNullException_WhenSourceIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => CreateMapper().Map<PersonDto>(
                    null));

            Assert.Equal(
                "source",
                exception.ParamName);
        }

        /// <summary>
        /// Verifies that destination types without a public parameterless constructor are rejected.
        /// </summary>
        [Fact]
        public void Map_ThrowsInvalidOperationException_WhenDestHasNoDefaultCtor()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => CreateMapper().Map<NoCtor>(
                    new PersonEntity()));

            Assert.Contains(
                "parameterless constructor",
                exception.Message);
        }

        /// <summary>
        /// Verifies that mapping a null list returns an empty result rather than throwing an exception.
        /// </summary>
        [Fact]
        public void MapList_ReturnsEmptyList_WhenSourcesIsNull()
        {
            var result = CreateMapper().MapList<PersonDto>(
                null);

            Assert.Empty(
                result);
        }

        /// <summary>
        /// Verifies that null elements are skipped when mapping a heterogeneous object list.
        /// </summary>
        [Fact]
        public void MapList_SkipsNullElements()
        {
            var sources = new List<object>
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

            var result = CreateMapper().MapList<PersonDto>(
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
        /// Verifies that each element in the source list is mapped to the corresponding destination instance.
        /// </summary>
        [Fact]
        public void MapList_MapsAllElements()
        {
            var sources = new List<object>
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

            var result = CreateMapper().MapList<PersonDto>(
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

        /// <summary>
        /// Verifies that strict mode throws when the destination exposes a property with no matching source member.
        /// </summary>
        [Fact]
        public void StrictMode_ThrowsWhenDestPropertyHasNoSourceMatch()
        {
            var options = new HydrixMapperOptions
            {
                StrictMode = true,
            };

            var exception = Assert.Throws<InvalidOperationException>(
                () => new HydrixMapper(
                    options).Map<PersonDtoStrictExtra>(
                    new PersonEntityStrict()));

            Assert.Contains(
                "strict",
                exception.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that strict mode succeeds when every destination member has a matching source member.
        /// </summary>
        [Fact]
        public void StrictMode_Passes_WhenAllDestPropertiesMatch()
        {
            var options = new HydrixMapperOptions
            {
                StrictMode = true,
            };
            var entity = new PersonEntityStrict
            {
                Id = 1,
                Name = "Alice",
            };

            var dto = new HydrixMapper(
                options).Map<PersonDtoStrictNoExtra>(
                entity);

            Assert.Equal(
                1,
                dto.Id);
        }
    }
}

