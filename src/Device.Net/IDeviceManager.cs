using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceManager
    {
        List<IDeviceFactory> DeviceFactories { get; }
        Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync();
        IDevice GetDevice(ConnectedDeviceDefinition connectedDeviceDefinition);
        bool IsInitialized { get; }
    }

    //public static class DeviceManagerExtensions
    //{
    //    public static Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(
    //        this IDeviceManager deviceManager, 
    //        IList<FilterDeviceDefinition> deviceDefinitions)
    //    {
    //        deviceManager.GetConnectedDeviceDefinitionsAsync(async (deviceFactory) => 
    //        {
    //            var retVal = new List<ConnectedDeviceDefinition>();

    //            var connectedDeviceDefinitions = await deviceFactory.GetConnectedDeviceDefinitionsAsync(deviceDefinition);

    //            foreach (var connectedDeviceDefinition in connectedDeviceDefinitions)
    //            {
    //                //Don't add the same device twice
    //                //Note: this probably won't cause issues where there is no DeviceId, but funny behaviour is probably going on when there isn't anyway...
    //                if (retVal.Select(d => d.DeviceId).Contains(connectedDeviceDefinition.DeviceId)) continue;

    //                retVal.Add(connectedDeviceDefinition);
    //            }

    //        });
    //    }
    //}
}