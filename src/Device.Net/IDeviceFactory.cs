using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceFactory
    {
        Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync();
        Task<IDevice> GetDevice(ConnectedDeviceDefinition deviceDefinition);
        DeviceType DeviceType { get; }
    }
}
