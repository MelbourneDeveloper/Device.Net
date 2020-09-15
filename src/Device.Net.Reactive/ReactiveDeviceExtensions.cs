namespace Device.Net.Reactive
{
    public static class ReactiveDeviceExtensions
    {
        public static DeviceDataStreamer<T> CreateDeviceDataStreamer<T>(this IDeviceManager deviceManager, ProcessData<T> processData) => new DeviceDataStreamer<T>(processData, deviceManager);
    }
}
