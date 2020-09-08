using Device.Net;
using Device.Net.Exceptions;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
using System;

namespace Hid.Net.Windows
{


    public static class WindowsHidDeviceFactoryExtensions
    {
        public static IDeviceFactory CreateWindowsHidDeviceFactory(ILoggerFactory loggerFactory, IHidApiService hidApiService = null, Guid? classGuid = null)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            var selectedHidApiService = hidApiService ?? new WindowsHidApiService(loggerFactory);

            var windowsDeviceEnumerator = new WindowsDeviceEnumerator(
                loggerFactory.CreateLogger<WindowsDeviceEnumerator>(),
                classGuid,
                (d) => GetDeviceDefinition(d, selectedHidApiService, loggerFactory.CreateLogger<WindowsHidDeviceFactoryExtensions>())
                );

            return CreateWindowsHidDeviceFactory(windowsDeviceEnumerator.GetConnectedDeviceDefinitionsAsync, (c) => new WindowsHidDevice(c.DeviceId, loggerFactory, hidApiService: selectedHidApiService), loggerFactory);
        }

        public static IDeviceFactory CreateWindowsHidDeviceFactory(this GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync, GetDevice getDevice, ILoggerFactory loggerFactory)
        {
            return getConnectedDeviceDefinitionsAsync == null
                ? throw new ArgumentNullException(nameof(getConnectedDeviceDefinitionsAsync))
                : getDevice == null
                ? throw new ArgumentNullException(nameof(getDevice))
                : loggerFactory == null
                ? throw new ArgumentNullException(nameof(loggerFactory))
                : new DeviceFactory(loggerFactory, getConnectedDeviceDefinitionsAsync, getDevice);
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
