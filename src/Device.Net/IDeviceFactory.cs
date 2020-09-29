using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceFactory
    {
        Task<IReadOnlyCollection<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync();
        Task<IDevice> GetDevice(ConnectedDeviceDefinition deviceDefinition);
        DeviceType DeviceType { get; }
    }
}
