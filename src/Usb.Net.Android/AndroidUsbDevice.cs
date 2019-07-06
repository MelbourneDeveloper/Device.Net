using Usb.Net;
using Usb.Net.Android;

namespace Device.Net
{
    public class AndroidUsbDevice : UsbDevice, IUsbDevice
    {
        public AndroidUsbDevice(AndroidUsbDeviceHandler androidUsbDeviceHandler, ILogger logger, ITracer tracer) : base(androidUsbDeviceHandler, logger, tracer)
        {
        }
    }
}