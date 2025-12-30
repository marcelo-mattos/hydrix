using Hydrix.Attributes.Schemas.Contract;
using Hydrix.Attributes.Schemas.Contract.Base;
using Xunit;

namespace Hydrix.UnitTests.Attributes.Schemas.Contract
{
    /// <summary>
    /// Provides unit tests for the ISqlProcedureAttribute interface to verify its type characteristics and inheritance.
    /// </summary>
    /// <remarks>These tests ensure that ISqlProcedureAttribute is defined as an interface and that it
    /// inherits from Base.ISqlSchemaAttribute. This class is intended for use in automated test suites to validate
    /// interface design contracts.</remarks>
    public class ISqlProcedureAttributeTestsImpl
    {
        /// <summary>
        /// Verifies that the ISqlProcedureAttribute type is an interface.
        /// </summary>
        [Fact]
        public void ISqlProcedureAttribute_IsInterface()
        {
            Assert.True(typeof(ISqlProcedureAttribute).IsInterface);
        }

        /// <summary>
        /// Verifies that the ISqlProcedureAttribute interface inherits from the ISqlSchemaAttribute interface.
        /// </summary>
        /// <remarks>This test ensures that ISqlProcedureAttribute implements the ISqlSchemaAttribute
        /// interface as part of the type hierarchy. Use this test to validate interface inheritance relationships in
        /// the codebase.</remarks>
        [Fact]
        public void ISqlProcedureAttribute_InheritsFrom_ISqlSchemaAttribute()
        {
            var baseInterfaces = typeof(ISqlProcedureAttribute).GetInterfaces();
            Assert.Contains(typeof(ISqlSchemaAttribute), baseInterfaces);
        }
    }
}