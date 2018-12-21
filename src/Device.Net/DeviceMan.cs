using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public class DeviceMan
    {
        public List<IDeviceFactory> DeviceFactories { get; } = new List<IDeviceFactory>();

        #region Public Static Properties
        public static DeviceMan Current { get; } = new DeviceMan();
        #endregion

        #region Public Methods
        public async Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(uint? vendorId, uint? productId, DeviceType deviceType)
        {
            return null;
        }

        public T GetDevice<T>(string deviceId)
        {
            foreach (dynamic deviceFactory in DeviceFactories)
            {
                if (deviceFactory.GetType().GenericTypeArguments.FirstOrDefault() == typeof(T))
                {
                    return (T)deviceFactory.GetDevice(deviceId);
                }
            }

            return default(T);
        }
        #endregion
    }
}
