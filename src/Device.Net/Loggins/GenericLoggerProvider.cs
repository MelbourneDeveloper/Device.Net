# if !NET45

using Microsoft.Extensions.Logging;
using System;

namespace CallbackLogger
{
    public class GenericLoggerProvider : ILoggerProvider
    {
        private Func<string, ILogger> _createLogger;

        public GenericLoggerProvider(Func<string, ILogger> createLogger)
        {
            _createLogger = createLogger;
        }

        public ILogger CreateLogger(string categoryName) => _createLogger(categoryName);

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public void Dispose() => _createLogger = null;
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    }
}

#endif