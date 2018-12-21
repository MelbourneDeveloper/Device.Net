using Device.Net;

namespace Hid.Net.UWP
{
    public class UWPHidDeviceFactory : IDeviceFactory<UWPHidDevice>
    {
        public static void Register()
        {
            DeviceMan.Current.DeviceFactories.Add(new UWPHidDeviceFactory());
        }

        public UWPHidDevice GetDevice(DeviceDefinition deviceDefinition)
        {
            return new UWPHidDevice(deviceDefinition.DeviceId);
        }
    }
}
