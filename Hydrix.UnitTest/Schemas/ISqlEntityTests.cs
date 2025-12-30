using Hydrix.Schemas;
using Xunit;

namespace Hydrix.UnitTest.Schemas
{
    /// <summary>
    /// Contains unit tests for verifying the contract and implementation of the ISqlEntity interface.
    /// </summary>
    /// <remarks>This class is intended for use in test projects to ensure that types implementing ISqlEntity
    /// conform to expected behaviors and interface requirements.</remarks>
    public class ISqlEntityTests
    {
        /// <summary>
        /// Represents a placeholder implementation of the ISqlEntity interface for internal use.
        /// </summary>
        /// <remarks>This class is intended for scenarios where a non-functional or mock ISqlEntity is
        /// required, such as testing or stubbing. It should not be used in production code.</remarks>
        private class DummySqlEntity : 
            ISqlEntity 
        { }

        /// <summary>
        /// Verifies that the DummySqlEntity class implements the ISqlEntity interface.
        /// </summary>
        /// <remarks>This test ensures that DummySqlEntity can be assigned to a variable of type
        /// ISqlEntity, confirming interface implementation. Use this test to validate that changes to DummySqlEntity do
        /// not break its contract with ISqlEntity.</remarks>
        [Fact]
        public void DummySqlEntity_Implements_ISqlEntity()
        {
            // Arrange
            var entity = new DummySqlEntity();

            // Act & Assert
            Assert.IsAssignableFrom<ISqlEntity>(entity);
        }

        /// <summary>
        /// Verifies that the ISqlEntity type is a public interface.
        /// </summary>
        /// <remarks>This test ensures that ISqlEntity is defined as a public interface, which is required
        /// for consumers to implement or use it outside of its defining assembly.</remarks>
        [Fact]
        public void ISqlEntity_Is_Public_Interface()
        {
            // Act
            var type = typeof(ISqlEntity);

            // Assert
            Assert.True(type.IsInterface);
            Assert.True(type.IsPublic);
        }
    }
}