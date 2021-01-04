using Microsoft.Extensions.Logging;

namespace Device.Net
{
    public static class LoggingExtensions
    {
        public static void LogDataTransfer(this ILogger logger, Trace trace)
            => logger?.LogDebug("{trace}", trace);
    }
}

