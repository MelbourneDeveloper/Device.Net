using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wde = Windows.Devices.Enumeration;

namespace Device.Net
{
    public static class DeviceManager
    {
        public async static Task<IEnumerable<string>> GetDeviceIds(int? vendorId, int? productId)
        {
            var aqsFilter = $"System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True AND System.DeviceInterface.Hid.VendorId:={vendorId} AND System.DeviceInterface.Hid.ProductId:={productId} ";
            var deviceInformationCollection = await wde.DeviceInformation.FindAllAsync(aqsFilter).AsTask();
            var deviceIds = deviceInformationCollection.Select(d => d.Id).ToList();
            return deviceIds;
        }
    }
}
