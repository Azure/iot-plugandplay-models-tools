using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using System;
using Azure.IoT.DeviceModelsRepository.CLI;

namespace Azure.IoT.DeviceModelsRepository.Validation.Tests
{
    public class ValidateTests
    {
        [Test]
        public async Task FailsOnMissingRootId()
        {
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;
            var fileInfo = new FileInfo("../../../TestModelRepo/badfile/AllBad.json");
            var fileName = fileInfo.FullName;
            var validationResult = await Validations.Validate(fileInfo, logger);
            Assert.False(validationResult);
            mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Exactly(7));
        }
    }
}