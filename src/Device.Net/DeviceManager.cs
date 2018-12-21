using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net
{
    public class DeviceManager
    {
        #region Public Properties
        public List<IDeviceFactory> DeviceFactories { get; } = new List<IDeviceFactory>();
        #endregion

        #region Public Static Properties
        public static DeviceManager Current { get; } = new DeviceManager();
        #endregion

        #region Public Methods
        public async Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(uint? vendorId, uint? productId)
        {
            var retVal = new List<DeviceDefinition>();
            foreach (var deviceFactory in DeviceFactories)
            {
                var definitions = await deviceFactory.GetConnectedDeviceDefinitions(vendorId, productId);
                foreach (var deviceDefinition in definitions)
                {
                    retVal.Add(deviceDefinition);
                }
            }

            return retVal;
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

        public async Task<IEnumerable<T>> GetDevices<T>(uint? vendorId, uint? productId)
        {
            var retVal = new List<T>();

            var deviceFactory = DeviceFactories.FirstOrDefault(df => df.GetType().GenericTypeArguments.FirstOrDefault() == typeof(T));
            if (deviceFactory == null)
            {
                throw new Exception($"No DeviceFactory has been defined for the type {typeof(T)}. Try calling {typeof(T)}.Register before calling {nameof(GetDevices)}");
            }

            var definitions = await deviceFactory.GetConnectedDeviceDefinitions(vendorId, productId);

            foreach (var deviceDefinition in definitions)
            {
                retVal.Add(GetDevice<T>(deviceDefinition));
            }

            return retVal;
        }

        #endregion
    }
}
