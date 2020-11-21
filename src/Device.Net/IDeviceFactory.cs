using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceFactory
    {
        Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync();
        Task<IDevice> GetDeviceAsync(ConnectedDeviceDefinition deviceDefinition);
        Task<bool> SupportsDeviceAsync(ConnectedDeviceDefinition deviceDefinition);
    }
}
