namespace Device.Net
{
    public interface IDeviceFactory<T>
    {
        T GetDevice(string deviceId);
    }
}
