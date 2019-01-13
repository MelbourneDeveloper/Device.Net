using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceFactory
    {
        Task<IEnumerable<DeviceDefinitionPlus>> GetConnectedDeviceDefinitions(DeviceDefinition deviceDefinition);
        IDevice GetDevice(DeviceDefinitionPlus deviceDefinition);
        DeviceType DeviceType { get; }
    }
}
