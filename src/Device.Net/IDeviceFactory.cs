using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceFactory
    {
        Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitions(FilterDeviceDefinition deviceDefinition);
        IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition);
        DeviceType DeviceType { get; }
    }
}
