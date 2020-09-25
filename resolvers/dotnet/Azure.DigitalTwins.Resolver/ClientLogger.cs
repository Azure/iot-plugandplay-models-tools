using Microsoft.Extensions.Logging;

namespace Azure.DigitalTwins.Resolver
{
    /// <summary>
    /// Wrapper around ILogger to ensure safe logging across the client.
    /// </summary>
    public class ClientLogger
    {
        private readonly ILogger _logger;

        public ClientLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void LogInformation(string info, params object[] args)
        {
            if (_logger == null)
                return;
            _logger.LogInformation(info, args);
        }

        public void LogWarning(string info, params object[] args)
        {
            if (_logger == null)
                return;
            _logger.LogWarning(info, args);
        }

        public void LogError(string info, params object[] args)
        {
            if (_logger == null)
                return;
            _logger.LogError(info, args);
        }
    }
}
