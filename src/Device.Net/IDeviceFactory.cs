namespace Device.Net
{
    public interface IDeviceFactory
    {
    }

    public interface IDeviceFactory<T> : IDeviceFactory where T : IDevice
    {
        T GetDevice(DeviceDefinition deviceDefinition);
    }
}
