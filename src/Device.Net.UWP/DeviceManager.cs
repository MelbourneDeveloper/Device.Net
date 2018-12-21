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
        public async Task<IEnumerable<string>> GetDeviceIds(uint? vendorId, uint? productId, DeviceType deviceType)
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
            var deviceIds = deviceInformationCollection.Select(d => d.Id).ToList();
            return deviceIds;
        }
        #endregion
    }
}
