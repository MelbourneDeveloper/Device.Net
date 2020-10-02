using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceManager
    {
        Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync();
        Task<IDevice> GetDevice(ConnectedDeviceDefinition connectedDeviceDefinition);
    }
}