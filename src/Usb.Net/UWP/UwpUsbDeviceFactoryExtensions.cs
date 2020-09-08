using Device.Net;
using Device.Net.UWP;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Usb.Net.UWP
{
    public static class UwpUsbDeviceFactoryExtensions
    {
        public static IDeviceFactory UwpUsbDeviceFactory(
            this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
            ILoggerFactory loggerFactory,
            GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
            GetUsbInterfaceManager getUsbInterfaceManager = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null
            )
        {
            if (getConnectedDeviceDefinitionsAsync == null)
            {
                var uwpHidDeviceEnumerator = new UwpDeviceEnumerator(
                    loggerFactory,
                    aqs,
                    DeviceType.Usb,
                    async (d) => new ConnectionInfo { CanConnect = true });

                getConnectedDeviceDefinitionsAsync = uwpHidDeviceEnumerator.GetConnectedDeviceDefinitionsAsync;
            }

            if (getUsbInterfaceManager == null)
            {
                getUsbInterfaceManager = (d) =>
                    new UWPUsbInterfaceManager(
                    deviceDefinition,
                    loggerFactory,
                    readBufferSize,
                    writeBufferSize);
            }

            return UsbDeviceFactoryExtensions.CreateUsbDeviceFactory(loggerFactory, getConnectedDeviceDefinitionsAsync, getUsbInterfaceManager);
        }
    }
}
