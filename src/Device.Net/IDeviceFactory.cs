using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceFactory
    {
        Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(DeviceDefinition deviceDefinition);
        IDevice GetDevice(DeviceDefinition deviceDefinition);
        DeviceType DeviceType { get; }
    }
}
