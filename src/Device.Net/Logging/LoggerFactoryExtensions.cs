namespace Microsoft.Extensions.Logging.Abstractions
{
    public static class LoggerFactoryExtensions
    {
        public static ILogger<T> CreateLogger<T>(this ILoggerFactory loggerFactory) => new NullLogger<T>(loggerFactory);
    }
}
