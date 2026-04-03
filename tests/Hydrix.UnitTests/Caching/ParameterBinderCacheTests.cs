using Hydrix.Binders.Parameter;
using Hydrix.Caching;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Caching
{
    /// <summary>
    /// Contains unit tests for the ParameterBinderCache class to verify its behavior when handling types without
    /// readable properties or with only indexers.
    /// </summary>
    /// <remarks>These tests ensure that ParameterBinderCache returns a binder with no bindings for types that
    /// do not expose any readable properties, validating the correctness of the caching mechanism in such
    /// scenarios.</remarks>
    public class ParameterBinderCacheTests
    {
        /// <summary>
        /// Class that contains no properties, which should result in a binder with no bindings when processed by the
        /// ParameterBinderCache.
        /// </summary>
        private class NoProperties
        { }

        /// <summary>
        /// Class that contains only an indexer property, which should not be considered a valid parameter for binding.
        /// </summary>
        private class OnlyIndexer
        {
            /// <summary>
            /// Provides an indexer that takes an integer parameter and returns a fixed value. This indexer is not a valid
            /// parameter for binding.
            /// </summary>
            /// <param name="i">The index parameter.</param>
            /// <returns>The fixed value.</returns>
            public int this[int i] => 42;
        }

        /// <summary>
        /// Represents a class that encapsulates a property which can only be assigned a value, but not retrieved,
        /// enabling controlled write-only access.
        /// </summary>
        /// <remarks>This design is useful for scenarios where you want to allow assignment to a property
        /// without exposing its value for reading. The 'Id' property can be set externally, but its value cannot be
        /// accessed directly.</remarks>
        private class WriteOnlyProperty
        {
            /// <summary>
            /// Backing field for the write-only property, storing the assigned value internally while preventing external access.
            /// </summary>
            private int _id;

            /// <summary>
            /// Gets or sets the identifier for this class, allowing write-only access. The property can be assigned a value,
            /// but its value cannot be retrieved.
            /// </summary>
            public int Id { set => _id = value; }
        }

        /// <summary>
        /// Represents a type with both a value-type property and a reference-type property, used to exercise both
        /// branches of the <c>IsValueType</c> check inside <c>CreateGetter</c>.
        /// </summary>
        private class MixedProperties
        {
            /// <summary>
            /// Gets or sets the unique identifier for the entity.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name associated with this instance.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Verifies that the ParameterBinderCache.GetOrAdd method returns a binder with no bindings when the specified
        /// type does not have any readable properties.
        /// </summary>
        /// <remarks>This test ensures that the binder produced for types without readable properties
        /// contains no bindings, confirming correct handling of such types by the cache mechanism.</remarks>
        [Fact]
        public void GetOrAdd_ReturnsBinderWithNoBindings_WhenTypeHasNoReadableProperties()
        {
            var binder = ParameterBinderCache.GetOrAdd(typeof(NoProperties));
            var bindingsField = typeof(ParameterObjectBinder).GetField("_bindings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var bindings = (ParameterObjectBinding[])bindingsField.GetValue(binder);
            Assert.Empty(bindings);
        }

        /// <summary>
        /// Verifies that the ParameterBinderCache.GetOrAdd method returns a binder with no parameter bindings when the
        /// specified type contains only an indexer.
        /// </summary>
        /// <remarks>This test ensures that types with only indexers do not produce parameter bindings,
        /// confirming that the binder reflects the absence of parameters. It validates correct behavior for scenarios
        /// where parameterless types are expected.</remarks>
        [Fact]
        public void GetOrAdd_ReturnsBinderWithNoBindings_WhenTypeHasOnlyIndexer()
        {
            var binder = ParameterBinderCache.GetOrAdd(typeof(OnlyIndexer));
            var bindingsField = typeof(ParameterObjectBinder).GetField("_bindings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var bindings = (ParameterObjectBinding[])bindingsField.GetValue(binder);
            Assert.Empty(bindings);
        }

        /// <summary>
        /// Verifies that write-only properties are not included in the parameter binder cache.
        /// </summary>
        /// <remarks>This test ensures that when a type with write-only properties is passed to the
        /// parameter binder, no bindings are created, confirming that write-only properties are excluded from the
        /// binding process.</remarks>
        [Fact]
        public void GetOrAdd_DoesNotIncludeWriteOnlyProperties()
        {
            var binder = ParameterBinderCache.GetOrAdd(typeof(WriteOnlyProperty));
            var bindingsField = typeof(ParameterObjectBinder).GetField("_bindings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var bindings = (ParameterObjectBinding[])bindingsField.GetValue(binder);
            Assert.Empty(bindings);
        }

        /// <summary>
        /// Verifies that <see cref="ParameterBinderCache.GetOrAdd"/> builds getters correctly for both value-type and
        /// reference-type properties, exercising both branches of the <c>IsValueType</c> check in
        /// <c>CreateGetter</c>.
        /// </summary>
        /// <remarks>This test ensures that the compiled getter delegates return the expected property
        /// values for a type that contains both a value-type property (<c>int</c>) and a reference-type property
        /// (<c>string</c>).</remarks>
        [Fact]
        public void GetOrAdd_CreatesBinder_WithValueTypeAndReferenceTypeProperties()
        {
            var instance = new MixedProperties { Id = 7, Name = "Test" };

            var binder = ParameterBinderCache.GetOrAdd(typeof(MixedProperties));

            var bindingsField = typeof(ParameterObjectBinder).GetField(
                "_bindings",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var bindings = (ParameterObjectBinding[])bindingsField.GetValue(binder);

            Assert.Equal(2, bindings.Length);

            var idBinding = System.Array.Find(bindings, b => b.Name == nameof(MixedProperties.Id));
            var nameBinding = System.Array.Find(bindings, b => b.Name == nameof(MixedProperties.Name));

            Assert.NotNull(idBinding.Getter);
            Assert.NotNull(nameBinding.Getter);
            Assert.Equal(7, idBinding.Getter(instance));
            Assert.Equal("Test", nameBinding.Getter(instance));
        }
    }
}
