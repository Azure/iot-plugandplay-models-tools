using Microsoft.Extensions.Logging;
using Moq;
using System;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Azure.DigitalTwins.Resolver.Tests
{
    public class Mocks
    {
        public static Mock<ILogger> GetGenericILogger()
        {
            Mock<ILogger> logger = new Mock<ILogger>();
            logger.Setup(x =>
                x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>())
                ).Verifiable();

            return logger;
        }
    }
}
