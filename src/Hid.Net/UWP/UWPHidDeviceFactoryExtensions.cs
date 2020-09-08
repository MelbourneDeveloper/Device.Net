using Device.Net;
using Device.Net.UWP;
using Microsoft.Extensions.Logging;
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

        /// <summary>
        /// TODO: This is wrong. It will only search for one device
        /// </summary>
        public static IDeviceFactory CreateUwpHidDeviceFactory(
#pragma warning disable IDE0060 // Remove unused parameter
            this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
#pragma warning restore IDE0060 // Remove unused parameter
            ILoggerFactory loggerFactory,
            GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
            GetDevice getDevice = null)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            if (getDevice == null) getDevice = async (c) => new UWPHidDevice(c.DeviceId, loggerFactory);

            var firstDevice = filterDeviceDefinitions.First();

            //TODO: WRONG!!!
            var aqs = $"{AqsHelpers.InterfaceEnabledPart} {AqsHelpers.GetVendorPart(firstDevice.VendorId, DeviceType.Hid)} {AqsHelpers.GetProductPart(firstDevice.ProductId, DeviceType.Hid)}";

            var logger = loggerFactory.CreateLogger<UwpDeviceEnumerator>();

            if (getConnectedDeviceDefinitionsAsync == null)
            {
                var uwpHidDeviceEnumerator = new UwpDeviceEnumerator(
                    loggerFactory,
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
                    });

                getConnectedDeviceDefinitionsAsync = uwpHidDeviceEnumerator.GetConnectedDeviceDefinitionsAsync;
            }

            return new DeviceFactory(
                loggerFactory,
                getConnectedDeviceDefinitionsAsync,
                getDevice);
        }
    }
}
