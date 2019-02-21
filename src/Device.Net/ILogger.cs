using System;

namespace Device.Net
{
    public interface ILogger
    {
        void Log(string message, string region, Exception ex, LogLevel logLevel);
    }
}
