using Device.Net;

namespace Hid.Net.UWP
{
    public abstract class UWPHidDeviceFactoryBase<T> : IDeviceFactory<T>
    {
        public abstract T GetDevice(string deviceId);
    }
}
