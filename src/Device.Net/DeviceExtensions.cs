using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Device.Net
{
    public static class DeviceExtensions
    {
        public static IDeviceManager ToDeviceManager(this IDeviceFactory deviceFactory, ILoggerFactory loggerFactory = null)
            => new DeviceManager(new ReadOnlyCollection<IDeviceFactory>(new List<IDeviceFactory> { deviceFactory }), loggerFactory);

        public static IDeviceManager ToDeviceManager(this IList<IDeviceFactory> deviceFactories, ILoggerFactory loggerFactory = null)
            => new DeviceManager(new ReadOnlyCollection<IDeviceFactory>(deviceFactories), loggerFactory);

    }
}