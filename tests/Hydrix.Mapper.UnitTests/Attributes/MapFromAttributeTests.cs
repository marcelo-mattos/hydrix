using Hydrix.Mapper.Attributes;
using System;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Attributes
{
    /// <summary>
    /// Covers MapFromAttribute constructor and property branches.
    /// </summary>
    public class MapFromAttributeTests
    {
        /// <summary>
        /// Verifies that the constructor sets the SourceType property when a valid type is provided.
        /// </summary>
        [Fact]
        public void Ctor_SetsSourceTypeProperty()
        {
            var attr = new MapFromAttribute(typeof(string));
            Assert.Equal(typeof(string), attr.SourceType);
        }

        /// <summary>
        /// Verifies that the constructor throws ArgumentNullException when null is passed.
        /// </summary>
        [Fact]
        public void Ctor_ThrowsArgumentNullException_WhenSourceTypeIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MapFromAttribute(null));
        }

        /// <summary>
        /// Verifies that the constructor throws ArgumentException when typeof(void) is passed,
        /// preventing collision with the internal sentinel used by the plan-cache lookup.
        /// </summary>
        [Fact]
        public void Ctor_ThrowsArgumentException_WhenSourceTypeIsVoid()
        {
            Assert.Throws<ArgumentException>(() => new MapFromAttribute(typeof(void)));
        }
    }
}
