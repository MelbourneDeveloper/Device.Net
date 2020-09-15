#pragma warning disable

using Microsoft.Extensions.Logging;
using System;

namespace Device.Net
{
    public class DummyLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName) => new DummyLogger();

        public void Dispose()
        {
        }
    }

    public class DummyDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }

    public class DummyLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) => new DummyDisposable();

        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }
    }
}


#pragma warning restore 
