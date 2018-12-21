using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public class DeviceManager
    {
        public List<IDeviceFactory> DeviceFactories { get; } = new List<IDeviceFactory>();

        #region Public Static Properties
        public static DeviceManager Current { get; } = new DeviceManager();
        #endregion

        #region Public Methods
        public async Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(uint? vendorId, uint? productId)
        {
            return null;
        }

        public T GetDevice<T>(DeviceDefinition deviceDefinition)
        {
            foreach (dynamic deviceFactory in DeviceFactories)
            {
                if (deviceFactory.GetType().GenericTypeArguments.FirstOrDefault() == typeof(T))
                {
                    return (T)deviceFactory.GetDevice(deviceDefinition);
                }
            }

            return default(T);
        }
        #endregion
    }
}
