using Microsoft.Extensions.Logging;
using System;

namespace Device.Net
{
    public static class DeviceExtensions
    {
        /// <summary>
        /// Register the factory for enumerating devices.
        /// </summary>
        public static void RegisterDeviceFactory(this IDeviceManager deviceManager, IDeviceFactory newDeviceFactory)
        {
            if (deviceManager == null) throw new ArgumentNullException(nameof(deviceManager));

            foreach (var deviceFactory in deviceManager.DeviceFactories)
            {
                if (ReferenceEquals(deviceFactory, newDeviceFactory)) return;
            }

            deviceManager.DeviceFactories.Add(newDeviceFactory);
        }

        public static IDeviceManager ToDeviceManager(this IDeviceFactory deviceFactory, ILoggerFactory loggerFactory = null) => new DeviceManager(loggerFactory) { DeviceFactories = { deviceFactory } };

    }
}