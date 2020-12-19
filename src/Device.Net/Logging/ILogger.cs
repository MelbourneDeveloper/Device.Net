#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;

namespace Microsoft.Extensions.Logging
{
    public interface ILogger
    {
        IDisposable BeginScope(string messageFormat, params object[] args);
        void LogError(EventId eventId, Exception exception, string message, params object[] args);
        void LogError(Exception exception, string message, params object[] args);
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogDebug(string message, params object[] args);
        void LogTrace<T>(T state);
    }

    public interface ILogger<T> : ILogger
    {

    }
}


#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
