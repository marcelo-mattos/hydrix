using Hydrix.Mapper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Builders
{
    /// <summary>
    /// Covers private branch-heavy helpers in MapPlanBuilder through reflection-based invocation.
    /// </summary>
    public class MapPlanBuilderCoverageTests
    {
        /// <summary>
        /// Stores the runtime type for the internal MapPlanBuilder implementation.
        /// </summary>
        private static readonly Type MapPlanBuilderType = typeof(HydrixMapper).Assembly.GetType(
            "Hydrix.Mapper.Builders.MapPlanBuilder",
            throwOnError: true);

        /// <summary>
        /// Stores the private BuildNestedObjectExpression helper.
        /// </summary>
        private static readonly MethodInfo BuildNestedObjectExpressionMethod = GetRequiredPrivateStaticMethod(
            "BuildNestedObjectExpression");

        /// <summary>
        /// Stores the private BuildNestedCollectionExpression helper.
        /// </summary>
        private static readonly MethodInfo BuildNestedCollectionExpressionMethod = GetRequiredPrivateStaticMethod(
            "BuildNestedCollectionExpression");

        /// <summary>
        /// Stores the private TryGetEnumerableElementType helper.
        /// </summary>
        private static readonly MethodInfo TryGetEnumerableElementTypeMethod = GetRequiredPrivateStaticMethod(
            "TryGetEnumerableElementType");

        /// <summary>
        /// Stores the private TryGetCollectionDestElementType helper.
        /// </summary>
        private static readonly MethodInfo TryGetCollectionDestElementTypeMethod = GetRequiredPrivateStaticMethod(
            "TryGetCollectionDestElementType");

        /// <summary>
        /// Represents a nested destination element type used by nested helper coverage.
        /// </summary>
        private sealed class NestedDestination
        {
            /// <summary>
            /// Gets or sets a mapped value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Verifies that nested object expression for value-type sources returns direct invocation without null-guard branch.
        /// </summary>
        [Fact]
        public void BuildNestedObjectExpression_ValueTypeSource_ReturnsInvokeExpression()
        {
            var options = new HydrixMapperOptions();
            var source = Expression.Parameter(
                typeof(int),
                "source");

            var expression = (Expression)BuildNestedObjectExpressionMethod.Invoke(
                null,
                new object[]
                {
                    source,
                    typeof(int),
                    typeof(NestedDestination),
                    options,
                });

            var block = Assert.IsAssignableFrom<BlockExpression>(
                expression);
            Assert.NotNull(
                block);
        }

        /// <summary>
        /// Verifies that nested collection expression returns direct list expression when destination property type is
        /// exactly List of destination element.
        /// </summary>
        [Fact]
        public void BuildNestedCollectionExpression_ReturnsDirectCall_WhenDestinationIsList()
        {
            var options = new HydrixMapperOptions();
            var source = Expression.Parameter(
                typeof(List<int>),
                "source");

            var expression = (Expression)BuildNestedCollectionExpressionMethod.Invoke(
                null,
                new object[]
                {
                    source,
                    typeof(int),
                    typeof(NestedDestination),
                    typeof(List<NestedDestination>),
                    options,
                });

            Assert.NotNull(
                expression);
        }

        /// <summary>
        /// Verifies that nested collection expression inserts conversion when destination property type is not concrete
        /// List of destination element.
        /// </summary>
        [Fact]
        public void BuildNestedCollectionExpression_ReturnsConvertedExpression_WhenDestinationIsInterface()
        {
            var options = new HydrixMapperOptions();
            var source = Expression.Parameter(
                typeof(List<int>),
                "source");

            var expression = (Expression)BuildNestedCollectionExpressionMethod.Invoke(
                null,
                new object[]
                {
                    source,
                    typeof(int),
                    typeof(NestedDestination),
                    typeof(IReadOnlyList<NestedDestination>),
                    options,
                });

            Assert.NotNull(
                expression);
        }

        /// <summary>
        /// Verifies that TryGetEnumerableElementType handles direct IEnumerable of T definitions through the fast path.
        /// </summary>
        [Fact]
        public void TryGetEnumerableElementType_ReturnsTrue_ForDirectIEnumerableDefinition()
        {
            var args = new object[]
            {
                typeof(IEnumerable<int>),
                null,
            };

            var success = (bool)TryGetEnumerableElementTypeMethod.Invoke(
                null,
                args);

            Assert.True(success);
            Assert.Equal(
                typeof(int),
                args[1]);
        }

        /// <summary>
        /// Verifies that TryGetCollectionDestElementType recognizes IEnumerable destination definitions.
        /// </summary>
        [Fact]
        public void TryGetCollectionDestElementType_ReturnsTrue_ForIEnumerableDefinition()
        {
            var args = new object[]
            {
                typeof(IEnumerable<int>),
                null,
            };

            var success = (bool)TryGetCollectionDestElementTypeMethod.Invoke(
                null,
                args);

            Assert.True(success);
            Assert.Equal(
                typeof(int),
                args[1]);
        }

        /// <summary>
        /// Verifies that TryGetCollectionDestElementType returns false for unsupported generic collection definitions.
        /// </summary>
        [Fact]
        public void TryGetCollectionDestElementType_ReturnsFalse_ForUnsupportedGenericCollection()
        {
            var args = new object[]
            {
                typeof(ICollection<int>),
                null,
            };

            var success = (bool)TryGetCollectionDestElementTypeMethod.Invoke(
                null,
                args);

            Assert.False(success);
            Assert.Null(args[1]);
        }

        /// <summary>
        /// Resolves a required private static method from MapPlanBuilder.
        /// </summary>
        /// <param name="name">The method name to resolve.</param>
        /// <returns>The resolved method metadata.</returns>
        private static MethodInfo GetRequiredPrivateStaticMethod(
            string name) =>
            MapPlanBuilderType.GetMethod(
                name,
                BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException(
                $"Method '{name}' was not found on MapPlanBuilder.");
    }
}
