using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wde = Windows.Devices.Enumeration;

namespace Usb.Net.UWP
{
    public static class UWPHelpers
    {
        public static async Task<List<wde.DeviceInformation>> GetDevicesByProductAndVendorAsync(int vendorId, int productId)
        {
            return ((IEnumerable<wde.DeviceInformation>)await wde.DeviceInformation.FindAllAsync().AsTask()).ToList();
        }

        public static async Task<wde.DeviceInformationCollection> GetAllDevicesAsync()
        {
            return await wde.DeviceInformation.FindAllAsync().AsTask();
        }
    }
}
