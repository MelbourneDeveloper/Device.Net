using Device.Net;
using Device.Net.Exceptions;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hid.Net.Windows
{

    public static class WindowsHidDeviceFactoryExtensions
    {
        public static IDeviceManager CreateWindowsHidDeviceManager(
        this FilterDeviceDefinition filterDeviceDefinition,
        ILoggerFactory loggerFactory,
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

            return new DeviceManager(loggerFactory) { DeviceFactories = { factory } };
        }

        public static IDeviceFactory CreateWindowsHidDeviceFactory(
        this FilterDeviceDefinition filterDeviceDefinition,
        ILoggerFactory loggerFactory,
        IHidApiService hidApiService = null,
        Guid? classGuid = null,
        ushort? readBufferSize = null,
        ushort? writeBufferSize = null,
        byte? defaultReportId = null)
        {
            return CreateWindowsHidDeviceFactory(
                new List<FilterDeviceDefinition> { filterDeviceDefinition },
                loggerFactory,
                hidApiService,
                classGuid,
                readBufferSize,
                writeBufferSize,
                defaultReportId
                );
        }

        public static IDeviceFactory CreateWindowsHidDeviceFactory(
            this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
            ILoggerFactory loggerFactory,
            IHidApiService hidApiService = null,
            Guid? classGuid = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null,
            byte? defaultReportId = null)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            var selectedHidApiService = hidApiService ?? new WindowsHidApiService(loggerFactory);

            var windowsDeviceEnumerator = new WindowsDeviceEnumerator(
                loggerFactory.CreateLogger<WindowsDeviceEnumerator>(),
                classGuid ?? selectedHidApiService.GetHidGuid(),
                (d) => GetDeviceDefinition(d, selectedHidApiService, loggerFactory.CreateLogger(nameof(WindowsHidDeviceFactoryExtensions))),
                async (c) =>
                    filterDeviceDefinitions.FirstOrDefault((f) => DeviceManager.IsDefinitionMatch(f, c, DeviceType.Hid)) != null
                );

            return new DeviceFactory(
                loggerFactory,
                windowsDeviceEnumerator.GetConnectedDeviceDefinitionsAsync,
                async (c) => new WindowsHidDevice
                (
                    c,
                    loggerFactory: loggerFactory,
                    hidService: selectedHidApiService,
                    readBufferSize: readBufferSize,
                    writeBufferSize: writeBufferSize,
                    defaultReportId: defaultReportId
                ),
                DeviceType.Hid);
        }

        private static ConnectedDeviceDefinition GetDeviceDefinition(string deviceId, IHidApiService HidService, ILogger Logger)
        {
            IDisposable logScope = null;

            try
            {
                logScope = Logger?.BeginScope("DeviceId: {deviceId} Call: {call}", deviceId, nameof(GetDeviceDefinition));

                using (var safeFileHandle = HidService.CreateReadConnection(deviceId, FileAccessRights.None))
                {
                    if (safeFileHandle.IsInvalid) throw new DeviceException($"{nameof(HidService.CreateReadConnection)} call with Id of {deviceId} failed.");

                    Logger?.LogDebug(Messages.InformationMessageFoundDevice);

                    return HidService.GetDeviceDefinition(deviceId, safeFileHandle);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, Messages.ErrorMessageCouldntGetDevice);
                return null;
            }
            finally
            {
                logScope?.Dispose();
            }
        }
    }

}
