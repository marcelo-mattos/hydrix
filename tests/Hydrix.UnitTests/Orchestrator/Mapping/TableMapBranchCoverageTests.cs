using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Resolvers;
using Hydrix.Schemas.Contract;
using Moq;
using System;
using System.Data;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Mapping
{
    /// <summary>
    /// Provides targeted branch-coverage tests for private nested-entity materialization paths in <see cref="TableMap"/>.
    /// </summary>
    public class TableMapBranchCoverageTests
    {
        /// <summary>
        /// Represents a parent table used by nested-binding branch tests.
        /// </summary>
        private sealed class BranchParent : ITable
        {
            /// <summary>
            /// Gets or sets the child table instance assigned during nested materialization.
            /// </summary>
            public BranchChild Child { get; set; }
        }

        /// <summary>
        /// Represents a child table used by nested-binding branch tests.
        /// </summary>
        private sealed class BranchChild : ITable
        {
            /// <summary>
            /// Gets or sets the identifier value for the child table.
            /// </summary>
            public int Id { get; set; }
        }

        /// <summary>
        /// Verifies that <c>SetResolvedEntityNestedEntities</c> skips nested activation when no fast-path materializer
        /// is available and instantiation preconditions evaluate to false.
        /// </summary>
        [Fact]
        public void SetResolvedEntityNestedEntities_SkipsActivation_WhenShouldInstantiateIsFalse_AndMaterializerIsNull()
        {
            var parent = new BranchParent();
            var record = new Mock<IDataRecord>();
            var nestedBindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                Array.Empty<ResolvedNestedBinding>());

            var activatorCalls = 0;
            var nestedBinding = new ResolvedNestedBinding(
                usesPrimaryKey: true,
                primaryKeyOrdinal: -1,
                candidateOrdinals: Array.Empty<int>(),
                activator: _ =>
                {
                    activatorCalls++;
                    return new BranchChild();
                },
                bindings: nestedBindings,
                materializer: null);

            typeof(TableMap)
                .GetMethod("SetResolvedEntityNestedEntities", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { parent, record.Object, new[] { nestedBinding } });

            Assert.Equal(0, activatorCalls);
            Assert.Null(parent.Child);
        }

        /// <summary>
        /// Verifies that <c>SetResolvedEntityNestedEntities</c> evaluates the non-primary-key branch and instantiates
        /// a nested entity when at least one candidate ordinal is not <see cref="DBNull"/>.
        /// </summary>
        [Fact]
        public void SetResolvedEntityNestedEntities_Instantiates_WhenUsesPrimaryKeyIsFalse_AndAnyCandidateIsNotDbNull()
        {
            var parent = new BranchParent();
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(true);
            record.Setup(r => r.IsDBNull(1)).Returns(false);

            var nestedBindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                Array.Empty<ResolvedNestedBinding>());

            var activatorCalls = 0;
            var nestedBinding = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: new[] { 0, 1 },
                activator: entity =>
                {
                    activatorCalls++;
                    var child = new BranchChild();
                    ((BranchParent)entity).Child = child;
                    return child;
                },
                bindings: nestedBindings,
                materializer: null);

            typeof(TableMap)
                .GetMethod("SetResolvedEntityNestedEntities", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { parent, record.Object, new[] { nestedBinding } });

            Assert.Equal(1, activatorCalls);
            Assert.NotNull(parent.Child);
        }

        /// <summary>
        /// Verifies that <c>HasAnyNonDbNull</c> returns <see langword="true"/> when at least one ordinal contains a
        /// non-<see cref="DBNull"/> value.
        /// </summary>
        [Fact]
        public void HasAnyNonDbNull_ReturnsTrue_WhenAnyOrdinalIsNotDbNull()
        {
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(true);
            record.Setup(r => r.IsDBNull(1)).Returns(false);

            var method = typeof(TableMap).GetMethod(
                "HasAnyNonDbNull",
                BindingFlags.NonPublic | BindingFlags.Static);

            var result = (bool)method.Invoke(
                null,
                new object[] { record.Object, new[] { 0, 1 } });

            Assert.True(result);
        }

        /// <summary>
        /// Verifies that <c>HasAnyNonDbNull</c> returns <see langword="false"/> when all inspected ordinals contain
        /// <see cref="DBNull"/> values.
        /// </summary>
        [Fact]
        public void HasAnyNonDbNull_ReturnsFalse_WhenAllOrdinalsAreDbNull()
        {
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(true);
            record.Setup(r => r.IsDBNull(1)).Returns(true);

            var method = typeof(TableMap).GetMethod(
                "HasAnyNonDbNull",
                BindingFlags.NonPublic | BindingFlags.Static);

            var result = (bool)method.Invoke(
                null,
                new object[] { record.Object, new[] { 0, 1 } });

            Assert.False(result);
        }
    }
}
