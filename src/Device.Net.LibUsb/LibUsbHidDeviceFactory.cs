using System;

namespace Device.Net.LibUsb
{
    public class LibUsbHidDeviceFactory : LibUsbDeviceFactoryBase
    {
        public override DeviceType DeviceType => DeviceType.Hid;

        public static void Register()
        {
            DeviceManager.Current.DeviceFactories.Add(new LibUsbHidDeviceFactory());
        }
    }
}
