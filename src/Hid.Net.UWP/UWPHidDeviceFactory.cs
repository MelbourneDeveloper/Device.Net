namespace Hid.Net.UWP
{
    public class UWPHidDeviceFactory : UWPHidDeviceFactoryBase<UWPHidDevice>
    {
        public override UWPHidDevice GetDevice(string deviceId)
        {
            return new UWPHidDevice(deviceId);
        }
    }
}
