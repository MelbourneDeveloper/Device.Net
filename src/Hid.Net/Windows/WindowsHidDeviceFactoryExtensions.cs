using Device.Net;
using Device.Net.Exceptions;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Hid.Net.Windows
{
    public static class WindowsHidDeviceFactoryExtensions
    {
        #region Public Methods

        /// <summary>
        /// Creates a <see cref="IDeviceFactory"/> for Windows Hid devices
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="hidApiService"></param>
        /// <param name="classGuid">Filters by specified class guid</param>
        /// <param name="readBufferSize"></param>
        /// <param name="writeBufferSize"></param>
        /// <param name="getConnectedDeviceDefinitionsAsync">Override the default call for getting definitions</param>
        /// <param name="defaultWriteReportId">The default Hid Report Id when WriteAsync is called instead of WriteReportAsync</param>
        /// <param name="readTransferTransform">Exposes the raw data from the device (including Report Id) on reads and allows you to format the returned <see cref="TransferResult"/></param>
        /// <param name="writeTransferTransform">Given the Report Id and data supplied for the write, allow you to format the raw data that is sent to the device</param>
        /// <returns></returns>
        public static IDeviceFactory CreateWindowsHidDeviceFactory(
        ILoggerFactory loggerFactory = null,
        IHidApiService hidApiService = null,
        Guid? classGuid = null,
        ushort? readBufferSize = null,
        ushort? writeBufferSize = null,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        byte defaultWriteReportId = 0,
        Func<TransferResult, ReadReport> readTransferTransform = null,
        Func<byte[], byte, byte[]> writeTransferTransform = null)
        {
            return CreateWindowsHidDeviceFactory(
                new ReadOnlyCollection<FilterDeviceDefinition>(new List<FilterDeviceDefinition>()),
                loggerFactory,
                hidApiService,
                classGuid,
                readBufferSize,
                writeBufferSize,
                getConnectedDeviceDefinitionsAsync,
                defaultWriteReportId,
                readTransferTransform,
                writeTransferTransform
                );
        }

        /// <summary>
        /// Creates a factory Hid devices
        /// </summary>
        /// <param name="filterDeviceDefinition"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="hidApiService"></param>
        /// <param name="classGuid"></param>
        /// <param name="readBufferSize"></param>
        /// <param name="writeBufferSize"></param>
        /// <param name="getConnectedDeviceDefinitionsAsync"></param>
        /// <param name="defaultWriteReportId">The default Hid Report Id when WriteAsync is called instead of WriteReportAsync</param>
        /// <param name="readTransferTransform">Exposes the raw data from the device (including Report Id) on reads and allows you to format the returned <see cref="TransferResult"/></param>
        /// <param name="writeTransferTransform">Given the Report Id and data supplied for the write, allow you to format the raw data that is sent to the device</param>
        /// <returns></returns>
        public static IDeviceFactory CreateWindowsHidDeviceFactory(
        this FilterDeviceDefinition filterDeviceDefinition,
        ILoggerFactory loggerFactory = null,
        IHidApiService hidApiService = null,
        Guid? classGuid = null,
        ushort? readBufferSize = null,
        ushort? writeBufferSize = null,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        byte defaultWriteReportId = 0,
        Func<TransferResult, ReadReport> readTransferTransform = null,
        Func<byte[], byte, byte[]> writeTransferTransform = null)
        {
            return CreateWindowsHidDeviceFactory(
                new ReadOnlyCollection<FilterDeviceDefinition>(new List<FilterDeviceDefinition> { filterDeviceDefinition }),
                loggerFactory,
                hidApiService,
                classGuid,
                readBufferSize,
                writeBufferSize,
                getConnectedDeviceDefinitionsAsync,
                defaultWriteReportId,
                readTransferTransform,
                writeTransferTransform
                );
        }

        /// <summary>
        /// Creates a factory Hid devices
        /// </summary>
        /// <param name="filterDeviceDefinitions"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="hidApiService"></param>
        /// <param name="classGuid"></param>
        /// <param name="readBufferSize"></param>
        /// <param name="writeBufferSize"></param>
        /// <param name="getConnectedDeviceDefinitionsAsync">Specify custom code for getting the device definitions</param>
        /// <param name="defaultWriteReportId">The default Hid Report Id when WriteAsync is called instead of WriteReportAsync</param>
        /// <param name="readTransferTransform">Exposes the raw data from the device (including Report Id) on reads and allows you to format the returned <see cref="TransferResult"/></param>
        /// <param name="writeTransferTransform">Given the Report Id and data supplied for the write, allow you to format the raw data that is sent to the device</param>
        /// <returns></returns>
        public static IDeviceFactory CreateWindowsHidDeviceFactory(
            this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
            ILoggerFactory loggerFactory = null,
            IHidApiService hidApiService = null,
            Guid? classGuid = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null,
            GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
            byte defaultWriteReportId = 0,
            Func<TransferResult, ReadReport> readTransferTransform = null,
            Func<byte[], byte, byte[]> writeTransferTransform = null
            )
        {
            if (filterDeviceDefinitions == null) throw new ArgumentNullException(nameof(filterDeviceDefinitions));

            loggerFactory ??= NullLoggerFactory.Instance;

            var selectedHidApiService = hidApiService ?? new WindowsHidApiService(loggerFactory);

            classGuid ??= selectedHidApiService.GetHidGuid();

            if (getConnectedDeviceDefinitionsAsync == null)
            {
                var windowsDeviceEnumerator = new WindowsDeviceEnumerator(
                    loggerFactory.CreateLogger<WindowsDeviceEnumerator>(),
                    classGuid.Value,
                    (d, guid) => GetDeviceDefinition(d, selectedHidApiService, loggerFactory.CreateLogger(nameof(WindowsHidDeviceFactoryExtensions))),
                    c => Task.FromResult(!filterDeviceDefinitions.Any() || filterDeviceDefinitions.FirstOrDefault(f => f.IsDefinitionMatch(c, DeviceType.Hid)) != null)
                    );

                getConnectedDeviceDefinitionsAsync = windowsDeviceEnumerator.GetConnectedDeviceDefinitionsAsync;
            }

            return new DeviceFactory(
                loggerFactory,
                getConnectedDeviceDefinitionsAsync,
                (c, cancellationToken) => Task.FromResult<IDevice>(new HidDevice
                (
                    new WindowsHidHandler(
                        c.DeviceId,
                        writeBufferSize,
                        readBufferSize,
                        hidApiService,
                        loggerFactory,
                        readTransferTransform,
                        writeTransferTransform),
                    loggerFactory,
                    defaultWriteReportId
                )),
                (c, cancellationToken) => Task.FromResult(c.DeviceType == DeviceType.Hid));
        }

        #endregion Public Methods

        #region Private Methods

        private static ConnectedDeviceDefinition GetDeviceDefinition(string deviceId, IHidApiService HidService, ILogger logger)
        {
            logger ??= NullLogger.Instance;

            using var logScope = logger.BeginScope("DeviceId: {deviceId} Call: {call}", deviceId, nameof(GetDeviceDefinition));

            try
            {
                using var safeFileHandle = HidService.CreateReadConnection(deviceId, FileAccessRights.None);

                if (safeFileHandle.IsInvalid) throw new DeviceException($"{nameof(HidService.CreateReadConnection)} call with Id of {deviceId} failed.");

                logger.LogDebug(Messages.InformationMessageFoundDevice);

                return HidService.GetDeviceDefinition(deviceId, safeFileHandle);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, Messages.ErrorMessageCouldntGetDevice);
                return null;
            }
        }

        #endregion Private Methods
    }

}
