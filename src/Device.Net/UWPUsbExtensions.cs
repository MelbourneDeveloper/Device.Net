using Device.Net;
using System;

namespace Usb.Net.UWP
{
    public static class DeviceExtensions
    {
        /// <summary>
        /// Register the factory for enumerating devices.
        /// </summary>
        public static void RegisterDeviceFactory(this IDeviceManager deviceManager, Func<ILogger, ITracer, IDeviceFactory> createDeviceFactory, ILogger logger, ITracer tracer)
        {
            if (deviceManager == null) throw new ArgumentNullException(nameof(deviceManager));
            if (createDeviceFactory == null) throw new ArgumentNullException(nameof(createDeviceFactory));

            var newDeviceFactory = createDeviceFactory(logger, tracer);

            foreach (var deviceFactory in deviceManager.DeviceFactories)
            {
                if (ReferenceEquals(deviceFactory, newDeviceFactory)) return;
            }

            deviceManager.DeviceFactories.Add(newDeviceFactory);
        }
    }
}