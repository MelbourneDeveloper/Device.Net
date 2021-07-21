using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Device.Net
{
    public interface IFactoryBuilder
    {
        ILoggerFactory LoggerFactory { get; }
        ReadOnlyCollection<IDeviceFactory> Factories { get; }
        IDeviceFactory Build();
    }
}
