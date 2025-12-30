using Hydrix.Schemas;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Schemas
{
    /// <summary>
    /// Represents a parameter to a command object, such as a SQL query or stored procedure, for use with data providers
    /// that implement the IDataParameter interface.
    /// </summary>
    /// <remarks>This class is typically used to define input, output, or return value parameters when
    /// executing database commands. It allows specifying the parameter's name, data type, direction, and value, as well
    /// as additional metadata such as the source column and data row version. The IsNullable property always returns
    /// false, indicating that this parameter does not support null values.</remarks>
    public class DummyParameter : IDataParameter
    {
        /// <summary>
        /// Gets or sets the database type of the parameter.
        /// </summary>
        public DbType DbType { get; set; }

        /// <summary>
        /// Gets or sets the direction of the parameter within a command or stored procedure.
        /// </summary>
        /// <remarks>The direction determines whether the parameter is used for input, output,
        /// bidirectional, or as a return value. The default is typically Input. Ensure that the direction matches the
        /// expected usage in the associated command or stored procedure.</remarks>
        public ParameterDirection Direction { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current type allows null values.
        /// </summary>
        public bool IsNullable => false;

        /// <summary>
        /// Gets or sets the name of the parameter associated with the operation.
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// Gets or sets the name of the source column mapped to the DataSet.
        /// </summary>
        /// <remarks>This property is typically used in data binding scenarios to specify which column
        /// from the data source should be used for mapping or updating values. The value is case-sensitive and should
        /// match the column name in the data source exactly.</remarks>
        public string SourceColumn { get; set; }

        /// <summary>
        /// Gets or sets the version of data in a DataRow to use when loading parameter values.
        /// </summary>
        /// <remarks>This property determines which version of the DataRow's data is used when the
        /// parameter value is obtained, such as when updating a database. Common values include Current, Original, and
        /// Proposed. The default is typically DataRowVersion.Current.</remarks>
        public DataRowVersion SourceVersion { get; set; }

        /// <summary>
        /// Gets or sets the value associated with this instance.
        /// </summary>
        public object Value { get; set; }
    }

    /// <summary>
    /// Represents a dummy implementation of a SQL stored procedure for use with the Hydrix schema framework.
    /// </summary>
    /// <remarks>This class is intended for testing or demonstration purposes and does not provide actual
    /// database functionality. It implements the ISqlProcedure interface using the DummyParameter type
    /// parameter.</remarks>
    public class DummySqlProcedure : 
        ISqlProcedure<DummyParameter>
    { }

    /// <summary>
    /// Provides unit tests for verifying implementations of the ISqlProcedure interface and related parameter types.
    /// </summary>
    /// <remarks>This class contains tests to ensure that custom SQL procedure and parameter types conform to
    /// expected interfaces, such as ISqlProcedure&lt;T&gt; and IDataParameter. These tests are intended for use with the
    /// xUnit testing framework.</remarks>
    public class ISqlProcedureTestsImpl
    {

        /// <summary>
        /// Verifies that a DummySqlProcedure instance implements the ISqlProcedure interface with DummyParameter as the
        /// parameter type.
        /// </summary>
        /// <remarks>This test ensures that DummySqlProcedure can be assigned to
        /// ISqlProcedure&lt;DummyParameter&gt;, confirming correct interface implementation for valid parameter
        /// types.</remarks>
        [Fact]
        public void CanImplementISqlProcedure_WithValidParameterType()
        {
            var proc = new DummySqlProcedure();
            Assert.IsAssignableFrom<Hydrix.Schemas.ISqlProcedure<DummyParameter>>(proc);
        }

        /// <summary>
        /// Verifies that the DummyParameter class implements the IDataParameter interface.
        /// </summary>
        /// <remarks>Use this test to ensure that DummyParameter can be used wherever an IDataParameter is
        /// required, such as in data access scenarios that depend on standard ADO.NET interfaces.</remarks>
        [Fact]
        public void DummyParameter_ImplementsIDataParameter()
        {
            var param = new DummyParameter();
            Assert.IsAssignableFrom<IDataParameter>(param);
        }
    }
}