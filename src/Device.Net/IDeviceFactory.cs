using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceFactory
    {
        Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(FilterDeviceDefinition deviceDefinition);
        IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition);
        DeviceType DeviceType { get; }
        ILogger Logger { get; }
    }
}
