#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Microsoft.Extensions.Logging
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(string name);
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
