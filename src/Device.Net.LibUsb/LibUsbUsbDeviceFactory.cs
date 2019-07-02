namespace Device.Net.LibUsb
{
    public class LibUsbUsbDeviceFactory : LibUsbDeviceFactoryBase
    {
        public override DeviceType DeviceType => DeviceType.Usb;

        public static void Register()
        {
            Register(null);
        }

        public static void Register(ILogger logger)
        {
            DeviceManager.Current.DeviceFactories.Add(new LibUsbUsbDeviceFactory() { Logger = logger });
        }
    }
}
