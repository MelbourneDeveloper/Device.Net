namespace Device.Net.Reactive
{
    public static class ReactiveDeviceExtensions
    {
        public static DeviceDataStreamer CreateDeviceDataStreamer(this IDeviceManager deviceManager, ProcessData processData) => new DeviceDataStreamer(processData, deviceManager);
    }
}
