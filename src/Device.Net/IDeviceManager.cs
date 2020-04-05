using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceManager
    {
        Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(FilterDeviceDefinition deviceDefinition);
        IDevice GetDevice(ConnectedDeviceDefinition connectedDeviceDefinition);
        Task<List<IDevice>> GetDevicesAsync(IList<FilterDeviceDefinition> deviceDefinitions);
        bool IsInitialized { get; }
    }
}