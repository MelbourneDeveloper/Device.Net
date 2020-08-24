# if !NET45

using Microsoft.Extensions.Logging;
using System;

namespace CallbackLogger
{
    public class LogMessage
    {
        public LogMessage(
            LogLevel? logLevel,
            EventId eventId,
            object state,
            Exception? exception,
            bool isScope,
            ILogger logger)
        {
            LogLevel = logLevel;
            EventId = eventId;
            State = state;
            Exception = exception;
            IsScope = isScope;
            Logger = logger;
        }

        public LogLevel? LogLevel { get; }
        public EventId EventId { get; }
        public object State { get; }
        public Exception? Exception { get; }
        public bool IsScope { get; }
        public ILogger Logger { get; }
    }
}

#endif