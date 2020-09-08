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
        private const string InterfaceEnabledPart = "AND System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True";
        private const string VendorFilterName = "System.DeviceInterface.Hid.VendorId";
        private const string ProductFilterName = "System.DeviceInterface.Hid.ProductId";

        private static string GetVendorPart(uint? vendorId)
        {
            string vendorPart = null;
            if (vendorId.HasValue) vendorPart = $"AND {VendorFilterName}:={vendorId.Value}";
            return vendorPart;
        }

        private static string GetProductPart(uint? productId)
        {
            string productPart = null;
            if (productId.HasValue) productPart = $"AND {ProductFilterName}:={productId.Value}";
            return productPart;
        }

        public static IDeviceFactory CreateUwpHidDeviceFactory(
#pragma warning disable IDE0060 // Remove unused parameter
            this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
#pragma warning restore IDE0060 // Remove unused parameter
            ILoggerFactory loggerFactory,
            GetDevice getDevice = null)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            if (getDevice == null) getDevice = (c) => new UWPHidDevice(c.DeviceId, loggerFactory);

            var firstDevice = filterDeviceDefinitions.First();

            var aqs = $"{InterfaceEnabledPart} {GetVendorPart(firstDevice.VendorId)} {GetProductPart(firstDevice.ProductId)}";

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
