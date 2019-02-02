namespace Device.Net.LibUsb
{
    public class LibUsbHidDeviceFactory : LibUsbDeviceFactoryBase
    {
        public override DeviceType DeviceType => DeviceType.Hid;
    }
}
