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

        public async Task<List<IDevice>> GetDevices(IEnumerable<DeviceDefinition> deviceDefinitions)
        {
            var retVal = new List<IDevice>();

            foreach (var deviceFactory in DeviceFactories)
            {
                foreach (var filterDeviceDefinition in deviceDefinitions)
                {
                    if (filterDeviceDefinition.DeviceType.HasValue && (deviceFactory.DeviceType != filterDeviceDefinition.DeviceType)) continue;

                    var connectedDeviceDefinitions = await deviceFactory.GetConnectedDeviceDefinitions(filterDeviceDefinition.VendorId, filterDeviceDefinition.ProductId);
                    retVal.AddRange
                    (
                        connectedDeviceDefinitions.Select
                        (
                            connectedDeviceDefinition => deviceFactory.GetDevice(connectedDeviceDefinition)
                        ).
                        Where
                        (
                            device => device != null
                        )
                    );
                }
            }

            return retVal;
        }
        #endregion
    }
}
