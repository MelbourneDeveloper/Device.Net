namespace Microsoft.Extensions.Logging
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(string categoryName);
    }
}
