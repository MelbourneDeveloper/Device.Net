using Device.Net;
using Device.Net.UWP;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wde = Windows.Devices.Enumeration;

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
        byte defaultReportId = 0) => CreateUwpHidDeviceFactory(
            new List<FilterDeviceDefinition> { filterDeviceDefinitions },
            loggerFactory,
            getConnectedDeviceDefinitionsAsync,
            getDevice,
            defaultReportId);


        //TODO: This is wrong. It will only search for one device

        public static IDeviceFactory CreateUwpHidDeviceFactory(
        this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
        ILoggerFactory loggerFactory = null,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        GetDeviceAsync getDevice = null,
        byte defaultReportId = 0,
        Guid? classGuid = null,
        Func<wde.DeviceInformation, bool> deviceInformationFilter = null,
        IDataReceiver dataReceiver = null,
        ushort? writeBufferSize = null,
        ushort? readBufferSize = null)
        {
            loggerFactory ??= NullLoggerFactory.Instance;

            getDevice ??= (c, cancellationToken) => Task.FromResult<IDevice>(
                new UWPHidDevice
                (
                    c,
                    dataReceiver ??
                    new UWPDataReceiver(
                        new Observable<TransferResult>(),
                        loggerFactory.CreateLogger<UWPDataReceiver>()),
                    loggerFactory,
                    writeBufferSize,
                    readBufferSize,
                    defaultReportId));

            var aqs = AqsHelpers.GetAqs(filterDeviceDefinitions, DeviceType.Hid);

            var logger = loggerFactory.CreateLogger<UwpDeviceEnumerator>();

            if (getConnectedDeviceDefinitionsAsync == null)
            {
                //Filter to by device Id. 
                //TODO: There is surely a better way to do this
                deviceInformationFilter ??= d =>
                    d.Id.Contains(@"\\?\hid", StringComparison.OrdinalIgnoreCase) &&
                    d.Id.Contains(@"vid", StringComparison.OrdinalIgnoreCase) &&
                    d.Id.Contains(@"pid", StringComparison.OrdinalIgnoreCase);

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
                    loggerFactory,
                    deviceInformationFilter);

                getConnectedDeviceDefinitionsAsync = uwpHidDeviceEnumerator.GetConnectedDeviceDefinitionsAsync;
            }

            return new DeviceFactory(
                loggerFactory,
                getConnectedDeviceDefinitionsAsync,
                getDevice,
                (c, cancellationToken) => Task.FromResult(c.DeviceType == DeviceType.Hid && (classGuid == null || classGuid.Value == c.ClassGuid))
                );
        }
    }
}
