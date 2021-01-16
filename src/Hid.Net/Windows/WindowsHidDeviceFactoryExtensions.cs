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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class WindowsHidDeviceFactoryExtensions
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        /// <summary>
        /// Creates a <see cref="IDeviceFactory"/> for Windows Hid devices
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="hidApiService"></param>
        /// <param name="classGuid">Filters by specified class guid</param>
        /// <param name="readBufferSize"></param>
        /// <param name="writeBufferSize"></param>
        /// <param name="getConnectedDeviceDefinitionsAsync">Override the default call for getting definitions</param>
        /// <returns></returns>
        public static IDeviceFactory CreateWindowsHidDeviceFactory(
        ILoggerFactory loggerFactory = null,
        IHidApiService hidApiService = null,
        Guid? classGuid = null,
        ushort? readBufferSize = null,
        ushort? writeBufferSize = null,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null)
        {
            return CreateWindowsHidDeviceFactory(
                new ReadOnlyCollection<FilterDeviceDefinition>(new List<FilterDeviceDefinition>()),
                loggerFactory,
                hidApiService,
                classGuid,
                readBufferSize,
                writeBufferSize,
                getConnectedDeviceDefinitionsAsync
                );
        }

        //TODO: this is named incorrectly. This needs to be fixed


        public static IDeviceFactory CreateWindowsHidDeviceManager(
        this FilterDeviceDefinition filterDeviceDefinition,
        ILoggerFactory loggerFactory = null,
        IHidApiService hidApiService = null,
        Guid? classGuid = null,
        ushort? readBufferSize = null,
        ushort? writeBufferSize = null)
        {
            var factory = CreateWindowsHidDeviceFactory(
                filterDeviceDefinition,
                loggerFactory,
                hidApiService,
                classGuid,
                readBufferSize,
                writeBufferSize
                );

            return new DeviceManager(new ReadOnlyCollection<IDeviceFactory>(new List<IDeviceFactory> { factory }), loggerFactory);
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
        /// <returns></returns>
        public static IDeviceFactory CreateWindowsHidDeviceFactory(
        this FilterDeviceDefinition filterDeviceDefinition,
        ILoggerFactory loggerFactory = null,
        IHidApiService hidApiService = null,
        Guid? classGuid = null,
        ushort? readBufferSize = null,
        ushort? writeBufferSize = null,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        byte defaultWriteReportId = 0)
        {
            return CreateWindowsHidDeviceFactory(
                new ReadOnlyCollection<FilterDeviceDefinition>(new List<FilterDeviceDefinition> { filterDeviceDefinition }),
                loggerFactory,
                hidApiService,
                classGuid,
                readBufferSize,
                writeBufferSize,
                getConnectedDeviceDefinitionsAsync,
                defaultWriteReportId
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
        /// <returns></returns>
        public static IDeviceFactory CreateWindowsHidDeviceFactory(
            this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
            ILoggerFactory loggerFactory = null,
            IHidApiService hidApiService = null,
            Guid? classGuid = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null,
            GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
            byte defaultWriteReportId = 0)
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
                        (tr) =>
                        {
                            //Grab the report id
                            var reportId = tr.Data[0];

                            //Create a new array and copy the data to it without the report id
                            var data = TrimFirstByte(tr);

                            //Convert to a read report
                            return new ReadReport(reportId, new TransferResult(data, tr.BytesTransferred));
                        },
                        (data, reportId) =>
                        {
                            //Create a new array which is one byte larger 
                            var transformedData = new byte[data.Length + 1];

                            //Set the report id at index 0
                            transformedData[0] = reportId;

                            //copy the data to it without the report id at index 1
                            Array.Copy(data, 0, transformedData, 1, data.Length);

                            return transformedData;
                        },
                        writeBufferSize,
                        readBufferSize,
                        hidApiService,
                        loggerFactory),
                    loggerFactory,
                    defaultWriteReportId
                )),
                (c, cancellationToken) => Task.FromResult(c.DeviceType == DeviceType.Hid));
        }

        private static byte[] TrimFirstByte(TransferResult tr)
        {
            var length = tr.Data.Length - 1;
            var data = new byte[length];
            Array.Copy(tr.Data, 1, data, 0, length);
            return data;
        }

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
    }

}
