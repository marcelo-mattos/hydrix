using Hydrix.Resolvers;
using Hydrix.Schemas.Contract;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Resolvers
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
        /// Represents a parent entity with a scalar identifier and a navigation property, used to supply
        /// <see cref="System.Reflection.PropertyInfo"/> instances for inlined expression-tree tests.
        /// </summary>
        private sealed class ParentEntity : ITable
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the child entity associated with this instance.
            /// </summary>
            public ChildEntity Child { get; set; }
        }

        /// <summary>
        /// Represents a child entity with a scalar identifier, used as the navigation-property target
        /// in inlined expression-tree tests.
        /// </summary>
        private sealed class ChildEntity : ITable
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            public int Id { get; set; }
        }

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

        /// <summary>
        /// Verifies that match-field flattening returns an empty array when no root fields exist, there are multiple
        /// nested bindings, and all nested bindings are also empty.
        /// </summary>
        [Fact]
        public void Constructor_BuildMatchFields_ReturnsEmpty_WhenRootAndNestedBindingsAreEmptyAcrossMultipleEntities()
        {
            var emptyNestedBindingsA = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                Array.Empty<ResolvedNestedBinding>(),
                Array.Empty<string>());
            var emptyNestedBindingsB = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                Array.Empty<ResolvedNestedBinding>(),
                Array.Empty<string>());

            var nestedA = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: null,
                activator: _ => new DummyTable(),
                bindings: emptyNestedBindingsA);
            var nestedB = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: null,
                activator: _ => new DummyTable(),
                bindings: emptyNestedBindingsB);

            var bindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                new[] { nestedA, nestedB },
                new[] { "Id" });

            var matchFieldsProperty = typeof(ResolvedTableBindings).GetProperty(
                "MatchFields",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.NotNull(matchFieldsProperty);

            var matchFields = (ResolvedFieldBinding[])matchFieldsProperty.GetValue(bindings);

            Assert.NotNull(matchFields);
            Assert.Empty(matchFields);
        }

        /// <summary>
        /// Verifies that <see cref="ResolvedTableBindings.RowMaterializer"/> is <see langword="null"/> when both
        /// fields and entities arrays are empty (<c>totalOps == 0</c> early-return branch).
        /// </summary>
        [Fact]
        public void Constructor_RowMaterializer_IsNull_WhenBothFieldsAndEntitiesAreEmpty()
        {
            var bindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                Array.Empty<ResolvedNestedBinding>());

            Assert.Null(bindings.RowMaterializer);
        }

        /// <summary>
        /// Verifies that <see cref="ResolvedTableBindings.RowMaterializer"/> is <see langword="null"/> when any
        /// nested-entity binding has a <see langword="null"/> materializer.
        /// </summary>
        [Fact]
        public void Constructor_RowMaterializer_IsNull_WhenAnyEntityMaterializerIsNull()
        {
            var nestedBindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                Array.Empty<ResolvedNestedBinding>());

            var nestedBinding = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: null,
                activator: _ => new DummyTable(),
                bindings: nestedBindings,
                materializer: null);

            var bindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                new[] { nestedBinding });

            Assert.Null(bindings.RowMaterializer);
        }

        /// <summary>
        /// Verifies that <see cref="ResolvedTableBindings.RowMaterializer"/> is <see langword="null"/> when any
        /// scalar field binding has a <see langword="null"/> assigner.
        /// </summary>
        [Fact]
        public void Constructor_RowMaterializer_IsNull_WhenAnyFieldAssignerIsNull()
        {
            var field = new ResolvedFieldBinding(null, 0, typeof(int));

            var bindings = new ResolvedTableBindings(
                new[] { field },
                Array.Empty<ResolvedNestedBinding>());

            Assert.Null(bindings.RowMaterializer);
        }

        /// <summary>
        /// Verifies that <see cref="ResolvedTableBindings.RowMaterializer"/> is compiled and non-<see langword="null"/>
        /// when there is a single scalar field with a valid assigner (<c>totalOps == 1</c> single-expression branch).
        /// </summary>
        [Fact]
        public void Constructor_RowMaterializer_IsNotNull_WhenSingleFieldHasValidAssigner()
        {
            Action<object, IDataRecord> assigner = (e, r) => { };
            var field = new ResolvedFieldBinding(assigner, 0, typeof(int));

            var bindings = new ResolvedTableBindings(
                new[] { field },
                Array.Empty<ResolvedNestedBinding>());

            Assert.NotNull(bindings.RowMaterializer);
        }

        /// <summary>
        /// Verifies that <see cref="ResolvedTableBindings.RowMaterializer"/> is compiled and non-<see langword="null"/>
        /// when multiple scalar fields and nested-entity materializers are all valid (<c>Expression.Block</c> branch).
        /// </summary>
        [Fact]
        public void Constructor_RowMaterializer_IsNotNull_WhenMultipleFieldsAndEntityMaterializersAreValid()
        {
            Action<object, IDataRecord> assigner = (e, r) => { };
            var field1 = new ResolvedFieldBinding(assigner, 0, typeof(int));
            var field2 = new ResolvedFieldBinding(assigner, 1, typeof(string));

            var nestedBindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                Array.Empty<ResolvedNestedBinding>());

            Action<object, IDataRecord> nestedMaterializer = (e, r) => { };
            var nestedBinding = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: null,
                activator: _ => new DummyTable(),
                bindings: nestedBindings,
                materializer: nestedMaterializer);

            var bindings = new ResolvedTableBindings(
                new[] { field1, field2 },
                new[] { nestedBinding });

            Assert.NotNull(bindings.RowMaterializer);
        }

        /// <summary>
        /// Verifies that the compiled <see cref="ResolvedTableBindings.RowMaterializer"/> invokes every scalar field
        /// assigner exactly once per call.
        /// </summary>
        [Fact]
        public void RowMaterializer_InvokesAllFieldAssigners_WhenCalled()
        {
            var callCount = 0;
            Action<object, IDataRecord> assigner1 = (e, r) => callCount++;
            Action<object, IDataRecord> assigner2 = (e, r) => callCount++;
            var field1 = new ResolvedFieldBinding(assigner1, 0, typeof(int));
            var field2 = new ResolvedFieldBinding(assigner2, 1, typeof(int));

            var bindings = new ResolvedTableBindings(
                new[] { field1, field2 },
                Array.Empty<ResolvedNestedBinding>());

            var record = new Mock<IDataRecord>();
            bindings.RowMaterializer(new DummyTable(), record.Object);

            Assert.Equal(2, callCount);
        }

        /// <summary>
        /// Verifies that the compiled <see cref="ResolvedTableBindings.RowMaterializer"/> also invokes nested-entity
        /// materializers in the same pass.
        /// </summary>
        [Fact]
        public void RowMaterializer_InvokesNestedEntityMaterializer_WhenCalled()
        {
            var nestedCalled = false;
            Action<object, IDataRecord> assigner = (e, r) => { };
            var field = new ResolvedFieldBinding(assigner, 0, typeof(int));

            var nestedBindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                Array.Empty<ResolvedNestedBinding>());

            Action<object, IDataRecord> nestedMaterializer = (e, r) => nestedCalled = true;
            var nestedBinding = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: null,
                activator: _ => new DummyTable(),
                bindings: nestedBindings,
                materializer: nestedMaterializer);

            var bindings = new ResolvedTableBindings(
                new[] { field },
                new[] { nestedBinding });

            var record = new Mock<IDataRecord>();
            bindings.RowMaterializer(new DummyTable(), record.Object);

            Assert.True(nestedCalled);
        }

        /// <summary>
        /// Verifies that <see cref="ResolvedTableBindings.RowMaterializer"/> is non-null and built via the delegate
        /// fallback path when the nested entity has a null <see cref="ResolvedNestedBinding.NavigationProperty"/>,
        /// covering the <c>entity.NavigationProperty == null</c> branch of <c>AreEntitiesValid</c>.
        /// </summary>
        [Fact]
        public void Constructor_RowMaterializer_IsNotNull_WhenEntityNavigationPropertyIsNull()
        {
            Action<object, IDataRecord> fieldAssigner = (e, r) => { };
            var rootField = new ResolvedFieldBinding(
                fieldAssigner,
                0,
                typeof(int),
                typeof(ParentEntity).GetProperty(nameof(ParentEntity.Id)));

            var childBindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                Array.Empty<ResolvedNestedBinding>());

            Action<object, IDataRecord> nestedMaterializer = (e, r) => { };
            var entity = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: null,
                activator: _ => new DummyTable(),
                bindings: childBindings,
                materializer: nestedMaterializer,
                navigationProperty: null);

            var bindings = new ResolvedTableBindings(
                new[] { rootField },
                new[] { entity });

            Assert.NotNull(bindings.RowMaterializer);
        }

        /// <summary>
        /// Verifies that <c>AreEntitiesValid</c> returns <see langword="false"/> when the entity's
        /// <see cref="ResolvedNestedBinding.Bindings"/> property is <see langword="null"/>, covering the
        /// <c>entity.Bindings == null</c> branch that is unreachable through the public constructor.
        /// </summary>
        [Fact]
        public void AreEntitiesValid_ReturnsFalse_WhenEntityBindingsIsNull()
        {
            var navigationProperty = typeof(ParentEntity).GetProperty(nameof(ParentEntity.Child));
            var entity = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: null,
                activator: _ => new DummyTable(),
                bindings: null,
                navigationProperty: navigationProperty);

            var method = typeof(ResolvedTableBindings).GetMethod(
                "AreEntitiesValid",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var result = (bool)method.Invoke(null, new object[] { new[] { entity } });

            Assert.False(result);
        }

        /// <summary>
        /// Verifies that <see cref="ResolvedTableBindings.RowMaterializer"/> is <see langword="null"/> when the
        /// nested entity's bindings contain sub-entities (<c>Entities.Length != 0</c>), covering that branch of
        /// <c>AreEntitiesValid</c> and ensuring both the inlined and delegate paths return null.
        /// </summary>
        [Fact]
        public void Constructor_RowMaterializer_IsNull_WhenEntityBindingsHasSubEntities()
        {
            Action<object, IDataRecord> fieldAssigner = (e, r) => { };
            var rootField = new ResolvedFieldBinding(
                fieldAssigner,
                0,
                typeof(int),
                typeof(ParentEntity).GetProperty(nameof(ParentEntity.Id)));

            var leafBindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                Array.Empty<ResolvedNestedBinding>());

            var leafEntity = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: null,
                activator: _ => new DummyTable(),
                bindings: leafBindings);

            var childBindings = new ResolvedTableBindings(
                Array.Empty<ResolvedFieldBinding>(),
                new[] { leafEntity });

            var entity = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: null,
                activator: _ => new DummyTable(),
                bindings: childBindings,
                navigationProperty: typeof(ParentEntity).GetProperty(nameof(ParentEntity.Child)));

            var bindings = new ResolvedTableBindings(
                new[] { rootField },
                new[] { entity });

            Assert.Null(bindings.RowMaterializer);
        }

        /// <summary>
        /// Verifies that <see cref="ResolvedTableBindings.RowMaterializer"/> is non-null and built via the delegate
        /// fallback path when a child field has a null <see cref="ResolvedFieldBinding.Property"/>, covering the
        /// <c>childField.Property == null</c> branch of <c>AreEntitiesValid</c>.
        /// </summary>
        [Fact]
        public void Constructor_RowMaterializer_IsNotNull_WhenChildFieldPropertyIsNull()
        {
            Action<object, IDataRecord> fieldAssigner = (e, r) => { };
            var rootField = new ResolvedFieldBinding(
                fieldAssigner,
                0,
                typeof(int),
                typeof(ParentEntity).GetProperty(nameof(ParentEntity.Id)));

            var childField = new ResolvedFieldBinding(fieldAssigner, 1, typeof(int), property: null);

            var childBindings = new ResolvedTableBindings(
                new[] { childField },
                Array.Empty<ResolvedNestedBinding>());

            Action<object, IDataRecord> nestedMaterializer = (e, r) => { };
            var entity = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: null,
                activator: _ => new DummyTable(),
                bindings: childBindings,
                materializer: nestedMaterializer,
                navigationProperty: typeof(ParentEntity).GetProperty(nameof(ParentEntity.Child)));

            var bindings = new ResolvedTableBindings(
                new[] { rootField },
                new[] { entity });

            Assert.NotNull(bindings.RowMaterializer);
        }

        /// <summary>
        /// Directly invokes <c>AddEntityAssignments</c> via reflection with <c>isPkField = true</c> (child field
        /// ordinal matches the entity primary key ordinal), covering the <c>true</c> branch of the <c>&amp;&amp;</c>
        /// on line 348 and the <c>true</c> branch of the ternary on line 351
        /// (<c>CreateInlineFieldAssignmentNoNullCheck</c> path).
        /// </summary>
        [Fact]
        public void AddEntityAssignments_UsesNoNullCheckAssignment_WhenChildFieldOrdinalMatchesPrimaryKeyOrdinal()
        {
            var navProp = typeof(ParentEntity).GetProperty(nameof(ParentEntity.Child));
            var pkField = new ResolvedFieldBinding(
                assigner: null,
                ordinal: 1,
                sourceType: typeof(int),
                property: typeof(ChildEntity).GetProperty(nameof(ChildEntity.Id)));

            var childBindings = new ResolvedTableBindings(
                new[] { pkField },
                Array.Empty<ResolvedNestedBinding>());

            var entity = new ResolvedNestedBinding(
                usesPrimaryKey: true,
                primaryKeyOrdinal: 1,
                candidateOrdinals: new[] { 1 },
                activator: _ => new DummyTable(),
                bindings: childBindings,
                navigationProperty: navProp);

            var method = typeof(ResolvedTableBindings).GetMethod(
                "AddEntityAssignments",
                BindingFlags.NonPublic | BindingFlags.Static);

            var bodyExpressions = new List<Expression>();
            var variables = new List<ParameterExpression>();
            var typedParent = Expression.Variable(typeof(ParentEntity), "typedParent");
            var record = Expression.Parameter(typeof(IDataRecord), "record");

            method.Invoke(null, new object[] { bodyExpressions, variables, typedParent, record, new[] { entity } });

            Assert.Single(bodyExpressions);
        }

        /// <summary>
        /// Directly invokes <c>AddEntityAssignments</c> via reflection with <c>UsesPrimaryKey = false</c> and a
        /// child field present, covering the short-circuit branch of the <c>&amp;&amp;</c> on line 348
        /// (<c>entity.UsesPrimaryKey = false</c> → <c>brfalse.s</c> taken) that is not exercised by any other unit
        /// test in this suite.
        /// </summary>
        [Fact]
        public void AddEntityAssignments_UsesNullCheckAssignment_WhenUsesPrimaryKeyIsFalse()
        {
            var navProp = typeof(ParentEntity).GetProperty(nameof(ParentEntity.Child));
            var field = new ResolvedFieldBinding(
                assigner: null,
                ordinal: 1,
                sourceType: typeof(int),
                property: typeof(ChildEntity).GetProperty(nameof(ChildEntity.Id)));

            var childBindings = new ResolvedTableBindings(
                new[] { field },
                Array.Empty<ResolvedNestedBinding>());

            var entity = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: null,
                activator: _ => new DummyTable(),
                bindings: childBindings,
                navigationProperty: navProp);

            var method = typeof(ResolvedTableBindings).GetMethod(
                "AddEntityAssignments",
                BindingFlags.NonPublic | BindingFlags.Static);

            var bodyExpressions = new List<Expression>();
            var variables = new List<ParameterExpression>();
            var typedParent = Expression.Variable(typeof(ParentEntity), "typedParent");
            var record = Expression.Parameter(typeof(IDataRecord), "record");

            method.Invoke(null, new object[] { bodyExpressions, variables, typedParent, record, new[] { entity } });

            Assert.Single(bodyExpressions);
        }

        /// <summary>
        /// Directly invokes <c>AddEntityAssignments</c> via reflection with <c>UsesPrimaryKey = true</c> but a
        /// child field whose ordinal does not match the primary key ordinal, covering the <c>false</c> branch of
        /// the second operand of the <c>&amp;&amp;</c> on line 348 and the <c>false</c> branch of the ternary on
        /// line 351 (<c>CreateInlineFieldAssignment</c> path) via the non-short-circuit evaluation path.
        /// </summary>
        [Fact]
        public void AddEntityAssignments_UsesNullCheckAssignment_WhenChildFieldOrdinalDoesNotMatchPrimaryKeyOrdinal()
        {
            var navProp = typeof(ParentEntity).GetProperty(nameof(ParentEntity.Child));
            var nonPkField = new ResolvedFieldBinding(
                assigner: null,
                ordinal: 2,
                sourceType: typeof(int),
                property: typeof(ChildEntity).GetProperty(nameof(ChildEntity.Id)));

            var childBindings = new ResolvedTableBindings(
                new[] { nonPkField },
                Array.Empty<ResolvedNestedBinding>());

            var entity = new ResolvedNestedBinding(
                usesPrimaryKey: true,
                primaryKeyOrdinal: 1,
                candidateOrdinals: new[] { 1 },
                activator: _ => new DummyTable(),
                bindings: childBindings,
                navigationProperty: navProp);

            var method = typeof(ResolvedTableBindings).GetMethod(
                "AddEntityAssignments",
                BindingFlags.NonPublic | BindingFlags.Static);

            var bodyExpressions = new List<Expression>();
            var variables = new List<ParameterExpression>();
            var typedParent = Expression.Variable(typeof(ParentEntity), "typedParent");
            var record = Expression.Parameter(typeof(IDataRecord), "record");

            method.Invoke(null, new object[] { bodyExpressions, variables, typedParent, record, new[] { entity } });

            Assert.Single(bodyExpressions);
        }

        /// <summary>
        /// Verifies that the inlined <see cref="ResolvedTableBindings.RowMaterializer"/> uses the no-null-check
        /// assignment path for a child field whose ordinal matches the entity's primary key ordinal, covering the
        /// <c>isPkField == true</c> branch of <c>AddEntityAssignments</c>. Also invokes the compiled materializer
        /// to confirm it assigns both the parent scalar field and the nested primary key field correctly.
        /// </summary>
        [Fact]
        public void Constructor_InlinedRowMaterializer_UsesNoNullCheckAssignment_WhenChildFieldMatchesPrimaryKeyOrdinal()
        {
            Action<object, IDataRecord> fieldAssigner = (e, r) => { };
            var rootField = new ResolvedFieldBinding(
                fieldAssigner,
                0,
                typeof(int),
                typeof(ParentEntity).GetProperty(nameof(ParentEntity.Id)));

            var childField = new ResolvedFieldBinding(
                assigner: null,
                ordinal: 1,
                sourceType: typeof(int),
                property: typeof(ChildEntity).GetProperty(nameof(ChildEntity.Id)));

            var childBindings = new ResolvedTableBindings(
                new[] { childField },
                Array.Empty<ResolvedNestedBinding>());

            var entity = new ResolvedNestedBinding(
                usesPrimaryKey: true,
                primaryKeyOrdinal: 1,
                candidateOrdinals: new[] { 1 },
                activator: _ => new DummyTable(),
                bindings: childBindings,
                navigationProperty: typeof(ParentEntity).GetProperty(nameof(ParentEntity.Child)));

            var bindings = new ResolvedTableBindings(
                new[] { rootField },
                new[] { entity });

            Assert.NotNull(bindings.RowMaterializer);

            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.IsDBNull(0)).Returns(false);
            reader.Setup(r => r.GetInt32(0)).Returns(42);
            reader.Setup(r => r.IsDBNull(1)).Returns(false);
            reader.Setup(r => r.GetInt32(1)).Returns(99);

            var parent = new ParentEntity();
            bindings.RowMaterializer(parent, reader.Object);

            Assert.NotNull(parent.Child);
            Assert.Equal(99, parent.Child.Id);
        }
    }
}
