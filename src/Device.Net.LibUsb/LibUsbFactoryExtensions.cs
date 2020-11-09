using LibUsbDotNet;
using LibUsbDotNet.Main;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net.LibUsb
{
    public static class LibUsbFactoryExtensions
    {
        public static IDeviceFactory CreateLibUsbDeviceFactory(
            this IReadOnlyList<FilterDeviceDefinition> filterDeviceDefinitions,
            ILoggerFactory loggerFactory = null,
            int? timeout = null)
        {
            return new DeviceFactory(
                loggerFactory,
                () => GetConnectedDeviceDefinitionsAsync(filterDeviceDefinitions),
                async c => new LibUsbDevice(GetDevice(c), timeout ?? 1000),
                new ReadOnlyCollection<DeviceType>(new List<DeviceType> { DeviceType.Usb }));
        }

        public static async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(IReadOnlyList<FilterDeviceDefinition> filterDeviceDefinitions)
        {
            return await Task.Run<IEnumerable<ConnectedDeviceDefinition>>(
                () =>
           {
               IEnumerable<UsbRegistry> devices = UsbDevice.AllDevices;

               return filterDeviceDefinitions == null || filterDeviceDefinitions.Count == 0
                   ? devices.Select(usbRegistry
                   =>
                   new ConnectedDeviceDefinition(usbRegistry.DevicePath, DeviceType.Usb, vendorId: (uint)usbRegistry.Vid, productId: (uint)usbRegistry.Pid)
                   ).ToList()
                   : devices
               .Where(d => filterDeviceDefinitions.FirstOrDefault(f
                   =>
                   (f.VendorId == null || f.VendorId == d.Vid) &&
                   (f.ProductId == null || f.ProductId == d.Pid)
                   )
               != null)
               .Select(usbRegistry => new ConnectedDeviceDefinition
               (
                   usbRegistry.DevicePath,
                   vendorId: (uint)usbRegistry.Vid,
                   productId: (uint)usbRegistry.Pid,
                   deviceType: DeviceType.Usb
               )).ToList();
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

