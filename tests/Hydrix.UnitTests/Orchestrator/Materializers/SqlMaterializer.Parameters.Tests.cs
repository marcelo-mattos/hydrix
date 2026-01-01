using Hydrix.Orchestrator.Materializers;
using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers
{
    /// <summary>
    /// Contains unit tests for the SqlMaterializer class, verifying parameter handling and formatting behaviors for SQL
    /// command construction.
    /// </summary>
    /// <remarks>These tests cover scenarios such as scalar and enumerable parameter binding, parameter name
    /// formatting, and value formatting for various data types. The class uses mock implementations of IDbCommand and
    /// related interfaces to isolate and validate SqlMaterializer logic without requiring a real database
    /// connection.</remarks>
    public partial class SqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the IsEnumerableParameter method correctly determines whether the specified value is
        /// considered an enumerable parameter.
        /// </summary>
        /// <param name="value">The value to test for enumerable parameter status. Can be null or any object.</param>
        /// <param name="expected">The expected result indicating whether the value should be recognized as an enumerable parameter.</param>
        [Theory]
        [InlineData(null, false)]
        [InlineData("string", false)]
        [InlineData(new byte[] { 1, 2 }, false)]
        [InlineData(new int[] { 1, 2 }, true)]
        [InlineData(new string[] { "a", "b" }, true)]
        [InlineData(42, false)]
        public void IsEnumerableParameter_Works(object value, bool expected)
        {
            Assert.Equal(expected, TestSqlMaterializerParameter.CallIsEnumerableParameter(value));
        }

        /// <summary>
        /// Verifies that the AddScalarParameter method adds a parameter with the correct name and value to the command.
        /// </summary>
        /// <remarks>This unit test ensures that when AddScalarParameter is called, the resulting
        /// parameter in the command has the expected name and value. It is intended to validate correct parameter
        /// handling in the TestSqlMaterializerParameter class.</remarks>
        [Fact]
        public void AddScalarParameter_AddsParameterWithCorrectNameAndValue()
        {
            var mat = new TestSqlMaterializerParameter("@");
            var cmd = new MockDbCommand();
            mat.CallAddScalarParameter(cmd, "foo", 123);
            var param = cmd.Parameters[0] as IDbDataParameter;
            Assert.Equal("@foo", param.ParameterName);
            Assert.Equal(123, param.Value);
        }

        /// <summary>
        /// Verifies that adding a scalar parameter with a null value results in the parameter's value being set to
        /// DBNull.Value.
        /// </summary>
        /// <remarks>This test ensures that the AddScalarParameter method correctly handles null values by
        /// converting them to DBNull.Value when adding parameters to a database command. This behavior is important for
        /// compatibility with ADO.NET, which requires DBNull.Value to represent database nulls.</remarks>
        [Fact]
        public void AddScalarParameter_NullValue_UsesDBNull()
        {
            var mat = new TestSqlMaterializerParameter("@");
            var cmd = new MockDbCommand();
            mat.CallAddScalarParameter(cmd, "bar", null);
            var param = cmd.Parameters[0] as IDbDataParameter;
            Assert.Equal(DBNull.Value, param.Value);
        }

        /// <summary>
        /// Verifies that expanding an enumerable parameter in a SQL command adds individual parameters for each value
        /// and updates the command text accordingly.
        /// </summary>
        /// <remarks>This test ensures that when an enumerable parameter is expanded, the command text
        /// replaces the original parameter placeholder with unique parameter names for each value in the collection,
        /// and that the corresponding parameters are added to the command. This behavior is important for supporting
        /// SQL 'IN' clauses with parameterized queries.</remarks>
        [Fact]
        public void ExpandEnumerableParameter_AddsParametersAndUpdatesCommandText()
        {
            var mat = new TestSqlMaterializerParameter("@");
            var cmd = new MockDbCommand { CommandText = "SELECT * FROM T WHERE id IN (@ids)" };
            mat.CallExpandEnumerableParameter(cmd, "ids", new[] { 1, 2, 3 });
            Assert.Equal(3, cmd.Parameters.Count);
            Assert.All(cmd.Parameters.Cast<IDbDataParameter>(), p => Assert.StartsWith("@ids_", p.ParameterName));
            Assert.Contains("@ids_0", cmd.CommandText);
            Assert.Contains("@ids_1", cmd.CommandText);
            Assert.Contains("@ids_2", cmd.CommandText);
            Assert.DoesNotContain("@ids)", cmd.CommandText);
        }

        /// <summary>
        /// Verifies that adding a scalar parameter using the AddParameter method results in a single parameter being
        /// added to the command with the correct name and value.
        /// </summary>
        /// <remarks>This unit test ensures that when a scalar value is provided to AddParameter, the
        /// parameter is named with the expected prefix and assigned the correct value in the command's parameter
        /// collection.</remarks>
        [Fact]
        public void AddParameter_Scalar_AddsSingleParameter()
        {
            var mat = new TestSqlMaterializerParameter("@");
            var cmd = new MockDbCommand();
            mat.CallAddParameter(cmd, "foo", 42);
            Assert.Single(cmd.Parameters);
            Assert.Equal("@foo", ((IDbDataParameter)cmd.Parameters[0]).ParameterName);
            Assert.Equal(42, ((IDbDataParameter)cmd.Parameters[0]).Value);
        }

        /// <summary>
        /// Verifies that adding an enumerable parameter to a SQL command expands the parameter into multiple individual
        /// parameters in the command text and parameter collection.
        /// </summary>
        /// <remarks>This test ensures that when an enumerable value is provided as a parameter, each
        /// element is assigned a unique parameter name and added to the command, allowing for correct SQL 'IN' clause
        /// expansion.</remarks>
        [Fact]
        public void AddParameter_Enumerable_ExpandsParameters()
        {
            var mat = new TestSqlMaterializerParameter("@");
            var cmd = new MockDbCommand { CommandText = "WHERE x IN (@x)" };
            mat.CallAddParameter(cmd, "x", new[] { 7, 8 });
            Assert.Equal(2, cmd.Parameters.Count);
            Assert.Contains("@x_0", cmd.CommandText);
            Assert.Contains("@x_1", cmd.CommandText);
        }

        /// <summary>
        /// Verifies that all public properties of an object are correctly bound as parameters to a database command.
        /// </summary>
        /// <remarks>This test ensures that each property of the provided object results in a
        /// corresponding parameter on the command, with the correct parameter name and value. It validates the behavior
        /// of the parameter binding logic when handling objects with multiple properties.</remarks>
        [Fact]
        public void BindParametersFromObject_BindsAllProperties()
        {
            var mat = new TestSqlMaterializerParameter("@");
            var cmd = new MockDbCommand();
            var obj = new { A = 1, B = "b" };
            mat.CallBindParametersFromObject(cmd, obj);
            Assert.Equal(2, cmd.Parameters.Count);
            Assert.Contains(cmd.Parameters.Cast<IDbDataParameter>(), p => p.ParameterName == "@A" && (int)p.Value == 1);
            Assert.Contains(cmd.Parameters.Cast<IDbDataParameter>(), p => p.ParameterName == "@B" && (string)p.Value == "b");
        }

        /// <summary>
        /// Verifies that binding parameters from an object with an enumerable property expands the property into
        /// individual parameters in the command text and parameter collection.
        /// </summary>
        /// <remarks>This test ensures that when an object containing an enumerable property is used to
        /// bind parameters to a database command, each element in the enumerable is assigned a separate parameter, and
        /// the command text is updated to reference each expanded parameter.</remarks>
        [Fact]
        public void BindParametersFromObject_ExpandsEnumerableProperty()
        {
            var mat = new TestSqlMaterializerParameter("@");
            var cmd = new MockDbCommand { CommandText = "IN (@Ids)" };
            var obj = new { Ids = new[] { 1, 2 } };
            mat.CallBindParametersFromObject(cmd, obj);
            Assert.Equal(2, cmd.Parameters.Count);
            Assert.Contains("@Ids_0", cmd.CommandText);
            Assert.Contains("@Ids_1", cmd.CommandText);
        }

        /// <summary>
        /// Verifies that the parameter formatting logic correctly handles common data types and produces the expected
        /// string representations.
        /// </summary>
        /// <remarks>This test covers typical cases for parameter formatting, including null values,
        /// strings, numeric types, booleans, and date strings. It ensures that the formatting method produces
        /// consistent and correct output for these common scenarios.</remarks>
        /// <param name="value">The input value to be formatted. Can be null or an instance of a supported type such as string, integer,
        /// boolean, or date string.</param>
        /// <param name="expected">The expected string representation of the formatted parameter value.</param>
        [Theory]
        [InlineData(null, "NULL")]
        [InlineData("abc", "'abc'")]
        [InlineData(123, "123")]
        [InlineData(true, "1")]
        [InlineData(false, "0")]
        [InlineData("2020-01-01", "'2020-01-01'")]
        public void FormatParameterValue_HandlesCommonTypes(object value, string expected)
        {
            if (value is string s && DateTime.TryParse(s, out var dt))
                Assert.Equal($"'{dt:yyyy-MM-dd HH:mm:ss.fff}'", TestSqlMaterializerParameter.CallFormatParameterValue(dt));
            else
                Assert.Equal(expected, TestSqlMaterializerParameter.CallFormatParameterValue(value));
        }

        /// <summary>
        /// Tests that the parameter value formatting method correctly formats a Guid value as a string enclosed in
        /// single quotes.
        /// </summary>
        [Fact]
        public void FormatParameterValue_Guid()
        {
            var guid = Guid.NewGuid();
            Assert.Equal($"'{guid}'", TestSqlMaterializerParameter.CallFormatParameterValue(guid));
        }

        /// <summary>
        /// Verifies that formatting a parameter value of type DBNull returns the string "NULL".
        /// </summary>
        [Fact]
        public void FormatParameterValue_DBNull()
        {
            Assert.Equal("NULL", TestSqlMaterializerParameter.CallFormatParameterValue(DBNull.Value));
        }
    }
}