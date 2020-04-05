namespace Device.Net.LibUsb
{
    public class LibUsbUsbDeviceFactory : LibUsbDeviceFactoryBase
    {
        public LibUsbUsbDeviceFactory(ILogger logger, ITracer tracer) : base(logger, tracer)
        {
        }

        public override DeviceType DeviceType => DeviceType.Usb;
    }
}
