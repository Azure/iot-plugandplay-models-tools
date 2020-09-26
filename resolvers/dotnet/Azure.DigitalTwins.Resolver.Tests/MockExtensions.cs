using Microsoft.Extensions.Logging;
using Moq;
using System;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Azure.DigitalTwins.Resolver.Tests
{
    public static class MockExtensions
    {
        public static void ValidateLog(this Mock<ILogger> mockLogger, string message, LogLevel level, Times times)
        {
            mockLogger.Verify(l =>
                l.Log(level,
                      It.IsAny<EventId>(),
                      It.Is<It.IsAnyType>((o, _) => o.ToString() == message),
                      It.IsAny<Exception>(),
                      It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), times);
        }
    }
}
