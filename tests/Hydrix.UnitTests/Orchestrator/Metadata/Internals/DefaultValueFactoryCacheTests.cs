using Hydrix.Orchestrator.Metadata.Internals;
using System;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Metadata.Internals
{
    /// <summary>
    /// Contains unit tests for the DefaultValueFactoryCache class to verify that it returns correct default value
    /// factories for various types.
    /// </summary>
    /// <remarks>These tests ensure that DefaultValueFactoryCache provides default value delegates that return
    /// null for reference types, default values for value types, and that the same delegate instance is returned for
    /// repeated requests of the same type. The tests cover both built-in and custom types to validate consistent
    /// behavior.</remarks>
    public class DefaultValueFactoryCacheTests
    {
        /// <summary>
        /// Custom struct for DefaultValueFactoryCache test.
        /// </summary>
        private struct MyStruct
        {
            /// <summary>
            /// Gets or sets the value of X.
            /// </summary>
            public int X { get; set; }
        }

        /// <summary>
        /// Custom class for DefaultValueFactoryCache test.
        /// </summary>
        private class MyClass
        {
            /// <summary>
            /// Gets or sets the Y-coordinate of the point.
            /// </summary>
            public int Y { get; set; }
        }

        /// <summary>
        /// Verifies that the default value factory returns null for a reference type, specifically for the string type.
        /// </summary>
        /// <remarks>This test ensures that the DefaultValueFactoryCache behaves as expected when
        /// retrieving a factory for a reference type, returning null as the default value. This is important for
        /// scenarios where reference types are expected to have a default value of null.</remarks>
        [Fact]
        public void Get_ReturnsNull_ForReferenceType()
        {
            var factory = DefaultValueFactoryCache.Get(typeof(string));
            Assert.Null(factory());
        }

        /// <summary>
        /// Verifies that the default value factory for the integer value type returns zero.
        /// </summary>
        /// <remarks>This test ensures that the DefaultValueFactoryCache correctly provides the default
        /// value for value types, specifically validating that the default for int is zero.</remarks>
        [Fact]
        public void Get_ReturnsDefault_ForValueType_Int()
        {
            var factory = DefaultValueFactoryCache.Get(typeof(int));
            Assert.Equal(0, factory());
        }

        /// <summary>
        /// Verifies that the default value factory returns the default value for the DateTime value type.
        /// </summary>
        /// <remarks>This test ensures that DefaultValueFactoryCache correctly provides a factory that
        /// returns DateTime.MinValue when requested for the DateTime type. This behavior is important for scenarios
        /// where uninitialized value types are expected to have their default values.</remarks>
        [Fact]
        public void Get_ReturnsDefault_ForValueType_DateTime()
        {
            var factory = DefaultValueFactoryCache.Get(typeof(DateTime));
            Assert.Equal(default(DateTime), factory());
        }

        /// <summary>
        /// Verifies that the default value factory for a custom struct type returns the expected default value.
        /// </summary>
        /// <remarks>This test ensures that the DefaultValueFactoryCache correctly generates a factory
        /// that produces the default value for the specified struct type. Use this test to confirm that custom structs
        /// are supported by the cache mechanism.</remarks>
        [Fact]
        public void Get_ReturnsDefault_ForCustomStruct()
        {
            var factory = DefaultValueFactoryCache.Get(typeof(MyStruct));
            Assert.Equal(default(MyStruct), factory());
        }

        /// <summary>
        /// Verifies that the DefaultValueFactoryCache returns null when requested for a factory for a custom class
        /// type.
        /// </summary>
        /// <remarks>This test ensures that, for types without a predefined default value, the
        /// DefaultValueFactoryCache correctly returns a factory that produces null. This behavior is important for
        /// maintaining consistency when handling custom reference types.</remarks>
        [Fact]
        public void Get_ReturnsNull_ForCustomClass()
        {
            var factory = DefaultValueFactoryCache.Get(typeof(MyClass));
            Assert.Null(factory());
        }

        /// <summary>
        /// Verifies that the DefaultValueFactoryCache.Get method returns the same delegate instance when called
        /// multiple times with the same type parameter.
        /// </summary>
        /// <remarks>This test ensures that the caching mechanism in DefaultValueFactoryCache is
        /// functioning correctly by confirming that repeated requests for a factory delegate for the same type yield
        /// the same object reference. This behavior is important for performance and consistency in scenarios where
        /// factory delegates are reused.</remarks>
        [Fact]
        public void Get_ReturnsSameDelegate_ForSameType()
        {
            var factory1 = DefaultValueFactoryCache.Get(typeof(int));
            var factory2 = DefaultValueFactoryCache.Get(typeof(int));
            Assert.Same(factory1, factory2);
        }
    }
}