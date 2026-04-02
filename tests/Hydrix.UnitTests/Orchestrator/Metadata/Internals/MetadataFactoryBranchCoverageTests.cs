using Hydrix.Orchestrator.Metadata.Internals;
using Hydrix.Schemas.Contract;
using Moq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Metadata.Internals
{
    /// <summary>
    /// Provides targeted branch-coverage tests for nested-instantiation condition paths in <see cref="MetadataFactory"/>.
    /// </summary>
    public class MetadataFactoryBranchCoverageTests
    {
        /// <summary>
        /// Represents a parent object used by nested materializer coverage tests.
        /// </summary>
        private sealed class BranchParent
        {
            /// <summary>
            /// Gets or sets the nested child entity.
            /// </summary>
            public BranchChild Child { get; set; }
        }

        /// <summary>
        /// Represents a child entity materialized by nested materializer delegates.
        /// </summary>
        [Table("branch_child")]
        private sealed class BranchChild : ITable
        {
            /// <summary>
            /// Gets or sets the identifier value.
            /// </summary>
            [Column]
            public int Id { get; set; }
        }

        /// <summary>
        /// Verifies that nested materialization is skipped when candidate ordinals are null in non-primary-key mode.
        /// </summary>
        [Fact]
        public void CreateNestedEntityMaterializer_Skips_WhenCandidatesAreNull_AndPrimaryKeyModeIsDisabled()
        {
            var property = typeof(BranchParent).GetProperty(nameof(BranchParent.Child));
            var materializer = MetadataFactory.CreateNestedEntityMaterializer(
                property,
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: null,
                fieldAssigners: null);

            var parent = new BranchParent();
            var record = new Mock<IDataRecord>();

            materializer(parent, record.Object);

            Assert.Null(parent.Child);
        }

        /// <summary>
        /// Verifies that nested materialization is skipped when candidate ordinals are empty in non-primary-key mode.
        /// </summary>
        [Fact]
        public void CreateNestedEntityMaterializer_Skips_WhenCandidatesAreEmpty_AndPrimaryKeyModeIsDisabled()
        {
            var property = typeof(BranchParent).GetProperty(nameof(BranchParent.Child));
            var materializer = MetadataFactory.CreateNestedEntityMaterializer(
                property,
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: Array.Empty<int>(),
                fieldAssigners: null);

            var parent = new BranchParent();
            var record = new Mock<IDataRecord>();

            materializer(parent, record.Object);

            Assert.Null(parent.Child);
        }

        /// <summary>
        /// Verifies that nested materialization uses OR-combination across candidate ordinals and instantiates when at
        /// least one candidate is not <see cref="DBNull"/>.
        /// </summary>
        [Fact]
        public void CreateNestedEntityMaterializer_Instantiates_WhenAnyCandidateOrdinalIsNotDbNull()
        {
            var property = typeof(BranchParent).GetProperty(nameof(BranchParent.Child));
            var childProperty = typeof(BranchChild).GetProperty(nameof(BranchChild.Id));
            var fieldAssigner = MetadataFactory.CreateRecordAssigner(childProperty, 0, typeof(int));
            var materializer = MetadataFactory.CreateNestedEntityMaterializer(
                property,
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: new[] { 0, 1 },
                fieldAssigners: new[] { fieldAssigner });

            var parent = new BranchParent();
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(true);
            record.Setup(r => r.IsDBNull(1)).Returns(false);
            record.Setup(r => r.GetInt32(0)).Returns(27);

            materializer(parent, record.Object);

            Assert.NotNull(parent.Child);
            Assert.Equal(0, parent.Child.Id);
        }

        /// <summary>
        /// Verifies that nested materializer ignores null field assigners and still instantiates the nested entity.
        /// </summary>
        [Fact]
        public void CreateNestedEntityMaterializer_Instantiates_WhenFieldAssignerEntryIsNull()
        {
            var property = typeof(BranchParent).GetProperty(nameof(BranchParent.Child));
            var materializer = MetadataFactory.CreateNestedEntityMaterializer(
                property,
                usesPrimaryKey: true,
                primaryKeyOrdinal: 0,
                candidateOrdinals: null,
                fieldAssigners: new Action<object, IDataRecord>[] { null });

            var parent = new BranchParent();
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(false);

            materializer(parent, record.Object);

            Assert.NotNull(parent.Child);
            Assert.Equal(0, parent.Child.Id);
        }

        /// <summary>
        /// Verifies that nested materializer creation throws when property metadata has no declaring type.
        /// </summary>
        [Fact]
        public void CreateNestedEntityMaterializer_Throws_WhenDeclaringTypeIsNull()
        {
            var property = new Mock<System.Reflection.PropertyInfo>();
            property.SetupGet(p => p.Name).Returns("OrphanNested");
            property.SetupGet(p => p.PropertyType).Returns(typeof(BranchChild));
            property.SetupGet(p => p.DeclaringType).Returns((Type)null);

            Assert.Throws<InvalidOperationException>(() =>
                MetadataFactory.CreateNestedEntityMaterializer(
                    property.Object,
                    usesPrimaryKey: true,
                    primaryKeyOrdinal: 0,
                    candidateOrdinals: null,
                    fieldAssigners: null));
        }
    }
}
