using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Materializers.Contract
{
    /// <summary>
    /// Provides unit tests for the ISqlMaterializer interface to verify its exposed properties and behaviors.
    /// </summary>
    /// <remarks>These tests ensure that the ISqlMaterializer implementation correctly reports connection
    /// state, transaction activity, timeout settings, and disposal status. The class uses mock objects to isolate and
    /// validate the expected values of each property.</remarks>
    public partial class ISqlMaterializerTests
    {
        /// <summary>
        /// Verifies that the IsTransactionActive property of ISqlMaterializer returns the expected value when mocked.
        /// </summary>
        /// <remarks>This test ensures that the IsTransactionActive property can be reliably set and
        /// retrieved using a mock implementation, confirming correct behavior for unit testing scenarios.</remarks>
        [Fact]
        public void IsTransactionActive_ReturnsExpectedValue()
        {
            _materializerMock.SetupGet(m => m.IsTransactionActive).Returns(true);
            Assert.True(_materializerMock.Object.IsTransactionActive);
        }

        /// <summary>
        /// Verifies that the ConnectionString property of ISqlMaterializer returns the expected value.
        /// </summary>
        /// <remarks>This test ensures that the mock implementation of ISqlMaterializer correctly provides
        /// the configured connection string. Use this test to validate property behavior when setting up
        /// database-related mocks.</remarks>
        [Fact]
        public void ConnectionString_ReturnsExpectedValue()
        {
            _materializerMock.SetupGet(m => m.ConnectionString).Returns("TestConnection");
            Assert.Equal("TestConnection", _materializerMock.Object.ConnectionString);
        }

        /// <summary>
        /// Verifies that the State property of ISqlMaterializer returns the expected connection state value.
        /// </summary>
        /// <remarks>This test ensures that when the State property is set up to return
        /// ConnectionState.Open, the property returns the correct value. Use this test to validate property behavior in
        /// mock implementations.</remarks>
        [Fact]
        public void State_ReturnsExpectedValue()
        {
            _materializerMock.SetupGet(m => m.State).Returns(ConnectionState.Open);
            Assert.Equal(ConnectionState.Open, _materializerMock.Object.State);
        }

        /// <summary>
        /// Verifies that the Timeout property of ISqlMaterializer can be set and retrieved as expected.
        /// </summary>
        /// <remarks>This test ensures that assigning a value to the Timeout property updates its value
        /// correctly and that the initial value can be set using SetupProperty. It is intended to validate the
        /// property’s get and set behavior in a mock context.</remarks>
        [Fact]
        public void Timeout_GetSet_Works()
        {
            _materializerMock.SetupProperty(m => m.Timeout, 30);
            _materializerMock.Object.Timeout = 15;
            Assert.Equal(15, _materializerMock.Object.Timeout);
        }

        /// <summary>
        /// Verifies that the IsDisposed property of ISqlMaterializer returns the expected value when set via a mock.
        /// </summary>
        /// <remarks>This test ensures that the IsDisposed property correctly reflects the disposed state
        /// as configured in the mock. It is useful for validating behavior in scenarios where resource disposal is
        /// tracked.</remarks>
        [Fact]
        public void IsDisposed_ReturnsExpectedValue()
        {
            _materializerMock.SetupGet(m => m.IsDisposed).Returns(true);
            Assert.True(_materializerMock.Object.IsDisposed);
        }

        /// <summary>
        /// Verifies that the IsDisposing property of ISqlMaterializer returns the expected value when configured via a
        /// mock.
        /// </summary>
        /// <remarks>This test ensures that the IsDisposing property can be set and retrieved correctly
        /// using a mocking framework. It is intended to validate the behavior of the property in unit testing
        /// scenarios.</remarks>
        [Fact]
        public void IsDisposing_ReturnsExpectedValue()
        {
            _materializerMock.SetupGet(m => m.IsDisposing).Returns(true);
            Assert.True(_materializerMock.Object.IsDisposing);
        }
    }
}