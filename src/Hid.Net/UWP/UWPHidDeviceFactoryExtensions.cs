using Device.Net;
using Device.Net.UWP;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Hid.Net.UWP
{
    /// <summary>
    /// TODO: Merge this factory class with other factory classes. I.e. create a DeviceFactoryBase class
    /// </summary>
    public static class UWPHidDeviceFactoryExtensions
    {
        public static IDeviceFactory CreateUwpHidDeviceFactory(
#pragma warning disable IDE0060 // Remove unused parameter
            this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
#pragma warning restore IDE0060 // Remove unused parameter
            ILoggerFactory loggerFactory,
            GetDevice getDevice)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            //var aqs = $"{InterfaceEnabledPart} {GetVendorPart(vendorId)} {GetProductPart(productId)}";
            var aqs = "";
            throw new NotImplementedException("Need to build the aqs string");

            var logger = loggerFactory.CreateLogger<UwpHidDeviceEnumerator>();

            var uwpHidDeviceEnumerator = new UwpHidDeviceEnumerator(
                loggerFactory,
                logger,
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

            return new DeviceFactory(
                loggerFactory,
                uwpHidDeviceEnumerator.GetConnectedDeviceDefinitionsAsync,
                getDevice);
        }
    }
}
