using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wde = Windows.Devices.Enumeration;

namespace Hid.Net.UWP
{
    public static class UWPHelpers
    {
        public static async Task<List<wde.DeviceInformation>> GetDevicesByProductAndVendorAsync(int vendorId, int productId)
        {
            return ((IEnumerable<wde.DeviceInformation>)await wde.DeviceInformation.FindAllAsync($"System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True AND System.DeviceInterface.Hid.VendorId:={vendorId} AND System.DeviceInterface.Hid.ProductId:={productId} ").AsTask()).ToList();
        }

        public static async Task<wde.DeviceInformationCollection> GetAllDevicesAsync()
        {
            return await wde.DeviceInformation.FindAllAsync().AsTask();
        }
    }
}
