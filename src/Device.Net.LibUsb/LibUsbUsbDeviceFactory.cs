using Microsoft.Extensions.Logging;

namespace Device.Net.LibUsb
{
    public class LibUsbUsbDeviceFactory : LibUsbDeviceFactoryBase
    {
        public LibUsbUsbDeviceFactory(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        public override DeviceType DeviceType => DeviceType.Usb;
    }
}
