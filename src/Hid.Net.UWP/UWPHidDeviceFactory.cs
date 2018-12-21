using Device.Net;

namespace Hid.Net.UWP
{
    public class UWPHidDeviceFactory : IDeviceFactory<UWPHidDevice>
    {
        public static void Register()
        {
            DeviceMan.Current.DeviceFactories.Add(new UWPHidDeviceFactory());
        }

        public UWPHidDevice GetDevice(string deviceId)
        {
            return new UWPHidDevice(deviceId);
        }
    }
}
