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
        public static IDeviceFactory CreateWindowsHidDeviceFactory(
            this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
            ILoggerFactory loggerFactory,
            IHidApiService hidApiService = null,
            Guid? classGuid = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            var selectedHidApiService = hidApiService ?? new WindowsHidApiService(loggerFactory);

            var windowsDeviceEnumerator = new WindowsDeviceEnumerator(
                loggerFactory.CreateLogger<WindowsDeviceEnumerator>(),
                classGuid ?? selectedHidApiService.GetHidGuid(),
                (d) => GetDeviceDefinition(d, selectedHidApiService, loggerFactory.CreateLogger(nameof(WindowsHidDeviceFactoryExtensions))),
                async (c) =>
                    filterDeviceDefinitions.FirstOrDefault((f) => DeviceManager.IsDefinitionMatch(f, c)) != null
                );

            return new DeviceFactory(
                loggerFactory,
                windowsDeviceEnumerator.GetConnectedDeviceDefinitionsAsync,
                (c) => new WindowsHidDevice
                (
                    c.DeviceId,
                    loggerFactory,
                    hidApiService: selectedHidApiService,
                    readBufferSize: readBufferSize,
                    writeBufferSize: writeBufferSize
                ));
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
