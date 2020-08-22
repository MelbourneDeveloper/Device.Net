using Microsoft.Extensions.Logging;

namespace Device.Net.LibUsb
{
    public class LibUsbUsbDeviceFactory : LibUsbDeviceFactoryBase
    {
        public LibUsbUsbDeviceFactory(ILoggerFactory loggerFactory, ITracer tracer) : base(loggerFactory, tracer)
        {
        }

        public override DeviceType DeviceType => DeviceType.Usb;
    }
}
