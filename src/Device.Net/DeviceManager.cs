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
        public async Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(DeviceDefinition deviceDefinition)
        {
            var retVal = new List<DeviceDefinition>();
            foreach (var deviceFactory in DeviceFactories)
            {
                retVal.AddRange(await deviceFactory.GetConnectedDeviceDefinitions(deviceDefinition));
            }

            return retVal;
        }

        //TODO: Duplicate code here...
        public IDevice GetDevice(DeviceDefinition filterDeviceDefinition)
        {
            foreach (var deviceFactory in DeviceFactories)
            {
                if (filterDeviceDefinition.DeviceType.HasValue && (deviceFactory.DeviceType != filterDeviceDefinition.DeviceType)) continue;
                return deviceFactory.GetDevice(filterDeviceDefinition);
            }

            throw new System.Exception("Couldn't get a device");
        }

        public async Task<List<IDevice>> GetDevices(IList<DeviceDefinition> deviceDefinitions)
        {
            var retVal = new List<IDevice>();

            foreach (var deviceFactory in DeviceFactories)
            {
                foreach (var filterDeviceDefinition in deviceDefinitions)
                {
                    if (filterDeviceDefinition.DeviceType.HasValue && (deviceFactory.DeviceType != filterDeviceDefinition.DeviceType)) continue;

                    var connectedDeviceDefinitions = await deviceFactory.GetConnectedDeviceDefinitions(filterDeviceDefinition);
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

        #region Public Static Methods
        public static bool IsDefinitionMatch(DeviceDefinition filterDevice, DeviceDefinition actualDevice)
        {
            return
                (!filterDevice.VendorId.HasValue || filterDevice.VendorId == actualDevice.VendorId) &&
                (!filterDevice.ProductId.HasValue || filterDevice.ProductId == actualDevice.ProductId) &&
                (!filterDevice.DeviceType.HasValue || filterDevice.DeviceType == actualDevice.DeviceType) &&
                (!filterDevice.UsagePage.HasValue || filterDevice.UsagePage == actualDevice.UsagePage);
        }
        #endregion
    }
}
