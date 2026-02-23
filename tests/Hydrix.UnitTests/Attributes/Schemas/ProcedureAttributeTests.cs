using Hydrix.Attributes.Schemas;
using System;
using System.Data;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Attributes.Schemas
{
    /// <summary>
    /// Contains unit tests for the ProcedureAttribute class, verifying its construction and attribute usage
    /// behavior.
    /// </summary>
    /// <remarks>These tests ensure that ProcedureAttribute correctly sets its properties and enforces its
    /// intended usage constraints. The class is intended for use with a unit testing framework such as xUnit.</remarks>
    public class ProcedureAttributeTests
    {
        /// <summary>
        /// Verifies that the ProcedureAttribute constructor correctly sets the Schema and Name properties based on
        /// the provided arguments.
        /// </summary>
        /// <remarks>This unit test ensures that when a ProcedureAttribute is instantiated with
        /// specific schema and name values, the corresponding properties reflect those values. This helps validate the
        /// attribute's initialization behavior.</remarks>
        [Fact]
        public void Constructor_SetsSchemaAndNameProperties()
        {
            // Arrange
            var schema = "dbo";
            var name = "MyProc";

            // Act
            var attr = new ProcedureAttribute(name)
            {
                Schema = schema
            };

            // Assert
            Assert.Equal(schema, attr.Schema);
            Assert.Equal(name, attr.Name);
        }

        /// <summary>
        /// Verifies that the constructor of ProcedureAttribute sets the CommandType to StoredProcedure and
        /// initializes the CommandText with the specified schema and procedure name.
        /// </summary>
        /// <remarks>This test ensures that when a ProcedureAttribute is instantiated with a schema and
        /// procedure name, its CommandType property is set to CommandType.StoredProcedure and its CommandText property
        /// is formatted as "{schema}.{name}".</remarks>
        [Fact]
        public void Constructor_SetsBaseCommandTypeAndCommandText()
        {
            // Arrange
            var schema = "hr";
            var name = "GetEmployee";

            // Act
            var attr = new ProcedureAttribute(name)
            {
                Schema = schema
            };

            // Assert
            Assert.Equal(CommandType.StoredProcedure, attr.CommandType);
            Assert.Equal($"{schema}.{name}", attr.CommandText);
        }

        /// <summary>
        /// Verifies that the ProcedureAttribute can only be applied to classes and does not allow multiple instances
        /// on the same class.
        /// </summary>
        /// <remarks>This test ensures that the AttributeUsageAttribute applied to ProcedureAttribute
        /// restricts its usage to class declarations and prevents multiple applications to a single class. This helps
        /// enforce correct attribute usage patterns in consuming code.</remarks>
        [Fact]
        public void AttributeUsage_IsClassOnly_AndNotAllowMultiple()
        {
            // Act
            var usage = typeof(ProcedureAttribute)
                .GetCustomAttribute<AttributeUsageAttribute>();

            // Assert
            Assert.NotNull(usage);
            Assert.Equal(AttributeTargets.Class, usage.ValidOn);
            Assert.False(usage.AllowMultiple);
        }
    }
}