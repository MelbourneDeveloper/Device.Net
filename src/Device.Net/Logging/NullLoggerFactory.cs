
using System;

namespace Microsoft.Extensions.Logging.Abstractions
{
    public class NullLoggerFactory : ILoggerFactory
    {
        public static NullLoggerFactory Instance { get; } = new NullLoggerFactory();

        public ILogger CreateLogger<T>() => new NullLogger();

        public ILogger CreateLogger(string name) => new NullLogger();
    }

    public class DummyDisposable : IDisposable
    {
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        {
        }
    }

    public class NullLogger : ILogger
    {
        public static NullLogger Instance { get; } = new NullLogger();

        public IDisposable BeginScope(string messageFormat, params object[] args) => new DummyDisposable();

        public void LogDebug(string message, params object[] args)
        {
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
        }

        public void LogInformation(string message, params object[] args)
        {
        }

        public void LogTrace<T>(T state)
        {
        }

        public void LogWarning(string message, params object[] args)
        {
        }
    }
}


