using Device.Net;
using Device.Net.UWP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hid.Net.UWP
{
    /// <summary>
    /// TODO: Merge this factory class with other factory classes. I.e. create a DeviceFactoryBase class
    /// </summary>
    public static class UWPHidDeviceFactoryExtensions
    {

        public static IDeviceFactory CreateUwpHidDeviceFactory(
        this FilterDeviceDefinition filterDeviceDefinitions,
        ILoggerFactory loggerFactory = null,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        GetDeviceAsync getDevice = null,
        byte? defaultReportId = null) => CreateUwpHidDeviceFactory(new List<FilterDeviceDefinition> { filterDeviceDefinitions }, loggerFactory, getConnectedDeviceDefinitionsAsync, getDevice, defaultReportId);

        /// <summary>
        /// TODO: This is wrong. It will only search for one device
        /// </summary>
        public static IDeviceFactory CreateUwpHidDeviceFactory(
        this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
        ILoggerFactory loggerFactory = null,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        GetDeviceAsync getDevice = null,
        byte? defaultReportId = null,
        Guid? classGuid = null)
        {
            loggerFactory ??= NullLoggerFactory.Instance;

            getDevice ??= (c, cancellationToken) => Task.FromResult<IDevice>(new UWPHidDevice(c, loggerFactory, defaultReportId));

            var deviceFilters = filterDeviceDefinitions.Select(firstDevice => $"({ AqsHelpers.GetVendorPart(firstDevice.VendorId, DeviceType.Hid) } AND { AqsHelpers.GetProductPart(firstDevice.ProductId, DeviceType.Hid)})");

            var deviceListFilter = string.Join(", OR ", deviceFilters);

            var aqs = $"{AqsHelpers.InterfaceEnabledPart} AND {deviceListFilter}";

            var logger = loggerFactory.CreateLogger<UwpDeviceEnumerator>();

            if (getConnectedDeviceDefinitionsAsync == null)
            {
                var uwpHidDeviceEnumerator = new UwpDeviceEnumerator(
                    aqs,
                    DeviceType.Hid,
                    async (deviceId, cancellationToken) =>
                    {
                        using var hidDevice = await UWPHidDevice.GetHidDevice(deviceId).AsTask(cancellationToken);

                        var canConnect = hidDevice != null;

                        if (!canConnect) return new ConnectionInfo { CanConnect = false };

                        logger?.LogInformation("Testing device connection. Id: {deviceId}. Can connect: {canConnect}", deviceId, true);

                        return new ConnectionInfo { CanConnect = true, UsagePage = hidDevice.UsagePage };
                    },
                    loggerFactory);

                getConnectedDeviceDefinitionsAsync = uwpHidDeviceEnumerator.GetConnectedDeviceDefinitionsAsync;
            }

            return new DeviceFactory(
                loggerFactory,
                getConnectedDeviceDefinitionsAsync,
                getDevice,
                (c, cancellationToken) => Task.FromResult(c.DeviceType == DeviceType.Usb && (classGuid == null || classGuid.Value == c.ClassGuid))
                );
        }
    }
}
