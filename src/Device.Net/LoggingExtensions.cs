using Microsoft.Extensions.Logging;

namespace Device.Net
{
    internal static class LoggingExtensions
    {
        public static void LogDataTransfer(this ILogger logger, Trace trace)
            => logger?.LogDebug("{trace}", trace);
    }
}

