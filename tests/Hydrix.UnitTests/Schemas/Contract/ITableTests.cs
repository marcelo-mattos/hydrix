using Hydrix.Schemas.Contract;
using Xunit;

namespace Hydrix.UnitTests.Schemas
{
    /// <summary>
    /// Contains unit tests for verifying the contract and implementation of the ITable interface.
    /// </summary>
    /// <remarks>This class is intended for use in test projects to ensure that types implementing ITable
    /// conform to expected behaviors and interface requirements.</remarks>
    public class ITableTests
    {
        /// <summary>
        /// Represents a placeholder implementation of the ITable interface for internal use.
        /// </summary>
        /// <remarks>This class is intended for scenarios where a non-functional or mock ITable is
        /// required, such as testing or stubbing. It should not be used in production code.</remarks>
        private class DummyTable :
            ITable
        { }

        /// <summary>
        /// Verifies that the DummySqlEntity class implements the ITable interface.
        /// </summary>
        /// <remarks>This test ensures that DummySqlEntity can be assigned to a variable of type
        /// ITable, confirming interface implementation. Use this test to validate that changes to DummySqlEntity do
        /// not break its contract with ITable.</remarks>
        [Fact]
        public void DummySqlEntity_Implements_ITable()
        {
            // Arrange
            var entity = new DummyTable();

            // Act & Assert
            Assert.IsAssignableFrom<ITable>(entity);
        }

        /// <summary>
        /// Verifies that the ITable type is a public interface.
        /// </summary>
        /// <remarks>This test ensures that ITable is defined as a public interface, which is required
        /// for consumers to implement or use it outside of its defining assembly.</remarks>
        [Fact]
        public void ITable_Is_Public_Interface()
        {
            // Act
            var type = typeof(ITable);

            // Assert
            Assert.True(type.IsInterface);
            Assert.True(type.IsPublic);
        }
    }
}