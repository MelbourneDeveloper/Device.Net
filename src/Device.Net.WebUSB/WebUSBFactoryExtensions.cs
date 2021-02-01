using Blazor.Extensions.WebUSB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Usb.Net;

namespace Device.Net.WebUSB
{
    public static class WebUSBFactoryExtensions
    {
        public static IDeviceFactory CreateWebUSBDeviceFactory(
        this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
        IUSB usb,
        ILoggerFactory loggerFactory = null,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        GetUsbInterfaceManager getUsbInterfaceManager = null
        )
        {
            if (filterDeviceDefinitions == null) throw new ArgumentNullException(nameof(filterDeviceDefinitions));

            loggerFactory ??= NullLoggerFactory.Instance;

            getConnectedDeviceDefinitionsAsync ??= (c) => GetConnectedDevicesAsync(usb);

            //getUsbInterfaceManager ??=

            return UsbDeviceFactoryExtensions.CreateUsbDeviceFactory(getConnectedDeviceDefinitionsAsync, getUsbInterfaceManager, loggerFactory);
        }

        private static async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDevicesAsync(IUSB usb)
        {
            var devices = await usb.GetDevices().ConfigureAwait(false);

            IEnumerable<ConnectedDeviceDefinition> connectedDeviceDefinitions = null;

            connectedDeviceDefinitions = devices.Select(d => new ConnectedDeviceDefinition(
                d.SerialNumber,
                DeviceType.Usb,
                (uint)d.VendorId,
                (uint)d.ProductId,
                d.ProductName,
                d.ManufacturerName,
                d.SerialNumber));

            return connectedDeviceDefinitions;
        }
    }
}
