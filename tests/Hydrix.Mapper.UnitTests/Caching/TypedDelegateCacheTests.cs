using Hydrix.Mapper.Configuration;
using System;
using System.Reflection;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Caching
{
    /// <summary>
    /// Covers the strongly typed delegate cache used by the generic mapper hot path.
    /// </summary>
    public class TypedDelegateCacheTests
    {
        /// <summary>
        /// Represents the source type used by the cache scenarios.
        /// </summary>
        private sealed class SourceModel
        {
            /// <summary>
            /// Gets or sets the source name copied into the destination model.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Represents the destination type used by the cache scenarios.
        /// </summary>
        private sealed class DestinationModel
        {
            /// <summary>
            /// Gets or sets the mapped name.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Verifies that the cache returns the same strongly typed delegate instance for repeated requests against the
        /// same mapper and type pair.
        /// </summary>
        [Fact]
        public void GetOrAdd_ReturnsSameDelegate_ForSameMapperAndTypePair()
        {
            var mapper = new HydrixMapper(
                new HydrixMapperOptions());
            var method = GetTypedDelegateCacheMethod();

            var first = (Delegate)method.Invoke(
                null,
                new object[] { mapper });
            var second = (Delegate)method.Invoke(
                null,
                new object[] { mapper });

            Assert.Same(
                first,
                second);
        }

        /// <summary>
        /// Verifies that the cache rejects a <see langword="null"/> mapper instance.
        /// </summary>
        [Fact]
        public void GetOrAdd_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            var method = GetTypedDelegateCacheMethod();

            var exception = Assert.Throws<TargetInvocationException>(
                () => method.Invoke(
                    null,
                    new object[] { null }));

            var inner = Assert.IsType<ArgumentNullException>(
                exception.InnerException);

            Assert.Equal(
                "mapper",
                inner.ParamName);
        }

        /// <summary>
        /// Resolves the closed generic cache method used by the current test type pair.
        /// </summary>
        /// <returns>The <c>GetOrAdd</c> method on the closed generic cache type.</returns>
        private static MethodInfo GetTypedDelegateCacheMethod()
        {
            var genericType = typeof(HydrixMapper).Assembly.GetType(
                "Hydrix.Mapper.Caching.TypedDelegateCache`2",
                throwOnError: true);
            var closedType = genericType.MakeGenericType(
                typeof(SourceModel),
                typeof(DestinationModel));

            return closedType.GetMethod(
                "GetOrAdd",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) ?? throw new InvalidOperationException(
                "Typed delegate cache method was not found.");
        }
    }
}
