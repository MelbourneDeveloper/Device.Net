using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceManager
    {
        IReadOnlyCollection<IDeviceFactory> DeviceFactories { get; }
        Task<IReadOnlyCollection<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync();
        Task<IDevice> GetDevice(ConnectedDeviceDefinition connectedDeviceDefinition);
    }
}