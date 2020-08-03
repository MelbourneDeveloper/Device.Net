using System;

namespace Microsoft.Extensions.Logging
{
    public interface ILogger
    {
        void LogError(Exception exception, string message, params object[] args);
        void LogInformation(string message, params object[] args);
    }
}
