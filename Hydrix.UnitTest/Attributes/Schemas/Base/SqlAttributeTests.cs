using Hydrix.Attributes.Schemas.Base;
using System;
using System.Linq;
using Xunit;

namespace Hydrix.UnitTest.Attributes.Schemas.Base
{
    /// <summary>
    /// Identifies a test method or class that uses a SQL database for its execution.
    /// </summary>
    /// <remarks>Apply this attribute to test methods or classes to indicate that they require a SQL database
    /// context. This can be used by test frameworks or infrastructure to set up and tear down database resources as
    /// needed.</remarks>
    sealed class TestSqlAttribute : 
        SqlAttribute { }

    /// <summary>
    /// Represents a test class that is associated with the TestSql attribute for use in SQL-related testing scenarios.
    /// </summary>
    [TestSql]
    class TestClassWithAttribute
    {
        /// <summary>
        /// Gets or sets the test value used for SQL-related operations.
        /// </summary>
        [TestSql]
        public int TestProperty { get; set; }
    }

    /// <summary>
    /// Represents a class that derives from TestClassWithAttribute, providing additional functionality or
    /// specialization.
    /// </summary>
    class Derived : 
        TestClassWithAttribute 
    { }

    /// <summary>
    /// Contains unit tests that verify the behavior and usage constraints of the SqlAttribute class.
    /// </summary>
    /// <remarks>These tests ensure that the SqlAttribute can be applied to classes and properties, is
    /// inherited by derived types, and cannot be applied multiple times to the same element. The tests are intended to
    /// validate the attribute's usage as defined by its AttributeUsage settings.</remarks>
    public class SqlAttributeTests
    {
        /// <summary>
        /// Verifies that the TestSqlAttribute can be applied to a class and is present on TestClassWithAttribute.
        /// </summary>
        /// <remarks>This test ensures that the TestSqlAttribute is correctly recognized as an attribute
        /// on the specified class. Use this test to validate attribute usage in custom attribute scenarios.</remarks>
        [Fact]
        public void Attribute_CanBeAppliedToClass()
        {
            var attrs = typeof(TestClassWithAttribute).GetCustomAttributes(typeof(TestSqlAttribute), true);
            Assert.Single(attrs);
        }

        /// <summary>
        /// Verifies that the TestSqlAttribute can be applied to a property and is correctly retrieved via reflection.
        /// </summary>
        /// <remarks>This test ensures that the TestSqlAttribute is present on the TestProperty property
        /// of the TestClassWithAttribute class. It checks that the attribute is not null and that exactly one instance
        /// is found.</remarks>
        [Fact]
        public void Attribute_CanBeAppliedToProperty()
        {
            var prop = typeof(TestClassWithAttribute).GetProperty(nameof(TestClassWithAttribute.TestProperty));
            var attrs = prop.GetCustomAttributes(typeof(TestSqlAttribute), true);
            Assert.NotNull(attrs);
            Assert.Single(attrs);
        }

        /// <summary>
        /// Verifies that the TestSqlAttribute is inherited by the Derived class when using reflection to retrieve
        /// custom attributes with inheritance enabled.
        /// </summary>
        /// <remarks>This test ensures that the TestSqlAttribute, when applied to a base class, is also
        /// present on derived classes if the attribute is marked as inheritable. It uses reflection with the inherit
        /// parameter set to true to confirm the expected behavior.</remarks>
        [Fact]
        public void Attribute_IsInherited()
        {
            var attrs = typeof(Derived).GetCustomAttributes(typeof(TestSqlAttribute), true);
            Assert.Single(attrs);
        }

        /// <summary>
        /// Verifies that the SqlAttribute cannot be applied multiple times to a single program element.
        /// </summary>
        /// <remarks>This test ensures that the AttributeUsageAttribute applied to SqlAttribute has
        /// AllowMultiple set to false, enforcing that only one instance of the attribute can be used per
        /// target.</remarks>
        [Fact]
        public void Attribute_CannotBeAppliedMultipleTimes()
        {
            var usage = (AttributeUsageAttribute)typeof(SqlAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault();
            Assert.NotNull(usage);
            Assert.False(usage.AllowMultiple);
        }
    }
}