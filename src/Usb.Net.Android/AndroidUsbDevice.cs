using Device.Net;

namespace Usb.Net.Android
{
    public class AndroidUsbDevice : UsbDevice
    {
        public AndroidUsbDevice(AndroidUsbDeviceHandler androidUsbDeviceHandler, ILogger logger, ITracer tracer) : base(androidUsbDeviceHandler, logger, tracer)
        {
        }
    }
}