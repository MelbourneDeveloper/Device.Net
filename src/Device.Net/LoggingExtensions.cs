#if !NET45

using Microsoft.Extensions.Logging;

namespace Device.Net
{
    public static class LoggingExtensions
    {
        public static void LogDataTransfer(this ILogger logger, Trace trace, string message = null)
            => logger?.LogTrace(
            "Trace - {message} Data: {state}",
             message,
             trace);
    }
}

#endif

