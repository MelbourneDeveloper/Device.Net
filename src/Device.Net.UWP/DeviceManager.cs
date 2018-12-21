using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wde = Windows.Devices.Enumeration;

namespace Device.Net
{
    public class DeviceManager
    {
        #region Public Static Properties
        public static DeviceManager Current { get; } = new DeviceManager();
        #endregion

        #region Public Methods
        public async Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(uint? vendorId, uint? productId, DeviceType deviceType)
        {
            string aqsFilter = null;

            switch (deviceType)
            {
                case DeviceType.Hid:
                    aqsFilter = $"System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True AND System.DeviceInterface.Hid.VendorId:={vendorId} AND System.DeviceInterface.Hid.ProductId:={productId} ";
                    break;
                case DeviceType.Usb:
                    aqsFilter = "System.Devices.InterfaceClassGuid:=\"{DEE824EF-729B-4A0E-9C14-B7117D33A817}\" AND System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True AND " + $" System.DeviceInterface.WinUsb.UsbVendorId:={vendorId.Value} AND System.DeviceInterface.WinUsb.UsbProductId:={productId.Value}";
                    break;
            }

            var deviceInformationCollection = await wde.DeviceInformation.FindAllAsync(aqsFilter).AsTask();

            //TODO: return the vid/pid if we can get it from the properties. Also read/write buffer size

            var deviceIds = deviceInformationCollection.Select(d => new DeviceDefinition { DeviceId = d.Id, DeviceType = deviceType }).ToList();
            return deviceIds;
        }
        #endregion
    }
}
