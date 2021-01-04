#if !NET45

using Microsoft.Extensions.Logging;

namespace Device.Net
{
    public static class LoggingExtensions
    {
        public static void LogTrace<T>(this ILogger logger, T state, string message = null)
            => logger?.LogTrace(
            "Trace - {message} Data: {state}",
             message,
             state);
    }
}

#endif

