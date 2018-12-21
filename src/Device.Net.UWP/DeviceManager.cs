using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wde = Windows.Devices.Enumeration;

namespace Device.Net
{
    public static class DeviceManager
    {
        public async static Task<IEnumerable<string>> GetDeviceIds(int? vendorId, int? productId)
        {
            return ((IEnumerable<wde.DeviceInformation>)await wde.DeviceInformation.FindAllAsync($"System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True AND System.DeviceInterface.Hid.VendorId:={vendorId} AND System.DeviceInterface.Hid.ProductId:={productId} ").AsTask()).ToList();
        }
    }
}
