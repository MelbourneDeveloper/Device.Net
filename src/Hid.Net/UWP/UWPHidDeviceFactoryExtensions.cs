using Device.Net;
using Device.Net.UWP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hid.Net.UWP
{
    /// <summary>
    /// TODO: Merge this factory class with other factory classes. I.e. create a DeviceFactoryBase class
    /// </summary>
    public static class UWPHidDeviceFactoryExtensions
    {

        public static IDeviceFactory CreateUwpHidDeviceFactory(
        this FilterDeviceDefinition filterDeviceDefinitions,
        ILoggerFactory loggerFactory,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        GetDevice getDevice = null,
        byte? defaultReportId = null) => CreateUwpHidDeviceFactory(new List<FilterDeviceDefinition> { filterDeviceDefinitions }, loggerFactory, getConnectedDeviceDefinitionsAsync, getDevice, defaultReportId);

        /// <summary>
        /// TODO: This is wrong. It will only search for one device
        /// </summary>
        public static IDeviceFactory CreateUwpHidDeviceFactory(
        this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
        ILoggerFactory loggerFactory = null,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        GetDevice getDevice = null,
        byte? defaultReportId = null)
        {
            loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;

            if (getDevice == null) getDevice = async (c) => new UWPHidDevice(c, loggerFactory, defaultReportId);

            var firstDevice = filterDeviceDefinitions.First();

            //TODO: WRONG!!!
            var aqs = $"{AqsHelpers.InterfaceEnabledPart} {AqsHelpers.GetVendorPart(firstDevice.VendorId, DeviceType.Hid)} {AqsHelpers.GetProductPart(firstDevice.ProductId, DeviceType.Hid)}";

            var logger = loggerFactory.CreateLogger<UwpDeviceEnumerator>();

            if (getConnectedDeviceDefinitionsAsync == null)
            {
                var uwpHidDeviceEnumerator = new UwpDeviceEnumerator(
                    aqs,
                    DeviceType.Hid,
                    async (deviceId) =>
                    {
                        using (var hidDevice = await UWPHidDevice.GetHidDevice(deviceId).AsTask())
                        {
                            var canConnect = hidDevice != null;

                            if (!canConnect) return new ConnectionInfo { CanConnect = false };

                            logger?.LogInformation("Testing device connection. Id: {deviceId}. Can connect: {canConnect}", deviceId, canConnect);

                            return new ConnectionInfo { CanConnect = canConnect, UsagePage = hidDevice.UsagePage };
                        }
                    },
                    loggerFactory);

                getConnectedDeviceDefinitionsAsync = uwpHidDeviceEnumerator.GetConnectedDeviceDefinitionsAsync;
            }

            return new DeviceFactory(
                loggerFactory,
                getConnectedDeviceDefinitionsAsync,
                getDevice,
                DeviceType.Hid);
        }
    }
}
