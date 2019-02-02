namespace Device.Net.LibUsb
{
    public class LibUsbUsbDeviceFactory : LibUsbDeviceFactoryBase
    {
        public override DeviceType DeviceType => DeviceType.Usb;
    }
}
