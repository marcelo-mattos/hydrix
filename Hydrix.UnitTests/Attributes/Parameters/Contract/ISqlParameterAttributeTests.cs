using System;
using System.Data;
using Xunit;
using Hydrix.Attributes.Parameters.Contract;

namespace Hydrix.UnitTests.Attributes.Parameters.Contract
{
    /// <summary>
    /// Contains unit tests for verifying the behavior of the TestSqlParameterAttribute class, which implements
    /// ISqlParameterAttribute for testing SQL parameter metadata.
    /// </summary>
    /// <remarks>These tests validate that the TestSqlParameterAttribute correctly exposes its properties and
    /// enforces valid values for database type and parameter direction. The tests ensure that exceptions are thrown for
    /// invalid input, supporting robust usage in test scenarios.</remarks>
    public class SqlParameterAttributeTests
    {
        /// <summary>
        /// Represents metadata for a SQL parameter, including its name, database type, and direction, for use in
        /// testing scenarios.
        /// </summary>
        private class TestSqlParameterAttribute : 
            ISqlParameterAttribute
        {
            /// <summary>
            /// Gets the name of the parameter associated with the exception.
            /// </summary>
            public string ParameterName { get; }

            /// <summary>
            /// Gets the database type of the parameter.
            /// </summary>
            public DbType DbType { get; }

            /// <summary>
            /// Gets the direction of the parameter within a command or stored procedure.
            /// </summary>
            /// <remarks>The direction indicates whether the parameter is used for input, output,
            /// bidirectional, or as a return value. This property is typically set when configuring database command
            /// parameters.</remarks>
            public ParameterDirection Direction { get; }

            /// <summary>
            /// Initializes a new instance of the TestSqlParameterAttribute class with the specified parameter name,
            /// database type, and direction.
            /// </summary>
            /// <param name="parameterName">The name of the SQL parameter. Cannot be null.</param>
            /// <param name="dbType">The database type of the parameter. Must be a valid value of the DbType enumeration.</param>
            /// <param name="direction">The direction of the parameter (input, output, or both). Must be a valid value of the ParameterDirection
            /// enumeration.</param>
            /// <exception cref="ArgumentOutOfRangeException">Thrown if dbType is not a defined value of the DbType enumeration.</exception>
            /// <exception cref="ArgumentException">Thrown if direction is not a defined value of the ParameterDirection enumeration.</exception>
            public TestSqlParameterAttribute(
                string parameterName, 
                DbType dbType, 
                ParameterDirection direction)
            {
                ParameterName = parameterName;

                if (!Enum.IsDefined(typeof(DbType), dbType))
                    throw new ArgumentOutOfRangeException(nameof(dbType));
                DbType = dbType;
                
                if (!Enum.IsDefined(typeof(ParameterDirection), direction))
                    throw new ArgumentException(nameof(direction));
                Direction = direction;
            }
        }

        /// <summary>
        /// Verifies that the ParameterName property returns the expected value for a TestSqlParameterAttribute
        /// instance.
        /// </summary>
        [Fact]
        public void ParameterName_ShouldReturnValue()
        {
            var attr = new TestSqlParameterAttribute("p1", DbType.String, ParameterDirection.Input);
            Assert.Equal("p1", attr.ParameterName);
        }

        /// <summary>
        /// Verifies that the DbType property returns the value specified during construction.
        /// </summary>
        [Fact]
        public void DbType_ShouldReturnValue()
        {
            var attr = new TestSqlParameterAttribute("p2", DbType.Int32, ParameterDirection.Output);
            Assert.Equal(DbType.Int32, attr.DbType);
        }

        /// <summary>
        /// Verifies that the Direction property returns the value specified during construction.
        /// </summary>
        [Fact]
        public void Direction_ShouldReturnValue()
        {
            var attr = new TestSqlParameterAttribute("p3", DbType.Boolean, ParameterDirection.InputOutput);
            Assert.Equal(ParameterDirection.InputOutput, attr.Direction);
        }

        /// <summary>
        /// Verifies that creating a TestSqlParameterAttribute with an invalid DbType value throws an
        /// ArgumentOutOfRangeException.
        /// </summary>
        /// <remarks>This test ensures that the constructor of TestSqlParameterAttribute enforces valid
        /// DbType values and throws the appropriate exception when an out-of-range value is provided.</remarks>
        [Fact]
        public void DbType_InvalidValue_ShouldThrow()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new TestSqlParameterAttribute("p4", (DbType)999, ParameterDirection.Input));
        }

        /// <summary>
        /// Verifies that creating a TestSqlParameterAttribute with an invalid ParameterDirection value throws an
        /// ArgumentException.
        /// </summary>
        [Fact]
        public void Direction_InvalidValue_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() =>
                new TestSqlParameterAttribute("p5", DbType.String, (ParameterDirection)999));
        }
    }
}