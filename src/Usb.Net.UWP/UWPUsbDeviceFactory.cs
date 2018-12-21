using Device.Net;
using Usb.Net.UWP;

namespace Hid.Net.UWP
{
    public class UWPUsbDeviceFactory : IDeviceFactory<UWPUsbDevice>
    {
        public static void Register()
        {
            DeviceMan.Current.DeviceFactories.Add(new UWPUsbDeviceFactory());
        }

        public UWPUsbDevice GetDevice(DeviceDefinition deviceDefinition)
        {
            return new UWPUsbDevice(deviceDefinition.DeviceId);
        }
    }
}
