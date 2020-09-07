using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDeviceFactory
    {
        Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync();
        IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition);
        DeviceType DeviceType { get; }
    }

    //public static class DeviceFactoryExtensions
    //{
    //    public static async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(
    //        this IDeviceFactory deviceFactory,
    //        IList<FilterDeviceDefinition> deviceDefinitions)
    //    {
    //        if (deviceFactory == null) throw new ArgumentNullException(nameof(deviceFactory));
    //        if (deviceDefinitions == null) throw new ArgumentNullException(nameof(deviceDefinitions));

    //        var retVal = new List<ConnectedDeviceDefinition>();

    //        var connectedDeviceDefinitions = await deviceFactory.GetConnectedDeviceDefinitionsAsync(()=> { });

    //        foreach (var connectedDeviceDefinition in connectedDeviceDefinitions)
    //        {
    //            //Don't add the same device twice
    //            //Note: this probably won't cause issues where there is no DeviceId, but funny behaviour is probably going on when there isn't anyway...
    //            if (retVal.Select(d => d.DeviceId).Contains(connectedDeviceDefinition.DeviceId)) continue;

    //            retVal.Add(connectedDeviceDefinition);
    //        }


    //    }
    //}

}
