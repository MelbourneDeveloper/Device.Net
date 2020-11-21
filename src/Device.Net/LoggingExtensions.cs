#if !NET45

using Microsoft.Extensions.Logging;

namespace Device.Net
{
    public static class LoggingExtensions
    {
        public static void LogTrace<T>(this ILogger logger, T state, string message = null) => logger?.Log(LogLevel.Trace, default, state, null, (s, e) => $"Trace\r\nState: {state} Message: {message}");
    }
}

#endif

