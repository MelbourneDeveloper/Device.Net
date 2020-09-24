using Device.Net;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Usb.Net.Windows
{

    public static class WindowsUsbDeviceFactoryExtensions
    {
        public static IDeviceFactory CreateWindowsUsbDeviceFactory(
            this FilterDeviceDefinition filterDeviceDefinition,
            ILoggerFactory loggerFactory,
            GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
            GetUsbInterfaceManager getUsbInterfaceManager = null,
            Guid? classGuid = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null
        ) => CreateWindowsUsbDeviceFactory(
            new List<FilterDeviceDefinition> { filterDeviceDefinition },
            loggerFactory,
            getConnectedDeviceDefinitionsAsync,
            getUsbInterfaceManager,
            classGuid,
            readBufferSize,
            writeBufferSize);

        public static IDeviceFactory CreateWindowsUsbDeviceFactory(
        this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
        ILoggerFactory loggerFactory,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        GetUsbInterfaceManager getUsbInterfaceManager = null,
        Guid? classGuid = null,
        ushort? readBufferSize = null,
        ushort? writeBufferSize = null
    )
        {
            loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;

            if (getConnectedDeviceDefinitionsAsync == null)
            {
                var logger = loggerFactory.CreateLogger<WindowsDeviceEnumerator>();

                var uwpHidDeviceEnumerator = new WindowsDeviceEnumerator(
                    logger,
                    classGuid ?? WindowsDeviceConstants.WinUSBGuid,
                    (d) => DeviceBase.GetDeviceDefinitionFromWindowsDeviceId(d, DeviceType.Usb, logger),
                    async (c) =>
                    filterDeviceDefinitions.FirstOrDefault((f) => DeviceManager.IsDefinitionMatch(f, c, DeviceType.Usb)) != null);

                getConnectedDeviceDefinitionsAsync = uwpHidDeviceEnumerator.GetConnectedDeviceDefinitionsAsync;
            }

            if (getUsbInterfaceManager == null)
            {
                getUsbInterfaceManager = async (d) =>
                    new WindowsUsbInterfaceManager(
                    //TODO: no idea if this is OK...
                    d,
                    loggerFactory,
                    readBufferSize,
                    writeBufferSize);
            }

            return UsbDeviceFactoryExtensions.CreateUsbDeviceFactory(getConnectedDeviceDefinitionsAsync, getUsbInterfaceManager, loggerFactory);
        }
    }
}