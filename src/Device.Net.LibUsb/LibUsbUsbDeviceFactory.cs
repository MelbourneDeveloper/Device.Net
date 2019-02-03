namespace Device.Net.LibUsb
{
    public class LibUsbUsbDeviceFactory : LibUsbDeviceFactoryBase
    {
        public override DeviceType DeviceType => DeviceType.Usb;

        public static void Register()
        {
            DeviceManager.Current.DeviceFactories.Add(new LibUsbUsbDeviceFactory());
        }
    }
}
