namespace Device.Net
{
    public interface IDeviceFactory
    {
        IDevice GetDevice(string deviceId);
    }

    public interface IDeviceFactory<T> : IDeviceFactory where T : IDevice
    {
        T GetDevice(string deviceId);
    }
}
