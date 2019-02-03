using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net.LibUsb
{
    public abstract class LibUsbDeviceFactoryBase : IDeviceFactory
    {
        public abstract DeviceType DeviceType { get; }

        public async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(FilterDeviceDefinition deviceDefinition)
        {
            return await Task.Run(() =>
            {
                UsbDeviceFinder usbDeviceFinder = null;

                if (deviceDefinition.VendorId.HasValue && !deviceDefinition.ProductId.HasValue)
                {
                    usbDeviceFinder = new UsbDeviceFinder((int)deviceDefinition.VendorId.Value);
                }

                var usbDevice = UsbDevice.OpenUsbDevice(usbDeviceFinder);

                var retVal = new List<ConnectedDeviceDefinition>();

                if (usbDevice != null)
                {
                    retVal.Add(new ConnectedDeviceDefinition(usbDevice.UsbRegistryInfo.DeviceInterfaceGuids[0].ToString()));
                    usbDevice.Close();
                    return retVal;
                }

                return retVal;
            });
        }

        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            throw new NotImplementedException();
        }
    }
}
