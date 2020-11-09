using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Device.Net
{
    public static class DeviceExtensions
    {
        public static IDeviceFactory Aggregate(this IList<IDeviceFactory> deviceFactories, ILoggerFactory loggerFactory = null)
            => deviceFactories == null ? throw new ArgumentNullException(nameof(deviceFactories)) :
            new DeviceManager(new ReadOnlyCollection<IDeviceFactory>(deviceFactories), loggerFactory);

        public static DeviceDataStreamer CreateDeviceDataStreamer(
    this IDeviceFactory deviceFactory,
    ProcessData processData,
    Func<IDevice, Task> initializeFunc = null) =>
    new DeviceDataStreamer(
        processData,
        deviceFactory,
        initializeFunc: initializeFunc);
    }
}