using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceFactory
    {
        Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitions(DeviceDefinition deviceDefinition);
        IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition);
        DeviceType DeviceType { get; }
    }
}
