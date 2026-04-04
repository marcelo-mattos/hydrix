using Hydrix.Mapping;
using System;
using Xunit;

namespace Hydrix.UnitTests.Mapping
{
    /// <summary>
    /// Provides unit tests for the ColumnMap class to verify correct property assignment and handling of null values
    /// for setter and reader delegates.
    /// </summary>
    /// <remarks>This class uses the xUnit testing framework to ensure that the ColumnMap constructor assigns
    /// properties as expected and supports null values for optional delegates. The tests validate both the assignment
    /// and invocation of delegates, as well as the behavior when delegates are not provided.</remarks>
    public class ColumnMapTests
    {
        /// <summary>
        /// Verifies that the ColumnMap constructor correctly assigns the Name, Setter, and Reader properties, and that
        /// the provided delegates are invoked as expected.
        /// </summary>
        /// <remarks>This test ensures that the ColumnMap instance initializes its properties with the
        /// supplied arguments and that the setter and reader delegates function as intended when invoked.</remarks>
        [Fact]
        public void Constructor_AssignsPropertiesCorrectly()
        {
            // Arrange
            string name = "MyField";
            bool setterCalled = false;
            bool readerCalled = false;

            Action<object, object> setter = (obj, val) => setterCalled = true;
            FieldReader reader = (record, ordinal) => { readerCalled = true; return 42; };

            // Act
            var map = new ColumnMap(name, setter, reader);

            // Assert
            Assert.Equal(name, map.Name);
            Assert.Equal(setter, map.Setter);
            Assert.Equal(reader, map.Reader);

            // Test delegates
            map.Setter(new object(), new object());
            Assert.True(setterCalled);

            var result = map.Reader(null, 0);
            Assert.True(readerCalled);
            Assert.Equal(42, result);
        }

        /// <summary>
        /// Verifies that the ColumnMap constructor allows null values for the setter and reader parameters without
        /// throwing exceptions.
        /// </summary>
        /// <remarks>This test ensures that a ColumnMap instance can be created with only a name
        /// specified, and that both the Setter and Reader properties are null when not provided. This supports
        /// scenarios where column mapping does not require value assignment or retrieval.</remarks>
        [Fact]
        public void Constructor_AllowsNullSetterAndReader()
        {
            // Arrange
            string name = "NullField";

            // Act
            var map = new ColumnMap(name, null, null);

            // Assert
            Assert.Equal(name, map.Name);
            Assert.Null(map.Setter);
            Assert.Null(map.Reader);
        }
    }
}
