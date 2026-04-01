using Hydrix.Orchestrator.Metadata.Internals;
using Moq;
using System;
using System.Data;
using Xunit;

namespace Hydrix.UnitTests.Orchestrator.Metadata.Internals
{
    public class FieldReaderFactoryCoverageTests
    {
        private enum StringBackedEnum
        {
            Zero = 0,
            One = 1
        }

        [Fact]
        public void Create_WithEnumAndMismatchedSourceType_FallsBackToEnumConverter()
        {
            var record = new Mock<IDataRecord>();
            record.Setup(r => r.IsDBNull(0)).Returns(false);
            record.Setup(r => r.GetValue(0)).Returns("One");

            var reader = FieldReaderFactory.Create(typeof(StringBackedEnum), typeof(string));
            var result = reader(record.Object, 0);

            Assert.Equal(StringBackedEnum.One, result);
        }
    }
}
