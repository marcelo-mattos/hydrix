using Hydrix.Orchestrator.Builders.Query.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Builders.Query.Conditions
{
    /// <summary>
    /// Unit tests for the internal methods of WhereBuilder.
    /// </summary>
    public partial class WhereBuilderTests
    {
        /// <summary>
        /// Invokes a non-public instance method on the specified object using reflection.
        /// </summary>
        /// <remarks>This method uses reflection to access and invoke non-public instance methods. Use
        /// with caution, as invoking private members can have security and maintenance implications.</remarks>
        /// <param name="instance">The object instance on which to invoke the method. Cannot be null.</param>
        /// <param name="method">The name of the non-public instance method to invoke. Cannot be null or empty.</param>
        /// <param name="args">An array of arguments to pass to the method. May be empty if the method does not require parameters.</param>
        /// <returns>The return value of the invoked method, or null if the method has no return value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a method with the specified name and argument types cannot be found on the object.</exception>
        private static object InvokePrivate(object instance, string method, params object[] args)
        {
            var argTypes = args?.Select(a => a?.GetType() ?? typeof(object)).ToArray() ?? Type.EmptyTypes;
            var m = instance.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance, null, argTypes, null);
            if (m == null)
                throw new InvalidOperationException($"Method {method} not found.");
            return m.Invoke(instance, args);
        }

        /// <summary>
        /// Invokes a non-public instance method with the specified name and parameter types on the given object,
        /// passing the provided arguments.
        /// </summary>
        /// <remarks>This method uses reflection to access and invoke non-public instance methods. Use
        /// with caution, as invoking private members can lead to unexpected behavior and may break in future versions
        /// of the class.</remarks>
        /// <param name="instance">The object instance on which to invoke the non-public method. Cannot be null.</param>
        /// <param name="method">The name of the non-public method to invoke. Cannot be null or empty.</param>
        /// <param name="paramTypes">An array of parameter types that defines the method signature to match. Cannot be null.</param>
        /// <param name="args">An array of arguments to pass to the method. The number, order, and type of the arguments must match the
        /// method's parameters.</param>
        /// <returns>The return value of the invoked method, or null if the method has no return value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a method with the specified name and parameter types cannot be found on the instance.</exception>
        private static object InvokePrivateOverload(object instance, string method, Type[] paramTypes, params object[] args)
        {
            var m = instance.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance, null, paramTypes, null);
            if (m == null)
                throw new InvalidOperationException($"Method {method} not found.");
            return m.Invoke(instance, args);
        }

        /// <summary>
        /// Retrieves the list of tokens used by the specified <see cref="WhereBuilder"/> instance.
        /// </summary>
        /// <remarks>The returned list reflects the internal state of the <see cref="WhereBuilder"/>.
        /// Modifying the list may affect the builder's behavior.</remarks>
        /// <param name="builder">The <see cref="WhereBuilder"/> instance from which to retrieve the tokens. Cannot be null.</param>
        /// <returns>A list of strings containing the tokens associated with the specified <see cref="WhereBuilder"/>
        /// instance.</returns>
        private static List<string> GetTokens(WhereBuilder builder)
        {
            var field = typeof(WhereBuilder).GetField("_tokens", BindingFlags.NonPublic | BindingFlags.Instance);
            return (List<string>)field.GetValue(builder);
        }

        /// <summary>
        /// Verifies that BuildInternal returns an empty string when no tokens are present.
        /// </summary>
        [Fact]
        public void BuildInternal_Returns_Empty_When_NoTokens()
        {
            var builder = WhereBuilder.Create();
            var result = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Verifies that Add adds the first condition without any logical operator.
        /// </summary>
        [Fact]
        public void Add_Adds_First_Condition_Without_Operator()
        {
            var builder = WhereBuilder.Create();
            InvokePrivate(builder, "Add", "A = 1", false, false);
            var tokens = GetTokens(builder);
            Assert.Single(tokens);
            Assert.Equal("A = 1", tokens[0]);
        }

        /// <summary>
        /// Verifies that Add adds a second condition with the AND operator.
        /// </summary>
        [Fact]
        public void Add_Adds_Second_Condition_With_And()
        {
            var builder = WhereBuilder.Create();
            InvokePrivate(builder, "Add", "A = 1", false, false);
            InvokePrivate(builder, "Add", "B = 2", false, false);
            var tokens = GetTokens(builder);
            Assert.Equal(2, tokens.Count);
            Assert.Equal("AND B = 2", tokens[1]);
        }

        /// <summary>
        /// Verifies that Add adds a condition with the OR operator.
        /// </summary>
        [Fact]
        public void Add_Adds_Condition_With_Or()
        {
            var builder = WhereBuilder.Create();
            InvokePrivate(builder, "Add", "A = 1", false, false);
            InvokePrivate(builder, "Add", "B = 2", false, true);
            var tokens = GetTokens(builder);
            Assert.Equal("OR B = 2", tokens[1]);
        }

        /// <summary>
        /// Verifies that Add adds a condition with the NOT operator.
        /// </summary>
        [Fact]
        public void Add_Adds_Condition_With_Not()
        {
            var builder = WhereBuilder.Create();
            InvokePrivate(builder, "Add", "A = 1", true, false);
            var tokens = GetTokens(builder);
            Assert.Equal("NOT A = 1", tokens[0]);
        }

        /// <summary>
        /// Verifies that Add ignores empty or whitespace-only conditions.
        /// </summary>
        [Fact]
        public void Add_Ignores_Empty_Or_Whitespace_Condition()
        {
            var builder = WhereBuilder.Create();
            InvokePrivate(builder, "Add", "   ", false, false);
            var tokens = GetTokens(builder);
            Assert.Empty(tokens);
        }

        /// <summary>
        /// Verifies that Add returns the same instance for chaining.
        /// </summary>
        [Fact]
        public void Add_Returns_Same_Instance()
        {
            var builder = WhereBuilder.Create();
            var result = InvokePrivate(builder, "Add", "A = 1", false, false);
            Assert.Same(builder, result);
        }

        /// <summary>
        /// Verifies that AddGroup (AND/OR) adds a group with OR logic.
        /// </summary>
        [Fact]
        public void AddGroup_AndOr_Adds_Group_With_Or()
        {
            var builder = WhereBuilder.Create();
            InvokePrivateOverload(builder, "AddGroup",
                new[] { typeof(bool), typeof(bool), typeof(string[]) },
                false, true, new[] { "A = 1", "B = 2" });
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Contains("(A = 1 OR B = 2)", sql);
        }

        /// <summary>
        /// Verifies that AddGroup (AND/OR) adds a group with NOT logic.
        /// </summary>
        [Fact]
        public void AddGroup_AndOr_Adds_Group_With_Not()
        {
            var builder = WhereBuilder.Create();
            InvokePrivateOverload(builder, "AddGroup",
                new[] { typeof(bool), typeof(bool), typeof(string[]) },
                true, true, new[] { "A = 1", "B = 2" });
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Contains("NOT (A = 1 OR B = 2)", sql);
        }

        /// <summary>
        /// Verifies that AddGroup (OR/AND) adds a group with AND logic.
        /// </summary>
        [Fact]
        public void AddGroup_OrAnd_Adds_Group_With_And()
        {
            var builder = WhereBuilder.Create();
            InvokePrivateOverload(builder, "AddGroup",
                new[] { typeof(bool), typeof(bool), typeof(string[]) },
                false, false, new[] { "A = 1", "B = 2" });
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Contains("(A = 1 AND B = 2)", sql);
        }

        /// <summary>
        /// Verifies that AddGroup ignores empty or null conditions.
        /// </summary>
        [Fact]
        public void AddGroup_Ignores_Empty_Conditions()
        {
            var builder = WhereBuilder.Create();
            InvokePrivateOverload(builder, "AddGroup",
                new[] { typeof(bool), typeof(bool), typeof(string[]) },
                false, true, new[] { "   ", null, "" });
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Equal(string.Empty, sql);
        }

        /// <summary>
        /// Verifies that AddGroup returns the same instance for chaining.
        /// </summary>
        [Fact]
        public void AddGroup_Returns_Same_Instance()
        {
            var builder = WhereBuilder.Create();
            var result = InvokePrivateOverload(builder, "AddGroup",
                new[] { typeof(bool), typeof(bool), typeof(string[]) },
                false, true, new[] { "A = 1" });
            Assert.Same(builder, result);
        }

        /// <summary>
        /// Verifies that AddGroup with Action adds a nested group.
        /// </summary>
        [Fact]
        public void AddGroup_Action_Adds_Nested_Group()
        {
            var builder = WhereBuilder.Create();
            InvokePrivate(builder, "Add", "A = 1", false, false);
            InvokePrivateOverload(builder, "AddGroup",
                new[] { typeof(Action<WhereBuilder>), typeof(bool), typeof(bool) },
                new Action<WhereBuilder>(g =>
                {
                    InvokePrivate(g, "Add", "B = 2", false, false);
                    InvokePrivate(g, "Add", "C = 3", false, false);
                }), false, false);
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Contains("(B = 2 AND C = 3)", sql);
        }

        /// <summary>
        /// Verifies that AddGroup with Action adds a nested group with NOT and OR logic.
        /// </summary>
        [Fact]
        public void AddGroup_Action_Adds_Nested_Group_With_Not_And_Or()
        {
            var builder = WhereBuilder.Create();
            InvokePrivate(builder, "Add", "A = 1", false, false);
            InvokePrivateOverload(builder, "AddGroup",
                new[] { typeof(Action<WhereBuilder>), typeof(bool), typeof(bool) },
                new Action<WhereBuilder>(g =>
                {
                    InvokePrivate(g, "Add", "B = 2", false, false);
                }), true, true);
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Contains("OR NOT (B = 2)", sql);
        }

        /// <summary>
        /// Verifies that AddGroup with Action ignores an empty group.
        /// </summary>
        [Fact]
        public void AddGroup_Action_Ignores_Empty_Group()
        {
            var builder = WhereBuilder.Create();
            InvokePrivateOverload(builder, "AddGroup",
                new[] { typeof(Action<WhereBuilder>), typeof(bool), typeof(bool) },
                new Action<WhereBuilder>(g => { }), false, false);
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Equal(string.Empty, sql);
        }

        /// <summary>
        /// Verifies that the AddGroup method returns the same WhereBuilder instance when the conditions parameter
        /// is null.
        /// </summary>
        /// <remarks>This test ensures that invoking AddGroup with a null conditions argument does not
        /// modify the builder's state or add any tokens. It confirms that the method maintains the current state when
        /// no conditions are specified.</remarks>
        [Fact]
        public void AddGroup_Returns_Same_Instance_When_Conditions_IsNull()
        {
            var builder = WhereBuilder.Create();
            var result = InvokePrivateOverload(builder, "AddGroup",
                new[] { typeof(bool), typeof(bool), typeof(string[]) },
                false, false, null);
            Assert.Same(builder, result);
            Assert.Empty(GetTokens(builder));
        }

        /// <summary>
        /// Verifies that invoking the AddGroup method with an empty conditions array returns the same WhereBuilder
        /// instance and does not modify its state.
        /// </summary>
        /// <remarks>This test ensures that when no conditions are provided to AddGroup, the method does
        /// not add any tokens to the builder and maintains the original instance. This behavior is important to prevent
        /// unnecessary modifications or allocations when no conditions are specified.</remarks>
        [Fact]
        public void AddGroup_Returns_Same_Instance_When_Conditions_IsEmpty()
        {
            var builder = WhereBuilder.Create();
            var result = InvokePrivateOverload(builder, "AddGroup",
                new[] { typeof(bool), typeof(bool), typeof(string[]) },
                false, false, new string[0]);
            Assert.Same(builder, result);
            Assert.Empty(GetTokens(builder));
        }

        /// <summary>
        /// Verifies that the AddGroup method correctly adds a group of conditions using the logical 'OR' operator and
        /// does not include a 'NOT' clause when specified parameters are used.
        /// </summary>
        /// <remarks>This test ensures that when AddGroup is invoked with parameters indicating an 'OR'
        /// operation and no negation, the resulting SQL condition contains the expected grouping and logical operator,
        /// and omits any 'NOT' keyword. This helps validate the correct construction of dynamic SQL queries based on
        /// user-defined conditions.</remarks>
        [Fact]
        public void AddGroup_AndOr_Adds_Group_With_Or_Without_Not()
        {
            var builder = WhereBuilder.Create();
            InvokePrivateOverload(builder, "AddGroup",
                new[] { typeof(bool), typeof(bool), typeof(string[]) },
                false, true, new[] { "A = 1", "B = 2" });
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Contains("(A = 1 OR B = 2)", sql);
            Assert.DoesNotContain("NOT", sql);
        }

        /// <summary>
        /// Verifies that the AddGroup method correctly adds a group of conditions using logical AND without including a
        /// NOT clause when both negation parameters are false.
        /// </summary>
        /// <remarks>This test ensures that the generated SQL condition string combines multiple
        /// conditions with 'AND' and does not contain 'NOT' when negation is not requested.</remarks>
        [Fact]
        public void AddGroup_OrAnd_Adds_Group_With_And_Without_Not()
        {
            var builder = WhereBuilder.Create();
            InvokePrivateOverload(builder, "AddGroup",
                new[] { typeof(bool), typeof(bool), typeof(string[]) },
                false, false, new[] { "A = 1", "B = 2" });
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Contains("(A = 1 AND B = 2)", sql);
            Assert.DoesNotContain("NOT", sql);
        }

        /// <summary>
        /// Verifies that the AddGroup method correctly adds a group of conditions using logical AND and NOT operators.
        /// </summary>
        /// <remarks>This test ensures that when a group is added with both AND and NOT logic, the
        /// resulting SQL query contains the expected negated condition group. It validates the correct construction of
        /// complex logical expressions within the condition builder.</remarks>
        [Fact]
        public void AddGroup_AndOr_Adds_Group_With_And_With_Not()
        {
            var builder = WhereBuilder.Create();
            InvokePrivateOverload(builder, "AddGroup",
                new[] { typeof(bool), typeof(bool), typeof(string[]) },
                true, true, new[] { "A = 1", "B = 2" });
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Contains("NOT (A = 1 OR B = 2)", sql);
        }

        /// <summary>
        /// Verifies that the AddGroup method correctly adds a group of conditions using the AND operator and applies
        /// logical negation when specified.
        /// </summary>
        /// <remarks>This test ensures that when AddGroup is invoked with the appropriate parameters, the
        /// resulting SQL condition includes both the AND operator and a NOT clause as expected. It validates the
        /// correct construction of grouped conditions in the WhereBuilder.</remarks>
        [Fact]
        public void AddGroup_OrAnd_Adds_Group_With_And_With_Not()
        {
            var builder = WhereBuilder.Create();
            InvokePrivateOverload(builder, "AddGroup",
                new[] { typeof(bool), typeof(bool), typeof(string[]) },
                true, false, new[] { "A = 1", "B = 2" });
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Contains("NOT (A = 1 AND B = 2)", sql);
        }

        /// <summary>
        /// Verifies that AddAndOrGroup adds an OR group with AND logic.
        /// </summary>
        [Fact]
        public void AddAndOrGroup_Adds_Or_Group_With_And()
        {
            var builder = WhereBuilder.Create();
            InvokePrivate(builder, "AddAndOrGroup", false, new[] { "A = 1", "B = 2" });
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Contains("(A = 1 OR B = 2)", sql);
        }

        /// <summary>
        /// Verifies that AddAndOrGroup adds a NOT OR group.
        /// </summary>
        [Fact]
        public void AddAndOrGroup_Adds_Not_Or_Group()
        {
            var builder = WhereBuilder.Create();
            InvokePrivate(builder, "AddAndOrGroup", true, new[] { "A = 1", "B = 2" });
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Contains("NOT (A = 1 OR B = 2)", sql);
        }

        /// <summary>
        /// Verifies that AddOrAndGroup adds an AND group with OR logic.
        /// </summary>
        [Fact]
        public void AddOrAndGroup_Adds_And_Group_With_Or()
        {
            var builder = WhereBuilder.Create();
            InvokePrivate(builder, "AddOrAndGroup", false, new[] { "A = 1", "B = 2" });
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Contains("(A = 1 AND B = 2)", sql);
        }

        /// <summary>
        /// Verifies that AddOrAndGroup adds a NOT AND group.
        /// </summary>
        [Fact]
        public void AddOrAndGroup_Adds_Not_And_Group()
        {
            var builder = WhereBuilder.Create();
            InvokePrivate(builder, "AddOrAndGroup", true, new[] { "A = 1", "B = 2" });
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Contains("NOT (A = 1 AND B = 2)", sql);
        }

        /// <summary>
        /// Verifies that AddAndOrGroup ignores empty or null conditions.
        /// </summary>
        [Fact]
        public void AddAndOrGroup_Ignores_Empty_Conditions()
        {
            var builder = WhereBuilder.Create();
            InvokePrivate(builder, "AddAndOrGroup", false, new[] { "   ", null, "" });
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Equal(string.Empty, sql);
        }

        /// <summary>
        /// Verifies that AddOrAndGroup ignores empty or null conditions.
        /// </summary>
        [Fact]
        public void AddOrAndGroup_Ignores_Empty_Conditions()
        {
            var builder = WhereBuilder.Create();
            InvokePrivate(builder, "AddOrAndGroup", false, new[] { "   ", null, "" });
            var sql = (string)InvokePrivate(builder, "BuildInternal");
            Assert.Equal(string.Empty, sql);
        }
    }
}