using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceManager
    {
        List<IDeviceFactory> DeviceFactories { get; }
        Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync();
        Task<IDevice> GetDevice(ConnectedDeviceDefinition connectedDeviceDefinition);
        bool IsInitialized { get; }
    }
}