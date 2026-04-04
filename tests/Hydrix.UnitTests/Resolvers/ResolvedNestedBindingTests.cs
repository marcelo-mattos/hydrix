using Hydrix.Resolvers;
using Hydrix.Schemas.Contract;
using System;
using Xunit;

namespace Hydrix.UnitTests.Resolvers
{
    /// <summary>
    /// Contains unit tests for <see cref="ResolvedNestedBinding"/>.
    /// </summary>
    public class ResolvedNestedBindingTests
    {
        /// <summary>
        /// Represents a parent object used by nested-binding activation tests.
        /// </summary>
        private sealed class ParentEntity
        {
            /// <summary>
            /// Gets or sets the nested child assigned by binding activators.
            /// </summary>
            public ChildEntity Child { get; set; }
        }

        /// <summary>
        /// Represents a child entity used in nested-binding activation tests.
        /// </summary>
        private sealed class ChildEntity : ITable
        {
            /// <summary>
            /// Gets or sets the identifier value.
            /// </summary>
            public int Id { get; set; }
        }

        /// <summary>
        /// Verifies that the factory/setter constructor stores compatibility members and composes an activator that
        /// creates and assigns the nested entity.
        /// </summary>
        [Fact]
        public void Constructor_WithFactoryAndSetter_ComposesActivatorAndStoresCompatibilityMembers()
        {
            var bindings = new ResolvedTableBindings(null, null, new[] { "Id" });
            Func<object> factory = () => new ChildEntity { Id = 7 };
            Action<object, object> setter = (parent, child) => ((ParentEntity)parent).Child = (ChildEntity)child;

            var binding = new ResolvedNestedBinding(
                usesPrimaryKey: true,
                primaryKeyOrdinal: 0,
                candidateOrdinals: null,
                factory: factory,
                setter: setter,
                bindings: bindings);

            var parentEntity = new ParentEntity();
            var activated = binding.Activator(parentEntity);

            Assert.True(binding.UsesPrimaryKey);
            Assert.Equal(0, binding.PrimaryKeyOrdinal);
            Assert.Empty(binding.CandidateOrdinals);
            Assert.Same(factory, binding.Factory);
            Assert.Same(setter, binding.Setter);
            Assert.Same(bindings, binding.Bindings);
            Assert.NotNull(activated);
            Assert.IsType<ChildEntity>(activated);
            Assert.Same(activated, parentEntity.Child);
            Assert.Equal(7, parentEntity.Child.Id);
        }

        /// <summary>
        /// Verifies that the activator constructor keeps compatibility members unset and uses the provided activator.
        /// </summary>
        [Fact]
        public void Constructor_WithActivator_UsesProvidedActivator_AndKeepsCompatibilityMembersNull()
        {
            var bindings = new ResolvedTableBindings(null, null, new[] { "Id" });
            Func<object, ITable> activator = parent =>
            {
                var child = new ChildEntity { Id = 9 };
                ((ParentEntity)parent).Child = child;
                return child;
            };

            var binding = new ResolvedNestedBinding(
                usesPrimaryKey: false,
                primaryKeyOrdinal: -1,
                candidateOrdinals: new[] { 2, 3 },
                activator: activator,
                bindings: bindings);

            var parentEntity = new ParentEntity();
            var activated = binding.Activator(parentEntity);

            Assert.False(binding.UsesPrimaryKey);
            Assert.Equal(-1, binding.PrimaryKeyOrdinal);
            Assert.Equal(new[] { 2, 3 }, binding.CandidateOrdinals);
            Assert.Null(binding.Factory);
            Assert.Null(binding.Setter);
            Assert.Same(bindings, binding.Bindings);
            Assert.NotNull(activated);
            Assert.IsType<ChildEntity>(activated);
            Assert.Same(activated, parentEntity.Child);
            Assert.Equal(9, parentEntity.Child.Id);
        }
    }
}
