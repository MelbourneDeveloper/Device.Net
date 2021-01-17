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
    /// Instantiates UWP Hid Factories. Use these methods as extension methods with <see cref="FilterDeviceDefinition"/> or directly to get all devices
    /// </summary>
    public static class UwpHidDeviceFactoryExtensions
    {

        #region Public Methods

        /// <summary>
        /// Creates a <see cref="IDeviceFactory"/> for UWP Hid devices
        /// </summary>
        /// <param name="filterDeviceDefinition">Devices must match this</param>
        /// <param name="loggerFactory"><see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.iloggerfactory"/></param>
        /// <param name="classGuid">Filters by specified class guid</param>
        /// <param name="deviceInformationFilter"></param>
        /// <param name="dataReceiver"></param>
        /// <param name="readBufferSize">Override the input report size</param>
        /// <param name="readTransferTransform">Exposes the raw data from the device (including Report Id) on reads and allows you to format the returned <see cref="TransferResult"/></param>
        /// <param name="writeTransferTransform">Given the Report Id and data supplied for the write, allow you to format the raw data that is sent to the device</param>
        /// <param name="writeBufferSize">Override the output report size</param>
        /// <param name="getConnectedDeviceDefinitionsAsync">Override the default call for getting definitions</param>
        /// <param name="getDevice"></param>
        /// <param name="defaultWriteReportId">The default Hid Report Id when WriteAsync is called instead of WriteReportAsync. If you specify null, the Report Id will come from the byte at index 0 of the array.</param>
        /// <returns>A factory which enumerates and instantiates devices</returns>
        public static IDeviceFactory CreateUwpHidDeviceFactory(
        this FilterDeviceDefinition filterDeviceDefinition,
        ILoggerFactory loggerFactory = null,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        GetDeviceAsync getDevice = null,
        byte? defaultWriteReportId = 0,
        Guid? classGuid = null,
        Func<wde.DeviceInformation, bool> deviceInformationFilter = null,
        IDataReceiver dataReceiver = null,
        ushort? writeBufferSize = null,
        ushort? readBufferSize = null,
        Func<TransferResult, ReadReport> readTransferTransform = null,
        Func<byte[], byte, byte[]> writeTransferTransform = null) => CreateUwpHidDeviceFactory(
            new List<FilterDeviceDefinition> { filterDeviceDefinition },
            loggerFactory,
            getConnectedDeviceDefinitionsAsync,
            getDevice,
            defaultWriteReportId,
            classGuid,
            deviceInformationFilter,
            dataReceiver,
            writeBufferSize,
            readBufferSize,
            readTransferTransform,
            writeTransferTransform);


        /// <summary>
        /// Creates a <see cref="IDeviceFactory"/> for UWP Hid devices
        /// </summary>
        /// <param name="filterDeviceDefinitions">Devices must match these</param>
        /// <param name="loggerFactory"><see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.iloggerfactory"/></param>
        /// <param name="classGuid">Filters by specified class guid</param>
        /// <param name="deviceInformationFilter"></param>
        /// <param name="dataReceiver"></param>
        /// <param name="readBufferSize">Override the input report size</param>
        /// <param name="readTransferTransform">Exposes the raw data from the device (including Report Id) on reads and allows you to format the returned <see cref="TransferResult"/></param>
        /// <param name="writeTransferTransform">Given the Report Id and data supplied for the write, allow you to format the raw data that is sent to the device</param>
        /// <param name="writeBufferSize">Override the output report size</param>
        /// <param name="getConnectedDeviceDefinitionsAsync">Override the default call for getting definitions</param>
        /// <param name="getDevice"></param>
        /// <param name="defaultWriteReportId">The default Hid Report Id when WriteAsync is called instead of WriteReportAsync. If you specify null, the Report Id will come from the byte at index 0 of the array.</param>
        /// <returns>A factory which enumerates and instantiates devices</returns>
        public static IDeviceFactory CreateUwpHidDeviceFactory(
        this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
        ILoggerFactory loggerFactory = null,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        GetDeviceAsync getDevice = null,
        byte? defaultWriteReportId = 0,
        Guid? classGuid = null,
        Func<wde.DeviceInformation, bool> deviceInformationFilter = null,
        IDataReceiver dataReceiver = null,
        ushort? writeBufferSize = null,
        ushort? readBufferSize = null,
        Func<TransferResult, ReadReport> readTransferTransform = null,
        Func<byte[], byte, byte[]> writeTransferTransform = null)
        {
            loggerFactory ??= NullLoggerFactory.Instance;

            getDevice ??= (c, cancellationToken) => Task.FromResult<IDevice>(
                new HidDevice
                (
                    new UwpHidDeviceHandler(
                    c,
                    dataReceiver ??
                    new UwpDataReceiver(
                        new Observable<TransferResult>(),
                        loggerFactory.CreateLogger<UwpDataReceiver>()),
                    loggerFactory,
                    writeBufferSize,
                    readBufferSize,
                    readTransferTransform,
                    writeTransferTransform), loggerFactory, defaultWriteReportId));

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
                        using var hidDevice = await UwpHidDeviceHandler.GetHidDevice(deviceId).AsTask(cancellationToken);

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

        #endregion Public Methods
    }
}
