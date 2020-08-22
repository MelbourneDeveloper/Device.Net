using System;

namespace Microsoft.Extensions.Logging
{
    public interface ILogger
    {
        IDisposable BeginScope(string messageFormat, params object[] args);
        void LogError(Exception exception, string message, params object[] args);
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogDebug(string message, params object[] args);
        void LogTrace<T>(T state);
    }
}
