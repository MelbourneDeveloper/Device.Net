using Device.Net;
using Device.Net.UWP;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using wde = Windows.Devices.Enumeration;
using windowsUsbDevice = Windows.Devices.Usb.UsbDevice;

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
            ) =>
            CreateUwpUsbDeviceFactory(
                new List<FilterDeviceDefinition> { filterDeviceDefinitions },
                loggerFactory,
                getConnectedDeviceDefinitionsAsync,
                getUsbInterfaceManager,
                readBufferSize,
                writeBufferSize);

        public static IDeviceFactory CreateUwpUsbDeviceFactory(
        this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
        ILoggerFactory loggerFactory,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        GetUsbInterfaceManager getUsbInterfaceManager = null,
        ushort? readBufferSize = null,
        ushort? writeBufferSize = null,
        Func<wde.DeviceInformation, bool> deviceInformationFilter = null,
        Func<windowsUsbDevice, SetupPacket, byte[], CancellationToken, Task<TransferResult>> performControlTransferAsync = null,
        IDataReceiver dataReceiver = null)
        {
            if (getConnectedDeviceDefinitionsAsync == null)
            {
                //Filter to by device Id. 
                //TODO: There is surely a better way to do this
                deviceInformationFilter ??= d =>
                    d.Id.Contains(@"\\?\usb", StringComparison.OrdinalIgnoreCase) &&
                    d.Id.Contains(@"vid", StringComparison.OrdinalIgnoreCase) &&
                    d.Id.Contains(@"pid", StringComparison.OrdinalIgnoreCase);

                var uwpHidDeviceEnumerator = new UwpDeviceEnumerator(
                    AqsHelpers.GetAqs(filterDeviceDefinitions, DeviceType.Usb),
                    DeviceType.Usb,
                    (d, cancellationToken) => Task.FromResult(new ConnectionInfo { CanConnect = true }),
                    loggerFactory,
                    deviceInformationFilter);

                getConnectedDeviceDefinitionsAsync = uwpHidDeviceEnumerator.GetConnectedDeviceDefinitionsAsync;
            }

            getUsbInterfaceManager ??= (deviceId, cancellationToken) =>
                Task.FromResult<IUsbInterfaceManager>(
                    new UWPUsbInterfaceManager(
                        //TODO: no idea if this is OK...
                        new ConnectedDeviceDefinition(deviceId, DeviceType.Usb),
                        performControlTransferAsync,
                        dataReceiver ?? new UWPDataReceiver(
                            new Observable<TransferResult>(),
                            loggerFactory.CreateLogger<UWPDataReceiver>()),
                        loggerFactory,
                        readBufferSize,
                        writeBufferSize));

            return UsbDeviceFactoryExtensions.CreateUsbDeviceFactory(getConnectedDeviceDefinitionsAsync, getUsbInterfaceManager, loggerFactory);
        }
    }
}
