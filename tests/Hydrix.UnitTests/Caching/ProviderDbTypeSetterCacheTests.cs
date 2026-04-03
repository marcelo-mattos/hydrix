using Hydrix.Caching;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Caching
{
    /// <summary>
    /// Contains unit tests for the ProviderDbTypeSetterCache class, validating the behavior of the GetOrAdd method and
    /// its handling of IDataParameter implementations.
    /// </summary>
    /// <remarks>These tests ensure that the cache correctly returns delegates for types with a ProviderDbType
    /// property and verifies that the same delegate is returned for repeated requests for the same type. The tests also
    /// check the correct setting of enum properties through the delegate.</remarks>
    public class ProviderDbTypeSetterCacheTests
    {
        /// <summary>
        /// Specifies the types of dummy database entries used for testing or categorization purposes.
        /// </summary>
        /// <remarks>This enumeration defines two values, A and B, which can be used to distinguish
        /// between different kinds of dummy database entries. The intended usage may vary depending on the context in
        /// which the enumeration is applied.</remarks>
        private enum DummyDbType
        {
            A = 1,
            B = 2
        }

        /// <summary>
        /// Represents a database parameter with associated type information for use in data access operations.
        /// </summary>
        /// <remarks>This class implements the IDataParameter interface, allowing it to be used in ADO.NET
        /// scenarios. It encapsulates properties such as SqlDbType and DbType to specify the parameter's data type, as
        /// well as direction, value, and source column information. Use this class to define parameters when executing
        /// database commands that require explicit type and direction specification.</remarks>
        private class ParameterWithDbType :
            IDataParameter
        {
            /// <summary>
            /// Gets or sets the SQL data type used for database operations.
            /// </summary>
            /// <remarks>This property allows the specification of the data type for SQL parameters,
            /// which can affect how data is processed and stored in the database. Ensure that the assigned value
            /// corresponds to the expected SQL data type for the database schema.</remarks>
            public DummyDbType SqlDbType { get; set; }

            /// <summary>
            /// Gets or sets the database type associated with the current instance.
            /// </summary>
            /// <remarks>This property is used to specify the type of database that the instance
            /// interacts with, which can affect how data is processed and stored. Valid values correspond to the types
            /// defined in the DbType enumeration.</remarks>
            public DbType DbType { get; set; }

            /// <summary>
            /// Gets or sets the direction of the parameter, indicating whether it is used for input, output, or both in
            /// a database operation.
            /// </summary>
            /// <remarks>This property is typically used when configuring parameters for database
            /// commands or stored procedures. Setting the correct direction is essential for ensuring that the
            /// parameter behaves as expected during execution, especially when retrieving output values or passing
            /// input values to the database.</remarks>
            public ParameterDirection Direction { get; set; }

            /// <summary>
            /// Gets a value indicating whether the associated value can be null.
            /// </summary>
            /// <remarks>This property always returns <see langword="false"/>, indicating that the
            /// value is not nullable.</remarks>
            public bool IsNullable => false;

            /// <summary>
            /// Gets or sets the name of the parameter, which is used to identify it in a command or stored procedure.
            /// </summary>
            public string ParameterName { get; set; }

            /// <summary>
            /// Gets or sets the name of the source column that is mapped to the parameter. This property is used when
            /// performing data operations that involve mapping between a data source and a parameter.
            /// </summary>
            public string SourceColumn { get; set; }

            /// <summary>
            /// Gets or sets the version of the data row to use when updating the data source.
            /// </summary>
            /// <remarks>This property specifies which version of the data row is considered during
            /// update operations. Valid values are defined by the DataRowVersion enumeration, such as Current,
            /// Original, or Proposed. Setting this property allows control over whether updates use the original
            /// values, current values, or proposed values in the data row.</remarks>
            public DataRowVersion SourceVersion { get; set; }

            /// <summary>
            /// Gets or sets the value of the parameter, which is used when executing a command against a database.
            /// The value can be of any type that is compatible with the database column.
            /// </summary>
            public object Value { get; set; }
        }

        /// <summary>
        /// Represents a database parameter that implements the IDataParameter interface, providing properties to define
        /// its characteristics and behavior for use in database operations.
        /// </summary>
        /// <remarks>This class is used to configure parameters for database commands, such as specifying
        /// the parameter's type, direction, value, and source column mapping. It enables precise control over how data
        /// is passed to and from a database, supporting scenarios like executing stored procedures or updating data
        /// sources. The properties exposed by this class allow developers to define input, output, and bidirectional
        /// parameters, as well as map parameters to specific columns and data row versions. The class always indicates
        /// that its value is not nullable.</remarks>
        private class ParameterWithoutDbType :
            IDataParameter
        {
            /// <summary>
            /// Gets or sets the database type associated with the current instance.
            /// </summary>
            /// <remarks>This property is used to specify the type of database that the instance
            /// interacts with, which can affect how data is processed and stored. Valid values correspond to the types
            /// defined in the DbType enumeration.</remarks>
            public DbType DbType { get; set; }

            /// <summary>
            /// Gets or sets the direction of the parameter, indicating whether it is used for input, output, or both in
            /// a database operation.
            /// </summary>
            /// <remarks>This property is typically used when configuring parameters for database
            /// commands or stored procedures. Setting the correct direction is essential for ensuring that the
            /// parameter behaves as expected during execution, especially when retrieving output values or passing
            /// input values to the database.</remarks>
            public ParameterDirection Direction { get; set; }

            /// <summary>
            /// Gets a value indicating whether the associated value can be null.
            /// </summary>
            /// <remarks>This property always returns <see langword="false"/>, indicating that the
            /// value is not nullable.</remarks>
            public bool IsNullable => false;

            /// <summary>
            /// Gets or sets the name of the parameter, which is used to identify it in a command or stored procedure.
            /// </summary>
            public string ParameterName { get; set; }

            /// <summary>
            /// Gets or sets the name of the source column that is mapped to the parameter. This property is used when
            /// performing data operations that involve mapping between a data source and a parameter.
            /// </summary>
            public string SourceColumn { get; set; }

            /// <summary>
            /// Gets or sets the version of the data row to use when updating the data source.
            /// </summary>
            /// <remarks>This property specifies which version of the data row is considered during
            /// update operations. Valid values are defined by the DataRowVersion enumeration, such as Current,
            /// Original, or Proposed. Setting this property allows control over whether updates use the original
            /// values, current values, or proposed values in the data row.</remarks>
            public DataRowVersion SourceVersion { get; set; }

            /// <summary>
            /// Gets or sets the value of the parameter, which is used when executing a command against a database.
            /// The value can be of any type that is compatible with the database column.
            /// </summary>
            public object Value { get; set; }
        }

        /// <summary>
        /// Represents a database parameter that can be used in database commands, providing properties to configure its
        /// behavior and characteristics.
        /// </summary>
        /// <remarks>This class implements the IDataParameter interface, allowing for the specification of
        /// parameter details such as type, direction, and value. It is essential for managing parameters in database
        /// operations effectively.</remarks>
        private class ParameterNoEnumDbType :
            IDataParameter
        {
            /// <summary>
            /// Gets or sets the database type of the parameter, which determines how the value is interpreted by the
            /// database.
            /// </summary>
            /// <remarks>The DbType property specifies the data type for the parameter when executing
            /// database commands. Setting this property ensures that the parameter is sent to the database with the
            /// correct type, which can affect compatibility and performance. It is important to set DbType
            /// appropriately to match the expected type in the database schema.</remarks>
            DbType IDataParameter.DbType
            {
                get => throw new System.NotImplementedException();
                set => throw new System.NotImplementedException();
            }

            /// <summary>
            /// Gets or sets the direction of the parameter, indicating whether it is used for input, output, or both in
            /// </summary>
            public int SomeInt { get; set; }

            /// <summary>
            /// Gets or sets the direction of the parameter, indicating whether it is used for input, output, or both in
            /// a database operation.
            /// </summary>
            /// <remarks>This property is typically used when configuring parameters for database
            /// commands or stored procedures. Setting the correct direction is essential for ensuring that the
            /// parameter behaves as expected during execution, especially when retrieving output values or passing
            /// input values to the database.</remarks>
            public ParameterDirection Direction { get; set; }

            /// <summary>
            /// Gets a value indicating whether the associated value can be null.
            /// </summary>
            /// <remarks>This property always returns <see langword="false"/>, indicating that the
            /// value is not nullable.</remarks>
            public bool IsNullable => false;

            /// <summary>
            /// Gets or sets the name of the parameter, which is used to identify it in a command or stored procedure.
            /// </summary>
            public string ParameterName { get; set; }

            /// <summary>
            /// Gets or sets the name of the source column that is mapped to the parameter. This property is used when
            /// performing data operations that involve mapping between a data source and a parameter.
            /// </summary>
            public string SourceColumn { get; set; }

            /// <summary>
            /// Gets or sets the version of the data row to use when updating the data source.
            /// </summary>
            /// <remarks>This property specifies which version of the data row is considered during
            /// update operations. Valid values are defined by the DataRowVersion enumeration, such as Current,
            /// Original, or Proposed. Setting this property allows control over whether updates use the original
            /// values, current values, or proposed values in the data row.</remarks>
            public DataRowVersion SourceVersion { get; set; }

            /// <summary>
            /// Gets or sets the value of the parameter, which is used when executing a command against a database.
            /// The value can be of any type that is compatible with the database column.
            /// </summary>
            public object Value { get; set; }
        }

        /// <summary>
        /// Verifies that the GetOrAdd method returns a no-op delegate when the specified type does not define a
        /// provider-specific DbType property.
        /// </summary>
        /// <remarks>This test ensures that ProviderDbTypeSetterCache.GetOrAdd always returns a non-null
        /// delegate, even when no provider-specific DbType property exists.</remarks>
        [Fact]
        public void GetOrAdd_ReturnsNoopDelegate_WhenNoProviderDbTypeProperty()
        {
            var setter = ProviderDbTypeSetterCache.GetOrAdd(typeof(ParameterNoEnumDbType));

            Assert.NotNull(setter);

            var parameter = new ParameterNoEnumDbType
            {
                Value = 123,
                SomeInt = 7
            };

            setter(parameter, 9999);

            Assert.Equal(123, parameter.Value);
            Assert.Equal(7, parameter.SomeInt);
        }

        /// <summary>
        /// Verifies that the GetOrAdd method returns a non-null delegate when the specified type contains a
        /// ProviderDbType property.
        /// </summary>
        /// <remarks>This test ensures that ProviderDbTypeSetterCache correctly retrieves or creates a
        /// delegate for types with a ProviderDbType property. It validates that the cache mechanism works as expected
        /// and that the returned delegate is usable for further operations.</remarks>
        [Fact]
        public void GetOrAdd_ReturnsDelegate_WhenProviderDbTypePropertyExists()
        {
            var setter = ProviderDbTypeSetterCache.GetOrAdd(typeof(ParameterWithDbType));
            Assert.NotNull(setter);
            Assert.Contains("lambda_method", setter.Method.Name);
        }

        /// <summary>
        /// Verifies that a delegate correctly assigns an enum value to the SqlDbType property of a ParameterWithDbType
        /// instance.
        /// </summary>
        /// <remarks>This test ensures that the setter delegate obtained from ProviderDbTypeSetterCache
        /// properly sets the SqlDbType property when provided with an integer value representing a DummyDbType enum. It
        /// is important for validating that the delegate handles enum assignment as expected.</remarks>
        [Fact]
        public void Delegate_SetsEnumPropertyCorrectly()
        {
            var setter = ProviderDbTypeSetterCache.GetOrAdd(typeof(ParameterWithDbType));
            var param = new ParameterWithDbType();
            setter(param, (int)DummyDbType.B);
            Assert.Equal(DummyDbType.B, param.SqlDbType);
        }

        /// <summary>
        /// Verifies that the ProviderDbTypeSetterCache.GetOrAdd method returns the same delegate instance for repeated
        /// calls with the same type.
        /// </summary>
        /// <remarks>This test ensures that delegate caching is functioning correctly, which is important
        /// for performance and consistency when retrieving delegates for identical types.</remarks>
        [Fact]
        public void GetOrAdd_ReturnsSameDelegate_ForSameType()
        {
            var setter1 = ProviderDbTypeSetterCache.GetOrAdd(typeof(ParameterWithDbType));
            var setter2 = ProviderDbTypeSetterCache.GetOrAdd(typeof(ParameterWithDbType));
            Assert.Same(setter1, setter2);
        }
    }
}
