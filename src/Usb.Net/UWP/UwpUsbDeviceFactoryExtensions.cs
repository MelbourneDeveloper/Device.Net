using Device.Net;
using Device.Net.UWP;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Usb.Net.UWP
{
    public static class UwpUsbDeviceFactoryExtensions
    {

        public static IDeviceFactory CreateUwpUsbDeviceFactory(
            this FilterDeviceDefinition filterDeviceDefinitions,
            ILoggerFactory loggerFactory = null,
            GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
            GetUsbInterfaceManager getUsbInterfaceManager = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null
            )
        {
            return CreateUwpUsbDeviceFactory(
                new List<FilterDeviceDefinition> { filterDeviceDefinitions },
                loggerFactory,
                getConnectedDeviceDefinitionsAsync,
                getUsbInterfaceManager,
                readBufferSize,
                writeBufferSize);
        }

        public static IDeviceFactory CreateUwpUsbDeviceFactory(
        this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
        ILoggerFactory loggerFactory,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        GetUsbInterfaceManager getUsbInterfaceManager = null,
        ushort? readBufferSize = null,
        ushort? writeBufferSize = null
        )
        {
            var firstDevice = filterDeviceDefinitions.First();

            //TODO: WRONG!!!

            var interfaceClassGuid = "System.Devices.InterfaceClassGuid:=\"{" + WindowsDeviceConstants.WinUSBGuid + "}\"";
            var aqs = $"{interfaceClassGuid} {AqsHelpers.InterfaceEnabledPart} {AqsHelpers.GetVendorPart(firstDevice.VendorId, DeviceType.Usb)} {AqsHelpers.GetProductPart(firstDevice.ProductId, DeviceType.Usb)}";

            if (getConnectedDeviceDefinitionsAsync == null)
            {
                var uwpHidDeviceEnumerator = new UwpDeviceEnumerator(
                    aqs,
                    DeviceType.Usb,
                    async d => new ConnectionInfo { CanConnect = true },
                    loggerFactory);

                getConnectedDeviceDefinitionsAsync = uwpHidDeviceEnumerator.GetConnectedDeviceDefinitionsAsync;
            }

            if (getUsbInterfaceManager == null)
            {
                getUsbInterfaceManager = async d =>
                    new UWPUsbInterfaceManager(
                    //TODO: no idea if this is OK...
                    new ConnectedDeviceDefinition(d),
                    loggerFactory,
                    readBufferSize,
                    writeBufferSize);
            }

            return UsbDeviceFactoryExtensions.CreateUsbDeviceFactory(getConnectedDeviceDefinitionsAsync, getUsbInterfaceManager, loggerFactory);
        }
    }
}
