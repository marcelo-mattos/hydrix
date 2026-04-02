using Hydrix.Orchestrator.Resolvers;
using Hydrix.Schemas.Contract;
using Moq;
using System;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Resolvers
{
    /// <summary>
    /// Contains unit tests for <see cref="ResolvedTableBindings"/> matching behavior.
    /// </summary>
    public class ResolvedTableBindingsTests
    {
        /// <summary>
        /// Represents a test table used by nested binding activators.
        /// </summary>
        private sealed class DummyTable : ITable
        { }

        /// <summary>
        /// Verifies that <see cref="ResolvedTableBindings.Matches(IDataReader)"/> returns false when reader is null.
        /// </summary>
        [Fact]
        public void Matches_ReturnsFalse_WhenReaderIsNull()
        {
            var bindings = new ResolvedTableBindings(null, null, new[] { "Id" });

            var result = bindings.Matches(null);

            Assert.False(result);
        }

        /// <summary>
        /// Verifies that <see cref="ResolvedTableBindings.Matches(IDataReader)"/> returns false when field count differs from schema snapshot.
        /// </summary>
        [Fact]
        public void Matches_ReturnsFalse_WhenFieldCountDiffers()
        {
            var bindings = new ResolvedTableBindings(null, null, new[] { "Id" });
            var reader = new Mock<IDataReader>();
            reader.SetupGet(r => r.FieldCount).Returns(2);

            var result = bindings.Matches(reader.Object);

            Assert.False(result);
        }

        /// <summary>
        /// Verifies that <see cref="ResolvedTableBindings.Matches(IDataReader)"/> returns false when a column name does not match.
        /// </summary>
        [Fact]
        public void Matches_ReturnsFalse_WhenColumnNameDiffers()
        {
            var bindings = new ResolvedTableBindings(null, null, new[] { "Id", "Name" });
            var reader = new Mock<IDataReader>();
            reader.SetupGet(r => r.FieldCount).Returns(2);
            reader.Setup(r => r.GetName(0)).Returns("Id");
            reader.Setup(r => r.GetName(1)).Returns("Different");

            var result = bindings.Matches(reader.Object);

            Assert.False(result);
        }

        /// <summary>
        /// Verifies that <see cref="ResolvedTableBindings.Matches(IDataReader)"/> accepts case-insensitive schema name matches.
        /// </summary>
        [Fact]
        public void Matches_ReturnsTrue_WhenColumnNamesMatchIgnoringCase()
        {
            var bindings = new ResolvedTableBindings(null, null, new[] { "ID", "NAME" });
            var reader = new Mock<IDataReader>();
            reader.SetupGet(r => r.FieldCount).Returns(2);
            reader.Setup(r => r.GetName(0)).Returns("id");
            reader.Setup(r => r.GetName(1)).Returns("name");

            var result = bindings.Matches(reader.Object);

            Assert.True(result);
        }

        /// <summary>
        /// Verifies that schema matching treats null column names from the reader as empty strings.
        /// </summary>
        [Fact]
        public void Matches_ReturnsTrue_WhenReaderColumnNameIsNullAndCachedNameIsEmpty()
        {
            var bindings = new ResolvedTableBindings(null, null, new[] { string.Empty });
            var reader = new Mock<IDataReader>();
            reader.SetupGet(r => r.FieldCount).Returns(1);
            reader.Setup(r => r.GetName(0)).Returns((string)null);

            var result = bindings.Matches(reader.Object);

            Assert.True(result);
        }

        /// <summary>
        /// Verifies that field type matching skips bindings with negative ordinals and keeps successful result.
        /// </summary>
        [Fact]
        public void Matches_ReturnsTrue_WhenFieldBindingOrdinalIsNegative()
        {
            var field = new ResolvedFieldBinding(null, -1, null);
            var bindings = new ResolvedTableBindings(new[] { field }, null, new[] { "Id" });
            var reader = new Mock<IDataReader>();
            reader.SetupGet(r => r.FieldCount).Returns(1);
            reader.Setup(r => r.GetName(0)).Returns("Id");

            var result = bindings.Matches(reader.Object);

            Assert.True(result);
        }

        /// <summary>
        /// Verifies that matching fails when a field source type is missing.
        /// </summary>
        [Fact]
        public void Matches_ReturnsFalse_WhenFieldSourceTypeIsNull()
        {
            var field = new ResolvedFieldBinding(null, 0, null);
            var bindings = new ResolvedTableBindings(new[] { field }, null, new[] { "Id" });
            var reader = new Mock<IDataReader>();
            reader.SetupGet(r => r.FieldCount).Returns(1);
            reader.Setup(r => r.GetName(0)).Returns("Id");

            var result = bindings.Matches(reader.Object);

            Assert.False(result);
        }

        /// <summary>
        /// Verifies that nested binding field-type validation succeeds when nested bindings are compatible.
        /// </summary>
        [Fact]
        public void Matches_ReturnsTrue_WhenNestedBindingsMatchFieldTypes()
        {
            var rootField = new ResolvedFieldBinding(null, 0, typeof(int));
            var nestedBindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                Array.Empty<ResolvedNestedBinding>(),
                Array.Empty<string>());

            var nestedBinding = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: null,
                activator: _ => new DummyTable(),
                bindings: nestedBindings);

            var bindings = new ResolvedTableBindings(
                new[] { rootField },
                new[] { nestedBinding },
                new[] { "Id" });

            var reader = new Mock<IDataReader>();
            reader.SetupGet(r => r.FieldCount).Returns(1);
            reader.Setup(r => r.GetName(0)).Returns("Id");
            reader.Setup(r => r.GetFieldType(0)).Returns((Type)null);
            reader.Setup(r => r.IsDBNull(0)).Returns(false);
            reader.Setup(r => r.GetValue(0)).Returns(1);

            var result = bindings.Matches(reader.Object);

            Assert.True(result);
        }

        /// <summary>
        /// Verifies that matching fails when provider field type differs from cached source type.
        /// </summary>
        [Fact]
        public void Matches_ReturnsFalse_WhenProviderFieldTypeDiffersFromCachedSourceType()
        {
            var field = new ResolvedFieldBinding(null, 0, typeof(int));
            var bindings = new ResolvedTableBindings(new[] { field }, null, new[] { "Id" });
            var reader = new Mock<IDataReader>();
            reader.SetupGet(r => r.FieldCount).Returns(1);
            reader.Setup(r => r.GetName(0)).Returns("Id");
            reader.Setup(r => r.GetFieldType(0)).Returns(typeof(long));

            var result = bindings.Matches(reader.Object);

            Assert.False(result);
        }

        /// <summary>
        /// Verifies that matching fails when field value runtime type differs from cached source type.
        /// </summary>
        [Fact]
        public void Matches_ReturnsFalse_WhenFieldValueTypeDiffersFromCachedSourceType()
        {
            var field = new ResolvedFieldBinding(null, 0, typeof(int));
            var bindings = new ResolvedTableBindings(new[] { field }, null, new[] { "Id" });
            var reader = new Mock<IDataReader>();
            reader.SetupGet(r => r.FieldCount).Returns(1);
            reader.Setup(r => r.GetName(0)).Returns("Id");
            reader.Setup(r => r.GetFieldType(0)).Returns((Type)null);
            reader.Setup(r => r.IsDBNull(0)).Returns(false);
            reader.Setup(r => r.GetValue(0)).Returns(1L);

            var result = bindings.Matches(reader.Object);

            Assert.False(result);
        }

        /// <summary>
        /// Verifies that matching fails when nested bindings do not match provider field types.
        /// </summary>
        [Fact]
        public void Matches_ReturnsFalse_WhenNestedBindingsDoNotMatchFieldTypes()
        {
            var rootField = new ResolvedFieldBinding(null, 0, typeof(int));
            var nestedField = new ResolvedFieldBinding(null, 1, typeof(int));
            var nestedBindings = new ResolvedTableBindings(new[] { nestedField }, null, Array.Empty<string>());
            var nestedBinding = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: null,
                activator: _ => new DummyTable(),
                bindings: nestedBindings);

            var bindings = new ResolvedTableBindings(
                new[] { rootField },
                new[] { nestedBinding },
                new[] { "Id" });

            var reader = new Mock<IDataReader>();
            reader.SetupGet(r => r.FieldCount).Returns(1);
            reader.Setup(r => r.GetName(0)).Returns("Id");
            reader.Setup(r => r.GetFieldType(0)).Returns((Type)null);
            reader.Setup(r => r.IsDBNull(0)).Returns(true);

            var result = bindings.Matches(reader.Object);

            Assert.False(result);
        }

        /// <summary>
        /// Verifies that matching tolerates unsupported provider field type metadata and continues using value checks.
        /// </summary>
        [Fact]
        public void Matches_ReturnsTrue_WhenGetFieldTypeThrowsNotSupportedException_AndValueTypeMatches()
        {
            var field = new ResolvedFieldBinding(null, 0, typeof(int));
            var bindings = new ResolvedTableBindings(new[] { field }, null, new[] { "Id" });
            var reader = new Mock<IDataReader>();
            reader.SetupGet(r => r.FieldCount).Returns(1);
            reader.Setup(r => r.GetName(0)).Returns("Id");
            reader.Setup(r => r.GetFieldType(0)).Throws(new NotSupportedException());
            reader.Setup(r => r.IsDBNull(0)).Returns(false);
            reader.Setup(r => r.GetValue(0)).Returns(1);

            var result = bindings.Matches(reader.Object);

            Assert.True(result);
        }
    }
}
