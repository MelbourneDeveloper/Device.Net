using LibUsbDotNet;
using LibUsbDotNet.Main;
using System.Collections.Generic;
using System.Linq;
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
                IEnumerable<UsbRegistry> devices = null;

                if (deviceDefinition.VendorId.HasValue)
                {
                    if (deviceDefinition.ProductId.HasValue)
                    {
                        devices = UsbDevice.AllDevices.Where(d => d.Vid == deviceDefinition.VendorId.Value && d.Pid == deviceDefinition.ProductId.Value);
                    }
                    else
                    {
                        devices = UsbDevice.AllDevices.Where(d => d.Vid == deviceDefinition.VendorId.Value);
                    }
                }

                var retVal = new List<ConnectedDeviceDefinition>();

                foreach (var usbRegistry in devices)
                {
                    const string classPropertyName = "Class";

                    var usbDeviceClass = DeviceType == DeviceType.Usb ? "USBDevice" : null;

                    if (!usbRegistry.DeviceProperties.ContainsKey(classPropertyName) || (string)usbRegistry.DeviceProperties[classPropertyName] != usbDeviceClass)
                    {
                        continue;
                    }

                    retVal.Add(new ConnectedDeviceDefinition(usbRegistry.DevicePath)
                    {
                        VendorId = (uint)usbRegistry.Vid,
                        ProductId = (uint)usbRegistry.Pid,
                        DeviceType = DeviceType
                    });
                }


                return retVal;
            });
        }

        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            var usbDeviceFinder = new UsbDeviceFinder((int)deviceDefinition.VendorId.Value, (int)deviceDefinition.ProductId.Value);
            var usbDevice = UsbDevice.OpenUsbDevice(usbDeviceFinder);
            return usbDevice != null ? new LibUsbDevice(usbDevice, 3000) : null;
        }
    }
}
