using LibUsbDotNet;
using LibUsbDotNet.Main;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.LibUsb
{
    public static class LibUsbFactoryExtensions
    {
        public static IDeviceFactory CreateLibUsbDeviceFactory(
            this FilterDeviceDefinition filterDeviceDefinition,
            ILoggerFactory loggerFactory = null,
            int? timeout = null,
            ushort? writeBufferSize = null,
            ushort? readBufferSize = null,
            Func<ConnectedDeviceDefinition, CancellationToken, Task<bool>> supportsDevice = null
            )
             => CreateLibUsbDeviceFactory
                    (
                        new ReadOnlyCollection<FilterDeviceDefinition>(new List<FilterDeviceDefinition> { filterDeviceDefinition }),
                        loggerFactory,
                        timeout,
                        writeBufferSize,
                        readBufferSize,
                        supportsDevice
                 );

        public static IDeviceFactory CreateLibUsbDeviceFactory(
            this IReadOnlyList<FilterDeviceDefinition> filterDeviceDefinitions,
            ILoggerFactory loggerFactory = null,
            int? timeout = null,
            ushort? writeBufferSize = null,
            ushort? readBufferSize = null,
            Func<ConnectedDeviceDefinition, CancellationToken, Task<bool>> supportsDevice = null
            )
             => new DeviceFactory(
                loggerFactory,
                (cancellationToken) => GetConnectedDeviceDefinitionsAsync(filterDeviceDefinitions, cancellationToken),
                (c, cancellationToken) =>
                Task.FromResult<IDevice>(
                    new Usb.Net.UsbDevice
                    (
                        c.DeviceId,
                        new LibUsbInterfaceManager(
                            GetDevice(c),
                            timeout ?? 1000,
                            loggerFactory,
                            writeBufferSize,
                            readBufferSize), loggerFactory
                    )
                ),
                supportsDevice ??
                ((c, cancellationToken) => Task.FromResult(c.DeviceType == DeviceType.Usb))
            );

        public static async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(
            IReadOnlyList<FilterDeviceDefinition> filterDeviceDefinitions,
            CancellationToken cancellationToken = default)
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
           }, cancellationToken);
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

