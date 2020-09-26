using System;
using System.Threading.Tasks;

namespace Device.Net.Reactive
{
    public static class ReactiveDeviceExtensions
    {
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
