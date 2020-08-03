using Microsoft.Extensions.Logging;

namespace Device.Net
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(string categoryName);
    }
}
