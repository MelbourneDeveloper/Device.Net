using LibUsbDotNet;
using LibUsbDotNet.Main;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net.LibUsb
{
    public static class LibUsbFactoryExtensions
    {
        public static IDeviceFactory CreateLibUsbDeviceFactory(
            this FilterDeviceDefinition filterDeviceDefinition,
            ILoggerFactory loggerFactory = null,
            int? timeout = null)
        {
            return new DeviceFactory(
                loggerFactory,
                () => GetConnectedDeviceDefinitionsAsync(filterDeviceDefinition),
                async c => new LibUsbDevice(GetDevice(c), timeout ?? 1000),
                DeviceType.Usb);
        }

        public static async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(FilterDeviceDefinition deviceDefinition)
        {
            return await Task.Run<IEnumerable<ConnectedDeviceDefinition>>(() =>
           {
               IEnumerable<UsbRegistry> devices = UsbDevice.AllDevices;

               if (deviceDefinition == null)
               {
                   return devices.Select(usbRegistry => new ConnectedDeviceDefinition(usbRegistry.DevicePath)
                   {
                       VendorId = (uint)usbRegistry.Vid,
                       ProductId = (uint)usbRegistry.Pid,
                       DeviceType = DeviceType.Usb
                   }).ToList();
               }

               if (deviceDefinition.VendorId.HasValue)
               {
                   devices = devices.Where(d => d.Vid == deviceDefinition.VendorId.Value);
               }

               if (deviceDefinition.ProductId.HasValue)
               {
                   devices = devices.Where(d => d.Pid == deviceDefinition.ProductId.Value);
               }

               return devices.Select(usbRegistry => new ConnectedDeviceDefinition(usbRegistry.DevicePath) { VendorId = (uint)usbRegistry.Vid, ProductId = (uint)usbRegistry.Pid, DeviceType = DeviceType.Usb }).ToList();
           });
        }

        public static UsbDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition == null) throw new ArgumentNullException(nameof(deviceDefinition));
#pragma warning disable CA2208 
            if (deviceDefinition.VendorId == null) throw new ArgumentNullException(nameof(ConnectedDeviceDefinition.VendorId));
            if (deviceDefinition.ProductId == null) throw new ArgumentNullException(nameof(ConnectedDeviceDefinition.ProductId));
#pragma warning restore CA2208 

            var usbDeviceFinder = new UsbDeviceFinder((int)deviceDefinition.VendorId.Value, (int)deviceDefinition.ProductId.Value);
            return UsbDevice.OpenUsbDevice(usbDeviceFinder);
        }
    }
}
