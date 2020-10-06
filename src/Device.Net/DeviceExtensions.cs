using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Device.Net
{
    public static class DeviceExtensions
    {
        public static IDeviceManager ToDeviceManager(this IDeviceFactory deviceFactory, ILoggerFactory loggerFactory = null)
             =>
            deviceFactory == null ? throw new ArgumentNullException(nameof(deviceFactory)) :
            new DeviceManager(new ReadOnlyCollection<IDeviceFactory>(new List<IDeviceFactory> { deviceFactory }), loggerFactory);

        public static IDeviceManager ToDeviceManager(this IList<IDeviceFactory> deviceFactories, ILoggerFactory loggerFactory = null)
            => deviceFactories == null ? throw new ArgumentNullException(nameof(deviceFactories)) :
            new DeviceManager(new ReadOnlyCollection<IDeviceFactory>(deviceFactories), loggerFactory);

        public static DeviceDataStreamer CreateDeviceDataStreamer(
    this IDeviceManager deviceManager,
    ProcessData processData,
    Func<IDevice, Task> initializeFunc = null) =>
    new DeviceDataStreamer(
        processData,
        deviceManager,
        initializeFunc: initializeFunc);

    }
}