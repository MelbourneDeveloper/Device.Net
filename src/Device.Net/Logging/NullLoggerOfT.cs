using System;

namespace Microsoft.Extensions.Logging.Abstractions
{

    internal class NullLogger<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public NullLogger(ILoggerFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _logger = factory.CreateLogger(typeof(T).Name);
        }

        public IDisposable BeginScope(string messageFormat, params object[] args) => _logger.BeginScope(messageFormat, args);
        public void LogDebug(string message, params object[] args) => _logger.LogDebug(message, args);
        public void LogError(EventId eventId, Exception exception, string message, params object[] args) => _logger.LogError(eventId, exception, message, args);
        public void LogError(Exception exception, string message, params object[] args) => _logger.LogError(exception, message, args);
        public void LogInformation(string message, params object[] args) => _logger.LogInformation(message, args);
        public void LogTrace<T1>(T1 state) => _logger.LogTrace(state);
        public void LogWarning(string message, params object[] args) => _logger.LogWarning(message, args);
    }
}
