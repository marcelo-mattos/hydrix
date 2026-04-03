using Hydrix.Extensions;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace Hydrix.UnitTests.Extensions
{
    /// <summary>
    /// Contains unit tests for the ParameterExtensions class, specifically testing the AsIDataParameters method.
    /// </summary>
    /// <remarks>These tests validate the behavior of the AsIDataParameters method when provided with various
    /// types of input, including null, single IDataParameter instances, collections of IDataParameter, and unsupported
    /// types. Each test ensures that the method behaves as expected under these conditions.</remarks>
    public class ParameterExtensionsTests
    {
        /// <summary>
        /// Verifies that the AsIDataParameters extension method returns an empty IDataParameters instance when the
        /// input is null.
        /// </summary>
        /// <remarks>This test ensures that passing a null object to AsIDataParameters does not result in
        /// an exception and produces an empty result, supporting safe handling of optional parameters in data
        /// operations.</remarks>
        [Fact]
        public void AsIDataParameters_ReturnsEmpty_WhenNull()
        {
            object parameters = null;
            var result = parameters.AsIDataParameters();
            Assert.Empty(result);
        }

        /// <summary>
        /// Verifies that the AsIDataParameters extension method returns an enumerable containing only the original
        /// IDataParameter instance when called on a single parameter.
        /// </summary>
        /// <remarks>This test ensures that the AsIDataParameters method correctly wraps a single
        /// IDataParameter in an enumerable collection, which is useful for APIs that expect a collection of parameters
        /// even when only one is provided.</remarks>
        [Fact]
        public void AsIDataParameters_ReturnsSingle_WhenIDataParameter()
        {
            var param = new Mock<IDataParameter>().Object;
            var result = param.AsIDataParameters();
            Assert.Single(result);
            Assert.Same(param, result.First());
        }

        /// <summary>
        /// Verifies that the AsIDataParameters extension method returns all elements from an IEnumerable of
        /// IDataParameter instances.
        /// </summary>
        /// <remarks>This test ensures that when AsIDataParameters is called on a collection containing
        /// multiple IDataParameter objects, the resulting list includes all original parameters. It validates the
        /// method's ability to preserve the input collection's contents without omission.</remarks>
        [Fact]
        public void AsIDataParameters_ReturnsAll_WhenIEnumerableOfIDataParameter()
        {
            var param1 = new Mock<IDataParameter>().Object;
            var param2 = new Mock<IDataParameter>().Object;
            IEnumerable<IDataParameter> parameters = new[] { param1, param2 };
            var result = parameters.AsIDataParameters().ToList();
            Assert.Equal(2, result.Count);
            Assert.Contains(param1, result);
            Assert.Contains(param2, result);
        }

        /// <summary>
        /// Verifies that the AsIDataParameters extension method throws an ArgumentException when called with an
        /// unsupported parameter type.
        /// </summary>
        /// <remarks>This test ensures that passing an unsupported type, such as an integer, to the
        /// AsIDataParameters method results in an appropriate exception. This helps validate that the method enforces
        /// type constraints and provides clear feedback to callers when invalid types are used.</remarks>
        [Fact]
        public void AsIDataParameters_Throws_WhenUnsupportedType()
        {
            object parameters = 123; // int is not supported
            var ex = Assert.Throws<ArgumentException>(() => parameters.AsIDataParameters().ToList());
            Assert.Contains("Parameter type", ex.Message);
        }
    }
}
